namespace SpellBrigadeMousePing.Game;

using System;
using UnityEngine;

/// <summary>
/// The mod's only injected MonoBehaviour (IL2CPP needs the IntPtr constructor). Lives on BepInEx's persistent
/// manager object, so it survives scene changes.
/// </summary>
public class PingRunner : MonoBehaviour
{
    public PingRunner(IntPtr ptr) : base(ptr)
    {
    }

    public void Update()
    {
        try
        {
            CursorProbe.Track(Time.unscaledTime);
            PingPatches.RestoreRangeIfLeftOver();
        }
        catch (Exception ex)
        {
            ModLog.ErrorThrottled("runner", ex, Time.unscaledTime);
        }
    }

    /// <summary>Marks a normal quit, so the next start can tell a crash from a clean exit.</summary>
    public void OnApplicationQuit() => ModLog.Step(ModLog.CleanExit);
}
