using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Git;

namespace MultiRepoTool.Profiles
{
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
        private readonly IEnumerable<GitRepository> _repositories;
        private readonly ProfilesManager _manager;

        public Profile(ProfilesManager manager, IEnumerable<GitRepository> allRepositories)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _repositories = allRepositories;
        }

        public string Name { get; init; }

        /// <summary>
        /// Indicates if repositories specified in <see cref="Repositories"/> should be threaten as black list or as white list.
        /// Default: black list. 
        /// </summary>
        public ListMode RepositoriesMode { get; init; } = ListMode.Black;
        
        public string[] Repositories { get; init; }

        internal void Activate()
        {
            var result = _repositories.ToList();
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
        }
    }
}
