# Hung to Dry: all settings

The settings file is `BepInEx/config/Militus.HungToDry.cfg`. It appears after the first time you run the game with the mod. Everyone in the lobby should use the same settings (they are not synced from the host).

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
| `VenomSeconds` (`[Venom]`) | 8 | How long a bitten player stumbles about before collapsing and being wrapped (0 = wrapped instantly, as before). Everyone should use the same value |
| `VenomHeartbeat` (`[Venom]`) | true | A heartbeat sound that speeds up as the venom takes hold (only the bitten player hears it) |
| `VenomHandoverMeters` (`[Venom]`) | 25 | If the spider dies while the venom is in a player and another spider is within this many metres, that spider takes over and wraps them (0 = off: they collapse and recover) |
| `BiteCooldownSeconds` (`[Venom]`) | 8 | After a spider bites someone it will not bite anyone else for this long (0 = off) |
| `ImmunityEndsOnAttack` | true | A freed player who hits a monster loses their immunity straight away |
| `SuffocationDeath` | true | The cause of death shown on a victim's body is Suffocation instead of Mauling |
| `SpiderFootstepRange` | 1 | How far away spider footsteps can be heard, as a fraction of normal (1 = unchanged, 0.5 = half). Only changes what you hear |
| `EmergencyReleaseKey` | false | Press F9 to free yourself instantly (see below) |
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
| `LootBugYield` | true | A loot bug backs off when a Bracken is going for a cocoon nearby |
| `LootBugYieldMeters` | 15 | How close a Bracken has to be (to the cocoon or the bug) for the loot bug to back off |
| `BrackenAmbush` | true | A Bracken hides out of sight until the spider has hung the cocoon and gone invisible. Off = it walks straight to the victim |
| `BrackenAmbushMinMeters` / `BrackenAmbushMaxMeters` | 8 / 16 | How far from the victim the Bracken looks for a hiding place |
| `BrackenAmbushMaxWaitSeconds` | 90 | Longest the Bracken hides before giving up |
| `BrackenAmbushPatienceSeconds` | 12 | If the cocoon has been hung this long and the spider is still about, the Bracken comes out anyway |
| `BrackenCarryStyle` | Shoulder | How the Bracken carries the cocoon: `Shoulder`, `Drag` (trails behind) or `Hand` (the game's own carry) |
| `BrackenCarryHeight` / `BrackenCarrySide` / `BrackenCarryBack` | 2.7 / 0.75 / -0.1 | Shoulder style: where the cocoon is held, in metres (height, to the right, behind) |
| `BrackenCarryPitch` / `BrackenCarryYaw` / `BrackenCarryRoll` | 90 / 0 / 0 | Shoulder style: the angle the cocoon is held at, in degrees |
| `BrackenDragDistance` / `BrackenDragHeight` | 1.2 / 0.9 | Drag style: how far behind and how high above the floor the cocoon trails |
| `BrackenDragNormalPose` | true | The Bracken walks in its normal pose while carrying instead of the game's arms-out pose |
| `BrackenArmOver` | true | The Bracken's arm is draped over the cocoon while carrying |
| `BrackenArmUp` / `BrackenArmIn` / `BrackenArmForward` | -0.2 / 0.25 / 0 | Where the hand rests relative to the cocoon's carry point, in metres |
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
| `EnableDebugSpawnKey` | false | Testing only: lets the host press F6 to spawn a spider, F8 a Bracken, F10 a loot bug, F11 to remove every enemy (F4 and F7 write debug lists to the log) |
| `DebugLogging` (`[Testing]`) | false | Writes extra, repetitive detail to the log. Leave off unless you are chasing a problem |

## Added in 1.9.0

Section names in brackets are the headings inside the config file. Settings starting with "Advanced." in the file are hidden from the in-game LethalConfig menu unless `ShowAdvancedInMenu` is on.

### Rescue (`[Rescue]`)

| Setting | Default | What it does |
|---|---|---|
| `VenomedCutMultiplier` | 3 | While you are venomed (bitten, but not yet wrapped) cutting a friend down takes this many times longer, as your muscles stop obeying you. 1 = no change. |
| `KnifeCutMultiplier` | 0.5 | Cutting a friend down with a knife in your hand takes this share of the normal time (0.5 = half as long). 1 = a knife makes no difference. Does not apply to digging someone out. |

### Venom (`[Venom]`)

| Setting | Default | What it does |
|---|---|---|
| `VenomReachMeters` | 15 | When the venom runs out and you collapse, the spider only comes for you if it can actually walk to you: there must be a walkable route, and it must be no longer than this many metres. Otherwise you wake up where you fell (no wrap) and the spider goes back to patrolling. 0 = no limit on the length. |
| `VenomSpiderSlowPercent` | 50 | After a bite, while the venom is working on its victim, the spider moves this much slower (50 = half speed), giving the victim a fair chance to run. It returns to normal speed when the victim collapses. 0 = no slowdown. |
| `VenomSwaySeconds` | 3 | In the last few seconds of the venom the bitten player is forced to sway: the view drifts left, right, up and down, building up until they collapse. (The forced sideways stagger of the body was removed.) 0 = off. |
| `VenomSwayStartsAfterSeconds` | 4 | The sway begins this many seconds after the bite and builds until the collapse. -1 = use VenomSwaySeconds instead (the last few seconds only). |
| `VenomSwayStrength` | 1 | How hard the forced sway pushes (1 = default). |
| `VenomHeartbeatVolume` | 3 | How loud the venom heartbeat is. 1 = the game's own heartbeat volume (quiet), 3 = loud (default), up to 6. It still starts softer and builds up as the venom takes hold. |
| `SpiderAvoidsWeapons` | true | While a bitten player is still stumbling, if they are holding a weapon (shovel, knife, stop sign, shotgun...), the spider backs off and stays out of sight until the venom takes them. Needs VenomSeconds above 0. |
| `SpiderAvoidMeters` | 10 | How far the spider tries to stay from an armed, bitten player. |

### Defend (fighting back) (`[Defend]`)

| Setting | Default | What it does |
|---|---|---|
| `SpiderDropsWhenHit` | true | Hit a spider that is carrying a wrapped player and it drops them to go after you. The victim must then struggle free (or be cut down). |
| `StruggleKey` | E | Mash this key to struggle free of a cocoon the spider has dropped. |
| `StrugglePresses` | 14 | How many presses it takes to break free if you never stop. |
| `StruggleDecayPerSecond` | 0.12 | Share of the struggle bar that drains each second you are not pressing (0 = never drains). |
| `DropChaseSeconds` | 10 | Seconds the spider hunts the attacker before it goes back to pick the cocoon up again. |

### Spawning (`[Spawning]`)

| Setting | Default | What it does |
|---|---|---|
| `OutdoorSpawnRarity` | 20 | How often bunker spiders are picked as outdoor monsters, compared with the other outdoor monsters (the game's own outdoor monsters use numbers from about 5 to 30). 0 = spiders only spawn inside, as in the normal game. Only the host's setting matters. |

### Trapdoor ambush (outdoors) (`[Trapdoor]`)

| Setting | Default | What it does |
|---|---|---|
| `TrapdoorSpiders` | true | A spider that lives outdoors (for now only the test spawn key makes one) picks a random node, walks to it, sinks under the ground and waits to ambush. |
| `TrapdoorDepth` | 2.2 | How far under the ground (metres) the spider sinks. |
| `TrapdoorTriggerMeters` | 6 | A player this close to a buried spider makes it burst out and attack. |
| `TrapdoorRelocateSeconds` | 90 | If nobody comes near for this long, the spider climbs out and buries itself somewhere else. |
| `TrapdoorHoldSeconds` | 20 | A victim the spider has dragged underground is let go after this many seconds (for testing). The death timer still runs, so keep this below HangSeconds. |
| `DigOutSeconds` | 6 | How many seconds a teammate must hold the rescue key at the disturbed earth to dig a buried victim out. The spider comes back up to defend them. |
| `TrapdoorBodyNoCollide` | true | While the spider drags a victim underground, switch off every collider and collision on the stand-in body so the ground cannot stop it. The log (lines starting 'Cocoon dump') shows what was blocking it either way. |

### Tree ambush (outdoors) (`[Tree]`)

| Setting | Default | What it does |
|---|---|---|
| `TreeSpiders` | true | A spider that lives outdoors (for now only the test spawn key makes one) walks to a tree, climbs up under its branches and drops on a player who walks beneath. Spiders that are not chosen for the trees bury themselves instead (see the Trapdoor section). |
| `TreeChance` | 100 | Percent of outdoor spiders that wait in trees (the rest use the trapdoor ambush). Decided once per spider, the same on every player's game. |
| `TreeHeight` | 16 | How high up the trunk (metres) the spider climbs before it waits for someone to pass beneath. |
| `TreeGap` | -0.7 | How far (metres) the spider's feet sit from the trunk's solid surface once it is up. The solid part is wider than the trunk you can see further up, so a negative number pulls it closer to the visible trunk. Lower = closer. |
| `TreeLegSpread` | 100 | How far the feet spread out sideways while the spider clings to a trunk, as a percentage of normal. 100 = normal, lower pulls the feet in towards the body (a tighter grip), higher spreads them wider. |
| `TreeHangOut` | 1.2 | How far out from the trunk (metres) a hung victim's rope hangs. Too small and the cocoon swings into the bark. |

### Stalking and the web pull (`[Stalk]`)

| Setting | Default | What it does |
|---|---|---|
| `TreeStalk` | true | A spider waiting in a tree that sees a player nearby (who is not looking at it) drops down and stalks them like the Feiopar: it freezes whenever anyone looks at it, creeps closer when nobody does, then charges. Off = it only drops on someone directly beneath it. |
| `StalkTriggerMeters` | 25 | A player this close (sideways) to the tree, and not looking at the spider, makes it drop and begin stalking. |
| `StalkFootstepPercent` | 5 | How loud (percent of normal) the spider's footsteps are while it stalks. Only on the host's game for now. |
| `IndoorStalkChance` | 50 | Percent of spiders inside the facility that roam and stalk players (freeze when watched, creep when not, then charge) instead of waiting on the ceiling. Decided once per spider, the same on every player's game. Needs TreeStalk on. |
| `IndoorStalkTriggerMeters` | 15 | Inside the facility, a player this close (and not looking at the spider) starts the stalk. |
| `TreeCloak` | true | A spider waiting in a tree or stalking dissolves (down to the Spider WaitingOpacityPercent) as soon as a player starts turning towards it, and becomes solid again once nobody is looking at it. |
| `TreeCloakOpacityPercent` | 2 | How visible (percent) a stalking or tree-waiting spider is while it is dissolved. 0-2 = almost invisible, 10 = the same as the normal waiting fade. |
| `WebPull` | true | The end of a stalk: instead of charging from afar, the spider shoots a web line at you and reels you in (like the fox's tongue), then bites. Break the line of sight (get behind a wall) or hit the spider to get free. Off = it just charges. |
| `WebOriginForward` | 0 | How far (metres) from the spider's abdomen, towards you, the web line starts. 0 = right at the abdomen (the default). Negative = further back, positive = towards the head. |
| `WebOriginUp` | 0.1 | How high (metres) above the abdomen the web line starts. Negative = lower (the underside of the abdomen, the default). |
| `WebRange` | 8 | A stalking spider this close (and with a clear line to you, and nobody looking at it) shoots its web. |
| `WebBreakMeters` | 12 | The web line snaps if you get this far (metres) from the spider while it is pulling you. It shoots from at most WebRange, so this is how far you can stretch it. Lower = easier to escape. |
| `WebRampSeconds` | 4 | How many seconds the web takes to build from a light tug up to its full pull. A longer ramp gives you more time to fight it. |
| `WebPeakMultiplier` | 5 | How much stronger the pull is at its peak than at the start (the start is 0.8 times WebStrength's base pull). Raise it if sprinting away can still beat the web. |
| `WebStrength` | 1 | How hard the web line pulls you, as a multiple of the fox tongue's pull. 1 = the same as the fox; lower is easier to resist. |
| `TreeCloakStepLead` | 0.4 | How far ahead (seconds) the spider looks along its own path. If it is about to step into your view (round a corner, out from behind something), it dissolves before it gets there. 0 = off. |
| `TreeCloakLead` | 0.25 | How far ahead (seconds) the spider predicts where your view is heading. If you are turning towards it, it dissolves this much earlier. 0 = no prediction. |
| `TreeCloakAngle` | 90 | How close to the centre of a player's view (degrees) the spider must be for it to dissolve. The spider freezes at 67, so a bigger number (the default is 90) makes it dissolve before it freezes, so it dissolves as soon as you start turning towards it. Smaller = it only dissolves when you look almost straight at it. |
| `StalkDirectChance` | 70 | Percent of outdoor spiders that roam and stalk players directly, with no tree. The rest wait in trees first (TreeChance). Decided once per spider, the same on every player's game. |

### Ceiling ambush (indoors) (`[Ceiling]`)

| Setting | Default | What it does |
|---|---|---|
| `CeilingSpiders` | true | A spider inside the facility climbs up to the ceiling, turns invisible, and drops on a player who walks underneath. |
| `CeilingChance` | 100 | Percent chance that a spider goes to the ceiling to ambush (rolled each time it picks a new spot). The rest of the time it behaves as normal. |
| `CeilingGap` | 0.15 | The gap (metres) between the spider's highest foot and the ceiling once it is in place. 0 = touching. |
| `CeilingTriggerMeters` | 2.8 | A player this close (sideways) beneath the spider makes it drop. |
| `CeilingRelocateSeconds` | 90 | If nobody comes underneath for this long, the spider comes down and picks another spot. |
| `CeilingCloak` | false | On: the spider dissolves like the waiting spider (down to the Spider WaitingOpacityPercent) as it reaches the ceiling, but is never fully invisible. Off: it stays fully solid up there. |
| `CeilingRiseSpeed` | 3 | How fast (metres per second) the spider climbs up to the ceiling. |

### Webs (`[Webs]`)

| Setting | Default | What it does |
|---|---|---|
| `WebMinSpacing` | 3 | A spider will not lay a new trap web closer than this many metres to one it has already laid (measured between the middles of the strands, and between their ends). Applies to the game's own floor webs and to ceiling webs. 0 = off. |
| `CeilingWebChance` | 40 | Percent chance that, each time a patrolling spider lays a web, it ALSO hangs a strand from the ceiling down to the floor a few metres away (across where you walk). Only spiders indoors with a ceiling above them do this. 0 = off, the game's own webs only. |

### Cocoon (`[Cocoon]`)

| Setting | Default | What it does |
|---|---|---|
| `WrapExtraRolls` | 2 | How many extra full rolls the victim does while being wrapped. The game's own wrap animation only rolls them for about the first 2 of its 4 seconds, then they hang still. The extra rolls fill the rest of the wrap. 0 = the game's own behaviour. |
| `MuffleVoices` | true | While someone is wrapped in a cocoon, their voice chat sounds muffled to everyone else, the same way the centipede muffles its victim. |
| `MuffleCutoffHz` | 500 | How muffled a wrapped player's voice is: sounds above this pitch are cut off. Lower is more muffled. The snare flea uses 500. |
| `MuffleResonance` | 5 | How much the muffled voice rings at the cut-off pitch. High values can sound like an echo; 1 is flat. The snare flea uses 5. |

### Spider (`[Spider]`)

| Setting | Default | What it does |
|---|---|---|
| `SmoothJoinerSpider` | true | On a joining player's game, the spider's body is moved smoothly towards the positions the host sends, instead of jumping from one to the next (the jumps made its legs jitter). Host unaffected. |

### General (`[General]`)

| Setting | Default | What it does |
|---|---|---|
| `ShowAdvancedInMenu` | false | The in-game LethalConfig menu hides the advanced and testing settings so it stays short. Set this to true to show every setting there (restart the game to apply). Every setting is always in this file either way. |

