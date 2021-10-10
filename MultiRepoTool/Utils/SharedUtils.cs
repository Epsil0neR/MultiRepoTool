using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MultiRepoTool.Utils
{
    public static class SharedUtils
    {
        public static bool IsGitKrakenInstalled()
        {
            string exePath = GetGitKrakenPath();
            return File.Exists(exePath);
        }

        public static string GetGitKrakenPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var exePath = $"{localAppData}/gitkraken/update.exe";
            return exePath;
        }

        public static Task OpenInGitKraken(string path)
        {
            var exePath = SharedUtils.GetGitKrakenPath();
            var param = "--processStart=gitkraken.exe --process-start-args=\"-p \\\"{0}\\\"\"";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = exePath,
                    Arguments = string.Format(param, path),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            process.Start();
            return process.WaitForExitAsync();
        }
    }


}