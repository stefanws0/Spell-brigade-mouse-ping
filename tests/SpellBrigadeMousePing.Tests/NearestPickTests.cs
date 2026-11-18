namespace SpellBrigadeMousePing.Tests;

using SpellBrigadeMousePing.Core;
using Xunit;

public class NearestPickTests
{
    private static readonly Vec3 Center = new(0, 0, 0);

    [Fact]
    public void No_candidates_gives_minus_one() => Assert.Equal(-1, NearestPick.Pick(Center, new Vec3[0], 2.5f));

    [Fact]
    public void Candidates_outside_the_radius_are_ignored() =>
        Assert.Equal(-1, NearestPick.Pick(Center, new[] { new Vec3(3, 0, 0), new Vec3(0, 0, -2.6f) }, 2.5f));

    [Fact]
    public void Nearest_candidate_wins() =>
        Assert.Equal(1, NearestPick.Pick(Center, new[] { new Vec3(2, 0, 0), new Vec3(0, 0, 1), new Vec3(-1.5f, 0, 0) }, 2.5f));

    [Fact]
    public void Height_is_ignored() =>
        Assert.Equal(0, NearestPick.Pick(Center, new[] { new Vec3(0.5f, 9, 0), new Vec3(1, 0, 0) }, 2.5f));

    [Fact]
    public void Tie_keeps_the_first() =>
        Assert.Equal(0, NearestPick.Pick(Center, new[] { new Vec3(1, 0, 0), new Vec3(-1, 0, 0) }, 2.5f));

    [Fact]
    public void Exactly_on_the_radius_counts() =>
        Assert.Equal(0, NearestPick.Pick(Center, new[] { new Vec3(2.5f, 0, 0) }, 2.5f));
}
