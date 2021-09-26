using System;
using System.Collections.Generic;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems
{
	public class Pull : MenuItem
	{
		public IEnumerable<GitRepository> Repositories { get; }

		public Pull(IEnumerable<GitRepository> repositories) 
			: base("Pull")
		{
			Repositories = repositories;
		}

		public override bool Execute(Menu menu)
		{
			Console.WriteLine($"Executing {Title}.");
			foreach (var repository in Repositories)
			{
				ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Pulling ");
				ConsoleUtils.Write(repository.Name, Constants.ColorRepository);
				repository.Pull();
				ConsoleUtils.WriteLine($" {repository.ActiveBranch}", Constants.ColorBranchLocal);
			}
			return true;
		}
	}
}