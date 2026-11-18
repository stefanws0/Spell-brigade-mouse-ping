# Mouse Ping

A mod for The Spell Brigade: your quick ping (middle mouse or V) lands where your mouse cursor is, instead of on your
wizard. If a potion, magnet, objective or anything else pingable is close to the cursor, it pings that.

With a controller, or when the game hides the cursor, pinging works like normal.

Install it from Thunderstore with r2modman or Gale:
[StrayingFox/Mouse_Ping](https://thunderstore.io/c/the-spell-brigade/p/StrayingFox/Mouse_Ping/).

## Building

Copy `GamePaths.props.user.example` to `GamePaths.props.user` and point it at your BepInEx folder. Then, with the game
closed:

```bash
dotnet build src/SpellBrigadeMousePing -c Release -p:DeployToGame=true
```

Tests: `dotnet test tests/SpellBrigadeMousePing.Tests`. Thunderstore zip: `dotnet build src/SpellBrigadeMousePing -c Release -t:Package`.

## My other mods

- [Dash](https://github.com/stefanws0/Spell-brigade-dash)
- [Minimap](https://github.com/stefanws0/Spell-Bridge-Minimap)
