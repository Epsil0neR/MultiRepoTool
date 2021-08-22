using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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

			if (false)
			{
				var privateKeyPath = @"C:\Users\epsil\.ssh\id_rsa";
				var argsBuilder = new StringBuilder();
				argsBuilder.Append($"cd \"{dir}\" &&");
				argsBuilder.Append($"SET GIT_SSH_COMMAND=ssh -o StrictHostKeyChecking=no -i \\\"{privateKeyPath}\\\" && ");
				argsBuilder.Append(command);
				argsBuilder.Append(" && exit");
				cmdPath = @"C:\Program Files\Git\git-cmd.exe";
				arguments = argsBuilder.ToString();
			}

			Process proc = new Process // https://stackoverflow.com/a/22869734/1763586
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