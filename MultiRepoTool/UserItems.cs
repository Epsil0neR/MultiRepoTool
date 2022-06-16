using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Utils;

namespace MultiRepoTool;

public class UserItems
{
    public Options Options { get; }
    public IReadOnlyList<MenuItem> MenuItems { get; }

    public UserItems(Options options)
    {
        Options = options;
        var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directory = Path.Combine(rootDirectory, options.UserMenuItemsFolder);
        bool exists = Directory.Exists(directory);
        if (!exists)
        {
            MenuItems = Array.Empty<MenuItem>();
            return;
        }

        var items = new List<MenuItem>();
        var files = Directory.GetFiles(directory);
        foreach (string file in files)
        {
            var info = new FileInfo(file);
            var mi = new MenuItem(info.Name, () => Execute(info));
            items.Add(mi);
        }

        MenuItems = items;
    }

    private bool Execute(FileInfo info)
    {
        switch (info.Extension.ToLowerInvariant())
        {
            case ".cmd":
            case ".bat":
                RunCmd(info);
                break;
            case ".exe":
                RunExe(info);
                break;
            case ".lnk":
                RunLnk(info);
                break;
        }

        return true;
    }

    private void RunCmd(FileInfo info)
    {
        var cmdPath = @"C:\Windows\System32\cmd.exe";
        var arguments = $@"/C ""{info.FullName}""";
        var proc = new Process
        {
            StartInfo =
            {
                FileName = cmdPath,
                Arguments = arguments,
                WorkingDirectory = Options.Path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        proc.Start();
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        ConsoleUtils.WriteLine(output);
    }

    private void RunExe(FileInfo info)
    {
        var proc = new Process
        {
            StartInfo =
            {
                FileName = info.FullName,
                WorkingDirectory = Options.Path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        proc.Start();
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        ConsoleUtils.WriteLine(output);
    }

    private void RunLnk(FileInfo info)
    {
        var proc = new Process
        {
            StartInfo =
            {
                FileName = info.FullName,
                WorkingDirectory = Options.Path,
                //UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        proc.Start();
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        ConsoleUtils.WriteLine(output);
    }
}