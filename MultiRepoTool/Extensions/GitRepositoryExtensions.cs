using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiRepoTool.Git;
using MultiRepoTool.Utils;

namespace MultiRepoTool.Extensions
{
    public static class GitRepositoryExtensions
    {
        /// <param name="includeActive">True to include <see cref="GitRepository.ActiveBranch"/> into result without check for filters match.</param>
        public static IEnumerable<GitBranch> Search(this GitRepository repository, IReadOnlyList<string> filters, bool includeActive)
        {
            foreach (var branch in repository.Branches)
            {
                var name = branch.HasLocal()
                    ? branch.Local
                    : branch.RemoteBranch;

                if (branch.IsActive && includeActive)
                    yield return branch;

                if (filters.Any(x => name.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
                    yield return branch;
            }
        }

        /// <summary>
        /// Looks for branches that matches <paramref name="filters"/>.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="filters">Filter</param>
        /// <param name="includeActive">True to include <see cref="GitRepository.ActiveBranch"/> into result without check for filters match.</param>
        /// <returns></returns>
        public static IEnumerable<GitBranch> Search(this GitRepository repository, IReadOnlyList<SearchFilter> filters, bool includeActive)
        {
            foreach (var branch in repository.Branches)
            {
                var name = branch.HasLocal()
                    ? branch.Local
                    : branch.RemoteBranch;

                if (branch.IsActive && includeActive)
                    yield return branch;

                if (filters.Any(x => x.Matched(name)))
                    yield return branch;
            }
        }

        /// <param name="includeActive">True to include <see cref="GitRepository.ActiveBranch"/> into result without check for filters match.</param>
        public static IEnumerable<GitBranch> Search(this GitRepository repository, string query, bool includeActive)
        {
            foreach (var branch in repository.Branches)
            {
                var name = branch.HasLocal() ? branch.Local : branch.RemoteBranch;
                if (branch.IsActive && includeActive)
                    yield return branch;

                if (name.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                    yield return branch;
            }
        }

        /// <param name="includeActive">True to include <see cref="GitRepository.ActiveBranch"/> into result without check for filters match.</param>
        public static Dictionary<GitRepository, IEnumerable<GitBranch>> Search(this IEnumerable<GitRepository> repositories, string query, bool includeActive)
        {
            var filters = (query ?? string.Empty)
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => SearchFilter.CreateFrom(x))
                .ToList();
            return repositories.ToDictionary(x => x, x => x.Search(filters, includeActive));
        }

        public static Task OpenInGitKraken(this GitRepository repository)
        {
            return SharedUtils.OpenInGitKraken(repository.Directory.FullName);
        }
    }
}