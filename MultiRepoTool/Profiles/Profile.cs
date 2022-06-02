using System;
using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Git;

namespace MultiRepoTool.Profiles
{
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
        public string[] Blacklist { get; init; }
        public string[] Whitelist { get; init; }

        internal void Activate()
        {
            var result = _repositories.ToList();
            if (Blacklist is { Length: > 0 })
            {
                var toRemove = result.Where(x => Blacklist.Contains(x.Name)).ToList();
                if (toRemove.Count > 0)
                    result.RemoveAll(toRemove.Contains);
            }

            if (Whitelist is { Length: > 0 })
            {
                result.RemoveAll(x => !Whitelist.Contains(x.Name));
            }

            IoC.RegisterInstance<IEnumerable<GitRepository>>(result);
            IoC.RegisterInstance<IReadOnlyList<GitRepository>>(result);
        }
    }
}
