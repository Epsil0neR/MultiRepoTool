using System;
using System.Collections.Generic;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems
{
    public class StatusShort : MenuItem
    {
        public IEnumerable<GitRepository> Repositories { get; }

        public StatusShort(IEnumerable<GitRepository> repositories)
            : base("Status short")
        {
            Repositories = repositories;
        }

        public override bool Execute(Menu menu)
        {
            Console.WriteLine($"Executing {Title}.");

            foreach (var repository in Repositories)
            {
                ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - ");
                ConsoleUtils.Write(repository.Name, Constants.ColorRepository);
                ConsoleUtils.Write(" ");
                ConsoleUtils.Write(repository.ActiveBranch.GetNameWithTrackingInfo(), Constants.ColorBranchLocal);
                ConsoleUtils.WriteLine();
            }

            return true;
        }
    }
}