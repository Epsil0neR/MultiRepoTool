namespace MultiRepoTool.Git
{
	public class GitBranch
	{
		public GitRepository Repository { get; }
		public string Local { get; init; }
		public string Remote { get; init; }

		public uint Behind { get; set; }
		public uint Ahead { get; set; }

		public GitBranch(GitRepository repository)
		{
			Repository = repository;
		}
	}
}