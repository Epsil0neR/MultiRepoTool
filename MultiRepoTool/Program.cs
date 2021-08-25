using CommandLine;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MultiRepoTool
{
	class Program
	{
		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr windowHandle);

		private const ConsoleColor ColorBranchLocal = ConsoleColor.Green;
		private const ConsoleColor ColorRepository = ConsoleColor.Yellow;
		private const ConsoleColor ColorBranchRemote = ConsoleColor.Red;

		static void Main(string[] args)
		{
			ConfigureIoC();

			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(WithParsed)
				.WithNotParsed(WithNotParsed);
		}

		private static void ConfigureIoC()
		{

		}

		private static void WithNotParsed(IEnumerable<Error> enumerable)
		{
			Console.WriteLine("Error parsing arguments.");
			Console.Write("Press any key to exit...");
			Console.ReadKey(false);
		}

		private static void WithParsed(Options options)
		{
			if (string.IsNullOrEmpty(options.Path))
				options.Path = Environment.CurrentDirectory;
			var di = new DirectoryInfo(options.Path);
			if (!di.Exists)
			{
				Console.WriteLine("Path to directory with all repositories not found.");
				Console.Write("Press any key to exit...");
				Console.ReadKey(false);
			}

			Write("Directory with repositories: ");
			Write(di.FullName, ConsoleColor.Cyan);
			WriteLine();

			var directories = di.GetDirectories();
			var repositories = directories
				.Select(x => GitRepository.FromDirectory(x))
				.Where(x => x != null);

			var longestName = directories.Max(x => x.Name.Length);

			//TODO: Find a way to fetch GIT without entering credentials manually or embedding them into repository directory.
			//foreach (var repo in repositories)
			//	repo.Fetch();

			var actions = new List<Func<bool>>
			{
				() => TryOpenInGitKraken(repositories, options, longestName).GetAwaiter().GetResult(),
				() => TrySearchBranch(repositories, options, longestName),
				() => ListAllChanges(repositories, longestName)
			};

			_ = actions.Any(x => x());

			Console.WriteLine();
			Console.Write("Press any key to exit...");
			Console.ReadKey(false);
		}

		private static string GetNameWithTrackingInfo(GitBranch branch)
		{
			if (branch == null)
				return string.Empty;

			var name = branch.HasLocal() ? branch.Local : branch.Remote;
			if (branch.Behind == 0 && branch.Ahead == 0) return name;
			var sb = new StringBuilder();
			sb.Append(name);
			if (branch.Ahead > 0) sb.Append($"[A:{branch.Ahead}]");
			if (branch.Behind > 0) sb.Append($"[B:{branch.Behind}]");
			return sb.ToString();
		}

		private static void Write(string text, ConsoleColor? color = null)
		{
			var current = Console.ForegroundColor;
			if (color.HasValue)
			{
				Console.ForegroundColor = color.Value;
			}
			Console.Write(text);
			Console.ForegroundColor = current;
		}

		private static void WriteLine(string text = null, ConsoleColor? color = null)
		{
			if (text == null)
			{
				Console.WriteLine();
				return;
			}

			var current = Console.ForegroundColor;
			if (color.HasValue)
			{
				Console.ForegroundColor = color.Value;
			}
			Console.WriteLine(text);
			Console.ForegroundColor = current;
		}

		private static void SetCursorLeft(int left)
		{
			(int _, var top) = Console.GetCursorPosition();
			Console.SetCursorPosition(left, top);
		}

		private static void Focus()
		{
			var hWnd = Process.GetCurrentProcess().MainWindowHandle;
			SetForegroundWindow(hWnd);
		}

		private static bool TrySearchBranch(IEnumerable<GitRepository> repositories, Options options, int longestName)
		{
			if (string.IsNullOrWhiteSpace(options.Search))
				return false;

			var result = repositories.Search(options.Search, false);
			Write("Search results for: ");
			WriteLine(options.Search, ColorBranchLocal);

			var onCorrect = result
				.Where(x => x.Value.Contains(x.Key.ActiveBranch))
				.ToList();
			var toChange = result
				.Except(onCorrect)
				.Where(x => x.Value.Any())
				.ToList();

			WriteLine($"  Already on that branch: {onCorrect.Count}");
			foreach (var (repository, branches) in onCorrect)
			{
				Write($"    {repository.Name}", ColorRepository);
				SetCursorLeft(longestName + 8);
				Write(GetNameWithTrackingInfo(repository.ActiveBranch), ColorBranchLocal);
				WriteLine($" {string.Join("  ", branches.Where(x => !ReferenceEquals(x, repository.ActiveBranch)).Select(GetNameWithTrackingInfo))}", ColorBranchRemote);
			}

			WriteLine();
			WriteLine($"  Has that branch: {toChange.Count}");
			foreach (var (repository, branches) in toChange)
			{
				Write($"    {repository.Name}", ColorRepository);
				SetCursorLeft(longestName + 8);
				WriteLine($"{string.Join("  ", branches.Select(GetNameWithTrackingInfo))}", ColorBranchRemote);
			}
			WriteLine();

			var notFound = result.Where(x => !x.Value.Any()).ToList();
			if (notFound.Any())
			{
				WriteLine($"  Nothing found: {notFound.Count}", ConsoleColor.Red);
				foreach (var (repository, _) in notFound)
				{
					Write($"    {repository.Name}", ColorRepository);
					SetCursorLeft(longestName + 8);

					if (repository.ActiveBranch != null)
						Write(repository.ActiveBranch.Local, ColorBranchLocal);
					else
						Write("HEAD detached", ConsoleColor.DarkRed);
				}
			}

			return true;
		}

		private static async Task<bool> TryOpenInGitKraken(IEnumerable<GitRepository> repositories, Options options, int longestName)
		{
			if (!options.OpenInGitKraken)
				return false;

			WriteLine("Open all repositories in GitKraken:");
			var filter = !string.IsNullOrWhiteSpace(options.Search);
			var tasks = new List<Task>();
			foreach (var repository in repositories)
			{
				if (filter && !repository.Name.Contains(options.Search, StringComparison.InvariantCultureIgnoreCase))
				{
					Write($"  {repository.Name}", ColorRepository);
					SetCursorLeft(longestName + 2);
					WriteLine(" - skipped.");
					continue;
				}

				WriteLine($"  {repository.Name}", ColorRepository);
				var task = repository.OpenInGitKraken();
				tasks.Add(task);
				switch (options.OpenInGitKrakenDelay)
				{
					case 0:
						await task;
						break;
					case > 0:
						task.Wait(options.OpenInGitKrakenDelay);
						break;
				}
			}

			await Task.WhenAll(tasks);

			WriteLine();
			WriteLine($"Opened {tasks.Count} repositories in GitKraken.");

			await Task.Delay(5000);
			Focus();

			return true;
		}


		private static bool ListAllChanges(IEnumerable<GitRepository> repositories, int longestName)
		{
			WriteLine("List all repositories with status:");

			foreach (var repository in repositories)
			{
				var branch = repository.ActiveBranch;
				if (branch == null)
				{
					WriteLine("HEAD detached", ConsoleColor.DarkRed);
					continue;
				}

				Write(repository.Name, ColorRepository);
				SetCursorLeft(longestName + 8);
				Write($" {GetNameWithTrackingInfo(branch)}", ColorBranchLocal);
				Write("...");
				Write(branch.Remote, ColorBranchRemote);
				WriteLine();
				if (!string.IsNullOrWhiteSpace(branch.Status))
					WriteLine(branch.Status);
				WriteLine();
			}

			return true;
		}
	}
}
