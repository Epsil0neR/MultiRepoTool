﻿using MultiRepoTool.Git;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiRepoTool.Extensions
{
	public static class GitExtensions
	{
	}

	public static class GitRepositoryExtensions
	{
		public static IEnumerable<GitBranch> Search(this GitRepository repository, string query, bool includeActive)
		{
			foreach (var branch in repository.Branches)
			{
				var name = branch.HasLocal() ? branch.Local : branch.Remote;
				if (name.Contains(query, StringComparison.InvariantCultureIgnoreCase))
					yield return branch;
				else if (includeActive && branch.IsActive)
					yield return branch;
			}
		}

		public static Dictionary<GitRepository, IEnumerable<GitBranch>> Search(this IEnumerable<GitRepository> repositories, string query, bool includeActive)
		{
			return repositories.ToDictionary(x => x, x => x.Search(query, includeActive));
		}
	}

	public static class GitBranchExtensions
	{
		/// <summary>
		/// Checks if <paramref name="branch"/> exists locally.
		/// </summary>
		/// <param name="branch"></param>
		/// <returns></returns>
		public static bool HasLocal(this GitBranch branch)
		{
			if (branch == null)
				throw new ArgumentNullException(nameof(branch));

			return !string.IsNullOrEmpty(branch.Local);
		}

		/// <summary>
		/// Checks if <paramref name="branch"/> exists in remote.
		/// </summary>
		/// <param name="branch"></param>
		/// <returns></returns>
		public static bool HasRemote(this GitBranch branch)
		{
			if (branch == null)
				throw new ArgumentNullException(nameof(branch));

			return !string.IsNullOrEmpty(branch.Remote);
		}

		/// <summary>
		/// Checks if branch contains only remote information.
		/// </summary>
		/// <param name="branch"></param>
		/// <returns></returns>
		public static bool IsRemoteOnly(this GitBranch branch)
		{
			return !HasLocal(branch) && HasRemote(branch);
		}

		/// <summary>
		/// Checks if branch contains only local information.
		/// </summary>
		/// <param name="branch"></param>
		/// <returns></returns>
		public static bool IsLocalOnly(this GitBranch branch)
		{
			return HasLocal(branch) && !HasRemote(branch);
		}
	}
}