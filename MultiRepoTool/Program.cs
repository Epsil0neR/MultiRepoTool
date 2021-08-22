using CommandLine;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MultiRepoTool
{
	class Program
	{
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
			options.Path = @"C:\Projects\_git\tradezero1";

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

			var longestName = directories.Max(x => x.Name.Length) + 4;

			//TODO: Find a way to fetch GIT without entering credentials manually or embedding them into repository directory.
			//foreach (var repo in repositories)
			//	repo.Fetch();

			string ToResultString(GitBranch branch)
			{
				var name = branch.HasLocal() ? branch.Local : branch.Remote;
				if (branch.Behind == 0 && branch.Ahead == 0)
					return name;
				var sb = new StringBuilder();
				sb.Append(name);
				if (branch.Ahead > 0)
					sb.Append($"[A:{branch.Ahead}]");
				if (branch.Behind > 0)
					sb.Append($"[B:{branch.Behind}]");
				return sb.ToString();
			}

			if (!string.IsNullOrWhiteSpace(options.SearchBranch))
			{
				var result = repositories.Search(options.SearchBranch, false);
				Write("Search results for: ");
				WriteLine(options.SearchBranch, ConsoleColor.Green);

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
					Write($"    {repository.Name}", ConsoleColor.Yellow);
					(int _, var top) = Console.GetCursorPosition();
					Console.SetCursorPosition(longestName + 4, top);
					Write(repository.ActiveBranch.Local, ConsoleColor.Green);
					WriteLine($" {string.Join("  ", branches.Where(x => !ReferenceEquals(x, repository.ActiveBranch)).Select(ToResultString))}", ConsoleColor.Red);
				}

				WriteLine($"  Has that branch: {toChange.Count}");
				foreach (var (repository, branches) in toChange)
				{
					Write($"    {repository.Name}", ConsoleColor.Yellow);
					(int _, var top) = Console.GetCursorPosition();
					Console.SetCursorPosition(longestName + 4, top);
					WriteLine($"{string.Join("  ", branches.Select(ToResultString))}", ConsoleColor.Red);
				}
				WriteLine();
			}
			else
			{
				WriteLine("List all repositories with status:");

				foreach (var repository in repositories)
				{
					var branch = repository.ActiveBranch;
					Write(repository.Name, ConsoleColor.Yellow);
					(int _, var top) = Console.GetCursorPosition();
					Console.SetCursorPosition(longestName + 4, top);
					Write($" {ToResultString(branch)}", ConsoleColor.Green);
					Write("...");
					Write(branch.Remote, ConsoleColor.Red);
					WriteLine();
					if (!string.IsNullOrWhiteSpace(branch.Status))
						WriteLine(branch.Status);
					WriteLine();
				}
			}


			Console.WriteLine();
			Console.Write("Press any key to exit...");
			Console.ReadKey(false);
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
	}
}
