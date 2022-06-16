namespace MultiRepoTool.Git;

public static class GitConst
{
    public const string GitSubFolder = ".git";
    public const string CommandPull = "git pull";
    public const string CommandFetch = "git fetch --all --quiet";
    public const string CommandPush = "git push";
    public const string CommandBranchStatus = "git status -sb";
    public const string CommandListBranchesLocal = "git branch -l";
    public const string CommandListBranchesRemote = "git branch -r";
    public const string CommandCurrentBranch = "git branch --show-current";
    public const string CommandListRemoteNames = "git remote";
    public const string CommandDiff = "git log --pretty=%B {0}...{1}";
}