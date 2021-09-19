using MultiRepoTool.Git;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiRepoTool.Extensions
{
	public static class GitExtensions
	{
	}

	public static class GitRepositoryExtensions
	{
		public static IEnumerable<GitBranch> Search(this GitRepository repository, IReadOnlyList<string> filters, bool includeActive)
		{
			foreach (var branch in repository.Branches)
			{
				var name = branch.HasLocal()
					? branch.Local
					: branch.Remote;

				if (branch.IsActive && includeActive)
					yield return branch;

				if (filters.Any(x => name.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
					yield return branch;
			}
		}

		public static IEnumerable<GitBranch> Search(this GitRepository repository, string query, bool includeActive)
		{
			foreach (var branch in repository.Branches)
			{
				var name = branch.HasLocal() ? branch.Local : branch.Remote;
				if (branch.IsActive && includeActive)
					yield return branch;

				if (name.Contains(query, StringComparison.InvariantCultureIgnoreCase))
					yield return branch;
			}
		}

		public static Dictionary<GitRepository, IEnumerable<GitBranch>> Search(this IEnumerable<GitRepository> repositories, string query, bool includeActive)
		{
			var filters = (query ?? string.Empty)
				.Split(',')
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrEmpty(x))
				.ToList();
			return repositories.ToDictionary(x => x, x => x.Search(filters, includeActive));
		}

		public static Task OpenInGitKraken(this GitRepository repository)
		{
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var exePath = $"{localAppData}/gitkraken/update.exe";
			var param = "--processStart=gitkraken.exe --process-start-args=\"-p \\\"{0}\\\"\"";

			var process = new Process
			{
				StartInfo =
				{
					FileName = exePath,
					Arguments = string.Format(param, repository.Directory.FullName),
					UseShellExecute = false,
					RedirectStandardOutput = true,
					WindowStyle = ProcessWindowStyle.Hidden
				}
			};
			process.Start();
			return process.WaitForExitAsync();
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

		public static string GetNameWithTrackingInfo(this GitBranch branch)
		{
			if (branch == null)
				return string.Empty;

			var name = branch.HasLocal() ? branch.Local : branch.Remote;
			if (branch.Behind == 0 && branch.Ahead == 0) return name;
			var sb = new StringBuilder();
			sb.Append(name);
			if (branch.Ahead > 0) sb.Append($"[A:{branch.Ahead}]");
			if (branch.Behind > 0) sb.Append($"[B:{branch.Behind}]");
			return sb.ToString();
		}
	}
}