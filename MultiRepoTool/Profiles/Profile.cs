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

internal class Profile
{
    private readonly ProfilesManager _manager;
    private readonly GitRepositoriesManager _repositoriesManager;

    public Profile(ProfilesManager manager, GitRepositoriesManager repositoriesManager)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _repositoriesManager = repositoriesManager;
    }

    public string Name { get; init; }

    /// <summary>
    /// Indicates if repositories specified in <see cref="Repositories"/> should be threaten as black list or as white list.
    /// Default: black list. 
    /// </summary>
    public ListMode RepositoriesMode { get; init; } = ListMode.Black;

    public string[] Repositories { get; init; } = Array.Empty<string>();
        
    public string[] MenuItemsToHide { get; init; } = Array.Empty<string>();

    internal void Activate()
    {
        var result = _repositoriesManager.AllRepositories.ToList();
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
        _repositoriesManager.Repositories = result;
    }
}