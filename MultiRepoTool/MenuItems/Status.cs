using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;
using System;
using System.Linq;

namespace MultiRepoTool.MenuItems;

public class Status : MenuItem
{
    public GitRepositoriesManager Manager { get; }
    public Options Options { get; }

    public Status(GitRepositoriesManager manager, Options options)
        : base("Status")
    {
        Manager = manager;
        Options = options;
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Executing {Title}.");

        var longestNameLength = Manager.Repositories.Max(x => x.Name.Length);
        foreach (var repository in Manager.Repositories)
        {
            if (Options.ReloadBeforeStatus)
                repository.Reload();

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
            ConsoleUtils.Write(branch.RemoteBranch, Constants.ColorBranchRemote);
            ConsoleUtils.WriteLine();
            PrintStatus(branch.Status);
            ConsoleUtils.WriteLine();
        }

        return true;
    }

    private static void PrintStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return;

        var lines = status.Split('\n');

        foreach (var line in lines) 
            ConsoleUtils.WriteLine(line, GetColor(line));
    }

    private static ConsoleColor GetColor(string line)
    {
        if (line.IsProjectRelatedFile())
            return ConsoleColor.Blue;

        return ConsoleColor.White;
    }
}