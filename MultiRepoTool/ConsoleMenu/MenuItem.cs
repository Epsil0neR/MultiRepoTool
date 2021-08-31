using System;

namespace MultiRepoTool.ConsoleMenu
{
	public class MenuItem
	{
		public string Title { get; }
		public Func<Menu, bool> Func { get; protected set; }

		protected MenuItem(string title)
		{
			Title = title ?? throw new ArgumentNullException(nameof(title));
		}

		public MenuItem(string title, Func<Menu, bool> func)
		: this(title)
		{
			Func = func ?? throw new ArgumentNullException(nameof(func));
		}

		public MenuItem(string title, Func<bool> func)
		: this(title)
		{
			if (func == null)
				throw new ArgumentNullException(nameof(func));

			Func = _ => func();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <paramref name="menu">Menu that executes this menu item.</paramref>
		/// <returns></returns>
		public virtual bool Execute(Menu menu)
		{
			Console.WriteLine($"Executing {Title}.");
			return Func.Invoke(menu);
		}

		public bool CanExecute { get; set; } = true;
	}
}