using System;
using System.Collections.Generic;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;

namespace MultiRepoTool.MenuItems;

public class ClearConsole : MenuItem
{
    public ClearConsole()
        : base("Clear console")
    {
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Executing {Title}.");
        menu.ClearMenu(0);
        return true;
    }
}