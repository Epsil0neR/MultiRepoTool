using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MultiRepoTool.Utils;

public static class SharedUtils
{
    private static readonly Lazy<string> GitKrakenPath = new(GitKrakenPathFactory, LazyThreadSafetyMode.ExecutionAndPublication);

    private static string GitKrakenPathFactory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var exePath = $"{localAppData}/gitkraken/update.exe";
        return exePath;
    }

    public static bool IsGitKrakenInstalled()
    {
        var exePath = GetGitKrakenPath();
        return File.Exists(exePath);
    }

    public static string GetGitKrakenPath()
    {
        return GitKrakenPath.Value;
    }

    public static Task OpenInGitKraken(string path)
    {
        var exePath = GetGitKrakenPath();
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

    public static Version GetVersion()
    {
        var path = GetExecutablePath();
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var info = FileVersionInfo.GetVersionInfo(path);
        if (string.IsNullOrWhiteSpace(info.ProductVersion))
            return null;

        return Version.Parse(info.ProductVersion);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    static extern uint GetModuleFileName(IntPtr hModule, System.Text.StringBuilder lpFilename, int nSize);
    static readonly int MAX_PATH = 255;
    public static string GetExecutablePath()
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            var sb = new System.Text.StringBuilder(MAX_PATH);
            GetModuleFileName(IntPtr.Zero, sb, MAX_PATH);
            return sb.ToString();
        }
        else
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }
    }
}