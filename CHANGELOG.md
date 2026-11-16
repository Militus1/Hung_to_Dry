# Changelog

## 1.8.1
- **Fix:** a player who dies in a cocoon now leaves an ordinary body that teammates can pick up and carry back, instead of an unpickable one.
- **Fix:** the Bracken no longer runs back and forth with a cocoon. It carries it to its lair and puts it down there; nobody steals it again afterwards. Rescue time is no longer wasted.
- **Fix:** snare fleas and thumpers now ignore a wrapped player, including one being carried by a Bracken or loot bug.
- New experimental setting `SpiderFootstepVolume` (default 1 = unchanged). It may not take effect on every spider.
- Extra diagnostics appear in the log only when `DebugLogging` is on.

## 1.8.0
- **Pecking order:** only one thief goes for a cocoon at a time, and the Bracken outranks the loot bug. A loot bug that sees a Bracken heading for the cocoon backs off and lets it through (`LootBugYield`, `LootBugYieldMeters`).
- **Bracken ambush:** a Bracken that notices a victim being hung now hides out of sight and waits. It only comes out once the spider has hung the cocoon and gone invisible (`BrackenAmbush`, `BrackenAmbushMinMeters`, `BrackenAmbushMaxMeters`, `BrackenAmbushMaxWaitSeconds`). If the spider is still about after `BrackenAmbushPatienceSeconds` (12 s), it comes out anyway, so it never hides until the victim dies.
- A Bracken can take a cocoon a loot bug has dropped at its nest. A loot bug will never take a cocoon a thief has already carried.
- **New carry style:** the Bracken now carries the cocoon over its shoulder, with its arm draped over it (`BrackenCarryStyle`: Shoulder, Drag or Hand; position, angle and arm settings in `[Thieves]`). Set `BrackenArmOver` to false to keep the arm in its normal pose.
- **Fix:** if something blocks the death (for example Imperium's god mode), the player is now freed from the cocoon with the camera restored, instead of being left able to move but unable to see.
- The log is much quieter. Repetitive detail is hidden unless `DebugLogging` (`[Testing]`) is on.
- Testing keys for developers (all off unless `EnableDebugSpawnKey` is true): F6 spider, F8 Bracken, F10 loot bug, F11 remove every enemy, F4 Bracken bone list, F7 label list.
- New settings are added to your config automatically. Existing values are kept. Delete your old config file if you want the new carry defaults.

## 1.7.0
- **Thieves:** a Bracken or a loot bug that notices a hung victim can now cut the cocoon down and carry it off. The Bracken takes it to its lair, the loot bug holds it up over its head and carries it to its nest. Each waits a few seconds first so the spider can finish hanging the victim.
- A stolen victim gets 30 extra seconds before dying (`BrackenStealExtraSeconds`), so the team has time to find them.
- Wrapped players can no longer be targeted or killed by any monster, so a Bracken no longer strangles the invisible body left where they were caught.
- The hidden body of a wrapped player now follows the cocoon, landing on the nearest AI node, which is always safe ground. A rescued player appears at the cocoon.
- Spider webs no longer catch or slow a wrapped player's hidden body, and a player caught in a web just before being wrapped is freed from it.
- After a rescue (or F9), no monster can target the freed player for `ImmunitySeconds` (10 s by default), not just the spider.
- A countdown ("Time left") now shows on the wrapped player's screen, and next to the cut-down prompt for rescuers (`ShowDeathTimer`).
- A Bracken that loses its chase target now stays around and stalks again instead of walking away (`StaysAfterChase`).
- No black fade-out screen when a Bracken kills you (`NoBlackoutOnBrackenKill`).
- New settings: `[Thieves]` (`BrackenSteals`, `LootBugSteals`, notice range, chance, delay, reach, give-up time, scared distance, extra life, loot bug carry height and rear-up angle), `[Bracken]` `StaysAfterChase` and `NoBlackoutOnBrackenKill`, `[General]` `ShowDeathTimer`. Delete your old config file to pick up the new defaults.

## 1.6.7
- The spider now wraps a victim in silk right where they are caught, then drags them off, instead of dragging first.
- The stall safety net wraps a victim before hanging them, and triggers after 8 seconds instead of 12.
- New setting `WrapOnCatch`. Delete your old config file to pick up the new defaults.

## 1.6.6
- Chase boost: the spider speeds up as it closes in, only while it has line of sight. Acceleration and turning scale with the boost so it no longer loses speed at corners.
- Web alert: stepping in a web sends the spider sprinting to it, faster the further away it is.
- Waiting camouflage: a motionless spider dissolves using the Barber's dissolve effect (your screen only).
- Victims hang at a set height above the floor (tight spaces use the game's own height), so teammates can always reach them.
- Spiders place more webs, more often, over a wider patrol area.
- Safety nets for spiders that cannot find a hanging spot: carry timeout and a wall-reach tolerance.
- Debug spawn key default moved to F6 (still off unless enabled).
- Bug reports: see the README for how to send your log.

## 1.0.0
- First release: paralyse, wrap, drag and hang; cut-down rescue; multiplayer sync; spider camera; F9 emergency release.
