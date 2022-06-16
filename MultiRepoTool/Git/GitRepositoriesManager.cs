using System.Collections.Generic;

namespace MultiRepoTool.Git;

public class GitRepositoriesManager
{
    public GitRepositoriesManager(IReadOnlyList<GitRepository> allRepositories)
    {
        AllRepositories = allRepositories;
        Repositories = allRepositories;
    }

    /// <summary>
    /// All currently known repositories.
    /// </summary>
    public IReadOnlyList<GitRepository> AllRepositories { get; }

    /// <summary>
    /// Filtered repositories that should be used in most cases instead of <see cref="AllRepositories"/>. 
    /// </summary>
    public IReadOnlyList<GitRepository> Repositories { get; set; }
}