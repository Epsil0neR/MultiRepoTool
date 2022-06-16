using System.Collections.Generic;
using System.Linq;

namespace MultiRepoTool.Git;

public class GitStatus
{
    public IReadOnlyList<GitStatusItem> Items { get; init; }

    private GitStatus()
    {
    }

    public static GitStatus? FromString(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        var statuses = status
            .Split('\n')
            .Select(x => x.Trim())
            .Select(GitStatusItem.FromString)
            .Where(x => x is not null)
            .ToList();

        var rv = new GitStatus
        {
            Items = statuses
        };

        return rv;
    }
}

public class GitStatusItem
{
    public string Code { get; init; }

    /// <summary>
    /// Current path to file.
    /// </summary>
    public string Path { get; init; }

    /// <summary>
    /// Previous path to file.
    /// </summary>
    public string PathOld { get; init; }

    public static GitStatusItem? FromString(string statusLine)
    {
        if (string.IsNullOrWhiteSpace(statusLine))
            return null;

        var code = statusLine.Substring(0, 2);
        var pathParts = statusLine.Substring(2);
        var parts = pathParts.Split(" -> ");
        (string? path, string? pathOld) = parts.Length == 2 
            ? (parts[0], parts[1]) 
            : (pathParts, null);

        var rv = new GitStatusItem
        {
            Code = code,
            Path = path,
            PathOld = pathOld
        };

        return rv;
    }
}