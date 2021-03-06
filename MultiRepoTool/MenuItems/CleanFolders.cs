using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class CleanFolders : MenuItem
{
    public GitRepositoriesManager Manager { get; }

    public CleanFolders(GitRepositoriesManager manager)
        : base("Clean-up folders")
    {
        Manager = manager;
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Executing {Title}.");

        var filters = RunSubMenu();
        if (!filters.Any())
            return true;

        foreach (var repository in Manager.Repositories)
        {
            ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Cleaning ");
            ConsoleUtils.Write(repository.Name, Constants.ColorRepository);
            try
            {
                var dir = repository.Executor.WorkingDirectory;
                var allDirs = dir.GetDirectories("*", SearchOption.AllDirectories);
                var filtered = allDirs
                    .Where(x => filters.Contains(x.Name))
                    .ToList();
                foreach (var directory in filtered)
                {
                    try
                    {
                        directory.Delete(true);
                    }
                    catch (Exception) { }
                }
            }
            finally
            {
                ConsoleUtils.WriteLine();
            }
        }

        return true;
    }

    private IReadOnlyList<string> RunSubMenu()
    {
        var filters = new List<CheckableMenuItem<string>>();

        void Create(string filter)
        {
            filters.Add(new(filter, filter) { IsChecked = true });
        }

        Create("bin");
        Create("obj");
        Create(".vs");

        var menus = new List<MenuItem>
        {
            new Exit("Execute"),
            new SeparatorMenuItem(),
        };
        menus.AddRange(filters);
        var menu = new Menu(menus)
        {
            PreventNewLineOnExecution = true,
            LoopNavigation = true
        };
        menu.Run("Select which folders to clean-up:");

        return filters
            .Where(x => x.IsChecked)
            .Select(x => x.Value)
            .ToList();
    }
}