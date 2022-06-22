using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Git;

namespace MultiRepoTool.Profiles;

public class ProfilesManager : List<Profile>
{
    private const string DefaultProfileName = "Default";

    private readonly Lazy<JsonSerializerOptions> _jsonOptions = new(() => new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    private JsonSerializerOptions JsonSerializerOptions => _jsonOptions.Value;

    private Profile _current;

    public Options Options { get; }
    public GitRepositoriesManager RepositoriesManager { get; }
    public List<MenuItem> RootMenuItems { get; }

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

    public ProfilesManager(
        Options options,
        GitRepositoriesManager repositoriesManager,
        List<MenuItem> rootMenuItems)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        RepositoriesManager = repositoriesManager ?? throw new ArgumentNullException(nameof(repositoriesManager));
        RootMenuItems = rootMenuItems ?? throw new ArgumentNullException(nameof(rootMenuItems));

        var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directory = Path.Combine(rootDirectory, options.UserProfilesFolder);

        if (Directory.Exists(directory))
        {
            var files = Directory.GetFiles(directory, "*.profile");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var json = File.ReadAllText(info.FullName, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<ProfileDto>(json, JsonSerializerOptions);
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
            Default = new(this)
            {
                Name = DefaultProfileName
            };
            Insert(0, Default);
            Save(Default);
        }

        if (!Activate(options.Profile))
            Current = Default;
    }

    public void Save(Profile profile)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        var name = profile.Name;
        if (string.IsNullOrWhiteSpace(name))
            name = DefaultProfileName;

        var dto = profile.ToDto();
        var json = JsonSerializer.Serialize(dto, JsonSerializerOptions);
        var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directory = Path.Combine(rootDirectory, Options.UserProfilesFolder);
        var path = Path.Combine(directory, $"{name}.profile");

        Directory.CreateDirectory(directory);
        File.WriteAllText(path, json, Encoding.UTF8);
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

    public Profile Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        // Look for profile with same name.
        if (this.Any(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)))
            throw new ArgumentException("Profile with same name already exists.", nameof(name));

        var rv = new Profile(this)
        {
            Name = name
        };
        Add(rv);
        Save(rv);
        return rv;
    }
}

public static class ProfileExtensions
{
    public static MenuItem ToMenuItem(this Profile profile)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        var title = new List<ColoredTextPart>
        {
            new(profile.Name)
        };
        if (ReferenceEquals(profile, profile.Manager.Current))
            title.Add(new(" (active)", Constants.ColorRepository));

        return new(title, () =>
        {
            profile.Manager.Current = profile;
            return false;
        });
    }
}