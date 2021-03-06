using System;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class CheckDiffs : MenuItem
{
    public GitRepositoriesManager Manager { get; }

    public CheckDiffs(GitRepositoriesManager manager)
        : base("Check diffs in 2 branches")
    {
    }


    public override bool Execute(Menu menu)
    {
        var repos = Manager.Repositories
            .Select(x => new CheckableMenuItem<GitRepository>(x.Name, x))
            .ToList<MenuItem>();

        repos.Add(new SeparatorMenuItem());
        repos.Add(new Exit("Done"));

        var reposMenu = new Menu(repos)
        {
            PreventNewLineOnExecution = true
        };
        reposMenu.Run("Select repositories to perform check diff:");

        var repositories = repos
            .OfType<CheckableMenuItem<GitRepository>>()
            .Where(x => x.IsChecked)
            .Select(x => x.Value)
            .ToList();

        foreach (var repository in repositories)
        {
            var l = SelectBranchName(repository, null);
            if (l == null)
                continue;

            var r = SelectBranchName(repository, l.Value.branch);
            if (r == null)
                continue;

            var lName = l.Value.local ? l.Value.branch.Local : l.Value.branch.RemoteBranch;
            var rName = r.Value.local ? r.Value.branch.Local : r.Value.branch.RemoteBranch;

            ConsoleUtils.Write("Left branch: ");
            ConsoleUtils.WriteLine(lName, Constants.ColorBranchLocal);

            ConsoleUtils.Write("Right branch: ");
            ConsoleUtils.WriteLine(rName, Constants.ColorBranchLocal);

            Console.Clear();
            var output = repository.Executor.Execute("Git Diff", string.Format(GitConst.CommandDiff, lName, rName));
            Console.WriteLine(output);
        }

        return true;
    }

    private (GitBranch branch, bool local)? SelectBranchName(GitRepository repository, GitBranch toSkip)
    {
        var items = new List<MenuItem>();
        (GitBranch, bool)? rv = null;
        items.AddRange(
            repository.Branches
                .Where(x => x.HasLocal() && !ReferenceEquals(toSkip, x))
                .Select(x => new MenuItem(x.Local, _ =>
                {
                    rv = (x, true);
                    return false;
                })
                {
                    HideExecutionText = true
                }));
        items.AddRange(
            repository.Branches
                .Where(x => x.HasRemoteBranch() && !ReferenceEquals(toSkip, x))
                .Select(x => new MenuItem(x.RemoteBranch, _ =>
                {
                    rv = (x, false);
                    return false;
                })
                {
                    HideExecutionText = true
                }));

        items.Add(new SeparatorMenuItem());
        items.Add(new Exit("Cancel"));

        var menu = new Menu(items)
        {
            LoopNavigation = true,
            PreventNewLineOnExecution = true
        };
        menu.Run("Select branch:");

        return rv;
    }
}