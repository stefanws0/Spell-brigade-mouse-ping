# Changelog

## 0.2.1 (2026-11-18)

- New icon in the game's own badge style (white ring, black disc, rough hand-cut silhouette): a mouse pointer
  pinging the ground. No code changes.

## 0.2.0 (2026-11-18)

- First release package (r2modman / Thunderstore zip: `dotnet build src/SpellBrigadeMousePing -c Release -t:Package`).
- `Debug.LogPings` is now off by default (an existing config keeps its saved value).
- Depends on `BepInEx-BepInExPack_IL2CPP-6.0.788`, the BepInEx build the game runs (the commit message saying 6.0.755
  "isn't listed" was wrong: it exists, 6.0.788 is just the matching one).
- Published on Thunderstore as `StrayingFox-SpellBrigadeMousePing` (The Spell Brigade, category Mods), 18 Nov.

## 0.1.1 (2026-11-18, verified in game)

- Verified by Stefan (solo, 16:37-16:44): pings at the cursor, snaps to a HealthPickUp and a HerdingTarget, the game's
  cooldown still blocks spam, holding the middle button opens the emote wheel, hidden cursor + V = ping at the wizard.

- Fix: 0.1.0 crashed the game at startup. Its diagnostic `XrefScanner.XrefScan(SmartPing)` in `Plugin.Load` hit an
  AccessViolationException inside Il2CppInterop (uncatchable in .NET 6). The scan is gone; nothing replaces it.

## 0.1.0 (2026-11-18, crashed at startup)

- Quick pings (middle mouse / V) land at the mouse cursor instead of around the wizard.
- Snap to the nearest pingable thing within 2.5 units of the cursor (the game's own "is this pingable?" checks).
- Gamepad or hidden cursor: the game's own ping, unchanged.
