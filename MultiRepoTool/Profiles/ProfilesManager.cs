using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using MultiRepoTool.Git;

namespace MultiRepoTool.Profiles;

internal class ProfilesManager : List<Profile>
{
    public Options Options { get; }

    public ProfilesManager(Options options, List<GitRepository> allRepositories)
    {
        Options = options;

        // Add default profile without black/white lists.
        Add(new Profile(allRepositories));

        var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directory = Path.Combine(rootDirectory, options.UserMenuItemsFolder);

        if (!Directory.Exists(directory))
            return;

        var files = Directory.GetFiles(directory, "*.profile");
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            var json = File.ReadAllText(info.FullName, Encoding.UTF8);
            var data = JsonSerializer.Deserialize<ProfileJson>(json);
            var profile = new Profile(allRepositories)
            {
                Name = Path.GetFileNameWithoutExtension(info.Name),
                Blacklist = data?.Blacklist,
                Whitelist = data?.Whitelist,
            };
        }
    }

    public void Activate(string name = null)
    {
        foreach (var profile in this)
        {
            if (profile.Name != name)
                continue;

            profile.Activate();
            break;
        }
    }
}