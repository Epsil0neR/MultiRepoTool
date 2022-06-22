namespace MultiRepoTool.Profiles;

public class ProfileDto
{
    public ListMode RepositoriesMode { get; init; }
    public string[]? Repositories { get; init; }
    
    public string[]? HideMenuItems { get; init; }
}