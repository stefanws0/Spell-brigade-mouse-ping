# Changelog

## 0.1.1 (2026-11-18)

- Fix: 0.1.0 crashed the game at startup. Its diagnostic `XrefScanner.XrefScan(SmartPing)` in `Plugin.Load` hit an
  AccessViolationException inside Il2CppInterop (uncatchable in .NET 6). The scan is gone; nothing replaces it.

## 0.1.0 (2026-11-18, crashed at startup)

- Quick pings (middle mouse / V) land at the mouse cursor instead of around the wizard.
- Snap to the nearest pingable thing within 2.5 units of the cursor (the game's own "is this pingable?" checks).
- Gamepad or hidden cursor: the game's own ping, unchanged.
