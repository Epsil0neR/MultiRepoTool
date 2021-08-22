using System.Collections.Generic;

namespace MultiRepoTool.Extensions
{
	public static class ListExtensions
	{
		public static void ReplaceAll<T>(this List<T> list, IEnumerable<T> items)
		{
			list.Clear();
			list.AddRange(items);
		}
	}
}