using System.Collections.Generic;

namespace MultiRepoTool.Extensions;

public static class ListExtensions
{
    public static void ReplaceAll<T>(this List<T> list, IEnumerable<T> items)
    {
        list.Clear();
        list.AddRange(items);
    }

    public static void AddTo<T>(this IEnumerable<T> items, IList<T> target)
    {
        if (target is List<T> list)
            list.AddRange(items);
        else
            foreach (var item in items)
                target.Add(item);
    }
    public static void AddTo<T>(this T item, IList<T> target)
    {
        target.Add(item);
    }
}