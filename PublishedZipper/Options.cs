using System.Collections.Generic;
using CommandLine;

namespace PublishedZipper
{
	public class Options
	{
		[Option('p', "path", HelpText = "Path to directory with source files.")]
		public string Path { get; set; }

        [Option('o', "output", HelpText = "Path to directory where zip will be placed.")]
        public string OutputPath { get; set; }

        [Option('e', "exe", HelpText = "Filename to main executable that contains version.")]
        public string ExeName { get; set; }

        [Option("exclude", HelpText = "Extensions to exclude.", Separator = ',')]
        public IEnumerable<string> ExtensionsToExclude { get; set; }
	}
}