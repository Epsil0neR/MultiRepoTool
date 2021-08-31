using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiRepoTool.MenuItems
{
	public class Status : MenuItem
	{
		public IEnumerable<GitRepository> Repositories { get; }

		public Status(IEnumerable<GitRepository> repositories)
			: base("Status")
		{
			Repositories = repositories;
		}

		public override bool Execute(Menu menu)
		{
			Console.WriteLine($"Executing {Title}.");

			var longestNameLength = Repositories.Max(x => x.Name.Length);
			foreach (var repository in Repositories)
			{
				var branch = repository.ActiveBranch;
				if (branch == null)
				{
					ConsoleUtils.WriteLine("HEAD detached", ConsoleColor.DarkRed);
					continue;
				}

				ConsoleUtils.Write(repository.Name, Constants.ColorRepository);
				ConsoleUtils.SetCursorLeft(longestNameLength + 8);
				ConsoleUtils.Write($" {branch.GetNameWithTrackingInfo()}", Constants.ColorBranchLocal);
				ConsoleUtils.Write("...");
				ConsoleUtils.Write(branch.Remote, Constants.ColorBranchRemote);
				ConsoleUtils.WriteLine();
				if (!string.IsNullOrWhiteSpace(branch.Status))
					ConsoleUtils.WriteLine(branch.Status);
				ConsoleUtils.WriteLine();
			}

			return true;
		}
	}
}

