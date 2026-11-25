# Changelog

## 1.10.0
The trapdoor spider is rebuilt from the ground up, and a death in the cocoon now has consequences. Everyone in the lobby needs 1.10.0. Some settings were renamed, so check SETTINGS.md if you had changed them (your old values for those are not carried over).

**Trapdoor spider**
- **Trip-lines:** the spider now waits under a lid with silk trip-lines around it. Step within reach (`TrapdoorTripLineMeters`, 3 m) and it grabs you from below.
- **Tug of war:** once it has you, mash the struggle key (E by default) to loosen its grip before it drags you under. A teammate who hits the spider makes it let go. The grip can be tuned (`TrapdoorGripStart`, `TrapdoorGripRate`, `TrapdoorMashStep`).
- **Buried for good:** a dragged-under victim is no longer let go after a while. They stay buried until a teammate digs them out or the timer runs out (`TrapdoorReleaseSeconds`, 0 = never).
- **A real burrow:** a lid that blends with the ground, an earth rim, and a silk-lined tunnel with roots below (`TrapdoorBurrowModel`). Burrows are dug on open, lit ground rather than in corners.
- **The view from below:** while buried you look in at your cocoon and the spider through a gap in the tunnel wall, in a faint light, and you can no longer see through the ground to the rest of the map (`UndergroundViewMeters`, `UndergroundLightPercent`).
- **Weather:** no trapdoor spiders in rain, storms or floods; those spiders stalk instead (`TrapdoorNoSpawnWeather`).
- **Fix:** the spider no longer climbs all the way out of its burrow after losing its grip on someone.

**Outdoor spiders**
- Outdoor spiders are now a mix: about half stalk you directly, a third wait in trees and the rest are trapdoor spiders (`StalkDirectPercent`, `TreeWaitPercent`).
- On moons without trees, a spider that would have waited in a tree digs a burrow instead.
- **Tree spider:** it starts stalking from closer (`TreeStalkMeters`, 12 m), stays faintly visible when it fades (`TreeCloakPercent`), and its victims now hang at the right height instead of high up in the branches.
- **Web pull:** a stalking spider now shoots its web when you try to get away from it, instead of at a fixed distance (`WebEscapeMeters`, `WebMaxMeters`).
- **Caught in the web:** when the web reels you in, the spider grabs you and bites at once. Mash the struggle key to break its grip, but its venom makes that harder (`StalkGrab`, `StalkGrabVenomMashPercent`). Lose and it wraps you on the spot. Break free and it leaves you alone for a few seconds (`StalkGrabCooldownSeconds`), but the venom is still in you: get well away before it wears off.
- **No giving up:** if the web line snaps, or you break it by ducking behind something, the spider stops reeling and comes after you.

**Ceiling spider**
- It fades out once it reaches the ceiling by default (`CeilingCloakOn`).
- **Fix:** it could bite someone while walking to its spot and then leave the victim lying there. It now carries them off.

**Cocoons and masks**
- **Dead stay hanging:** when the timer runs out the victim dies, but the cocoon stays up. The body only drops once a teammate cuts it open (`DeadStayHanging`).
- **Haunted masks:** a player wrapped while a mask is taking them over is not saved by the cocoon. The first time someone cuts it open, a Masked that looks like them bursts out (`MaskTrap`).

**Other fixes**
- Spiders spawned close together no longer tend to all pick the same role.
- Burrows and buried cocoons are cleared away at the end of a round.
- Quicksand and water slow you down again right after you are cut free (they used to have no effect on you for a few minutes).
- A spider on the ceiling no longer flies off into the sky when the whole crew is dead.
- Cutting open a dead teammate's cocoon no longer briefly muffles their voice again.

