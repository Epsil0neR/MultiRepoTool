using CommandLine;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MultiRepoTool
{
	public struct Constants
	{
		public const ConsoleColor ColorBranchLocal = ConsoleColor.Green;
		public const ConsoleColor ColorRepository = ConsoleColor.Yellow;
		public const ConsoleColor ColorBranchRemote = ConsoleColor.Red;
	}

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
			//TODO: Get rid of this temp options:
			options.Path = @"C:\Projects\_git\tradezero1";
			options.Search = "proj";

			if (string.IsNullOrEmpty(options.Path))
				options.Path = Environment.CurrentDirectory;

			var di = new DirectoryInfo(options.Path);
			if (!di.Exists)
			{
				Console.WriteLine("Path to directory with all repositories not found.");
				Console.Write("Press any key to exit...");
				Console.ReadKey(false);
			}

			SetTitle(di.FullName);

			ConsoleUtils.Write("Directory with repositories: ");
			ConsoleUtils.Write(di.FullName, ConsoleColor.Cyan);
			ConsoleUtils.WriteLine();

			var directories = di.GetDirectories();
			var repositories = directories
				.Select(x => GitRepository.FromDirectory(x))
				.Where(x => x != null);

			IoC.RegisterInstance(repositories);

			var order = new List<Type>
			{
				typeof(MenuItems.Reload),
				typeof(MenuItems.Fetch),
				typeof(MenuItems.Search),
				typeof(MenuItems.Status),
				typeof(MenuItems.OpenInGitKraken),
				typeof(MenuItems.EndActionsSeparator),
				typeof(MenuItems.ClearConsole),
				typeof(MenuItems.Exit),
			};
			var menuItems = IoC.ResolveAll<MenuItem>()
				.OrderBy(x => order.IndexOf(x.GetType()))
				.ToList();

			var menu = new Menu(menuItems)
			{
				LoopNavigation = true,
			};

			RunFromOptions(menu, options);

			menu.Run();

			if (options.AutoExit)
				return;

			Console.WriteLine();
			Console.Write("Press any key to exit...");
			Console.ReadKey(false);
		}

		private static void RunFromOptions(Menu menu, Options options)
		{
			if (options.Fetch)
				menu.Items.OfType<MenuItems.Fetch>().FirstOrDefault()?.Execute(menu);

			if (options.OpenInGitKraken)
				menu.Items.OfType<MenuItems.OpenInGitKraken>().FirstOrDefault()?.Exec(options.Search, true);
			else if (!string.IsNullOrWhiteSpace(options.Search))
				menu.Items.OfType<MenuItems.Search>().FirstOrDefault()?.Exec(options.Search, true);
		}

		private static void SetTitle(string workPath)
		{
			var asm = Assembly.GetExecutingAssembly();
			var exe = Path.GetFileName(asm.Location);

			Console.Title = $"{exe}: {workPath}";
		}
	}
}
