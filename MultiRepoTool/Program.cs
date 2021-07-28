using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MultiRepoTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(WithParsed)
                .WithNotParsed(WithNotParsed);
        }

        private static void WithNotParsed(IEnumerable<Error> enumerable)
        {
            Console.WriteLine("Error parsing arguments.");
            Console.Write("Press any key to exit...");
            Console.ReadKey(false);
        }

        private static void WithParsed(Options options)
        {
            if (string.IsNullOrEmpty(options.Path))
                options.Path = Environment.CurrentDirectory;
            var di = new DirectoryInfo(options.Path);
            if (!di.Exists)
            {
                Console.WriteLine("Path to directory with all repositories not found.");
                Console.Write("Press any key to exit...");
                Console.ReadKey(false);
            }

            var repositories = di.GetDirectories()
                .Where(x => x.GetDirectories().Any(y => y.Name == ".git"))
                .ToList();
            var longestName = repositories.Max(x => x.Name.Length) + 4;

            var hasSearchBranch = new List<string>();
            var onSearchBranch = new List<string>();

            foreach (var repo in repositories)
            {
                var output = ExecuteCommand("Branches", "git branch -a", repo.FullName);
                var allBranches = output.Split('\n');

                Write(repo.Name, ConsoleColor.Yellow);
                (int _, var top) = Console.GetCursorPosition();
                Console.SetCursorPosition(longestName, top);
                Console.WriteLine(repo.FullName);
                if (!string.IsNullOrEmpty(options.SearchBranch))
                {
                    foreach (string branch in allBranches)
                    {
                        bool matchesSearch = branch.Contains(options.SearchBranch, StringComparison.InvariantCultureIgnoreCase);
                        bool isActive = branch.StartsWith('*');

                        if (matchesSearch) 
                            (isActive ? onSearchBranch : hasSearchBranch).Add($"{repo.Name.PadRight(longestName)} - {branch}");

                        if (isActive)
                            WriteLine($"* {branch.Trim('*').Trim()}", ConsoleColor.Red);
                        else if (matchesSearch)
                                Console.WriteLine($"  {branch.Trim()}");
                    }
                }
                else
                {
                    foreach (string branch in allBranches)
                    {
                        if (branch.StartsWith('*'))
                            WriteLine($"* {branch.Trim('*').Trim()}", ConsoleColor.Red);
                        else
                            Console.WriteLine($"  {branch.Trim()}");
                    }
                }

                var changes = ExecuteCommand("Changes", "git status -sb", repo.FullName);
                WriteLine(changes, ConsoleColor.Cyan);

                Console.WriteLine();
            }

            if (!string.IsNullOrEmpty(options.SearchBranch))
            {
                Console.WriteLine();
                Write($"Search results: ");
                WriteLine(options.SearchBranch, ConsoleColor.Red);
                WriteLine($"  On that branch already: {onSearchBranch.Count}");
                foreach (var item in onSearchBranch) 
                    WriteLine($"    {item}", ConsoleColor.Green);

                hasSearchBranch = hasSearchBranch.Except(onSearchBranch).ToList();
                WriteLine($"  Has that branch: {hasSearchBranch.Count}"); 
                foreach (var item in hasSearchBranch) 
                    WriteLine($"    {item}", ConsoleColor.Cyan);
            }

            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey(false);
        }

        private static string ExecuteCommand(string commandName, string command, string workingDirectory)
        {
            var arguments = $@"/C {command}";
            var dir = workingDirectory;
            Process proc = new Process // https://stackoverflow.com/a/22869734/1763586
            {
                StartInfo =
                {
                    FileName = @"C:\Windows\System32\cmd.exe",
                    Arguments = arguments,
                    WorkingDirectory = dir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                }
            };

            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            return output;
        }

        private static void Write(string text, ConsoleColor? color = null)
        {
            var current = Console.ForegroundColor;
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            Console.Write(text);
            Console.ForegroundColor = current;
        }

        private static void WriteLine(string text, ConsoleColor? color = null)
        {
            var current = Console.ForegroundColor;
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            Console.WriteLine(text);
            Console.ForegroundColor = current;
        }
    }

    public class Options
    {
        [Option('p', "path", HelpText = "Path to directory with all repositories.")]
        public string Path { get; set; }

        [Option('b', "branch", HelpText = "Branch to search.")]
        public string SearchBranch { get; set; }
    }
}
