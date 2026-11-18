namespace SpellBrigadeMousePing.Game;

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Quick pings land at the mouse cursor. The game's <c>SmartPing()</c> still runs (cooldown, anti-spam, emote wheel):
///  * its prefix stores the ground point under the cursor and shrinks the game's search range around the wizard to
///    almost nothing, so the game falls through to its location ping;
///  * the <c>PingLocationRpc</c> prefix swaps the wizard's position for the cursor point, or pings the nearest
///    pingable thing near the cursor through the game's own <c>PingRpc</c> (called, not patched);
///  * the postfix puts the range back (<see cref="RestoreRangeIfLeftOver"/> is the per-frame failsafe).
/// Any failure leaves the game's own ping untouched.
/// </summary>
internal static class PingPatches
{
    private const float NoSearchRange = 0.001f;
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static Vector3? _pendingCursorPoint;
    private static float? _savedRange;
    private static SmartPingManager? _rangeOwner;
    private static int _pingCount;

    public static void Install()
    {
        var smartPing = typeof(SmartPingManager).GetMethod(nameof(SmartPingManager.SmartPing), AnyInstance);
        var pingLocation = typeof(SmartPingManager).GetMethod(nameof(SmartPingManager.PingLocationRpc), AnyInstance);
        if (smartPing == null || pingLocation == null)
        {
            ModLog.Error("SmartPingManager.SmartPing or PingLocationRpc not found: mouse pings are off.");
            return;
        }

        // Inspect both before judging either: shared native code only shows once both are known.
        NativeCode.Inspect(smartPing, "SmartPingManager.SmartPing");
        NativeCode.Inspect(pingLocation, "SmartPingManager.PingLocationRpc");
        if (!NativeCode.CanPatch(pingLocation, "SmartPingManager.PingLocationRpc", out var whyLocation))
        {
            ModLog.Error($"Refusing to patch SmartPingManager.PingLocationRpc ({whyLocation}): mouse pings are off.");
            return;
        }

        if (!NativeCode.CanPatch(smartPing, "SmartPingManager.SmartPing", out var whySmart))
        {
            ModLog.Error($"Refusing to patch SmartPingManager.SmartPing ({whySmart}): mouse pings are off.");
            return;
        }

        // Redirect first: never shrink the game's range without the redirect in place.
        if (!Patch(pingLocation, $"SmartPingManager.PingLocationRpc [{whyLocation}]", nameof(PingLocationPrefix), null)) return;
        Patch(smartPing, $"SmartPingManager.SmartPing [{whySmart}]", nameof(SmartPingPrefix), nameof(SmartPingPostfix));
    }

    /// <summary>Per-frame failsafe: if SmartPing's postfix never ran, the game's range must not stay shrunk.</summary>
    public static void RestoreRangeIfLeftOver()
    {
        if (_savedRange == null) return;
        ModLog.WarnOnce("range-leftover", "SmartPing's range was still shrunk a frame later (its postfix didn't run); restored.");
        _pendingCursorPoint = null;
        RestoreRange();
    }

    // ---------------------------------------------------------------- patches

    private static void SmartPingPrefix(SmartPingManager __instance)
    {
        try
        {
            _pendingCursorPoint = null;
            if (!ModConfig.Enabled.Value) return;
            ModLog.StepOnce("smartping-prefix", "first SmartPing prefix");
            if (!CursorProbe.Usable(Time.unscaledTime))
            {
                LogPing("cursor hidden: the game's own ping");
                return;
            }

            var player = __instance.GetLocalPlayer();
            if (player == null)
            {
                LogPing("no local player: the game's own ping");
                return;
            }

            var point = CursorProbe.GroundUnderCursor(player.transform.position.y);
            if (point == null)
            {
                LogPing("no ground under the cursor: the game's own ping");
                return;
            }

            _pendingCursorPoint = point;
            _savedRange = __instance.range;
            _rangeOwner = __instance;
            __instance.range = NoSearchRange;
        }
        catch (Exception ex)
        {
            _pendingCursorPoint = null;
            RestoreRange();
            ModLog.ErrorThrottled("smartping-prefix", ex, Time.unscaledTime);
        }
    }

