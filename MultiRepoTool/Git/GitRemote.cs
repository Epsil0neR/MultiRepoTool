using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiRepoTool.Git
{
	public class GitRemote
	{
		private static string CommandFetch(string name) => $"git remote get-url {name}";
		private static string CommandPush(string name) => $"git remote get-url {name} --push";

		public GitRepository Repository { get; }
		public string Name { get; init; }
		public IReadOnlyList<string> UrlsPush { get; init; }
		public IReadOnlyList<string> UrlsFetch { get; init; }

		private GitRemote(GitRepository repository)
		{
			Repository = repository;
		}

		public static GitRemote FromName(string name, GitRepository repository)
		{
			if (string.IsNullOrEmpty(name))
				return null;
			if (repository == null)
				return null;

			var urlsPush = repository.Executor.Execute("List push urls", CommandPush(name));
			var urlsFetch = repository.Executor.Execute("List fetch urls", CommandPush(name));

			IReadOnlyList<string> Parse(string output) =>
				output
					.Split("\n")
					.Select(x => x.Trim(' ', '\r'))
					.Where(x => !string.IsNullOrEmpty(x))
					.ToList();


			var rv = new GitRemote(repository)
			{
				Name = name,
				UrlsFetch = Parse(urlsFetch),
				UrlsPush = Parse(urlsPush)
			};
			return rv;
		}
	}
}