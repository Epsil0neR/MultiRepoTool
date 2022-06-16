using MultiRepoTool.ConsoleMenu;

namespace MultiRepoTool.MenuItems;

public class SeparatorMenuItem : MenuItem
{
    public SeparatorMenuItem()
        : base("=================")
    {
        CanExecute = false;
        Func = _ => false;
    }
}