    private static void SmartPingPostfix()
    {
        try
        {
            RestoreRange();
            if (_pendingCursorPoint != null) LogPing("the game sent no location ping (cooldown, or it pinged something at the wizard)");
            _pendingCursorPoint = null;
        }
        catch (Exception ex)
        {
            ModLog.ErrorThrottled("smartping-postfix", ex, Time.unscaledTime);
        }
    }

    /// <summary>__0 = the pinging client's id, __1 = the position the game wants to ping (the wizard's).</summary>
    private static bool PingLocationPrefix(SmartPingManager __instance, ulong __0, ref Vector3 __1)
    {
        if (_pendingCursorPoint == null) return true; // not a cursor ping (or NGO delivering one we already changed)
        var cursor = _pendingCursorPoint.Value;
        _pendingCursorPoint = null; // consume once
        try
        {
            ModLog.StepOnce("pinglocation-prefix", "first PingLocationRpc redirect");
            if (ModConfig.SnapToObjects.Value)
            {
                var seen = ModConfig.LogPings.Value ? new List<string>() : null;
                NetworkObject? target = null;
                try
                {
                    target = PingTargets.Nearest(cursor, ModConfig.SnapRadius.Value, seen);
                }
                catch (Exception ex)
                {
                    ModLog.ErrorThrottled("ping-targets", ex, Time.unscaledTime);
                }

                if (target != null)
                {
                    LogPing($"object '{target.name}' near the cursor {Fmt(cursor)} (wizard at {Fmt(__1)}); near cursor: {Join(seen)}");
                    ModLog.StepOnce("pingrpc", "first PingRpc for an object near the cursor");
                    __instance.PingRpc(__0, new NetworkObjectReference(target));
                    return false;
                }

                LogPing($"spot {Fmt(cursor)} (wizard at {Fmt(__1)}); near cursor: {Join(seen)}");
            }
            else
            {
                LogPing($"spot {Fmt(cursor)} (wizard at {Fmt(__1)})");
            }

            __1 = cursor;
            return true;
        }
        catch (Exception ex)
        {
            ModLog.ErrorThrottled("pinglocation-prefix", ex, Time.unscaledTime);
            return true;
        }
    }

    // ---------------------------------------------------------------- helpers

    private static void RestoreRange()
    {
        if (_savedRange == null) return;
        try
        {
            if (_rangeOwner != null) _rangeOwner.range = _savedRange.Value;
        }
        finally
        {
            _savedRange = null;
            _rangeOwner = null;
        }
    }

    private static bool Patch(MethodInfo target, string label, string? prefix, string? postfix)
    {
        try
        {
            ModLog.Step($"patch {label}");
            Plugin.Harmony.Patch(target,
                prefix: prefix == null ? null : new HarmonyMethod(typeof(PingPatches).GetMethod(prefix, BindingFlags.Static | BindingFlags.NonPublic)),
                postfix: postfix == null ? null : new HarmonyMethod(typeof(PingPatches).GetMethod(postfix, BindingFlags.Static | BindingFlags.NonPublic)));
            ModLog.Step($"patch ok: {label}");
            ModLog.Info($"Patched {label}.");
            return true;
        }
        catch (Exception ex)
        {
            ModLog.Error($"Could not patch {label}: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    // No Il2CppInterop XrefScanner here: 0.1.0 ran XrefScanner.XrefScan(SmartPing) in Plugin.Load and it killed the
    // game at startup (AccessViolationException in XrefGlobalClassFilter, uncatchable in .NET 6).

    private static void LogPing(string what)
    {
        if (!ModConfig.LogPings.Value) return;
        ModLog.Info($"Ping {++_pingCount}: {what} [cursor visible: {CursorProbe.VisibleNow()}, mouse active: {CursorProbe.MouseActiveText()}]");
    }

    private static string Fmt(Vector3 v) => $"({v.x:0.0}, {v.y:0.0}, {v.z:0.0})";

    private static string Join(List<string>? seen) => seen == null || seen.Count == 0 ? "nothing" : string.Join(", ", seen);
}
