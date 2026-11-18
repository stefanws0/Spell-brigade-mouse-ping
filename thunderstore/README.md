# Spell Brigade Mouse Ping

Point at something with your mouse and ping it: your quick ping (**middle mouse button** or **V**) lands where your
**cursor** is, not on your wizard.

- **Ping anywhere on screen:** the ping marker appears on the ground under your cursor.
- **Snaps to things:** if a potion, magnet, objective, revive zone or anything else the game lets you ping is within
  2.5 units of the cursor, that thing is pinged instead of the bare ground.
- **Everything else stays the same:** the game's own ping cooldown and spam protection still apply, and holding the
  middle button still opens the emote wheel.
- **Gamepad and hidden cursor:** when you play with a controller, or the game has hidden the mouse cursor, pings work
  exactly like in the unmodded game.

## Multiplayer

Only you need the mod to ping at your cursor. Your teammates should see those pings where you pointed even without it,
because the position travels in the game's own ping message. That part hasn't been tested in co-op yet.

## Configuration

`BepInEx/config/com.stefanws0.spellbrigademouseping.cfg` is created after the first launch:

| Setting | Default | What it does |
|---|---|---|
| `General.Enabled` | true | Cursor pings on or off |
| `General.SnapToObjects` | true | Snap to pingable things near the cursor |
| `General.SnapRadius` | 2.5 | How close to the cursor (game units) something must be to snap to it |
| `Debug.LogPings` | false | Write one line per ping to `BepInEx/SpellBrigadeMousePing.log` (for bug reports) |

## Source

https://github.com/stefanws0/Spell-brigade-mouse-ping
