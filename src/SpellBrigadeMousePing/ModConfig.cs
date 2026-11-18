namespace SpellBrigadeMousePing;

using BepInEx.Configuration;

/// <summary>BepInEx/config/com.stefanws0.spellbrigademouseping.cfg</summary>
internal static class ModConfig
{
    public static ConfigEntry<bool> Enabled = null!;
    public static ConfigEntry<bool> SnapToObjects = null!;
    public static ConfigEntry<float> SnapRadius = null!;
    public static ConfigEntry<bool> LogPings = null!;

    public static void Init(ConfigFile cfg)
    {
        Enabled = cfg.Bind("General", "Enabled", true,
            "Quick pings (middle mouse / V) land at the mouse cursor instead of around your wizard. Read at every ping; " +
            "if it is off when the game starts, the mod doesn't patch anything.");
        SnapToObjects = cfg.Bind("General", "SnapToObjects", true,
            "Ping the nearest pingable thing (potion, magnet, objective, revive zone...) near the cursor instead of the bare spot.");
        SnapRadius = cfg.Bind("General", "SnapRadius", 2.5f, new ConfigDescription(
            "How close to the cursor (game units) something must be to snap the ping to it.",
            new AcceptableValueRange<float>(0.1f, 10f)));
        LogPings = cfg.Bind("Debug", "LogPings", false,
            "Write one line per ping (cursor state, chosen target, what was near the cursor) to BepInEx/SpellBrigadeMousePing.log.");
    }
}
