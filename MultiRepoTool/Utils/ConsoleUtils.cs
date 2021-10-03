using System;

namespace MultiRepoTool.Utils
{
	public static class ConsoleUtils
	{
		public static void SetCursorLeft(int left)
		{
			(int _, var top) = Console.GetCursorPosition();
			Console.SetCursorPosition(left, top);
		}

		public static void Write(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
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

		public static void WriteLine(string text = null, ConsoleColor? foreground = null, ConsoleColor? background = null)
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

		public static string ReadLine(ConsoleColor? foreground = null, ConsoleColor? background = null)
		{
			var fore = Console.ForegroundColor;
			var back = Console.BackgroundColor;

			if (foreground.HasValue)
				Console.ForegroundColor = foreground.Value;
			if (background.HasValue)
				Console.BackgroundColor = background.Value;

			var rv = Console.ReadLine();

			Console.ForegroundColor = fore;
			Console.BackgroundColor = back;

			return rv;
		}
	}
}
