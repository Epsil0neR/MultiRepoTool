using CommandLine;

namespace MultiRepoTool
{
	public class Options
	{
		[Option('p', "path", HelpText = "Path to directory with all repositories.")]
		public string Path { get; set; }

		[Option('s', "search", HelpText = "Search for a branches with name.")]
		public string SearchBranch { get; set; }
	}
}