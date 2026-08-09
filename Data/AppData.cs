namespace KidsGameLauncher.Data;

using KidsGameLauncher.Models;

/// <summary>
/// Everything persisted to local storage lives in this single object,
/// stored under one key. Keeping it as one blob (rather than separate
/// keys per entity) avoids juggling multiple round trips and keeps
/// reads/writes atomic from the app's point of view.
/// </summary>
public class AppData
{
    public List<Profile> Profiles { get; set; } = new();
    public List<Game> Games { get; set; } = new();

    // profileId -> set of gameIds that profile is allowed to see/play.
    // Only meaningful for Kid profiles; Admin profiles always see everything.
    public Dictionary<string, List<string>> ProfileGameAccess { get; set; } = new();

    // Every completed play session, across all profiles and games.
    public List<PlayHistoryEntry> PlayHistory { get; set; } = new();
}
