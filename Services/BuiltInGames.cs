using KidsGameLauncher.Models;

namespace KidsGameLauncher.Services;

/// <summary>
/// Single source of truth for the games built into this app. GameHost
/// switches on a game's LaunchTarget to decide which component to render,
/// so the LaunchTarget keys here must stay in sync with GameHost.razor.
/// The admin "add game" picker lists these instead of asking a parent to
/// type an internal route key by hand.
/// </summary>
public static class BuiltInGames
{
    public const string MemoryMatch = "memory-match";
    public const string Fishing = "fishing-catch";
    public const string DressUp = "dress-up";
    public const string MannersGarden = "manners-garden";

    public static readonly IReadOnlyList<BuiltInGame> All = new List<BuiltInGame>
    {
        new BuiltInGame("Memory Match", "🧠", MemoryMatch),
        new BuiltInGame("Fishing Catch", "🎣", Fishing),
        new BuiltInGame("Dress Up", "👗", DressUp),
        new BuiltInGame("Manners Garden", "🌷", MannersGarden),
    };
}
