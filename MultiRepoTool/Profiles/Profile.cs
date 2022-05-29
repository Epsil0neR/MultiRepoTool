using System.Collections.Generic;
using System.Linq;
using MultiRepoTool.Git;

namespace MultiRepoTool.Profiles
{
    internal class Profile
    {
        private readonly IEnumerable<GitRepository> _repositories;

        public Profile(IEnumerable<GitRepository> allRepositories)
        {
            _repositories = allRepositories;
        }

        public string Name { get; init; }
        public string[] Blacklist { get; init; }
        public string[] Whitelist { get; init; }

        public void Activate()
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
