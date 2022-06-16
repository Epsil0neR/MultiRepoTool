using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class Search : MenuItem
{
    public GitRepositoriesManager Manager { get; }

    public Search(GitRepositoriesManager manager)
        : base("Search")
    {
        Manager = manager;
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Executing {Title}.");
        ConsoleUtils.Write("Branch filter (optional): ");
        var filter = ConsoleUtils.ReadLine(ConsoleColor.Red);
        Exec(filter);

        return true;
    }

    public void Exec(string filter, bool writeExecuting = false)
    {
        if (writeExecuting)
        {
            Console.Write($"Executing {Title}. Filter: " );
            ConsoleUtils.WriteLine(filter, ConsoleColor.Red);
        }

        var longestNameLength = Manager.Repositories.Max(x => x.Name.Length);
        var result = Manager.Repositories.Search(filter, false);
        var onCorrect = result
            .Where(x => x.Value.Contains(x.Key.ActiveBranch))
            .ToList();
        var toChange = result
            .Except(onCorrect)
            .Where(x => x.Value.Any())
            .ToList();

        ConsoleUtils.WriteLine($"  Already on that branch: {onCorrect.Count}");
        foreach (var (repository, branches) in onCorrect)
        {
            ConsoleUtils.Write($"    {repository.Name}", Constants.ColorRepository);
            ConsoleUtils.SetCursorLeft(longestNameLength + 8);
            ConsoleUtils.Write(repository.ActiveBranch.GetNameWithTrackingInfo(), Constants.ColorBranchLocal);
            ConsoleUtils.WriteLine(
                $" {string.Join("  ", branches.Where(x => !ReferenceEquals(x, repository.ActiveBranch)).Select(GitBranchExtensions.GetNameWithTrackingInfo))}",
                Constants.ColorBranchRemote);
        }

        ConsoleUtils.WriteLine();
        ConsoleUtils.WriteLine($"  Has that branch: {toChange.Count}");
        foreach (var (repository, branches) in toChange)
        {
            ConsoleUtils.Write($"    {repository.Name}", Constants.ColorRepository);
            ConsoleUtils.SetCursorLeft(longestNameLength + 8);
            ConsoleUtils.WriteLine($"{string.Join("  ", branches.Select(GitBranchExtensions.GetNameWithTrackingInfo))}",
                Constants.ColorBranchRemote);
        }

        ConsoleUtils.WriteLine();

        var notFound = result.Where(x => !x.Value.Any()).ToList();
        if (notFound.Any())
        {
            ConsoleUtils.WriteLine($"  Nothing found: {notFound.Count}", ConsoleColor.Red);
            foreach (var (repository, _) in notFound)
            {
                ConsoleUtils.Write($"    {repository.Name}", Constants.ColorRepository);
                ConsoleUtils.SetCursorLeft(longestNameLength + 8);

                if (repository.ActiveBranch != null)
                    ConsoleUtils.Write(repository.ActiveBranch.Local, Constants.ColorBranchLocal);
                else
                    ConsoleUtils.Write("HEAD detached", ConsoleColor.DarkRed);

                Console.WriteLine();
            }
        }
    }
}