using System;
using System.Collections.Generic;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;

namespace MultiRepoTool.MenuItems
{
	public class Exit : MenuItem
	{
		public IEnumerable<GitRepository> Repositories { get; }

		public Exit(IEnumerable<GitRepository> repositories)
			: base("Exit")
		{
			Repositories = repositories;
		}

		public override bool Execute(Menu menu)
		{
			Console.WriteLine($"Exited");
			return false;
		}
	}
}