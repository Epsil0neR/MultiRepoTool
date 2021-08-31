using System;
using System.Collections.Generic;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems
{
	public class Reload : MenuItem
	{
		public IEnumerable<GitRepository> Repositories { get; }

		public Reload(IEnumerable<GitRepository> repositories)
			: base("Reload")
		{
			Repositories = repositories;
		}

		public override bool Execute(Menu menu)
		{
			Console.WriteLine($"Executing {Title}.");
			foreach (var repository in Repositories)
			{
				ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Reloading ");
				ConsoleUtils.WriteLine(repository.Name, Constants.ColorRepository);
				repository.Reload();
			}
			return true;
		}
	}
}