using System;

namespace MultiRepoTool;

public class SearchFilter
{
    private readonly string _filter;
    private readonly Mode _mode;

    private SearchFilter(string filter, Mode mode)
    {
        _filter = filter;
        _mode = mode;
    }

    [Flags]
    private enum Mode 
    {
        Exact = 0,
        Before = 1,
        After = 2,
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="symbol">Symbol to use for filter mode check. On start and end of filter text.</param>
    /// <returns></returns>
    public static SearchFilter CreateFrom(string filter, char symbol = '*')
    {
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));

        var mode = Mode.Exact;
        if (filter.StartsWith(symbol))
            mode |= Mode.Before;
        if (filter.EndsWith(symbol))
            mode |= Mode.After;

        return new(filter.Trim(symbol), mode);
    }

    public bool Matched(string input)
    {
        if (input == null)
            return false;
        if (_mode.HasFlag(Mode.After | Mode.Before))
            return input.Contains(_filter, StringComparison.InvariantCultureIgnoreCase);
        if (_mode.HasFlag(Mode.After) && input.StartsWith(_filter, StringComparison.InvariantCultureIgnoreCase))
            return true;
        if (_mode.HasFlag(Mode.Before) && input.EndsWith(_filter, StringComparison.InvariantCultureIgnoreCase))
            return true;
        return string.Equals(input, _filter, StringComparison.InvariantCultureIgnoreCase);
    }
}