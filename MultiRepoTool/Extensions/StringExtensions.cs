using System;
using System.Linq;

namespace MultiRepoTool.Extensions;

public static class StringExtensions
{
    private static readonly string[] ProjectRelatedFileExtensions = {
        ".csproj",
        ".sln"
    };

    public static bool IsProjectRelatedFile(this string file)
    {
        if (file is null)
            return false;
        return ProjectRelatedFileExtensions.Any(x => file.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
    }
}