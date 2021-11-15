using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems
{
    public class OpenInGitKraken : MenuItem
    {
        private readonly Action? _delay;
        private uint _counter;
        public IEnumerable<GitRepository> Repositories { get; }
        public Options Options { get; }

        public OpenInGitKraken(IEnumerable<GitRepository> repositories, Options options)
            : base("Open in GitKraken")
        {
            Repositories = repositories;
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
            Console.WriteLine($"Executing {Title}.");

            var items = new List<MenuItem>()
            {
                new("All", ExecuteAll),
                new("Filter", ExecuteWithFilter),
                new("Behind or ahead", ExecuteWithDesync),
                new("Not committed changed", ExecuteWithStatus)
            };
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

        public bool ExecuteAll()
        {
            foreach (var repository in Repositories)
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

            foreach (var repository in Repositories)
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
            var repositories = Repositories
                .Where(x => x.ActiveBranch is not null && (x.ActiveBranch.Ahead > 0 || x.ActiveBranch.Behind > 0))
                .ToList();
            RunSubMenu(repositories, TitleForDesync);
            return false;
        }

        private bool ExecuteWithStatus()
        {
            var repositories = Repositories
                .Where(x => !string.IsNullOrWhiteSpace(x.ActiveBranch?.Status))
                .ToList();
            RunSubMenu(repositories, TitleForStatus);
            return false;
        }

        private string TitleForStatus(GitRepository repository)
        {
            var status = GitStatus.FromString(repository.ActiveBranch?.Status);
            var rv = $"{repository.Name} - {repository.ActiveBranch?.Local}";
            if (status is not null)
            {
                var p = status.Items.Count(x => x.Path.IsProjectRelatedFile());
                var a = status.Items.Count;
                if (p > 0 && p == a)
                    rv = $"{rv}\n- Special: {p:00}";
                else if (p > 0)
                    rv = $"{rv}\n- All: {a:00}. Special: {p:00}";
                else if (a > 0)
                    rv = $"{rv}\n- All: {a:00}";
            }

            return rv;
        }

        private string TitleForDesync(GitRepository repository)
        {
            return $"{repository.Name}\n- {repository.ActiveBranch?.GetNameWithTrackingInfo()}";
        }

        private void RunSubMenu(IReadOnlyList<GitRepository> repositories, Func<GitRepository, string> titleResolver)
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
                new EndActionsSeparator()
            };
            foreach (var repository in repositories)
            {
                bool Open()
                {
                    this.Open(repository);
                    return true;
                }

                var title = titleResolver(repository);
                var mi = new MenuItem(title, Open);
                menus.Add(mi);
            }
            if (repositories.Count > 0)
                menus.Add(new EndActionsSeparator());
            menus.Add(new Exit("Done"));
            var menu = new Menu(menus)
            {
                LoopNavigation = true,
                PreventNewLineOnExecution = true
            };
            menu.Run();
        }
    }
}