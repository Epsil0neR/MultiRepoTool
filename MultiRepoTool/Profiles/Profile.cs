using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Git;

namespace MultiRepoTool.Profiles;

/// <summary>
/// Indicates how list will be threaten - as <see cref="White"/> or as <see cref="Black"/>.
/// </summary>
public enum ListMode {
    /// <summary>
    /// Items selected by user will not be available in other places.
    /// </summary>
    Black,
        
    /// <summary>
    /// Only selected by user items will be available in other places.
    /// </summary>
    White    
}

public class Profile
{
    public Profile(ProfilesManager manager)
    {
        Manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }
    
    public ProfilesManager Manager { get; }

    public string Name { get; init; }

    /// <summary>
    /// Indicates if repositories specified in <see cref="Repositories"/> should be threaten as black list or as white list.
    /// Default: black list. 
    /// </summary>
    public ListMode RepositoriesMode { get; set; } = ListMode.Black;

    public string[] Repositories { get; set; } = Array.Empty<string>();
        
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

    public void ApplyChangesAndSave()
    {
        Manager.Save(this);
        Activate();
    }
}