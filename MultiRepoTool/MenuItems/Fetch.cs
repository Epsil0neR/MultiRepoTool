using System;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class Fetch : MenuItem
{
    public GitRepositoriesManager Manager { get; }

    public Fetch(GitRepositoriesManager manager)
        : base("Fetch")
    {
        Manager = manager;
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Executing {Title}.");
        foreach (var repository in Manager.Repositories)
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