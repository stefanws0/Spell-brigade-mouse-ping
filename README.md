# Spell Brigade Mouse Ping

On Thunderstore: [Mouse Ping by StrayingFox](https://thunderstore.io/c/the-spell-brigade/p/StrayingFox/Mouse_Ping/).

A BepInEx 6 (IL2CPP) mod for The Spell Brigade: quick pings (middle mouse button or V) land where your **mouse
cursor** points instead of around your wizard. If something pingable (potion, magnet, objective, revive zone...) is
within 2.5 units of the cursor, that is pinged; otherwise the spot under the cursor. With a gamepad, or when the game
has hidden the cursor, pings work exactly as in the unmodded game. Holding the middle button still opens the emote wheel.

Only the player who pings should need the mod (the ping's position travels in the game's own network message; to be
confirmed in co-op).

## Config (`BepInEx/config/com.stefanws0.spellbrigademouseping.cfg`)

| Setting | Default | What it does |
|---|---|---|
| `General.Enabled` | true | Cursor pings on/off (read at every ping; off at game start = no patches at all) |
| `General.SnapToObjects` | true | Snap to the nearest pingable thing near the cursor |
| `General.SnapRadius` | 2.5 | How close (game units) something must be to snap to it |
| `Debug.LogPings` | false | One line per ping in `BepInEx/SpellBrigadeMousePing.log` |

## Build

Copy `GamePaths.props.user.example` to `GamePaths.props.user` and point it at your BepInEx folder, then:

```bash
dotnet test tests/SpellBrigadeMousePing.Tests
```

```bash
dotnet build src/SpellBrigadeMousePing -c Release -p:DeployToGame=true
```

Deploy only while the game is closed (the DLL is locked while it runs).

Release zip for r2modman / Thunderstore (`dist/SpellBrigadeMousePing-<version>.zip`; bump `<Version>` in the csproj,
`Plugin.Version` and `thunderstore/manifest.json` together, the build refuses a mismatched manifest):

```bash
dotnet build src/SpellBrigadeMousePing -c Release -t:Package
```

The icon art is `art/icon_source.png` (generated in the style of the game's ability icons);
`python -I tools/make_icon.py art/icon_source.png thunderstore/icon.png` crops it to the 256px transparent badge.
