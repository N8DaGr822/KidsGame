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

    // profileId -> gameId -> a parent-locked difficulty name ("Easy",
    // "Medium", "Hard") for games that expose a difficulty picker. Absent
    // (or not present for a given gameId) means the kid picks their own -
    // this only ever narrows a kid's choice, never widens it, so it's
    // safe to leave unset by default.
    public Dictionary<string, Dictionary<string, string>> GameDifficultyOverrides { get; set; } = new();

    // profileId -> gameId -> metricKey -> best value achieved so far, e.g.
    // "time:Animals:Medium" -> 42 (seconds). Each game defines its own
    // metric keys and whether lower or higher counts as "better" - see
    // AppDataService.TryRecordBestAsync - this is just a flat value store,
    // direction isn't persisted since it's a property of the metric, not
    // the data.
    public Dictionary<string, Dictionary<string, Dictionary<string, double>>> GameBests { get; set; } = new();
}
