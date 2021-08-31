﻿using System;
using System.Collections.Generic;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems
{
	public class Fetch : MenuItem
	{
		public IEnumerable<GitRepository> Repositories { get; }

		public Fetch(IEnumerable<GitRepository> repositories)
			: base("Fetch")
		{
			Repositories = repositories;
		}

		public override bool Execute(Menu menu)
		{
			Console.WriteLine($"Executing {Title}.");
			foreach (var repository in Repositories)
			{
				ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Fetching ");
				ConsoleUtils.WriteLine(repository.Name, Constants.ColorRepository);
				repository.Fetch();
			}
			return true;
		}
	}
}