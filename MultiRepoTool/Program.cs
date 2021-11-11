﻿using CommandLine;
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
            IoC.RegisterInstance(SharedUtils.GetVersion());
		}

        private static void WithNotParsed(IEnumerable<Error> enumerable)
		{
			Console.WriteLine("Error parsing arguments.");
			Console.Write("Press any key to exit...");
			Console.ReadKey(false);
		}

		private static void WithParsed(Options options)
		{
            IoC.RegisterInstance(options);

			if (string.IsNullOrEmpty(options.Path))
				options.Path = Environment.CurrentDirectory;

			var di = new DirectoryInfo(options.Path);
			if (!di.Exists)
			{
				Console.WriteLine("Path to directory with all repositories not found.");
				Console.Write("Press any key to exit...");
				Console.ReadKey(false);
                return;
			}

			SetTitle(di.FullName);

			ConsoleUtils.Write("Directory with repositories: ");
			ConsoleUtils.Write(di.FullName, ConsoleColor.Cyan);
			ConsoleUtils.WriteLine();

			var directories = di.GetDirectories();
			var repositories = directories
				.Select(x => GitRepository.FromDirectory(x))
				.Where(x => x != null)
                .ToList();

			IoC.RegisterInstance<IEnumerable<GitRepository>>(repositories);

			var menuItems = new List<MenuItem>
			{
				IoC.Resolve<MenuItems.Reload>(),
				IoC.Resolve<MenuItems.Fetch>(),
                IoC.Resolve<MenuItems.Pull>(),
				IoC.Resolve<MenuItems.Search>(),
				IoC.Resolve<MenuItems.StatusShort>(),
				IoC.Resolve<MenuItems.Status>(),
				SharedUtils.IsGitKrakenInstalled() ? IoC.Resolve<MenuItems.OpenInGitKraken>() : null,
				IoC.Resolve<MenuItems.OpenSolution>(),
				IoC.Resolve<MenuItems.CheckDiffs>(),
				IoC.Resolve<MenuItems.CleanFolders>(),
				IoC.Resolve<MenuItems.EndActionsSeparator>(),
				IoC.Resolve<MenuItems.ClearConsole>(),
				IoC.Resolve<MenuItems.Exit>(),
			}
                .Where(x=>x is not null)
                .ToList();

			var menu = new Menu(menuItems)
			{
				LoopNavigation = true,
			};

			if (!RunFromOptions(menu, options) || !options.Menu)
				menu.Run();

			if (options.AutoExit)
				return;

			Console.WriteLine();
			Console.Write("Press any key to exit...");
			Console.ReadKey(false);
		}

		private static bool RunFromOptions(Menu menu, Options options)
		{
			if (options.Fetch)
			{
				menu.Items.OfType<MenuItems.Fetch>().FirstOrDefault()?.Execute(menu);
				return true;
			}

			if (!string.IsNullOrWhiteSpace(options.Search))
			{
				menu.Items.OfType<MenuItems.Search>().FirstOrDefault()?.Exec(options.Search, true);
				return true;
			}

			if (!options.Menu)
			{
				menu.Items.OfType<MenuItems.Status>()?.FirstOrDefault()?.Execute(menu);
				return true;
			}

			return false;
		}

		private static void SetTitle(string workPath)
		{
			var asm = Assembly.GetExecutingAssembly();
			var exe = Path.GetFileName(asm.Location);
            var ver = IoC.Resolve<Version>();

			Console.Title = $"{exe} v{ver}: {workPath}";
		}
	}
}
