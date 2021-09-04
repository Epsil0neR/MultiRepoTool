using System;
using System.Diagnostics;
using System.IO;

namespace MultiRepoTool
{
	public class CommandExecutor
	{
		public DirectoryInfo WorkingDirectory { get; }

		public CommandExecutor(DirectoryInfo workingDirectory)
		{
			WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
		}

		public string Execute(string commandName, string command)
		{
			var cmdPath = @"C:\Windows\System32\cmd.exe";
			var arguments = $@"/C {command}";
			var dir = WorkingDirectory.FullName;

			var proc = new Process // https://stackoverflow.com/a/22869734/1763586
			{
				StartInfo =
				{
					FileName = cmdPath,
					Arguments = arguments,
					WorkingDirectory = dir,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					WindowStyle = ProcessWindowStyle.Hidden,
				}
			};

			proc.Start();
			var output = proc.StandardOutput.ReadToEnd();
			proc.WaitForExit();

			return output;
		}

	}
}