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
