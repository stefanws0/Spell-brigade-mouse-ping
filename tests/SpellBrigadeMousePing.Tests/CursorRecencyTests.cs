namespace SpellBrigadeMousePing.Tests;

using SpellBrigadeMousePing.Core;
using Xunit;

public class CursorRecencyTests
{
    [Fact]
    public void Never_seen_and_hidden_is_not_usable() => Assert.False(new CursorRecency(0.25f).Usable(false, 5f));

    [Fact]
    public void Visible_now_is_usable() => Assert.True(new CursorRecency(0.25f).Usable(true, 5f));

    [Fact]
    public void Hidden_but_seen_within_the_grace_time_is_usable()
    {
        var r = new CursorRecency(0.25f);
        r.Observe(true, 5f);
        Assert.True(r.Usable(false, 5.2f));
    }

    [Fact]
    public void Hidden_longer_than_the_grace_time_is_not_usable()
    {
        var r = new CursorRecency(0.25f);
        r.Observe(true, 5f);
        r.Observe(false, 5.1f);
        Assert.False(r.Usable(false, 5.3f));
    }
}
