using System.Numerics;

namespace KidsGameLauncher.Services;

/// <summary>
/// Shared top-down racing track/car layer, extracted from
/// Components/RacingGame.razor once Components/TimeTrialRacer.razor
/// became a second consumer of the exact same track, car roster, and
/// movement physics (same "extract once a second consumer genuinely
/// needs it" rule this codebase follows elsewhere - see GameAi.cs/
/// Physics2D.cs). Track layouts are hand-laid tile grids - one curve
/// tile image reused at every corner via CSS rotation, plus dedicated
/// horizontal/vertical straight tiles - offered as several selectable
/// TrackDef layouts (see Tracks) rather than a single hardcoded loop.
///
/// Coordinate space is the same "field units, not pixels or percent"
/// convention as Physics2D consumers (AirHockey/SpaceGame/RacingGame) -
/// callers render position as Position/FieldSize as a percentage of a
/// CSS-constrained field box, using the SELECTED track's own
/// FieldWidth/FieldHeight (tracks can be different sizes/aspect ratios).
/// </summary>
public static class RaceTrack
{
    public const float TileSize = 50f;
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

    public class TrackDef
    {
        public required string Id;
        public required string Name;
        public required string Description;
        public required int GridCols;
        public required int GridRows;
        public required TrackTileDef[] Tiles;
        public Vector2[] Waypoints = Array.Empty<Vector2>();
        public float FieldWidth => GridCols * TileSize;
        public float FieldHeight => GridRows * TileSize;
    }

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

    // One curve tile image reused at every corner across every track via
    // CSS rotation. Verified directly against the pixels (not assumed):
    // at rotate(0deg) the drivable asphalt touches the tile's West and
    // South edges - the opposite red/white rumble strip caps the North
    // and East sides. A corner's required rotation is fully determined by
    // its two neighbor directions (a set of two perpendicular compass
    // directions - the road always turns exactly 90 degrees):
    //   {South,West} -> 0deg (the art's own base orientation)
    //   {West,North} -> 90deg
    //   {North,East} -> 180deg
    //   {East,South} -> 270deg
    // Every corner below was assigned by working out its actual two
    // neighbor tiles' compass directions and looking up this table - not
    // guessed - since an earlier version of this file guessed wrong and
    // every corner rendered disconnected from its neighbors.
    public static readonly TrackDef[] Tracks = BuildTracks();

