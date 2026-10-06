namespace KidsGameLauncher.Models;

/// <summary>
/// A reported result or unfinished game visit, recorded for a profile/game pair so a
/// parent can later see progress (moves, time) over time.
/// </summary>
public class PlayHistoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public PlaySessionStatus Status { get; set; }
    public string ProfileId { get; set; } = "";
    public string GameId { get; set; } = "";
    public string Theme { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int Moves { get; set; }
    public int ElapsedSeconds { get; set; }
    public DateTime PlayedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum PlaySessionStatus
{
    ResultRecorded,
    InProgress,
    Abandoned,
    Ended
}
