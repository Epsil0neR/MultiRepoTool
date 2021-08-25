﻿using CommandLine;

namespace MultiRepoTool
{
	public class Options
	{
		[Option('p', "path", HelpText = "Path to directory with all repositories.")]
		public string Path { get; set; }

		[Option("gk", HelpText = "Open all repositories in GitKraken. -s --search can filter repositories by name.")]
		public bool OpenInGitKraken { get; set; }

		[Option('s', "search", HelpText = "Search for a branches with name.")]
		public string Search { get; set; }


		[Option("gk-delay", Required = false, HelpText = "Delay while opening repositories in GitKraken.")]
		public int OpenInGitKrakenDelay { get; set; }
	}
}