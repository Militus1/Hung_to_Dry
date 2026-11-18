# Hung to Dry

Bunker spiders no longer kill you. They **paralyse you, wrap you in silk and hang you from the ceiling**, and your team has a limited time to cut you down.

## How it works

- **Get bitten** and the venom takes hold. You stumble, your vision warps and your heart pounds. You can't outrun it for long.
- **Get caught** and you're wrapped and hung up. You can still look around from a camera, but you can't move.
- **A countdown starts.** If nobody cuts you down in time, you die in the cocoon.
- **Rescue a teammate** by standing near their cocoon and holding the cut-down key (E by default).

That's the whole idea. The rest you can find out by playing, and it's more fun that way. Keep an eye on what else is lurking around a hanging victim, and don't assume a spider that has gone quiet has gone away.

## Important: multiplayer

**Everyone in the lobby needs the mod, on the same version, with the same settings.** Settings are not synced from the host. The easiest way is to share one config file or leave everything on default.

## Settings

Everything can be tuned in `BepInEx/config/Militus.HungToDry.cfg` (it appears after the first run). The ones most people want:

| Setting | Default | What it does |
|---|---|---|
| `SecondsHangingBeforeDeath` | 45 | How long a victim lasts in the cocoon |
| `CutDownSeconds` | 4 | How long you hold the key to free someone |
| `VenomSeconds` | 8 | Time between the bite and collapsing (0 = wrapped instantly) |
| `ImmunitySeconds` | 10 | How long a rescued player is left alone by monsters |
| `EmergencyReleaseKey` | false | Turn on to free yourself with F9 if something goes wrong |

[Full list of settings](https://github.com/Militus1/Hung_to_Dry/blob/main/SETTINGS.md)

## Found a bug?

[Open an issue on GitHub](https://github.com/Militus1/Hung_to_Dry/issues/new/choose) and include your log (`BepInEx/LogOutput.log` from your r2modman profile folder). Say which version you have and whether you were the host or a client.

## Credits

Made by Militus, with AI assistance (Claude) for the code. Designed and tested by hand.
