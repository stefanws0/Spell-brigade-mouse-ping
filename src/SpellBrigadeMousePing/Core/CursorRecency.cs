namespace SpellBrigadeMousePing.Core;

/// <summary>
/// "Is the mouse cursor there?": visible now, or visible within a short grace time (a click can unhide the game's
/// auto-hidden cursor in the same frame the ping fires). Fed once per frame with <see cref="Observe"/>.
/// </summary>
internal sealed class CursorRecency
{
    private readonly float _graceSeconds;
    private float _lastVisible = float.NegativeInfinity;

    public CursorRecency(float graceSeconds) => _graceSeconds = graceSeconds;

    public void Observe(bool visible, float now)
    {
        if (visible) _lastVisible = now;
    }

    public bool Usable(bool visibleNow, float now) => visibleNow || now - _lastVisible <= _graceSeconds;
}
