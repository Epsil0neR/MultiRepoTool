namespace MultiRepoTool.Git
{
	public class GitBranch
	{
		/// <summary>
		/// Owner repository.
		/// </summary>
		public GitRepository Repository { get; }

		/// <summary>
		/// Indicates if branch is currently active.
		/// </summary>
		public bool IsActive { get; internal set; }

		/// <summary>
		/// Local branch name.
		/// </summary>
		public string Local { get; init; }

		/// <summary>
		/// Branch name from remote.
		/// </summary>
		public string Remote { get; init; }

		/// <summary>
		/// Number of commits on the base branch that do not exist on this branch.
		/// </summary>
		public uint Behind { get; set; }

		/// <summary>
		/// Number of commits on this branch that do not exist on the base branch.
		/// </summary>
		public uint Ahead { get; set; }

		/// <summary>
		/// Result of 'git status -sb'.
		/// </summary>
		public string Status { get; set; }

		public GitBranch(GitRepository repository)
		{
			Repository = repository;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"L:{Local} - A:{Ahead}-B:{Behind} - R:{Remote}";
		}
	}
}