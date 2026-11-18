namespace SpellBrigadeMousePing.Game;

using System;
using Il2CppInterop.Runtime;
using SpellBrigadeMousePing.Core;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Where the mouse cursor points on the ground, and whether the cursor is there at all.</summary>
internal static class CursorProbe
{
    private const float VisibleGraceSeconds = 0.25f;
    private static readonly CursorRecency Recency = new(VisibleGraceSeconds);

    /// <summary>Called every frame by <see cref="PingRunner"/>.</summary>
    public static void Track(float now) => Recency.Observe(VisibleNow(), now);

    public static bool Usable(float now) => Recency.Usable(VisibleNow(), now);

    public static bool VisibleNow()
    {
        try
        {
            return Cursor.visible;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>The game's own "is the mouse the active input?" (for the log; spec: fallback signal if Cursor.visible lies).</summary>
    public static string MouseActiveText()
    {
        try
        {
            var found = UnityEngine.Object.FindAnyObjectByType(Il2CppType.Of<CursorService>());
            var detector = found?.TryCast<CursorService>()?.inputActivityDetector;
            return detector == null ? "?" : detector.IsMouseInputActive().ToString();
        }
        catch (Exception ex)
        {
            return "error " + ex.GetType().Name;
        }
    }

    /// <summary>Mouse position in screen pixels, or null when no mouse can be read.</summary>
    public static Vector2? ScreenPosition()
    {
        try
        {
            var mouse = Mouse.current;
            if (mouse != null) return mouse.position.ReadValue();
        }
        catch (Exception ex)
        {
            ModLog.WarnOnce("mouse-inputsystem", $"Mouse.current.position failed ({ex.Message}); trying Input.mousePosition.");
        }

        try
        {
            var p = Input.mousePosition;
            return new Vector2(p.x, p.y);
        }
        catch (Exception ex)
        {
            ModLog.WarnOnce("mouse-legacy", $"Input.mousePosition failed too ({ex.Message}): cursor pings can't work.");
            return null;
        }
    }

    /// <summary>The ground point under the cursor at height <paramref name="groundY"/>, or null (no camera or mouse, ray misses).</summary>
    public static Vector3? GroundUnderCursor(float groundY)
    {
        var screen = ScreenPosition();
        var cam = Camera.main;
        if (screen == null || cam == null) return null;
        var ray = cam.ScreenPointToRay(new Vector3(screen.Value.x, screen.Value.y, 0f));
        var o = ray.origin;
        var d = ray.direction;
        if (!GroundPoint.TryHit(new Vec3(o.x, o.y, o.z), new Vec3(d.x, d.y, d.z), groundY, out var hit)) return null;
        return new Vector3(hit.X, hit.Y, hit.Z);
    }
}
