namespace MultiRepoTool.Git
{
	public class GitBranch
	{
		public GitRepository Repository { get; }
		public bool IsActive { get; set; }

		public string Local { get; init; }
		public string Remote { get; init; }

		public uint Behind { get; set; }
		public uint Ahead { get; set; }

		/// <summary>
		/// Result of 'git status -sb'.
		/// </summary>
		public string Status { get; set; }

		public GitBranch(GitRepository repository)
		{
			Repository = repository;
		}

		public override string ToString()
		{
			return $"L:{Local} - A:{Ahead}-B:{Behind} - R:{Remote}";
		}
	}
}