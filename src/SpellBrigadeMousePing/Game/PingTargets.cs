namespace SpellBrigadeMousePing.Game;

using System;
using System.Collections.Generic;
using SpellBrigadeMousePing.Core;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Pingable things near a point, judged by the game's own SmartPing filters: the capture-free lambdas
/// <c>&lt;SmartPing&gt;b__19_0..4</c> on <c>SmartPingManager.&lt;&gt;c</c>. They are CALLED here, never patched.
/// </summary>
internal static class PingTargets
{
    private const int MaxLogged = 12;

    /// <summary>
    /// The pingable NetworkObject nearest to <paramref name="center"/> within <paramref name="radius"/>, or null.
    /// <paramref name="seen"/> (optional) collects "collider" or "collider -> object" for the log.
    /// </summary>
    public static NetworkObject? Nearest(Vector3 center, float radius, List<string>? seen)
    {
        ModLog.StepOnce("overlap", "first cursor OverlapSphere + game ping filters");
        var filters = SmartPingManager.__c.__9 ?? new SmartPingManager.__c();
        var colliders = Physics.OverlapSphere(center, radius, Physics.AllLayers, QueryTriggerInteraction.Collide);
        var objects = new List<NetworkObject>();
        var points = new List<Vec3>();
        if (colliders != null)
        {
            foreach (var c in colliders)
            {
                if (c == null) continue;
                NetworkObject? obj = null;
                bool ok;
                try
                {
                    ok = filters._SmartPing_b__19_0(c) && filters._SmartPing_b__19_1(c) && filters._SmartPing_b__19_2(c);
                    if (ok)
                    {
                        obj = filters._SmartPing_b__19_3(c);
                        ok = obj != null && filters._SmartPing_b__19_4(obj);
                    }
                }
                catch (Exception ex)
                {
                    ModLog.WarnOnce("filter-" + ex.GetType().Name, $"A game ping filter threw on {c.name}: {ex.Message}");
                    ok = false;
                }

                if (seen != null && seen.Count < MaxLogged) seen.Add(ok ? $"{c.name} -> {obj!.name}" : c.name);
                if (!ok) continue;
                var p = c.ClosestPointOnBounds(center);
                objects.Add(obj!);
                points.Add(new Vec3(p.x, p.y, p.z));
            }
        }

        var i = NearestPick.Pick(new Vec3(center.x, center.y, center.z), points, radius);
        return i < 0 ? null : objects[i];
    }
}
