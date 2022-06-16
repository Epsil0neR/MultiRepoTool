using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;
using MultiRepoTool.MenuItems;
using MultiRepoTool.Utils;

namespace MultiRepoTool.Profiles;

//TODO: 1. Save profile to file once it created.
//TODO: 2. Root menu item should be "Profile". It opens menu with: 1. Select profile. Active: <Profile.Name>. 2+. profile configuration. Last: Go back.
//TODO: 3. Menu item to save current profile. Menu item: "Go Back" / "Save". Maybe add menu item "Discard changes"
//TODO: [Done] 4. List mode toggle - Black list / White list.
//TODO: 5. Select repos for list. List should be similar to "Open in GitKraken" menu.
//TODO: 6. Select menu items to hide. Like "Pull", "Search", "Status short", etc items from root. NOTE: Some menu items should be always visible. Like "Profile", "Exit"
//TODO: 7. ??? Add support for different repositories location??? - Just idea for now, check all pros and cons.

internal class ProfilesManager : List<Profile>
{
    private readonly GitRepositoriesManager _repositoriesManager;
    internal const string DefaultProfileName = "Default";
    
    private Profile _current;
    
    public Options Options { get; }

    public Profile Default { get; }

    public Profile Current
    {
        get => _current;
        set
        {
            if (ReferenceEquals(_current, value))
                return;

            _current = value ?? throw new ArgumentNullException(nameof(value));
            value.Activate();
            IoC.RegisterInstance(value);
        }
    }

    public ProfilesManager(Options options, GitRepositoriesManager repositoriesManager)
    {
        Options = options;
        _repositoriesManager = repositoriesManager;
        

        var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directory = Path.Combine(rootDirectory, options.UserProfilesFolder);

        if (Directory.Exists(directory))
        {
            var files = Directory.GetFiles(directory, "*.profile");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var json = File.ReadAllText(info.FullName, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<ProfileDto>(json);
                var profile = data?.FromDto(Path.GetFileNameWithoutExtension(info.Name), this, repositoriesManager);
                
                if (profile is null)
                    continue;
                
                Add(profile);

                if (DefaultProfileName.Equals(profile.Name, StringComparison.InvariantCultureIgnoreCase))
                    Default = profile;
            }
        }

        if (Default is null)
        {
            Default = new Profile(this, repositoriesManager)
            {
                Name = DefaultProfileName
            };
            Insert(0, Default);
            _ = Save(Default);
        }

        if (!Activate(options.Profile))
            Current = Default;
    }

    public async Task Save(Profile profile, CancellationToken token = default)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        var name = profile.Name;
        if (string.IsNullOrWhiteSpace(name))
            name = DefaultProfileName;

        var dto = profile.ToDto();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(dto, options);
        var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directory = Path.Combine(rootDirectory, Options.UserProfilesFolder);
        var path = Path.Combine(directory, $"{name}.profile");

        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(path, json, Encoding.UTF8, token);
    }


    /// <summary>
    /// Activates profile with specified name.
    /// </summary>
    /// <param name="name">Profile name.</param>
    /// <returns>True if <see cref="Profile"/> got activated, otherwise false.</returns>
    public bool Activate(string? name = null)
    {
        foreach (var profile in this)
        {
            if (profile.Name != name)
                continue;

            Current = profile;
            return true;
        }

        return false;
    }

    public MenuItem CreateMenuItem()
    {
        return new MenuItem("Profiles", RootMenuHandler);
    }

    private bool RootMenuHandler()
    {
        var menus = new List<MenuItem>()
        {
            ToMenuItem(Default),
        };

        if (Count > 1)
        {
            menus.Add(new SeparatorMenuItem());

            foreach (var profile in this)
            {
                if (ReferenceEquals(profile, Default))
                    continue;

                menus.Add(ToMenuItem(profile));
            }
        }

        menus.Add(new SeparatorMenuItem());
        menus.Add(new MenuItem("Create new profile...", CreateProfileHandler));
        menus.Add(new MenuItem("Cancel", () => false));

        var menu = new Menu(menus)
        {
            PreventNewLineOnExecution = true,
            LoopNavigation = true
        };

        menu.Run();
        return true;
    }

    private bool CreateProfileHandler()
    {
        ConsoleUtils.Write("Profile name: ");
        var name = ConsoleUtils.ReadLine(ConsoleColor.Red);

        if (string.IsNullOrEmpty(name))
        {
            ConsoleUtils.WriteLine("Profile name cannot be empty.");
            return true;
        }
        
        var duplicate = this.Any(x => string.Equals(name, x.Name, StringComparison.InvariantCultureIgnoreCase));
        if (duplicate)
        {
            ConsoleUtils.WriteLine("Profile with same name already exists.");
            return true;
        }

        var rv = new Profile(this, _repositoriesManager)
        {
            Name = name
        };
        Add(rv);
        Current = rv;
        Save(rv);
        return false;
    }

    private MenuItem ToMenuItem(Profile profile)
    {
        var title = new List<ColoredTextPart>
        {
            new(profile.Name)
        };
        if (ReferenceEquals(profile, Current))
            title.Add(new(" (active)", Constants.ColorRepository));

        return new(title, () =>
        {
            Current = profile;
            return false;
        });
    }
}