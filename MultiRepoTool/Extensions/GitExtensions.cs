using System;
using MultiRepoTool.Git;

namespace MultiRepoTool.Extensions
{
	public static class GitExtensions
	{
		/// <summary>
		/// Checks if branch contains only remote information.
		/// </summary>
		/// <param name="branch"></param>
		/// <returns></returns>
		public static bool IsRemoteOnly(this GitBranch branch)
		{
			if (branch == null)
				throw new ArgumentNullException(nameof(branch));

			if (string.IsNullOrEmpty(branch.Remote))
				return false;

			return string.IsNullOrEmpty(branch.Local);
		}

		/// <summary>
		/// Checks if branch contains only local information.
		/// </summary>
		/// <param name="branch"></param>
		/// <returns></returns>
		public static bool IsLocalOnly(this GitBranch branch)
		{
			if (branch == null)
				throw new ArgumentNullException(nameof(branch));

			if (string.IsNullOrEmpty(branch.Local))
				return false;

			return string.IsNullOrEmpty(branch.Remote);
		}
	}
}