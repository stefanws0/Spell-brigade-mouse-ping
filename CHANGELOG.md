# Changelog

## 0.2.0 (2026-11-18)

- First release package (r2modman / Thunderstore zip: `dotnet build src/SpellBrigadeMousePing -c Release -t:Package`).
- `Debug.LogPings` is now off by default (an existing config keeps its saved value).

## 0.1.1 (2026-11-18, verified in game)

- Verified by Stefan (solo, 16:37-16:44): pings at the cursor, snaps to a HealthPickUp and a HerdingTarget, the game's
  cooldown still blocks spam, holding the middle button opens the emote wheel, hidden cursor + V = ping at the wizard.

- Fix: 0.1.0 crashed the game at startup. Its diagnostic `XrefScanner.XrefScan(SmartPing)` in `Plugin.Load` hit an
  AccessViolationException inside Il2CppInterop (uncatchable in .NET 6). The scan is gone; nothing replaces it.

## 0.1.0 (2026-11-18, crashed at startup)

- Quick pings (middle mouse / V) land at the mouse cursor instead of around the wizard.
- Snap to the nearest pingable thing within 2.5 units of the cursor (the game's own "is this pingable?" checks).
- Gamepad or hidden cursor: the game's own ping, unchanged.
