using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Utils;

namespace MultiRepoTool.ConsoleMenu;

public class MenuItem
{
    public string Title { get; protected set; }

    public IReadOnlyList<ColoredTextPart> ColoredTitle { get; protected set; }

    public Func<Menu, bool> Func { get; protected set; }

    public bool HideExecutionText { get; set; }

    protected MenuItem(string title)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    protected MenuItem(IReadOnlyList<ColoredTextPart> title)
    {
        if (title?.Any() != true)
            throw new ArgumentNullException(nameof(title));
        ColoredTitle = title;
    }

    public MenuItem(string title, Func<Menu, bool> func)
        : this(title)
    {
        Func = func ?? throw new ArgumentNullException(nameof(func));
    }
    public MenuItem(IReadOnlyList<ColoredTextPart> title, Func<Menu, bool> func)
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

    public MenuItem(IReadOnlyList<ColoredTextPart> title, Func<bool> func)
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
        if (!HideExecutionText)
            Console.WriteLine($"Executing {Title}.");
        return Func.Invoke(menu);
    }

    public bool CanExecute { get; set; } = true;

    public void WriteTitle(bool isSelected, string lineStart)
    {
        if (Title is not null)
        {
            var fore = isSelected ? Console.BackgroundColor : Console.ForegroundColor;
            var back = isSelected ? Console.ForegroundColor : Console.BackgroundColor;

            var lines = Title.Split('\n');
            foreach (var line in lines)
            {
                if (lineStart is not null)
                    ConsoleUtils.Write(lineStart);
                ConsoleUtils.WriteLine(line, fore, back);
            }
        }
        else if (ColoredTitle?.Any() == true)
        {
            if (lineStart is not null)
                ConsoleUtils.Write(lineStart);
            foreach (var part in ColoredTitle)
            {
                var fore = isSelected ? part.Background : part.Foreground;
                var back = isSelected ? part.Foreground : part.Background;
                var text = part.Text.Replace("\n", "\n" + lineStart);
                ConsoleUtils.Write(text, fore, back);
            }
            Console.WriteLine();
        }
    }

    public bool MatchesFilter(string filter)
    {
        if (Title is not null)
            return Title.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase);

        if (ColoredTitle?.Any() == true)
        {
            var title = string.Join(null, ColoredTitle.Select(x => x.Text));
            return title.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase);
        }

        return false;
    }
}

public struct ColoredTextPart
{
    public ColoredTextPart(
        string text,
        ConsoleColor? foreground = null,
        ConsoleColor? background = null)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Foreground = foreground ?? Console.ForegroundColor;
        Background = background ?? Console.BackgroundColor;
    }

    public string Text { get; }
    public ConsoleColor Foreground { get; }
    public ConsoleColor Background { get; }
}