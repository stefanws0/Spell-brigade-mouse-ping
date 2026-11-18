# Mouse Ping

Your quick ping (middle mouse button or V) lands where your mouse cursor is, instead of on your wizard.

If something pingable is close to the cursor, like a potion, a magnet, an objective or a revive zone, that gets pinged
instead of the ground. The game's ping cooldown still applies, and holding the middle button still opens the emote
wheel. With a controller, or when the game hides the cursor, pings work like they normally do.

Only you need the mod. Your teammates should see your pings where you pointed even without it, but I haven't tested
that in co-op yet.

## Settings

In `BepInEx/config/com.stefanws0.spellbrigademouseping.cfg`, created on first launch:

- `Enabled`: cursor pings on or off
- `SnapToObjects`: snap to pingable things near the cursor
- `SnapRadius`: how close something has to be to snap to it (default 2.5)
- `LogPings`: log every ping, handy for bug reports

Source and bug reports: https://github.com/stefanws0/Spell-brigade-mouse-ping
