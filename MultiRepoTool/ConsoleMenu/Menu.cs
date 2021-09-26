using MultiRepoTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Extensions;

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

		public bool LoopNavigation { get; set; } //TODO: Not used :(

		public bool PreventNewLineOnExecution { get; set; }

		public void Run(string menuText = "Select from menu using arrows up, down, Enter and first symbol of menu item:")
		{
			var items = Items.ToList();

			Selected ??= items.First();

			if (Console.CursorLeft != 0)
				ConsoleUtils.WriteLine();
			CurrentTop = Console.CursorTop;

			var @continue = true;
			do
			{
				ClearMenu();

				// Print menu
				ConsoleUtils.WriteLine(menuText);
				foreach (var item in items)
				{
					PrintMenuItem(item);
				}

				var navigation = ReadMenuNavigation(out var symbol);
				int index;
				List<MenuItem> list;
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
						list = items
							.Take(index)
							.Where(x => x.CanExecute)
							.Reverse()
							.ToList();
						if (LoopNavigation)
							items
								.Skip(index + 1)
								.Where(x=>x.CanExecute)
								.Reverse()
								.AddTo(list);

						if (!symbol.HasValue)
						{
							Selected = list.FirstOrDefault() ?? Selected;
						}
						else
						{
							var filter = char.ToLowerInvariant(symbol.Value).ToString();
							Selected = list.FirstOrDefault(x => x.Title.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase)) ?? Selected;
						}

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
						list = items
							.Skip(index + 1)
							.Where(x=>x.CanExecute)
							.ToList();
						if (LoopNavigation)
							items
								.Take(index)
								.Where(x => x.CanExecute)
								.AddTo(list);

						if (!symbol.HasValue)
						{
							Selected = list.FirstOrDefault() ?? Selected;
						}
						else
						{
							var filter = char.ToLowerInvariant(symbol.Value).ToString();
							Selected = list.FirstOrDefault(x=>x.Title.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase)) ?? Selected;
						}

						break;
					case MenuNavigation.Execute:
						if (Selected == null)
							continue;

						ClearMenu();
						@continue = Selected.Execute(this);

						if (Console.CursorLeft != 0)
							ConsoleUtils.WriteLine();

						if (Console.CursorTop != 0 && !PreventNewLineOnExecution)
							ConsoleUtils.WriteLine();

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
			ConsoleUtils.Write("  ");
			ConsoleUtils.WriteLine(text, fore, back);
		}

		private static MenuNavigation ReadMenuNavigation(out char? symbol)
		{
			var cursorVisible = Console.CursorVisible;
			symbol = null;
			Console.CursorVisible = false;
			try
			{
				// Handle user input
				do
				{
					var input = Console.ReadKey(true);

					switch (input.Key)
					{
						case ConsoleKey.UpArrow:
							return MenuNavigation.SelectPrevious;
						case ConsoleKey.DownArrow:
							return MenuNavigation.SelectNext;
						case ConsoleKey.Enter:
							return MenuNavigation.Execute;
						default:
							if (char.IsLetter(input.KeyChar))
							{
								symbol = input.KeyChar;

								return input.Modifiers.HasFlag(ConsoleModifiers.Shift) 
									? MenuNavigation.SelectPrevious 
									: MenuNavigation.SelectNext;
							}
							continue;
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

	}
}