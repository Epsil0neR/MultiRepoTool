using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class OpenSolution : MenuItem
{
    public GitRepositoriesManager Manager { get; }

    public OpenSolution(GitRepositoriesManager manager)
        : base("Open solution")
    {
        Manager = manager;
    }

    public override bool Execute(Menu menu)
    {
        Execute().GetAwaiter().GetResult();
        return true;
    }

    private async Task Execute()
    {
        var filesPerRepository = await Task.WhenAll(Manager.Repositories.Select(x => x.SolutionFiles));
        var menus = new List<MenuItem>();
        for (int index = 0; index < filesPerRepository.Length; index++)
        {
            var files = filesPerRepository[index];
            var repo = Manager.Repositories.ElementAt(index);

            menus.AddRange(ToMenuItem(repo, files));
        }

        menus.Add(new SeparatorMenuItem());
        menus.Add(new Exit("Done"));

        var reposMenu = new Menu(menus)
        {
            PreventNewLineOnExecution = true,
            LoopNavigation = true
        };
        reposMenu.Run();
    }

    private IEnumerable<MenuItem> ToMenuItem(GitRepository repository, IReadOnlyList<FileInfo> files)
    {
        if (!files.Any())
            yield break;

        yield return new($"=== {repository.Name} ===", _ => false)
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
                    StartInfo = new(file.FullName)
                    {
                        UseShellExecute = true
                    }
                };
                p.Start();
                return false;
            }

            yield return new(file.Name, Handler);
        }
    }
}