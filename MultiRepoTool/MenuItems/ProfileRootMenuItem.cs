using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.ConsoleMenu;
using MultiRepoTool.Extensions;
using MultiRepoTool.Git;
using MultiRepoTool.Profiles;
using MultiRepoTool.Utils;

namespace MultiRepoTool.MenuItems;

public class ProfileRootMenuItem : MenuItem
{
    private Action? _subMenuExit;

    private ProfilesManager ProfilesManager { get; }
    public GitRepositoriesManager GitRepositoriesManager { get; }

    public ProfileRootMenuItem(ProfilesManager profilesManager, GitRepositoriesManager gitRepositoriesManager)
        : base("Profile")
    {
        ProfilesManager = profilesManager;
        GitRepositoriesManager = gitRepositoriesManager;
        HideExecutionText = true;
    }

    public override bool Execute(Menu menu)
    {
        List<ColoredTextPart> GetCurrentProfileTitle()
        {
            return new()
            {
                new("Current: "),
                new(ProfilesManager.Current.Name, ConsoleColor.Yellow)
            };
        }

        var miCurrent = new MenuItem(
            GetCurrentProfileTitle(),
            () => false)
        {
            CanExecute = false
        };

        var menus = new List<MenuItem>
        {
            miCurrent,
            new("Change profile...", SubMenuChangeProfile),
            new SeparatorMenuItem(),
            new("Edit list of repositories...", SubMenuRepositories),
            new("Edit menu...", SubMenuEditMenu),
            new SeparatorMenuItem(),
            new("Back", () => false)
        };

        var submenu = new Menu(menus)
        {
            PreventNewLineOnExecution = true,
            LoopNavigation = true
        };

        _subMenuExit = () => { miCurrent.ColoredTitle = GetCurrentProfileTitle(); };

        submenu.Run();
        return true;
    }

    private bool SubMenuChangeProfile()
    {
        MenuItem Convert(Profile profile)
        {
            var title = new List<ColoredTextPart>
            {
                new(profile.Name)
            };
            if (ReferenceEquals(ProfilesManager.Current, profile))
                title.Add(new(" (Active)", ConsoleColor.Yellow));

            return new(title, () =>
            {
                ProfilesManager.Current = profile;
                _subMenuExit?.Invoke();
                return false;
            });
        }

        var d = ProfilesManager.Default;
        var menus = new List<MenuItem>()
        {
            new("Create new profile...", CreateProfileHandler),
            new SeparatorMenuItem(),
            Convert(d),
        };
        foreach (var profile in ProfilesManager)
        {
            if (ReferenceEquals(d, profile))
                continue;
            menus.Add(Convert(profile));
        }

        if (ProfilesManager.Count > 1)
            menus.Add(new SeparatorMenuItem());

        menus.Add(new("Back", () => false));

        var menu = new Menu(menus)
        {
            LoopNavigation = true,
            PreventNewLineOnExecution = true,
        };
        menu.Run();
        return true;
    }

    private bool SubMenuRepositories()
    {
        var p = ProfilesManager.Current;
        var mode = p.RepositoriesMode;
        var repos = p.Repositories.ToList();
        var reposMenuItems = new List<CheckableMenuItem<GitRepository>>();

        List<ColoredTextPart> GetModeTitle()
        {
            return new()
            {
                new("Mode: "),
                new(mode.ToString(), ConsoleColor.Yellow)
            };
        }

        bool Save()
        {
            p.RepositoriesMode = mode;
            p.Repositories = reposMenuItems
                .Where(x => x.IsChecked)
                .Select(x => x.Value.Name)
                .ToArray();
            p.ApplyChangesAndSave();
            return false;
        }

        bool DiscardChanges()
        {
            return false;
        }

        CheckableMenuItem<GitRepository> ToMenuItem(GitRepository repository)
        {
            return new(repository.Name, repository)
            {
                IsChecked = repos.Contains(repository.Name, StringComparer.InvariantCultureIgnoreCase)
            };
        }

        MenuItem miMode = null;
        miMode = new(GetModeTitle(), () =>
        {
            mode = mode == ListMode.Black
                ? ListMode.White
                : ListMode.Black;

            miMode.ColoredTitle = GetModeTitle();
            return true;
        })
        {
            HideExecutionText = true
        };

        var menus = new List<MenuItem>
        {
            miMode,
            new SeparatorMenuItem(),
        };

        reposMenuItems.ReplaceAll(GitRepositoriesManager.AllRepositories.Select(ToMenuItem));
        menus.AddRange(reposMenuItems);

        if (GitRepositoriesManager.AllRepositories.Count > 0)
            menus.Add(new SeparatorMenuItem());

        menus.Add(new("Save and back", Save));
        menus.Add(new("Discard changes and back", DiscardChanges));

        var menu = new Menu(menus)
        {
            LoopNavigation = true,
            PreventNewLineOnExecution = true
        };
        menu.Run();

        return true;
    }

    private bool SubMenuEditMenu()
    {
        var p = ProfilesManager.Current;
        var rootMenuItems = IoC.Resolve<List<MenuItem>>("RootMenuItems");
        var toHide = p.MenuItemsToHide;

        var menus = new List<MenuItem>();
        var menusToHide = new List<CheckableMenuItem<MenuItem>>();
        foreach (var item in rootMenuItems)
        {
            var mi = new CheckableMenuItem<MenuItem>(item.Title, item)
            {
                IsChecked = toHide.Contains(item.Title, StringComparer.InvariantCultureIgnoreCase)
            };
            menusToHide.Add(mi);
            menus.Add(mi);
        }

        bool Save()
        {
            foreach (var item in menusToHide)
            {
                item.Value.IsHidden = item.IsChecked;
            }

            p.MenuItemsToHide = menusToHide
                .Where(x => x.IsChecked)
                .Select(x => x.Value.Title)
                .ToArray();
            p.ApplyChangesAndSave();

            return false;
        }

        menus.Add(new("Save and back", Save));
        menus.Add(new("Discard changes and back", () => false));

        var menu = new Menu(menus)
        {
            LoopNavigation = true,
            PreventNewLineOnExecution = true
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

        var duplicate = ProfilesManager.Any(x => string.Equals(name, x.Name, StringComparison.InvariantCultureIgnoreCase));
        if (duplicate)
        {
            ConsoleUtils.WriteLine("Profile with same name already exists.");
            return true;
        }

        try
        {
            var profile = ProfilesManager.Create(name);
            ProfilesManager.Current = profile;
        }
        catch (Exception e)
        {
            Console.Write("Could not create profile with specified name. Error: ");
            Console.WriteLine(e.Message);
            throw;
        }

        return false;
    }
}