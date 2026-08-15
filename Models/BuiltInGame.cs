namespace KidsGameLauncher.Models;

/// <summary>
/// Describes one of the games actually implemented inside this app (as
/// opposed to a game added by URL that loads in an iframe).
/// </summary>
/// <param name="MinAge">Suggested youngest age this game suits, in years.
/// A rough editorial judgment call for admin sorting/filtering, not a
/// hard gate - nothing stops a kid profile outside the range from
/// playing.</param>
/// <param name="MaxAge">Suggested oldest age this game still suits, in
/// years. Same caveat as <see cref="MinAge"/>.</param>
public record BuiltInGame(string Title, string ThumbnailEmoji, string LaunchTarget, string? ThumbnailImagePath = null, int? MinAge = null, int? MaxAge = null);
