using MultiRepoTool.ConsoleMenu;

namespace MultiRepoTool.MenuItems
{
	public class EndActionsSeparator : MenuItem
	{
		public EndActionsSeparator()
			: base("=================")
		{
			CanExecute = false;
			Func = _ => false;
		}
	}
}