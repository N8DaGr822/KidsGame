using System.Numerics;

namespace KidsGameLauncher.Services;

/// <summary>
/// Shared top-down racing track/car layer, extracted from
/// Components/RacingGame.razor once Components/TimeTrialRacer.razor
/// became a second consumer of the exact same track, car roster, and
/// movement physics (same "extract once a second consumer genuinely
/// needs it" rule this codebase follows elsewhere - see GameAi.cs/
/// Physics2D.cs). A single hand-laid 4x6 tile grid forming a rounded-
/// rectangle loop: one curve tile image reused at all four corners via
/// CSS rotation, plus dedicated horizontal/vertical straight tiles.
///
/// Coordinate space is the same "field units, not pixels or percent"
/// convention as Physics2D consumers (AirHockey/SpaceGame/RacingGame) -
/// callers render position as Position/FieldSize as a percentage of a
/// CSS-constrained field box.
/// </summary>
public static class RaceTrack
{
    public const float TileSize = 50f;
    public const int GridCols = 4;
    public const int GridRows = 6;
    public const float FieldWidth = TileSize * GridCols;
    public const float FieldHeight = TileSize * GridRows;
    public const float TrackHalfWidth = 20f;
    public const float CarRadius = 9f;
    public const float WaypointReachRadius = 24f;
    public const float CarAcceleration = 240f;
    public const float CarDragPerSecond = 1.4f;
    public const float OffTrackDragPerSecond = 5f;
    public const float OffTrackMaxSpeedFactor = 0.5f;

    public enum TileKind { Corner, StraightH, StraightV }

    public record TrackTileDef(int Col, int Row, TileKind Kind, float RotationDeg);
    public record CarOption(string Slug, string Name);

    public static readonly CarOption[] CarRoster =
    {
        new("silver", "Silver Streak"), new("navygold", "Midnight Gold"), new("yellow", "Sunburst"),
        new("blue", "Blue Bolt"), new("red", "Crimson"), new("green", "Emerald"), new("phoenix", "Phoenix"),
        new("shadow", "Shadow"), new("patriot", "Patriot"), new("sunset", "Sunset"), new("sky", "Sky Cruiser"),
        new("police", "Police Cruiser"), new("nightpatrol", "Night Patrol"), new("rally", "Rally Runner"),
    };
    // car-f1.png is excluded from the roster: it's drawn in side profile
    // (not top-down like every other car), so it can never look right
    // rotating around the track - it just spins in place. It's still used
    // as Time Trial Racer's launcher thumbnail (a static image, no
    // rotation involved there).

    // One curve tile image reused at all four corners via CSS rotation.
    // Verified directly against the pixels (not assumed): at rotate(0deg)
    // the drivable asphalt touches the tile's West and South edges - the
    // opposite red/white rumble strip caps the North and East sides. Each
    // corner tile below needs whichever rotation makes the art's open
    // edges land on that corner's two actual track neighbors:
    //   TL(0,0) neighbors South(0,1) + East(1,0)  -> needs {S,W}->{S,E}: 270deg
    //   TR(3,0) neighbors West(2,0) + South(3,1)  -> needs {S,W}->{S,W}: 0deg
    //   BR(3,5) neighbors West(2,5) + North(3,4)  -> needs {S,W}->{N,W}: 90deg
    //   BL(0,5) neighbors East(1,5) + North(0,4)  -> needs {S,W}->{N,E}: 180deg
    public static readonly TrackTileDef[] TrackTiles =
    {
        new(0, 0, TileKind.Corner, 270f),
        new(1, 0, TileKind.StraightH, 0f),
        new(2, 0, TileKind.StraightH, 0f),
        new(3, 0, TileKind.Corner, 0f),
        new(3, 1, TileKind.StraightV, 0f),
        new(3, 2, TileKind.StraightV, 0f),
        new(3, 3, TileKind.StraightV, 0f),
        new(3, 4, TileKind.StraightV, 0f),
        new(3, 5, TileKind.Corner, 90f),
        new(2, 5, TileKind.StraightH, 0f),
        new(1, 5, TileKind.StraightH, 0f),
        new(0, 5, TileKind.Corner, 180f),
        new(0, 4, TileKind.StraightV, 0f),
        new(0, 3, TileKind.StraightV, 0f),
        new(0, 2, TileKind.StraightV, 0f),
        new(0, 1, TileKind.StraightV, 0f),
    };

