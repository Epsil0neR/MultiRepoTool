using MultiRepoTool.Git;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MultiRepoTool.Extensions
{
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
}