using System;
using System.Collections.Generic;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class Pull : MenuItem
{
    public GitRepositoriesManager Manager { get; }

    public Pull(GitRepositoriesManager manager) 
        : base("Pull")
    {
        Manager = manager;
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Executing {Title}.");
        foreach (var repository in Manager.Repositories)
        {
            ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Pulling ");
            ConsoleUtils.Write(repository.Name, Constants.ColorRepository);
            repository.Pull();
            ConsoleUtils.WriteLine($" {repository.ActiveBranch}", Constants.ColorBranchLocal);
        }
        return true;
    }
}