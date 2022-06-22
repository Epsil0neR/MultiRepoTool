using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Git;

namespace MultiRepoTool.Profiles;

public class Profile
{
    public Profile(ProfilesManager manager)
    {
        Manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public ProfilesManager Manager { get; }

    /// <summary>
    /// Profile name. It is also a file name without extension to save profile on disk.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Indicates if repositories specified in <see cref="Repositories"/> should be threaten as black list or as white list.
    /// Default: black list. 
    /// </summary>
    public ListMode RepositoriesMode { get; set; } = ListMode.Black;

    /// <summary>
    /// Names of repositories that will be blacklisted/whitelisted.
    /// </summary>
    public string[] Repositories { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Root menu items titles to hide.  
    /// </summary>
    public string[] MenuItemsToHide { get; set; } = Array.Empty<string>();

    internal void Activate()
    {
        ActivateRepositories();
        ActivateRootMenu();
    }

    private void ActivateRepositories()
    {
        var result = Manager.RepositoriesManager.AllRepositories.ToList();
        switch (RepositoriesMode)
        {
            case ListMode.White:
                result.RemoveAll(x => !Repositories.Contains(x.Name));
                break;
            case ListMode.Black:
            default:
                var toRemove = result.Where(x => Repositories.Contains(x.Name)).ToList();
                if (toRemove.Count > 0)
                    result.RemoveAll(toRemove.Contains);
                break;
        }

        IoC.RegisterInstance<IEnumerable<GitRepository>>(result);
        IoC.RegisterInstance<IReadOnlyList<GitRepository>>(result);
        Manager.RepositoriesManager.Repositories = result;
    }

    private void ActivateRootMenu()
    {
        var menuItemsToHide = MenuItemsToHide;
        foreach (var item in Manager.RootMenuItems)
            item.IsHidden = menuItemsToHide?.Contains(item.Title, StringComparer.InvariantCultureIgnoreCase) ?? false;
    }

    /// <summary>
    /// Saves profile and re-actives this profile.
    /// </summary>
    public void ApplyChangesAndSave()
    {
        if (!ReferenceEquals(Manager.Current, this))
            return;

        Manager.Save(this);
        Activate();
    }
}