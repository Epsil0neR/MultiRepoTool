using System;
using MultiRepoTool.Git;

namespace MultiRepoTool.Profiles;

public static class Extensions
{
    public static ProfileDto ToDto(this Profile profile)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        return new ProfileDto
        {
            RepositoriesMode = profile.RepositoriesMode,
            Repositories = profile.Repositories,
            HideMenuItems = profile.MenuItemsToHide
        };
    }

    public static Profile FromDto(
        this ProfileDto profileDto,
        string name,
        ProfilesManager profilesManager,
        GitRepositoriesManager repositoriesManager)
    {
        if (profileDto is null)
            throw new ArgumentNullException(nameof(profileDto));

        var rv = new Profile(profilesManager)
        {
            Name = name,
            RepositoriesMode = profileDto.RepositoriesMode,
            Repositories = profileDto.Repositories ?? Array.Empty<string>(),
            MenuItemsToHide = profileDto.HideMenuItems ?? Array.Empty<string>()
        };

        return rv;
    }
}