namespace SpellBrigadeMousePing.Core;

/// <summary>A plain 3-float vector, so the pure logic compiles and tests without Unity.</summary>
internal struct Vec3
{
    public float X;
    public float Y;
    public float Z;

    public Vec3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString() => $"({X:0.0}, {Y:0.0}, {Z:0.0})";
}

/// <summary>Where the camera ray through the mouse cursor meets the ground.</summary>
internal static class GroundPoint
{
    /// <summary>
    /// The point where the ray (origin, direction) meets the horizontal plane y = <paramref name="groundY"/>.
    /// False when the ray is (nearly) parallel to the ground or points away from it.
    /// </summary>
    public static bool TryHit(Vec3 origin, Vec3 direction, float groundY, out Vec3 hit)
    {
        hit = default;
        if (System.Math.Abs(direction.Y) < 1e-4f) return false;
        var t = (groundY - origin.Y) / direction.Y;
        if (t <= 0f) return false;
        hit = new Vec3(origin.X + direction.X * t, groundY, origin.Z + direction.Z * t);
        return true;
    }
}
