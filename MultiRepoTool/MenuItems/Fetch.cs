using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;
using System;
using System.Collections.Generic;
using MultiRepoTool.Extensions;

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
				ConsoleUtils.Write(repository.Name, Constants.ColorRepository);
				try
				{
					repository.Fetch();
					ConsoleUtils.Write(" " + repository.ActiveBranch.GetNameWithTrackingInfo(), Constants.ColorBranchLocal);
				}
				finally
				{
					ConsoleUtils.WriteLine();
				}
			}
			return true;
		}
	}
}