    private static TrackDef[] BuildTracks()
    {
        // Grand Prix Loop: the original 4x6 rounded rectangle.
        var grandPrix = new TrackDef
        {
            Id = "grand-prix",
            Name = "Grand Prix Loop",
            Description = "The classic rounded circuit",
            GridCols = 4,
            GridRows = 6,
            Tiles = new[]
            {
                new TrackTileDef(0, 0, TileKind.Corner, 270f),
                new TrackTileDef(1, 0, TileKind.StraightH, 0f),
                new TrackTileDef(2, 0, TileKind.StraightH, 0f),
                new TrackTileDef(3, 0, TileKind.Corner, 0f),
                new TrackTileDef(3, 1, TileKind.StraightV, 0f),
                new TrackTileDef(3, 2, TileKind.StraightV, 0f),
                new TrackTileDef(3, 3, TileKind.StraightV, 0f),
                new TrackTileDef(3, 4, TileKind.StraightV, 0f),
                new TrackTileDef(3, 5, TileKind.Corner, 90f),
                new TrackTileDef(2, 5, TileKind.StraightH, 0f),
                new TrackTileDef(1, 5, TileKind.StraightH, 0f),
                new TrackTileDef(0, 5, TileKind.Corner, 180f),
                new TrackTileDef(0, 4, TileKind.StraightV, 0f),
                new TrackTileDef(0, 3, TileKind.StraightV, 0f),
                new TrackTileDef(0, 2, TileKind.StraightV, 0f),
                new TrackTileDef(0, 1, TileKind.StraightV, 0f),
            },
        };

        // Thunder Oval: the same rounded-rectangle shape as Grand Prix,
        // just wide and short (6x4) instead of narrow and tall (4x6) -
        // longer straights, quicker corners. Every corner's two neighbor
        // directions are identical to Grand Prix's own four corners (a
        // rectangle's corners have the same relative neighbor layout
        // regardless of its size), so the same four rotations apply.
        var thunderOval = new TrackDef
        {
            Id = "thunder-oval",
            Name = "Thunder Oval",
            Description = "Wide and fast, long straights",
            GridCols = 6,
            GridRows = 4,
            Tiles = new[]
            {
                new TrackTileDef(0, 0, TileKind.Corner, 270f),
                new TrackTileDef(1, 0, TileKind.StraightH, 0f),
                new TrackTileDef(2, 0, TileKind.StraightH, 0f),
                new TrackTileDef(3, 0, TileKind.StraightH, 0f),
                new TrackTileDef(4, 0, TileKind.StraightH, 0f),
                new TrackTileDef(5, 0, TileKind.Corner, 0f),
                new TrackTileDef(5, 1, TileKind.StraightV, 0f),
                new TrackTileDef(5, 2, TileKind.StraightV, 0f),
                new TrackTileDef(5, 3, TileKind.Corner, 90f),
                new TrackTileDef(4, 3, TileKind.StraightH, 0f),
                new TrackTileDef(3, 3, TileKind.StraightH, 0f),
                new TrackTileDef(2, 3, TileKind.StraightH, 0f),
                new TrackTileDef(1, 3, TileKind.StraightH, 0f),
                new TrackTileDef(0, 3, TileKind.Corner, 180f),
                new TrackTileDef(0, 2, TileKind.StraightV, 0f),
                new TrackTileDef(0, 1, TileKind.StraightV, 0f),
            },
        };

        // Switchback Circuit: an L-shaped loop (a 6x6 bounding box with
        // its top-right 3x2 corner left empty) instead of a plain
        // rectangle - six corners instead of four, for a real change of
        // pace rather than just a resized oval. Each corner's rotation
        // was derived the same way as Grand Prix's (see the lookup table
        // above), from that specific corner's own two actual neighbors:
        //   (0,0) neighbors East(1,0)+South(0,1)   -> {E,S}: 270deg
        //   (2,0) neighbors West(1,0)+South(2,1)   -> {W,S}: 0deg
        //   (2,2) neighbors North(2,1)+East(3,2)   -> {N,E}: 180deg
        //   (5,2) neighbors West(4,2)+South(5,3)   -> {W,S}: 0deg
        //   (5,5) neighbors North(5,4)+West(4,5)   -> {N,W}: 90deg
        //   (0,5) neighbors East(1,5)+North(0,4)   -> {N,E}: 180deg
        var switchback = new TrackDef
        {
            Id = "switchback",
            Name = "Switchback Circuit",
            Description = "An L-shaped track with extra turns",
            GridCols = 6,
            GridRows = 6,
            Tiles = new[]
            {
                new TrackTileDef(0, 0, TileKind.Corner, 270f),
                new TrackTileDef(1, 0, TileKind.StraightH, 0f),
                new TrackTileDef(2, 0, TileKind.Corner, 0f),
                new TrackTileDef(2, 1, TileKind.StraightV, 0f),
                new TrackTileDef(2, 2, TileKind.Corner, 180f),
                new TrackTileDef(3, 2, TileKind.StraightH, 0f),
                new TrackTileDef(4, 2, TileKind.StraightH, 0f),
                new TrackTileDef(5, 2, TileKind.Corner, 0f),
                new TrackTileDef(5, 3, TileKind.StraightV, 0f),
                new TrackTileDef(5, 4, TileKind.StraightV, 0f),
                new TrackTileDef(5, 5, TileKind.Corner, 90f),
                new TrackTileDef(4, 5, TileKind.StraightH, 0f),
                new TrackTileDef(3, 5, TileKind.StraightH, 0f),
                new TrackTileDef(2, 5, TileKind.StraightH, 0f),
                new TrackTileDef(1, 5, TileKind.StraightH, 0f),
                new TrackTileDef(0, 5, TileKind.Corner, 180f),
                new TrackTileDef(0, 4, TileKind.StraightV, 0f),
                new TrackTileDef(0, 3, TileKind.StraightV, 0f),
                new TrackTileDef(0, 2, TileKind.StraightV, 0f),
                new TrackTileDef(0, 1, TileKind.StraightV, 0f),
            },
        };

        var tracks = new[] { grandPrix, thunderOval, switchback };
        foreach (var track in tracks)
        {
            track.Waypoints = track.Tiles.Select(t => new Vector2(TileCenterX(t), TileCenterY(t))).ToArray();
        }
        return tracks;
    }

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
        public float FacingDeg;
        public int NextWaypointIndex = 1;
        public int LapsCompleted;
        public string CarSlug = "silver";
        public float MaxSpeed = 95f;
        public bool IsPlayer;

