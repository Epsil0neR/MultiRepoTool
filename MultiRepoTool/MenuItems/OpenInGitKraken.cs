using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems
{
    public class OpenInGitKraken : MenuItem
    {
        public IEnumerable<GitRepository> Repositories { get; }
        public Options Options { get; }

        public OpenInGitKraken(IEnumerable<GitRepository> repositories, Options options)
            : base("Open in GitKraken")
        {
            Repositories = repositories;
            Options = options;
        }

        public override bool Execute(Menu menu)
        {
            Console.WriteLine($"Executing {Title}.");
            ConsoleUtils.Write("Repository filter (optional): ");
            var filter = ConsoleUtils.ReadLine(ConsoleColor.Red);
            Exec(filter);

            return true;
        }

        public void Exec(string filter, bool writeExecuting = false)
        {
            if (writeExecuting)
            {
                Console.Write($"Executing {Title}. Filter: ");
                ConsoleUtils.WriteLine(filter, ConsoleColor.Red);
            }

            var filters = (filter ?? string.Empty)
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => SearchFilter.CreateFrom(x))
                .ToList();
            var opened = 0;

            Action? delay = null;
            if (Options.DelayOpenInGitKraken > 0)
            {
                var d = (int) Math.Min(10000, Options.DelayOpenInGitKraken);
                delay = () => Task.Delay(d).Wait();
            }

            foreach (var repository in Repositories)
            {
                if (filters.Count > 0 && filters.All(x => !x.Matched(repository.Name)))
                    continue;

                ConsoleUtils.Write($"{DateTime.Now:HH:mm:ss.fff} - Opening ");
                ConsoleUtils.WriteLine(repository.Name, Constants.ColorRepository);
                opened++;
                repository.OpenInGitKraken();
                delay?.Invoke();
            }

            ConsoleUtils.Write("Opened in GitKraken: ");
            ConsoleUtils.WriteLine(opened.ToString(), opened == 0 ? ConsoleColor.Red : ConsoleColor.Green);
        }
    }
}