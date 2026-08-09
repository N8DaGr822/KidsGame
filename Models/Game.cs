namespace KidsGameLauncher.Models;

public class Game
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";

    // Placeholder art until real thumbnails/art assets are added.
    public string ThumbnailEmoji { get; set; } = "🎮";

    // Where GameHost navigates to launch this game - a route within this
    // app (e.g. "/play/inland-hauler") for games built as routed pages,
    // or an absolute URL if a game is hosted elsewhere and loaded in an
    // iframe.
    public string LaunchTarget { get; set; } = "";
    public GameLaunchMode LaunchMode { get; set; } = GameLaunchMode.InternalRoute;

    // Admin can hide a game from the entire catalog (e.g. still being
    // built) independent of any one kid's access list.
    public bool IsCatalogEnabled { get; set; } = true;
}

public enum GameLaunchMode
{
    InternalRoute,
    ExternalIframe
}