    public static readonly Vector2[] Waypoints = TrackTiles
        .Select(t => new Vector2(TileCenterX(t), TileCenterY(t)))
        .ToArray();

    public static float TileCenterX(TrackTileDef t) => t.Col * TileSize + TileSize / 2f;
    public static float TileCenterY(TrackTileDef t) => t.Row * TileSize + TileSize / 2f;

    public static string TileImageFile(TileKind k) => k switch
    {
        TileKind.Corner => "tile-corner.png",
        TileKind.StraightH => "tile-straight-h.png",
        _ => "tile-straight-v.png",
    };

    public class RacerState
    {
        public PhysicsBody Body = new() { Radius = CarRadius };
        public float FacingDeg = -90f;
        public int NextWaypointIndex = 1;
        public int LapsCompleted;
        public string CarSlug = "silver";
        public float MaxSpeed = 95f;
        public bool IsPlayer;
    }

    /// <summary>Advances one racer by dt: accelerates toward target (if
    /// any) exactly like SpaceGame's ship, caps speed lower and adds
    /// extra drag when off the track's centerline (grass, not a wall),
    /// then advances the racer's waypoint/lap progress on reaching its
    /// next waypoint. Used identically for a human's pointer-drag target
    /// and a CPU's next-waypoint target - same function either way.</summary>
    public static void UpdateRacer(RacerState racer, Vector2? target, float dt)
    {
        if (target is { } t)
        {
            var toTarget = t - racer.Body.Position;
            if (toTarget.LengthSquared() > 4f)
            {
                var dir = Vector2.Normalize(toTarget);
                racer.Body.Velocity += dir * CarAcceleration * dt;
            }
        }

        var onTrack = DistanceToTrackCenterline(racer.Body.Position) <= TrackHalfWidth;
        var maxSpeed = onTrack ? racer.MaxSpeed : racer.MaxSpeed * OffTrackMaxSpeedFactor;
        if (racer.Body.Velocity.LengthSquared() > maxSpeed * maxSpeed)
        {
            racer.Body.Velocity = Vector2.Normalize(racer.Body.Velocity) * maxSpeed;
        }

        Physics2D.ApplyDrag(racer.Body, dt, onTrack ? CarDragPerSecond : OffTrackDragPerSecond);
        racer.Body.Position += racer.Body.Velocity * dt;
        racer.Body.Position = new Vector2(
            Math.Clamp(racer.Body.Position.X, CarRadius, FieldWidth - CarRadius),
            Math.Clamp(racer.Body.Position.Y, CarRadius, FieldHeight - CarRadius));

        if (racer.Body.Velocity.LengthSquared() > 4f)
        {
            racer.FacingDeg = MathF.Atan2(racer.Body.Velocity.Y, racer.Body.Velocity.X) * 180f / MathF.PI;
        }

        var waypoint = Waypoints[racer.NextWaypointIndex];
        if ((racer.Body.Position - waypoint).LengthSquared() <= WaypointReachRadius * WaypointReachRadius)
        {
            racer.NextWaypointIndex++;
            if (racer.NextWaypointIndex >= Waypoints.Length)
            {
                racer.NextWaypointIndex = 0;
                racer.LapsCompleted++;
            }
        }
    }

    // Distance to the nearest point on the closed waypoint loop, not just
    // the nearest waypoint - a car centered exactly between two waypoints
    // on a straight is ~30 units from each one even though it's dead
    // center on the road, which would wrongly read as off-track against
    // a ~20-unit track half-width if only the nearest *point* counted.
    public static float DistanceToTrackCenterline(Vector2 p)
    {
        var minDist = float.MaxValue;
        for (var i = 0; i < Waypoints.Length; i++)
        {
            var a = Waypoints[i];
            var b = Waypoints[(i + 1) % Waypoints.Length];
            var d = DistancePointToSegment(p, a, b);
            if (d < minDist) minDist = d;
        }
        return minDist;
    }

    private static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        var t = Vector2.Dot(p - a, ab) / MathF.Max(ab.LengthSquared(), 0.0001f);
        t = Math.Clamp(t, 0f, 1f);
        var closest = a + ab * t;
        return Vector2.Distance(p, closest);
    }

    // Car art points "up" (negative Y) at rotate(0deg) - same convention
    // as SpaceGame's ship, same +90 alignment.
    public static float CssAngle(float facingDeg) => facingDeg + 90f;
}
