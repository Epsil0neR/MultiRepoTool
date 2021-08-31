using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiRepoTool.ConsoleMenu
{
	public class Menu
	{
		public Menu(IReadOnlyList<MenuItem> items)
		{
			if (items.Count == 0)
				throw new ArgumentException("Menu items must contain at least one menu item.", nameof(items));

			Items = items;
		}

		public IReadOnlyList<MenuItem> Items { get; }
		public MenuItem Selected { get; set; }

		public int CurrentTop { get; private set; }

		public bool LoopNavigation { get; set; }

		public void Run()
		{
			var items = Items.ToList();

			Selected ??= items.First();

			if (Console.CursorLeft != 0)
				WriteLine();
			CurrentTop = Console.CursorTop;

			var @continue = true;
			do
			{
				ClearMenu();

				// Print menu
				WriteLine("Select from menu using arrows up, down, Enter and x (to exit):");
				foreach (var item in items)
				{
					PrintMenuItem(item);
				}

				var navigation = ReadMenuNavigation();
				int index;
				switch (navigation)
				{
					case MenuNavigation.SelectPrevious:
						if (items.All(x => !x.CanExecute))
							break;

						if (Selected == null)
						{
							Selected = items.LastOrDefault(x => x.CanExecute);
							break;
						}

						index = items.IndexOf(Selected);
						Selected = items
									   .Take(index)
									   .LastOrDefault(x => x.CanExecute) ??
								   items
									   .Skip(index)
									   .Last(x => x.CanExecute);

						break;
					case MenuNavigation.SelectNext:
						if (items.All(x => !x.CanExecute))
							break;

						if (Selected == null)
						{
							Selected = items.FirstOrDefault(x => x.CanExecute);
							break;
						}

						index = items.IndexOf(Selected);

						Selected = items
									   .Skip(index + 1)
									   .FirstOrDefault(x => x.CanExecute) ??
								   items
									   .Take(index)
									   .First(x => x.CanExecute);
						break;
					case MenuNavigation.Execute:
						if (Selected == null)
							continue;

						ClearMenu();
						@continue = Selected.Execute(this);

						if (Console.CursorLeft != 0)
							WriteLine();

						if (Console.CursorTop != 0)
							WriteLine();

						CurrentTop = Console.CursorTop;

						break;
					case MenuNavigation.None:
					default:
						@continue = false;
						break;
				}

			} while (@continue);
		}

		private void PrintMenuItem(MenuItem item)
		{
			var isSelected = ReferenceEquals(Selected, item);
			var fore = isSelected ? Console.BackgroundColor : Console.ForegroundColor;
			var back = isSelected ? Console.ForegroundColor : Console.BackgroundColor;
			var text = item.Title;
			Write("  ");
			WriteLine(text, fore, back);
		}

		private static MenuNavigation ReadMenuNavigation()
		{
			var cursorVisible = Console.CursorVisible;
			Console.CursorVisible = false;
			try
			{
				// Handle user input
				do
				{
					var input = Console.ReadKey(false);

					switch (input.Key)
					{
						case ConsoleKey.UpArrow:
							return MenuNavigation.SelectPrevious;
						case ConsoleKey.DownArrow:
							return MenuNavigation.SelectNext;
						case ConsoleKey.Enter:
							return MenuNavigation.Execute;
						case ConsoleKey.X:
							return MenuNavigation.None;
					}
				} while (true);
			}
			finally
			{
				Console.CursorVisible = cursorVisible;
			}
		}

		public void ClearMenu(int? currentTop = null)
		{
			var from = currentTop ?? CurrentTop;
			var last = Console.CursorTop;

			var empty = new string(' ', Console.BufferWidth);
			for (var i = from; i <= last; i++)
			{
				Console.SetCursorPosition(0, i);
				Console.Write(empty);
			}
			Console.SetCursorPosition(0, from);
			if (currentTop.HasValue)
				CurrentTop = currentTop.Value;
		}

		private static void Write(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
		{

			var fore = Console.ForegroundColor;
			var back = Console.BackgroundColor;

			if (foreground.HasValue)
				Console.ForegroundColor = foreground.Value;
			if (background.HasValue)
				Console.BackgroundColor = background.Value;

			Console.Write(text);

			Console.ForegroundColor = fore;
			Console.BackgroundColor = back;
		}

		private static void WriteLine(string text = null, ConsoleColor? foreground = null, ConsoleColor? background = null)
		{
			if (text == null)
			{
				Console.WriteLine();
				return;
			}

			var fore = Console.ForegroundColor;
			var back = Console.BackgroundColor;

			if (foreground.HasValue)
				Console.ForegroundColor = foreground.Value;
			if (background.HasValue)
				Console.BackgroundColor = background.Value;

			Console.WriteLine(text);

			Console.ForegroundColor = fore;
			Console.BackgroundColor = back;
		}
	}
}