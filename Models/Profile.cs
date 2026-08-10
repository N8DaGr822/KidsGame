namespace KidsGameLauncher.Models;

public enum ProfileType
{
    Kid,
    Admin
}

public class Profile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";

    // Local image path under wwwroot, e.g. "images/profiles/buddy.png".
    // Emoji stays as a fallback so profiles can be migrated one at a time.
    public string? AvatarImagePath { get; set; }
    public string AvatarEmoji { get; set; } = "🙂";

    // Each profile gets a color identity used consistently across the app
    // (card ring, header accent, etc.) so kids recognize "their" screens
    // by color before they can reliably read names.
    public string ColorHex { get; set; } = "#4ECDC4";

    public ProfileType Type { get; set; } = ProfileType.Kid;

    // Only set for Admin profiles. Stored as a simple hash, not plaintext.
    // This is a light deterrent for a kid's tablet, not real security.
    public string? PinHash { get; set; }

    // Parent-set daily play cap in minutes, for Kid profiles. Null or 0
    // means no limit. Enforced by PlayTimeTracker against
    // AppData.DailyUsageSeconds, not against this field directly.
    public int? DailyTimeLimitMinutes { get; set; }
}