        // Continuous "how far around the loop" measure, see TrackProgressAt.
        // Starts negative as a sentinel so UpdateRacer's first tick doesn't
        // compare against an uninitialized 0 and misfire a lap.
        public float TrackProgress = -1f;
    }

    /// <summary>Advances one racer by dt on the given track: accelerates
    /// toward target (if any) exactly like SpaceGame's ship, caps speed
    /// lower and adds extra drag when off the track's centerline (grass,
    /// not a wall), then advances lap progress and the CPU steering
    /// waypoint. Used identically for a human's pointer-drag target and a
    /// CPU's next-waypoint target - same function either way.</summary>
    public static void UpdateRacer(TrackDef track, RacerState racer, Vector2? target, float dt)
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

        var onTrack = DistanceToTrackCenterline(track, racer.Body.Position) <= TrackHalfWidth;
        var maxSpeed = onTrack ? racer.MaxSpeed : racer.MaxSpeed * OffTrackMaxSpeedFactor;
        if (racer.Body.Velocity.LengthSquared() > maxSpeed * maxSpeed)
        {
            racer.Body.Velocity = Vector2.Normalize(racer.Body.Velocity) * maxSpeed;
        }

        Physics2D.ApplyDrag(racer.Body, dt, onTrack ? CarDragPerSecond : OffTrackDragPerSecond);
        racer.Body.Position += racer.Body.Velocity * dt;
        racer.Body.Position = new Vector2(
            Math.Clamp(racer.Body.Position.X, CarRadius, track.FieldWidth - CarRadius),
            Math.Clamp(racer.Body.Position.Y, CarRadius, track.FieldHeight - CarRadius));

        if (racer.Body.Velocity.LengthSquared() > 4f)
        {
            racer.FacingDeg = MathF.Atan2(racer.Body.Velocity.Y, racer.Body.Velocity.X) * 180f / MathF.PI;
        }

        // Lap completion is driven by continuous progress along the closed
        // loop, not by whether the car ever entered one specific
        // waypoint's small capture circle. A real driven line (especially
        // a kid cutting a corner, or just not tracking the tile-center
        // line exactly) can easily swing wide of a single waypoint and
        // never enter its ~24-unit radius at all - with the old "advance
        // NextWaypointIndex only on capture" scheme that permanently
        // stalled every future lap for the rest of the run, since nothing
        // else ever re-synced the index. Progress dropping sharply means
        // it wrapped forward around the loop back near the start.
        var progress = TrackProgressAt(track, racer.Body.Position);
        if (racer.TrackProgress >= 0f && progress - racer.TrackProgress < -track.Waypoints.Length / 2f)
        {
            racer.LapsCompleted++;
        }
        racer.TrackProgress = progress;

        // NextWaypointIndex still exists purely to give a CPU racer a
        // concrete steering target (it always drives straight at this
        // exact point, so it reliably enters the capture radius) - no
        // longer used for lap counting.
        var waypoint = track.Waypoints[racer.NextWaypointIndex % track.Waypoints.Length];
        if ((racer.Body.Position - waypoint).LengthSquared() <= WaypointReachRadius * WaypointReachRadius)
        {
            racer.NextWaypointIndex = (racer.NextWaypointIndex + 1) % track.Waypoints.Length;
        }
    }

    // Distance to the nearest point on the closed waypoint loop, not just
    // the nearest waypoint - a car centered exactly between two waypoints
    // on a straight is ~30 units from each one even though it's dead
    // center on the road, which would wrongly read as off-track against
    // a ~20-unit track half-width if only the nearest *point* counted.
    public static float DistanceToTrackCenterline(TrackDef track, Vector2 p) => NearestTrackSegment(track, p).Dist;

    /// <summary>Continuous "how far around the closed track loop" measure
    /// for a position: segment index (0..Waypoints.Length-1) plus the
    /// 0..1 fraction along that segment - e.g. 3.5 means halfway between
    /// Waypoints[3] and Waypoints[4]. Monotonically increases as a car
    /// drives forward around the loop and wraps back near 0 only after a
    /// genuine full circuit, which is what makes it safe for lap
    /// detection (see UpdateRacer) in a way discrete waypoint capture
    /// isn't for a human-driven line.</summary>
    public static float TrackProgressAt(TrackDef track, Vector2 p)
    {
        var (segment, t, _) = NearestTrackSegment(track, p);
        return segment + t;
    }

    private static (int Segment, float T, float Dist) NearestTrackSegment(TrackDef track, Vector2 p)
    {
        var waypoints = track.Waypoints;
        var bestDist = float.MaxValue;
        var bestSegment = 0;
        var bestT = 0f;
        for (var i = 0; i < waypoints.Length; i++)
        {
            var a = waypoints[i];
            var b = waypoints[(i + 1) % waypoints.Length];
            var ab = b - a;
            var t = Vector2.Dot(p - a, ab) / MathF.Max(ab.LengthSquared(), 0.0001f);
            t = Math.Clamp(t, 0f, 1f);
            var closest = a + ab * t;
            var dist = Vector2.Distance(p, closest);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestSegment = i;
                bestT = t;
            }
        }
        return (bestSegment, bestT, bestDist);
    }

    // Car art points "up" (negative Y) at rotate(0deg) - same convention
    // as SpaceGame's ship, same +90 alignment.
    public static float CssAngle(float facingDeg) => facingDeg + 90f;

    /// <summary>Facing direction (same convention as RacerState.FacingDeg)
    /// a parked car should idle at before the race starts on the given
    /// track - the direction from the start point toward the next
    /// waypoint, so a parked car visually matches that track's own
    /// direction at the start line instead of a fixed guess (an earlier
    /// version used a fixed "facing up" default that looked wrong parked
    /// in a start corner whose own local road direction isn't
    /// north-south at all).</summary>
    public static float StartFacingDeg(TrackDef track)
    {
        var dir = track.Waypoints[1] - track.Waypoints[0];
        return MathF.Atan2(dir.Y, dir.X) * 180f / MathF.PI;
    }
}
