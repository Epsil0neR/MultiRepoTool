using CommandLine;

namespace MultiRepoTool;

public class Options
{
    [Option('p', "path", HelpText = "Path to directory with all repositories.")]
    public string Path { get; set; }

    [Option("auto-exit", HelpText = "Indicates if application will not wait for user input to be closed.")]
    public bool AutoExit { get; set; }

    [Option('f', "fetch", HelpText = "Perform fetch before other operations or not. Default: false")]
    public bool Fetch { get; set; }

    [Option('s', "search", HelpText = "Search for a branches with name.")]
    public string Search { get; set; }

    [Option('m', "menu", Default = true, HelpText = "Indicates if menu will be shown. (Default = true)")]
    public bool Menu { get; set; } = true;

    [Option("reload-before-status", Default = false, HelpText = "Indicates if repositories will be reloaded before any status action. (Default = false)")]
    public bool ReloadBeforeStatus { get; set; }

    [Option("delay-open-gk", Default = 0, HelpText = "Delay between opening repository in GitKraken. Max value is 10000.")]
    public uint DelayOpenInGitKraken { get; set; }

    [Option('u', "user-menus", Default = "Custom",
        HelpText = "Sub-directory name in executable directory where locates custom menu items as executable files. Supported formats: .cmd, .exe, .bat, .lnk (runs in default shell).")]
    public string UserMenuItemsFolder { get; set; }

    [Option('r', "profiles", Default = "Profiles", HelpText = "Sub-directory name in executable directory where profiles with whitelist and/or blacklist of repository folders.")]
    public string UserProfilesFolder { get; set; }

    [Option("profile", Default = null, HelpText = "Profile to use on start-up.")]
    public string Profile { get; set; }
}