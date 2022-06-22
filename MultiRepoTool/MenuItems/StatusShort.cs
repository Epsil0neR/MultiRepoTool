using System;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class StatusShort : MenuItem
{
    public GitRepositoriesManager Manager { get; }
    public Options Options { get; }

    public StatusShort(GitRepositoriesManager manager, Options options)
        : base("Status short")
    {
        Manager = manager;
        Options = options;
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Executing {Title}.");

        foreach (var repository in Manager.Repositories)
        {
            if (Options.ReloadBeforeStatus)
                repository.Reload();

            ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - ");
            ConsoleUtils.Write(repository.Name, Constants.ColorRepository);
            ConsoleUtils.Write(" ");
            ConsoleUtils.Write(repository.ActiveBranch.GetNameWithTrackingInfo(), Constants.ColorBranchLocal);
            ConsoleUtils.WriteLine();
        }

        return true;
    }
}