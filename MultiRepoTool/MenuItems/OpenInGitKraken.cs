using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiRepoTool.MenuItems
{
	public class OpenInGitKraken : MenuItem
	{
		public IEnumerable<GitRepository> Repositories { get; }

		public OpenInGitKraken(IEnumerable<GitRepository> repositories)
			: base("Open in GitKraken")
		{
			Repositories = repositories;
		}

		public override bool Execute(Menu menu)
		{
			Console.WriteLine($"Executing {Title}.");
			ConsoleUtils.Write("Repository filter (optional): ");
			var filter = ConsoleUtils.ReadLine(ConsoleColor.Red);
			Exec(filter);

			return true;
		}

		public void Exec(string filter, bool writeExecuting = false)
		{
			if (writeExecuting)
			{
				Console.Write($"Executing {Title}. Filter: ");
				ConsoleUtils.WriteLine(filter, ConsoleColor.Red);
			}

			var filters = (filter ?? string.Empty)
				.Split(',')
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToList();
			var opened = 0;

			foreach (var repository in Repositories)
			{
				if (filters.Count > 0 && filters.All(x => !repository.Name.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
					continue;

				ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Opening ");
				ConsoleUtils.WriteLine(repository.Name, Constants.ColorRepository);
				opened++;
				repository.OpenInGitKraken();
			}

			ConsoleUtils.Write("Opened in GitKraken: ");
			ConsoleUtils.WriteLine(opened.ToString(), opened == 0 ? ConsoleColor.Red : ConsoleColor.Green);
		}
	}
}