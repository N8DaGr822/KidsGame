namespace KidsGameLauncher.Models;

/// <summary>
/// Describes one of the games actually implemented inside this app (as
/// opposed to a game added by URL that loads in an iframe).
/// </summary>
public record BuiltInGame(string Title, string ThumbnailEmoji, string LaunchTarget, string? ThumbnailImagePath = null);
