namespace MultiRepoTool.Profiles;

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