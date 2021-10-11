using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems
{
    public class CleanFolders : MenuItem
    {
        public IEnumerable<GitRepository> Repositories { get; }

        public CleanFolders(IEnumerable<GitRepository> repositories)
            : base("Clean-up folders")
        {
            Repositories = repositories;
        }

        public override bool Execute(Menu menu)
        {
            Console.WriteLine($"Executing {Title}.");

            var filters = RunSubMenu();
            if (!filters.Any())
                return true;

            var cmdFilters = string.Join(',', filters);
            var cmd = $"for /d /r . %%d in ({cmdFilters}) do @if exist \"%%d\" rd /s/q \"%%d\"";
            foreach (var repository in Repositories)
            {
                ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Cleaning ");
                ConsoleUtils.Write(repository.Name, Constants.ColorRepository);
                try
                {
                    repository.Executor.Execute("Clean-up", cmd);
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
                filters.Add(new CheckableMenuItem<string>(filter, filter) { IsChecked = true });
            }

            Create("bin");
            Create("obj");
            Create(".vs");

            var menus = new List<MenuItem>
            {
                new Exit("Execute"),
                new EndActionsSeparator(),
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
}