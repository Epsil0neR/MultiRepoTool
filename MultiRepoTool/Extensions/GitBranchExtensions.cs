using System;
using System.Linq;
using System.Text;
using MultiRepoTool.Git;

namespace MultiRepoTool.Extensions;

public static class GitBranchExtensions
{
    /// <summary>
    /// Checks if <paramref name="branch"/> exists locally.
    /// </summary>
    /// <param name="branch"></param>
    /// <returns></returns>
    public static bool HasLocal(this GitBranch branch)
    {
        if (branch == null)
            throw new ArgumentNullException(nameof(branch));

        return !string.IsNullOrEmpty(branch.Local);
    }

    /// <summary>
    /// Checks if <paramref name="branch"/> exists in remote.
    /// </summary>
    /// <param name="branch"></param>
    /// <returns></returns>
    public static bool HasRemoteBranch(this GitBranch branch)
    {
        if (branch == null)
            throw new ArgumentNullException(nameof(branch));

        return !string.IsNullOrEmpty(branch.RemoteBranch);
    }

    /// <summary>
    /// Checks if branch contains only remote information.
    /// </summary>
    /// <param name="branch"></param>
    /// <returns></returns>
    public static bool IsRemoteOnly(this GitBranch branch)
    {
        return !HasLocal(branch) && HasRemoteBranch(branch);
    }

    /// <summary>
    /// Checks if branch contains only local information.
    /// </summary>
    /// <param name="branch"></param>
    /// <returns></returns>
    public static bool IsLocalOnly(this GitBranch branch)
    {
        return HasLocal(branch) && !HasRemoteBranch(branch);
    }

    public static string GetNameWithTrackingInfo(this GitBranch branch)
    {
        if (branch == null)
            return string.Empty;

        var name = branch.HasLocal() 
            ? branch.Local 
            : branch.RemoteBranch;

        if (branch.Behind == 0 && branch.Ahead == 0)
        {
            if (branch.Repository.Remotes.Count() >= 2 && !string.IsNullOrWhiteSpace(branch.Remote))
                return $"{name} ({branch.Remote})";
            return name;
        }

        var sb = new StringBuilder();
        sb.Append(name);
        if (branch.Ahead > 0) 
            sb.Append($"[A:{branch.Ahead}]");
        if (branch.Behind > 0) 
            sb.Append($"[B:{branch.Behind}]");
        if (branch.Repository.Remotes.Count() >= 2 && !string.IsNullOrWhiteSpace(branch.Remote))
            sb.Append($" ({branch.Remote})");
        return sb.ToString();
    }
}