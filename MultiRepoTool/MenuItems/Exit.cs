using MultiRepoTool.ConsoleMenu;
using System;

namespace MultiRepoTool.MenuItems
{
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
}