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

    // profileId -> "yyyy-MM-dd" (local date) -> total seconds the app was
    // open with that profile active. Ticked by PlayTimeTracker whenever a
    // Kid profile is current, independent of which screen or game is
    // showing - this is what daily time limits are enforced against, since
    // PlayHistory above only captures completed/exited game sessions.
    public Dictionary<string, Dictionary<string, int>> DailyUsageSeconds { get; set; } = new();

    // profileId -> every Manners Garden item earned, all-time (not reset
    // per session) - the garden is meant to keep growing across days.
    public Dictionary<string, List<string>> GardenItems { get; set; } = new();
}
