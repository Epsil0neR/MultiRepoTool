using System;
using MultiRepoTool.ConsoleMenu;

namespace MultiRepoTool.MenuItems;

public class Exit : MenuItem
{
    public Exit()
        : base("Exit")
    {
    }

    public Exit(string title)
        : base(title)
    {
    }

    public override bool Execute(Menu menu)
    {
        Console.WriteLine($"Exited");
        return false;
    }
}