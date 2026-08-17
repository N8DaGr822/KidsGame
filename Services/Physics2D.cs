using System.Numerics;

namespace KidsGameLauncher.Services;

/// <summary>
/// A body in the 2D physics world - a circle with position, velocity, and
/// mass. Set <see cref="IsKinematic"/> for player/CPU-controlled objects
/// (paddles, cues) that should push other bodies around but never be
/// pushed themselves; their position is driven directly by input/AI each
/// step rather than by forces.
/// </summary>
public sealed class PhysicsBody
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius = 1f;
    public float Mass = 1f;
    public bool IsKinematic;

    public float InverseMass => IsKinematic || Mass <= 0f ? 0f : 1f / Mass;
}

/// <summary>
/// Minimal real-time 2D rigid-body layer for games that need actual
/// continuous collision physics (Air Hockey, and later Pool/Billiards)
/// rather than the analytic single-arc trajectory solve used by
/// TankDuel/ArcheryChallenge/BasketballShot - a puck or ball bounces
/// repeatedly off walls and other bodies instead of flying one parabola
/// to a single impact point.
///
/// Deliberately scoped small: circle/circle and circle/wall collision
/// with impulse response and positional correction, plus time-based drag.
/// No angular velocity/spin yet - that's a follow-up layer for Pool once
/// this feels right in Air Hockey first (see GAMES_ROADMAP.md).
///
/// Callers are expected to step the world at a small fixed timestep
/// (e.g. 1/120s) with a capped number of substeps per rendered frame,
/// not once per render - a puck moving several radii in a single big
/// step can tunnel straight through a paddle or wall without ever
/// registering an overlap.
/// </summary>
public static class Physics2D
{
    /// <summary>Resolves an overlapping circle/circle collision: pushes
    /// the two bodies apart along the collision normal (proportional to
    /// each body's inverse mass, so a kinematic body like a paddle never
    /// moves) and applies an equal-and-opposite impulse so momentum
    /// transfers realistically instead of the two just sliding through
    /// each other. No-ops if the circles aren't overlapping.</summary>
    public static void ResolveCircleCollision(PhysicsBody a, PhysicsBody b, float restitution)
    {
        var delta = b.Position - a.Position;
        var radiusSum = a.Radius + b.Radius;
        var distanceSquared = delta.LengthSquared();
        if (distanceSquared >= radiusSum * radiusSum) return;

        var inverseMassSum = a.InverseMass + b.InverseMass;
        if (inverseMassSum <= 0f) return; // both kinematic - nothing to resolve

        var distance = MathF.Sqrt(distanceSquared);
        var normal = distance > 0.0001f ? delta / distance : Vector2.UnitX;

        // Positional correction: separate the overlap first so bodies
        // don't stay stuck inside each other at low relative speed.
        var penetration = radiusSum - distance;
        var correction = normal * (penetration / inverseMassSum);
        a.Position -= correction * a.InverseMass;
        b.Position += correction * b.InverseMass;

        var relativeVelocity = b.Velocity - a.Velocity;
        var velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);
        if (velocityAlongNormal >= 0f) return; // already separating

        var impulseMagnitude = -(1f + restitution) * velocityAlongNormal / inverseMassSum;
        var impulse = impulseMagnitude * normal;
        a.Velocity -= impulse * a.InverseMass;
        b.Velocity += impulse * b.InverseMass;
    }

    /// <summary>Bounces a circle off the left/right walls of an
    /// axis-aligned table [0, width], clamping position back inside and
    /// reflecting the X velocity.</summary>
    public static void ResolveWallCollisionX(PhysicsBody body, float width, float restitution)
    {
        if (body.Position.X - body.Radius < 0f)
        {
            body.Position = new Vector2(body.Radius, body.Position.Y);
            body.Velocity = new Vector2(-body.Velocity.X * restitution, body.Velocity.Y);
        }
        else if (body.Position.X + body.Radius > width)
        {
            body.Position = new Vector2(width - body.Radius, body.Position.Y);
            body.Velocity = new Vector2(-body.Velocity.X * restitution, body.Velocity.Y);
        }
    }

    /// <summary>Bounces a circle off the top/bottom walls of an
    /// axis-aligned table [0, height], clamping position back inside and
    /// reflecting the Y velocity. Split from the X version so a caller
    /// with goal mouths (Air Hockey) can skip this on the goal-mouth
    /// stretch of the top/bottom edge instead of always bouncing.</summary>
    public static void ResolveWallCollisionY(PhysicsBody body, float height, float restitution)
    {
        if (body.Position.Y - body.Radius < 0f)
        {
            body.Position = new Vector2(body.Position.X, body.Radius);
            body.Velocity = new Vector2(body.Velocity.X, -body.Velocity.Y * restitution);
        }
        else if (body.Position.Y + body.Radius > height)
        {
            body.Position = new Vector2(body.Position.X, height - body.Radius);
            body.Velocity = new Vector2(body.Velocity.X, -body.Velocity.Y * restitution);
        }
    }

    /// <summary>Exponential time-based drag, so the slowdown rate is
    /// independent of step size - a flat per-step multiplier like
    /// `velocity *= 0.99` would make friction depend on how many steps
    /// ran, not on real elapsed time.</summary>
    public static void ApplyDrag(PhysicsBody body, float dt, float dragPerSecond)
    {
        body.Velocity *= MathF.Exp(-dragPerSecond * dt);
        if (body.Velocity.LengthSquared() < 0.01f) body.Velocity = Vector2.Zero;
    }
}
