using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems
{
    public class OpenSolution : MenuItem
    {
        public IEnumerable<GitRepository> Repositories { get; }

        public OpenSolution(IEnumerable<GitRepository> repositories)
            : base("Open solution")
        {
            Repositories = repositories;
        }

        public override bool Execute(Menu menu)
        {
            var menuItems = Repositories
                .SelectMany(ToMenuItem)
                .ToList();
            menuItems.Add(new EndActionsSeparator());
            menuItems.Add(new Exit("Done"));

            var reposMenu = new Menu(menuItems)
            {
                PreventNewLineOnExecution = true,
                LoopNavigation = true
            };
            reposMenu.Run();

            return true;
        }

        private IEnumerable<MenuItem> ToMenuItem(GitRepository repository)
        {
            var files = repository.Directory.GetFiles("*.sln", SearchOption.AllDirectories);
        
            if (!files.Any())
                yield break;

            yield return new MenuItem($"=== {repository.Name} ===", _ => false)
            {
                CanExecute = false
            };

            foreach (var file in files)
            {
                bool Handler()
                {
                    ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Opening ");
                    ConsoleUtils.Write(file.FullName, Constants.ColorBranchLocal);
                    ConsoleUtils.Write(" Repository: ");
                    ConsoleUtils.WriteLine(repository.Name, Constants.ColorRepository);
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(file.FullName)
                        {
                            UseShellExecute = true
                        }
                    };
                    p.Start();
                    return false;
                }

                yield return new MenuItem(file.Name, Handler);
            }
        }
    }
}