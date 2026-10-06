namespace KidsGameLauncher.Models;

/// <summary>
/// A saved Mine for the One Piece expedition - a checkpoint taken each
/// time the pod docks at a surface station, so a long dig toward the
/// treasure can continue across sessions. Loading always puts the pod
/// back on the surface.
/// </summary>
public class MineSave
{
    /// <summary>Bumped if the tile encoding changes; older saves are ignored.</summary>
    public int Version { get; set; }
    public string Difficulty { get; set; } = "";

    /// <summary>Row-major world grid, one character per tile.</summary>
    public string Tiles { get; set; } = "";

    public int Gold { get; set; }
    public int DrillTier { get; set; } = 1;
    public int EngineTier { get; set; } = 1;
    public int FuelTankTier { get; set; } = 1;
    public int CargoTier { get; set; } = 1;
    public int HullTier { get; set; } = 1;
    public int RadiatorTier { get; set; } = 1;
    public double Fuel { get; set; }
    public double Hull { get; set; }
    public Dictionary<string, int> Cargo { get; set; } = new();
    public Dictionary<string, int> Items { get; set; } = new();
    public int DeepestRow { get; set; }
    public int NextMilestone { get; set; }
    public int ElapsedSeconds { get; set; }
    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Per-profile Mine for the One Piece preferences that outlive any single
/// expedition: owned/selected pod paint jobs and the touch control style.
/// </summary>
public class MineProfile
{
    public List<string> OwnedPaints { get; set; } = new();
    public string SelectedPaint { get; set; } = "classic";
    public bool UseButtons { get; set; }
}
