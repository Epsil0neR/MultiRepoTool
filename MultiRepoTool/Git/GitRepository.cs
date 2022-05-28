using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MultiRepoTool.Extensions;

namespace MultiRepoTool.Git
{
	public static class GitConst
	{
		public const string GitSubFolder = ".git";
		public const string CommandPull = "git pull";
		public const string CommandFetch = "git fetch --all --quiet";
		public const string CommandPush = "git push";
		public const string CommandBranchStatus = "git status -sb";
		public const string CommandListBranchesLocal = "git branch -l";
		public const string CommandListBranchesRemote = "git branch -r";
		public const string CommandCurrentBranch = "git branch --show-current";
		public const string CommandListRemoteNames = "git remote";
		public const string CommandDiff = "git log --pretty=%B {0}...{1}";
	}

	public class GitRepository
	{
		private readonly List<GitBranch> _branches = new();
		private GitBranch _activeBranch;

		public CommandExecutor Executor { get; }

        /// <summary>
        /// Repository name.
        /// </summary>
		public string Name { get; }

		/// <summary>
		/// Directory where GIT repository is located.
		/// </summary>
		public DirectoryInfo Directory { get; }

        /// <summary>
        /// Remote branches.
        /// </summary>
		public IEnumerable<GitRemote> Remotes { get; }

        /// <summary>
        /// All .sln files in repository.
        /// </summary>
        public Task<IReadOnlyList<FileInfo>> SolutionFiles { get; private set; }

        /// <summary>
        /// Gets GIT repository from <paramref name="directory"/> or null if directory is not a GIT repository.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static GitRepository FromDirectory(DirectoryInfo directory, string name = null)
        {
			if (directory == null)
				return null;

			if (!directory.Exists)
				return null;

			// Check if directory contains GIT directory.
			if (directory.GetDirectories().All(x => x.Name != GitConst.GitSubFolder))
				return null;

			return new GitRepository(directory, name);
		}

		private GitRepository(DirectoryInfo directory, string name = null)
		{
            Directory = directory;
			Executor = new CommandExecutor(directory);

			Name = string.IsNullOrWhiteSpace(name) ? Directory.Name : name;
			Remotes = GetRemotes().ToList();
			_branches.ReplaceAll(GetBranches());
			ActiveBranch = Branches.FirstOrDefault(x => x.IsActive);
            FetchSolutionFiles();
        }

        public IReadOnlyList<GitBranch> Branches => _branches;

		public GitBranch ActiveBranch
		{
			get => _activeBranch;
			private set
			{
				if (ReferenceEquals(_activeBranch, value))
					return;

				if (_activeBranch != null)
					_activeBranch.IsActive = false;
				if (value != null)
					value.IsActive = true;

				_activeBranch = value;

			}
		}

		private IEnumerable<GitBranch> GetBranches()
		{
			uint GetValueFromTrack(string track, string keyword)
			{
				if (string.IsNullOrEmpty(track))
					return 0;

				uint rv = 0;
				var ind = track.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase);
				if (ind < 0)
					return 0;

				var start = ind + keyword.Length;
				var length = 1;
				do
				{
					var value = track.Substring(start, length);
					if (uint.TryParse(value, out var v))
						rv = v;
					else
						break;
					length++;
				} while (true);

				return rv;
			}

			var currentBranch = Executor.Execute("Current branch", GitConst.CommandCurrentBranch)
				.Trim('\r', '\n', ' ');
			var remoteBranchesOutput = Executor.Execute("Remote branches", GitConst.CommandListBranchesRemote);
			List<string> ParseOutput(string output) =>
				output
					.Split("\n")
					.Select(x => x.Trim(' ', '*'))
					.Where(x => !string.IsNullOrEmpty(x))
					.ToList();

			var remotes = ParseOutput(remoteBranchesOutput);
			var localsDataOutput = Executor.Execute("Locals to remotes with track",
				"git for-each-ref --format=\"%(refname:short);%(upstream:short);%(push:track);%(upstream:track)\" refs/heads");
			// Command above, but without string escaping:
			// git for-each-ref --format="%(refname:short);%(upstream:short);%(push:track)" refs/heads
			var localsData = ParseOutput(localsDataOutput);
			foreach (var localData in localsData)
			{
				var data = localData.Split(';');
				var local = data[0];
				var remoteBranch = data[1];
                var trackPush = data[2];
                var trackUpstream = data[3];
                var remote = string.IsNullOrEmpty(remoteBranch) ? string.Empty : remoteBranch.Split("/")[0];
				var branch = new GitBranch(this)
				{
					Local = local,
					RemoteBranch = remoteBranch,
                    Remote = remote,
					Ahead = Math.Max(GetValueFromTrack(trackPush, "ahead "), GetValueFromTrack(trackUpstream, "ahead ")),
					Behind = Math.Max(GetValueFromTrack(trackPush, "behind "), GetValueFromTrack(trackUpstream, "behind ")),
				};

				if (currentBranch == local)
				{
					var status = Executor.Execute("Branch status", GitConst.CommandBranchStatus)
						.TrimEnd(' ', '\r', '\n');

					branch.Status = string.Join('\n',
						status
							.Split('\n')
							.Skip(1));

					ActiveBranch = branch;
				}

				remotes.Remove(remoteBranch);
				yield return branch;
			}

			foreach (var remote in remotes)
			{
				var branch = new GitBranch(this)
				{
					RemoteBranch = remote
				};
				yield return branch;
			}

		}

		/// <summary>
		/// Gets all info about remotes for current repository.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<GitRemote> GetRemotes()
		{
			var remoteNamesOutput = Executor.Execute("List remotes names", GitConst.CommandListRemoteNames);
			var remoteNames = remoteNamesOutput
				.Split("\n")
				.Select(x => x.Trim(' ', '\r'))
				.Where(x => !string.IsNullOrEmpty(x));
			foreach (var name in remoteNames)
			{
				var remote = GitRemote.FromName(name, this);
				if (remote != null)
					yield return remote;
			}
		}

		public void Fetch()
		{
			Executor.Execute("Fetch repository", GitConst.CommandFetch);
			Reload();
		}

		public void Pull()
		{
			Executor.Execute("Pull repository", GitConst.CommandPull);
			Reload();
		}

		public void Reload()
		{
			foreach (var branch in GetBranches())
			{
				// 1. Find old branch that matches by Local or by Remote:
				var oldLocal = _branches.FirstOrDefault(x => x.HasLocal() && x.Local == branch.Local);
				var oldRemote = _branches.FirstOrDefault(x => x.HasRemoteBranch() && x.RemoteBranch == branch.RemoteBranch);

				// 2. Remove old remote in case local and remote now are the same branch.
				if (oldRemote != null && oldLocal != null && !ReferenceEquals(oldRemote, oldLocal))
					_branches.Remove(oldRemote);

				var old = oldLocal ?? oldRemote;
				// 2. Update all in found branch:
				if (old != null)
				{
					var ind = _branches.IndexOf(old);
					_branches[ind] = branch;
					//TODO: Dispose old (local and remote) branch.
				}
				else
				{
					_branches.Add(branch);
				}
			}
			ActiveBranch = _branches.FirstOrDefault(x => x.IsActive);
            FetchSolutionFiles();
		}

        private void FetchSolutionFiles()
        {
            SolutionFiles = Task.Run(() => (IReadOnlyList<FileInfo>) Directory.GetFiles("*.sln", SearchOption.AllDirectories));
        }
    }
}