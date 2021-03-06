using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class OpenInGitKraken : MenuItem
{
    private readonly Action? _delay;
    private uint _counter;
    public GitRepositoriesManager Manager { get; }
    public Options Options { get; }

    public OpenInGitKraken(GitRepositoriesManager manager, Options options)
        : base("Open in GitKraken")
    {
        Manager = manager;
        Options = options;

        _delay = null;
        if (Options.DelayOpenInGitKraken > 0)
        {
            var d = (int) Math.Min(10000, Options.DelayOpenInGitKraken);
            _delay = () => Task.Delay(d).Wait();
        }
    }

    public override bool Execute(Menu menu)
    {
        _counter = 0;
        Console.WriteLine($"Executing {Title}.");

        var items = new List<MenuItem>()
        {
            new("All", ExecuteAll),
            new("Filter", ExecuteWithFilter),
            new("Behind or ahead", ExecuteWithDesync),
            new("Not committed changed", ExecuteWithStatus)
        };
        items.AddRange(CreateRootMenuItems());
        var subMenu = new Menu(items)
        {
            LoopNavigation = true,
            PreventNewLineOnExecution = true
        };
        subMenu.Run();


        ConsoleUtils.Write("Opened in GitKraken: ");
        ConsoleUtils.WriteLine(_counter.ToString(), _counter == 0 ? ConsoleColor.Red : ConsoleColor.Green);

        return true;
    }

    private IEnumerable<MenuItem> CreateRootMenuItems()
    {
        yield return new SeparatorMenuItem();
        foreach (var repo in Manager.Repositories)
        {
            var title = new List<ColoredTextPart>()
            {
                new(repo.Name, Constants.ColorRepository)
            };
            var b = repo.ActiveBranch;
            if (b is not null)
            {
                title.Add(new(" - "));
                title.Add(new(b.GetNameWithTrackingInfo(), Constants.ColorBranchLocal));
            }

            yield return new(title, () =>
            {
                Open(repo);
                return true;
            });
        }
        yield return new SeparatorMenuItem();
        yield return new Exit("Done");
    }

    public bool ExecuteAll()
    {
        foreach (var repository in Manager.Repositories)
            Open(repository);

        return false;
    }


    public bool ExecuteWithFilter()
    {
        ConsoleUtils.Write("Repository filter (optional): ");
        var filter = ConsoleUtils.ReadLine(ConsoleColor.Red);
        var filters = (filter ?? string.Empty)
            .Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => SearchFilter.CreateFrom(x))
            .ToList();

        foreach (var repository in Manager.Repositories)
        {
            if (filters.Count > 0 && filters.All(x => !x.Matched(repository.Name)))
                continue;

            Open(repository);
        }

        return false;
    }

    private void Open(GitRepository repository)
    {
        ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Opening ");
        ConsoleUtils.WriteLine(repository.Name, Constants.ColorRepository);
        repository.OpenInGitKraken();
        _counter++;
        _delay?.Invoke();
    }

    private bool ExecuteWithDesync()
    {
        if (Options.ReloadBeforeStatus)
            foreach (var repository in Manager.Repositories)
                repository.Reload();

        var repositories = Manager.Repositories
            .Where(x => x.ActiveBranch is not null && (x.ActiveBranch.Ahead > 0 || x.ActiveBranch.Behind > 0))
            .ToList();
        RunSubMenu(repositories, TitleForDesync);
        return false;
    }

    private bool ExecuteWithStatus()
    {
        if (Options.ReloadBeforeStatus)
            foreach (var repository in Manager.Repositories)
                repository.Reload();

        var repositories = Manager.Repositories
            .Where(x => !string.IsNullOrWhiteSpace(x.ActiveBranch?.Status))
            .ToList();
        RunSubMenu(repositories, ColoredTitleForStatus);
        return false;
    }

    public MenuItem ColoredTitleForStatus(GitRepository repository, Func<bool> func)
    {
        var rv = new List<ColoredTextPart>()
        {
            new(repository.Name, Constants.ColorRepository),
        };
        var b = repository.ActiveBranch;
        if (b is not null)
        {
            rv.Add(new(" - "));
            if (b.HasLocal())
                rv.Add(new(b.Local, Constants.ColorBranchLocal));
            else
                rv.Add(new(b.RemoteBranch, Constants.ColorBranchRemote));
        }
        var status = GitStatus.FromString(repository.ActiveBranch?.Status);
        if (status is null)
            return new(rv, func);

        var p = status.Items.Count(x => x.Path.IsProjectRelatedFile());
        var a = status.Items.Count;
        if (p > 0 && p == a)
        {
            rv.Add(new(" - "));
            rv.Add(new($"Special: {p:D2}", ConsoleColor.Blue));
        }
        else if (p > 0)
        {
            rv.Add(new(" - "));
            rv.Add(new($"All: {a:D2}", ConsoleColor.Red));
            rv.Add(new(" - "));
            rv.Add(new($"Special: {p:D2}", ConsoleColor.Blue));
        }
        else if (a > 0)
        {
            rv.Add(new(" - "));
            rv.Add(new($"All: {a:D2}", ConsoleColor.Red));
        }

        return new(rv, func);
    }

    private MenuItem TitleForDesync(GitRepository repository, Func<bool> func)
    {
        var title = new List<ColoredTextPart>()
        {
            new(repository.Name, Constants.ColorRepository),
        };
        var b = repository.ActiveBranch;
        if (b is not null)
        {
            title.Add(new(" - "));
            title.Add(new(b.GetNameWithTrackingInfo(), Constants.ColorBranchLocal));
        }

        return new(title, func);
    }

    private void RunSubMenu(IReadOnlyList<GitRepository> repositories, Func<GitRepository, Func<bool>, MenuItem> menuItemResolver)
    {
        bool OpenAll()
        {
            foreach (var repository in repositories)
                Open(repository);

            return false;
        }
        var menus = new List<MenuItem>()
        {
            new("All", OpenAll),
            new SeparatorMenuItem()
        };
        var itemsWithCodeChanges = new List<MenuItem>();
        foreach (var repository in repositories)
        {
            bool Open()
            {
                this.Open(repository);
                return true;
            }

            var mi = menuItemResolver(repository, Open);
            menus.Add(mi);

            if (mi.ColoredTitle?.Any(x=>x.Foreground == ConsoleColor.Red) == true)
                itemsWithCodeChanges.Add(mi);
        }
        if (repositories.Count > 0)
            menus.Add(new SeparatorMenuItem());
        menus.Add(new Exit("Done"));

        if (itemsWithCodeChanges.Count > 0)
        {
            var mi = new MenuItem("All with code changes", menu =>
            {
                foreach (var item in itemsWithCodeChanges)
                {
                    item.Execute(menu);
                }

                return false;
            });
            menus.Insert(1, mi);
        }

        var menu = new Menu(menus)
        {
            LoopNavigation = true,
            PreventNewLineOnExecution = true
        };
        menu.Run();
    }
}