namespace SpellBrigadeMousePing;

using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SpellBrigadeMousePing.Game;

/// <summary>BepInEx entry point: config, the per-frame runner and the two ping patches.</summary>
[BepInPlugin(Guid, Name, Version)]
public class Plugin : BasePlugin
{
    public const string Guid = "com.stefanws0.spellbrigademouseping";
    public const string Name = "SpellBrigadeMousePing";
    public const string Version = "0.2.0";

    internal static Harmony Harmony { get; private set; } = null!;

    public override void Load()
    {
        ModLog.Init(Log);
        ModLog.Step("Load: config");
        ModConfig.Init(Config);
        Harmony = new Harmony(Guid);
        ModLog.Step("Load: registering PingRunner");
        AddComponent<PingRunner>();
        if (ModConfig.Enabled.Value) PingPatches.Install();
        else ModLog.Info("General.Enabled is off: no patches, pings stay as the game makes them.");
        ModLog.Step("Load: done");
        ModLog.Info($"{Name} {Version} loaded.");
    }
}
