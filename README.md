# Hung to Dry

Bunker spiders no longer kill outright. They paralyse you, wrap you in silk, drag you to their lair and hang you from the ceiling. Hold a key to cut a teammate down before the timer runs out. Brackens and loot bugs may steal the cocoon.

The Bunker Spider's bite no longer kills. Instead, you're paralysed, wrapped in web and carried back to the spider's lair, where you're hung from the ceiling in a cocoon. You watch it all happen from a camera you can turn with the mouse. If nobody frees you before the timer runs out, you die in the cocoon.

**Teammates can save you.** Stand near a hung cocoon and hold the cut-down key until the silk gives way. A countdown on your screen (and next to the cut-down prompt) shows how long is left.

**Thieves.** A Bracken or a loot bug that spots a hung cocoon may cut it down and carry it away: the Bracken to its lair, the loot bug up over its head to its nest. The victim stays wrapped and gets 30 extra seconds, so find them fast. Nobody is spared by being carried off: if the timer runs out, they die.

## Features

- Paralyse, wrap, drag and hang, using the spider's own animations
- Look around while you're wrapped (mouse to turn, mouse wheel to zoom)
- Teammates cut victims down by holding a key near the cocoon, with a short immunity afterwards
- Everything is synced in multiplayer
- **Thieves:** Brackens and loot bugs can steal a hung victim (the victim gets extra time)
- **Death countdown** on the wrapped player's screen and next to the rescue prompt
- Wrapped players cannot be targeted by any monster, and a freed player is left alone by every monster for a few seconds
- A Bracken that loses its target keeps stalking instead of retreating; no black screen when a Bracken kills you
- **Chase boost:** the spider speeds up as it closes in, but only while it has a clear line of sight to you, so breaking line of sight slows it down again
- **Web alert:** step in a web and the spider sprints to it. The further away it is, the faster it runs
- **Waiting camouflage:** a spider waiting motionless dissolves into flickering speckles (using the Barber's dissolve effect) and is easy to miss until you get close
- **Hang height:** hung victims are lifted to a set height above the floor, so teammates can always reach them to cut them down
- **More webs:** spiders place more webs, more often, over a wider area
- Anti-stall protection for spiders that get stuck or drop their victim
- Wrapped players are invisible, non-solid and have no name tag, so nobody bumps into a ghost

## Installing

**Every player in the lobby needs the mod, and it must be the same version for everyone.** The spider speed boosts and the web length are worked out on each player's own game or the host, so mismatched versions will look and play differently.

## Settings

The settings file is `BepInEx/config/Militus.HungToDry.cfg`. It appears after the first time you run the game with the mod.

**Everyone in the lobby should use the same settings.** The settings are not synced from the host. Each player's own file applies on their own game, and the death timer in particular runs on the hung player's game. If settings differ, timers on other players' screens can disagree with when the victim actually dies. The easiest way is to share one config file, or leave everything on default.

| Setting | Default | What it does |
|---|---|---|
| `SecondsHangingBeforeDeath` | 45 | How long a victim stays hung up before dying |
| `CutDownKey` | E | Hold this near a hung cocoon to cut the victim down |
| `CutDownSeconds` | 4 | How long the key must be held |
| `ImmunitySeconds` | 10 | Time after a rescue when no monster (spider, Bracken, loot bug) can target the freed player |
| `VictimHoldSeconds` | 60 | Safety net: if the spider never hangs the victim, they are released after this long |
| `CarryTimeoutSeconds` | 30 | A spider carrying a victim this long without hanging them hangs them where it is (0 = off) |
| `WallReachMeters` | 0.8 | How close the spider must get to its wall spot before it hangs a victim (the game wants 0.1) |
| `WrapOnCatch` | true | The spider wraps the victim where they were caught, then drags them to its lair (false = the game's own order) |
| `StuckSeconds` | 8 | A spider standing still this long while carrying someone wraps (if needed) and hangs them where it stands |
| `ShowDeathTimer` | true | Show the death countdown on the victim's screen and next to the cut-down prompt |
| `StaysAfterChase` (`[Bracken]`) | true | A Bracken that loses its chase target goes back to stalking instead of retreating |
| `NoBlackoutOnBrackenKill` (`[Bracken]`) | true | No fade to black when a Bracken kills you (you still die and spectate normally) |
| `BrackenSteals` (`[Thieves]`) | true | A Bracken may carry a hung victim off to its lair |
| `LootBugSteals` (`[Thieves]`) | true | A loot bug may carry a hung victim off to its nest |
| `BrackenNoticeMeters` | 25 | How close a thief must be to a hung victim to notice them (shared by Bracken and loot bug) |
| `BrackenStealChancePercent` | 50 | Chance that a thief that notices a victim goes for them (rolled once per victim) |
| `BrackenStealDelaySeconds` | 5 | How long the victim must have been hanging before a thief may take them |
| `BrackenReachMeters` | 2 | How close the thief has to get to take the victim |
| `BrackenGiveUpSeconds` | 45 | A thief that has not reached the victim after this long gives up |
| `BrackenScaredMeters` | 6 | A thief will not go for a victim while another player is this close |
| `BrackenStealExtraSeconds` | 30 | Extra seconds added to the victim's death timer when they are stolen |
| `LootBugCarryHeight` | 2.2 | How high (metres) above itself a loot bug holds the cocoon |
| `LootBugRearUpDegrees` | 55 | How far a loot bug rears up while carrying (0 = stays on all fours, negative = tips the other way) |
| `DropHeldItems` | true | Victims drop what they are holding when caught |
| `LookSensitivity` | 0.12 | Mouse speed for the cocoon camera |
| `ChaseBoostEnabled` | true | Spider speeds up as it closes in, only with line of sight |
| `ChaseBoostMaxPercent` | 35 | Extra speed at point-blank range, in percent |
| `ChaseBoostFarMeters` / `ChaseBoostNearMeters` | 20 / 4 | No boost beyond the far distance, full boost inside the near distance |
| `ChaseBoostRampSeconds` | 2 | Seconds to build up to the full boost |
| `WebAlertEnabled` | true | Spider rushes to a web a player has stepped in |
| `WebAlertPercent` | 100 | Base extra speed during the web alert, in percent |
| `WebAlertFarBonusPercent` | 120 | Extra speed added when the spider is far from the web |
| `WebAlertFarMeters` | 20 | Distance at which the far bonus is at its maximum |
| `WebAlertSeconds` | 4 | Longest the web alert lasts (it also ends when the spider arrives) |
| `WaitingFadeEnabled` | true | A waiting spider dissolves out of sight (your screen only) |
| `WaitingDissolve` | true | Use the Barber's dissolve effect, otherwise a smooth fade |
| `WaitingOpacityPercent` | 10 | How much of the waiting spider stays visible |
| `WaitingFadeSeconds` | 2 | How long the dissolve takes |
| `WaitingRevealMeters` | 1 | A player closer than this sees the spider normally |
| `HangHeightAboveGround` | 1.6 | How high above the floor a victim hangs, in metres (0 = the game's own height). Tight spaces use the game's own height |
| `WebCountMultiplier` | 2 | How many webs a spider places (1 = the game's normal 6 to 8) |
| `WebPlaceRateMultiplier` | 2 | How often a patrolling spider places webs |
| `PatrolRadiusMultiplier` | 2 | How far from its start the spider roams while placing webs |
| `EnableDebugSpawnKey` | false | Testing only: host presses the spawn key (default F6) to spawn a spider (F7 logs nearby labels) |

## Emergency release

If anything ever goes wrong, press **F9** to free yourself. This frees you for everyone in the game.

## Reporting bugs

Found a problem? Please [open an issue on GitHub](https://github.com/Militus1/Hung_to_Dry/issues/new/choose) and include your log file.

1. Play until the problem happens, then close the game.
2. In r2modman go to **Settings, then Browse profile folder**, and open `BepInEx/LogOutput.log`.
3. Drag that file into the issue, or paste it at [mclo.gs](https://mclo.gs) and share the link. Do not paste it into chat.

The log contains your player name and some file paths, but no passwords. Say which mod version you have and whether you were the host or a client, because the host's game decides how the spider behaves.

## Known quirks

- Mods that make you untargetable (for example debug tools) will stop the spider catching you, as you'd expect.
- If your game log shows "Tried to wrap body but it could not be found", that is the game's own message and is harmless.

## Credits

Made by Militus.