If you hit a bug, please [open an issue on GitHub](https://github.com/Militus1/Hung_to_Dry/issues/new/choose) with your log (`BepInEx/LogOutput.log`) and say whether you were the host or a client.

## 1.9.0
This is a big update. Spiders are no longer just something you meet indoors, and rescuing a friend now takes thought. Everything below is on by default and can be turned off or tuned in the config (see SETTINGS.md). Everyone in the lobby needs 1.9.0.

**New ways to be caught**
- **Outdoor spiders:** bunker spiders can now turn up outside the facility (`OutdoorSpawnRarity`). They either ambush from under the ground (trapdoor) or from a tree, or simply stalk you.
- **Trapdoor ambush:** an outdoor spider sinks under the ground and waits. Walk near it and it bursts out. A victim dragged underground can be dug out by a teammate holding the rescue key (`DigOutSeconds`).
- **Tree ambush:** an outdoor spider climbs a tree and waits under the branches, then drops on a player who walks beneath it.
- **Ceiling ambush:** inside the facility a spider may climb to the ceiling, fade out of sight and drop on a player walking underneath (`CeilingSpiders`).
- **Stalking:** some spiders follow you. They freeze when you look at them, creep closer when you do not, then charge. A stalking spider may also shoot a web line and reel you in like the fox's tongue (`WebPull`).
- **Fading spiders:** a waiting or stalking spider dissolves out of sight as you turn towards it.
- **Ceiling strands:** a spider may hang a web strand from the ceiling to the floor across a corridor (`CeilingWebChance`). Webs are also kept apart from each other (`WebMinSpacing`).

**Venom and rescue**
- **Venom slows the rescuer:** if you are venomed yourself, cutting a friend down takes longer (`VenomedCutMultiplier`, 3x by default), so a bitten player cannot just free everyone before they collapse.
- **Knife speeds up the rescue:** holding a knife while cutting a friend down takes half the time (`KnifeCutMultiplier`). The prompt shows "(knife)" or "(venom slows you)".
- **Fighting back:** hit a spider that is carrying a wrapped player and it drops them to come after you. The victim then mashes a key (E by default) to struggle free (`SpiderDropsWhenHit`, `StruggleKey`).
- **Armed players:** a spider that has bitten a player holding a weapon backs off and stays out of sight until the venom has run its course (`SpiderAvoidsWeapons`).
- **Venom changes:** the spider only fetches a collapsed victim if it can actually walk to them (`VenomReachMeters`), moves slower while the venom works (`VenomSpiderSlowPercent`), and the sway before the collapse is adjustable (`VenomSway...`, `VenomHeartbeatVolume`).
- **Wrapped players sound muffled** to everyone else (`MuffleVoices`, `MuffleCutoffHz`, `MuffleResonance`).
- **Wrapping looks better:** the victim gets extra rolls while the silk goes on (`WrapExtraRolls`).

**Multiplayer fixes**
- **Cocoons** now hang in the same place on every player's game, and two cocoons no longer clip into each other.
- **The spider's legs** move smoothly on joining players' games instead of jumping (`SmoothJoinerSpider`).
- **A web's stuck-sound** no longer keeps looping after the player has left the web.
- **Fix:** a player who was dropped outdoors by the spider could die unexpectedly.
- **Fix:** the red venom screen could stay on after the venom had ended.
- **Web pull:** the web line now snaps when you break line of sight, for example by going round a corner or through a door.

This is a big update with a lot of new behaviour, so some rough edges are likely. If you hit a bug, please [open an issue on GitHub](https://github.com/Militus1/Hung_to_Dry/issues/new/choose) with your log (`BepInEx/LogOutput.log`) and say whether you were the host or a client.

## 1.8.2
- **Venom (new, on by default):** a bite no longer wraps you instantly. For `VenomSeconds` (8 s by default) you stumble about slowed and dizzy, with your screen warping and a heartbeat that speeds up (`VenomHeartbeat`). Then you collapse where you stand and the spider walks over, wraps you and hangs you as before. Set `VenomSeconds` to 0 for the old instant wrap. Everyone in the lobby should use the same value. This is new and has had limited multiplayer testing, so reports are welcome.
- **If the spider dies while the venom is in you,** you still collapse, lie there a moment and recover. If another spider is within `VenomHandoverMeters` (25 m by default, 0 = off), it takes over and wraps you instead.
- **Leaving the facility** while venomed means the spider cannot follow: you collapse and recover instead of being dropped outside.
- **Bite cooldown (`BiteCooldownSeconds`, 8 s by default):** a spider that has just bitten someone will not bite anyone else for a few seconds, so a group is not caught in one pass.
- **Freed players** lose their immunity the moment they attack a monster (`ImmunityEndsOnAttack`), so a rescue cannot be used as a free window to fight.
- **Cause of death** on a victim who dies in a cocoon is now Suffocation (`SuffocationDeath`), which is what the scanner shows on the body.
- **Fix:** being freed while using the door no longer leaves you stranded on the other side of it.
- **Fix:** a player who gets stuck slowed after leaving a web is now freed from the slowdown automatically.
- **Emergency release (F9) is now off by default** (`EmergencyReleaseKey`); turn it on in the config if you want it. A stuck cocoon is still freed automatically after `VictimHoldSeconds`.
- **New setting `SpiderFootstepRange`** (default 1 = unchanged): how far away the spider's footsteps can be heard. The experimental `SpiderFootstepVolume` from 1.8.1 is removed because it did not work reliably.
- Smaller things: the log is tidier, and two minor performance improvements.
- New settings are added to your config automatically; existing values are kept. Delete your old config file if you want the new defaults.

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
