namespace SpellBrigadeMousePing.Core;

using System.Collections.Generic;

/// <summary>Which pingable thing a cursor ping snaps to.</summary>
internal static class NearestPick
{
    /// <summary>
    /// Index of the candidate closest to <paramref name="center"/> on the ground (x/z only, height ignored), or -1 when
    /// none is within <paramref name="radius"/>. A tie keeps the earlier candidate.
    /// </summary>
    public static int Pick(Vec3 center, IReadOnlyList<Vec3> candidates, float radius)
    {
        var best = -1;
        var bestSq = radius * radius;
        for (var i = 0; i < candidates.Count; i++)
        {
            var dx = candidates[i].X - center.X;
            var dz = candidates[i].Z - center.Z;
            var sq = dx * dx + dz * dz;
            if (sq > bestSq) continue;
            if (best >= 0 && sq >= bestSq) continue;
            best = i;
            bestSq = sq;
        }

        return best;
    }
}
