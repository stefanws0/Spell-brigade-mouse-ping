namespace SpellBrigadeMousePing.Tests;

using SpellBrigadeMousePing.Core;
using Xunit;

public class GroundPointTests
{
    [Fact]
    public void Straight_down_hits_right_below_the_origin()
    {
        Assert.True(GroundPoint.TryHit(new Vec3(3, 10, -2), new Vec3(0, -1, 0), 0f, out var hit));
        Assert.Equal(3f, hit.X, 4);
        Assert.Equal(0f, hit.Y, 4);
        Assert.Equal(-2f, hit.Z, 4);
    }

    [Fact]
    public void Slanted_ray_hits_the_plane_at_the_given_height()
    {
        // camera 10 up looking 45 degrees down along +z, ground at y = 2 → 8 units ahead
        Assert.True(GroundPoint.TryHit(new Vec3(0, 10, 0), new Vec3(0, -1, 1), 2f, out var hit));
        Assert.Equal(0f, hit.X, 4);
        Assert.Equal(2f, hit.Y, 4);
        Assert.Equal(8f, hit.Z, 4);
    }

    [Fact]
    public void Ray_parallel_to_the_ground_misses() =>
        Assert.False(GroundPoint.TryHit(new Vec3(0, 10, 0), new Vec3(1, 0, 0), 0f, out _));

    [Fact]
    public void Ray_pointing_away_from_the_ground_misses() =>
        Assert.False(GroundPoint.TryHit(new Vec3(0, 10, 0), new Vec3(0, 1, 1), 0f, out _));
}
