using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MultiRepoTool.Git
{
	public static class GitConst
	{
		public const string GitSubFolder = ".git";
		public const string CommandPull = "git pull";
		public const string CommandFetch = "git fetch";
		public const string CommandPush = "git push";
		public const string CommandListBranchesLocal = "git branch -l";
		public const string CommandListBranchesRemote = "git branch -r";
		public const string CommandCurrentBranch = "git branch --show-current";
		public const string CommandListRemoteNames = "git remote";
	}

	public class GitRepository
	{
		public CommandExecutor Executor { get; }

		public string Name { get; }

		/// <summary>
		/// Directory where GIT repository is located.
		/// </summary>
		public DirectoryInfo Directory { get; }

		public IEnumerable<GitRemote> Remotes { get; }

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
			Branches = GetBranches(out var active);
			ActiveBranch = active;
		}

		public IReadOnlyList<GitBranch> Branches { get; set; } //TODO: Check setter access.
		public GitBranch ActiveBranch { get; set; } //TODO: Check setter access.

		private IReadOnlyList<GitBranch> GetBranches(out GitBranch active)
		{
			uint GetValueFromTrack(string track, string keyword)
			{
				if (string.IsNullOrEmpty(track))
					return 0;

				uint rv = 0;
				var ind = track.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase);
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
				.Trim('\r','\n', ' ');
			var localBranchesOutput = Executor.Execute("Local branches", GitConst.CommandListBranchesLocal);
			var remoteBranchesOutput = Executor.Execute("Remote branches", GitConst.CommandListBranchesRemote);
			List<string> ParseOutput(string output) =>
				output
					.Split("\n")
					.Select(x => x.Trim(' ', '*'))
					.Where(x => !string.IsNullOrEmpty(x))
					.ToList();

			ParseOutput(localBranchesOutput);
			var remotes = ParseOutput(remoteBranchesOutput);

			//TODO: For now it works only with 1 remote. What will happen when repo will have multiple remotes?
			active = null;
			var rv = new List<GitBranch>();

			var localsDataOutput = Executor.Execute("Locals to remotes with track",
				"git for-each-ref --format=\"%(refname:short);%(upstream:short);%(push:track)\" refs/heads");
			// Command above, but without string escaping:
			// git for-each-ref --format="%(refname:short);%(upstream:short);%(push:track)" refs/heads
			var localsData = ParseOutput(localsDataOutput);
			foreach (var localData in localsData)
			{
				var data = localData.Split(';');
				var local = data[0];
				var remote = data[1];
				var track = data[2];

				var branch = new GitBranch(this)
				{
					Local = local,
					Remote = remote,
					Ahead = GetValueFromTrack(track, "ahead "),
					Behind = GetValueFromTrack(track, "behind ")
				};

				if (currentBranch == local)
					active = branch;

				rv.Add(branch);
			}

			foreach (var remote in remotes)
			{
				if (rv.Any(x => x.Remote == remote))
					continue;

				var branch = new GitBranch(this)
				{
					Remote = remote
				};
				rv.Add(branch);
			}

			return rv;
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
			//TODO: Update all branches info.
		}
	}
}