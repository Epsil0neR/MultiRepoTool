using System;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class Reload : MenuItem
{
    public GitRepositoriesManager Manager { get; }

    public Reload(GitRepositoriesManager manager)
        : base("Reload")
    {
        Manager = manager;
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Executing {Title}.");
        foreach (var repository in Manager.Repositories)
        {
            ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Reloading ");
            ConsoleUtils.WriteLine(repository.Name, Constants.ColorRepository);
            repository.Reload();
        }
        return true;
    }
}