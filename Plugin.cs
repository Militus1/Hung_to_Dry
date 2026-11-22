using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace SpiderCocoon
{
    [BepInPlugin("Militus.HungToDry", "Hung to Dry", "1.9.0")]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static ConfigFile Cfg;

        // Repetitive diagnostic detail: only written when DebugLogging is on.
        internal static void Verbose(string message)
        {
            if (Log != null && VerboseLogs != null && VerboseLogs.Value) Log.LogInfo(message);
        }

        internal static void Dbg(string message)
        {
            if (Log != null && DebugLogging != null && DebugLogging.Value) Log.LogInfo(message);
        }

        // All of these can be edited in BepInEx\config\Militus.HungToDry.cfg
        internal static ConfigEntry<float> HoldSeconds;      // safety net: release if the spider never hangs the body
        internal static ConfigEntry<float> HangSeconds;      // time between being hung up and dying
        internal static ConfigEntry<bool> ImmunityEndsOnAttack;  // hitting a monster while immune ends the immunity
        internal static ConfigEntry<float> ImmunitySeconds;  // only used if a victim is released
        internal static ConfigEntry<UnityEngine.InputSystem.Key> CutDownKey;   // hold this near a hung cocoon to rescue
        internal static ConfigEntry<bool> BrackenStaysAfterChase;
        internal static ConfigEntry<float> LootBugCarryHeight;
        internal static ConfigEntry<float> LootBugRearUpDegrees;
        internal static ConfigEntry<bool> LootBugSteals;
        internal static ConfigEntry<bool> BrackenSteals;                       // a Bracken may carry off a hung victim
        internal static ConfigEntry<float> BrackenNoticeMeters;
        internal static ConfigEntry<float> BrackenStealChancePercent;
        internal static ConfigEntry<float> BrackenStealDelaySeconds;
        internal static ConfigEntry<float> BrackenReachMeters;
        internal static ConfigEntry<float> BrackenGiveUpSeconds;
        internal static ConfigEntry<float> BrackenStealExtraSeconds;
        internal static ConfigEntry<bool> NoBlackoutOnBrackenKill;
        internal static ConfigEntry<bool> ShowDeathTimer;
        internal static ConfigEntry<float> BrackenScaredMeters;
        internal static ConfigEntry<string> BrackenCarryStyle;
        internal static ConfigEntry<float> BrackenCarryHeight;
        internal static ConfigEntry<float> BrackenCarryBack;
        internal static ConfigEntry<float> BrackenCarrySide;
        internal static ConfigEntry<float> BrackenCarryPitch;
        internal static ConfigEntry<float> BrackenCarryYaw;
        internal static ConfigEntry<float> BrackenCarryRoll;
        internal static ConfigEntry<float> BrackenDragDistance;
        internal static ConfigEntry<float> BrackenDragHeight;
        internal static ConfigEntry<bool> BrackenDragNormalPose;
        internal static ConfigEntry<bool> BrackenArmOver;
        internal static ConfigEntry<float> BrackenArmUp;
        internal static ConfigEntry<float> BrackenArmIn;
        internal static ConfigEntry<float> BrackenArmForward;
        internal static ConfigEntry<bool> BrackenAmbush;
        internal static ConfigEntry<float> BrackenAmbushMinMeters;
        internal static ConfigEntry<float> BrackenAmbushMaxMeters;
        internal static ConfigEntry<float> BrackenAmbushMaxWaitSeconds;
        internal static ConfigEntry<float> BrackenAmbushPatienceSeconds;
        internal static ConfigEntry<bool> LootBugYield;
        internal static ConfigEntry<float> LootBugYieldMeters;
        internal static ConfigEntry<bool> EmergencyReleaseKey;                // F9 frees yourself instantly (off by default: it skips the whole mechanic)
        internal static ConfigEntry<bool> SuffocationDeath;                    // cause of death shown on scanner/other mods: Suffocation instead of Mauling
        internal static ConfigEntry<bool> WrapOnCatch;                         // wrap the victim where they are caught, then drag
        internal static ConfigEntry<float> StuckSeconds;                        // spider standing still this long while carrying = stuck
        internal static ConfigEntry<float> CarryTimeoutSeconds;                // spider carrying this long without hanging = hang where it is
        internal static ConfigEntry<float> WallReachMeters;                    // how close to its wall spot the spider must get before it hangs a victim
        internal static ConfigEntry<bool> SpiderDropsWhenHit;                  // hit a spider that is carrying a victim and it drops them to fight
        internal static ConfigEntry<UnityEngine.InputSystem.Key> StruggleKey;  // mash this to wriggle free of a dropped cocoon
        internal static ConfigEntry<int> StrugglePresses;                      // presses needed to break free (with no rest)
        internal static ConfigEntry<float> StruggleDecayPerSecond;             // share of the bar lost per second when not pressing
        internal static ConfigEntry<float> DropChaseSeconds;
        internal static ConfigEntry<bool> TrapdoorEnabled;       // outdoor spiders hide under the ground and ambush
        internal static ConfigEntry<float> TrapdoorDepth;
        internal static ConfigEntry<float> TrapdoorTriggerMeters;
        internal static ConfigEntry<float> TrapdoorRelocateSeconds;
        internal static ConfigEntry<float> TrapdoorHoldSeconds;
        internal static ConfigEntry<float> DigOutSeconds;
        internal static ConfigEntry<bool> CeilingEnabled;
        internal static ConfigEntry<bool> TreeEnabled;
        internal static ConfigEntry<float> TreeChance;
        internal static ConfigEntry<float> TreeHeight;
        internal static ConfigEntry<float> TreeGap;
        internal static ConfigEntry<float> TreeHangOut;
        internal static ConfigEntry<bool> TreeStalk;
        internal static ConfigEntry<float> StalkTriggerMeters;
        internal static ConfigEntry<float> StalkFootstepPercent;
        internal static ConfigEntry<bool> TreeCloak;
        internal static ConfigEntry<bool> MuffleVoices;
        internal static ConfigEntry<bool> SmoothJoinerSpider;
        internal static ConfigEntry<float> MuffleCutoff;
        internal static ConfigEntry<float> MuffleResonance;
        internal static ConfigEntry<bool> ShowAdvancedInMenu;
        internal static ConfigEntry<float> CeilingWebChance;
        internal static ConfigEntry<float> WebMinSpacing;
        internal static ConfigEntry<float> WrapExtraRolls;
        internal static ConfigEntry<bool> VerboseLogs;
        internal static ConfigEntry<bool> LogAllSounds;
        internal static ConfigEntry<float> TreeCloakAngle;
        internal static ConfigEntry<float> TreeCloakOpacityPercent;
        internal static ConfigEntry<float> TreeCloakLead;
        internal static ConfigEntry<float> TreeCloakStepLead;
        internal static ConfigEntry<bool> WebPull;
        internal static ConfigEntry<bool> WebEvenIfWatched;
        internal static ConfigEntry<float> WebOriginForward;
        internal static ConfigEntry<float> WebOriginUp;
        internal static ConfigEntry<float> WebRange;
        internal static ConfigEntry<float> WebStrength;
        internal static ConfigEntry<float> WebRampSeconds;
        internal static ConfigEntry<float> WebBreakMeters;
        internal static ConfigEntry<float> WebPeakMultiplier;
        internal static ConfigEntry<float> StalkDirectChance;
        internal static ConfigEntry<float> IndoorStalkChance;
        internal static ConfigEntry<float> IndoorStalkTriggerMeters;
        internal static ConfigEntry<float> OutdoorSpawnRarity;
        internal static ConfigEntry<float> TreeLegSpread;
        internal static ConfigEntry<float> CeilingChance;
        internal static ConfigEntry<float> CeilingGap;
        internal static ConfigEntry<float> CeilingTriggerMeters;
        internal static ConfigEntry<float> CeilingRelocateSeconds;
        internal static ConfigEntry<float> CeilingRiseSpeed;
        internal static ConfigEntry<bool> CeilingCloak;
        internal static ConfigEntry<bool> TrapdoorBodyNoCollide;
        internal static ConfigEntry<float> VenomSwaySeconds;
        internal static ConfigEntry<float> VenomReachMeters;
        internal static ConfigEntry<float> VenomSpiderSlowPercent;
        internal static ConfigEntry<float> VenomSwayStrength;
        internal static ConfigEntry<float> VenomSwayStartsAfter;                   // how long the spider hunts the attacker before it goes back for the cocoon
        internal static ConfigEntry<float> VenomedCutMultiplier;
        internal static ConfigEntry<float> KnifeCutMultiplier;
        internal static ConfigEntry<float> CutDownSeconds;                      // how long the key must be held
        internal static ConfigEntry<bool> DropHeldItems;     // victim drops what they are holding when caught
        internal static ConfigEntry<bool> ChaseBoostEnabled;           // spider speeds up as it closes in (needs line of sight)
        internal static ConfigEntry<float> ChaseBoostMaxPercent;       // extra speed at point-blank range, in percent
        internal static ConfigEntry<float> ChaseBoostFarMeters;        // beyond this distance there is no boost
        internal static ConfigEntry<float> ChaseBoostNearMeters;       // at or inside this distance the boost is at its maximum
        internal static ConfigEntry<float> ChaseBoostRampSeconds;      // seconds to build up from no boost to the maximum
        internal static ConfigEntry<bool> WebAlertEnabled;             // spider sprints to a web a player has disturbed
        internal static ConfigEntry<float> WebAlertPercent;            // extra speed in percent
        internal static ConfigEntry<float> WebAlertFarBonusPercent;    // extra percent added when the spider is far from the web
        internal static ConfigEntry<float> WebAlertFarMeters;          // distance at which the far bonus is at its maximum
        internal static ConfigEntry<float> WebAlertSeconds;            // how long the sprint lasts at most
        internal static ConfigEntry<float> WebCountMultiplier;         // how many webs the spider places (1 = the game's normal 6-8)
        internal static ConfigEntry<float> WebPlaceRateMultiplier;     // how often it places them (1 = normal)
        internal static ConfigEntry<float> PatrolRadiusMultiplier;     // how far from its home it roams while placing webs (1 = normal)
        internal static ConfigEntry<float> HangHeightAboveGround;      // how high above the floor a victim hangs (0 = the game's own height)
        internal static ConfigEntry<bool> WaitingDissolve;             // use the Barber's dissolve effect instead of a plain fade
        internal static ConfigEntry<bool> WaitingFadeEnabled;          // spider fades out while it is waiting, motionless
        internal static ConfigEntry<float> WaitingOpacityPercent;      // how visible it is while waiting (100 = not at all faded)
        internal static ConfigEntry<float> WaitingFadeSeconds;         // how long the fade takes
        internal static ConfigEntry<float> WaitingRevealMeters;        // a player closer than this sees the spider normally
        internal static ConfigEntry<float> LookSensitivity;                   // how fast the mouse turns the cocoon camera
        internal static ConfigEntry<bool> EnableDebugSpawn;                    // testing only: key that spawns a spider
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugSpawnKey; // testing only
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugCollapseKey; // testing only
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugSpawnBrackenKey; // testing only
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugSpawnLootBugKey; // testing only
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugClearEnemiesKey; // testing only
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugFakeHitKey;      // testing only: pretend someone hit the carrying spider
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugFakeBiteKey; // testing only
        internal static ConfigEntry<bool> DebugKeysNeedShift; // testing only
        internal static ConfigEntry<bool> DebugLogging;
        internal static ConfigEntry<float> VenomSeconds;
        internal static ConfigEntry<bool> VenomHeartbeat;
        internal static ConfigEntry<float> VenomHeartbeatVolume;
        internal static ConfigEntry<bool> VenomAvoidsWeapons;   //
        internal static ConfigEntry<float> VenomAvoidMeters;      //
        internal static ConfigEntry<float> VenomHandoverMeters;
        internal static ConfigEntry<float> BiteCooldownSeconds;
        internal static ConfigEntry<float> SpiderFootstepRange;

        private void Awake()
        {
            Log = Logger;
            Cfg = Config;
            // Stops Unity destroying this object when the game changes scene.
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            HoldSeconds = Config.Bind(
                "General", "VictimHoldSeconds", 60f,
                "Safety net: if the spider has NOT hung the victim up after this many seconds, they are released.");
            HangSeconds = Config.Bind(
                "General", "SecondsHangingBeforeDeath", 45f,
                "How many seconds a victim stays hung up before they die.");
            ImmunitySeconds = Config.Bind(
                "General", "ImmunitySeconds", 10f,
                "Seconds after a release during which no monster (spider, Bracken, loot bug) can target the freed player.");
            ImmunityEndsOnAttack = Config.Bind(
                "General", "ImmunityEndsOnAttack", true,
                "If a freed player hits a monster while they are still immune, the immunity ends straight away (so a rescue cannot be used as a free window to attack).");
            DropHeldItems = Config.Bind(
                "General", "DropHeldItems", true,
                "Victims drop the items they are holding when they are caught.");
            CutDownKey = Config.Bind(
                "Rescue", "CutDownKey", UnityEngine.InputSystem.Key.E,
                "Hold this key near a hung cocoon to cut the victim down.");
            SpiderDropsWhenHit = Config.Bind(
                "Defend", "SpiderDropsWhenHit", true,
                "Advanced. Hit a spider that is carrying a wrapped player and it drops them to go after you. The victim must then struggle free (or be cut down).");
            StruggleKey = Config.Bind(
                "Defend", "StruggleKey", UnityEngine.InputSystem.Key.E,
                "Advanced. Mash this key to struggle free of a cocoon the spider has dropped.");
            StrugglePresses = Config.Bind(
                "Defend", "StrugglePresses", 14,
                new ConfigDescription("Advanced. How many presses it takes to break free if you never stop.", new AcceptableValueRange<int>(3, 60)));
            StruggleDecayPerSecond = Config.Bind(
                "Defend", "StruggleDecayPerSecond", 0.12f,
                new ConfigDescription("Advanced. Share of the struggle bar that drains each second you are not pressing (0 = never drains).", new AcceptableValueRange<float>(0f, 1f)));
            DropChaseSeconds = Config.Bind(
                "Defend", "DropChaseSeconds", 10f,
                new ConfigDescription("Advanced. Seconds the spider hunts the attacker before it goes back to pick the cocoon up again.", new AcceptableValueRange<float>(3f, 60f)));
            TrapdoorEnabled = Config.Bind(
                "Trapdoor", "TrapdoorSpiders", true,
                "Advanced. A spider that lives outdoors (for now only the test spawn key makes one) picks a random node, walks to it, sinks under the ground and waits to ambush.");
            TrapdoorDepth = Config.Bind(
                "Trapdoor", "TrapdoorDepth", 2.2f,
                new ConfigDescription("Advanced. How far under the ground (metres) the spider sinks.", new AcceptableValueRange<float>(0.5f, 6f)));
            TrapdoorTriggerMeters = Config.Bind(
                "Trapdoor", "TrapdoorTriggerMeters", 6f,
                new ConfigDescription("Advanced. A player this close to a buried spider makes it burst out and attack.", new AcceptableValueRange<float>(2f, 20f)));
            TrapdoorRelocateSeconds = Config.Bind(
                "Trapdoor", "TrapdoorRelocateSeconds", 90f,
                new ConfigDescription("Advanced. If nobody comes near for this long, the spider climbs out and buries itself somewhere else.", new AcceptableValueRange<float>(15f, 600f)));
            TrapdoorHoldSeconds = Config.Bind(
                "Trapdoor", "TrapdoorHoldSeconds", 20f,
                new ConfigDescription("Advanced. A victim the spider has dragged underground is let go after this many seconds (for testing). The death timer still runs, so keep this below HangSeconds.", new AcceptableValueRange<float>(5f, 120f)));
            TreeEnabled = Config.Bind(
                "Tree", "TreeSpiders", true,
                "Advanced. A spider that lives outdoors (for now only the test spawn key makes one) walks to a tree, climbs up under its branches and drops on a player who walks beneath. Spiders that are not chosen for the trees bury themselves instead (see the Trapdoor section).");
            TreeChance = Config.Bind(
                "Tree", "TreeChance", 100f,
                new ConfigDescription("Advanced. Percent of outdoor spiders that wait in trees (the rest use the trapdoor ambush). Decided once per spider, the same on every player's game.", new AcceptableValueRange<float>(0f, 100f)));
            TreeHeight = Config.Bind(
                "Tree", "TreeHeight", 16f,
                new ConfigDescription("Advanced. How high up the trunk (metres) the spider climbs before it waits for someone to pass beneath.", new AcceptableValueRange<float>(2f, 30f)));
            TreeGap = Config.Bind(
                "Tree", "TreeGap", -0.7f,
                new ConfigDescription("Advanced. How far (metres) the spider's feet sit from the trunk's solid surface once it is up. The solid part is wider than the trunk you can see further up, so a negative number pulls it closer to the visible trunk. Lower = closer.", new AcceptableValueRange<float>(-3f, 1f)));
            TreeLegSpread = Config.Bind(
                "Tree", "TreeLegSpread", 100f,
                new ConfigDescription("Advanced. How far the feet spread out sideways while the spider clings to a trunk, as a percentage of normal. 100 = normal, lower pulls the feet in towards the body (a tighter grip), higher spreads them wider.", new AcceptableValueRange<float>(30f, 160f)));
            TreeHangOut = Config.Bind(
                "Tree", "TreeHangOut", 1.2f,
                new ConfigDescription("Advanced. How far out from the trunk (metres) a hung victim's rope hangs. Too small and the cocoon swings into the bark.", new AcceptableValueRange<float>(0.3f, 4f)));
            TreeStalk = Config.Bind(
                "Stalk", "TreeStalk", true,
                "Advanced. A spider waiting in a tree that sees a player nearby (who is not looking at it) drops down and stalks them like the Feiopar: it freezes whenever anyone looks at it, creeps closer when nobody does, then charges. Off = it only drops on someone directly beneath it.");
            StalkTriggerMeters = Config.Bind(
                "Stalk", "StalkTriggerMeters", 25f,
                new ConfigDescription("Advanced. A player this close (sideways) to the tree, and not looking at the spider, makes it drop and begin stalking.", new AcceptableValueRange<float>(4f, 60f)));
            StalkFootstepPercent = Config.Bind(
                "Stalk", "StalkFootstepPercent", 5f,
                new ConfigDescription("Advanced. How loud (percent of normal) the spider's footsteps are while it stalks. Only on the host's game for now.", new AcceptableValueRange<float>(0f, 100f)));
            IndoorStalkChance = Config.Bind(
                "Stalk", "IndoorStalkChance", 50f,
                new ConfigDescription("Advanced. Percent of spiders inside the facility that roam and stalk players (freeze when watched, creep when not, then charge) instead of waiting on the ceiling. Decided once per spider, the same on every player's game. Needs TreeStalk on.", new AcceptableValueRange<float>(0f, 100f)));
            IndoorStalkTriggerMeters = Config.Bind(
                "Stalk", "IndoorStalkTriggerMeters", 15f,
                new ConfigDescription("Advanced. Inside the facility, a player this close (and not looking at the spider) starts the stalk.", new AcceptableValueRange<float>(4f, 40f)));
            VerboseLogs = Config.Bind(
                "Testing", "VerboseLogs", false,
                "Testing only: also write the noisy log lines (every sound while wrapped, the stalk status each second, web reeling, door dumps, spider diag/touch lines, weapon-avoidance and hiding-spot chatter, audio source dumps). Off keeps the console quiet.");
            LogAllSounds = Config.Bind(
                "Testing", "LogAllSounds", false,
                "Testing only (needs DebugLogging and VerboseLogs too): log every sound that starts anywhere, all the time, not just while wrapped. Used to find the name of a sound.");
            WrapExtraRolls = Config.Bind(
                "Cocoon", "WrapExtraRolls", 2f,
                new ConfigDescription("Advanced. How many extra full rolls the victim does while being wrapped. The game's own wrap animation only rolls them for about the first 2 of its 4 seconds, then they hang still. The extra rolls fill the rest of the wrap. 0 = the game's own behaviour.", new AcceptableValueRange<float>(0f, 8f)));
            WebMinSpacing = Config.Bind(
                "Webs", "WebMinSpacing", 3f,
                new ConfigDescription("Advanced. A spider will not lay a new trap web closer than this many metres to one it has already laid (measured between the middles of the strands, and between their ends). Applies to the game's own floor webs and to ceiling webs. 0 = off.", new AcceptableValueRange<float>(0f, 15f)));
            CeilingWebChance = Config.Bind(
                "Webs", "CeilingWebChance", 40f,
                new ConfigDescription("Advanced. Percent chance that, each time a patrolling spider lays a web, it ALSO hangs a strand from the ceiling down to the floor a few metres away (across where you walk). Only spiders indoors with a ceiling above them do this. 0 = off, the game's own webs only.", new AcceptableValueRange<float>(0f, 100f)));
            ShowAdvancedInMenu = Config.Bind(
                "General", "ShowAdvancedInMenu", false,
                "The in-game LethalConfig menu hides the advanced and testing settings so it stays short. Set this to true to show every setting there (restart the game to apply). Every setting is always in this file either way.");
            MuffleVoices = Config.Bind(
                "Cocoon", "MuffleVoices", true,
                "While someone is wrapped in a cocoon, their voice chat sounds muffled to everyone else, the same way the centipede muffles its victim.");
            SmoothJoinerSpider = Config.Bind(
                "Spider", "SmoothJoinerSpider", true,
                "Advanced. On a joining player's game, the spider's body is moved smoothly towards the positions the host sends, instead of jumping from one to the next (the jumps made its legs jitter). Host unaffected.");
            MuffleCutoff = Config.Bind(
                "Cocoon", "MuffleCutoffHz", 500f,
                new ConfigDescription("How muffled a wrapped player's voice is: sounds above this pitch are cut off. Lower is more muffled. The snare flea uses 500.", new AcceptableValueRange<float>(200f, 5000f)));
            MuffleResonance = Config.Bind(
                "Cocoon", "MuffleResonance", 5f,
                new ConfigDescription("How much the muffled voice rings at the cut-off pitch. High values can sound like an echo; 1 is flat. The snare flea uses 5.", new AcceptableValueRange<float>(1f, 10f)));
            TreeCloak = Config.Bind(
                "Stalk", "TreeCloak", true,
                "Advanced. A spider waiting in a tree or stalking dissolves (down to the Spider WaitingOpacityPercent) as soon as a player starts turning towards it, and becomes solid again once nobody is looking at it.");
            TreeCloakOpacityPercent = Config.Bind(
                "Stalk", "TreeCloakOpacityPercent", 2f,
                new ConfigDescription("Advanced. How visible (percent) a stalking or tree-waiting spider is while it is dissolved. 0-2 = almost invisible, 10 = the same as the normal waiting fade.", new AcceptableValueRange<float>(0f, 50f)));
            WebPull = Config.Bind(
                "Stalk", "WebPull", true,
                "Advanced. The end of a stalk: instead of charging from afar, the spider shoots a web line at you and reels you in (like the fox's tongue), then bites. Break the line of sight (get behind a wall) or hit the spider to get free. Off = it just charges.");
            WebEvenIfWatched = Config.Bind(
                "Testing", "WebEvenIfWatched", false,
                "Testing only: the spider shoots its web even while you are looking straight at it (normally it only shoots when nobody is looking), so you can see where the line comes from.");
            WebOriginForward = Config.Bind(
                "Stalk", "WebOriginForward", 0f,
                new ConfigDescription("Advanced. How far (metres) from the spider's abdomen, towards you, the web line starts. 0 = right at the abdomen (the default). Negative = further back, positive = towards the head.", new AcceptableValueRange<float>(-3f, 3f)));
            WebOriginUp = Config.Bind(
                "Stalk", "WebOriginUp", 0.1f,
                new ConfigDescription("Advanced. How high (metres) above the abdomen the web line starts. Negative = lower (the underside of the abdomen, the default).", new AcceptableValueRange<float>(-1.5f, 2f)));
            WebRange = Config.Bind(
                "Stalk", "WebRange", 8f,
                new ConfigDescription("Advanced. A stalking spider this close (and with a clear line to you, and nobody looking at it) shoots its web.", new AcceptableValueRange<float>(4f, 20f)));
            WebBreakMeters = Config.Bind(
                "Stalk", "WebBreakMeters", 12f,
                new ConfigDescription("Advanced. The web line snaps if you get this far (metres) from the spider while it is pulling you. It shoots from at most WebRange, so this is how far you can stretch it. Lower = easier to escape.", new AcceptableValueRange<float>(6f, 40f)));
            WebRampSeconds = Config.Bind(
                "Stalk", "WebRampSeconds", 4f,
                new ConfigDescription("Advanced. How many seconds the web takes to build from a light tug up to its full pull. A longer ramp gives you more time to fight it.", new AcceptableValueRange<float>(0.5f, 10f)));
            WebPeakMultiplier = Config.Bind(
                "Stalk", "WebPeakMultiplier", 5f,
                new ConfigDescription("Advanced. How much stronger the pull is at its peak than at the start (the start is 0.8 times WebStrength's base pull). Raise it if sprinting away can still beat the web.", new AcceptableValueRange<float>(1f, 8f)));
            WebStrength = Config.Bind(
                "Stalk", "WebStrength", 1f,
                new ConfigDescription("Advanced. How hard the web line pulls you, as a multiple of the fox tongue's pull. 1 = the same as the fox; lower is easier to resist.", new AcceptableValueRange<float>(0.2f, 2.5f)));
            TreeCloakStepLead = Config.Bind(
                "Stalk", "TreeCloakStepLead", 0.4f,
                new ConfigDescription("Advanced. How far ahead (seconds) the spider looks along its own path. If it is about to step into your view (round a corner, out from behind something), it dissolves before it gets there. 0 = off.", new AcceptableValueRange<float>(0f, 1.5f)));
            TreeCloakLead = Config.Bind(
                "Stalk", "TreeCloakLead", 0.25f,
                new ConfigDescription("Advanced. How far ahead (seconds) the spider predicts where your view is heading. If you are turning towards it, it dissolves this much earlier. 0 = no prediction.", new AcceptableValueRange<float>(0f, 0.6f)));
            TreeCloakAngle = Config.Bind(
                "Stalk", "TreeCloakAngle", 90f,
                new ConfigDescription("Advanced. How close to the centre of a player's view (degrees) the spider must be for it to dissolve. The spider freezes at 67, so a bigger number (the default is 90) makes it dissolve before it freezes, so it dissolves as soon as you start turning towards it. Smaller = it only dissolves when you look almost straight at it.", new AcceptableValueRange<float>(5f, 120f)));
            StalkDirectChance = Config.Bind(
                "Stalk", "StalkDirectChance", 70f,
                new ConfigDescription("Advanced. Percent of outdoor spiders that roam and stalk players directly, with no tree. The rest wait in trees first (TreeChance). Decided once per spider, the same on every player's game.", new AcceptableValueRange<float>(0f, 100f)));
            OutdoorSpawnRarity = Config.Bind(
                "Spawning", "OutdoorSpawnRarity", 20f,
                new ConfigDescription("Advanced. How often bunker spiders are picked as outdoor monsters, compared with the other outdoor monsters (the game's own outdoor monsters use numbers from about 5 to 30). 0 = spiders only spawn inside, as in the normal game. Only the host's setting matters.", new AcceptableValueRange<float>(0f, 200f)));
            CeilingEnabled = Config.Bind(
                "Ceiling", "CeilingSpiders", true,
                "Advanced. A spider inside the facility climbs up to the ceiling, turns invisible, and drops on a player who walks underneath.");
            CeilingChance = Config.Bind(
                "Ceiling", "CeilingChance", 100f,
                new ConfigDescription("Advanced. Percent chance that a spider goes to the ceiling to ambush (rolled each time it picks a new spot). The rest of the time it behaves as normal.", new AcceptableValueRange<float>(0f, 100f)));
            CeilingGap = Config.Bind(
                "Ceiling", "CeilingGap", 0.15f,
                new ConfigDescription("Advanced. The gap (metres) between the spider's highest foot and the ceiling once it is in place. 0 = touching.", new AcceptableValueRange<float>(-0.5f, 3f)));
            CeilingTriggerMeters = Config.Bind(
                "Ceiling", "CeilingTriggerMeters", 2.8f,
                new ConfigDescription("Advanced. A player this close (sideways) beneath the spider makes it drop.", new AcceptableValueRange<float>(1f, 10f)));
            CeilingRelocateSeconds = Config.Bind(
                "Ceiling", "CeilingRelocateSeconds", 90f,
                new ConfigDescription("Advanced. If nobody comes underneath for this long, the spider comes down and picks another spot.", new AcceptableValueRange<float>(15f, 600f)));
            CeilingCloak = Config.Bind(
                "Ceiling", "CeilingCloak", false,
                "Advanced. On: the spider dissolves like the waiting spider (down to the Spider WaitingOpacityPercent) as it reaches the ceiling, but is never fully invisible. Off: it stays fully solid up there.");
            CeilingRiseSpeed = Config.Bind(
                "Ceiling", "CeilingRiseSpeed", 3f,
                new ConfigDescription("Advanced. How fast (metres per second) the spider climbs up to the ceiling.", new AcceptableValueRange<float>(0.5f, 12f)));
            DigOutSeconds = Config.Bind(
                "Trapdoor", "DigOutSeconds", 6f,
                new ConfigDescription("Advanced. How many seconds a teammate must hold the rescue key at the disturbed earth to dig a buried victim out. The spider comes back up to defend them.", new AcceptableValueRange<float>(1f, 30f)));
            TrapdoorBodyNoCollide = Config.Bind(
                "Trapdoor", "TrapdoorBodyNoCollide", true,
                "Advanced. While the spider drags a victim underground, switch off every collider and collision on the stand-in body so the ground cannot stop it. The log (lines starting 'Cocoon dump') shows what was blocking it either way.");
            CutDownSeconds = Config.Bind(
                "Rescue", "CutDownSeconds", 4f,
                "How many seconds the key must be held to cut a victim down.");
            VenomedCutMultiplier = Config.Bind(
                "Rescue", "VenomedCutMultiplier", 3f,
                new ConfigDescription("While you are venomed (bitten, but not yet wrapped) cutting a friend down takes this many times longer, as your muscles stop obeying you. 1 = no change.",
                    new AcceptableValueRange<float>(1f, 10f)));
            KnifeCutMultiplier = Config.Bind(
                "Rescue", "KnifeCutMultiplier", 0.5f,
                new ConfigDescription("Cutting a friend down with a knife in your hand takes this share of the normal time (0.5 = half as long). 1 = a knife makes no difference. Does not apply to digging someone out.",
                    new AcceptableValueRange<float>(0.1f, 1f)));
            CarryTimeoutSeconds = Config.Bind(
                "General", "CarryTimeoutSeconds", 30f,
                "If a spider has been carrying a victim this many seconds without hanging them (for example it cannot find a way to its lair), it hangs them where it is. 0 turns this off.");
            WallReachMeters = Config.Bind(
                "General", "WallReachMeters", 0.8f,
                "How close (in metres) the spider has to get to its hanging spot on the wall before it hangs a victim. The game wants 0.1, which a spider that bumps into a wall or grate may never reach. 0 uses the game's own value.");
            VenomReachMeters = Config.Bind(
                "Venom", "VenomReachMeters", 15f,
                new ConfigDescription("Advanced. When the venom runs out and you collapse, the spider only comes for you if it can actually walk to you: there must be a walkable route, and it must be no longer than this many metres. Otherwise you wake up where you fell (no wrap) and the spider goes back to patrolling. 0 = no limit on the length.", new AcceptableValueRange<float>(0f, 300f)));
            VenomSpiderSlowPercent = Config.Bind(
                "Venom", "VenomSpiderSlowPercent", 50f,
                new ConfigDescription("Advanced. After a bite, while the venom is working on its victim, the spider moves this much slower (50 = half speed), giving the victim a fair chance to run. It returns to normal speed when the victim collapses. 0 = no slowdown.", new AcceptableValueRange<float>(0f, 90f)));
            VenomSwaySeconds = Config.Bind(
                "Venom", "VenomSwaySeconds", 3f,
                new ConfigDescription("Advanced. In the last few seconds of the venom the bitten player is forced to sway: the view drifts left, right, up and down, building up until they collapse. (The forced sideways stagger of the body was removed.) 0 = off.",
                    new AcceptableValueRange<float>(0f, 10f)));
            VenomSwayStartsAfter = Config.Bind(
                "Venom", "VenomSwayStartsAfterSeconds", 4f,
                new ConfigDescription("Advanced. The sway begins this many seconds after the bite and builds until the collapse. -1 = use VenomSwaySeconds instead (the last few seconds only).",
                    new AcceptableValueRange<float>(-1f, 30f)));
            VenomSwayStrength = Config.Bind(
                "Venom", "VenomSwayStrength", 1f,
                new ConfigDescription("Advanced. How hard the forced sway pushes (1 = default).", new AcceptableValueRange<float>(0f, 3f)));
            VenomSeconds = Config.Bind(
                "Venom", "VenomSeconds", 8f,
                new ConfigDescription("How long the bitten player stumbles about under the spider's venom (slowed, dizzy, screen warping) before they collapse and get wrapped. 0 = off: wrapped the instant they are bitten, as in 1.8.1 and earlier. Everyone in the game should use the same value.",
                    new AcceptableValueRange<float>(0f, 30f)));
            VenomHeartbeat = Config.Bind(
                "Venom", "VenomHeartbeat", true,
                "Plays the game's heartbeat sound, getting faster as the venom takes hold. Only the bitten player hears it. Does nothing if the game has no heartbeat sound.");
            VenomHeartbeatVolume = Config.Bind(
                "Venom", "VenomHeartbeatVolume", 3f,
                new ConfigDescription("How loud the venom heartbeat is. 1 = the game's own heartbeat volume (quiet), 3 = loud (default), up to 6. It still starts softer and builds up as the venom takes hold.", new AcceptableValueRange<float>(0f, 6f)));
            VenomAvoidsWeapons = Config.Bind(
                "Venom", "SpiderAvoidsWeapons", true,
                "Advanced. While a bitten player is still stumbling, if they are holding a weapon (shovel, knife, stop sign, shotgun...), the spider backs off and stays out of sight until the venom takes them. Needs VenomSeconds above 0.");
            VenomAvoidMeters = Config.Bind(
                "Venom", "SpiderAvoidMeters", 10f,
                new ConfigDescription("Advanced. How far the spider tries to stay from an armed, bitten player.",
                    new AcceptableValueRange<float>(3f, 30f)));
            VenomHandoverMeters = Config.Bind(
                "Venom", "VenomHandoverMeters", 25f,
                new ConfigDescription("If the spider that bit a player dies while the venom is in them, and ANOTHER living spider is within this many metres, that spider walks over and carries on (wraps and hangs them). 0 = off: the player just collapses, lies there briefly and recovers.",
                    new AcceptableValueRange<float>(0f, 60f)));
            BiteCooldownSeconds = Config.Bind(
                "Venom", "BiteCooldownSeconds", 8f,
                new ConfigDescription("After a spider bites someone, that same spider will not bite anyone else for this many seconds, so a group walking past it is not all caught in one pass (it deals with the first victim first). 0 = off.",
                    new AcceptableValueRange<float>(0f, 60f)));
            EmergencyReleaseKey = Config.Bind(
                "General", "EmergencyReleaseKey", false,
                "Pressing F9 frees you from a cocoon instantly. Off by default because it skips the spider, the hang and the rescue. Turn on if you want a way out if something ever goes wrong (a stuck cocoon is also freed automatically after HoldSeconds).");
            SuffocationDeath = Config.Bind(
                "General", "SuffocationDeath", true,
                "The victim's cause of death is Suffocation (hung and wrapped in silk) instead of Mauling. This is what the scanner shows on the body, and what other mods that read the cause of death will see. Turn off for the old Mauling cause (which also makes the corpse bloody).");
            WrapOnCatch = Config.Bind(
                "General", "WrapOnCatch", true,
                "The spider wraps the victim in silk straight away, where they were caught, and only then drags them to its lair. Turn off for the game's own order (drag first, wrap at the lair).");
            BrackenStaysAfterChase = Config.Bind(
                "Bracken", "StaysAfterChase", true,
                "When a Bracken loses its chase target (its anger runs out) it normally retreats and hides for a while. With this on it goes straight back to stalking instead, so it keeps hanging around its prey.");
            BrackenSteals = Config.Bind(
                "Thieves", "BrackenSteals", true,
                "A Bracken that notices a hung victim may cut them down and carry them off to its own lair. The victim stays wrapped and their death timer keeps running, so teammates can still cut them free wherever the Bracken drops them.");
            BrackenNoticeMeters = Config.Bind(
                "Thieves", "BrackenNoticeMeters", 25f,
                "How close (in metres) a Bracken must be to a hung victim to notice them.");
            BrackenStealChancePercent = Config.Bind(
                "Thieves", "BrackenStealChancePercent", 50f,
                "Chance (0 to 100) that a Bracken that notices a victim goes for them. It rolls once per victim.");
            BrackenStealDelaySeconds = Config.Bind(
                "Thieves", "BrackenStealDelaySeconds", 5f,
                "How long (in seconds) a victim must have been hanging before a Bracken may take them. The Bracken waits next to the cocoon meanwhile, so the spider can finish hanging them first.");
            ShowDeathTimer = Config.Bind(
                "General", "ShowDeathTimer", true,
                "Show a countdown to death: on the cocooned player's screen, and next to the 'cut them down' prompt for rescuers.");
            LootBugSteals = Config.Bind(
                "Thieves", "LootBugSteals", true,
                "Loot bugs may also carry a hung victim off to their nest (they use the same notice, chance, delay and reach settings as the Bracken).");
            LootBugCarryHeight = Config.Bind(
                "Thieves", "LootBugCarryHeight", 2.2f,
                "How high (in metres) above itself a loot bug holds a stolen cocoon, so it is not dragged along the floor.");
            LootBugRearUpDegrees = Config.Bind(
                "Thieves", "LootBugRearUpDegrees", 55f,
                "How far (in degrees) a loot bug rears up on its back legs while holding a stolen cocoon overhead. 0 = stay on all fours. Use a negative number if it tips the wrong way.");
            BrackenStealExtraSeconds = Config.Bind(
                "Thieves", "BrackenStealExtraSeconds", 30f,
                "Extra seconds added to the victim's death timer when a Bracken steals them, so the team has time to find and rescue them.");
            NoBlackoutOnBrackenKill = Config.Bind(
                "Bracken", "NoBlackoutOnBrackenKill", true,
                "If true, the screen does not fade to black when a Bracken kills you (you still die and spectate normally).");
            BrackenReachMeters = Config.Bind(
                "Thieves", "BrackenReachMeters", 2f,
                "How close (in metres, sideways) the Bracken has to get to the victim to take them.");
            BrackenGiveUpSeconds = Config.Bind(
                "Thieves", "BrackenGiveUpSeconds", 45f,
                "A Bracken that has not reached the victim after this many seconds gives up.");
            BrackenScaredMeters = Config.Bind(
                "Thieves", "BrackenScaredMeters", 6f,
                "A Bracken will not go for (or will give up on) a victim while another player is within this many metres of them.");
            BrackenCarryStyle = Config.Bind(
                "Thieves", "BrackenCarryStyle", "Shoulder",
                "How a Bracken carries a stolen cocoon: Shoulder (over its shoulder), Drag (trailing behind it) or Hand (the game's normal carry, held out in front).");
            BrackenCarryHeight = Config.Bind(
                "Thieves", "BrackenCarryHeight", 2.7f,
                "Shoulder style: how high (in metres) above the Bracken's feet the cocoon is held.");
            BrackenCarryBack = Config.Bind(
                "Thieves", "BrackenCarryBack", -0.1f,
                "Shoulder style: how far (in metres) behind the Bracken's middle the cocoon is held. Negative = in front.");
            BrackenCarrySide = Config.Bind(
                "Thieves", "BrackenCarrySide", 0.75f,
                "Shoulder style: how far (in metres) to the Bracken's right the cocoon is held. Negative = left.");
            BrackenCarryPitch = Config.Bind(
                "Thieves", "BrackenCarryPitch", 90f,
                "Shoulder style: tips the cocoon forward or back, in degrees. 90 lays it flat across the shoulder if it hangs upright at 0.");
            BrackenCarryYaw = Config.Bind(
                "Thieves", "BrackenCarryYaw", 0f,
                "Shoulder style: turns the cocoon left or right, in degrees, when seen from above.");
            BrackenCarryRoll = Config.Bind(
                "Thieves", "BrackenCarryRoll", 0f,
                "Shoulder style: rolls the cocoon over onto its side, in degrees.");
            BrackenDragDistance = Config.Bind(
                "Thieves", "BrackenDragDistance", 1.2f,
                "How far (in metres) behind the Bracken the dragged cocoon trails.");
            BrackenDragHeight = Config.Bind(
                "Thieves", "BrackenDragHeight", 0.9f,
                "How high (in metres) above the floor the dragged cocoon is held. Lower drags it along the ground.");
            BrackenDragNormalPose = Config.Bind(
                "Thieves", "BrackenDragNormalPose", true,
                "While carrying a cocoon on its shoulder or dragging it, the Bracken walks in its normal pose instead of the game's arms-out carrying pose. Turn off to keep the carrying pose.");
            BrackenArmOver = Config.Bind(
                "Thieves", "BrackenArmOver", true,
                "While carrying a cocoon on its shoulder, the Bracken's arm on that side is draped up and over the cocoon.");
            BrackenArmUp = Config.Bind(
                "Thieves", "BrackenArmUp", -0.2f,
                "How far (in metres) above the cocoon's carry point the hand rests.");
            BrackenArmIn = Config.Bind(
                "Thieves", "BrackenArmIn", 0.25f,
                "How far (in metres) the hand reaches across towards the Bracken's body from the carry point. Larger puts the hand further over the cocoon.");
            BrackenArmForward = Config.Bind(
                "Thieves", "BrackenArmForward", 0f,
                "How far (in metres) in front of the carry point the hand rests. Negative moves it behind.");
            BrackenAmbush = Config.Bind(
                "Thieves", "BrackenAmbush", true,
                "A Bracken that has noticed a victim being hung hides out of sight, and only comes out to take the cocoon once the spider has hung it and gone invisible. Turn off and it walks straight up to the victim.");
            BrackenAmbushMinMeters = Config.Bind(
                "Thieves", "BrackenAmbushMinMeters", 8f,
                "The closest (in metres) to the hung cocoon that a Bracken's hiding spot can be.");
            BrackenAmbushMaxMeters = Config.Bind(
                "Thieves", "BrackenAmbushMaxMeters", 16f,
                "The furthest (in metres) from the hung cocoon that a Bracken's hiding spot can be.");
            BrackenAmbushPatienceSeconds = Config.Bind(
                "Thieves", "BrackenAmbushPatienceSeconds", 12f,
                "A hiding Bracken normally waits until the spider has gone invisible. If the cocoon has been hung this many seconds and the spider still has not gone, it comes out anyway (as long as no player is close). Stops it hiding until the victim dies.");
            BrackenAmbushMaxWaitSeconds = Config.Bind(
                "Thieves", "BrackenAmbushMaxWaitSeconds", 90f,
                "A hiding Bracken gives up and wanders off if it has not been able to take the cocoon after this many seconds.");
            LootBugYield = Config.Bind(
                "Thieves", "LootBugYield", true,
                "Loot bugs back away from a Bracken that is going for a cocoon (and from the cocoon it is going for), so they do not crowd and block its path.");
            LootBugYieldMeters = Config.Bind(
                "Thieves", "LootBugYieldMeters", 15f,
                "How close (in metres) a loot bug has to be to such a Bracken, or to the cocoon it wants, before it backs away.");
            StuckSeconds = Config.Bind(
                "General", "StuckSeconds", 8f,
                "If a spider stands still this many seconds while carrying a victim, it hangs them where it stands.");

            ChaseBoostEnabled = Config.Bind(
                "Spider", "ChaseBoostEnabled", true,
                "The bunker spider speeds up as it closes in on the player it is chasing, but ONLY while it has a clear line of sight to them. Break line of sight and it slows back down.");
            ChaseBoostMaxPercent = Config.Bind(
                "Spider", "ChaseBoostMaxPercent", 35f,
                "How much faster the spider gets at point-blank range, in percent (35 = 35% faster).");
            ChaseBoostFarMeters = Config.Bind(
                "Spider", "ChaseBoostFarMeters", 20f,
                "Distance in metres beyond which the spider gets no speed boost.");
            ChaseBoostNearMeters = Config.Bind(
                "Spider", "ChaseBoostNearMeters", 4f,
                "Distance in metres at or inside which the spider has its full speed boost.");
            ChaseBoostRampSeconds = Config.Bind(
                "Spider", "ChaseBoostRampSeconds", 2f,
                "Seconds it takes the boost to build up from nothing to the maximum. It fades away about twice as fast.");
            WebAlertEnabled = Config.Bind(
                "Spider", "WebAlertEnabled", true,
                "When a player steps in a web, the spider instantly gets a burst of speed to rush to the disturbed web.");
            WebAlertPercent = Config.Bind(
                "Spider", "WebAlertPercent", 100f,
                "How much faster the spider moves during the web alert, in percent (100 = twice as fast).");
            WebAlertFarBonusPercent = Config.Bind(
                "Spider", "WebAlertFarBonusPercent", 120f,
                "Extra speed (in percent, on top of WebAlertPercent) when the spider is at WebAlertFarMeters or further from the disturbed web. It scales down smoothly as the spider gets closer.");
            WebAlertFarMeters = Config.Bind(
                "Spider", "WebAlertFarMeters", 20f,
                "Distance in metres from the disturbed web at which the spider gets the full far bonus.");
            WebAlertSeconds = Config.Bind(
                "Spider", "WebAlertSeconds", 4f,
                "Maximum seconds the burst lasts. It also ends once the spider reaches the web.");
            WebCountMultiplier = Config.Bind(
                "Spider", "WebCountMultiplier", 2f,
                "Multiplies how many webs each spider places. 1 = the game's normal 6 to 8, 2 = 12 to 16. Host's setting is the one that counts.");
            WebPlaceRateMultiplier = Config.Bind(
                "Spider", "WebPlaceRateMultiplier", 2f,
                "How much faster the spider places webs while patrolling. 1 = normal, 2 = twice as often. Host's setting is the one that counts.");
            PatrolRadiusMultiplier = Config.Bind(
                "Spider", "PatrolRadiusMultiplier", 2f,
                "How much further from its starting area the spider roams while patrolling and placing webs. 1 = normal (about 17 m), 2 = about 34 m. Host's setting is the one that counts.");
            HangHeightAboveGround = Config.Bind(
                "Spider", "HangHeightAboveGround", 1.6f,
                "How high above the floor, in metres, a victim's body hangs (measured to their middle). Because it is measured from the floor, a victim is never out of reach of a rescuer standing there, and never hangs higher than just under the ceiling. Set 0 to use the game's own hanging height. Everyone should use the same value.");
            WaitingDissolve = Config.Bind(
                "Spider", "WaitingDissolve", true,
                "Use the Barber's dissolve effect (the spider breaks up into flickering speckles) instead of a smooth fade. If the Barber's material cannot be found the smooth fade is used.");
            WaitingFadeEnabled = Config.Bind(
                "Spider", "WaitingFadeEnabled", true,
                "While the bunker spider is waiting motionless (not chasing, not carrying anyone) it fades to partly see-through, so it is harder to spot. This is a visual effect on YOUR screen only.");
            WaitingOpacityPercent = Config.Bind(
                "Spider", "WaitingOpacityPercent", 10f,
                "How visible the waiting spider is, in percent. 100 = fully visible (no fade), 0 = invisible.");
            WaitingFadeSeconds = Config.Bind(
                "Spider", "WaitingFadeSeconds", 2f,
                "Seconds the spider takes to fade out once it has been still for a moment. It becomes visible again about four times faster.");
            WaitingRevealMeters = Config.Bind(
                "Spider", "WaitingRevealMeters", 1f,
                "A player closer to the waiting spider than this many metres sees it normally. Set 0 to never reveal it.");
            LookSensitivity = Config.Bind(
                "Camera", "LookSensitivity", 0.12f,
                "How fast the mouse turns the camera while you are wrapped up. Mouse wheel zooms in and out.");
            EnableDebugSpawn = Config.Bind(
                "Testing", "EnableDebugSpawnKey", false,
                "Testing only: lets the HOST press F6 to spawn a bunker spider, F8 a Bracken and F10 a loot bug in front of them, F11 to kill every enemy, F3 to fake a hit on a carrying spider (and F7 to log nearby labels). Set this to false for normal play.");
            DebugSpawnKey = Config.Bind(
                "Testing", "DebugSpawnKey", UnityEngine.InputSystem.Key.Numpad1,
                "The key that spawns a bunker spider (host only, needs EnableDebugSpawnKey).");
            DebugSpawnBrackenKey = Config.Bind(
                "Testing", "DebugSpawnBrackenKey", UnityEngine.InputSystem.Key.Numpad2,
                "The key that spawns a Bracken (host only, needs EnableDebugSpawnKey).");
            DebugSpawnLootBugKey = Config.Bind(
                "Testing", "DebugSpawnLootBugKey", UnityEngine.InputSystem.Key.Numpad3,
                "The key that spawns a loot bug (host only, needs EnableDebugSpawnKey).");
            DebugFakeHitKey = Config.Bind(
                "Testing", "DebugFakeHitKey", UnityEngine.InputSystem.Key.Numpad4,
                "The key that pretends someone hit a spider that is carrying a wrapped player, so you can test the drop and the struggle on your own (host only, needs EnableDebugSpawnKey). The spider does not chase anyone.");
            DebugCollapseKey = Config.Bind(
                "Testing", "DebugCollapseKey", UnityEngine.InputSystem.Key.Numpad5,
                "The key that makes you collapse from the venom straight away, while the venom is running (needs EnableDebugSpawnKey). For timing tests with other monsters.");
            DebugClearEnemiesKey = Config.Bind(
                "Testing", "DebugClearEnemiesKey", UnityEngine.InputSystem.Key.Numpad6,
                "The key that kills every enemy on the level (host only, needs EnableDebugSpawnKey). Enemies that cannot die are removed instead.");
            SpiderFootstepRange = Config.Bind(
                "General", "SpiderFootstepRange", 1f,
                new ConfigDescription("How far away the bunker spider's footsteps can be heard, as a fraction of the game's normal distance. 1 = unchanged, 0.5 = half the distance. Only changes what you hear on your own game.",
                    new AcceptableValueRange<float>(0.1f, 1f)));
            DebugLogging = Config.Bind(
                "Testing", "DebugLogging", false,
                "Writes extra, repetitive detail to the log (carry progress, notice-range checks, back-off messages). Leave off unless you are chasing a problem.");
            DebugKeysNeedShift = Config.Bind(
                "Testing", "DebugKeysNeedShift", false,
                "When true, the debug keys (spawn, collapse, fake bite and so on) only work while you hold Shift, so they do not clash with other mods that use the F keys (needs EnableDebugSpawnKey).");
            DebugFakeBiteKey = Config.Bind(
                "Testing", "DebugFakeBiteKey", UnityEngine.InputSystem.Key.Numpad7,
                "The key that pretends the nearest spider just bit you, so the venom starts (needs EnableDebugSpawnKey). For testing what happens when another monster already has you.");

            CocoonRunner.Ensure(); // exists from startup so the network messages are ready before the first catch
            Harmony harmony = new Harmony("Militus.HungToDry");
            harmony.PatchAll();
            AdvancedHider.Apply(Config);
            try
            {
                // Webs ignore wrapped-up players: patch the web trigger's enter/stay/exit by name.
                Type webTrigger = AccessTools.TypeByName("SandSpiderWebTrap");
                if (webTrigger != null)
                {
                    foreach (string m in new[] { "OnTriggerStay" }) // the only place a web catches a player
                    {
                        System.Reflection.MethodBase mb = AccessTools.Method(webTrigger, m, new[] { typeof(Collider) });
                        if (mb != null) harmony.Patch(mb, prefix: new HarmonyMethod(typeof(WebIgnorePatch), "Prefix"));
                    }
                }
                else Log.LogWarning("Could not find the spider web class; webs may still catch a wrapped player's hidden body.");
            }
            catch (Exception e)
            {
                Log.LogWarning("Web ignore patch failed: " + e.Message);
            }
            try
            {
                System.Reflection.MethodBase targetable = AccessTools.Method(typeof(EnemyAI), "PlayerIsTargetable");
                if (targetable != null)
                {
                    harmony.Patch(targetable, postfix: new HarmonyMethod(typeof(SpiderTargetPatch), "Postfix"));
                }
                else
                {
                    Log.LogWarning("Could not find EnemyAI.PlayerIsTargetable; spiders may re-target frozen players.");
                }
            }
            catch (Exception e)
            {
                Log.LogWarning("Could not patch PlayerIsTargetable: " + e.Message);
            }
            try
            {
                // A wrapped-up player (and the stand-in body the spider carries) must not set off landmines.
                Type mine = AccessTools.TypeByName("Landmine");
                if (mine != null)
                {
                    foreach (string m in new[] { "OnTriggerEnter", "OnTriggerExit" })
                    {
                        System.Reflection.MethodBase mb = AccessTools.Method(mine, m, new[] { typeof(Collider) });
                        if (mb != null) harmony.Patch(mb, prefix: new HarmonyMethod(typeof(LandmineIgnorePatch), "Prefix"));
                    }
                    System.Reflection.MethodBase upd = AccessTools.Method(mine, "Update");
                    if (upd != null) harmony.Patch(upd, prefix: new HarmonyMethod(typeof(LandmineIgnorePatch), "UpdatePrefix"));
                }
                else Log.LogWarning("Could not find the landmine class; a wrapped player may still set mines off.");
            }
            catch (Exception e)
            {
                Log.LogWarning("Landmine patch failed: " + e.Message);
            }
            try
            {
                // Turrets and spike traps look for the real body, which is hidden (but still there) while wrapped.
                Type turret = AccessTools.TypeByName("Turret");
                if (turret != null)
                {
                    System.Reflection.MethodBase mb = AccessTools.Method(turret, "CheckForPlayersInLineOfSight", new[] { typeof(float), typeof(bool) });
                    if (mb != null) harmony.Patch(mb, postfix: new HarmonyMethod(typeof(HazardIgnorePatch), "TurretPostfix"));
                    else Log.LogWarning("Could not find the turret's player check; a wrapped player may still be shot.");
                    // A turret already locked onto someone keeps aiming at their hidden real body (and firing at it) for as long as it
                    // can see it. Treat a wrapped target like a dead one: no line of sight, so it gives up after its 2 s timer.
                    System.Reflection.MethodBase tl = AccessTools.Method(turret, "TurnTowardsTargetIfHasLOS");
                    if (tl != null) harmony.Patch(tl, prefix: new HarmonyMethod(typeof(HazardIgnorePatch), "TurretLosPrefix"));
                    else Log.LogWarning("Could not find the turret's aim code; a turret may keep aiming at a wrapped player.");
                }
                Type spike = AccessTools.TypeByName("SpikeRoofTrap");
                if (spike != null)
                {
                    System.Reflection.MethodBase mb = AccessTools.Method(spike, "OnTriggerStay", new[] { typeof(Collider) });
                    if (mb != null) harmony.Patch(mb, prefix: new HarmonyMethod(typeof(HazardIgnorePatch), "SpikePrefix"));
                    else Log.LogWarning("Could not find the spike trap's crush check; a wrapped player may still be crushed.");
                }
            }
            catch (Exception e)
            {
                Log.LogWarning("Turret / spike trap patch failed: " + e.Message);
            }
            try
            {
                // Spider footsteps that the game plays as one-shot sounds: turn those down as well.
                System.Reflection.MethodBase oneShot = AccessTools.Method(typeof(AudioSource), "PlayOneShot", new Type[] { typeof(AudioClip), typeof(float) });
                // The patch only writes debug log lines now, yet it would run for every sound in the game: only add it when debugging.
                if (oneShot != null && DebugLogging.Value) harmony.Patch(oneShot, prefix: new HarmonyMethod(typeof(SpiderStepSoundPatch), "Prefix"));
            }
            catch (Exception e)
            {
                Log.LogWarning("Could not patch the spider step sounds: " + e.Message);
            }
            try
            {
                // Snare fleas, thumpers and anything else that picks a player by sight or distance must skip a
                // wrapped-up player's hidden body, exactly as the spider already does.
                foreach (string name in new string[] { "GetClosestPlayer", "CheckLineOfSightForPlayer", "CheckLineOfSightForClosestPlayer" })
                {
                    foreach (System.Reflection.MethodInfo mi in typeof(EnemyAI).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                    {
                        if (mi.Name != name || mi.ReturnType != typeof(PlayerControllerB)) continue;
                        harmony.Patch(mi, postfix: new HarmonyMethod(typeof(EnemyPickPlayerPatch), "Postfix"));
                        Dbg("Patched EnemyAI." + name + " so monsters skip wrapped players.");
                    }
                }
                HashSet<System.Reflection.MethodBase> patchedTargets = new HashSet<System.Reflection.MethodBase>();
                foreach (Type t in new Type[] { typeof(CentipedeAI), typeof(CrawlerAI) })
                {
                    foreach (string mn in new string[] { "Update", "DoAIInterval" })
                    {
                        System.Reflection.MethodInfo m = AccessTools.Method(t, mn);
                        if (m == null || !patchedTargets.Add(m)) continue;
                        harmony.Patch(m, postfix: new HarmonyMethod(typeof(EnemyDropTargetPatch), "Postfix"));
                        if (t == typeof(CentipedeAI) && mn == "Update") harmony.Patch(m, prefix: new HarmonyMethod(typeof(EnemyDropTargetPatch), "Prefix"));
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogWarning("Could not patch monster targeting: " + e.Message);
            }
            Log.LogInfo("Hung to Dry 1.9.0 loaded (spider camera, death timer, cut-down rescue, anti-stall).");
        }
    }

    // A tiny object that runs our timers and per-frame jobs. It protects itself from
    // scene changes and recreates itself if it was ever destroyed.
    internal class CocoonRunner : MonoBehaviour
    {
        private static CocoonRunner instance;

        public static void Ensure()
        {
            if (instance != null) return;
            GameObject go = new GameObject("SpiderCocoonRunner");
            go.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(go);
            instance = go.AddComponent<CocoonRunner>();
        }

        public static void Run(IEnumerator routine)
        {
            Ensure();
            instance.StartCoroutine(routine);
        }

        private static GUIStyle timerStyle;
        private static GUIStyle shadowStyle;

        private void OnGUI()
        {
            try
            {
                if (Event.current.type != EventType.Repaint) return;
                if (Plugin.DebugLogging.Value && Cocoon.VenomTimerLeft >= 0f)
                {
                    if (timerStyle == null)
                    {
                        timerStyle = new GUIStyle(GUI.skin.label);
                        timerStyle.alignment = TextAnchor.MiddleCenter;
                        timerStyle.fontStyle = FontStyle.Bold;
                        timerStyle.fontSize = 34;
                        shadowStyle = new GUIStyle(timerStyle);
                        shadowStyle.normal.textColor = Color.black;
                    }
                    timerStyle.normal.textColor = new Color(0.6f, 1f, 0.5f);
                    Rect vr = new Rect(0f, Screen.height * 0.2f, Screen.width, 60f);
                    string vt = "Venom: " + Cocoon.VenomTimerLeft.ToString("0.0") + "s";
                    GUI.Label(new Rect(vr.x + 2f, vr.y + 2f, vr.width, vr.height), vt, shadowStyle);
                    GUI.Label(vr, vt, timerStyle);
                }
                if (Cocoon.StruggleActive)
                {
                    if (timerStyle == null)
                    {
                        timerStyle = new GUIStyle(GUI.skin.label);
                        timerStyle.alignment = TextAnchor.MiddleCenter;
                        timerStyle.fontStyle = FontStyle.Bold;
                        timerStyle.fontSize = 34;
                        shadowStyle = new GUIStyle(timerStyle);
                        shadowStyle.normal.textColor = Color.black;
                    }
                    timerStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
                    Rect sr = new Rect(0f, Screen.height * 0.12f, Screen.width, 60f);
                    string st = "STRUGGLE! Mash [" + Plugin.StruggleKey.Value + "]";
                    GUI.Label(new Rect(sr.x + 2f, sr.y + 2f, sr.width, sr.height), st, shadowStyle);
                    GUI.Label(sr, st, timerStyle);
                    float bw = Screen.width * 0.3f, bx = (Screen.width - bw) * 0.5f, by = Screen.height * 0.12f + 62f;
                    GUI.color = new Color(0f, 0f, 0f, 0.6f);
                    GUI.DrawTexture(new Rect(bx - 3f, by - 3f, bw + 6f, 22f), Texture2D.whiteTexture);
                    GUI.color = new Color(1f, 0.85f, 0.3f, 0.95f);
                    GUI.DrawTexture(new Rect(bx, by, bw * Cocoon.StruggleBar, 16f), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
                if (!Plugin.ShowDeathTimer.Value) return;
                foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
                {
                    CocoonState s = kv.Value;
                    if (!s.Hung || !Cocoon.IsLocal(s.Player)) continue;
                    float left = Cocoon.SecondsLeft(s);
                    if (timerStyle == null)
                    {
                        timerStyle = new GUIStyle(GUI.skin.label);
                        timerStyle.alignment = TextAnchor.MiddleCenter;
                        timerStyle.fontStyle = FontStyle.Bold;
                        timerStyle.fontSize = 34;
                        shadowStyle = new GUIStyle(timerStyle);
                        shadowStyle.normal.textColor = Color.black;
                    }
                    timerStyle.normal.textColor = left <= 15f ? new Color(1f, 0.25f, 0.2f) : new Color(1f, 0.85f, 0.6f);
                    Rect r = new Rect(0f, Screen.height * 0.08f, Screen.width, 60f);
                    string text = "Time left: " + Cocoon.FormatTime(left);
                    GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), text, shadowStyle);
                    GUI.Label(r, text, timerStyle);
                    break;
                }
            }
            catch (Exception) { }
        }

        private float nextGhostSync;

        private void SyncGhosts()
        {
            if (Time.time < nextGhostSync) return;
            nextGhostSync = Time.time + 0.5f;
            foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
            {
                CocoonState s = kv.Value;
                if (!s.Hung || s.Player == null || !Cocoon.IsLocal(s.Player) || s.Player.deadBody == null) continue;
                Transform part = s.Player.deadBody.bodyParts != null && s.Player.deadBody.bodyParts.Length > 6 ? s.Player.deadBody.bodyParts[6].transform : null;
                if (part == null) continue;
                Vector3 a = s.Player.transform.position, b2 = part.position;
                a.y = 0f; b2.y = 0f;
                // Move the real body once per hang. (Moving it every time it is more than 3 m away made it land again and
                // again, with a thud each time, whenever the nearest safe spot was further than that from the cocoon.)
                if (s.Ghosted && Vector3.Distance(b2, s.GhostFor) < 6f) continue;
                if (Vector3.Distance(a, b2) > 3f)
                {
                    s.Ghosted = true; s.GhostFor = b2;
                    Plugin.Dbg("Moving the hidden body of player " + kv.Key + " to the cocoon (it was " + Vector3.Distance(a, b2).ToString("0.0") + " m away).");
                    Cocoon.MoveVictimUnderBody(s.Player, s.Spider);
                }
            }
        }

        private void Update()
        {
            SyncGhosts();
            // Emergency release: press F9 to free yourself if anything ever goes wrong.
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (Plugin.EmergencyReleaseKey.Value && keyboard != null && keyboard.f9Key.wasPressedThisFrame)
            {
                Cocoon.EmergencyRelease();
            }
            if (Plugin.EnableDebugSpawn.Value && keyboard != null)
            {
                bool dbgMod = !Plugin.DebugKeysNeedShift.Value || keyboard.shiftKey.isPressed;
                UnityEngine.InputSystem.Controls.KeyControl spawnKey = keyboard[Plugin.DebugSpawnKey.Value];
                if (spawnKey != null && spawnKey.wasPressedThisFrame && dbgMod) DebugSpawn.SpawnSpiderInFrontOfHost();
                UnityEngine.InputSystem.Controls.KeyControl brackenKey = keyboard[Plugin.DebugSpawnBrackenKey.Value];
                if (brackenKey != null && brackenKey.wasPressedThisFrame && dbgMod) DebugSpawn.SpawnInFrontOfHost(typeof(FlowermanAI), "Bracken");
                UnityEngine.InputSystem.Controls.KeyControl bugKey = keyboard[Plugin.DebugSpawnLootBugKey.Value];
                if (bugKey != null && bugKey.wasPressedThisFrame && dbgMod) DebugSpawn.SpawnInFrontOfHost(typeof(HoarderBugAI), "loot bug");
                UnityEngine.InputSystem.Controls.KeyControl collapseKey = keyboard[Plugin.DebugCollapseKey.Value];
                if (collapseKey != null && collapseKey.wasPressedThisFrame && dbgMod) Cocoon.DebugForceCollapse();
                UnityEngine.InputSystem.Controls.KeyControl clearKey = keyboard[Plugin.DebugClearEnemiesKey.Value];
                if (clearKey != null && clearKey.wasPressedThisFrame && dbgMod) DebugSpawn.ClearAllEnemies();
                UnityEngine.InputSystem.Controls.KeyControl fakeHitKey = keyboard[Plugin.DebugFakeHitKey.Value];
                if (fakeHitKey != null && fakeHitKey.wasPressedThisFrame && dbgMod) Cocoon.DebugFakeHit();
                if (keyboard[UnityEngine.InputSystem.Key.Numpad9].wasPressedThisFrame && dbgMod) CocoonRunner.Run(FleaWatch.Run());
                UnityEngine.InputSystem.Controls.KeyControl biteKey = keyboard[Plugin.DebugFakeBiteKey.Value];
                if (biteKey != null && biteKey.wasPressedThisFrame && dbgMod) Cocoon.DebugFakeBite();
            }
            Cocoon.RemoveDeadVictims();
            Cocoon.TickVenomCleanup();
            Cocoon.TickCorpseDump();
            Cocoon.TickStandIns();
            CocoonNet.EnsureRegistered();
            ConfigSync.Tick();
            SoundDebug.Tick();
            VoiceMuffle.Tick();
            OutdoorSync.Tick();
            DepthSync.Tick();
            Cocoon.WatchSpiders();
            Cocoon.TickStruggle();
        }

        private void LateUpdate()
        {
            Cocoon.UpdateCameras();
            Cocoon.UpdateRescue();
            try { BrackenThief.ArmPose(); } catch (Exception) { }
        }
    }

    // Testing helper: spawns a bunker spider a few metres in front of the host, the same way the
    // game spawns its own enemies. Only works for the host, and only inside a level.

    // Testing only (F5): watches the nearest snare flea for 40 s and logs every particle, line or trail effect on it
    // the moment it starts, stops or changes, so we can see what fires when it shoots up to the ceiling.
    internal static class FleaWatch
    {
        internal static System.Collections.IEnumerator Run()
        {
            Camera cam = Camera.main;
            Vector3 me = cam != null ? cam.transform.position : Vector3.zero;
            CentipedeAI best = null; float bd = 1e9f;
            foreach (CentipedeAI c in UnityEngine.Object.FindObjectsOfType<CentipedeAI>())
            {
                if (c == null || c.isEnemyDead) continue;
                float d = Vector3.Distance(c.transform.position, me);
                if (d < bd) { bd = d; best = c; }
            }
            if (best == null) { Plugin.Log.LogInfo("Flea watch (F5): no snare flea in the level."); yield break; }
            Plugin.Log.LogInfo("=== Flea watch (F5) started on a flea " + bd.ToString("F1") + " m away; 40 s ===");
            List<Component> fxList = new List<Component>();
            foreach (Component c in best.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                if (c is ParticleSystem || c is LineRenderer || c is TrailRenderer || c is Animator || c is AudioSource
                    || c.GetType().Name.IndexOf("VisualEffect", StringComparison.OrdinalIgnoreCase) >= 0) fxList.Add(c);
            }
            Component[] fx = fxList.ToArray();
            foreach (Component c in fx)
                Plugin.Log.LogInfo("Flea part: " + c.GetType().Name + " at " + Path(c.transform, best.transform));
            Dictionary<Component, string> last = new Dictionary<Component, string>();
            float end = Time.time + 40f;
            while (Time.time < end && best != null)
            {
                foreach (Component c in fx)
                {
                    if (c == null) continue;
                    string st = "";
                    ParticleSystem ps = c as ParticleSystem;
                    if (ps != null) st = (ps.isPlaying ? "playing" : "stopped") + " particles>0=" + (ps.particleCount > 0) + " objActive=" + ps.gameObject.activeInHierarchy;
                    LineRenderer lr = c as LineRenderer;
                    if (lr != null) st = "enabled=" + lr.enabled + " objActive=" + lr.gameObject.activeInHierarchy;
                    TrailRenderer tr = c as TrailRenderer;
                    if (tr != null) st = "emitting=" + tr.emitting + " objActive=" + tr.gameObject.activeInHierarchy;
                    AudioSource au = c as AudioSource;
                    if (au != null) st = (au.isPlaying ? "playing " + (au.clip != null ? au.clip.name : "?") : "silent");
                    Animator an = c as Animator;
                    if (an != null) { var info = an.GetCurrentAnimatorClipInfo(0); st = info.Length > 0 ? info[0].clip.name : "none"; }
                    if (st.Length == 0) st = "present";
                    string old;
                    if (!last.TryGetValue(c, out old) || old != st)
                    {
                        last[c] = st;
                        Plugin.Log.LogInfo("Flea fx t=" + (Time.time - (end - 40f)).ToString("F2") + " state=" + best.currentBehaviourStateIndex + " " + c.GetType().Name + " " + Path(c.transform, best.transform) + " -> " + st);
                    }
                }
                yield return null;
            }
            Plugin.Log.LogInfo("=== Flea watch (F5) finished ===");
        }

        private static string Path(Transform t, Transform root)
        {
            string s = t.name;
            while (t.parent != null && t != root) { t = t.parent; s = t.name + "/" + s; }
            return s;
        }
    }

    internal static class DebugSpawn
    {
        private static string PathOf(Transform t)
        {
            string s = t.name;
            int guard = 0;
            while (t.parent != null && guard++ < 8)
            {
                t = t.parent;
                s = t.name + "/" + s;
            }
            return s;
        }

        // Testing helper (F4): writes every Bracken's transform tree to the log (names, local positions
        // and rotations), flagging the ones that look like arm bones, so a carry arm pose can be built.
        internal static void DumpBrackenBones()
        {
            try
            {
                int n = 0;
                foreach (FlowermanAI b in UnityEngine.Object.FindObjectsOfType<FlowermanAI>())
                {
                    if (b == null) continue;
                    n++;
                    Plugin.Log.LogInfo("=== Bracken bones (F4) === carrying=" + (b.carryingPlayerBody) + " scale=" + b.transform.lossyScale.y);
                    Animator an = b.creatureAnimator;
                    if (an != null) Plugin.Log.LogInfo("Animator '" + an.name + "' layers=" + an.layerCount + " applyRootMotion=" + an.applyRootMotion);
                    foreach (Transform t in b.GetComponentsInChildren<Transform>(true))
                    {
                        if (t == null) continue;
                        string path = PathOf(t);
                        string low = t.name.ToLowerInvariant();
                        bool arm = low.Contains("arm") || low.Contains("hand") || low.Contains("shoulder") || low.Contains("clav")
                            || low.Contains("finger") || low.Contains("elbow") || low.Contains("wrist");
                        Plugin.Log.LogInfo("Bone " + (arm ? "[ARM] " : "") + path
                            + " localPos=" + t.localPosition.ToString("F2")
                            + " localRot=" + t.localEulerAngles.ToString("F0"));
                    }
                    foreach (SkinnedMeshRenderer r in b.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        if (r == null) continue;
                        Plugin.Log.LogInfo("SkinnedMesh '" + r.name + "' rootBone=" + (r.rootBone != null ? r.rootBone.name : "none")
                            + " bones=" + (r.bones != null ? r.bones.Length : 0));
                    }
                }
                if (n == 0) Plugin.Log.LogInfo("Bracken bones (F4): no Bracken in the level. Spawn one with F8 first.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Bracken bone dump failed: " + e.Message);
            }
        }

        // Testing helper (F7): writes to the log everything that could be drawing a floating label
        // near you, so we can see exactly what the "name badge" on a wrapped player really is.
        internal static void DumpLabelsNearby()
        {
            try
            {
                PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
                if (me == null) return;
                Vector3 here = me.transform.position;
                Plugin.Log.LogInfo("=== Label dump (F7) ===");
                foreach (PlayerControllerB p in StartOfRound.Instance.allPlayerScripts)
                {
                    if (p == null || p == me || !p.isPlayerControlled) continue;
                    Plugin.Log.LogInfo("Player " + p.playerClientId + " (" + p.playerUsername + ") cocooned=" + Cocoon.IsCocooned(p)
                        + " canvasActive=" + (p.usernameCanvas != null && p.usernameCanvas.gameObject.activeSelf)
                        + " alpha=" + (p.usernameAlpha != null ? p.usernameAlpha.alpha : -1f)
                        + " dist=" + Vector3.Distance(here, p.transform.position));
                }
                foreach (Canvas c in UnityEngine.Object.FindObjectsOfType<Canvas>())
                {
                    if (c == null || !c.gameObject.activeInHierarchy || c.renderMode != RenderMode.WorldSpace) continue;
                    float d = Vector3.Distance(here, c.transform.position);
                    if (d > 20f) continue;
                    Plugin.Log.LogInfo("WorldCanvas " + PathOf(c.transform) + " dist=" + d);
                }
                foreach (ScanNodeProperties n in UnityEngine.Object.FindObjectsOfType<ScanNodeProperties>())
                {
                    if (n == null || !n.gameObject.activeInHierarchy) continue;
                    float d = Vector3.Distance(here, n.transform.position);
                    if (d > 20f) continue;
                    Plugin.Log.LogInfo("ScanNode '" + n.headerText + "' / '" + n.subText + "' " + PathOf(n.transform) + " dist=" + d);
                }
                // What the spider's body is made of (tells us whether it can be faded out or only recoloured).
                foreach (SandSpiderAI sp in UnityEngine.Object.FindObjectsOfType<SandSpiderAI>())
                {
                    Renderer[] bodies = new Renderer[] { sp.spiderNormalMesh, sp.spiderSafeModeMesh };
                    foreach (Renderer r in bodies)
                    {
                        if (r == null) continue;
                        foreach (Material m in r.sharedMaterials)
                        {
                            if (m == null) continue;
                            Plugin.Log.LogInfo("Spider material '" + m.name + "' on " + r.name
                                + ": shader=" + (m.shader != null ? m.shader.name : "?")
                                + " queue=" + m.renderQueue
                                + " keywords=[" + string.Join(",", m.shaderKeywords) + "]"
                                + " baseColor=" + m.HasProperty("_BaseColor")
                                + " surfaceType=" + (m.HasProperty("_SurfaceType") ? m.GetFloat("_SurfaceType").ToString() : "none")
                                + " alphaCutoff=" + m.HasProperty("_AlphaCutoffEnable"));
                        }
                    }
                }
                Plugin.Log.LogInfo("=== End label dump ===");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Label dump failed: " + e);
            }
        }

        // Testing helper: KILLS every enemy that can die (so their death behaviour plays out as in a real fight);
        // enemies the game does not let die are removed from the level instead. Hung-up victims are left alone.
        internal static void ClearAllEnemies()
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer)
                {
                    Plugin.Log.LogWarning("Clear key: only the host can kill enemies.");
                    return;
                }
                RoundManager rm = RoundManager.Instance;
                int killed = 0, removed = 0;
                foreach (EnemyAI ai in UnityEngine.Object.FindObjectsOfType<EnemyAI>())
                {
                    if (ai == null || ai.isEnemyDead) continue;
                    bool canDie = ai.enemyType != null && (ai.enemyType.canDie || ai.enemyType.destroyOnDeath);
                    if (canDie)
                    {
                        if (ai.IsOwner) ai.KillEnemyOnOwnerClient();
                        else ai.KillEnemyServerRpc(false);
                        killed++;
                        continue;
                    }
                    if (rm != null && rm.SpawnedEnemies != null) rm.SpawnedEnemies.Remove(ai);
                    Unity.Netcode.NetworkObject no = ai.NetworkObject;
                    if (no != null && no.IsSpawned) no.Despawn(true);
                    else UnityEngine.Object.Destroy(ai.gameObject);
                    removed++;
                }
                Plugin.Log.LogInfo("Clear key: killed " + killed + " enemies, removed " + removed + " that cannot die.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Clear key failed: " + e);
            }
        }

        internal static void SpawnSpiderInFrontOfHost()
        {
            SpawnInFrontOfHost(typeof(SandSpiderAI), "bunker spider");
        }

        // Spawns the enemy whose AI component is aiType a few metres in front of the host.
        internal static void SpawnInFrontOfHost(Type aiType, string label)
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer)
                {
                    Plugin.Log.LogWarning("Spawn key: only the host can spawn a " + label + ".");
                    return;
                }
                PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
                if (me == null)
                {
                    Plugin.Log.LogWarning("Spawn key: no local player yet.");
                    return;
                }

                EnemyType enemyType = null;
                foreach (EnemyType t in Resources.FindObjectsOfTypeAll<EnemyType>())
                {
                    if (t != null && t.enemyPrefab != null && t.enemyPrefab.GetComponentInChildren(aiType) != null)
                    {
                        enemyType = t;
                        break;
                    }
                }
                if (enemyType == null)
                {
                    Plugin.Log.LogWarning("Spawn key: could not find the " + label + " type.");
                    return;
                }

                Vector3 pos = me.transform.position + me.transform.forward * 7f;
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 8f, UnityEngine.AI.NavMesh.AllAreas)) pos = hit.position;

                GameObject go = UnityEngine.Object.Instantiate(enemyType.enemyPrefab, pos, Quaternion.identity);
                Unity.Netcode.NetworkObject netObj = go.GetComponentInChildren<Unity.Netcode.NetworkObject>();
                if (netObj == null)
                {
                    UnityEngine.Object.Destroy(go);
                    Plugin.Log.LogWarning("Spawn key: the " + label + " prefab has no network object.");
                    return;
                }
                netObj.Spawn(true);
                EnemyAI ai = go.GetComponentInChildren<EnemyAI>();
                if (ai != null && RoundManager.Instance != null && RoundManager.Instance.SpawnedEnemies != null)
                {
                    RoundManager.Instance.SpawnedEnemies.Add(ai);
                }
                Plugin.Log.LogInfo("Spawn key: " + label + " spawned at " + pos + ".");
                // Indoor monsters ignore players who are outside, so for a test outdoors make this one an outdoor monster.
                // Go by where the player really is (the nodes nearest to them), since the inside/outside flag can be stale.
                if (ai != null && RoundManager.Instance != null)
                {
                    float dIn = Cocoon.NearestNodeDistance(RoundManager.Instance.insideAINodes, me.transform.position);
                    float dOut = Cocoon.NearestNodeDistance(RoundManager.Instance.outsideAINodes, me.transform.position);
                    if (dOut < dIn)
                    {
                        if (me.isInsideFactory)
                        {
                            Plugin.Log.LogInfo("Spawn key: you are standing outside but the game still had you flagged as inside; correcting the flag for this test.");
                            me.isInsideFactory = false;
                        }
                        CocoonRunner.Run(MakeOutdoors(ai));
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Spawn key failed: " + e);
            }
        }

        private static IEnumerator MakeOutdoors(EnemyAI ai)
        {
            yield return new WaitForSeconds(0.7f); // after the game's own start-up has picked the indoor nodes
            if (ai == null) yield break;
            try
            {
                RoundManager.Instance.GetOutsideAINodes();
                ai.isOutside = true;
                ai.allAINodes = RoundManager.Instance.outsideAINodesUnordered;
                SandSpiderAI sp = ai as SandSpiderAI;
                if (sp != null)
                {
                    // Its home point and patrol search were picked from the underground nodes at start-up: redo them up here.
                    if (sp.patrolHomeBase != null && sp.patrolHomeBase.inProgress) sp.StopSearch(sp.patrolHomeBase);
                    if (sp.currentSearch != null && sp.currentSearch.inProgress) sp.StopSearch(sp.currentSearch);
                    sp.homeNode = sp.ChooseClosestNodeToPosition(sp.transform.position, false, 2);
                }
                Plugin.Log.LogInfo("Spawn key: that monster now treats the outdoors as its home, so it can attack you out here.");
            }
            catch (Exception e) { Plugin.Log.LogWarning("Spawn key: could not make it an outdoor monster: " + e.Message); }
        }
    }

    // Remembers what we changed about a victim, so we can put it back exactly.
    internal class CocoonState
    {
        public PlayerControllerB Player;
        public SandSpiderAI Spider;
        public List<Renderer> Renderers = new List<Renderer>();
        public List<bool> WereEnabled = new List<bool>();

        // Colliders of a hidden remote player, switched off so nobody bumps into the invisible body.
        public List<Collider> Colliders = new List<Collider>();
        public List<bool> ColliderWasEnabled = new List<bool>();

        public bool Hung;
        public float StandInDeadline;
        public DeadBodyInfo StandIn; // the hanging stand-in body

        // Used to notice a spider that has stopped moving while carrying this victim.
        public float BeganAt;
        public bool DiagLogged;
        public float LostSince = -1f;
        public Vector3 LastSpiderPos;
        public float StillSince = -1f;
        public float HungAt;         // when the victim finished being hung up
        public float ExtraSeconds;   // extra life granted by a Bracken steal
        public FlowermanAI Thief;   // the Bracken carrying this victim off (null = nobody)
        public HoarderBugAI BugThief; // the loot bug carrying this victim off (null = nobody)
        public bool WasStolen;       // a Bracken or loot bug has carried this victim off at least once
        public bool BrackenDropped;  // a Bracken carried this victim to its lair and put them down: it will not pick them up again
        public EnemyAI Claim;        // the thief that has first call on this victim (null = nobody)
        public int ClaimRank;        // pecking order: Bracken 2 outranks loot bug 1
        public Transform BrackenDragPoint; // where a Bracken drags the cocoon (an empty point behind it)
        public float BugCarryStart;
        public Transform BugCarryPoint; // where a loot bug holds the cocoon (above its back)
        public bool NoRegrab;        // an outdoor spider that dropped the cocoon: it does not come back for it
        public bool WasSpooled;      // the body was already wrapped when it was dropped, so a regrab skips the wrap
        public bool NetWrapped;      // the stall net already made the spider wrap this victim
        public bool Dropped;         // the spider has dropped this victim at least once (it was hit while carrying them)
        public float LastDropAt = -100f;
        public float DropAppliedAt = -100f;
        public bool RegrabActive;    // a go-back-for-the-cocoon routine is already running for this victim
        public bool NoLiveSpider;    // wrapped with no living spider to carry them (venom took hold after the spider died): they just recover
        public float StruggleProgress; // 0..1, only used on the victim's own game
        public bool Buried;          // a trapdoor spider dragged this victim underground at DigSpot
        public bool Ghosted;         // the real body has been moved to the cocoon once already
        public Vector3 GhostFor;     // where the cocoon was when that happened
        public Vector3 DigSpot;      // where they went under (teammates dig them out here)
        public GameObject Marker;    // the patch of disturbed earth on that spot

        // Camera (only used on the victim's own game)
        public bool CameraActive;
        public bool CamInitialized;
        public Vector3 CamDir = Vector3.forward;
        public float OrbitYaw;       // how far the victim has turned the camera around their body
        public float OrbitPitch;     // ...and up or down
        public float ZoomMul = 1f;   // mouse wheel zoom
        public Vector3 SavedLocalPos;
        public Quaternion SavedLocalRot;
    }

    internal static class Cocoon
    {
        private const float CamDistance = 3.5f; // how far the camera sits from the body
        private const float CamHeight = 1.2f;   // and how high above it

        private static readonly Dictionary<int, CocoonState> Active = new Dictionary<int, CocoonState>();
        private static readonly Dictionary<int, float> ImmuneUntil = new Dictionary<int, float>();
        internal static Dictionary<int, CocoonState> ActiveStates { get { return Active; } }

        // The spider keeps these private, so we reach them through reflection.
        private static readonly MethodInfo GrabBodyMethod = AccessTools.Method(typeof(SandSpiderAI), "GrabBody");
        private static readonly MethodInfo CancelSpoolingMethod = AccessTools.Method(typeof(SandSpiderAI), "CancelSpoolingBody");
        private static readonly FieldInfo SpooledField = AccessTools.Field(typeof(SandSpiderAI), "spooledPlayerBody");
        private static readonly FieldInfo HeldBodyField = AccessTools.Field(typeof(SandSpiderAI), "currentlyHeldBody");
        private static readonly FieldInfo SpoolingField = AccessTools.Field(typeof(SandSpiderAI), "spoolingPlayerBody");
        private static readonly MethodInfo HangBodyMethod = AccessTools.Method(typeof(SandSpiderAI), "HangBodyFromCeiling");
        private static readonly FieldInfo StunField = AccessTools.Field(typeof(EnemyAI), "stunNormalizedTimer");
        private static readonly FieldInfo CoroutineField = AccessTools.Field(typeof(SandSpiderAI), "turnBodyIntoWebCoroutine");

        public static bool IsCocooned(PlayerControllerB player)
        {
            return player != null && Active.ContainsKey((int)player.playerClientId);
        }

        // ---- Venom: a bitten player stumbles about for a while before the wrap ----
        private static readonly HashSet<int> Venomed = new HashSet<int>();

        public static bool IsVenomed(PlayerControllerB p)
        {
            return p != null && Venomed.Contains((int)p.playerClientId);
        }

        private static readonly Dictionary<int, SandSpiderAI> VenomSpider = new Dictionary<int, SandSpiderAI>();

        // Testing only: the debug key makes the local player's running venom end at once.
        private static readonly HashSet<int> ForceCollapseIds = new HashSet<int>();
        internal static void DebugForceCollapse()
        {
            PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
            if (me == null) return;
            int id = (int)me.playerClientId;
            if (Venomed.Contains(id)) ForceCollapseIds.Add(id);
            else Plugin.Log.LogInfo("Debug key: you are not under venom right now, nothing to collapse.");
        }

        // Testing only: starts the venom on the local player as if the nearest spider had bitten them.
        internal static void DebugFakeBite()
        {
            PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
            if (me == null) return;
            SandSpiderAI best = null; float bd = 1e9f;
            foreach (SandSpiderAI sp in UnityEngine.Object.FindObjectsOfType<SandSpiderAI>())
            {
                if (sp == null || sp.isEnemyDead) continue;
                float d = Vector3.Distance(sp.transform.position, me.transform.position);
                if (d < bd) { bd = d; best = sp; }
            }
            if (best == null) { Plugin.Log.LogInfo("Debug key: no spider in the level to fake a bite with (press the spawn key first)."); return; }
            Plugin.Log.LogInfo("Debug key: faking a bite on player " + me.playerClientId + " from the spider " + bd.ToString("0") + " m away.");
            // Same message a real bite sends, so every player's game (the host's too) learns about it.
            try { best.HitPlayerServerRpc((int)me.playerClientId); }
            catch (Exception e) { Plugin.Log.LogWarning("Debug bite: could not send the bite message (" + e.Message + "); venom on this game only."); BeginVenom(me, best); }
        }

        // True while this spider has a bitten player whose venom is still working (not yet collapsed).
        public static bool SpiderVenomActive(SandSpiderAI spider)
        {
            if (spider == null) return false;
            foreach (KeyValuePair<int, SandSpiderAI> kv in VenomSpider)
                if (kv.Value == spider && Venomed.Contains(kv.Key) && !Active.ContainsKey(kv.Key)) return true;
            return false;
        }

        // True while this spider still has a victim: bitten and stumbling, wrapped, hanging or buried.
        public static bool SpiderHasVictim(SandSpiderAI spider)
        {
            if (spider == null) return false;
            foreach (KeyValuePair<int, SandSpiderAI> kv in VenomSpider)
                if (kv.Value == spider && (Venomed.Contains(kv.Key) || Active.ContainsKey(kv.Key))) return true;
            return false;
        }

        // the armed, still-stumbling player this spider just bit (null if none).
        public static PlayerControllerB ArmedVictimOf(SandSpiderAI spider, out string why)
        {
            why = "no bitten player recorded for this spider";
            foreach (KeyValuePair<int, SandSpiderAI> kv in VenomSpider)
            {
                if (kv.Value != spider || !Venomed.Contains(kv.Key) || Active.ContainsKey(kv.Key)) continue;
                PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
                if (kv.Key < 0 || kv.Key >= players.Length) continue;
                PlayerControllerB v = players[kv.Key];
                if (v == null || v.isPlayerDead) continue;
                if (v.isHoldingObject && v.currentlyHeldObjectServer != null && v.currentlyHeldObjectServer.itemProperties.isDefensiveWeapon) { why = "armed"; return v; }
                why = "bitten player " + kv.Key + " is not holding a defensive weapon (holding=" + (v.isHoldingObject && v.currentlyHeldObjectServer != null ? v.currentlyHeldObjectServer.itemProperties.itemName : "nothing") + ")";
            }
            return null;
        }

        // Runs on every player's game when the spider bites. With venom off, wraps straight away.
        public static void BeginVenom(PlayerControllerB victim, SandSpiderAI spider)
        {
            if (victim == null || victim.isPlayerDead) return;
            float secs = Plugin.VenomSeconds.Value;
            int id = (int)victim.playerClientId;
            if (secs < 0.5f) { Begin(victim, spider); return; }
            if (Active.ContainsKey(id) || Venomed.Contains(id)) return;
            // A trapdoor spider deals with one victim at a time: a second bite while it is busy is ignored.
            if (Trapdoor.Applies(spider) && SpiderHasVictim(spider))
            {
                Plugin.Log.LogInfo("Trapdoor spider is busy with another victim: the bite on player " + id + " is ignored.");
                return;
            }
            Venomed.Add(id);
            VenomSpider[id] = spider;
            if (IsLocal(victim)) VenomEpoch++; // a new bite on ME cancels my fade-out (a bite on someone else must not: it left my red screen on)
            Plugin.Log.LogInfo("Player " + id + " bitten: venom for " + secs.ToString("0") + "s.");
            CocoonRunner.Run(VenomRoutine(victim, spider, secs));
        }

        // ---- heartbeat that speeds up while the venom takes hold (uses a clip the game already has) ----
        private static AudioClip heartClip, heartClip2;
        private static int heartPhase;
        private static bool heartSearched;
        private static AudioSource heartSource;
        private static float nextHeartAt;
        private static bool heartLogged;

        private static void VenomHeartbeat(PlayerControllerB victim, float k)
        {
            try
            {
                if (!Plugin.VenomHeartbeat.Value) return;
                if (!heartSearched)
                {
                    heartSearched = true;
                    foreach (AudioClip c in Resources.FindObjectsOfTypeAll<AudioClip>())
                    {
                        if (c == null) continue;
                        string n = c.name.ToLowerInvariant();
                        if (n.Contains("heart") || n.Contains("pulse"))
                        {
                            Plugin.Verbose("Venom heartbeat: found clip '" + c.name + "' (" + c.length.ToString("0.00") + "s).");
                            if (n == "heartbeat") heartClip = c;
                            else if (n == "heartbeat2") heartClip2 = c;
                            else if (heartClip == null && n.Contains("heartbeat") && !n.Contains("drop") && !n.EndsWith("2")) heartClip = c;
                        }
                    }
                    if (heartClip == null) Plugin.Log.LogWarning("Venom heartbeat: no clip with 'heart' in its name was found in the game.");
                }
                if (heartClip == null || Time.time < nextHeartAt) return;
                if (heartSource == null)
                {
                    GameObject go = new GameObject("HungToDryHeartbeat");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    heartSource = go.AddComponent<AudioSource>();
                    heartSource.spatialBlend = 0f; // plain 2D sound: only the bitten player hears it
                    heartSource.playOnAwake = false;
                }
                if (heartClip.loadState == AudioDataLoadState.Unloaded) heartClip.LoadAudioData();
                if (heartClip2 != null && heartClip2.loadState == AudioDataLoadState.Unloaded) heartClip2.LoadAudioData();
                // The player's own status-effect speaker goes through the game's normal audio setup; ours is the fallback.
                AudioSource src = (victim != null && victim.statusEffectAudio != null) ? victim.statusEffectAudio : heartSource;
                float vol = Mathf.Lerp(0.25f, 1f, Mathf.Pow(Mathf.Clamp01(k), 1.2f)) * Plugin.VenomHeartbeatVolume.Value; // soft at first, full by the end
                // "lub-dub" like the Mansion's heart item: HeartBeat, then HeartBeat2 a moment later, then a pause.
                bool dub = heartClip2 != null && heartPhase == 1;
                src.PlayOneShot(dub ? heartClip2 : heartClip, vol);
                if (!heartLogged)
                {
                    heartLogged = true;
                    Plugin.Dbg("Venom heartbeat: playing '" + heartClip.name + "' through '" + src.gameObject.name + "' (volume " + src.volume.ToString("0.00") + ", mute=" + src.mute + ", loadState=" + heartClip.loadState + ").");
                }
                if (heartClip2 != null)
                {
                    nextHeartAt = Time.time + (dub ? Mathf.Lerp(0.7f, 0.25f, k) : Mathf.Lerp(0.5f, 0.3f, k));
                    heartPhase = dub ? 0 : 1;
                }
                else nextHeartAt = Time.time + Mathf.Max(heartClip.length, Mathf.Lerp(1.2f, 0.5f, k));
            }
            catch (Exception) { }
        }

        private static bool swayLogged;
        private static readonly FieldInfo CameraUpField = AccessTools.Field(typeof(PlayerControllerB), "cameraUp");

        // The last few seconds of the venom: the player cannot hold still. The view drifts left and right and up and down,
        // building up until they drop. The body is no longer pushed sideways.
        private static void ApplyVenomSway(PlayerControllerB victim, float secondsLeft, float elapsed, float total)
        {
            try
            {
                float startAfter = Plugin.VenomSwayStartsAfter.Value;
                float span = startAfter >= 0f ? Mathf.Max(0.5f, total - startAfter) : Plugin.VenomSwaySeconds.Value;
                if (span <= 0.05f || secondsLeft > span || victim == null || victim.isPlayerDead) return;
                if (!swayLogged) { swayLogged = true; Plugin.Log.LogInfo("Venom sway begins (" + secondsLeft.ToString("0.0") + " s of venom left, strength " + Plugin.VenomSwayStrength.Value.ToString("0.0") + ")."); }
                float build = Mathf.Clamp01(1f - secondsLeft / span);          // 0 at the start of the sway, 1 at the collapse
                build = build * build * (3f - 2f * build);                      // eases in
                float power = build * Plugin.VenomSwayStrength.Value;
                float t = Time.time;
                float dt = Time.deltaTime;
                float yawRate = (Mathf.Sin(t * 1.7f) + 0.6f * Mathf.Sin(t * 3.1f + 1f)) * 60f * power;      // degrees per second
                float pitchRate = (Mathf.Sin(t * 2.3f + 2f) + 0.5f * Mathf.Sin(t * 4.1f)) * 30f * power;
                victim.transform.Rotate(0f, yawRate * dt, 0f, Space.World);
                if (CameraUpField != null)
                {
                    float up = (float)CameraUpField.GetValue(victim);
                    CameraUpField.SetValue(victim, Mathf.Clamp(up + pitchRate * dt, -60f, 60f));
                }
            }
            catch (Exception) { }
        }

        private static void ApplyVenomLook(PlayerControllerB victim, float k)
        {
            VenomHeartbeat(victim, k);
            VenomLookOn = true;
            victim.overridePoisonValue = true;
            victim.poison = Mathf.Lerp(victim.poison, Mathf.Lerp(0.2f, 0.8f, k), Time.deltaTime * 2f);
            if (HUDManager.Instance != null) HUDManager.Instance.cadaverFilter = Mathf.Lerp(0f, 0.6f, k);
        }

        private static int VenomEpoch;
        public static float VenomTimerLeft = -1f; // testing aid: seconds of venom left for the local player (shown only with DebugLogging)

        // The spider died while its venom was in the player: the venom has already taken hold, so they still
        // drop at full strength. They collapse the same way a wrapped victim does (a ragdoll of them falls to
        // the ground where they stand, seen by everyone), lie there a few seconds, then get up and slowly recover.
        // Runs on every player's game.
        private static IEnumerator VenomCollapse(PlayerControllerB victim, SandSpiderAI spider)
        {
            int id = (int)victim.playerClientId;
            bool local = IsLocal(victim);
            Plugin.Log.LogInfo("Player " + id + ": the spider died but the venom has taken hold; collapsing, then recovering.");
            SandSpiderAI heir = FindHeir(victim, spider);
            if (heir != null)
            {
                // Another spider is close enough: it takes over where the dead one left off.
                Plugin.Log.LogInfo("Player " + id + ": another spider is close by and takes over.");
                Begin(victim, heir); // the heir's owner walks it over, grabs and wraps as usual
                if (local && victim != null)
                {
                    VenomEpoch++;
                    victim.overridePoisonValue = false;
                    victim.poison = 0f;
                    if (HUDManager.Instance != null) HUDManager.Instance.cadaverFilter = 0f;
                }
                yield break;
            }
            // A dead spider grabs nothing; a living one that cannot reach them must not either, so it gets no part in it.
            Begin(victim, (spider != null && spider.isEnemyDead) ? spider : null); // nothing grabs the body, no death timer
            CocoonState mine;
            Active.TryGetValue(id, out mine);
            int epoch = local ? ++VenomEpoch : VenomEpoch;
            float t = 0f;
            while (t < 8f && victim != null && !victim.isPlayerDead)
            {
                if (local) ApplyVenomLook(victim, 1f);
                t += Time.deltaTime;
                yield return null;
            }
            CocoonState now;
            if (mine != null && Active.TryGetValue(id, out now) && now == mine && !mine.Hung) End(id);
            if (local && victim != null && !victim.isPlayerDead && epoch == VenomEpoch)
                yield return VenomFade(victim, epoch, 10f);
        }

        // Spiders never leave the facility (or enter it): one is only any use if it is on the same side as the player.
        private static bool SpiderCanReach(SandSpiderAI spider, PlayerControllerB victim)
        {
            return spider != null && victim != null && SpiderOutdoors(spider) == PositionOutdoors(victim.transform.position);
        }

        // Can the spider really walk to this spot? A complete route on the navigation mesh, no longer than VenomReachMeters.
        internal static bool SpiderCanWalkTo(SandSpiderAI spider, Vector3 goal)
        {
            try
            {
                UnityEngine.AI.NavMeshHit a, b;
                if (!UnityEngine.AI.NavMesh.SamplePosition(spider.transform.position, out a, 3f, UnityEngine.AI.NavMesh.AllAreas)) return true; // cannot tell: do not interfere
                if (!UnityEngine.AI.NavMesh.SamplePosition(goal, out b, 4f, UnityEngine.AI.NavMesh.AllAreas)) return false;
                UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
                if (!UnityEngine.AI.NavMesh.CalculatePath(a.position, b.position, UnityEngine.AI.NavMesh.AllAreas, path)) return false;
                if (path.status != UnityEngine.AI.NavMeshPathStatus.PathComplete) return false;
                float max = Plugin.VenomReachMeters.Value;
                float len = 0f; Vector3[] c = path.corners;
                for (int i = 1; i < c.Length; i++) len += Vector3.Distance(c[i - 1], c[i]);
                Plugin.Dbg("Spider reach check: straight distance " + Vector3.Distance(spider.transform.position, goal).ToString("0.0") + " m, walking route " + len.ToString("0.0") + " m (limit " + max.ToString("0") + " m).");
                if (max > 0.5f && len > max) return false;
                return true;
            }
            catch (Exception) { return true; }
        }

        // The spider cannot get to the victim: they are let go where they lie and the spider goes back to patrolling.
        private static void GiveUp(SandSpiderAI spider, PlayerControllerB victim, string why)
        {
            try
            {
                Plugin.Log.LogInfo("Spider gives up on player " + (victim != null ? victim.playerClientId.ToString() : "?") + " (" + why + "): it goes back to patrolling.");
                if (SpiderAvoidWeaponPatch.AvoidSpider == spider) SpiderAvoidWeaponPatch.AvoidUntil = 0f;
                ResetSpiderToPatrol(spider);
                if (victim != null) CocoonNet.RequestRelease((int)victim.playerClientId);
            }
            catch (Exception) { }
        }

        internal static void ResetSpiderToPatrol(SandSpiderAI spider)
        {
            if (spider == null || spider.isEnemyDead || !spider.IsOwner) return;
            spider.targetPlayer = null;
            if (WatchFromDistanceField != null) WatchFromDistanceField.SetValue(spider, false);
            spider.SwitchToBehaviourState(0);
            Ceiling.CS cs = Ceiling.Get(spider, false);
            if (cs != null) { cs.M = Ceiling.Mode.Walk; cs.HasNode = false; cs.Frozen = false; cs.Since = Time.time; }
        }

        private static CauseOfDeath DeathCause()
        {
            return Plugin.SuffocationDeath.Value ? CauseOfDeath.Suffocation : CauseOfDeath.Mauling;
        }

        // Nearest living, idle spider within VenomHandoverMeters of the victim (other than the dead biter).
        // Every game picks by the same rule (nearest), so they all agree.
        private static SandSpiderAI FindHeir(PlayerControllerB victim, SandSpiderAI dead)
        {
            float range = Plugin.VenomHandoverMeters.Value;
            if (range <= 0.5f || victim == null) return null;
            SandSpiderAI best = null;
            float bestD = range;
            try
            {
                foreach (SandSpiderAI sp in UnityEngine.Object.FindObjectsOfType<SandSpiderAI>())
                {
                    if (sp == null || sp == dead || sp.isEnemyDead || !SpiderCanReach(sp, victim)) continue;
                    if (HeldBodyField != null && HeldBodyField.GetValue(sp) != null) continue; // already busy with someone
                    float d = Vector3.Distance(sp.transform.position, victim.transform.position);
                    if (d < bestD) { bestD = d; best = sp; }
                }
            }
            catch (Exception e) { Plugin.Log.LogError("Looking for another spider failed: " + e); }
            return best;
        }

        // The venom fades out over a few seconds instead of snapping off (e.g. when the spider is killed).
        private static int FadesRunning;
        private static bool VenomLookOn; // our own venom look is the one on (the game's Cadaver Bloom also uses overridePoisonValue: never touch that)
        private static float cleanupSince = -1f;

        // Safety net: if the venom look (our red screen / poison override) is still on for the local player with no venom,
        // no cocoon and no fade running for 2 s, switch it off.
        public static void TickVenomCleanup()
        {
            try
            {
                GameNetworkManager gnm = GameNetworkManager.Instance;
                PlayerControllerB me = gnm != null ? gnm.localPlayerController : null;
                if (me != null && !me.overridePoisonValue) VenomLookOn = false;
                if (me == null || !VenomLookOn || !me.overridePoisonValue || FadesRunning > 0 || Venomed.Contains((int)me.playerClientId) || Active.ContainsKey((int)me.playerClientId)) { cleanupSince = -1f; return; }
                if (cleanupSince < 0f) { cleanupSince = Time.time; return; }
                if (Time.time - cleanupSince < 2f) return;
                cleanupSince = -1f;
                VenomLookOn = false;
                me.overridePoisonValue = false;
                me.poison = 0f;
                if (HUDManager.Instance != null) HUDManager.Instance.cadaverFilter = 0f;
                Plugin.Log.LogInfo("The venom's red screen was still on with no venom or cocoon: switched it off.");
            }
            catch (Exception) { }
        }

        private static IEnumerator VenomFade(PlayerControllerB victim, int epoch, float duration)
        {
            FadesRunning++;
            try { yield return VenomFadeInner(victim, epoch, duration); }
            finally { FadesRunning--; }
        }

        private static IEnumerator VenomFadeInner(PlayerControllerB victim, int epoch, float duration)
        {
            float startPoison = victim != null ? victim.poison : 0f;
            float startFilter = HUDManager.Instance != null ? HUDManager.Instance.cadaverFilter : 0f;
            float t = 0f;
            while (t < duration && victim != null && !victim.isPlayerDead && epoch == VenomEpoch)
            {
                float k = t / duration;
                VenomLookOn = true;
                victim.overridePoisonValue = true;
                victim.poison = Mathf.Lerp(startPoison, 0f, k);
                if (HUDManager.Instance != null) HUDManager.Instance.cadaverFilter = Mathf.Lerp(startFilter, 0f, k);
                t += Time.deltaTime;
                yield return null;
            }
            // Only clean up if nothing newer (a fresh bite) has taken over.
            if (epoch == VenomEpoch && victim != null)
            {
                victim.overridePoisonValue = false;
                victim.poison = 0f;
                if (HUDManager.Instance != null) HUDManager.Instance.cadaverFilter = 0f;
            }
        }

        private static IEnumerator VenomRoutine(PlayerControllerB victim, SandSpiderAI spider, float secs)
        {
            int id = (int)victim.playerClientId;
            bool local = IsLocal(victim);
            bool finished = false;
            bool spiderDied = false;
            bool caught = false;
            string caughtBy = null;
            try
            {
                if (local)
                {
                    try { HUDManager.Instance.DisplayStatusEffect("VENOM DETECTED!\nMOTOR FUNCTION FAILING"); } catch (Exception) { }
                }
                swayLogged = false;
                float t = 0f;
                while (t < secs)
                {
                    if (victim == null || victim.isPlayerDead) yield break;
                    if (ForceCollapseIds.Remove(id)) { Plugin.Log.LogInfo("Debug key: collapsing player " + id + " from the venom now."); break; }
                    if (spider == null || spider.isEnemyDead) spiderDied = true; // the venom is already in them: it still runs its course
                    if (local) { ApplyVenomLook(victim, Mathf.Clamp01(t / secs)); VenomTimerLeft = secs - t; ApplyVenomSway(victim, secs - t, t, secs); }
                    t += Time.deltaTime;
                    yield return null;
                }
                // The player collapses where they stand; the spider walks over to them afterwards.
                if (spider == null || spider.isEnemyDead) spiderDied = true;
                else if (!SpiderCanReach(spider, victim))
                {
                    // The victim ran out of (or into) the facility during the venom: the spider cannot follow.
                    spiderDied = true;
                    Plugin.Log.LogInfo("Player " + id + ": the spider cannot reach them (inside/outside the facility).");
                    ResetSpiderToPatrol(spider);
                }
                else if (victim != null && CaughtByOtherMonster(victim, out caughtBy))
                {
                    // Another monster got them first: it takes priority, no collapse and no wrap.
                    Plugin.Log.LogInfo("Player " + id + " is already caught by a " + caughtBy + ": it takes priority over the venom and the wrap; the spider goes back to patrolling.");
                    ResetSpiderToPatrol(spider);
                    caught = true;
                }
                else if (!SpiderCanWalkTo(spider, victim.transform.position))
                {
                    // Too far, or no walkable route (across a gap, behind a barrier): they simply collapse and recover where they are.
                    spiderDied = true;
                    Plugin.Log.LogInfo("Player " + id + ": the spider cannot walk to them (no route, or further than VenomReachMeters); it goes back to patrolling.");
                    ResetSpiderToPatrol(spider);
                }
                finished = victim != null && !victim.isPlayerDead && !spiderDied && !caught;
            }
            finally
            {
                Venomed.Remove(id);
                if (local) VenomTimerLeft = -1f;
                if (spiderDied && victim != null && !victim.isPlayerDead) CocoonRunner.Run(VenomCollapse(victim, spider)); // killed the spider: drops anyway
                if (!finished) Plugin.Log.LogInfo("Player " + id + ": the venom wore off (spider dead or player gone).");
                if (local && victim != null)
                {
                    if (spiderDied && !victim.isPlayerDead) { /* the collapse routine keeps the effect going */ }
                    else if (!finished && !victim.isPlayerDead) CocoonRunner.Run(VenomFade(victim, ++VenomEpoch, 6f)); // wears off slowly
                    else
                    {
                        victim.overridePoisonValue = false;
                        victim.poison = 0f;
                        if (HUDManager.Instance != null) HUDManager.Instance.cadaverFilter = 0f;
                    }
                }
            }
            if (finished) Begin(victim, spider);
        }

        // Is another monster (Masked, Bracken, Bush Wolf, Puma...) already killing this player? Such a monster takes priority over the
        // venom and the wrap: being caught like that means death, as in the unmodified game.
        private static readonly Dictionary<Type, FieldInfo> SpecialAnimField = new Dictionary<Type, FieldInfo>();
        internal static bool CaughtByOtherMonster(PlayerControllerB v, out string by)
        {
            by = null;
            try
            {
                if (v == null) return false;
                if (v.inAnimationWithEnemy != null && !(v.inAnimationWithEnemy is SandSpiderAI)) { by = v.inAnimationWithEnemy.GetType().Name; return true; }
                foreach (EnemyAI e in UnityEngine.Object.FindObjectsOfType<EnemyAI>())
                {
                    if (e == null || e is SandSpiderAI || e.isEnemyDead) continue;
                    SandWormAI worm = e as SandWormAI;
                    if (worm != null)
                    {
                        // The Earth Leviathan has no grab animation: anything touching it while it is out is eaten at once.
                        if ((worm.emerged || worm.inEmergingState) && v.playerCollider != null)
                        {
                            // Rising out of the ground counts too: its colliders are not in place yet, so a worm that is
                            // coming up within a few metres of the victim already has them.
                            Vector3 flat = worm.transform.position - v.transform.position; flat.y = 0f;
                            if (worm.inEmergingState && flat.magnitude < 8f) { by = "SandWormAI"; return true; }
                            foreach (Collider wc in worm.GetComponentsInChildren<Collider>())
                                if (wc != null && wc.enabled && wc.bounds.Intersects(v.playerCollider.bounds)) { by = "SandWormAI"; return true; }
                        }
                        continue;
                    }
                    Type t = e.GetType();
                    FieldInfo f;
                    if (!SpecialAnimField.TryGetValue(t, out f)) { f = AccessTools.Field(t, "inSpecialAnimationWithPlayer"); SpecialAnimField[t] = f; }
                    if (f != null && f.GetValue(e) as PlayerControllerB == v) { by = t.Name; return true; }
                }
            }
            catch (Exception) { }
            return false;
        }

        // True for a few seconds after a release, so the victim has time to get away.
        public static bool IsImmune(PlayerControllerB player)
        {
            float until;
            return player != null
                && ImmuneUntil.TryGetValue((int)player.playerClientId, out until)
                && Time.time < until;
        }

        // A freed player who attacks a monster loses their immunity at once.
        public static void EndImmunityOnAttack(PlayerControllerB attacker)
        {
            try
            {
                if (attacker == null || !Plugin.ImmunityEndsOnAttack.Value || !IsImmune(attacker)) return;
                ImmuneUntil[(int)attacker.playerClientId] = Time.time;
                Plugin.Log.LogInfo("Player " + attacker.playerClientId + " attacked a monster while immune: immunity ended.");
            }
            catch (Exception) { }
        }

        internal static bool IsLocal(PlayerControllerB p)
        {
            return p != null && p == GameNetworkManager.Instance.localPlayerController;
        }

        // ------------------------------------------------------------------
        // Catching the victim
        // ------------------------------------------------------------------

        // Does exactly what the spider's own code does when it reaches a dead body:
        // grab it, tell everyone, switch to "carry it home". Only the spider's owner
        // runs its AI, so only the owner may do this.
        private static void MakeSpiderGrab(SandSpiderAI spider, PlayerControllerB victim, bool alreadyWrapped = false)
        {
            if (spider == null || spider.isEnemyDead || !spider.IsOwner) return;
            DeadBodyInfo body = victim.deadBody;
            if (body == null) return;

            // The game's own "stop wrapping" code stops the wrapping routine but forgets to clear its
            // reference to it, and the spider then never wraps (or hangs) a body again. Clear it,
            // unless the spider is genuinely in the middle of wrapping.
            if (!(bool)SpoolingField.GetValue(spider)) CoroutineField.SetValue(spider, null);

            SpooledField.SetValue(spider, false);
            GrabBodyMethod.Invoke(spider, new object[] { body });
            spider.GrabBodyServerRpc((int)victim.playerClientId);
            spider.targetPlayer = null; // the frozen player is not a chase target any more
            spider.SwitchToBehaviourState(1);
            Plugin.Dbg("Grab: spider=" + spider.transform.position.ToString("F1") + " player=" + victim.transform.position.ToString("F1") + " body=" + (body.bodyParts != null && body.bodyParts.Length > 5 ? body.bodyParts[5].transform.position.ToString("F1") : "?"));
            Plugin.Log.LogInfo("Spider grabbed player " + victim.playerClientId + "'s stand-in body.");
            if (alreadyWrapped)
            {
                // Picking up a cocoon that is already wrapped: no wrap animation, straight on to hanging it.
                SpooledField.SetValue(spider, true);
                Plugin.Log.LogInfo("Spider picks the wrapped cocoon back up (no second wrap).");
            }
            else if (Plugin.WrapOnCatch.Value) StartWrap(spider, "at the catch");
        }

        private static readonly FieldInfo WatchFromDistanceField = AccessTools.Field(typeof(SandSpiderAI), "watchFromDistance");
        private static readonly FieldInfo ChaseTimerField = AccessTools.Field(typeof(SandSpiderAI), "chaseTimer");

        // The victim has collapsed where they stand and the spider is some way off: let it walk over to the
        // body and grab it there, instead of the body jumping to the spider. Only the spider's owner steers it.
        private static IEnumerator SpiderApproach(CocoonState state)
        {
            SandSpiderAI spider = state.Spider;
            PlayerControllerB victim = state.Player;
            float t = 0f;
            int cannotWalk = 0;
            Vector3 lastGoal = victim.transform.position;
            Plugin.Log.LogInfo("Spider walks over to the collapsed player.");
            while (t < 20f)
            {
                if (spider == null || spider.isEnemyDead || victim == null || victim.isPlayerDead || !Active.ContainsKey((int)victim.playerClientId)) yield break;
                // After a drop the cocoon lies where it was dropped, not where the player first collapsed.
                Vector3 goal = victim.transform.position;
                bool toBody = false;
                try
                {
                    if (state.Dropped && victim.deadBody != null && victim.deadBody.bodyParts != null && victim.deadBody.bodyParts.Length > 6)
                    { goal = victim.deadBody.bodyParts[6].transform.position; toBody = true; }
                }
                catch (Exception) { }
                lastGoal = goal;
                if (Vector3.Distance(spider.transform.position, goal) < 3.2f) break;
                if (!toBody)
                {
                    cannotWalk = SpiderCanWalkTo(spider, goal) ? 0 : cannotWalk + 1;
                    if (cannotWalk >= 3) { GiveUp(spider, victim, "no walkable route to them"); yield break; }
                }
                // Keep it chasing this player (the game would otherwise give up or go back to its lair).
                if (spider.currentBehaviourStateIndex != 2) spider.SwitchToBehaviourState(2);
                spider.targetPlayer = victim;
                if (WatchFromDistanceField != null) WatchFromDistanceField.SetValue(spider, false);
                if (ChaseTimerField != null) ChaseTimerField.SetValue(spider, 12.5f);
                if (toBody)
                {
                    // walk to the cocoon itself (the per-frame patch keeps the game from steering it back to the player)
                    spider.SetDestinationToPosition(goal, true);
                    SpiderAvoidWeaponPatch.AvoidSpider = spider;
                    SpiderAvoidWeaponPatch.AvoidDest = goal;
                    SpiderAvoidWeaponPatch.AvoidSpeed = 5f;
                    SpiderAvoidWeaponPatch.AvoidUntil = Time.time + 0.6f;
                }
                t += 0.2f;
                yield return new WaitForSeconds(0.2f);
            }
            if (SpiderAvoidWeaponPatch.AvoidSpider == spider) SpiderAvoidWeaponPatch.AvoidUntil = 0f;
            if (spider != null && !spider.isEnemyDead && Vector3.Distance(spider.transform.position, lastGoal) > 6f)
            {
                GiveUp(spider, victim, "it could not get to them in time");
                yield break;
            }
            MakeSpiderGrab(spider, victim, state.Dropped && state.WasSpooled);
        }

        // Runs on EVERY player's game when a spider hit lands.
        public static void Begin(PlayerControllerB victim, SandSpiderAI spider)
        {
            if (victim == null || victim.isPlayerDead) return;
            int id = (int)victim.playerClientId;
            if (Active.ContainsKey(id)) return; // already cocooned (also guards the host running this twice)
            string otherMonster;
            if (CaughtByOtherMonster(victim, out otherMonster))
            {
                Plugin.Log.LogInfo("Player " + id + " is already caught by a " + otherMonster + ": no wrap.");
                ResetSpiderToPatrol(spider);
                return;
            }

            CocoonState state = new CocoonState { Player = victim, Spider = spider, BeganAt = Time.time, NoLiveSpider = (spider == null || spider.isEnemyDead) };
            Active[id] = state;
            MaskCancel.Run(victim);

            // Diagnostic for the "caught again straight after a rescue" question.
            float lastImmuneUntil;
            if (ImmuneUntil.TryGetValue(id, out lastImmuneUntil))
            {
                Plugin.Dbg("Player " + id + " caught again; their immunity ended "
                    + (Time.time - lastImmuneUntil).ToString("F1") + "s ago (negative = they were still immune).");
            }
            bool isLocal = IsLocal(victim);
            if (isLocal) WebIgnorePatch.LeaveAllWebs(victim); // webs often catch you just before the wrap

            try
            {
                // 1. Safety-net timer FIRST, so nothing can leave a player stuck.
                CocoonRunner.Run(FailsafeRelease(id, state, Plugin.HoldSeconds.Value));

                // 2. Hide the real player model (and first-person arms), remembering what was visible.
                // This covers the body AND loose pieces such as the chest ID badge ("intern"), which is a
                // plain mesh. Anything belonging to an item the player holds is left alone.
                foreach (Renderer r in victim.gameObject.GetComponentsInChildren<Renderer>(true))
                {
                    if (r == null) continue;
                    if (!(r is SkinnedMeshRenderer) && !(r is MeshRenderer)) continue;
                    if (r.GetComponentInParent<GrabbableObject>() != null) continue;
                    state.Renderers.Add(r);
                    state.WereEnabled.Add(r.enabled);
                    r.enabled = false;
                }

                // 2b. On every other player's game, make the invisible body non-solid so it can't be bumped into.
                //     (The victim's own game keeps its collider: switching it off would break their movement code.)
                if (!isLocal)
                {
                    AddCollider(state, victim.thisController);
                    AddCollider(state, victim.playerCollider);
                    // ...and any other collider on the player's body (the game gives the model several).
                    foreach (Collider c in victim.gameObject.GetComponentsInChildren<Collider>(true))
                    {
                        if (c == null || c.GetComponentInParent<GrabbableObject>() != null) continue;
                        AddCollider(state, c);
                    }
                    DisableColliders(state);
                    Plugin.Dbg("Player " + id + ": switched off " + state.Colliders.Count + " collider(s) so nobody bumps into the hidden body.");
                }

                // 3. Paralyse. Only the victim's own game controls their input.
                if (isLocal)
                {
                    victim.disableMoveInput = true;
                    victim.disableLookInput = true;

                    if (Plugin.DropHeldItems.Value)
                    {
                        victim.DropAllHeldItemsAndSync(
                            victim.transform.position,
                            victim.localItemHolder.position,
                            victim.localItemHolder.eulerAngles,
                            victim.playerEye.position,
                            victim.playerEye.eulerAngles);
                    }
                }

                // 4. Create the stand-in ragdoll the spider will carry.
                //    This is the same call the game makes when a player dies, minus the dying.
                victim.SpawnDeadBody(id, Vector3.zero, (int)DeathCause(), victim, 0);
                state.StandIn = victim.deadBody;
                if (Plugin.DebugLogging.Value)
                {
                    Vector3 sp = state.StandIn != null && state.StandIn.bodyParts != null && state.StandIn.bodyParts.Length > 5 ? state.StandIn.bodyParts[5].transform.position : Vector3.zero;
                    Plugin.Dbg("Player " + id + " collapse positions (local=" + isLocal + "): player=" + victim.transform.position.ToString("F1") + " playerBody=" + victim.thisPlayerBody.position.ToString("F1") + " standIn=" + sp.ToString("F1")
                        + " spider=" + (spider != null ? spider.transform.position.ToString("F1") : "none") + " spiderOwner=" + (spider != null && spider.IsOwner));
                }

                // 5. Hand the body to the spider (only does anything on the spider owner's game).
                if (Plugin.VenomSeconds.Value > 0.5f && spider != null && spider.IsOwner && !spider.isEnemyDead
                    && Vector3.Distance(spider.transform.position, victim.transform.position) > 3.5f)
                    CocoonRunner.Run(SpiderApproach(state));
                else
                    MakeSpiderGrab(spider, victim);

                // 6. The victim watches from a camera that follows their body.
                if (isLocal) StartCamera(state);

                Plugin.Log.LogInfo("Player " + id + " cocooned (this is my own player: " + isLocal + ").");
                if (isLocal) { LogDoors("when wrapped"); CocoonRunner.Run(LogDoorsLater(3f)); }
            }
            catch (Exception e)
            {
                // If anything went wrong, undo it all straight away.
                Plugin.Log.LogError("Cocoon.Begin failed, undoing: " + e);
                End(id);
            }
        }

        // ------------------------------------------------------------------
        // Hanging, death and release
        // ------------------------------------------------------------------

        // Called (on every player's game) when the spider hangs a body from the ceiling.
        // If a player's game is late getting the body (so the spider "holds" nothing there yet),
        // the spider itself is used to find the victim, so the death countdown still starts.
        public static void OnBodyHung(DeadBodyInfo body, SandSpiderAI spider = null)
        {
            if (body == null && spider == null) return;
            foreach (KeyValuePair<int, CocoonState> kv in Active)
            {
                CocoonState s = kv.Value;
                if (s.Hung || s.Player == null) continue;
                bool match = body != null ? s.Player.deadBody == body : s.Spider == spider;
                if (!match) continue;
                // Judging by the spider alone (this game never got the body): a victim the spider already dropped or buried is
                // not the one being hung, or their death timer would start by mistake.
                if (body == null && (LyingOnGround(s) || s.Buried)) continue;

                s.Hung = true;
                s.HungAt = Time.time;
                CocoonRunner.Run(DeathAfter(kv.Key, s, Plugin.HangSeconds.Value));
                Plugin.Log.LogInfo("Player " + kv.Key + " hung up. Death in " + Plugin.HangSeconds.Value + "s.");
                if (Plugin.DebugLogging.Value) CocoonRunner.Run(HangCheck(kv.Key, s, body));
                try { HangSync.Announce(s); } catch (Exception) { }
                break;
            }
        }

        // Diagnostic (DebugLogging): where the hanging body is a few seconds after the hang, on THIS game. Compare the
        // host's and a joiner's logs to see whether the cocoon hangs in a different place for them.
        private static IEnumerator HangCheck(int id, CocoonState s, DeadBodyInfo body)
        {
            yield return new WaitForSeconds(3f);
            try
            {
                DeadBodyInfo b = body != null ? body : (s.StandIn != null ? s.StandIn : (s.Player != null ? s.Player.deadBody : null));
                SandSpiderAI sp = s.Spider;
                if (b == null) { Plugin.Log.LogInfo("Hang check: player " + id + " has no hanging body on this game."); yield break; }
                Vector3 hip = b.bodyParts != null && b.bodyParts.Length > 5 ? b.bodyParts[5].transform.position : b.transform.position;
                Vector3 hp = b.attachedTo != null ? b.attachedTo.position : Vector3.zero;
                string wall = "none";
                RaycastHit hit; float best = 99f;
                Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
                foreach (Vector3 d in dirs)
                    if (Physics.Raycast(hip, d, out hit, 3f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && hit.distance < best) best = hit.distance;
                if (best < 99f) wall = best.ToString("0.00") + " m";
                Plugin.Log.LogInfo("Hang check: player " + id + " hip=" + hip.ToString("F2") + " hangPoint=" + (b.attachedTo != null ? hp.ToString("F2") : "none")
                    + " spider=" + (sp != null ? sp.transform.position.ToString("F2") : "none") + " spiderOwner=" + (sp != null && sp.IsOwner)
                    + " nearestWallToBody=" + wall);
            }
            catch (Exception e) { Plugin.Log.LogWarning("Hang check failed: " + e.Message); }
        }

        private static readonly MethodInfo WrapMethod = AccessTools.Method(typeof(SandSpiderAI), "turnBodyIntoWeb");

        // Starts the spider's own wrapping routine now (it normally only does this once it is home)
        // and tells the other games to do the same. The game freezes the spider while it wraps.
        private static void StartWrap(SandSpiderAI spider, string why)
        {
            try
            {
                if ((bool)SpooledField.GetValue(spider) || (bool)SpoolingField.GetValue(spider)) return;
                if (CoroutineField.GetValue(spider) != null) return;
                if (HeldBodyField.GetValue(spider) == null) return;
                CoroutineField.SetValue(spider, spider.StartCoroutine((IEnumerator)WrapMethod.Invoke(spider, null)));
                spider.SpiderTurnBodyIntoWebServerRpc();
                Plugin.Log.LogInfo("Spider wraps the victim " + why + ".");
                if (Plugin.DebugLogging.Value && Plugin.VerboseLogs.Value) CocoonRunner.Run(RollProbe(spider));
                WrapSpin.Begin(spider);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Starting the wrap failed: " + e);
            }
        }

        // DIAGNOSTIC: follows the held body during the wrap so we can see when the rolling stops (mouth turning vs. body turning).
        private static IEnumerator RollProbe(SandSpiderAI spider)
        {
            float t0 = Time.time; float next = 0f;
            Quaternion lastMouth = Quaternion.identity, lastLimb = Quaternion.identity; bool have = false;
            while (spider != null && Time.time - t0 < 5f)
            {
                if (Time.time - t0 >= next)
                {
                    next += 0.25f;
                    try
                    {
                        DeadBodyInfo b = HeldBodyField.GetValue(spider) as DeadBodyInfo;
                        Transform mt = spider.mouthTarget;
                        Transform limb = b != null && b.attachedLimb != null ? b.attachedLimb.transform : null;
                        string mouth = "-", body = "-"; float dm = 0f, dl = 0f;
                        if (mt != null) { mouth = mt.eulerAngles.ToString("0"); if (have) dm = Quaternion.Angle(lastMouth, mt.rotation); lastMouth = mt.rotation; }
                        if (limb != null) { body = limb.eulerAngles.ToString("0"); if (have) dl = Quaternion.Angle(lastLimb, limb.rotation); lastLimb = limb.rotation; }
                        have = mt != null && limb != null;
                        Plugin.Log.LogInfo("Roll probe t=" + (Time.time - t0).ToString("0.00") + " mouth " + mouth + " (turned " + dm.ToString("0") + " deg) | body limb " + body + " (turned " + dl.ToString("0") + " deg) | match=" + (b != null ? b.matchPositionExactly.ToString() : "-") + " spooling=" + SpoolingField.GetValue(spider) + " spooled=" + SpooledField.GetValue(spider));
                    }
                    catch (Exception) { }
                }
                yield return null;
            }
        }

        private static bool SpiderHoldsBody(SandSpiderAI spider)
        {
            return spider != null && HeldBodyField.GetValue(spider) != null;
        }

        // Waits (up to 10 s) until this game has given the spider its body. Returns via 'ready'.
        private static IEnumerator WaitForHeldBody(SandSpiderAI spider, Action<bool> ready)
        {
            float waited = 0f;
            while (spider != null && !SpiderHoldsBody(spider) && waited < 10f)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            ready(spider != null && !spider.isEnemyDead && SpiderHoldsBody(spider));
        }

        internal static IEnumerator WrapWhenReady(SandSpiderAI spider)
        {
            bool ok = false;
            yield return CocoonRunnerWait(spider, r => ok = r);
            if (!ok || CoroutineField.GetValue(spider) != null) yield break;
            try
            {
                CoroutineField.SetValue(spider, spider.StartCoroutine((IEnumerator)WrapMethod.Invoke(spider, null)));
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Late wrap failed: " + e);
            }
        }

        internal static IEnumerator HangWhenReady(SandSpiderAI spider)
        {
            bool ok = false;
            yield return CocoonRunnerWait(spider, r => ok = r);
            if (!ok)
            {
                // Never got the body: at least start the death countdown for whoever this spider caught.
                OnBodyHung(null, spider);
                yield break;
            }
            // Let a late wrap start, then let it finish before hanging, as on the spider's own game.
            yield return new WaitForSeconds(0.3f);
            float waited = 0f;
            while (spider != null && CoroutineField.GetValue(spider) != null && waited < 8f)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            if (spider == null || spider.isEnemyDead || !SpiderHoldsBody(spider)) yield break;
            try
            {
                HangBodyMethod.Invoke(spider, null);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Late hang failed: " + e);
            }
        }

        private static IEnumerator CocoonRunnerWait(SandSpiderAI spider, Action<bool> ready)
        {
            return WaitForHeldBody(spider, ready);
        }

        private static IEnumerator FailsafeRelease(int id, CocoonState owner, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            CocoonState s;
            // A victim who was dropped recently is busy struggling: give them time instead of cutting it short.
            while (Active.TryGetValue(id, out s) && s == owner && !s.Hung && s.Dropped && Time.time - s.LastDropAt < 40f)
                yield return new WaitForSeconds(5f);
            if (Active.TryGetValue(id, out s) && s == owner && !s.Hung)
            {
                Plugin.Log.LogWarning("Player " + id + " was never hung up: safety release.");
                End(id);
            }
        }

        private static IEnumerator DeathAfter(int id, CocoonState owner, float seconds)
        {
            float start = Time.time;
            while (Time.time < start + seconds + owner.ExtraSeconds)
            {
                yield return new WaitForSeconds(0.25f);
                CocoonState alive;
                if (!Active.TryGetValue(id, out alive) || alive != owner) yield break;
            }
            // A timer from an earlier catch must never kill someone who was freed and caught again.
            CocoonState current;
            if (!Active.TryGetValue(id, out current) || current != owner) yield break;
            try
            {
                Kill(id);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Kill failed: " + e);
            }
        }

        // Only the victim's own game kills them. Everyone else's game learns about it
        // from the game's normal death message, and RemoveDeadVictims() tidies up.
        public static void Kill(int id)
        {
            CocoonState state;
            if (!Active.TryGetValue(id, out state)) return;
            PlayerControllerB p = state.Player;
            if (!IsLocal(p)) return;

            Active.Remove(id);
            StopCamera(state, false); // KillPlayer switches to the death camera itself
            p.disableMoveInput = false;
            p.disableLookInput = false;

            Plugin.Log.LogInfo("Player " + id + " dies in the cocoon.");

            // Die the normal way WITH a body: the game then makes an ordinary pick-up-able corpse. First the
            // hanging stand-in is removed and the player's body is moved to where it hung, so the real corpse
            // appears there.
            bool normalBody = false;
            try
            {
                DeadBodyInfo standIn = state.StandIn != null ? state.StandIn : p.deadBody;
                Plugin.Dbg("Kill check: stand-in " + (state.StandIn != null ? "present" : "MISSING") + ", player's deadBody " + (p.deadBody != null ? "present" : "null")
                    + ", AllowPlayerDeath=" + p.AllowPlayerDeath() + ", IsOwner=" + p.IsOwner + ", bodyParts=" + (standIn != null && standIn.bodyParts != null ? standIn.bodyParts.Length.ToString() : "none") + ".");
                // Only when the death will really happen: if another mod or the game blocks it, the player is
                // freed below and the stand-in must still be there for that.
                if (p.AllowPlayerDeath() && standIn != null && standIn.bodyParts != null && standIn.bodyParts.Length > 5)
                {
                    Vector3 hip = standIn.bodyParts[5].transform.position;
                    p.thisPlayerBody.position = hip - Vector3.up * 1.32f;
                    p.transform.position = p.thisPlayerBody.position;
                    DiscardStandIn(state, standIn);
                    if (p.deadBody == standIn) p.deadBody = null;
                    normalBody = true;
                }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Could not swap the stand-in for a normal body: " + e.Message); }
            p.KillPlayer(Vector3.zero, normalBody, DeathCause(), 0);
            if (p.isPlayerDead && !normalBody) EnsureCorpseGrabbable(state); // does nothing unless this game is the host

            // If something blocked the death (Imperium's god mode, the game's "allow death" switch, another mod),
            // the player is still alive but our camera is already off: they would walk around seeing nothing.
            // Give them their own camera back and free them from the cocoon instead.
            if (!p.isPlayerDead)
            {
                Plugin.Log.LogWarning("Player " + id + " did not die: the game or another mod (for example Imperium's god mode) blocked it. Freeing them from the cocoon instead.");
                try
                {
                    StartOfRound sr = StartOfRound.Instance;
                    Plugin.Log.LogInfo("Why the death was refused: allowLocalPlayerDeath=" + sr.allowLocalPlayerDeath + ", testRoom=" + (sr.testRoom != null ? "yes" : "no")
                        + ", timeSinceRoundStarted=" + sr.timeSinceRoundStarted.ToString("0.0") + ", shipDoorsEnabled=" + sr.shipDoorsEnabled
                        + ", AllowPlayerDeath()=" + p.AllowPlayerDeath() + ", isPlayerDead=" + p.isPlayerDead + ", IsOwner=" + p.IsOwner + ", cause=" + DeathCause() + ", spawnBody=" + normalBody + ".");
                    HarmonyLib.Patches pi = HarmonyLib.Harmony.GetPatchInfo(AccessTools.Method(typeof(PlayerControllerB), "KillPlayer"));
                    if (pi != null)
                    {
                        foreach (HarmonyLib.Patch x in pi.Prefixes) Plugin.Log.LogInfo("KillPlayer prefix from: " + x.owner + " (" + x.PatchMethod.DeclaringType + "." + x.PatchMethod.Name + ")");
                        foreach (HarmonyLib.Patch x in pi.Postfixes) Plugin.Log.LogInfo("KillPlayer postfix from: " + x.owner + " (" + x.PatchMethod.DeclaringType + "." + x.PatchMethod.Name + ")");
                        foreach (HarmonyLib.Patch x in pi.Transpilers) Plugin.Log.LogInfo("KillPlayer transpiler from: " + x.owner + " (" + x.PatchMethod.DeclaringType + "." + x.PatchMethod.Name + ")");
                    }
                    else Plugin.Log.LogInfo("KillPlayer: no Harmony patches at all (a MonoMod hook, if any, would not show here).");
                }
                catch (Exception e) { Plugin.Log.LogWarning("Death diagnostic failed: " + e.Message); }
                StartOfRound sor = StartOfRound.Instance;
                if (sor != null && p.gameplayCamera != null)
                {
                    sor.SwitchCamera(p.gameplayCamera);
                    p.ChangeAudioListenerToObject(p.gameplayCamera.gameObject);
                }
                Active[id] = state;
                End(id, true);
                CocoonNet.RequestRelease(id);
            }
        }

        public static void EndAll()
        {
            foreach (int id in new List<int>(Active.Keys))
            {
                End(id);
            }
        }

        // F9: free MYSELF immediately (even if the network is broken), then tell everyone else's game
        // to do the same. It must not touch other players locally, or the games drift apart.
        public static void EmergencyRelease()
        {
            GameNetworkManager gnm = GameNetworkManager.Instance;
            PlayerControllerB me = gnm != null ? gnm.localPlayerController : null;
            if (me == null) return;
            int id = (int)me.playerClientId;
            if (!Active.ContainsKey(id)) return;

            Plugin.Log.LogInfo("Emergency release (F9) for player " + id + ".");
            End(id);
            CocoonNet.RequestRelease(id);
        }

        // DIAGNOSTIC: doors reported vanishing around a wrap. Counts the facility's doors and says how many are hidden.
        private static IEnumerator LogDoorsLater(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            LogDoors(seconds.ToString("0") + "s after the wrap");
        }

        internal static void LogDoors(string when)
        {
            try
            {
                Type doorType = AccessTools.TypeByName("DoorLock");
                if (doorType == null) { Plugin.Log.LogInfo("Doors " + when + ": no DoorLock type."); return; }
                UnityEngine.Object[] doors = UnityEngine.Object.FindObjectsOfType(doorType, true);
                GameNetworkManager gnm = GameNetworkManager.Instance;
                Vector3 me = gnm != null && gnm.localPlayerController != null ? gnm.localPlayerController.transform.position : Vector3.zero;
                int inactive = 0, noVisible = 0, noCollider = 0;
                string hidden = "";
                foreach (UnityEngine.Object o in doors)
                {
                    Component c = o as Component;
                    if (c == null) continue;
                    bool active = c.gameObject.activeInHierarchy;
                    bool anyRenderer = false, anyCollider = false;
                    foreach (Renderer r in c.GetComponentsInChildren<Renderer>(true)) if (r.enabled && r.gameObject.activeInHierarchy) { anyRenderer = true; break; }
                    foreach (Collider col in c.GetComponentsInChildren<Collider>(true)) if (col.enabled && col.gameObject.activeInHierarchy) { anyCollider = true; break; }
                    if (!active) inactive++;
                    if (!anyRenderer) noVisible++;
                    if (!anyCollider) noCollider++;
                    if ((!active || !anyRenderer) && hidden.Length < 300)
                        hidden += " [" + c.gameObject.name + " active=" + active + " renderers=" + anyRenderer + " " + Vector3.Distance(me, c.transform.position).ToString("0") + "m]";
                }
                if (Plugin.VerboseLogs.Value) Plugin.Log.LogInfo("Doors " + when + ": total=" + doors.Length + " inactive=" + inactive + " noVisibleRenderer=" + noVisible + " noEnabledCollider=" + noCollider + hidden);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Door diagnostic failed: " + e.Message);
            }
        }

        private static void AddCollider(CocoonState state, Collider c)
        {
            if (c == null || state.Colliders.Contains(c)) return;
            state.Colliders.Add(c);
            state.ColliderWasEnabled.Add(c.enabled);
        }

        private static void DisableColliders(CocoonState state)
        {
            for (int i = 0; i < state.Colliders.Count; i++)
            {
                if (state.Colliders[i] != null && state.Colliders[i].enabled) state.Colliders[i].enabled = false;
            }
        }

        private static void RestoreColliders(CocoonState state)
        {
            for (int i = 0; i < state.Colliders.Count; i++)
            {
                if (state.Colliders[i] != null) state.Colliders[i].enabled = state.ColliderWasEnabled[i];
            }
        }

        // Puts a victim back to normal (safety net / F9).
        public static void End(int id, bool teleportToBody = false)
        {
            CocoonState state;
            if (!Active.TryGetValue(id, out state)) return;
            Active.Remove(id);

            PlayerControllerB p = state.Player;
            if (p != null && !p.isPlayerDead) RestoreColliders(state); // a dead player's body stays non-solid
            if (p == null || p.isPlayerDead)
            {
                // The game already handled their model and body.
                StopCamera(state, false);
                return;
            }

            ImmuneUntil[id] = Time.time + Plugin.ImmunitySeconds.Value;
            StopCamera(state, true);

            for (int i = 0; i < state.Renderers.Count; i++)
            {
                if (state.Renderers[i] != null) state.Renderers[i].enabled = state.WereEnabled[i];
            }

            if (IsLocal(p))
            {
                p.disableMoveInput = false;
                p.disableLookInput = false;
            }

            if (p.deadBody != null)
            {
                // If the spider is still holding / wrapping this body, make it let go first,
                // otherwise its wrapping routine would hit a destroyed object.
                SandSpiderAI spider = state.Spider;
                if (spider != null && !spider.isEnemyDead && HeldBodyField.GetValue(spider) == (object)p.deadBody)
                {
                    CancelSpoolingMethod.Invoke(spider, null);
                    CoroutineField.SetValue(spider, null); // same game quirk as above
                }
                if (teleportToBody && (state.Hung || state.Dropped) && IsLocal(p))
                {
                    MoveVictimUnderBody(p, state.Spider, state.Buried ? (Vector3?)state.DigSpot : null);
                }
                BrackenThief.ReleaseCarry(state, p.deadBody);
                Transform hangPoint = p.deadBody.attachedTo;
                UnityEngine.Object.Destroy(p.deadBody.gameObject);
                p.deadBody = null;
                if (state.Hung && hangPoint != null) DestroyRope(hangPoint);
            }
            Plugin.Log.LogInfo("Player " + id + " released (this is my own player: " + IsLocal(p) + ").");
            if (IsLocal(p)) LogDoors("when released");
            if (IsLocal(p)) CocoonRunner.Run(HinderWatchdog(p));
        }

        // After a release the player may be left permanently slowed (a web that still thinks it holds them, or a
        // slow-down nothing is holding). For a few minutes, quietly put that right. Normal webs are left alone.
        private static IEnumerator HinderWatchdog(PlayerControllerB p)
        {
            float end = Time.time + 180f;
            while (Time.time < end)
            {
                yield return new WaitForSeconds(0.5f);
                if (p == null || p.isPlayerDead) yield break;
                if (Active.ContainsKey((int)p.playerClientId)) yield break; // wrapped again: the next release restarts this
                if (p.isMovementHindered <= 0 && Mathf.Abs(p.hinderedMultiplier - 1f) < 0.01f) continue;
                WebIgnorePatch.FixStuckHindrance(p);
            }
        }

        // Safety net: if a spider stands still while carrying a victim for too long (whatever the
        // cause), hang the body where it stands. That also starts the death countdown as normal.
        // Only the spider's owner does this, exactly like the game's own hanging code.
        public static void WatchSpiders()
        {
            if (Active.Count == 0) return;
            foreach (CocoonState s in new List<CocoonState>(Active.Values))
            {
                SandSpiderAI spider = s.Spider;

                // One-off diagnostic: wrapped up for a while but still not hung. Shows what the spider is doing.
                if (!s.DiagLogged && !s.Hung && spider != null && s.Player != null && Time.time - s.BeganAt > 25f)
                {
                    s.DiagLogged = true;
                    try
                    {
                        Plugin.Log.LogWarning("Spider diagnostic: player " + s.Player.playerClientId
                            + " wrapped 25s but not hung. state=" + spider.currentBehaviourStateIndex
                            + " owner=" + spider.IsOwner
                            + " spooling=" + SpoolingField.GetValue(spider)
                            + " spooled=" + SpooledField.GetValue(spider)
                            + " wrapRoutineSet=" + (CoroutineField.GetValue(spider) != null)
                            + " holdingTheirBody=" + (HeldBodyField.GetValue(spider) == (object)s.Player.deadBody)
                            + " stunTimer=" + (StunField != null ? StunField.GetValue(spider) : null));
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogWarning("Spider diagnostic failed: " + e.Message);
                    }
                }

                // The spider died while it had (or was fetching) a victim: they are left lying there and must struggle free.
                // Only the host decides, once.
                if (!s.Hung && !s.Dropped && !s.NoLiveSpider && spider != null && spider.isEnemyDead && s.Player != null && s.Player.deadBody != null
                    && s.Player.deadBody.attachedTo == null && Time.time - s.BeganAt > 2f
                    && Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsServer)
                {
                    Plugin.Log.LogInfo("The spider died: player " + s.Player.playerClientId + " must struggle free.");
                    CocoonNet.RequestDrop((int)s.Player.playerClientId, 255);
                    continue;
                }
                if (s.Hung || spider == null || spider.isEnemyDead || !spider.IsOwner || s.Player == null) continue;

                if (LyingOnGround(s)) { s.LostSince = -1f; s.StillSince = -1f; continue; } // dropped on purpose: no safety net

                DeadBodyInfo body = s.Player.deadBody;
                if (body == null || HeldBodyField.GetValue(spider) != (object)body)
                {
                    s.StillSince = -1f;
                    // The spider let go of the body before hanging it (the game does this when the
                    // spider starts chasing someone). Nothing would ever hang it, so free the victim.
                    if (body != null && Time.time - s.BeganAt > 6f)
                    {
                        if (s.LostSince < 0f) s.LostSince = Time.time;
                        else if (Time.time - s.LostSince > 4f)
                        {
                            // Already dropped on purpose a moment ago: leave it to the struggle / go-back logic.
                            if (s.Dropped && Time.time - s.LastDropAt < 15f) { s.LostSince = Time.time; continue; }
                            Plugin.Log.LogInfo("Drop check: dropped=" + s.Dropped + " attachedTo=" + (body.attachedTo != null ? body.attachedTo.name : "none")
                                + " claim=" + (s.Claim != null) + " hung=" + s.Hung);
                            // Not freed for nothing: they lie where the spider let go and have to struggle free (or be cut down).
                            Plugin.Log.LogWarning("Spider dropped player " + s.Player.playerClientId + " without hanging them: they must struggle free.");
                            s.LostSince = -1f;
                            CocoonNet.RequestDrop((int)s.Player.playerClientId, 255);
                        }
                    }
                    continue;
                }
                s.LostSince = -1f;

                // Wrapping the body (and being stunned) are legitimate reasons to stand still.
                bool busy = (bool)SpoolingField.GetValue(spider);
                if (StunField != null && (float)StunField.GetValue(spider) > 0f) busy = true;

                Vector3 pos = spider.meshContainer != null ? spider.meshContainer.position : spider.transform.position;
                bool carriedLong = !busy && Plugin.CarryTimeoutSeconds.Value > 0f && Time.time - s.BeganAt >= Plugin.CarryTimeoutSeconds.Value;
                if (!carriedLong && (s.StillSince < 0f || busy || Vector3.Distance(pos, s.LastSpiderPos) > 0.5f))
                {
                    s.LastSpiderPos = pos;
                    s.StillSince = Time.time;
                    continue;
                }
                bool stuckStill = Time.time - s.StillSince >= Plugin.StuckSeconds.Value;
                bool carriedTooLong = Plugin.CarryTimeoutSeconds.Value > 0f && Time.time - s.BeganAt >= Plugin.CarryTimeoutSeconds.Value;
                if (!stuckStill && !carriedTooLong) continue;

                // Stalled before it ever wrapped the victim: wrap first, then hang (the wrap counts as busy).
                if (!s.NetWrapped && !(bool)SpooledField.GetValue(spider))
                {
                    s.NetWrapped = true;
                    s.StillSince = Time.time;
                    StartWrap(spider, "because it stalled");
                    continue;
                }

                Plugin.Log.LogWarning((stuckStill ? "Spider stuck" : "Spider carrying too long") + " with player " + s.Player.playerClientId + ": hanging them where it stands.");
                s.StillSince = -1f;
                try
                {
                    HangBodyMethod.Invoke(spider, null);
                    spider.SpiderHangBodyServerRpc();
                    spider.targetPlayer = null;
                    if (spider.currentBehaviourStateIndex == 2) spider.SwitchToBehaviourState(0);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Unsticking the spider failed: " + e);
                }
            }
        }

        // ------------------------------------------------------------------
        // hit a spider that is carrying a victim and it drops them to fight back.
        // The victim lies helpless on the ground and must mash a key to struggle free (or a teammate cuts them down).
        // ------------------------------------------------------------------

        // The victim's body is lying where the spider dropped it (not carried, not hung up).
        internal static bool LyingOnGround(CocoonState s)
        {
            return s != null && s.Dropped && !s.Hung && s.Player != null && s.Player.deadBody != null
                && s.Player.deadBody.attachedTo == null && s.Claim == null;
        }

        // Runs on every game when a spider is hit; only the spider's owner acts.
        internal static void SpiderWasHit(SandSpiderAI spider, PlayerControllerB attacker)
        {
            try
            {
                if (!Plugin.SpiderDropsWhenHit.Value || spider == null || attacker == null) return;
                if (spider.isEnemyDead || !spider.IsOwner) return;
                foreach (KeyValuePair<int, CocoonState> kv in Active)
                {
                    CocoonState s = kv.Value;
                    if (s.Spider != spider || s.Hung || s.Player == null || s.Player.deadBody == null) continue;
                    if (HeldBodyField.GetValue(spider) != (object)s.Player.deadBody) continue;
                    if (Time.time - s.LastDropAt < 1.5f) return;
                    s.LastDropAt = Time.time;
                    Plugin.Log.LogInfo("Spider hit by player " + attacker.playerClientId + " while carrying player " + kv.Key + ": dropping them to fight back.");
                    CocoonNet.RequestDrop(kv.Key, (int)attacker.playerClientId);
                    return;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SpiderWasHit failed: " + e);
            }
        }

        // Testing only: acts as if somebody hit a spider that is carrying a victim (no attacker, so nobody is chased).
        internal static void DebugFakeHit()
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || !nm.IsServer) { Plugin.Log.LogWarning("Fake hit key: only the host can use it."); return; }
            foreach (KeyValuePair<int, CocoonState> kv in Active)
            {
                CocoonState s = kv.Value;
                SandSpiderAI spider = s.Spider;
                if (spider == null || spider.isEnemyDead || s.Hung || s.Player == null || s.Player.deadBody == null) continue;
                if (HeldBodyField.GetValue(spider) != (object)s.Player.deadBody) continue;
                Plugin.Log.LogInfo("Fake hit key: dropping player " + kv.Key + " as if the spider had been hit.");
                s.LastDropAt = Time.time;
                CocoonNet.RequestDrop(kv.Key, 255);
                return;
            }
            Plugin.Log.LogInfo("Fake hit key: no spider is carrying anyone right now.");
        }

        // Runs on EVERY player's game (the host applies its own broadcast directly).
        // An outdoor spider reaches the point where it would hang its catch: drop the cocoon here instead.
        internal static void OutdoorDrop(DeadBodyInfo held, SandSpiderAI spider)
        {
            try
            {
                if (held == null || spider == null || !spider.IsOwner) return;
                foreach (KeyValuePair<int, CocoonState> kv in Active)
                {
                    CocoonState s = kv.Value;
                    if (s == null || s.Player == null || s.Player.deadBody != held) continue;
                    if (s.Dropped && Time.time - s.LastDropAt < 5f) return;
                    s.NoRegrab = true;
                    Plugin.Log.LogInfo("Outdoor spider: no ceiling to hang player " + kv.Key + " from, so it drops the cocoon here. They must struggle free.");
                    CocoonNet.RequestDrop(kv.Key, 255);
                    spider.targetPlayer = null;
                    spider.SwitchToBehaviourState(0);
                    return;
                }
            }
            catch (Exception e) { Plugin.Log.LogError("OutdoorDrop failed: " + e); }
        }

        public static void ApplyDrop(int victimId, int attackerId)
        {
            CocoonState s;
            if (!Active.TryGetValue(victimId, out s) || s.Hung) return;
            if (Time.time - s.DropAppliedAt < 1f) return; // the same drop arriving twice (host + broadcast)
            s.DropAppliedAt = Time.time;
            s.Dropped = true;
            s.LastDropAt = Time.time;
            s.StruggleProgress = 0f;
            SandSpiderAI spider = s.Spider;
            try
            {
                if (spider != null && !spider.isEnemyDead && s.Player != null && s.Player.deadBody != null
                    && HeldBodyField.GetValue(spider) == (object)s.Player.deadBody)
                {
                    s.WasSpooled = (bool)SpooledField.GetValue(spider);
                    CancelSpoolingMethod.Invoke(spider, null);
                    CoroutineField.SetValue(spider, null); // same game quirk as elsewhere
                }
                Plugin.Log.LogInfo("Player " + victimId + " was dropped by the spider (this is my own player: " + IsLocal(s.Player) + ").");

                if (spider != null && !spider.isEnemyDead && spider.IsOwner)
                {
                    PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
                    PlayerControllerB attacker = attackerId >= 0 && attackerId < players.Length ? players[attackerId] : null;
                    if (attacker != null && !attacker.isPlayerDead)
                    {
                        spider.targetPlayer = attacker;
                        if (WatchFromDistanceField != null) WatchFromDistanceField.SetValue(spider, false);
                        if (ChaseTimerField != null) ChaseTimerField.SetValue(spider, 12.5f);
                        spider.SwitchToBehaviourState(2);
                    }
                    if (!s.RegrabActive && !s.NoRegrab) { s.RegrabActive = true; CocoonRunner.Run(RegrabAfterChase(s, spider)); }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("ApplyDrop failed: " + e);
            }
        }

        // After hunting the attacker for a while the spider goes back for the cocoon (if the victim is still in it).
        private static IEnumerator RegrabAfterChase(CocoonState s, SandSpiderAI spider)
        {
            try
            {
                IEnumerator inner = RegrabInner(s, spider);
                while (inner.MoveNext()) yield return inner.Current;
            }
            finally { s.RegrabActive = false; }
        }

        private static IEnumerator RegrabInner(CocoonState s, SandSpiderAI spider)
        {
            yield return new WaitForSeconds(Plugin.DropChaseSeconds.Value);
            while (true)
            {
                if (spider == null || spider.isEnemyDead || !spider.IsOwner || s.Player == null || s.Player.isPlayerDead) yield break;
                CocoonState cur;
                if (!Active.TryGetValue((int)s.Player.playerClientId, out cur) || cur != s) yield break;
                if (!LyingOnGround(s)) yield break; // picked up by something else, hung, or freed
                // Busy carrying somebody else (it bit the attacker, say): leave this one on the ground for now.
                object heldNow = HeldBodyField.GetValue(spider);
                if (heldNow != null && heldNow != (object)s.Player.deadBody) { yield return new WaitForSeconds(1f); continue; }
                // Do not go back for the cocoon while someone is still hitting it: wait for a quiet moment.
                if (Time.time - s.LastDropAt >= Plugin.DropChaseSeconds.Value) break;
                yield return new WaitForSeconds(1f);
            }
            Plugin.Log.LogInfo("Spider goes back for player " + s.Player.playerClientId + "'s cocoon.");
            s.BeganAt = Time.time; // the stall timers start afresh
            s.StillSince = -1f;
            s.LostSince = -1f;
            s.NetWrapped = false;
            CocoonRunner.Run(SpiderApproach(s));
        }

        // The victim's own game: mash the key to wriggle free of a cocoon lying on the ground.
        internal static bool StruggleActive; // for the on-screen bar
        internal static float StruggleBar;

        public static void TickStruggle()
        {
            StruggleActive = false;
            GameNetworkManager gnm = GameNetworkManager.Instance;
            PlayerControllerB me = gnm != null ? gnm.localPlayerController : null;
            if (me == null) return;
            CocoonState s;
            if (!Active.TryGetValue((int)me.playerClientId, out s) || !LyingOnGround(s)) { if (s != null) s.StruggleProgress = 0f; return; }
            if (me.isTypingChat || me.inTerminalMenu || (me.quickMenuManager != null && me.quickMenuManager.isMenuOpen)) return;

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            float step = 1f / Mathf.Max(3, Plugin.StrugglePresses.Value);
            s.StruggleProgress = Mathf.Max(0f, s.StruggleProgress - Plugin.StruggleDecayPerSecond.Value * Time.deltaTime);
            if (keyboard != null && keyboard[Plugin.StruggleKey.Value].wasPressedThisFrame) s.StruggleProgress += step;
            StruggleActive = true;
            StruggleBar = Mathf.Clamp01(s.StruggleProgress);

            if (s.StruggleProgress >= 1f)
            {
                s.StruggleProgress = 0f;
                StruggleActive = false;
                Plugin.Log.LogInfo("Player " + me.playerClientId + " struggled free of the cocoon.");
                CocoonNet.RequestRelease((int)me.playerClientId);
            }
        }

        // Runs on EVERY player's game when someone finishes cutting a victim down.
        public static void ApplyRelease(int id)
        {
            CocoonState s;
            if (!Active.TryGetValue(id, out s)) return;
            Plugin.Log.LogInfo("Player " + id + " was cut down.");
            End(id, true);
        }

        private sealed class SideState { public float Until; public bool Outside; }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, SideState> SideCache
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, SideState>();

        // Is this spot outdoors? Judged by which set of AI nodes (the monsters' own waypoints) is nearer, because the
        // game's inside/outside flags are only set on some players' games (a spawn-key spider, say) or are stale.
        internal static bool PositionOutdoors(Vector3 pos)
        {
            RoundManager rm = RoundManager.Instance;
            if (rm == null) return false;
            if (rm.outsideAINodes == null || rm.outsideAINodes.Length == 0)
            {
                try { rm.GetOutsideAINodes(); } catch (Exception) { }
            }
            float dIn = NearestNodeDistance(rm.insideAINodes, pos);
            float dOut = NearestNodeDistance(rm.outsideAINodes, pos);
            return dOut < dIn;
        }

        // Same answer on every player's game: true if the game says the spider is outside, or it stands nearer the outdoor nodes.
        internal static bool SpiderOutdoors(SandSpiderAI s)
        {
            if (s == null) return false;
            if (s.isOutside) return true;
            SideState c = SideCache.GetOrCreateValue(s);
            if (Time.time < c.Until) return c.Outside;
            c.Until = Time.time + 0.5f;
            c.Outside = PositionOutdoors(s.transform.position);
            return c.Outside;
        }

        internal static float NearestNodeDistance(GameObject[] nodes, Vector3 pos)
        {
            float best = float.MaxValue;
            if (nodes == null) return best;
            foreach (GameObject n in nodes)
            {
                if (n == null) continue;
                float d = Vector3.Distance(n.transform.position, pos);
                if (d < best) best = d;
            }
            return best;
        }

        // The freed player's real body is wherever they were hit, so bring them to the cocoon.
        internal static void MoveVictimUnderBody(PlayerControllerB p, SandSpiderAI spider = null, Vector3? spot = null)
        {
            try
            {
                Vector3 pos = spot.HasValue ? spot.Value : p.deadBody.bodyParts[6].transform.position;
                // The cocoon hangs on the spider's side of the door. If the player went through an entrance
                // while wrapped (the door was already in use as they collapsed), they must come back to that side.
                bool inside = (spider != null) ? !SpiderOutdoors(spider) : p.isInsideFactory;
                // The flags can be wrong (a spawned or stale one): trust where the cocoon really is. Whichever set of
                // AI nodes has a node nearer to the cocoon is the side it hangs on.
                if (RoundManager.Instance != null)
                {
                    float dIn = NearestNodeDistance(RoundManager.Instance.insideAINodes, pos);
                    float dOut = NearestNodeDistance(RoundManager.Instance.outsideAINodes, pos);
                    bool byPlace = dIn < dOut;
                    if (byPlace != inside)
                    {
                        Plugin.Log.LogInfo("Cocoon is " + (byPlace ? "inside" : "outside") + " by position (" + dIn.ToString("F0") + " m from inside nodes, " + dOut.ToString("F0") + " m from outside ones), whatever the flags said.");
                        inside = byPlace;
                    }
                }
                if (p.isInsideFactory != inside)
                {
                    Plugin.Log.LogInfo("Freed player was on the other side of the door from their cocoon; bringing them back " + (inside ? "inside" : "outside") + ".");
                    p.isInsideFactory = inside;
                    if (IsLocal(p) && TimeOfDay.Instance != null)
                    {
                        TimeOfDay.Instance.insideLighting = inside;
                        TimeOfDay.Instance.SetInsideLightingDimness(doNotLerp: true, inside);
                    }
                }
                // Land on the nearest AI node (the points the monsters themselves walk between): they are
                // always on safe, walkable ground, unlike the floor under a carried cocoon (a pit, say).
                if (spot.HasValue) { p.TeleportPlayer(spot.Value + Vector3.up * 0.3f); return; } // dug out: they come up right at the hole
                GameObject[] nodes = RoundManager.Instance != null
                    ? (inside ? RoundManager.Instance.insideAINodes : RoundManager.Instance.outsideAINodes)
                    : null;
                GameObject bestNode = null;
                float bestDist = float.MaxValue;
                if (nodes != null)
                {
                    foreach (GameObject n in nodes)
                    {
                        if (n == null) continue;
                        float d = Vector3.Distance(n.transform.position, pos);
                        if (d < bestDist) { bestDist = d; bestNode = n; }
                    }
                }
                if (bestNode == null)
                {
                    Plugin.Log.LogWarning("No AI node found near the cocoon; the hidden player stays where they are.");
                    return;
                }
                p.TeleportPlayer(bestNode.transform.position + Vector3.up * 0.3f);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Could not move the freed player to the cocoon: " + e);
            }
        }

        // Removes the web line the body was hanging from.
        // Removes the hanging stand-in body the clean way: the spider lets go first, a thief puts it down,
        // and the rope it hung from goes with it (the same steps End() takes).
        internal static void DiscardStandIn(CocoonState state, DeadBodyInfo body)
        {
            if (body == null) return;
            try
            {
                SandSpiderAI spider = state != null ? state.Spider : null;
                if (spider != null && !spider.isEnemyDead && HeldBodyField.GetValue(spider) == (object)body)
                {
                    CancelSpoolingMethod.Invoke(spider, null);
                    CoroutineField.SetValue(spider, null);
                }
                if (state != null) BrackenThief.ReleaseCarry(state, body);
                Transform hangPoint = body.attachedTo;
                UnityEngine.Object.Destroy(body.gameObject);
                if (state != null && state.Hung && hangPoint != null) DestroyRope(hangPoint);
            }
            catch (Exception e) { Plugin.Log.LogWarning("Could not remove the stand-in body cleanly: " + e.Message); }
        }

        internal static void DestroyRope(Transform hangPoint)
        {
            try
            {
                SetLineRendererPoints rope = hangPoint.GetComponentInParent<SetLineRendererPoints>();
                if (rope != null) UnityEngine.Object.Destroy(rope.gameObject);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not remove the rope: " + e.Message);
            }
        }

        // ------------------------------------------------------------------
        // Rescue: stand near a hung cocoon and hold the key to cut the victim down.
        // ------------------------------------------------------------------

        private const float RescueRange = 5f;
        private const float DigRange = 3f;
        private static int cutTarget = -1;
        private static float cutProgress;
        private static float lastRequestTime = -10f;
        private static bool promptShown;

        public static void UpdateRescue()
        {
            Trapdoor.TidyMarkers();
            GameNetworkManager gnm = GameNetworkManager.Instance;
            PlayerControllerB me = gnm != null ? gnm.localPlayerController : null;
            if (me == null) return;

            int target = -1;
            bool canAct = me.isPlayerControlled && !me.isPlayerDead && !IsCocooned(me)
                && !me.isTypingChat && !me.inTerminalMenu && !me.inSpecialMenu
                && (me.quickMenuManager == null || !me.quickMenuManager.isMenuOpen)
                && Time.time - lastRequestTime > 2f;

            if (canAct && Active.Count > 0)
            {
                float best = RescueRange;
                Vector3 eye = me.gameplayCamera.transform.position;
                foreach (KeyValuePair<int, CocoonState> kv in Active)
                {
                    CocoonState s = kv.Value;
                    if (!(s.Hung || LyingOnGround(s)) || s.Player == null || s.Player == me) continue;
                    float d;
                    if (s.Buried)
                    {
                        // Dragged underground: teammates dig at the disturbed earth, wherever the body is now.
                        d = Vector3.Distance(me.transform.position, s.DigSpot);
                        if (d > DigRange) continue;
                    }
                    else
                    {
                        DeadBodyInfo body = s.Player.deadBody;
                        if (body == null || body.bodyParts == null || body.bodyParts.Length <= 6) continue;
                        d = Vector3.Distance(eye, body.bodyParts[6].transform.position);
                    }
                    if (d < best)
                    {
                        best = d;
                        target = kv.Key;
                    }
                }
            }

            if (target < 0)
            {
                cutTarget = -1;
                cutProgress = 0f;
                if (promptShown)
                {
                    promptShown = false;
                    me.cursorTip.text = "";
                }
                return;
            }

            if (target != cutTarget)
            {
                cutTarget = target;
                cutProgress = 0f;
            }

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            bool held = keyboard != null && keyboard[Plugin.CutDownKey.Value].isPressed;
            cutProgress = held ? cutProgress + Time.deltaTime : 0f;

            CocoonState tState;
            bool dig = Active.TryGetValue(target, out tState) && tState.Buried;
            float need = Mathf.Max(0.5f, dig ? Plugin.DigOutSeconds.Value : Plugin.CutDownSeconds.Value);
            bool slowed = IsVenomed(me) && Plugin.VenomedCutMultiplier.Value > 1.01f;
            if (slowed) need *= Plugin.VenomedCutMultiplier.Value;
            bool knife = !dig && Plugin.KnifeCutMultiplier.Value < 0.99f && me.isHoldingObject && me.currentlyHeldObjectServer is KnifeItem;
            if (knife) need = Mathf.Max(0.5f, need * Plugin.KnifeCutMultiplier.Value);
            int percent = Mathf.Min(100, Mathf.RoundToInt(cutProgress / need * 100f));
            me.cursorTip.text = held
                ? (dig ? "Digging them out... " : "Cutting them down... ") + percent + "%" + (slowed ? " (venom slows you)" : "") + (knife ? " (knife)" : "") + TimeSuffix(target)
                : "[Hold " + Plugin.CutDownKey.Value + "] " + (dig ? "Dig them out" : "Cut them down") + TimeSuffix(target);
            promptShown = true;

            if (cutProgress >= need)
            {
                cutProgress = 0f;
                cutTarget = -1;
                lastRequestTime = Time.time;
                Plugin.Log.LogInfo("Cutting down player " + target);
                CocoonNet.RequestRelease(target);
            }
        }

        public static float SecondsLeft(CocoonState s)
        {
            return Mathf.Max(0f, s.HungAt + Plugin.HangSeconds.Value + s.ExtraSeconds - Time.time);
        }

        public static string FormatTime(float sec)
        {
            int n = Mathf.CeilToInt(sec);
            return (n / 60) + ":" + (n % 60).ToString("00");
        }

        private static string TimeSuffix(int id)
        {
            CocoonState s;
            if (!Plugin.ShowDeathTimer.Value || !Active.TryGetValue(id, out s) || !s.Hung) return "";
            return " (" + FormatTime(SecondsLeft(s)) + " left)";
        }

        // Forget victims who really died (killed by us, or by anything else).
        public static void RemoveDeadVictims()
        {
            if (Active.Count == 0) return;
            foreach (int id in new List<int>(Active.Keys))
            {
                CocoonState s = Active[id];
                if (s.Player == null || s.Player.isPlayerDead)
                {
                    StopCamera(s, false);
                    // If a thief was still carrying them, let go: otherwise the corpse stays glued to the thief
                    // and nobody can pick it up.
                    try
                    {
                        if (s.Thief != null || s.BugThief != null || s.BrackenDragPoint != null || s.BugCarryPoint != null)
                        {
                            Plugin.Log.LogInfo("Player " + id + " died while carried: setting the body down.");
                            BrackenThief.ReleaseCarry(s, s.StandIn != null ? s.StandIn : (s.Player != null ? s.Player.deadBody : null));
                        }
                    }
                    catch (Exception e) { Plugin.Log.LogWarning("Could not set the body down: " + e.Message); }
                    bool replaced = false;
                    try
                    {
                        // The game made a normal body: the hanging stand-in is no longer needed.
                        if (s.StandIn != null && s.Player != null && s.Player.deadBody != s.StandIn && s.Player.deadBody != null)
                        {
                            DiscardStandIn(s, s.StandIn);
                            replaced = true;
                        }
                    }
                    catch (Exception e) { Plugin.Log.LogWarning("Could not remove the stand-in body: " + e.Message); }
                    if (!replaced && s.StandIn != null && s.Player != null)
                    {
                        PendingStandIns.Add(s);
                        s.StandInDeadline = Time.time + 4f;
                    }
                    if (!replaced && s.StandIn != null) { if (Plugin.DebugLogging.Value) corpseDumpAt = Time.time + 6f; Active.Remove(id); continue; }
                    try
                    {
                        DeadBodyInfo db = (!replaced && s.Player != null) ? s.Player.deadBody : null;
                        if (db != null)
                        {
                            db.canBeGrabbedBackByPlayers = true;
                            if (db.attachedTo != null && db.grabBodyObject == null) { db.attachedTo = null; db.attachedLimb = null; db.matchPositionExactly = false; }
                            db.gameObject.SetActive(true);
                        }
                    }
                    catch (Exception e) { Plugin.Log.LogWarning("Could not free the corpse: " + e.Message); }
                    if (!replaced) EnsureCorpseGrabbable(s);
                    if (Plugin.DebugLogging.Value) corpseDumpAt = Time.time + 3f;
                    Active.Remove(id);
                }
            }
        }

        // The game only makes a body pick-up-able (its "ragdoll grabbable object") when a player dies WITH a
        // new body. A cocooned victim dies with the hanging stand-in body instead, so without this their
        // corpse could never be picked up. Only the host creates it, like the game does.
        static float corpseDumpAt = -1f;

        // The game's "bodyID" is either a plain int or a network variable holding one; handle both.
        static bool ReadBodyId(FieldInfo f, object inst, out int id)
        {
            id = -1;
            object v = f.GetValue(inst);
            if (v is int) { id = (int)v; return true; }
            if (v == null) return false;
            PropertyInfo vp = v.GetType().GetProperty("Value");
            if (vp == null) return false;
            object val = vp.GetValue(v, null);
            if (!(val is int)) return false;
            id = (int)val;
            return true;
        }

        static bool WriteBodyId(FieldInfo f, object inst, int id)
        {
            if (f.FieldType == typeof(int)) { f.SetValue(inst, id); return true; }
            object v = f.GetValue(inst);
            PropertyInfo vp = v != null ? v.GetType().GetProperty("Value") : null;
            if (vp == null) return false;
            vp.SetValue(v, id, null);
            return true;
        }

        // With DebugLogging on, 3 s after a wrapped player dies, write down what the game thinks of every
        // dead body and every pick-up object (so a body that cannot be picked up can be diagnosed).
        static readonly List<CocoonState> PendingStandIns = new List<CocoonState>();

        // The game's normal body can arrive a moment after the death is noticed. Once it is there the hanging
        // stand-in goes; if it never comes, fall back to making the stand-in itself pick-up-able.
        public static void TickStandIns()
        {
            for (int i = PendingStandIns.Count - 1; i >= 0; i--)
            {
                CocoonState s = PendingStandIns[i];
                try
                {
                    if (s.StandIn == null) { PendingStandIns.RemoveAt(i); continue; }
                    DeadBodyInfo cur = s.Player != null ? s.Player.deadBody : null;
                    if (cur != null && cur != s.StandIn)
                    {
                        DiscardStandIn(s, s.StandIn);
                        PendingStandIns.RemoveAt(i);
                    }
                    else if (Time.time > s.StandInDeadline)
                    {
                        PendingStandIns.RemoveAt(i);
                        // The game made no body for this death (an Earth Leviathan eating, a Forest Giant, a fall into the
                        // void...), and the cause is not our own: nothing should be left behind either.
                        if (cur == s.StandIn && s.Player != null && s.Player.isPlayerDead
                            && s.Player.causeOfDeath != CauseOfDeath.Suffocation && s.Player.causeOfDeath != CauseOfDeath.Mauling)
                        {
                            Plugin.Log.LogInfo("Player " + s.Player.playerClientId + " was consumed (" + s.Player.causeOfDeath + "): the hanging body is removed, no corpse is left.");
                            DiscardStandIn(s, s.StandIn);
                            if (s.Player.deadBody == s.StandIn) s.Player.deadBody = null;
                            continue;
                        }
                        if (cur != null)
                        {
                            cur.canBeGrabbedBackByPlayers = true;
                            if (cur.attachedTo != null && cur.grabBodyObject == null) { cur.attachedTo = null; cur.attachedLimb = null; cur.matchPositionExactly = false; }
                        }
                        EnsureCorpseGrabbable(s);
                    }
                }
                catch (Exception e) { PendingStandIns.RemoveAt(i); Plugin.Log.LogWarning("Stand-in clean-up failed: " + e.Message); }
            }
        }

        public static void TickCorpseDump()
        {
            if (corpseDumpAt < 0f || Time.time < corpseDumpAt) return;
            corpseDumpAt = -1f;
            try
            {
                Plugin.Log.LogInfo("=== Corpse dump ===");
                foreach (RagdollGrabbableObject r in UnityEngine.Object.FindObjectsOfType<RagdollGrabbableObject>())
                    Plugin.Log.LogInfo("Grab object '" + r.name + "' pos=" + r.transform.position + " " + DescribeFields(r));
                foreach (DeadBodyInfo d in UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>())
                    Plugin.Log.LogInfo("Dead body '" + d.name + "' pos=" + d.transform.position + " attachedTo=" + (d.attachedTo != null ? d.attachedTo.name : "-") + " " + DescribeFields(d));
            }
            catch (Exception e) { Plugin.Log.LogWarning("Corpse dump failed: " + e.Message); }
        }

        static string DescribeFields(object o)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (FieldInfo f in o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                {
                    object v = f.GetValue(o);
                    if (v == null) { sb.Append(f.Name).Append("=null "); continue; }
                    Type ft = f.FieldType;
                    if (ft == typeof(bool) || ft == typeof(int) || ft == typeof(float) || ft == typeof(string) || ft.IsEnum) sb.Append(f.Name).Append('=').Append(v).Append(' ');
                    else if (ft.Name.StartsWith("NetworkVariable"))
                    {
                        PropertyInfo vp = ft.GetProperty("Value");
                        sb.Append(f.Name).Append('=').Append(vp != null ? vp.GetValue(v, null) : "?").Append(' ');
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(ft)) sb.Append(f.Name).Append("=set ");
                }
                catch (Exception) { }
            }
            return sb.ToString();
        }

        private static void EnsureCorpseGrabbable(CocoonState s)
        {
            string stage = "start";
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer) return;
                PlayerControllerB p = s.Player;
                StartOfRound sor = StartOfRound.Instance;
                if (p == null || !p.isPlayerDead || sor == null) return;
                if (sor.ragdollGrabbableObjectPrefab == null) { Plugin.Log.LogWarning("Corpse pick-up: the game has no ragdoll grabbable prefab."); return; }
                int id = (int)p.playerClientId;

                stage = "find bodyID";
                FieldInfo bodyIdField = typeof(RagdollGrabbableObject).GetField("bodyID", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (bodyIdField == null) { Plugin.Log.LogWarning("Corpse pick-up: RagdollGrabbableObject.bodyID not found."); return; }

                stage = "look for an existing one";
                foreach (RagdollGrabbableObject r in UnityEngine.Object.FindObjectsOfType<RagdollGrabbableObject>())
                {
                    if (r == null) continue;
                    int existing;
                    if (ReadBodyId(bodyIdField, r, out existing) && existing == id) { Plugin.Dbg("Corpse pick-up: player " + id + " already has a grabbable body."); return; }
                }

                stage = "create";
                // Eaten by an Earth Leviathan: the game leaves no body at all, so none is made here either.
                if (p.deadBody == null) { Plugin.Dbg("Corpse pick-up: player " + id + " left no body (eaten), so none is made."); return; }
                Vector3 pos = p.deadBody != null ? p.deadBody.transform.position : p.transform.position;
                Transform parent = sor.propsContainer != null ? sor.propsContainer : null;
                GameObject go = parent != null
                    ? UnityEngine.Object.Instantiate(sor.ragdollGrabbableObjectPrefab, pos, Quaternion.identity, parent)
                    : UnityEngine.Object.Instantiate(sor.ragdollGrabbableObjectPrefab, pos, Quaternion.identity);

                stage = "find components";
                Unity.Netcode.NetworkObject netObj = go.GetComponentInChildren<Unity.Netcode.NetworkObject>(true);
                RagdollGrabbableObject rg = go.GetComponentInChildren<RagdollGrabbableObject>(true);
                if (netObj == null || rg == null)
                {
                    Plugin.Log.LogWarning("Corpse pick-up: the new object has netObj=" + (netObj != null) + " ragdoll=" + (rg != null));
                    UnityEngine.Object.Destroy(go);
                    return;
                }

                stage = "spawn";
                netObj.Spawn(false);

                stage = "set bodyID";
                if (!WriteBodyId(bodyIdField, rg, id))
                {
                    Plugin.Log.LogWarning("Corpse pick-up: could not set bodyID (field type " + bodyIdField.FieldType.Name + ").");
                    return;
                }
                Plugin.Log.LogInfo("Player " + id + " died in a cocoon: made their body pick-up-able.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not make the corpse pick-up-able (at step '" + stage + "'): " + e);
            }
        }

        // ------------------------------------------------------------------
        // The victim's camera: the game's spectator camera, placed in front of the
        // body and looking back at it, so you watch yourself being dragged away.
        // ------------------------------------------------------------------

        private static void StartCamera(CocoonState s)
        {
            StartOfRound sor = StartOfRound.Instance;
            if (sor == null || sor.spectateCamera == null) return;

            Camera cam = sor.spectateCamera;
            s.SavedLocalPos = cam.transform.localPosition;
            s.SavedLocalRot = cam.transform.localRotation;
            sor.SwitchCamera(cam);
            s.Player.ChangeAudioListenerToObject(cam.gameObject);
            s.CameraActive = true;
        }

        private static void StopCamera(CocoonState s, bool backToPlayer)
        {
            if (!s.CameraActive) return;
            s.CameraActive = false;

            StartOfRound sor = StartOfRound.Instance;
            if (sor == null) return;

            // Put the spectator camera back exactly how we found it.
            if (sor.spectateCamera != null)
            {
                sor.spectateCamera.transform.localPosition = s.SavedLocalPos;
                sor.spectateCamera.transform.localRotation = s.SavedLocalRot;
            }
            if (backToPlayer && s.Player != null && s.Player.gameplayCamera != null)
            {
                sor.SwitchCamera(s.Player.gameplayCamera);
                s.Player.ChangeAudioListenerToObject(s.Player.gameplayCamera.gameObject);
            }
        }

        public static void UpdateCameras()
        {
            if (Active.Count == 0) return;
            foreach (CocoonState s in Active.Values)
            {
                HideNameTag(s);
                if (!IsLocal(s.Player)) DisableColliders(s); // in case the game switched one back on
                for (int ri = 0; ri < s.Renderers.Count; ri++)
                {
                    if (s.Renderers[ri] != null && s.Renderers[ri].enabled) s.Renderers[ri].enabled = false; // and likewise the model
                }
                if (s.CameraActive && IsLocal(s.Player)) HideVisor(s.Player);
                if (!s.CameraActive) continue;
                try
                {
                    UpdateCamera(s);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Camera update failed, switching back: " + e);
                    StopCamera(s, true);
                }
            }
        }

        // The game moves the local player's helmet visor to their head every frame. While our camera
        // is elsewhere that leaves it floating in mid-air, so park it out of sight, exactly as the
        // game does when a player dies. (The game puts it back by itself once we stop doing this.)
        private static void HideVisor(PlayerControllerB p)
        {
            StartOfRound sor = StartOfRound.Instance;
            if (p == null || p.localVisor == null || sor == null || sor.notSpawnedPosition == null) return;
            p.localVisor.position = sor.notSpawnedPosition.position;
        }

        // The hidden player's body is still standing where they were caught, so the game would
        // keep showing their floating name tag when you look at it. Switch it off every frame.
        private static void HideNameTag(CocoonState s)
        {
            PlayerControllerB p = s.Player;
            if (p == null || IsLocal(p)) return;
            if (p.usernameAlpha != null) p.usernameAlpha.alpha = 0f;
            if (p.usernameCanvas != null && p.usernameCanvas.gameObject.activeSelf)
            {
                p.usernameCanvas.gameObject.SetActive(false);
            }
        }

        private static void UpdateCamera(CocoonState s)
        {
            StartOfRound sor = StartOfRound.Instance;
            PlayerControllerB p = s.Player;
            if (sor == null || p == null || sor.spectateCamera == null) return;
            if (p.deadBody == null || p.deadBody.bodyParts == null || p.deadBody.bodyParts.Length <= 6) return;

            Transform cam = sor.spectateCamera.transform;
            Vector3 anchor = p.deadBody.bodyParts[6].transform.position; // the spider carries the body by this part

            // While the spider is carrying the body, sit on the side of the body AWAY from the
            // spider, so we look back at the spider's face. Once hung up, keep the last angle.
            SandSpiderAI spider = s.Spider;
            if (!s.Hung && spider != null && !spider.isEnemyDead && spider.meshContainer != null)
            {
                Vector3 away = anchor - spider.meshContainer.position;
                away.y = 0f;
                if (away.sqrMagnitude > 0.25f) s.CamDir = away.normalized;
            }

            // The victim can look around their own body with the mouse (and zoom with the wheel).
            // Ignored while a menu has the cursor.
            UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 d = mouse.delta.ReadValue();
                float sens = Plugin.LookSensitivity.Value;
                s.OrbitYaw += d.x * sens;
                s.OrbitPitch = Mathf.Clamp(s.OrbitPitch - d.y * sens, -50f, 70f);
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f) s.ZoomMul = Mathf.Clamp(s.ZoomMul - Mathf.Sign(scroll) * 0.15f, 0.5f, 2.5f);
            }

            // Turn the "behind the body" direction by the victim's own yaw, and raise or lower it by their pitch.
            Vector3 horiz = Quaternion.AngleAxis(s.OrbitYaw, Vector3.up) * s.CamDir;
            float baseDist = Mathf.Sqrt(CamDistance * CamDistance + CamHeight * CamHeight) * s.ZoomMul;
            float elev = Mathf.Clamp(Mathf.Atan2(CamHeight, CamDistance) * Mathf.Rad2Deg + s.OrbitPitch, -40f, 80f) * Mathf.Deg2Rad;
            Vector3 offset = horiz * (baseDist * Mathf.Cos(elev)) + Vector3.up * (baseDist * Mathf.Sin(elev));
            Vector3 desired = anchor + offset;

            // Don't let the camera end up inside a wall.
            RaycastHit hit;
            if (Physics.Raycast(anchor, offset.normalized, out hit, offset.magnitude,
                    sor.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
            {
                desired = hit.point - offset.normalized * 0.25f;
            }

            float t = 1f - Mathf.Exp(-12f * Time.deltaTime); // smooth follow (quick enough to feel like mouse look)
            cam.position = s.CamInitialized ? Vector3.Lerp(cam.position, desired, t) : desired;

            Vector3 lookDir = anchor - cam.position;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(lookDir);
                cam.rotation = s.CamInitialized ? Quaternion.Slerp(cam.rotation, target, t) : target;
            }
            s.CamInitialized = true;
        }
    }

    // Config sync: while you are in someone else's lobby, the host's gameplay settings replace yours (in memory only;
    // your own config file is never touched and your values come back when you leave). Personal settings
    // (keys, testing, camera, display) stay yours.
    internal static class ConfigSync
    {
        internal const string RequestName = "SpiderCocoon.ConfigRequest";
        internal const string DataName = "SpiderCocoon.ConfigData";

        private static readonly Dictionary<ConfigEntryBase, string> Originals = new Dictionary<ConfigEntryBase, string>();
        private static bool requested;

        private static bool IsLocalOnly(ConfigEntryBase e)
        {
            string sec = e.Definition.Section, key = e.Definition.Key;
            if (sec == "Testing" || sec == "Camera") return true;
            if (e.SettingType == typeof(UnityEngine.InputSystem.Key)) return true;
            if (sec == "General" && (key == "EmergencyReleaseKey" || key == "ShowDeathTimer")) return true;
            if (sec == "Venom" && key == "VenomHeartbeat") return true;
            if (sec == "Spider" && key.StartsWith("Waiting")) return true;
            return false;
        }

        // A short fingerprint of the list of synced settings: it differs between builds that have different settings.
        private static string Signature()
        {
            List<string> names = new List<string>();
            foreach (ConfigEntryBase e in Plugin.Cfg.GetConfigEntries())
                if (!IsLocalOnly(e)) names.Add(e.Definition.Section + "/" + e.Definition.Key);
            names.Sort(StringComparer.Ordinal);
            int h = 17;
            foreach (string n in names) foreach (char c in n) h = unchecked(h * 31 + c);
            return names.Count + ":" + h.ToString("x");
        }

        private static string BuildPayload()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("SIG\t").Append(Signature()).Append('\n');
            foreach (ConfigEntryBase e in Plugin.Cfg.GetConfigEntries())
            {
                if (IsLocalOnly(e)) continue;
                sb.Append(e.Definition.Section).Append('\t').Append(e.Definition.Key).Append('\t').Append(e.GetSerializedValue()).Append('\n');
            }
            return sb.ToString();
        }

        private static void SendTo(ulong clientId)
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || nm.CustomMessagingManager == null) return;
            string payload = BuildPayload();
            int size = Unity.Netcode.FastBufferWriter.GetWriteSize(payload) + 16;
            using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(size, Unity.Collections.Allocator.Temp))
            {
                writer.WriteValueSafe(payload);
                nm.CustomMessagingManager.SendNamedMessage(DataName, clientId, writer, Unity.Netcode.NetworkDelivery.ReliableFragmentedSequenced);
            }
            Plugin.Log.LogInfo("Config sync: sent the host's settings to client " + clientId + " (" + payload.Length + " characters).");
        }

        // The host answers a joiner's request with its settings.
        internal static void OnRequest(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer) return;
                SendTo(senderClientId);
            }
            catch (Exception e) { Plugin.Log.LogError("Config sync request failed: " + e); }
        }

        // A joiner receives the host's settings and uses them while in this lobby.
        internal static void OnData(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || nm.IsServer) return; // the host never takes anyone else's settings
                string payload;
                reader.ReadValueSafe(out payload);
                Apply(payload);
            }
            catch (Exception e) { Plugin.Log.LogError("Config sync data failed: " + e); }
        }

        private static void Apply(string payload)
        {
            Dictionary<string, ConfigEntryBase> map = new Dictionary<string, ConfigEntryBase>();
            foreach (ConfigEntryBase e in Plugin.Cfg.GetConfigEntries()) map[e.Definition.Section + "/" + e.Definition.Key] = e;

            bool saveWas = Plugin.Cfg.SaveOnConfigSet;
            Plugin.Cfg.SaveOnConfigSet = false; // keep the host's values out of this player's config file
            int changed = 0, unknown = 0;
            string hostSig = null;
            try
            {
                foreach (string line in payload.Split('\n'))
                {
                    if (line.Length == 0) continue;
                    string[] p = line.Split(new char[] { '\t' }, 3);
                    if (p.Length < 2) continue;
                    if (p[0] == "SIG") { hostSig = p[1]; continue; }
                    if (p.Length < 3) continue;
                    ConfigEntryBase e;
                    if (!map.TryGetValue(p[0] + "/" + p[1], out e) || IsLocalOnly(e)) { unknown++; continue; }
                    string cur = e.GetSerializedValue();
                    if (cur == p[2]) continue;
                    if (!Originals.ContainsKey(e)) Originals[e] = cur;
                    e.SetSerializedValue(p[2]);
                    changed++;
                }
            }
            finally { Plugin.Cfg.SaveOnConfigSet = saveWas; }

            Plugin.Log.LogInfo(payload.StartsWith("PART") ? "Config sync: the host changed a setting (" + changed + " updated here)." : "Config sync: using the host's settings (" + changed + " differ from yours" + (unknown > 0 ? ", " + unknown + " unknown to this build and skipped" : "") + ").");
            string mine = Signature();
            if (hostSig != null && hostSig != mine)
            {
                Plugin.Log.LogWarning("Config sync: the host's Hung to Dry build differs from yours (settings list " + hostSig + " versus " + mine + "). Things may not work properly.");
                try
                {
                    if (HUDManager.Instance != null)
                        HUDManager.Instance.DisplayTip("Hung to Dry", "Your mod build is different from the host's. Get the host's version to avoid odd behaviour.", true);
                }
                catch (Exception) { }
            }
        }

        // Puts this player's own values back (on leaving the lobby).
        internal static void Restore()
        {
            if (Originals.Count == 0) return;
            bool saveWas = Plugin.Cfg.SaveOnConfigSet;
            Plugin.Cfg.SaveOnConfigSet = false;
            try
            {
                foreach (KeyValuePair<ConfigEntryBase, string> kv in Originals) kv.Key.SetSerializedValue(kv.Value);
            }
            finally { Plugin.Cfg.SaveOnConfigSet = saveWas; }
            Plugin.Log.LogInfo("Config sync: left the lobby, your own settings are back (" + Originals.Count + " restored).");
            Originals.Clear();
        }

        private static bool hooked;

        // The host: when a setting changes mid-game (an in-game config editor, say), tell every joiner at once.
        private static void HookChanges()
        {
            if (hooked || Plugin.Cfg == null) return;
            hooked = true;
            Plugin.Cfg.SettingChanged += OnSettingChanged;
        }

        private static void OnSettingChanged(object sender, SettingChangedEventArgs args)
        {
            try
            {
                ConfigEntryBase e = args != null ? args.ChangedSetting : null;
                if (e == null || IsLocalOnly(e)) return;
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (e == null || nm == null || !nm.IsListening || !nm.IsServer || nm.CustomMessagingManager == null) return;
                if (nm.ConnectedClientsIds.Count < 2) return; // nobody else is in the lobby
                string payload = "PART\n" + e.Definition.Section + "\t" + e.Definition.Key + "\t" + e.GetSerializedValue() + "\n";
                int size = Unity.Netcode.FastBufferWriter.GetWriteSize(payload) + 16;
                using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(size, Unity.Collections.Allocator.Temp))
                {
                    writer.WriteValueSafe(payload);
                    nm.CustomMessagingManager.SendNamedMessageToAll(DataName, writer, Unity.Netcode.NetworkDelivery.ReliableFragmentedSequenced);
                }
                Plugin.Log.LogInfo("Config sync: pushed the change to " + e.Definition.Key + " to the other players.");
            }
            catch (Exception ex) { Plugin.Log.LogWarning("Config sync: could not push a changed setting: " + ex.Message); }
        }

        // Called every frame.
        internal static void Tick()
        {
            HookChanges();
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            bool client = nm != null && nm.IsListening && nm.IsConnectedClient && !nm.IsServer;
            if (!client)
            {
                requested = false;
                Restore();
                return;
            }
            if (requested || !CocoonNet.Ready || nm.CustomMessagingManager == null) return;
            requested = true;
            try
            {
                using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(1, Unity.Collections.Allocator.Temp))
                {
                    writer.WriteByteSafe((byte)1);
                    nm.CustomMessagingManager.SendNamedMessage(RequestName, Unity.Netcode.NetworkManager.ServerClientId, writer);
                }
                Plugin.Log.LogInfo("Config sync: asked the host for its settings.");
            }
            catch (Exception e) { Plugin.Log.LogWarning("Config sync request could not be sent: " + e.Message); requested = false; }
        }
    }

    // Tiny custom messages so a rescue reaches every player's game.
    // A client asks the host; the host tells everyone (including itself) to release the victim.
    internal static class CocoonNet
    {
        private const string RequestName = "SpiderCocoon.RequestRelease";
        private const string ReleaseName = "SpiderCocoon.Release";
        private const string StealRequestName = "SpiderCocoon.RequestSteal";
        private const string StealName = "SpiderCocoon.Steal";
        private const string DropRequestName = "SpiderCocoon.RequestDrop";
        private const string DropName = "SpiderCocoon.Drop";
        internal static bool Ready { get { return registeredWith != null; } }

        private static Unity.Netcode.CustomMessagingManager registeredWith;
        private static bool reportedFailure;

        // Called every frame; registers our handlers as soon as the game's networking is up
        // (and again if the game restarts its networking, e.g. a new lobby).
        public static void EnsureRegistered()
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || !nm.IsListening)
            {
                registeredWith = null;
                return;
            }
            Unity.Netcode.CustomMessagingManager mm = nm.CustomMessagingManager;
            if (mm == null || ReferenceEquals(mm, registeredWith)) return;

            try
            {
                mm.RegisterNamedMessageHandler(RequestName, OnRequest);
                mm.RegisterNamedMessageHandler(ReleaseName, OnRelease);
                mm.RegisterNamedMessageHandler(StealRequestName, OnStealRequest);
                mm.RegisterNamedMessageHandler(StealName, OnSteal);
                mm.RegisterNamedMessageHandler(DropRequestName, OnDropRequest);
                mm.RegisterNamedMessageHandler(DropName, OnDrop);
                mm.RegisterNamedMessageHandler(ConfigSync.RequestName, ConfigSync.OnRequest);
                mm.RegisterNamedMessageHandler(ConfigSync.DataName, ConfigSync.OnData);
                mm.RegisterNamedMessageHandler(OutdoorSync.Name, OutdoorSync.OnMessage);
                mm.RegisterNamedMessageHandler(DepthSync.Name, DepthSync.OnMessage);
                mm.RegisterNamedMessageHandler(SpiderWeb.Name, SpiderWeb.OnMessage);
                mm.RegisterNamedMessageHandler(HangSync.Name, HangSync.OnMessage);
                registeredWith = mm;
                Plugin.Log.LogInfo("Hung to Dry network messages registered.");
            }
            catch (Exception e)
            {
                if (!reportedFailure)
                {
                    reportedFailure = true;
                    Plugin.Log.LogError("Could not register network messages: " + e);
                }
            }
        }

        public static void RequestRelease(int victimId)
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || nm.CustomMessagingManager == null) return;

            if (nm.IsServer)
            {
                Broadcast(victimId);
                return;
            }
            using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(1, Unity.Collections.Allocator.Temp))
            {
                writer.WriteByteSafe((byte)victimId);
                nm.CustomMessagingManager.SendNamedMessage(RequestName, Unity.Netcode.NetworkManager.ServerClientId, writer);
            }
        }

        // the spider's owner asks the host; the host tells every game (itself included) to apply the drop.
        public static void RequestDrop(int victimId, int attackerId)
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || nm.CustomMessagingManager == null) return;
            if (nm.IsServer) { BroadcastDrop(victimId, attackerId); return; }
            using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(2, Unity.Collections.Allocator.Temp))
            {
                writer.WriteByteSafe((byte)victimId);
                writer.WriteByteSafe((byte)attackerId);
                nm.CustomMessagingManager.SendNamedMessage(DropRequestName, Unity.Netcode.NetworkManager.ServerClientId, writer);
            }
        }

        private static void BroadcastDrop(int victimId, int attackerId)
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(2, Unity.Collections.Allocator.Temp))
            {
                writer.WriteByteSafe((byte)victimId);
                writer.WriteByteSafe((byte)attackerId);
                nm.CustomMessagingManager.SendNamedMessageToAll(DropName, writer);
            }
            Cocoon.ApplyDrop(victimId, attackerId); // the host does not receive its own broadcast
        }

        private static void OnDropRequest(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer) return;
                byte v, a;
                reader.ReadByteSafe(out v);
                reader.ReadByteSafe(out a);
                BroadcastDrop(v, a);
            }
            catch (Exception e) { Plugin.Log.LogError("Drop request failed: " + e); }
        }

        private static void OnDrop(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                byte v, a;
                reader.ReadByteSafe(out v);
                reader.ReadByteSafe(out a);
                Cocoon.ApplyDrop(v, a);
            }
            catch (Exception e) { Plugin.Log.LogError("Drop message failed: " + e); }
        }

        // A Bracken's owner asks the host; the host tells everyone (including itself) that this Bracken takes this victim.
        public static void RequestSteal(int victimId, ulong brackenNetworkId)
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || nm.CustomMessagingManager == null) return;
            if (nm.IsServer)
            {
                BroadcastSteal(victimId, brackenNetworkId);
                return;
            }
            using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(16, Unity.Collections.Allocator.Temp))
            {
                writer.WriteByteSafe((byte)victimId);
                writer.WriteValueSafe(brackenNetworkId);
                nm.CustomMessagingManager.SendNamedMessage(StealRequestName, Unity.Netcode.NetworkManager.ServerClientId, writer);
            }
        }

        private static void BroadcastSteal(int victimId, ulong brackenNetworkId)
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(16, Unity.Collections.Allocator.Temp))
            {
                writer.WriteByteSafe((byte)victimId);
                writer.WriteValueSafe(brackenNetworkId);
                nm.CustomMessagingManager.SendNamedMessageToAll(StealName, writer);
            }
            BrackenThief.ApplySteal(victimId, brackenNetworkId); // the host does not receive its own broadcast
        }

        private static void OnStealRequest(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer) return;
                byte id;
                ulong netId;
                reader.ReadByteSafe(out id);
                reader.ReadValueSafe(out netId);
                BroadcastSteal(id, netId);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Steal request failed: " + e);
            }
        }

        private static void OnSteal(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                byte id;
                ulong netId;
                reader.ReadByteSafe(out id);
                reader.ReadValueSafe(out netId);
                BrackenThief.ApplySteal(id, netId);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Steal message failed: " + e);
            }
        }

        private static void Broadcast(int victimId)
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            using (Unity.Netcode.FastBufferWriter writer = new Unity.Netcode.FastBufferWriter(1, Unity.Collections.Allocator.Temp))
            {
                writer.WriteByteSafe((byte)victimId);
                nm.CustomMessagingManager.SendNamedMessageToAll(ReleaseName, writer);
            }
            Cocoon.ApplyRelease(victimId); // the host does not receive its own broadcast
        }

        private static void OnRequest(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer) return;
                byte id;
                reader.ReadByteSafe(out id);
                Broadcast(id);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Rescue request failed: " + e);
            }
        }

        private static void OnRelease(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                byte id;
                reader.ReadByteSafe(out id);
                Cocoon.ApplyRelease(id);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Rescue release failed: " + e);
            }
        }
    }

    // A Bracken that notices a hung victim may walk over, cut them down and carry them off to its lair.
    // Only the Bracken's owner decides; the host then tells every game, which all do the same swap.
    internal static class BrackenThief
    {
        private sealed class Lure
        {
            public int VictimId = -1;
            public float Until;
            public float NextCheck;
            public HashSet<CocoonState> Rolled = new HashSet<CocoonState>(); // per catch, not per player slot
            public bool LoggedRunning;
            public float NextDistanceLog;
            public int LastSkipReason = -1;
            public int AmbushId = -1;
            public Vector3 HidePos;
            public bool Waiting;
            public float AmbushUntil;
            public Vector3 SpiderPos;
            public float SpiderStill;
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FlowermanAI, Lure> Lures
            = new System.Runtime.CompilerServices.ConditionalWeakTable<FlowermanAI, Lure>();

        // A victim that is hung up, still on their rope, and not already taken.
        internal static bool Available(CocoonState s, out DeadBodyInfo body)
        {
            body = null;
            if (s == null || !s.Hung || s.Thief != null || s.BugThief != null || s.Player == null || s.Player.isPlayerDead) return false;
            body = s.Player.deadBody;
            return body != null && body.attachedTo != null && body.bodyParts != null && body.bodyParts.Length > 6;
        }

        // A victim a thief already carried off and put down (for example at a loot bug's nest). Only a Bracken may take it from there.
        internal static bool AvailableDropped(CocoonState s, out DeadBodyInfo body)
        {
            body = null;
            if (s == null || !s.Hung || !s.WasStolen || s.BrackenDropped || s.Thief != null || s.BugThief != null || s.Player == null || s.Player.isPlayerDead) return false;
            body = s.Player.deadBody;
            return body != null && body.bodyParts != null && body.bodyParts.Length > 6;
        }

        // What a Bracken is allowed to take: a hung victim, or one a thief has put down.
        internal static bool StealableByBracken(CocoonState s, out DeadBodyInfo body)
        {
            return Available(s, out body) || AvailableDropped(s, out body);
        }

        // Any wrapped-up victim (still being dragged, or hung) that nobody has carried off yet. A Bracken keeps
        // its eye on these, so it does not wander away the moment its prey becomes untargetable.
        internal static bool Interesting(CocoonState s, out DeadBodyInfo body)
        {
            body = null;
            if (s == null || s.Thief != null || s.BugThief != null || s.Player == null || s.Player.isPlayerDead) return false;
            body = s.Player.deadBody;
            return body != null && body.bodyParts != null && body.bodyParts.Length > 6;
        }

        internal static bool PlayerNear(Vector3 pos, PlayerControllerB except)
        {
            StartOfRound sor = StartOfRound.Instance;
            if (sor == null) return false;
            float r = Plugin.BrackenScaredMeters.Value;
            foreach (PlayerControllerB p in sor.allPlayerScripts)
            {
                if (p == null || p == except || !p.isPlayerControlled || p.isPlayerDead || Cocoon.IsCocooned(p)) continue;
                if (Vector3.Distance(p.transform.position, pos) < r) return true;
            }
            return false;
        }

        internal static Vector3 FloorUnder(DeadBodyInfo body)
        {
            Vector3 pos = body.bodyParts[6].transform.position;
            RaycastHit hit;
            if (Physics.Raycast(pos, Vector3.down, out hit, 25f, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
                return hit.point;
            return pos;
        }

        // Brackens that are currently going for a cocoon (hiding for it, or walking to it), and which victim.
        internal static readonly Dictionary<FlowermanAI, int> Pursuers = new Dictionary<FlowermanAI, int>();

        // Is a Bracken going for a cocoon within range of this spot (either the Bracken itself, or the cocoon it wants)?
        internal static bool PursuerNear(Vector3 pos, float range, out Vector3 threat)
        {
            threat = Vector3.zero;
            bool found = false;
            float best = range;
            List<FlowermanAI> gone = null;
            foreach (KeyValuePair<FlowermanAI, int> kv in Pursuers)
            {
                FlowermanAI f = kv.Key;
                if (f == null || f.isEnemyDead)
                {
                    if (gone == null) gone = new List<FlowermanAI>();
                    gone.Add(f);
                    continue;
                }
                float d = Vector3.Distance(f.transform.position, pos);
                if (d < best) { best = d; threat = f.transform.position; found = true; }
                CocoonState cs;
                DeadBodyInfo cb;
                if (Cocoon.ActiveStates.TryGetValue(kv.Value, out cs) && Interesting(cs, out cb))
                {
                    Vector3 cp = cb.bodyParts[6].transform.position;
                    float dc = Vector3.Distance(cp, pos);
                    if (dc < best) { best = dc; threat = cp; found = true; }
                }
            }
            if (gone != null)
            {
                foreach (FlowermanAI f in gone) Pursuers.Remove(f);
            }
            return found;
        }

        // Called every frame on every Bracken; only the owner does anything.
        internal static void Tick(FlowermanAI b)
        {
            TickInner(b);
            try
            {
                Lure l;
                if (b != null && b.IsOwner && !b.isEnemyDead && Lures.TryGetValue(b, out l) && (l.AmbushId >= 0 || l.VictimId >= 0))
                    Pursuers[b] = l.AmbushId >= 0 ? l.AmbushId : l.VictimId;
                else if (b != null)
                    Pursuers.Remove(b);
            }
            catch (Exception) { }
        }

        private static void TickInner(FlowermanAI b)
        {
            if (b == null) return;
            if (!b.IsOwner)
            {
                // Ownership moved to another player's game: stop holding this Bracken still.
                Lure lo;
                if (Lures.TryGetValue(b, out lo) && lo.AmbushId >= 0) EndAmbush(b, lo, null, false);
                return;
            }
            if (!Plugin.BrackenSteals.Value) return;
            Dictionary<int, CocoonState> active = Cocoon.ActiveStates;
            if (active.Count == 0)
            {
                Lure none;
                if (Lures.TryGetValue(b, out none))
                {
                    if (none.AmbushId >= 0) EndAmbush(b, none, null, false);
                    none.VictimId = -1;
                }
                return;
            }

            Lure l = Lures.GetOrCreateValue(b);
            if (!l.LoggedRunning)
            {
                l.LoggedRunning = true;
                Plugin.Dbg("Bracken steal logic is watching this Bracken (state " + b.currentBehaviourStateIndex + ").");
            }
            bool chasingSomeone = b.currentBehaviourStateIndex == 2 && b.targetPlayer != null && b.PlayerIsTargetable(b.targetPlayer);
            if (b.isEnemyDead || b.inKillAnimation || b.carryingPlayerBody || chasingSomeone)
            {
                if (l.LastSkipReason != b.currentBehaviourStateIndex && Time.time >= l.NextDistanceLog)
                {
                    l.LastSkipReason = b.currentBehaviourStateIndex;
                    l.NextDistanceLog = Time.time + 5f;
                    Plugin.Dbg("Bracken is busy (state " + b.currentBehaviourStateIndex + ", carrying=" + b.carryingPlayerBody + ", killing=" + b.inKillAnimation + "), not looking for a cocoon.");
                }
                Abandon(b, l, active);
                return;
            }
            l.LastSkipReason = -1;

            // Hiding out of sight until the spider has hung the cocoon and gone invisible.
            if (l.AmbushId >= 0)
            {
                CocoonState sa;
                DeadBodyInfo ba;
                if (!active.TryGetValue(l.AmbushId, out sa) || !Interesting(sa, out ba) || Time.time > l.AmbushUntil || sa.Claim != b)
                {
                    Plugin.Log.LogInfo("Bracken gives up its ambush.");
                    Abandon(b, l, active);
                    return;
                }
                DeadBodyInfo hungA;
                if (StealableByBracken(sa, out hungA) && Time.time - sa.HungAt >= Plugin.BrackenStealDelaySeconds.Value
                    && !PlayerNear(ba.bodyParts[6].transform.position, null))
                {
                    bool gone = SpiderIsGone(sa, l);
                    bool impatient = Time.time - sa.HungAt >= Plugin.BrackenAmbushPatienceSeconds.Value;
                    if (gone || impatient)
                    {
                        int ambushId = l.AmbushId;
                        EndAmbush(b, l, sa, false);
                        l.VictimId = ambushId;
                        l.Until = Time.time + Mathf.Max(5f, Plugin.BrackenGiveUpSeconds.Value);
                        Plugin.Log.LogInfo(gone
                            ? "Bracken leaves its hiding place: the cocoon is hung and the spider is out of sight."
                            : "Bracken leaves its hiding place: it has waited " + (Time.time - sa.HungAt).ToString("F0") + " s since the cocoon was hung and the spider is still about.");
                    }
                }
                return;
            }

            if (l.VictimId >= 0)
            {
                CocoonState s;
                DeadBodyInfo body;
                if (!active.TryGetValue(l.VictimId, out s) || !Interesting(s, out body) || Time.time > l.Until
                    || PlayerNear(body.bodyParts[6].transform.position, null))
                {
                    Plugin.Log.LogInfo("Bracken gives up on the victim.");
                    DropClaim(s, b);
                    l.VictimId = -1;
                    return;
                }
                Vector3 bp = body.bodyParts[6].transform.position;
                Vector3 d = b.transform.position - bp;
                d.y = 0f;
                DeadBodyInfo hungBody;
                if (StealableByBracken(s, out hungBody) && Time.time - s.HungAt >= Plugin.BrackenStealDelaySeconds.Value && d.magnitude <= Plugin.BrackenReachMeters.Value && Mathf.Abs(b.transform.position.y - FloorUnder(body).y) < 2.5f)
                {
                    int id = l.VictimId;
                    l.VictimId = -1;
                    Plugin.Log.LogInfo("Bracken takes player " + id + "'s cocoon.");
                    CocoonNet.RequestSteal(id, b.NetworkObjectId);
                }
                return;
            }

            if (Time.time < l.NextCheck) return;
            l.NextCheck = Time.time + 0.5f;

            int bestId = -1;
            float best = Plugin.BrackenNoticeMeters.Value;
            float nearestAny = float.MaxValue;
            bool anyAvailable = false;
            foreach (KeyValuePair<int, CocoonState> kv in active)
            {
                DeadBodyInfo body;
                if (!Interesting(kv.Value, out body) || l.Rolled.Contains(kv.Value) || !Claimable(kv.Value, b, 2)) continue;
                anyAvailable = true;
                float dist = Vector3.Distance(b.transform.position, body.bodyParts[6].transform.position);
                if (dist < nearestAny) nearestAny = dist;
                if (dist < best) { best = dist; bestId = kv.Key; }
            }
            if (bestId < 0)
            {
                if (anyAvailable && Time.time >= l.NextDistanceLog)
                {
                    l.NextDistanceLog = Time.time + 20f;
                    Plugin.Dbg("Bracken: a hung victim is " + nearestAny.ToString("F1") + " m away, outside its notice range of " + Plugin.BrackenNoticeMeters.Value.ToString("F0") + " m.");
                }
                return;
            }

            l.Rolled.Add(active[bestId]);
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll >= Plugin.BrackenStealChancePercent.Value)
            {
                Plugin.Dbg("Bracken noticed a wrapped victim (" + best.ToString("F1") + " m) but passed: rolled " + roll.ToString("F0") + ", needed under " + Plugin.BrackenStealChancePercent.Value.ToString("F0") + ".");
                return;
            }
            CocoonState target = active[bestId];
            DeadBodyInfo chosen;
            if (!Interesting(target, out chosen)) return;
            if (!Claimable(target, b, 2)) return;

            // Hide first, strike later.
            if (Plugin.BrackenAmbush.Value && TryStartAmbush(b, l, chosen))
            {
                SetClaim(target, b, 2);
                l.AmbushId = bestId;
                l.AmbushUntil = Time.time + Mathf.Max(10f, Plugin.BrackenAmbushMaxWaitSeconds.Value);
                l.SpiderPos = target.Spider != null ? target.Spider.transform.position : Vector3.zero;
                l.SpiderStill = Time.time;
                return;
            }

            if (PlayerNear(chosen.bodyParts[6].transform.position, null))
            {
                Plugin.Dbg("Bracken noticed a hung victim but another player is within " + Plugin.BrackenScaredMeters.Value.ToString("F0") + " m of them.");
                return;
            }
            SetClaim(target, b, 2);
            l.VictimId = bestId;
            l.Until = Time.time + Mathf.Max(5f, Plugin.BrackenGiveUpSeconds.Value);
            Plugin.Log.LogInfo("Bracken has noticed a wrapped victim (" + best.ToString("F1") + " m away) and is following them.");
        }

        // Runs after the Bracken's own AI step: point it at the victim.
        static float nextCarryLog;
        static readonly Dictionary<FlowermanAI, Vector3> CarryGoals = new Dictionary<FlowermanAI, Vector3>();
        // While a Bracken carries a stolen cocoon, make sure it really walks away with it.
        static float nextAnimLog;
        // While carrying, the Bracken must look like it is walking. If its animator has been switched off or
        // slowed to a standstill (by the game or another mod), switch it back on. With DebugLogging on, also
        // write what the animator is doing every 2 s so a sliding Bracken can be diagnosed.
        static void KeepWalkAnimation(FlowermanAI b)
        {
            try
            {
                Animator an = b.creatureAnimator;
                if (an == null) return;
                if (!an.enabled) { an.enabled = true; Plugin.Dbg("Bracken animator was off while carrying: switched it back on."); }
                if (an.speed < 0.5f) { Plugin.Dbg("Bracken animator speed was " + an.speed.ToString("0.00") + " while carrying: set to 1."); an.speed = 1f; }
                if (Time.time < nextAnimLog) return;
                nextAnimLog = Time.time + 2f;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (AnimatorControllerParameter ap in an.parameters)
                {
                    sb.Append(ap.name).Append('=');
                    if (ap.type == AnimatorControllerParameterType.Bool) sb.Append(an.GetBool(ap.name));
                    else if (ap.type == AnimatorControllerParameterType.Float) sb.Append(an.GetFloat(ap.name).ToString("0.00"));
                    else if (ap.type == AnimatorControllerParameterType.Int) sb.Append(an.GetInteger(ap.name));
                    else sb.Append("trigger");
                    sb.Append(' ');
                }
                AnimatorStateInfo st = an.GetCurrentAnimatorStateInfo(0);
                float vel = b.agent != null ? b.agent.velocity.magnitude : -1f;
                Plugin.Dbg("Bracken animator: " + sb.ToString() + "| layer0 state=" + st.shortNameHash + " t=" + st.normalizedTime.ToString("0.00") +
                    " speed=" + an.speed.ToString("0.00") + " layer1 weight=" + (an.layerCount > 1 ? an.GetLayerWeight(1).ToString("0.00") : "-") +
                    " agent velocity=" + vel.ToString("0.0"));
            }
            catch (Exception) { }
        }

        static void DriveCarry(FlowermanAI b)
        {
            bool mine = false;
            foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
            {
                if (kv.Value.Thief != b) continue;
                if (b.carryingPlayerBody) mine = true;
                else
                {
                    // the game made the Bracken drop it (it reached its lair, or it was stunned): stop forcing
                    // its movement, and do not let it pick the same cocoon straight up again
                    Plugin.Log.LogInfo("Bracken has put the cocoon down.");
                    kv.Value.BrackenDropped = true;
                    ClearDragPoint(kv.Value, kv.Value.Player != null ? kv.Value.Player.deadBody : null);
                    kv.Value.Thief = null;
                }
            }
            if (!mine) { CarryGoals.Remove(b); return; }

            // Pick the place to carry the cocoon to ONCE: the Bracken's own lair (its favourite spot), or a far
            // corner if it is already standing at its lair. Re-picking on arrival sent it running back and forth.
            Vector3 goal;
            if (!CarryGoals.TryGetValue(b, out goal))
            {
                goal = b.favoriteSpot != null ? b.favoriteSpot.position : b.transform.position;
                if (b.favoriteSpot == null || Vector3.Distance(goal, b.transform.position) < 3f)
                {
                    Transform far = b.ChooseFarthestNodeFromPosition(b.transform.position);
                    if (far != null) { b.favoriteSpot = far; goal = far.position; }
                }
                CarryGoals[b] = goal;
            }

            Vector3 flat = b.transform.position - goal;
            flat.y = 0f;
            if (flat.magnitude < 3f)
            {
                // Arrived: put the cocoon down here and let the Bracken go back to normal.
                Plugin.Log.LogInfo("Bracken puts the cocoon down at its lair.");
                CarryGoals.Remove(b);
                foreach (KeyValuePair<int, CocoonState> kv in new List<KeyValuePair<int, CocoonState>>(Cocoon.ActiveStates))
                {
                    if (kv.Value.Thief != b) continue;
                    kv.Value.BrackenDropped = true;
                    ReleaseCarry(kv.Value, kv.Value.Player != null ? kv.Value.Player.deadBody : null);
                }
                return;
            }
            KeepWalkAnimation(b);
            b.movingTowardsTargetPlayer = false;
            b.moveTowardsDestination = true;
            if (b.agent != null && b.agent.enabled && b.agent.isOnNavMesh)
            {
                b.agent.isStopped = false;
                if (b.agent.speed < 4f) b.agent.speed = 4f;
                b.agent.SetDestination(goal);
                b.destination = goal;
            }
            if (Time.time >= nextCarryLog)
            {
                nextCarryLog = Time.time + 2f;
                Plugin.Dbg("Bracken carry: speed=" + (b.agent != null ? b.agent.speed.ToString("0.0") : "?") +
                    " onMesh=" + (b.agent != null && b.agent.isOnNavMesh) + " enabled=" + (b.agent != null && b.agent.enabled) +
                    " pathPending=" + (b.agent != null && b.agent.pathPending) + " dist=" + Vector3.Distance(goal, b.transform.position).ToString("0.0") +
                    " state=" + b.currentBehaviourStateIndex + " stunned=" + b.stunNormalizedTimer + " inKill=" + b.inKillAnimation);
            }
        }

        internal static void Steer(FlowermanAI b)
        {
            if (b == null || !b.IsOwner) return;
            DriveCarry(b);
            Lure l;
            if (!Lures.TryGetValue(b, out l)) return;

            // Ambush: walk to the hiding place, then stand still and wait.
            if (l.AmbushId >= 0)
            {
                Vector3 toHide = b.transform.position - l.HidePos;
                toHide.y = 0f;
                b.movingTowardsTargetPlayer = false;
                if (!l.Waiting && toHide.magnitude > 1.5f)
                {
                    b.moveTowardsDestination = true;
                    if (b.agent != null && b.agent.enabled && b.agent.isOnNavMesh)
                    {
                        b.agent.isStopped = false;
                        b.SetDestinationToPosition(l.HidePos, true);
                        b.agent.SetDestination(l.HidePos);
                    }
                }
                else
                {
                    if (!l.Waiting)
                    {
                        l.Waiting = true;
                        Plugin.Log.LogInfo("Bracken is in its hiding place, waiting.");
                    }
                    b.moveTowardsDestination = false;
                    if (b.agent != null && b.agent.enabled && b.agent.isOnNavMesh) b.agent.isStopped = true;
                }
                return;
            }

            if (l.VictimId < 0) return;
            CocoonState s;
            DeadBodyInfo body;
            if (!Cocoon.ActiveStates.TryGetValue(l.VictimId, out s) || !Interesting(s, out body)) return;
            Vector3 target = FloorUnder(body);
            b.movingTowardsTargetPlayer = false;
            if (!b.SetDestinationToPosition(target, true))
            {
                Plugin.Log.LogWarning("Bracken cannot reach the hung victim.");
                DropClaim(s, b);
                l.VictimId = -1;
                return;
            }
            if (b.agent != null && b.agent.enabled && b.agent.isOnNavMesh) b.agent.SetDestination(target);
        }

        // Runs on EVERY game: the Bracken takes the victim's body.
        internal static void ApplySteal(int victimId, ulong brackenNetworkId)
        {
            try
            {
                CocoonState s;
                if (!Cocoon.ActiveStates.TryGetValue(victimId, out s)) return;
                DeadBodyInfo body;
                bool hung = Available(s, out body);
                if (!hung && !AvailableDropped(s, out body)) return;

                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                Unity.Netcode.NetworkObject no;
                if (nm == null || nm.SpawnManager == null || !nm.SpawnManager.SpawnedObjects.TryGetValue(brackenNetworkId, out no)) return;
                FlowermanAI b = no.GetComponent<FlowermanAI>();
                if (b == null)
                {
                    if (!hung) return; // loot bugs only take a cocoon that is hung up, never one lying on the floor
                    LootBugThief.Apply(victimId, s, body, no.GetComponent<HoarderBugAI>());
                    return;
                }
                if (b.isEnemyDead || b.carryingPlayerBody || b.rightHandGrip == null) return;

                Transform hangPoint = body.attachedTo;
                // The spider's hold left another body part frozen in place (kinematic). Free it and let the
                // game set the body up again for the new holder, or the body stays where it hung.
                for (int bp = 0; bp < body.bodyParts.Length; bp++)
                {
                    if (body.bodyParts[bp] == null) continue;
                    body.bodyParts[bp].isKinematic = false;
                    body.bodyParts[bp].freezeRotation = false;
                }
                HarmonyLib.AccessTools.Field(typeof(DeadBodyInfo), "wasMatchingPosition")?.SetValue(body, false);
                Transform grip = b.rightHandGrip;
                bool dragging = false;
                string style = Plugin.BrackenCarryStyle.Value ?? "Shoulder";
                if (!string.Equals(style.Trim(), "Hand", StringComparison.OrdinalIgnoreCase))
                {
                    // Hold the cocoon on an empty point riding on the Bracken, instead of in its hand.
                    GameObject dp = new GameObject("CocoonCarryPoint");
                    dp.transform.SetParent(b.transform, false);
                    float sc = Mathf.Max(0.01f, Mathf.Abs(b.transform.lossyScale.y));
                    if (string.Equals(style.Trim(), "Drag", StringComparison.OrdinalIgnoreCase))
                    {
                        dp.transform.localPosition = new Vector3(0f, Plugin.BrackenDragHeight.Value / sc, -Plugin.BrackenDragDistance.Value / sc);
                    }
                    else
                    {
                        dp.transform.localPosition = new Vector3(Plugin.BrackenCarrySide.Value / sc, Plugin.BrackenCarryHeight.Value / sc, -Plugin.BrackenCarryBack.Value / sc);
                        dp.transform.localRotation = Quaternion.Euler(Plugin.BrackenCarryPitch.Value, Plugin.BrackenCarryYaw.Value, Plugin.BrackenCarryRoll.Value);
                    }
                    s.BrackenDragPoint = dp.transform;
                    grip = dp.transform;
                    dragging = true;
                    Plugin.Dbg("Bracken carries the cocoon in '" + style.Trim() + "' style.");
                }
                body.attachedTo = grip;
                body.attachedLimb = body.bodyParts[0];
                body.matchPositionExactly = true;
                b.bodyBeingCarried = body;
                b.carryingPlayerBody = true;
                if (!dragging && b.creatureAnimator != null) b.creatureAnimator.SetBool("carryingBody", true);
                s.Thief = b;
                s.WasStolen = true;
                s.ExtraSeconds += Plugin.BrackenStealExtraSeconds.Value;
                if (hangPoint != null) Cocoon.DestroyRope(hangPoint);

                if (b.IsOwner)
                {
                    if (b.favoriteSpot == null) b.favoriteSpot = b.ChooseClosestNodeToPosition(b.transform.position, true);
                    b.SwitchToBehaviourState(1);
                }
                Plugin.Log.LogInfo("A Bracken carries player " + victimId + "'s cocoon away.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Bracken steal failed: " + e);
            }
        }

        // ---- Pecking order: one thief at a time has first call on a cocoon. A Bracken (rank 2) outranks a loot bug (rank 1). ----
        internal static bool Claimable(CocoonState s, EnemyAI who, int rank)
        {
            if (s == null) return false;
            if (s.Claim == null || s.Claim == who || s.Claim.isEnemyDead) return true;
            return rank > s.ClaimRank;
        }

        internal static void SetClaim(CocoonState s, EnemyAI who, int rank)
        {
            if (s == null) return;
            s.Claim = who;
            s.ClaimRank = rank;
        }

        internal static void DropClaim(CocoonState s, EnemyAI who)
        {
            if (s == null) return;
            if (s.Claim == who) { s.Claim = null; s.ClaimRank = 0; }
        }

        // Leaves the Bracken free to move again after an ambush.
        private static void EndAmbush(FlowermanAI b, Lure l, CocoonState s, bool dropClaim)
        {
            if (dropClaim) DropClaim(s, b);
            l.AmbushId = -1;
            l.Waiting = false;
            try
            {
                if (b != null)
                {
                    b.moveTowardsDestination = true;
                    if (b.agent != null && b.agent.enabled && b.agent.isOnNavMesh) b.agent.isStopped = false;
                }
            }
            catch (Exception) { }
        }

        // Stop everything this Bracken was set on (ambush and/or following a victim) and let go of its claims.
        private static void Abandon(FlowermanAI b, Lure l, Dictionary<int, CocoonState> active)
        {
            CocoonState sx;
            if (l.AmbushId >= 0)
            {
                active.TryGetValue(l.AmbushId, out sx);
                EndAmbush(b, l, sx, true);
            }
            if (l.VictimId >= 0)
            {
                if (active.TryGetValue(l.VictimId, out sx)) DropClaim(sx, b);
                l.VictimId = -1;
            }
        }

        // The spider has finished hanging its prey and is out of sight (faded out), or, if it does not fade, has stood still a while.
        private static bool SpiderIsGone(CocoonState s, Lure l)
        {
            if (s.WasStolen) return true; // the spider is long gone from this cocoon
            SandSpiderAI sp = s.Spider;
            if (sp == null || sp.isEnemyDead) return true;
            int hidden = SpiderFadePatch.HiddenState(sp);
            if (hidden == 1) return true;
            if (hidden == 0) return false;
            Vector3 pos = sp.transform.position;
            if ((pos - l.SpiderPos).sqrMagnitude > 0.04f)
            {
                l.SpiderPos = pos;
                l.SpiderStill = Time.time;
            }
            return Time.time - l.SpiderStill >= 3f;
        }

        // 0 = nobody can see this spot, 1 = it could see the cocoon from here, 2 = a living player could see it.
        private static int HiddenReason(Vector3 pos, Vector3 bodyPos)
        {
            StartOfRound sor = StartOfRound.Instance;
            if (sor == null) return 1;
            int mask = sor.collidersAndRoomMask;
            Vector3 head = pos + Vector3.up * 1.7f;
            if (!Physics.Linecast(head, bodyPos, mask, QueryTriggerInteraction.Ignore)) return 1;
            foreach (PlayerControllerB pl in sor.allPlayerScripts)
            {
                if (pl == null || !pl.isPlayerControlled || pl.isPlayerDead || Cocoon.IsCocooned(pl) || pl.gameplayCamera == null) continue;
                if (!Physics.Linecast(pl.gameplayCamera.transform.position, head, mask, QueryTriggerInteraction.Ignore)) return 2;
            }
            return 0;
        }

        // Pick somewhere out of sight, 8 to 16 m (by default) from the cocoon, that the Bracken can walk to.
        // If there is nowhere, look again over a wider ring before giving up.
        private static bool TryStartAmbush(FlowermanAI b, Lure l, DeadBodyInfo body)
        {
            try
            {
                if (b.allAINodes == null || b.allAINodes.Length == 0) return false;
                Vector3 bodyPos = body.bodyParts[6].transform.position;
                float min0 = Mathf.Max(3f, Plugin.BrackenAmbushMinMeters.Value);
                float max0 = Mathf.Max(min0 + 2f, Plugin.BrackenAmbushMaxMeters.Value);
                Vector3 here = b.transform.position;

                float dHere = Vector3.Distance(here, bodyPos);
                if (dHere >= min0 && dHere <= max0 && HiddenReason(here, bodyPos) == 0)
                {
                    l.HidePos = here;
                    l.Waiting = false;
                    Plugin.Dbg("Bracken is already out of sight " + dHere.ToString("F1") + " m from the victim and waits there.");
                    return true;
                }

                for (int pass = 0; pass < 2; pass++)
                {
                    float min = pass == 0 ? min0 : Mathf.Max(3f, min0 * 0.6f);
                    float max = pass == 0 ? max0 : max0 * 1.6f;
                    int inRange = 0, seesCocoon = 0, seenByPlayer = 0;
                    List<KeyValuePair<float, Vector3>> found = new List<KeyValuePair<float, Vector3>>();
                    foreach (GameObject node in b.allAINodes)
                    {
                        if (node == null) continue;
                        Vector3 p = node.transform.position;
                        float d = Vector3.Distance(p, bodyPos);
                        if (d < min || d > max) continue;
                        inRange++;
                        int why = HiddenReason(p, bodyPos);
                        if (why == 1) { seesCocoon++; continue; }
                        if (why == 2) { seenByPlayer++; continue; }
                        found.Add(new KeyValuePair<float, Vector3>((p - here).sqrMagnitude, p));
                    }
                    found.Sort((x, y) => x.Key.CompareTo(y.Key));
                    for (int i = 0; i < found.Count && i < 6; i++)
                    {
                        if (b.SetDestinationToPosition(found[i].Value, true))
                        {
                            l.HidePos = found[i].Value;
                            l.Waiting = false;
                            Plugin.Dbg("Bracken will hide out of sight (" + found.Count + " hiding spots between " + min.ToString("F0") + " and " + max.ToString("F0") + " m) and wait for the spider to finish.");
                            return true;
                        }
                    }
                    Plugin.Dbg("Bracken hiding search " + min.ToString("F0") + "-" + max.ToString("F0") + " m: " + inRange + " nodes in range, "
                        + seesCocoon + " can see the cocoon, " + seenByPlayer + " can be seen by a player, " + found.Count + " usable but none reachable.");
                }
                Plugin.Log.LogInfo("Bracken found no hiding spot near the victim, going straight for it.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Bracken ambush setup failed: " + e.Message);
            }
            return false;
        }

        // Takes the cocoon off the empty point it was dragged on (and removes that point).
        private static void ClearDragPoint(CocoonState s, DeadBodyInfo body)
        {
            if (s == null) return;
            Transform point = s.BrackenDragPoint;
            s.BrackenDragPoint = null;
            if (point == null) return;
            try
            {
                if (body != null && body.attachedTo == point)
                {
                    body.matchPositionExactly = false;
                    body.attachedTo = null;
                }
                else if (body == null)
                {
                    // The victim's body is no longer known (they died): find whatever is still glued to the point.
                    foreach (DeadBodyInfo d in UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>())
                    {
                        if (d != null && d.attachedTo == point)
                        {
                            d.matchPositionExactly = false;
                            d.attachedTo = null;
                        }
                    }
                }
            }
            catch (Exception) { }
            UnityEngine.Object.Destroy(point.gameObject);
        }

        private sealed class ArmBones
        {
            internal Transform Upper, Lower, Hand;
            internal bool Right;
            internal bool Looked;
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FlowermanAI, ArmBones> ArmTable
            = new System.Runtime.CompilerServices.ConditionalWeakTable<FlowermanAI, ArmBones>();

        private static Transform FindChild(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == name) return t;
            }
            return null;
        }

        // Runs after the game's animation each frame: swings the arm on the carrying side up and over
        // the cocoon with a simple two-bone solve (visual only, nothing is networked).
        internal static void ArmPose()
        {
            if (!Plugin.BrackenArmOver.Value) return;
            foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
            {
                CocoonState st = kv.Value;
                FlowermanAI b = st != null ? st.Thief : null;
                if (b == null || st.BrackenDragPoint == null || !b.carryingPlayerBody) continue;
                if (Plugin.BrackenCarryStyle.Value == null
                    || !string.Equals(Plugin.BrackenCarryStyle.Value.Trim(), "Shoulder", StringComparison.OrdinalIgnoreCase)) continue;
                ArmBones ab = ArmTable.GetOrCreateValue(b);
                if (!ab.Looked)
                {
                    ab.Looked = true;
                    ab.Right = Plugin.BrackenCarrySide.Value >= 0f;
                    string sfx = ab.Right ? ".R" : ".L";
                    ab.Upper = FindChild(b.transform, "Arm1" + sfx);
                    ab.Lower = FindChild(b.transform, "Arm2" + sfx);
                    ab.Hand = FindChild(b.transform, "Hand1" + sfx);
                    if (ab.Upper == null || ab.Lower == null || ab.Hand == null)
                        Plugin.Log.LogWarning("Bracken arm bones not found; the arm pose is skipped.");
                }
                if (ab.Upper == null || ab.Lower == null || ab.Hand == null) continue;

                Transform root = st.BrackenDragPoint.parent != null ? st.BrackenDragPoint.parent : b.transform;
                float sc = Mathf.Max(0.01f, Mathf.Abs(b.transform.lossyScale.y));
                float sign = ab.Right ? 1f : -1f;
                Vector3 local = st.BrackenDragPoint.localPosition
                    + new Vector3(-sign * Plugin.BrackenArmIn.Value / sc, Plugin.BrackenArmUp.Value / sc, Plugin.BrackenArmForward.Value / sc);
                Vector3 target = root.TransformPoint(local);

                Vector3 S = ab.Upper.position;
                float l1 = Vector3.Distance(S, ab.Lower.position);
                float l2 = Vector3.Distance(ab.Lower.position, ab.Hand.position);
                Vector3 toT = target - S;
                float d = toT.magnitude;
                if (d < 0.01f || l1 < 0.001f || l2 < 0.001f) continue;
                d = Mathf.Min(d, (l1 + l2) * 0.995f);
                Vector3 dir = toT.normalized;
                Vector3 pole = b.transform.right * sign + b.transform.up * 0.6f;
                pole -= dir * Vector3.Dot(pole, dir);
                if (pole.sqrMagnitude < 0.0001f) pole = b.transform.up;
                pole.Normalize();
                float a = (l1 * l1 - l2 * l2 + d * d) / (2f * d);
                float h = Mathf.Sqrt(Mathf.Max(0f, l1 * l1 - a * a));
                Vector3 elbow = S + dir * a + pole * h;

                Vector3 cur = (ab.Lower.position - S).normalized;
                ab.Upper.rotation = Quaternion.FromToRotation(cur, (elbow - S).normalized) * ab.Upper.rotation;
                Vector3 cur2 = (ab.Hand.position - ab.Lower.position).normalized;
                Vector3 want2 = (S + dir * d - ab.Lower.position).normalized;
                ab.Lower.rotation = Quaternion.FromToRotation(cur2, want2) * ab.Lower.rotation;
            }
        }

        // Walk upright while dragging a cocoon, instead of the game's arms-out carrying pose.
        internal static void PoseFix(FlowermanAI b)
        {
            if (b == null || !b.carryingPlayerBody || !Plugin.BrackenDragNormalPose.Value) return;
            if (b.animationContainer == null || b.creatureAnimator == null) return;
            bool dragging = false;
            foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
            {
                if (kv.Value.Thief == b && kv.Value.BrackenDragPoint != null) { dragging = true; break; }
            }
            if (!dragging) return;
            Vector3 e = b.animationContainer.localEulerAngles;
            e.z = 0f;
            b.animationContainer.localEulerAngles = e;
            b.creatureAnimator.SetLayerWeight(1, 0f);
        }

        // The victim was freed (or the body is being removed): make sure no Bracken still holds it.
        internal static void ReleaseCarry(CocoonState s, DeadBodyInfo body)
        {
            if (s != null && s.BugThief != null)
            {
                LootBugThief.Release(s, body);
                return;
            }
            FlowermanAI b = s != null ? s.Thief : null;
            ClearDragPoint(s, body);
            if (s != null) s.Thief = null;
            if (b == null) return;
            try
            {
                if (b.bodyBeingCarried == body || (body == null && b.bodyBeingCarried != null && b.bodyBeingCarried.attachedTo == null))
                {
                    b.carryingPlayerBody = false;
                    b.bodyBeingCarried = null;
                    if (b.creatureAnimator != null) b.creatureAnimator.SetBool("carryingBody", false);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not make the Bracken let go: " + e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(FlowermanAI), nameof(FlowermanAI.Update))]
    internal static class BrackenStealUpdatePatch
    {
        private static void Postfix(FlowermanAI __instance)
        {
            try { BrackenThief.Tick(__instance); }
            catch (Exception e) { Plugin.Log.LogWarning("Bracken steal check failed: " + e.Message); }
            try { BrackenThief.PoseFix(__instance); }
            catch (Exception) { }
        }
    }

    // When a Bracken's anger runs out the game sends it into "evade" (retreat). Send it straight back to
    // stalking (state 0) instead, so it sticks around. Only the owner decides, like the game's own switch.
    [HarmonyPatch(typeof(FlowermanAI), nameof(FlowermanAI.Update))]
    internal static class BrackenStayPatch
    {
        private sealed class Track { public int Last = -1; }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FlowermanAI, Track> Tracks
            = new System.Runtime.CompilerServices.ConditionalWeakTable<FlowermanAI, Track>();

        private static void Postfix(FlowermanAI __instance)
        {
            try
            {
                FlowermanAI b = __instance;
                if (b == null) return;
                Track t = Tracks.GetOrCreateValue(b);
                int now = b.currentBehaviourStateIndex;
                if (Plugin.BrackenStaysAfterChase.Value && b.IsOwner && !b.isEnemyDead
                    && t.Last == 2 && now == 1 && !b.carryingPlayerBody && !b.inKillAnimation)
                {
                    b.SwitchToBehaviourState(0);
                    b.evadeStealthTimer = 0f;
                    now = b.currentBehaviourStateIndex;
                    Plugin.Log.LogInfo("Bracken lost its target but stays around, stalking again.");
                }
                t.Last = now;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Bracken stay check failed: " + e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(FlowermanAI), nameof(FlowermanAI.DoAIInterval))]
    internal static class BrackenStealAiPatch
    {
        private static void Postfix(FlowermanAI __instance)
        {
            try { BrackenThief.Steer(__instance); }
            catch (Exception e) { Plugin.Log.LogWarning("Bracken steering failed: " + e.Message); }
        }
    }

    // ---- Loot bugs: the same idea as the Bracken, but they carry the cocoon to their nest. ----
    internal static class LootBugThief
    {
        private sealed class Lure
        {
            public int VictimId = -1;
            public float Until;
            public float NextCheck;
            public HashSet<CocoonState> Rolled = new HashSet<CocoonState>();
            public bool Logged;
            public bool Yielding;
            public float NextYield;
            public Vector3 YieldGoal;
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<HoarderBugAI, Lure> Lures
            = new System.Runtime.CompilerServices.ConditionalWeakTable<HoarderBugAI, Lure>();

        private static float nextLog;

        private static bool Carrying(HoarderBugAI h, out CocoonState state)
        {
            state = null;
            foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
                if (kv.Value.BugThief == h) { state = kv.Value; return true; }
            return false;
        }

        internal static void Tick(HoarderBugAI h)
        {
            if (!Plugin.LootBugSteals.Value || h == null || !h.IsOwner) return;
            Dictionary<int, CocoonState> active = Cocoon.ActiveStates;
            if (active.Count == 0) { Lure none; if (Lures.TryGetValue(h, out none)) none.VictimId = -1; return; }

            Lure l = Lures.GetOrCreateValue(h);
            if (!l.Logged)
            {
                l.Logged = true;
                Plugin.Dbg("Loot bug steal logic is watching this loot bug (state " + h.currentBehaviourStateIndex + ").");
            }
            CocoonState carried;
            if (h.isEnemyDead || h.currentBehaviourStateIndex == 2 || h.heldItem != null || Carrying(h, out carried))
            {
                CocoonState dropped;
                if (l.VictimId >= 0 && active.TryGetValue(l.VictimId, out dropped)) BrackenThief.DropClaim(dropped, h);
                l.VictimId = -1;
                return;
            }

            if (l.VictimId >= 0)
            {
                CocoonState s;
                DeadBodyInfo body;
                if (!active.TryGetValue(l.VictimId, out s) || !BrackenThief.Interesting(s, out body) || s.WasStolen || Time.time > l.Until
                    || BrackenThief.PlayerNear(body.bodyParts[6].transform.position, null) || s.Claim != h)
                {
                    Plugin.Log.LogInfo("Loot bug gives up on the victim.");
                    BrackenThief.DropClaim(s, h);
                    l.VictimId = -1;
                    return;
                }
                Vector3 d = h.transform.position - body.bodyParts[6].transform.position;
                d.y = 0f;
                DeadBodyInfo hung;
                if (BrackenThief.Available(s, out hung) && Time.time - s.HungAt >= Plugin.BrackenStealDelaySeconds.Value
                    && d.magnitude <= Plugin.BrackenReachMeters.Value
                    && Mathf.Abs(h.transform.position.y - BrackenThief.FloorUnder(body).y) < 2.5f)
                {
                    int id = l.VictimId;
                    l.VictimId = -1;
                    Plugin.Log.LogInfo("Loot bug takes player " + id + "'s cocoon.");
                    CocoonNet.RequestSteal(id, h.NetworkObjectId);
                }
                return;
            }

            if (Time.time < l.NextCheck) return;
            l.NextCheck = Time.time + 0.5f;

            int bestId = -1;
            float best = Plugin.BrackenNoticeMeters.Value;
            foreach (KeyValuePair<int, CocoonState> kv in active)
            {
                DeadBodyInfo body;
                if (!BrackenThief.Interesting(kv.Value, out body) || kv.Value.WasStolen || l.Rolled.Contains(kv.Value) || !BrackenThief.Claimable(kv.Value, h, 1)) continue;
                float dist = Vector3.Distance(h.transform.position, body.bodyParts[6].transform.position);
                if (dist < best) { best = dist; bestId = kv.Key; }
            }
            if (bestId < 0) return;

            l.Rolled.Add(active[bestId]);
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll >= Plugin.BrackenStealChancePercent.Value)
            {
                Plugin.Dbg("Loot bug noticed a wrapped victim (" + best.ToString("F1") + " m) but passed: rolled " + roll.ToString("F0") + ".");
                return;
            }
            DeadBodyInfo chosen;
            if (!BrackenThief.Interesting(active[bestId], out chosen)) return;
            if (BrackenThief.PlayerNear(chosen.bodyParts[6].transform.position, null)) return;
            if (!BrackenThief.Claimable(active[bestId], h, 1)) return;
            BrackenThief.SetClaim(active[bestId], h, 1);
            l.VictimId = bestId;
            l.Until = Time.time + Mathf.Max(5f, Plugin.BrackenGiveUpSeconds.Value);
            Plugin.Log.LogInfo("Loot bug has noticed a wrapped victim (" + best.ToString("F1") + " m away) and is following them.");
        }

        // After the bug's own AI step: head for the victim, or carry the victim to the nest.
        internal static void Steer(HoarderBugAI h)
        {
            if (h == null || !h.IsOwner || h.isEnemyDead) return;

            CocoonState carried;
            if (Carrying(h, out carried))
            {
                DeadBodyInfo body = carried.Player != null ? carried.Player.deadBody : null;
                if (h.currentBehaviourStateIndex == 2 || body == null || body.attachedTo != carried.BugCarryPoint)
                {
                    Plugin.Log.LogInfo("Loot bug lets go of the cocoon.");
                    Release(carried, body);
                    return;
                }
                if (h.nestPosition == Vector3.zero)
                {
                    Transform far = h.ChooseFarthestNodeFromPosition(h.transform.position);
                    if (far != null) h.nestPosition = far.position;
                }
                Vector3 flat = h.transform.position - h.nestPosition;
                flat.y = 0f;
                if (flat.magnitude < 2f || Time.time - carried.BugCarryStart > 45f)
                {
                    Plugin.Log.LogInfo("Loot bug drops the cocoon at its nest.");
                    Release(carried, body);
                    return;
                }
                h.movingTowardsTargetPlayer = false;
                if (h.agent != null && h.agent.enabled && h.agent.isOnNavMesh)
                {
                    h.agent.isStopped = false;
                    if (h.agent.speed < 5f) h.agent.speed = 5f;
                    h.agent.SetDestination(h.nestPosition);
                    h.destination = h.nestPosition;
                }
                if (Time.time >= nextLog)
                {
                    nextLog = Time.time + 2f;
                    Plugin.Dbg("Loot bug carry: dist to nest " + flat.magnitude.ToString("0.0") + " speed=" + h.agent.speed.ToString("0.0") + " state=" + h.currentBehaviourStateIndex);
                }
                return;
            }

            // Back off while a Bracken is going for a cocoon nearby, so the swarm does not block its path.
            if (Plugin.LootBugYield.Value && h.currentBehaviourStateIndex != 2 && h.heldItem == null)
            {
                Lure ly = Lures.GetOrCreateValue(h);
                Vector3 threat;
                if (BrackenThief.PursuerNear(h.transform.position, Plugin.LootBugYieldMeters.Value, out threat))
                {
                    if (!ly.Yielding || Time.time >= ly.NextYield)
                    {
                        ly.NextYield = Time.time + 1.5f;
                        Vector3 away = h.transform.position - threat;
                        away.y = 0f;
                        if (away.sqrMagnitude < 0.01f) away = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
                        away = away.normalized * (Plugin.LootBugYieldMeters.Value + 5f);
                        Vector3 goal = h.transform.position + away;
                        UnityEngine.AI.NavMeshHit yhit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(goal, out yhit, 8f, UnityEngine.AI.NavMesh.AllAreas)) goal = yhit.position;
                        ly.YieldGoal = goal;
                        if (!ly.Yielding)
                        {
                            ly.Yielding = true;
                            Plugin.Dbg("Loot bug backs off: a Bracken is going for a cocoon nearby.");
                        }
                    }
                    CocoonState claimed;
                    if (ly.VictimId >= 0 && Cocoon.ActiveStates.TryGetValue(ly.VictimId, out claimed)) BrackenThief.DropClaim(claimed, h);
                    ly.VictimId = -1;
                    h.movingTowardsTargetPlayer = false;
                    if (h.agent != null && h.agent.enabled && h.agent.isOnNavMesh)
                    {
                        h.agent.isStopped = false;
                        h.agent.SetDestination(ly.YieldGoal);
                        h.destination = ly.YieldGoal;
                    }
                    return;
                }
                ly.Yielding = false;
            }

            Lure l;
            if (!Lures.TryGetValue(h, out l) || l.VictimId < 0) return;
            CocoonState s;
            DeadBodyInfo vb;
            if (!Cocoon.ActiveStates.TryGetValue(l.VictimId, out s) || !BrackenThief.Interesting(s, out vb)) return;
            Vector3 target = BrackenThief.FloorUnder(vb);
            // The spider may still be dragging the body (nothing walkable there yet): keep trying until it
            // is hung, and let the give-up timer decide, not a single failed path check.
            if (!h.SetDestinationToPosition(target, true)) return;
            if (h.agent != null && h.agent.enabled && h.agent.isOnNavMesh) h.agent.SetDestination(target);
        }

        // Runs on every game.
        internal static void Apply(int victimId, CocoonState s, DeadBodyInfo body, HoarderBugAI h)
        {
            if (h == null || h.isEnemyDead || h.grabTarget == null || h.heldItem != null || !Plugin.LootBugSteals.Value) return;
            Transform hangPoint = body.attachedTo;
            for (int bp = 0; bp < body.bodyParts.Length; bp++)
            {
                if (body.bodyParts[bp] == null) continue;
                body.bodyParts[bp].isKinematic = false;
                body.bodyParts[bp].freezeRotation = false;
            }
            body.wasMatchingPosition = false;
            // Hold the cocoon up above the bug (an empty point riding on its back), not down at its mouth.
            GameObject point = new GameObject("CocoonCarryPoint");
            point.transform.SetParent(h.transform, false);
            // The bug's model may be scaled down: convert the height from real metres into its local units.
            float yScale = Mathf.Max(0.01f, Mathf.Abs(h.transform.lossyScale.y));
            point.transform.localPosition = new Vector3(0f, Plugin.LootBugCarryHeight.Value / yScale, 0f);
            Plugin.Dbg("Loot bug carry point: scale " + yScale.ToString("0.00") + ", " + Plugin.LootBugCarryHeight.Value.ToString("0.0") + " m above the bug.");
            s.BugCarryPoint = point.transform;
            body.attachedTo = point.transform;
            body.attachedLimb = body.bodyParts[0];
            body.matchPositionExactly = true;
            s.BugThief = h;
            s.WasStolen = true;
            s.BugCarryStart = Time.time;
            s.ExtraSeconds += Plugin.BrackenStealExtraSeconds.Value;
            if (hangPoint != null) Cocoon.DestroyRope(hangPoint);
            if (h.IsOwner)
            {
                if (h.nestPosition == Vector3.zero)
                {
                    Transform far = h.ChooseFarthestNodeFromPosition(h.transform.position);
                    if (far != null) h.nestPosition = far.position;
                }
                h.SwitchToBehaviourState(1);
            }
            Plugin.Log.LogInfo("A loot bug carries player " + victimId + "'s cocoon away.");
        }

        internal static void Release(CocoonState s, DeadBodyInfo body)
        {
            HoarderBugAI h = s != null ? s.BugThief : null;
            Transform point = s != null ? s.BugCarryPoint : null;
            if (s != null) { s.BugThief = null; s.BugCarryPoint = null; }
            if (h == null || body == null)
            {
                if (point != null)
                {
                    try
                    {
                        // The victim's body is no longer known (they died): free whatever is still glued to the point.
                        foreach (DeadBodyInfo d in UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>())
                        {
                            if (d != null && d.attachedTo == point) { d.matchPositionExactly = false; d.attachedTo = null; }
                        }
                    }
                    catch (Exception) { }
                    UnityEngine.Object.Destroy(point.gameObject);
                }
                return;
            }
            try
            {
                if (body.attachedTo == point)
                {
                    body.matchPositionExactly = false;
                    body.attachedTo = null;
                }
                if (point != null) UnityEngine.Object.Destroy(point.gameObject);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not make the loot bug let go: " + e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(HoarderBugAI), nameof(HoarderBugAI.Update))]
    internal static class LootBugUpdatePatch
    {
        private static void Postfix(HoarderBugAI __instance)
        {
            try { LootBugThief.Tick(__instance); }
            catch (Exception e) { Plugin.Log.LogWarning("Loot bug steal check failed: " + e.Message); }
        }
    }

    [HarmonyPatch(typeof(HoarderBugAI), nameof(HoarderBugAI.DoAIInterval))]
    internal static class LootBugAiPatch
    {
        private static void Postfix(HoarderBugAI __instance)
        {
            try { LootBugThief.Steer(__instance); }
            catch (Exception e) { Plugin.Log.LogWarning("Loot bug steering failed: " + e.Message); }
        }
    }

    // The bug only raises its arms for a real item; do it for a carried cocoon too, and rear it up on
    // its back legs so it looks like it is holding the cocoon over its head.
    [HarmonyPatch(typeof(HoarderBugAI), "LateUpdate")]
    internal static class LootBugArmsPatch
    {
        private static readonly HashSet<HoarderBugAI> Tilted = new HashSet<HoarderBugAI>();

        private static void Postfix(HoarderBugAI __instance)
        {
            try
            {
                HoarderBugAI h = __instance;
                if (h == null || h.creatureAnimator == null || h.animationContainer == null) return;
                bool carrying = false;
                foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
                    if (kv.Value.BugThief == h) { carrying = true; break; }

                if (carrying)
                {
                    h.creatureAnimator.SetLayerWeight(1, 1f);
                    Tilted.Add(h);
                    Quaternion want = Quaternion.Euler(-Plugin.LootBugRearUpDegrees.Value, 0f, 0f);
                    h.animationContainer.localRotation = Quaternion.Slerp(h.animationContainer.localRotation, want, 6f * Time.deltaTime);
                }
                else if (Tilted.Contains(h))
                {
                    h.animationContainer.localRotation = Quaternion.Slerp(h.animationContainer.localRotation, Quaternion.identity, 6f * Time.deltaTime);
                    if (Quaternion.Angle(h.animationContainer.localRotation, Quaternion.identity) < 1f)
                    {
                        h.animationContainer.localRotation = Quaternion.identity;
                        Tilted.Remove(h);
                    }
                }
            }
            catch (Exception) { }
        }
    }

    // The game shows a player's floating name badge whenever you look at their body. A wrapped-up
    // player's real (invisible) body is still standing where they were caught, so stop the badge
    // being shown at the source instead of trying to hide it afterwards.
    // No fade-to-black when a Bracken's neck snap kills the local player.
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
    internal static class NoBlackoutPatch
    {
        static void Postfix(PlayerControllerB __instance, CauseOfDeath causeOfDeath)
        {
            try
            {
                if (!Plugin.NoBlackoutOnBrackenKill.Value || causeOfDeath != CauseOfDeath.Strangulation) return;
                if (GameNetworkManager.Instance == null || __instance != GameNetworkManager.Instance.localPlayerController) return;
                if (HUDManager.Instance != null && HUDManager.Instance.gameOverAnimator != null)
                    HUDManager.Instance.gameOverAnimator.ResetTrigger("gameOver");
            }
            catch (Exception e) { Plugin.Log.LogWarning("No-blackout patch: " + e.Message); }
        }
    }

    // A wrapped player's real body is still standing where they were caught. A Nutcracker's shotgun blast hits whoever is in its cone
    // (the game checks the position only), and its kick kills whoever it reaches. Neither may touch a wrapped player.
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
    internal static class WrappedNoShotPatch
    {
        private static bool Prefix(PlayerControllerB __instance, CauseOfDeath causeOfDeath)
        {
            try
            {
                if (causeOfDeath == CauseOfDeath.Gunshots && Cocoon.IsCocooned(__instance))
                {
                    Plugin.Verbose("A shotgun blast hit a wrapped player's hidden body: ignored.");
                    return false;
                }
            }
            catch (Exception) { }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
    internal static class WrappedNoKickPatch
    {
        private static bool Prefix(PlayerControllerB __instance, CauseOfDeath causeOfDeath)
        {
            try
            {
                if (causeOfDeath == CauseOfDeath.Kicking && Cocoon.IsCocooned(__instance))
                {
                    Plugin.Verbose("A Nutcracker kick hit a wrapped player's hidden body: ignored.");
                    return false;
                }
            }
            catch (Exception) { }
            return true;
        }
    }

    // A wrapped player's hidden body must not sink (and die) if it happens to stand in quicksand.
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.CheckConditionsForSinkingInQuicksand))]
    internal static class WrappedNoQuicksandPatch
    {
        private static void Postfix(PlayerControllerB __instance, ref bool __result)
        {
            try
            {
                if (__result && Cocoon.IsCocooned(__instance))
                {
                    __result = false;
                    Plugin.Verbose("Quicksand ignored for a wrapped player's hidden body.");
                }
            }
            catch (Exception) { }
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ShowNameBillboard))]
    internal static class NameBadgePatch
    {
        private static bool Prefix(PlayerControllerB __instance)
        {
            return !Cocoon.IsCocooned(__instance);
        }
    }

    internal static class WebIgnorePatch
    {
        // Make a player let go of every spider web they are stuck in (they were caught in one just before
        // being wrapped). Webs only ever hold the local player, so this runs on the victim's own game.
        internal static void LeaveAllWebs(PlayerControllerB victim)
        {
            try
            {
                System.Reflection.MethodInfo leave = AccessTools.Method(typeof(SandSpiderWebTrap), "PlayerLeaveWeb");
                System.Reflection.FieldInfo hindering = AccessTools.Field(typeof(SandSpiderWebTrap), "hinderingLocalPlayer");
                foreach (SandSpiderWebTrap web in UnityEngine.Object.FindObjectsOfType<SandSpiderWebTrap>())
                {
                    if (web == null) continue;
                    if (leave != null && hindering != null && (bool)hindering.GetValue(web)) leave.Invoke(web, new object[] { victim });
                    else if (web.currentTrappedPlayer == victim) web.currentTrappedPlayer = null;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not free the victim from webs: " + e.Message);
            }
        }

        // Lets go of any web that thinks it is holding the player although they are no longer in it, and then,
        // if nothing at all is holding them, puts their walking speed back to normal.
        internal static void FixStuckHindrance(PlayerControllerB p)
        {
            try
            {
                System.Reflection.MethodInfo leave = AccessTools.Method(typeof(SandSpiderWebTrap), "PlayerLeaveWeb");
                System.Reflection.FieldInfo hindering = AccessTools.Field(typeof(SandSpiderWebTrap), "hinderingLocalPlayer");
                if (leave == null || hindering == null) return;
                bool stillHeld = false;
                foreach (SandSpiderWebTrap web in UnityEngine.Object.FindObjectsOfType<SandSpiderWebTrap>())
                {
                    if (web == null || !(bool)hindering.GetValue(web)) continue;
                    Collider area = web.GetComponent<Collider>();
                    bool inside = area != null && p.thisController != null && area.bounds.Intersects(p.thisController.bounds);
                    if (inside) { stillHeld = true; continue; }
                    leave.Invoke(web, new object[] { p });
                    Plugin.Log.LogInfo("Freed the player from a web that was still slowing them after they had left it.");
                }
                if (!stillHeld && (p.isMovementHindered != 0 || Mathf.Abs(p.hinderedMultiplier - 1f) > 0.01f))
                {
                    Plugin.Log.LogInfo("Walking speed was stuck slow (hindered=" + p.isMovementHindered + ", x" + p.hinderedMultiplier.ToString("0.00") + "); put back to normal.");
                    p.isMovementHindered = 0;
                    p.hinderedMultiplier = 1f;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not check for a stuck slow-down: " + e.Message);
            }
        }

        // Returns false (skip the game's code) when the thing touching the web is a wrapped-up player's
        // hidden body, so it cannot trip, slow down or alert the spider.
        private static bool Prefix(Collider other)
        {
            try
            {
                if (other == null) return true;
                PlayerControllerB p = other.GetComponentInParent<PlayerControllerB>();
                if (p != null && Cocoon.IsCocooned(p)) return false;
            }
            catch (Exception) { }
            return true;
        }
    }

    // The spider is also told about a tripped web over the network; ignore that for wrapped players too.
    [HarmonyPatch(typeof(SandSpiderAI), "PlayerTripWebClientRpc")]
    internal static class WebTripRpcPatch
    {
        private static bool Prefix(int playerNum)
        {
            try
            {
                StartOfRound sor = StartOfRound.Instance;
                if (sor != null && playerNum >= 0 && playerNum < sor.allPlayerScripts.Length && Cocoon.IsCocooned(sor.allPlayerScripts[playerNum])) return false;
            }
            catch (Exception) { }
            return true;
        }
    }

    // Spiders must not treat a wrapped-up player as a normal target. Otherwise, once the body is
    // hung, the spider can decide to chase the frozen player's real (invisible) body and stand
    // next to it, unable to hit it. (Patched by hand in Awake; the arguments are read by position
    // so the patch does not depend on the game's parameter names.)
    internal static class SpiderTargetPatch
    {
        internal static void Postfix(EnemyAI __instance, object[] __args, ref bool __result)
        {
            try
            {
                if (__result && __args != null && __args.Length > 0)
                {
                    PlayerControllerB player = __args[0] as PlayerControllerB;
                    // A wrapped-up player's real (invisible) body is still standing where they were caught.
                    // No monster may target it or collide with it (a Bracken would otherwise strangle it).
                    if (Cocoon.IsCocooned(player)) __result = false;
                    // Freed players are invisible to every monster's AI during their immunity (spider, Bracken,
                    // loot bug...). Otherwise a monster waiting next to the cocoon gets them the moment they drop.
                    else if (Cocoon.IsImmune(player)) __result = false;
                }
            }
            catch (Exception)
            {
                // Never let this filter break the game's own targeting.
            }
        }
    }

    // DIAGNOSTIC: says why a touch from the spider did (not) turn into a bite, at most once a second per spider.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.OnCollideWithPlayer))]
    internal static class SpiderTouchDiag
    {
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, object[]> Last
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, object[]>();

        [HarmonyPriority(Priority.First)]
        private static void Prefix(SandSpiderAI __instance, Collider other, float ___timeSinceHittingPlayer, bool ___onWall, bool ___spoolingPlayerBody, DeadBodyInfo ___currentlyHeldBody)
        {
            try
            {
                if (!Plugin.DebugLogging.Value || !Plugin.VerboseLogs.Value || __instance == null) return;
                object[] box = Last.GetValue(__instance, k => new object[] { 0f });
                if (Time.time < (float)box[0]) return;
                box[0] = Time.time + 1f;
                PlayerControllerB p = __instance.MeetsStandardPlayerCollisionConditions(other, ___spoolingPlayerBody);
                List<string> why = new List<string>();
                if (__instance.isEnemyDead) why.Add("spider dead");
                if (___onWall) why.Add("spider is on a wall");
                if (___currentlyHeldBody != null) why.Add("spider is holding a body");
                if (SpiderBiteMemory.OnCooldown(__instance)) why.Add("bite cooldown");
                if (Trapdoor.Applies(__instance) && Cocoon.SpiderHasVictim(__instance)) why.Add("trapdoor spider already has a victim");
                if (p == null) why.Add("game's own collision check says this is not a valid player (stunned/just hit/special animation/untargetable)");
                else
                {
                    if (Cocoon.IsCocooned(p)) why.Add("player is cocooned");
                    if (Cocoon.IsVenomed(p)) why.Add("player is venomed");
                    if (Cocoon.IsImmune(p)) why.Add("player is immune");
                }
                if (___timeSinceHittingPlayer <= 1f) why.Add("hit " + ___timeSinceHittingPlayer.ToString("0.0") + " s ago (needs > 1)");
                Plugin.Verbose("Spider touch [" + __instance.GetInstanceID() + "]: " + (why.Count == 0 ? "bite should go through." : "NO bite because: " + string.Join("; ", why.ToArray())));
            }
            catch (Exception) { }
        }
    }

    // STAGE 1: replaces the spider's attack so it no longer damages anyone.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.OnCollideWithPlayer))]
    internal static class SpiderAttackPatch
    {
        private static bool Prefix(
            SandSpiderAI __instance,
            Collider other,
            ref float ___timeSinceHittingPlayer,
            bool ___onWall,
            bool ___spoolingPlayerBody,
            DeadBodyInfo ___currentlyHeldBody)
        {
            if (__instance.isEnemyDead || ___onWall) return false;

            // Busy carrying or wrapping someone: a second catch would leave the first victim orphaned.
            if (___currentlyHeldBody != null) return false;

            // Just bit someone: this spider leaves everybody else alone for a while.
            if (SpiderBiteMemory.OnCooldown(__instance)) return false;

            // A trapdoor spider deals with one victim at a time: it is busy until they are freed or released.
            if (Trapdoor.Applies(__instance) && Cocoon.SpiderHasVictim(__instance)) return false;

            PlayerControllerB player = __instance.MeetsStandardPlayerCollisionConditions(other, ___spoolingPlayerBody);

            // Ignore players who are already wrapped up (or just released).
            if (player != null && !Cocoon.IsCocooned(player) && !Cocoon.IsVenomed(player) && !Cocoon.IsImmune(player) && ___timeSinceHittingPlayer > 1f)
            {
                ___timeSinceHittingPlayer = 0f;
                Plugin.Log.LogInfo("Spider hit intercepted: no damage, cocooning player.");

                // Keeps the attack animation + sound, and tells every player's game about the hit.
                __instance.HitPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
            }
            return false;
        }
    }

    // Remembers when each spider last bit someone (every player's game records it from the same network message).
    internal static class SpiderBiteMemory
    {
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, System.Runtime.CompilerServices.StrongBox<float>> Last
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, System.Runtime.CompilerServices.StrongBox<float>>();

        internal static void Record(SandSpiderAI spider)
        {
            if (spider == null) return;
            System.Runtime.CompilerServices.StrongBox<float> box;
            if (Last.TryGetValue(spider, out box)) box.Value = Time.time;
            else Last.Add(spider, new System.Runtime.CompilerServices.StrongBox<float>(Time.time));
        }

        internal static bool OnCooldown(SandSpiderAI spider)
        {
            float cd = Plugin.BiteCooldownSeconds.Value;
            if (cd <= 0f || spider == null) return false;
            System.Runtime.CompilerServices.StrongBox<float> box;
            return Last.TryGetValue(spider, out box) && Time.time - box.Value < cd;
        }
    }

    // STAGE 2: the game already tells every player's game "the spider hit player X"
    // through this method. We piggyback on it, so no custom networking is needed.
    // Hitting a monster ends a freed player's immunity. Two entry points cover every game: the hitter's own
    // game (local hit) and everybody else's (the network message).
    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemyOnLocalClient))]
    internal static class ImmunityLocalHitPatch
    {
        private static void Postfix(PlayerControllerB playerWhoHit)
        {
            Cocoon.EndImmunityOnAttack(playerWhoHit);
        }
    }

    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemyClientRpc))]
    internal static class ImmunityRemoteHitPatch
    {
        private static void Postfix(int playerWhoHit)
        {
            try
            {
                PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
                if (playerWhoHit >= 0 && playerWhoHit < players.Length) Cocoon.EndImmunityOnAttack(players[playerWhoHit]);
            }
            catch (Exception) { }
        }
    }

    // while it is bitten an armed player, the spider's own thinking is replaced by "keep away and hide".
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.DoAIInterval))]
    internal static class SpiderAvoidWeaponPatch
    {
        private static readonly System.Reflection.FieldInfo HeldBody = AccessTools.Field(typeof(SandSpiderAI), "currentlyHeldBody");
        private static float nextPlan;
        private static float nextDbg;
        private static Vector3 lastDest;
        private static bool logged;
        private static Vector3 hidePos;
        private static bool hasHide;
        private static Vector3 fallbackDest;
        private static float episodeStart, stuckSince;
        internal static readonly System.Reflection.MethodInfo CalcMeshPath = AccessTools.Method(typeof(SandSpiderAI), "CalculateSpiderPathToPosition");
        private static bool hasBad;
        private static Vector3 badSpot;

        // What the per-frame patch below must keep the spider doing between two of the game's AI intervals.
        // (The game's own Update runs every frame and keeps writing "walk to the player" and the normal speed.)
        internal static SandSpiderAI AvoidSpider;
        internal static float AvoidUntil;
        internal static Vector3 AvoidDest;
        internal static float AvoidSpeed;

        // True if nothing in the level lets this player's eyes see a spider standing at pos.
        private static bool LineBlocked(Vector3 from, Vector3 to, PlayerControllerB pl, int mask)
        {
            Vector3 dir = to - from; float dist = dir.magnitude;
            if (dist < 0.05f) return false;
            RaycastHit[] hits = Physics.RaycastAll(from, dir / dist, dist, mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Transform t = hits[i].collider.transform;
                if (pl != null && t.IsChildOf(pl.transform)) continue;   // the player's own body does not hide anything
                if (t.GetComponentInParent<EnemyAI>() != null) continue; // nor does any monster
                return true;
            }
            return false;
        }

        // True only if the player's eyes cannot see the spider's feet, middle OR top at pos.
        private static bool HiddenFrom(PlayerControllerB pl, Vector3 pos)
        {
            StartOfRound sor = StartOfRound.Instance;
            if (sor == null || pl == null || pl.gameplayCamera == null) return false;
            Vector3 eye = pl.gameplayCamera.transform.position;
            return LineBlocked(eye, pos + Vector3.up * 0.4f, pl, sor.collidersAndRoomMask)
                && LineBlocked(eye, pos + Vector3.up * 1.0f, pl, sor.collidersAndRoomMask)
                && LineBlocked(eye, pos + Vector3.up * 1.8f, pl, sor.collidersAndRoomMask);
        }

        // Like the Bracken's ambush: of the AI nodes between minD and maxD from the player, keep the ones the player
        // cannot see, and take the nearest to the spider that it can actually walk to.
        private static bool PickHidingSpot(SandSpiderAI spider, PlayerControllerB v, Vector3 here, float minD, float maxD, out Vector3 spot)
        {
            spot = here;
            if (spider.allAINodes == null || spider.allAINodes.Length == 0) return false;
            Vector3 pp = v.transform.position;
            for (int pass = 0; pass < 2; pass++)
            {
                float lo = pass == 0 ? minD : minD * 0.6f, hi = pass == 0 ? maxD : maxD * 1.6f;
                int inRange = 0;
                List<KeyValuePair<float, Vector3>> found = new List<KeyValuePair<float, Vector3>>();
                foreach (GameObject node in spider.allAINodes)
                {
                    if (node == null) continue;
                    Vector3 p = node.transform.position;
                    float dp = Vector3.Distance(p, pp);
                    if (dp < lo || dp > hi) continue;
                    inRange++;
                    if (hasBad && Vector3.Distance(p, badSpot) < 3f) continue;
                    if (!HiddenFrom(v, p)) continue;
                    found.Add(new KeyValuePair<float, Vector3>((p - here).sqrMagnitude, p));
                }
                found.Sort((x, y) => x.Key.CompareTo(y.Key));
                for (int i = 0; i < found.Count && i < 6; i++)
                {
                    if (spider.SetDestinationToPosition(found[i].Value, true))
                    {
                        spot = found[i].Value;
                        Plugin.Verbose("Spider hiding spot: " + Vector3.Distance(spot, pp).ToString("0.0") + " m from the player, " + Vector3.Distance(spot, here).ToString("0.0") + " m away (" + found.Count + " hidden of " + inRange + " nodes between " + lo.ToString("0") + " and " + hi.ToString("0") + " m).");
                        return true;
                    }
                }
                Plugin.Dbg("Spider found no reachable hiding spot (" + found.Count + " hidden of " + inRange + " nodes between " + lo.ToString("0") + " and " + hi.ToString("0") + " m).");
            }
            return false;
        }

        private static bool Prefix(SandSpiderAI __instance)
        {
            try
            {
                if (!Plugin.VenomAvoidsWeapons.Value || __instance.isEnemyDead || !__instance.IsOwner) return true;
                string why;
                PlayerControllerB v = Cocoon.ArmedVictimOf(__instance, out why);
                if (v == null)
                {
                    if (Plugin.DebugLogging.Value && Plugin.VerboseLogs.Value && why != "no bitten player recorded for this spider" && Time.time >= nextDbg) { nextDbg = Time.time + 1f; Plugin.Dbg("Spider not avoiding: " + why); }
                    logged = false; hasHide = false;
                    return true;
                }
                // Really busy carrying a body: leave it to the game. (State 1 alone is not enough: it also means "heading home".)
                if (__instance.currentBehaviourStateIndex == 1 && HeldBody != null && HeldBody.GetValue(__instance) != null)
                {
                    if (Plugin.DebugLogging.Value && Plugin.VerboseLogs.Value && Time.time >= nextDbg) { nextDbg = Time.time + 1f; Plugin.Dbg("Spider not avoiding: it is carrying a body."); }
                    return true;
                }

                if (!logged)
                {
                    logged = true; Plugin.Verbose("The spider backs off from an armed, bitten player.");
                    // A fresh bite: forget everything from the last one (a stale destination meant the spider was never told to move).
                    lastDest = new Vector3(9999f, 9999f, 9999f); hasHide = false; hasBad = false; nextPlan = 0f;
                    episodeStart = Time.time; stuckSince = 0f;
                }
                __instance.movingTowardsTargetPlayer = false;
                float d = Vector3.Distance(__instance.transform.position, v.transform.position);
                float minD = Mathf.Max(8f, Plugin.VenomAvoidMeters.Value);   // never hide closer than this
                float maxD = Mathf.Max(minD + 6f, Plugin.VenomAvoidMeters.Value * 3f);
                Vector3 here = __instance.transform.position;
                bool seen = !HiddenFrom(v, here);

                // Not getting anywhere (blocked, or no path)? Give up on that hiding spot and pick another.
                if (hasHide && Time.time - episodeStart > 1.2f && __instance.agent.velocity.magnitude < 0.3f && Vector3.Distance(here, hidePos) > 1.5f)
                {
                    if (stuckSince <= 0f) stuckSince = Time.time;
                    else if (Time.time - stuckSince > 1.5f)
                    {
                        Plugin.Dbg("Spider is stuck on the way to its hiding spot: choosing another.");
                        badSpot = hidePos; hasBad = true; hasHide = false; nextPlan = 0f; stuckSince = 0f; lastDest = new Vector3(9999f, 9999f, 9999f);
                    }
                }
                else stuckSince = 0f;

                if (!seen && d >= minD)
                {
                    // Out of sight and far enough: wait for the venom to take them.
                    hasHide = false;
                    __instance.agent.speed = 0f;
                    __instance.spiderSpeed = 0f;
                    __instance.SetDestinationToPosition(here);
                    AvoidDest = here; AvoidSpeed = 0f;
                }
                else
                {
                    bool arrived = hasHide && Vector3.Distance(here, hidePos) < 1.5f;
                    bool spotStillGood = hasHide && HiddenFrom(v, hidePos) && Vector3.Distance(hidePos, v.transform.position) >= minD;
                    if ((!hasHide || !spotStillGood || arrived) && Time.time >= nextPlan)
                    {
                        nextPlan = Time.time + 1f;
                        hasHide = PickHidingSpot(__instance, v, here, minD, maxD, out hidePos);
                        if (!hasHide)
                        {
                            // Nowhere to hide: at least run, away from them.
                            Transform n = __instance.ChooseFarthestNodeFromPosition(v.transform.position, false, 0, false, 50, 35);
                            if (n != null) { __instance.SetDestinationToPosition(n.position); fallbackDest = n.position; }
                            else fallbackDest = here;
                        }
                    }
                    AvoidDest = hasHide ? hidePos : fallbackDest;
                    AvoidSpeed = 5.5f;
                    __instance.destination = AvoidDest;
                    __instance.agent.speed = 5.5f;
                    __instance.spiderSpeed = 5.5f;
                }
                AvoidSpider = __instance;
                AvoidUntil = Time.time + 0.6f;

                // We skipped the game's whole interval, and its first job is the part that actually sends the
                // spider to its destination (and tells the other players where it is). Do those two things ourselves.
                if (!__instance.inSpecialAnimation && __instance.agent.enabled && __instance.agent.isOnNavMesh
                    && (__instance.destination != lastDest || (!__instance.agent.hasPath && !__instance.agent.pathPending)) && __instance.agent.SetDestination(__instance.destination))
                    lastDest = __instance.destination;
                __instance.SyncPositionToClients();
                // The spider's VISIBLE body is steered by this call, which the game makes inside the interval we skipped.
                // Without it the invisible nav agent runs off while the model stands still.
                if (__instance.navigateMeshTowardsPosition && CalcMeshPath != null) CalcMeshPath.Invoke(__instance, null);
                if (Plugin.DebugLogging.Value && Time.time >= nextDbg)
                {
                    nextDbg = Time.time + 1f;
                    Plugin.Verbose("Spider avoids weapon: dist=" + d.ToString("0.0") + " seen=" + seen + " dest=" + __instance.destination.ToString("F1")
                        + " spiderAt=" + __instance.transform.position.ToString("F1") + " speed=" + __instance.agent.speed.ToString("0.0")
                        + " hasPath=" + __instance.agent.hasPath + " vel=" + __instance.agent.velocity.magnitude.ToString("0.0")
                        + " pending=" + __instance.agent.pathPending + " status=" + __instance.agent.pathStatus + " mesh=" + __instance.meshContainer.position.ToString("F1") + " meshTarget=" + __instance.meshContainerTarget.ToString("F1") + " stopped=" + __instance.agent.isStopped + " state=" + __instance.currentBehaviourStateIndex);
                }
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SpiderAvoidWeaponPatch failed: " + e);
                return true;
            }
        }
    }

    // the game's per-frame Update keeps resetting the spider's target (to the player it is chasing)
    // and its speed, between the AI intervals we control. Put our decision back right after it, every frame.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderAvoidWeaponFramePatch
    {
        private static void Postfix(SandSpiderAI __instance)
        {
            try
            {
                if (SpiderAvoidWeaponPatch.AvoidSpider != __instance || Time.time > SpiderAvoidWeaponPatch.AvoidUntil) return;
                if (__instance.isEnemyDead || !__instance.IsOwner) return;
                __instance.movingTowardsTargetPlayer = false;
                if (SpiderAvoidWeaponPatch.AvoidSpeed > 0f && __instance.agent.enabled && __instance.agent.isOnNavMesh) __instance.agent.isStopped = false;
                __instance.destination = SpiderAvoidWeaponPatch.AvoidDest;
                __instance.agent.speed = SpiderAvoidWeaponPatch.AvoidSpeed;
                __instance.spiderSpeed = SpiderAvoidWeaponPatch.AvoidSpeed;
            }
            catch (Exception) { }
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.HitPlayerClientRpc))]
    internal static class SpiderHitSyncPatch
    {
        private static void Postfix(SandSpiderAI __instance, int playerId)
        {
            try
            {
                PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
                if (playerId < 0 || playerId >= players.Length) return;
                SpiderBiteMemory.Record(__instance);
                Cocoon.BeginVenom(players[playerId], __instance);
            }
            catch (Exception e)
            {
                // Never let our code break the game's own network message.
                Plugin.Log.LogError("SpiderHitSyncPatch failed: " + e);
            }
        }
    }

    // a hit on a spider that is carrying a wrapped player makes it drop them and fight back.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.HitEnemy))]
    internal static class SpiderHitDropPatch
    {
        private static void Postfix(SandSpiderAI __instance, PlayerControllerB playerWhoHit)
        {
            Cocoon.SpiderWasHit(__instance, playerWhoHit);
        }
    }


    // ------------------------------------------------------------------
    // the trapdoor spider. An outdoor spider picks a random AI node, walks to it, sinks under the
    // ground (only its model moves; the invisible nav agent stays put) and waits. When a player comes close it bursts
    // out and chases them. After a chase or a catch it picks a new node and does it again.
    // ------------------------------------------------------------------
    internal static class Trapdoor
    {
        internal enum Mode { Walk, Burrowing, Waiting, Emerging, Surfaced, Pulling }

        internal class TD
        {
            public Mode M = Mode.Walk;
            public Vector3 Node;
            public bool HasNode;
            public float Depth;
            public float Since;
            public PlayerControllerB Target;
            public List<Renderer> Hidden = new List<Renderer>();
            public float NextPick;
            public int VictimId = -1;
            public float NextDump;
            public SandSpiderAI Spider;
        }

        private static readonly Dictionary<int, TD> All = new Dictionary<int, TD>();
        private static readonly System.Reflection.MethodInfo CalcMeshPath = AccessTools.Method(typeof(SandSpiderAI), "CalculateSpiderPathToPosition");
        private static readonly System.Reflection.FieldInfo HeldBody = AccessTools.Field(typeof(SandSpiderAI), "currentlyHeldBody");
        private static readonly System.Reflection.FieldInfo ChaseTimerF = AccessTools.Field(typeof(SandSpiderAI), "chaseTimer");
        private static readonly System.Reflection.FieldInfo TargetRotF = AccessTools.Field(typeof(SandSpiderAI), "meshContainerTargetRotation");
        private static readonly System.Reflection.FieldInfo LookingWallF = AccessTools.Field(typeof(SandSpiderAI), "lookingForWallPosition");
        private static readonly System.Reflection.FieldInfo ReachedWallF = AccessTools.Field(typeof(SandSpiderAI), "reachedWallPosition");
        private static readonly System.Reflection.FieldInfo SpooledF = AccessTools.Field(typeof(SandSpiderAI), "spooledPlayerBody");
        private static readonly System.Reflection.FieldInfo WatchF = AccessTools.Field(typeof(SandSpiderAI), "watchFromDistance");

        internal static bool Applies(SandSpiderAI s)
        {
            return Plugin.TrapdoorEnabled.Value && s != null && !s.isEnemyDead && Cocoon.SpiderOutdoors(s) && !TreePerch.Chosen(s) && !TreePerch.Roams(s);
        }

        internal static TD Get(SandSpiderAI s, bool create)
        {
            TD td;
            int id = s.GetInstanceID();
            if (!All.TryGetValue(id, out td) && create) { td = new TD { Since = Time.time, Spider = s }; All[id] = td; }
            return td;
        }

        // True while the spider is hiding or about to: the game must not let it spot players or lay webs.
        internal static bool Ambushing(SandSpiderAI s)
        {
            if (!Applies(s)) return false;
            TD td = Get(s, false);
            return td != null && td.M != Mode.Surfaced && s.currentBehaviourStateIndex == 0;
        }

        internal static void SetHidden(SandSpiderAI s, TD td, bool hide)
        {
            try
            {
                if (hide)
                {
                    if (td.Hidden.Count > 0) return;
                    foreach (Renderer r in s.GetComponentsInChildren<Renderer>(false))
                        if (r != null && r.enabled) { td.Hidden.Add(r); r.enabled = false; }
                }
                else
                {
                    foreach (Renderer r in td.Hidden) if (r != null) r.enabled = true;
                    td.Hidden.Clear();
                }
            }
            catch (Exception) { }
        }

        private static bool PickNode(SandSpiderAI s, TD td)
        {
            if (s.allAINodes == null || s.allAINodes.Length == 0) return false;
            Vector3 here = s.transform.position;
            for (int tries = 0; tries < 12; tries++)
            {
                GameObject n = s.allAINodes[UnityEngine.Random.Range(0, s.allAINodes.Length)];
                if (n == null) continue;
                float d = Vector3.Distance(n.transform.position, here);
                if (d < 12f || d > 60f) continue;
                if (s.SetDestinationToPosition(n.transform.position, true))
                {
                    td.Node = n.transform.position; td.HasNode = true; td.M = Mode.Walk; td.Since = Time.time;
                    Plugin.Log.LogInfo("Trapdoor spider: heading to a spot " + d.ToString("0") + " m away to bury itself.");
                    return true;
                }
            }
            return false;
        }

        // Switches the ragdoll's collisions off so nothing (the ground, say) can stop it being pulled under.
        internal static void NoCollide(DeadBodyInfo b)
        {
            if (b == null || b.bodyParts == null || !Plugin.TrapdoorBodyNoCollide.Value) return;
            try
            {
                foreach (Rigidbody rb in b.bodyParts)
                {
                    if (rb == null) continue;
                    rb.detectCollisions = false;
                    Collider c = rb.GetComponent<Collider>();
                    if (c != null) c.enabled = false;
                }
                Rigidbody root = b.GetComponent<Rigidbody>();
                if (root != null) root.detectCollisions = false;
            }
            catch (Exception) { }
        }

        // After the wrap the game turns the spider to face a wall (to hang its catch there). Keep it standing upright instead.
        private static void StandUpright(SandSpiderAI s)
        {
            try
            {
                if (LookingWallF != null) LookingWallF.SetValue(s, false);
                if (ReachedWallF != null) ReachedWallF.SetValue(s, false);
                if (TargetRotF != null && s.meshContainer != null)
                    TargetRotF.SetValue(s, Quaternion.Euler(0f, s.meshContainer.eulerAngles.y, 0f));
            }
            catch (Exception) { }
        }

        // Writes what the stand-in body and the ground look like to the log, to find out what is blocking it.
        internal static void DumpBody(SandSpiderAI s, TD td, string why)
        {
            try
            {
                DeadBodyInfo b = HeldBody != null ? HeldBody.GetValue(s) as DeadBodyInfo : null;
                if (b == null || b.bodyParts == null) { Plugin.Dbg("Cocoon dump (" + why + "): the spider holds no body."); return; }
                int colOn = 0, kin = 0, detect = 0, n = 0;
                float minY = float.MaxValue, maxY = float.MinValue;
                var blockers = new List<string>();
                StartOfRound sor = StartOfRound.Instance;
                int mask = sor != null ? sor.collidersAndRoomMaskAndDefault : ~0;
                foreach (Rigidbody rb in b.bodyParts)
                {
                    if (rb == null) continue;
                    n++;
                    Collider c = rb.GetComponent<Collider>();
                    if (c != null && c.enabled) colOn++;
                    if (rb.isKinematic) kin++;
                    if (rb.detectCollisions) detect++;
                    float y = rb.position.y;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                    if (blockers.Count < 6)
                    {
                        foreach (Collider hit in Physics.OverlapSphere(rb.position, 0.35f, mask, QueryTriggerInteraction.Ignore))
                        {
                            if (hit == null || hit.transform.IsChildOf(b.transform)) continue;
                            string nm = hit.name + "[layer " + LayerMask.LayerToName(hit.gameObject.layer) + "]";
                            if (!blockers.Contains(nm)) blockers.Add(nm);
                            if (blockers.Count >= 6) break;
                        }
                    }
                }
                float ground = float.NaN;
                if (s.mouthTarget != null && Physics.Raycast(s.mouthTarget.position + Vector3.up * 3f, Vector3.down, out RaycastHit gh, 30f, mask, QueryTriggerInteraction.Ignore)) ground = gh.point.y;
                Plugin.Dbg("Cocoon dump (" + why + "): depth=" + td.Depth.ToString("0.00")
                    + " mouthY=" + (s.mouthTarget != null ? s.mouthTarget.position.y.ToString("0.00") : "?")
                    + " meshY=" + s.meshContainer.position.y.ToString("0.00") + " groundY=" + ground.ToString("0.00")
                    + " limbY=" + (b.attachedLimb != null ? b.attachedLimb.position.y.ToString("0.00") : "none")
                    + " limbKinematic=" + (b.attachedLimb != null && b.attachedLimb.isKinematic)
                    + " attachedToMouth=" + (b.attachedTo == s.mouthTarget)
                    + " parts=" + n + " collidersOn=" + colOn + " kinematic=" + kin + " detectCollisions=" + detect
                    + " partsY=" + minY.ToString("0.00") + ".." + maxY.ToString("0.00")
                    + " touching=[" + string.Join(", ", blockers.ToArray()) + "]");
            }
            catch (Exception e) { Plugin.Log.LogWarning("Cocoon dump failed: " + e.Message); }
        }

        private static readonly List<CocoonState> Marked = new List<CocoonState>();
        private static Material dirtMaterial;
        private static bool dirtSearched;

        internal static CocoonState StateOf(DeadBodyInfo body)
        {
            if (body == null) return null;
            foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
                if (kv.Value != null && kv.Value.Player != null && kv.Value.Player.deadBody == body) return kv.Value;
            return null;
        }

        // Looks through the materials the game has loaded for something earthy (dirt, mud, gravel...). Logs what it picked.
        private static Material FindDirtMaterial()
        {
            if (dirtSearched) return dirtMaterial;
            dirtSearched = true;
            try
            {
                string[] prefer = { "dirt", "mud", "gravel", "soil", "sand", "ground" };
                string[] skip = { "steel", "cruiser", "particle", "snow", "wet", "road", "metal" };
                int bestRank = 99;
                List<string> seen = new List<string>();
                foreach (Material m in Resources.FindObjectsOfTypeAll<Material>())
                {
                    if (m == null || string.IsNullOrEmpty(m.name)) continue;
                    string n = m.name.ToLowerInvariant();
                    bool bad = false;
                    for (int i = 0; i < skip.Length; i++) if (n.Contains(skip[i])) { bad = true; break; }
                    if (bad) continue;
                    for (int i = 0; i < prefer.Length; i++)
                    {
                        if (!n.Contains(prefer[i])) continue;
                        if (seen.Count < 12) seen.Add(m.name);
                        if (i < bestRank) { bestRank = i; dirtMaterial = m; }
                        break;
                    }
                }
                Plugin.Log.LogInfo("Trapdoor marker: earthy materials found: [" + string.Join(", ", seen.ToArray()) + "]; using " + (dirtMaterial != null ? dirtMaterial.name : "none (will copy the ground or use a plain colour)"));
            }
            catch (Exception e) { Plugin.Log.LogWarning("Dirt material search failed: " + e.Message); }
            return dirtMaterial;
        }

        private static void AddLump(Transform parent, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Collider c = g.GetComponent<Collider>(); if (c != null) UnityEngine.Object.Destroy(c);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            g.transform.localScale = scale;
            Renderer r = g.GetComponent<Renderer>();
            if (r != null)
            {
                if (mat != null) r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        private static ParticleSystem dirtPuff;
        private static bool puffSearched;

        // A burst of dust from the game's own dirt particle effect (on every game that marks the spot).
        internal static void Puff(Vector3 pos)
        {
            try
            {
                if (!puffSearched)
                {
                    puffSearched = true;
                    foreach (ParticleSystem ps in Resources.FindObjectsOfTypeAll<ParticleSystem>())
                    {
                        if (ps != null && ps.name != null && ps.name.ToLowerInvariant().Contains("dirtparticle")) { dirtPuff = ps; break; }
                    }
                    Plugin.Log.LogInfo("Trapdoor puff: " + (dirtPuff != null ? "using the game's '" + dirtPuff.name + "'" : "no dirt particle effect is loaded, so no dust puff"));
                }
                if (dirtPuff == null) return;
                ParticleSystem inst = UnityEngine.Object.Instantiate(dirtPuff, pos + Vector3.up * 0.1f, Quaternion.identity);
                inst.gameObject.SetActive(true);
                inst.Play(true);
                UnityEngine.Object.Destroy(inst.gameObject, 4f);
            }
            catch (Exception e) { Plugin.Log.LogWarning("Dust puff failed: " + e.Message); }
        }

        // Remembers where a victim went under and puts a patch of churned earth there, so teammates can find them.
        internal static void MarkBuried(DeadBodyInfo body, SandSpiderAI s)
        {
            try
            {
                CocoonState st = StateOf(body);
                if (st == null || st.Buried || s == null) return;
                Vector3 spot = s.transform.position;
                Material mat = null;
                RaycastHit hit;
                if (Physics.Raycast(spot + Vector3.up * 2f, Vector3.down, out hit, 8f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                {
                    spot = hit.point;
                    mat = FindDirtMaterial();
                    if (mat == null && hit.collider != null)
                    {
                        Renderer gr = hit.collider.GetComponent<Renderer>();
                        if (gr != null) mat = gr.sharedMaterial; // the ground's own material
                    }
                }
                st.Buried = true; st.DigSpot = spot;
                GameObject m = new GameObject("HungToDry_DisturbedEarth");
                m.transform.position = spot;
                Material useMat = mat;
                if (useMat == null)
                {
                    Renderer src = s.GetComponentInChildren<Renderer>();
                    if (src != null)
                    {
                        Material plain = new Material(src.sharedMaterial);
                        try { plain.SetTexture("_BaseColorMap", null); plain.SetColor("_BaseColor", new Color(0.24f, 0.16f, 0.09f)); plain.color = new Color(0.24f, 0.16f, 0.09f); } catch (Exception) { }
                        useMat = plain;
                    }
                }
                // The same lumps on every game: the shape is worked out from the spot itself.
                System.Random rnd = new System.Random(unchecked(Mathf.RoundToInt(spot.x * 10f) * 73856093 ^ Mathf.RoundToInt(spot.z * 10f) * 19349663));
                AddLump(m.transform, Vector3.zero, new Vector3(1.7f, 0.4f, 1.7f), useMat);
                for (int i = 0; i < 5; i++)
                {
                    float ang = (float)(rnd.NextDouble() * Math.PI * 2.0);
                    float rad = 0.35f + (float)rnd.NextDouble() * 0.45f;
                    float size = 0.35f + (float)rnd.NextDouble() * 0.35f;
                    float flat = 0.5f + (float)rnd.NextDouble() * 0.5f;
                    AddLump(m.transform, new Vector3(Mathf.Cos(ang) * rad, 0.02f, Mathf.Sin(ang) * rad), new Vector3(size, size * flat, size), useMat);
                }
                st.Marker = m;
                Puff(spot);
                if (!Marked.Contains(st)) Marked.Add(st);
                Plugin.Log.LogInfo("Trapdoor spider: marked the burial spot at " + spot + ".");
            }
            catch (Exception e) { Plugin.Log.LogWarning("Could not mark the burial spot: " + e.Message); }
        }

        // Removes the patch of earth once its victim is no longer buried (dug out, released or dead).
        internal static void TidyMarkers()
        {
            for (int i = Marked.Count - 1; i >= 0; i--)
            {
                CocoonState st = Marked[i];
                bool alive = false;
                foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
                    if (kv.Value == st) { alive = true; break; }
                if (alive)
                {
                    if (st.Marker != null)
                    {
                        float tt = Time.time;
                        st.Marker.transform.position = st.DigSpot + new Vector3(Mathf.Sin(tt * 31f) * 0.02f, 0f, Mathf.Cos(tt * 27f) * 0.02f);
                        float sc = 1f + 0.04f * Mathf.Sin(tt * 7f);
                        st.Marker.transform.localScale = new Vector3(sc, 1f, sc);
                    }
                    continue;
                }
                if (st.Marker != null) { Puff(st.DigSpot); UnityEngine.Object.Destroy(st.Marker); }
                st.Marker = null;
                Marked.RemoveAt(i);
            }
        }

        // The spider has its catch wrapped: instead of hanging it from a ceiling it drags it down into the ground.
        internal static void BeginPull(SandSpiderAI s, DeadBodyInfo held)
        {
            if (s == null || held == null || !s.IsOwner) return;
            TD td = Get(s, true);
            if (td.M == Mode.Pulling) return;
            CocoonState victim = null;
            foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
                if (kv.Value != null && kv.Value.Player != null && kv.Value.Player.deadBody == held) { victim = kv.Value; td.VictimId = kv.Key; break; }
            if (victim == null) return;
            td.M = Mode.Pulling; td.Since = Time.time; td.HasNode = false; td.NextDump = 0f;
            DumpBody(s, td, "before the pull");
            StandUpright(s);
            NoCollide(held);
            Cocoon.OnBodyHung(held, s);          // starts the death timer and lets the rescue code see a "hung" cocoon
            MarkBuried(held, s);                 // remembers the spot and puts a patch of disturbed earth on it
            try { s.SpiderHangBodyServerRpc(); } catch (Exception) { } // tells the other games (they run OnBodyHung too)
            s.targetPlayer = null;
            s.SwitchToBehaviourState(0);
            Plugin.Log.LogInfo("Trapdoor spider: dragging player " + td.VictimId + " underground.");
        }

        // Kept so the callers stay simple: the depth now travels in its own message (DepthSync).
        internal static void ForceMeshSync(SandSpiderAI s) { }

        internal static List<TD> AllStates() { return new List<TD>(All.Values); }

        internal class RemoteInfo
        {
            public float Target; public bool Hidden; public float Stamp; public float Current; public float NextLog;
            public float RollTarget, RollCur, OpTarget = 1f, OpCur = 1f; public bool Ceil;
            public TD Td = new TD();
        }
        internal static readonly Dictionary<ulong, RemoteInfo> Remote = new Dictionary<ulong, RemoteInfo>();

        // Other players' games: sink and hide the spider as the owner says.
        private static void FrameRemote(SandSpiderAI s)
        {
            Unity.Netcode.NetworkObject no = s.NetworkObject;
            if (no == null) return;
            RemoteInfo r;
            if (!Remote.TryGetValue(no.NetworkObjectId, out r)) return;
            float dt = Time.deltaTime;
            if (Time.time - r.Stamp > 2f) { r.Target = 0f; r.Hidden = false; r.RollTarget = 0f; r.OpTarget = 1f; }
            r.RollCur = Mathf.MoveTowards(r.RollCur, r.RollTarget, 900f * dt);
            r.OpCur = Mathf.MoveTowards(r.OpCur, r.OpTarget, 6f * dt);
            if (r.Target < -0.001f || r.Current < -0.001f) r.Ceil = true;
            if ((Mathf.Abs(r.Target) > 0.001f || Mathf.Abs(r.Current) > 0.001f) && Time.time >= r.NextLog)
            {
                r.NextLog = Time.time + 1f;
                Plugin.Log.LogInfo("Depth (remote): target " + r.Target.ToString("F2") + ", now " + r.Current.ToString("F2") + ", hidden " + r.Hidden
                    + ", model y " + (s.meshContainer != null ? s.meshContainer.position.y.ToString("F2") : "?") + ", spider y " + s.transform.position.y.ToString("F2")
                    + ", state " + s.currentBehaviourStateIndex + ", model active " + (s.meshContainer != null && s.meshContainer.gameObject.activeInHierarchy));
            }
            r.Current = Mathf.MoveTowards(r.Current, r.Target, 12f * dt);
            if (Mathf.Abs(r.Current) > 0.001f && s.meshContainer != null)
            {
                Vector3 p = s.meshContainer.position;
                s.meshContainer.position = new Vector3(p.x, p.y - r.Current, p.z);
                if (r.Ceil) { Ceiling.ApplyRoll(s, r.RollCur); Ceiling.LegsFollow(s); }
            }
            SetHidden(s, r.Td, r.Hidden && Mathf.Abs(r.Current) > 0.01f);
            if (Mathf.Abs(r.Current) <= 0.001f && Mathf.Abs(r.Target) <= 0.001f && !r.Hidden && r.OpCur > 0.99f && r.RollCur < 0.5f)
            { SetHidden(s, r.Td, false); Remote.Remove(no.NetworkObjectId); }
        }

        // The game's AI interval (about five times a second) for a spider that is not busy with anything else.
        internal static bool Interval(SandSpiderAI s)
        {
            if (!Applies(s) || !s.IsOwner || s.currentBehaviourStateIndex != 0) return true;
            TD td0 = Get(s, false);
            bool pulling = td0 != null && td0.M == Mode.Pulling;
            if (!pulling && HeldBody != null && HeldBody.GetValue(s) != null) return true;
            TD td = Get(s, true);
            if (td.M == Mode.Surfaced)
            {
                // Back from a chase or a catch: pick somewhere new to hide.
                td.M = Mode.Walk; td.HasNode = false; td.Since = Time.time;
            }
            if (td.M == Mode.Walk)
            {
                if (!td.HasNode && Time.time >= td.NextPick)
                {
                    td.NextPick = Time.time + 1f;
                    PickNode(s, td);
                }
                if (td.HasNode)
                {
                    s.destination = td.Node;
                    s.agent.speed = 4.25f; s.spiderSpeed = 4.25f;
                    if (Vector3.Distance(s.transform.position, td.Node) < 1.6f || Time.time - td.Since > 45f)
                    {
                        td.M = Mode.Burrowing; td.Since = Time.time;
                        Plugin.Log.LogInfo("Trapdoor spider: burying itself.");
                    }
                }
            }
            if (td.M == Mode.Burrowing || td.M == Mode.Waiting || td.M == Mode.Emerging || td.M == Mode.Pulling)
            {
                s.destination = s.transform.position;
                s.agent.speed = 0f; s.spiderSpeed = 0f;
            }
            s.movingTowardsTargetPlayer = false;
            if (!s.inSpecialAnimation && s.agent.enabled && s.agent.isOnNavMesh) s.agent.SetDestination(s.destination);
            s.SyncPositionToClients();
            ForceMeshSync(s);
            if (s.navigateMeshTowardsPosition && CalcMeshPath != null) CalcMeshPath.Invoke(s, null);
            return false;
        }

        // Every frame, after the game has placed the model: sink it, raise it, watch for victims.
        internal static void Frame(SandSpiderAI s)
        {
            if (s == null || s.isEnemyDead) return;
            if (!s.IsOwner) { FrameRemote(s); return; }
            TD td = Get(s, false);
            if (td == null) return;
            if (!Applies(s)) return;

            float dt = Time.deltaTime;
            // Wrapped and being carried: pull the catch under instead of walking off to hang it.
            if (td.M == Mode.Surfaced && s.currentBehaviourStateIndex == 1 && HeldBody != null && SpooledF != null)
            {
                object held = HeldBody.GetValue(s);
                if (held != null && (bool)SpooledF.GetValue(s)) BeginPull(s, (DeadBodyInfo)held);
            }
            // Something else took over (it was hit, say): burst out at once.
            if ((td.M == Mode.Burrowing || td.M == Mode.Waiting) && s.currentBehaviourStateIndex != 0)
            {
                td.M = Mode.Emerging; td.Since = Time.time; SetHidden(s, td, false);
            }
            switch (td.M)
            {
                case Mode.Burrowing:
                    td.Depth = Mathf.Min(Plugin.TrapdoorDepth.Value, td.Depth + 1.4f * dt);
                    if (td.Depth >= Plugin.TrapdoorDepth.Value - 0.01f)
                    {
                        td.M = Mode.Waiting; td.Since = Time.time;
                        SetHidden(s, td, true);
                        Plugin.Log.LogInfo("Trapdoor spider: buried, waiting.");
                    }
                    break;
                case Mode.Waiting:
                {
                    PlayerControllerB hit = null; float best = Plugin.TrapdoorTriggerMeters.Value;
                    PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
                    for (int i = 0; i < players.Length; i++)
                    {
                        PlayerControllerB p = players[i];
                        if (p == null || !s.PlayerIsTargetable(p)) continue;
                        float d = Vector3.Distance(p.transform.position, s.transform.position);
                        if (d < best) { best = d; hit = p; }
                    }
                    if (hit != null)
                    {
                        td.Target = hit; td.M = Mode.Emerging; td.Since = Time.time; SetHidden(s, td, false);
                        Plugin.Log.LogInfo("Trapdoor spider: bursts out at player " + hit.playerClientId + " (" + best.ToString("0.0") + " m).");
                    }
                    else if (Time.time - td.Since > Plugin.TrapdoorRelocateSeconds.Value)
                    {
                        td.Target = null; td.M = Mode.Emerging; td.Since = Time.time; SetHidden(s, td, false);
                        Plugin.Log.LogInfo("Trapdoor spider: nobody came; moving to another spot.");
                    }
                    break;
                }
                case Mode.Pulling:
                {
                    bool victimGone = td.VictimId < 0 || !Cocoon.ActiveStates.ContainsKey(td.VictimId) || HeldBody.GetValue(s) == null;
                    if (victimGone || s.currentBehaviourStateIndex != 0)
                    {
                        // Dug out: whoever is nearest the hole is the one the spider goes for.
                        PlayerControllerB avenger = null; float ad = 12f;
                        PlayerControllerB[] ps = StartOfRound.Instance.allPlayerScripts;
                        for (int i = 0; i < ps.Length; i++)
                        {
                            PlayerControllerB q = ps[i];
                            if (q == null || q.isPlayerDead || !q.isPlayerControlled || !s.PlayerIsTargetable(q)) continue;
                            float qd = Vector3.Distance(q.transform.position, s.transform.position);
                            if (qd < ad) { ad = qd; avenger = q; }
                        }
                        td.Target = avenger; td.M = Mode.Emerging; td.Since = Time.time; SetHidden(s, td, false);
                        Plugin.Log.LogInfo("Trapdoor spider: the victim is gone (freed or dug out); climbing out" + (avenger != null ? " after player " + avenger.playerClientId + "." : "."));
                        break;
                    }
                    StandUpright(s);
                    // Sink only once it is standing upright again (or after a moment, whatever the pose).
                    bool upright = s.meshContainer == null || Vector3.Dot(s.meshContainer.up, Vector3.up) > 0.93f || Time.time - td.Since > 2f;
                    if (upright) td.Depth = Mathf.Min(Plugin.TrapdoorDepth.Value, td.Depth + 1.4f * dt);
                    if (td.Depth >= Plugin.TrapdoorDepth.Value - 0.01f) SetHidden(s, td, true);
                    if (Time.time >= td.NextDump)
                    {
                        td.NextDump = Time.time + 0.5f;
                        NoCollide(HeldBody.GetValue(s) as DeadBodyInfo); // the game may switch them back on
                        DumpBody(s, td, "pulling");
                    }
                    if (Time.time - td.Since > Plugin.TrapdoorHoldSeconds.Value)
                    {
                        Plugin.Log.LogInfo("Trapdoor spider: lets player " + td.VictimId + " go.");
                        int id = td.VictimId;
                        td.Target = null; td.M = Mode.Emerging; td.Since = Time.time; SetHidden(s, td, false);
                        CocoonNet.RequestRelease(id);
                    }
                    break;
                }
                case Mode.Emerging:
                    td.Depth = Mathf.Max(0f, td.Depth - 5f * dt);
                    if (td.Depth <= 0.01f)
                    {
                        td.Depth = 0f;
                        ForceMeshSync(s);
                        if (td.Target != null && !td.Target.isPlayerDead)
                        {
                            s.targetPlayer = td.Target;
                            if (WatchF != null) WatchF.SetValue(s, false);
                            if (ChaseTimerF != null) ChaseTimerF.SetValue(s, 12.5f);
                            s.SwitchToBehaviourState(2);
                            td.M = Mode.Surfaced;
                        }
                        else { td.M = Mode.Walk; td.HasNode = false; }
                        td.Since = Time.time; td.Target = null;
                    }
                    break;
                default:
                    if (td.Depth > 0f) td.Depth = Mathf.Max(0f, td.Depth - 5f * dt);
                    break;
            }
            if (td.Depth > 0f && s.meshContainer != null)
            {
                Vector3 p = s.meshContainer.position;
                s.meshContainer.position = new Vector3(p.x, p.y - td.Depth, p.z);
            }
        }
    }

    // Outdoor spiders: the trees. Which spiders wait in trees is decided the same way on every player's game (by the
    // spider's network id), so the host and the joiners never disagree about whether it should be buried or up a tree.
    internal static class TreePerch
    {
        private static GameObject[] trees;
        private static float builtAt = -999f;

        // Outdoor spiders that roam and stalk directly (no tree, no burying). Decided once per spider, the same on every game.
        internal static bool Roams(SandSpiderAI s)
        {
            if (s == null || !Plugin.TreeStalk.Value) return false;
            Unity.Netcode.NetworkObject no = s.NetworkObject;
            ulong id = no != null ? no.NetworkObjectId : (ulong)(uint)s.GetInstanceID();
            float pct = Cocoon.SpiderOutdoors(s) ? Plugin.StalkDirectChance.Value : Plugin.IndoorStalkChance.Value;
            return ((id * 2246822519UL) >> 7) % 100UL < (ulong)Mathf.RoundToInt(pct);
        }

        internal static bool Chosen(SandSpiderAI s)
        {
            if (s == null || !Plugin.TreeEnabled.Value || Roams(s)) return false;
            Unity.Netcode.NetworkObject no = s.NetworkObject;
            ulong id = no != null ? no.NetworkObjectId : (ulong)(uint)s.GetInstanceID();
            return (id * 2654435761UL) % 100UL < (ulong)Mathf.RoundToInt(Plugin.TreeChance.Value);
        }

        internal static GameObject[] Trees()
        {
            try
            {
                if (trees == null || trees.Length == 0 || trees[0] == null || Time.time - builtAt > 120f)
                {
                    trees = GameObject.FindGameObjectsWithTag("Tree");
                    builtAt = Time.time;
                    Plugin.Dbg("Tree perch: found " + trees.Length + " trees on this level.");
                }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Tree perch: could not list the trees: " + e.Message); trees = new GameObject[0]; }
            return trees;
        }

        // Two thin web lines from the top of the rope to the trunk (the tree only has a solid trunk, no branch to tie to).
        // They copy the rope's own look and are children of the rope, so they go when the rope does.
        private static void AddStrands(SetLineRendererPoints rope, Vector3 anchor, Vector3 trunkPoint)
        {
            try
            {
                LineRenderer lr = rope.GetComponent<LineRenderer>();
                if (lr == null) lr = rope.GetComponentInChildren<LineRenderer>();
                if (lr == null) { Plugin.Dbg("Tree spider: the rope has no line renderer to copy; no strands drawn."); return; }
                float[] rise = { 0.8f, -0.3f };
                for (int i = 0; i < rise.Length; i++)
                {
                    GameObject go = new GameObject("TreeWebStrand");
                    go.transform.SetParent(rope.transform, true);
                    LineRenderer s = go.AddComponent<LineRenderer>();
                    s.useWorldSpace = true;
                    s.positionCount = 2;
                    s.SetPosition(0, anchor);
                    s.SetPosition(1, new Vector3(trunkPoint.x, anchor.y + rise[i], trunkPoint.z));
                    s.sharedMaterial = lr.sharedMaterial;
                    s.startWidth = lr.startWidth; s.endWidth = lr.endWidth;
                    s.textureMode = lr.textureMode;
                    s.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Tree spider: could not draw the web strands: " + e.Message); }
        }

        // Hangs the spider's catch from the nearest tree (within 30 m): the rope's top is up in the tree beside the trunk and the
        // victim hangs below it, at the same height above the ground as a victim hung indoors (HangHeightAboveGround).
        // Returns false (and does nothing) when there is no tree to use; the caller then drops the cocoon on the ground.
        internal static bool TryHang(SandSpiderAI s, ref DeadBodyInfo held, ref bool decided)
        {
            try
            {
                if (s == null || held == null || held.bodyParts == null || held.bodyParts.Length <= 6 || s.hangBodyPhysicsPrefab == null) return false;
                Vector3 bodyPos = held.bodyParts[6].transform.position;
                GameObject best = null; float bd = 30f;
                foreach (GameObject tr in Trees())
                {
                    if (tr == null) continue;
                    Vector3 d = tr.transform.position - bodyPos; d.y = 0f;
                    if (d.magnitude < bd) { bd = d.magnitude; best = tr; }
                }
                if (best == null) return false;
                Vector3 from = new Vector3(bodyPos.x, bodyPos.y + 1f, bodyPos.z);
                Vector3 dir = new Vector3(best.transform.position.x, from.y, best.transform.position.z) - from;
                if (dir.sqrMagnitude < 0.01f) return false;
                dir.Normalize();
                RaycastHit hit;
                if (!Physics.Raycast(new Ray(from, dir), out hit, bd + 8f, 33554432, QueryTriggerInteraction.Ignore)) return false;
                Vector3 xz = hit.point - dir * Plugin.TreeHangOut.Value;
                RaycastHit floor;
                if (!Physics.Raycast(new Vector3(xz.x, bodyPos.y + 3f, xz.z), Vector3.down, out floor, 15f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)) return false;
                float h = Mathf.Max(Plugin.HangHeightAboveGround.Value, 0.5f);
                Vector3 anchor = new Vector3(xz.x, floor.point.y + Mathf.Max(Plugin.TreeHeight.Value, h + 2.5f), xz.z);
                Vector3 target = new Vector3(xz.x, floor.point.y + h, xz.z);

                Cocoon.OnBodyHung(held, s);
                SetLineRendererPoints line = UnityEngine.Object.Instantiate(s.hangBodyPhysicsPrefab, anchor, Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform).GetComponent<SetLineRendererPoints>();
                line.target.position = target;
                held.attachedTo = line.target;
                decided = false;
                // the solid trunk is wider than the trunk you can see up there, so the strands go further in (as far as TreeGap)
                AddStrands(line, anchor, hit.point + dir * (0.3f + Mathf.Max(0f, -Plugin.TreeGap.Value)));
                held.bodyAudio.volume = 0.8f;
                held.bodyAudio.PlayOneShot(s.hangPlayerSFX);
                held = null;
                Plugin.Log.LogInfo("Tree spider: hangs its catch from '" + best.name + "' (" + bd.ToString("0") + " m away), rope top "
                    + (anchor.y - floor.point.y).ToString("0.0") + " m up, victim " + h.ToString("0.0") + " m above the ground.");
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Tree hang failed: " + e);
                return false;
            }
        }
    }

    // A spider inside the facility climbs to the ceiling, turns invisible and drops on whoever walks beneath it.
    // The model is raised the same way the trapdoor spider's is lowered; after the drop the game's own chase,
    // bite, wrap and hang take over (and this mod's venom, cocoon and rescue with them).
    internal static class Ceiling
    {
        internal enum Mode { Walk, Rising, Waiting, Dropping, Done, Stalk, Web }

        internal class CS
        {
            public Mode M = Mode.Walk;
            public Vector3 Node; public bool HasNode;
            public float Height, Goal, Since, NextPick, FallSpeed;
            public bool Skip;
            public float NextLog;
            public float Roll;
            public float Goal0;
            public float DropFrom, DropRoll;
            public bool Tree;                 // outdoors: clinging to a trunk instead of a ceiling
            public Vector3 TreePos, WallP, WallN; public bool HasWall;
            public float Stand = 1f;          // how far the body's pivot sits out from the trunk surface
            public float Turn;                // 0 = head up the trunk, 1 = turned right round to face down
            public float CeilY = -99999f;
            public float Opacity = 1f;
            public bool FadeActive;
            public PlayerControllerB Target;
            public bool Stalk;                // the drop in progress ends in a stalk, not a chase
            public PlayerControllerB StalkTarget;
            public float Meter, CloseSince = -1f; public bool Frozen, StalkLogged, Roam, Cloaked; public float CloakOp = 1f; public float WebT, WebBlocked; public Dictionary<int, Vector2> PrevAng = new Dictionary<int, Vector2>();
            public List<Renderer> Hidden = new List<Renderer>();
            public SandSpiderAI Spider;
        }

        private static readonly Dictionary<int, CS> All = new Dictionary<int, CS>();
        private static readonly System.Reflection.MethodInfo LookAtF = AccessTools.Method(typeof(SandSpiderAI), "SetSpiderLookAtPosition");
        private static readonly System.Reflection.MethodInfo CalcMeshPath = AccessTools.Method(typeof(SandSpiderAI), "CalculateSpiderPathToPosition");
        private static readonly System.Reflection.FieldInfo HeldBody = AccessTools.Field(typeof(SandSpiderAI), "currentlyHeldBody");
        private static readonly System.Reflection.FieldInfo ChaseTimerF = AccessTools.Field(typeof(SandSpiderAI), "chaseTimer");
        private static readonly System.Reflection.FieldInfo WatchF = AccessTools.Field(typeof(SandSpiderAI), "watchFromDistance");

        // ---- DIAGNOSTICS for the "spider sits on the player and cannot attack" bug ----
        private sealed class DiagState { public Mode LastMode; public int LastState = -99; public float Next; public bool Inited; }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, DiagState> DiagStates
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, DiagState>();

        private static float YawOf(Transform tr) { return tr == null ? 0f : tr.eulerAngles.y; }

        internal static void Diag(SandSpiderAI s)
        {
            if (s == null || s.isEnemyDead || !Plugin.DebugLogging.Value || !Plugin.VerboseLogs.Value) return;
            CS cs = Get(s, false);
            if (cs == null) return;
            DiagState d = DiagStates.GetOrCreateValue(s);
            int bs = s.currentBehaviourStateIndex;
            string id = s.GetInstanceID().ToString();
            bool changed = !d.Inited || d.LastMode != cs.M || d.LastState != bs;
            // nearest targetable player
            PlayerControllerB near = null; float nd = 9999f;
            PlayerControllerB[] ps = StartOfRound.Instance.allPlayerScripts;
            for (int i = 0; i < ps.Length; i++)
            {
                PlayerControllerB p = ps[i];
                if (p == null || !p.isPlayerControlled || p.isPlayerDead) continue;
                float dd = Vector3.Distance(new Vector3(p.transform.position.x, s.transform.position.y, p.transform.position.z), s.transform.position);
                if (dd < nd) { nd = dd; near = p; }
            }
            bool close = near != null && nd < 2.6f;
            if (!changed && !(close && Time.time >= d.Next)) return;
            if (close) d.Next = Time.time + 0.5f;
            Transform m = s.meshContainer;
            float gy = YawOf(s.transform), my = YawOf(m);
            float yawDiff = Mathf.Abs(Mathf.DeltaAngle(gy, my));
            float meshToGhost = m != null ? Vector3.Distance(m.position, s.transform.position) : -1f;
            UnityEngine.AI.NavMeshAgent ag = s.agent;
            object chase = ChaseTimerF != null ? ChaseTimerF.GetValue(s) : null;
            object watch = WatchF != null ? WatchF.GetValue(s) : null;
            string tgt = s.targetPlayer != null ? s.targetPlayer.playerClientId.ToString() : "none";
            string why = changed ? ("CHANGE " + (d.Inited ? d.LastMode + "/" + d.LastState + " -> " : "first -> ")) : "NEAR";
            Plugin.Verbose("Spider diag [" + id + "] " + why + cs.M + "/state " + bs
                + " | nearest player " + (near != null ? near.playerClientId.ToString() : "-") + " at " + nd.ToString("0.0") + " m"
                + (near != null ? " (targetable " + s.PlayerIsTargetable(near) + ", cocooned " + Cocoon.IsCocooned(near) + ", venomed " + Cocoon.IsVenomed(near) + ", immune " + Cocoon.IsImmune(near) + ")" : "")
                + " | target " + tgt + " chaseTimer " + (chase != null ? ((float)chase).ToString("0.0") : "-") + " watchFromDistance " + watch
                + " | ghostYaw " + gy.ToString("0") + " meshYaw " + my.ToString("0") + " diff " + yawDiff.ToString("0")
                + " | mesh-to-ghost " + meshToGhost.ToString("0.0") + " m, meshTarget-to-ghost " + (m != null ? Vector3.Distance(s.meshContainerTarget, s.transform.position).ToString("0.0") : "-")
                + " | agent " + (ag != null ? "en=" + ag.enabled + " onMesh=" + (ag.enabled && ag.isOnNavMesh) + " stopped=" + (ag.enabled && ag.isStopped) + " speed=" + ag.speed.ToString("0.0") + " vel=" + ag.velocity.magnitude.ToString("0.0") + " path=" + (ag.enabled ? ag.pathStatus.ToString() : "-") : "none")
                + " | spiderSpeed " + s.spiderSpeed.ToString("0.0") + " special " + s.inSpecialAnimation + " stun " + s.stunNormalizedTimer.ToString("0.0")
                + " | tree " + cs.Tree + " roam " + cs.Roam + " frozen " + cs.Frozen + " web " + SpiderWeb.IsActive(s));
            d.LastMode = cs.M; d.LastState = bs; d.Inited = true;
        }

        internal static List<CS> AllStates() { return new List<CS>(All.Values); }

        internal static CS Get(SandSpiderAI s, bool create)
        {
            CS cs;
            if (s == null) return null;
            int id = s.GetInstanceID();
            if (!All.TryGetValue(id, out cs) && create) { cs = new CS { Since = Time.time, Spider = s }; All[id] = cs; Roll(cs); }
            return cs;
        }

        internal static bool Applies(SandSpiderAI s)
        {
            return Plugin.CeilingEnabled.Value && s != null && !s.isEnemyDead && (!Cocoon.SpiderOutdoors(s) || TreePerch.Chosen(s) || TreePerch.Roams(s));
        }

        private static void Roll(CS cs)
        {
            cs.Skip = UnityEngine.Random.Range(0f, 100f) >= Plugin.CeilingChance.Value;
            cs.Since = Time.time;
        }

        // True while the spider is on its way to, or up in, the ceiling: it must not notice players or lay webs.
        internal static bool Ambushing(SandSpiderAI s)
        {
            if (!Applies(s)) return false;
            CS cs = Get(s, false);
            if (Cocoon.SpiderOutdoors(s) && TreePerch.Roams(s)) return s.currentBehaviourStateIndex == 0; // a roaming stalker never spots or webs on its own
            if (cs == null || cs.Skip || s.currentBehaviourStateIndex != 0) return false;
            return cs.M == Mode.Rising || cs.M == Mode.Waiting || cs.M == Mode.Stalk || cs.M == Mode.Web || (cs.M == Mode.Walk && cs.HasNode);
        }

        // A roaming stalker (any location) never spots players by itself; the stalk is what starts the attack.
        internal static bool Stalker(SandSpiderAI s)
        {
            return Applies(s) && s.currentBehaviourStateIndex == 0 && TreePerch.Roams(s);
        }

        private static void SetHidden(SandSpiderAI s, CS cs, bool hide)
        {
            try
            {
                if (hide)
                {
                    if (cs.Hidden.Count > 0) return;
                    foreach (Renderer r in s.GetComponentsInChildren<Renderer>(false))
                        if (r != null && r.enabled) { cs.Hidden.Add(r); r.enabled = false; }
                }
                else
                {
                    foreach (Renderer r in cs.Hidden) if (r != null) r.enabled = true;
                    cs.Hidden.Clear();
                }
            }
            catch (Exception) { }
        }

        private static float lastCeilY = -99999f;

        // The ceiling above a spot, as a height to raise the spider by; negative if there is no usable ceiling.
        private static float GoalAbove(Vector3 pos)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Ray(pos + Vector3.up * 0.5f, Vector3.up), out hit, 30f, 256))
            {
                lastCeilY = hit.point.y;
                return hit.distance - 2f; // a first estimate; the climb is then fitted to the real feet
            }
            lastCeilY = -99999f;
            return -1f;
        }

        internal static bool Interval(SandSpiderAI s)
        {
            if (!Applies(s) || !s.IsOwner || s.currentBehaviourStateIndex != 0) return true;
            if (HeldBody != null && HeldBody.GetValue(s) != null) return true;
            CS cs = Get(s, true);
            if (cs.M == Mode.Done)
            {
                cs.M = Mode.Walk; cs.HasNode = false; Roll(cs);
            }
            cs.Roam = TreePerch.Roams(s);
            if (cs.Roam && cs.M == Mode.Walk)
            {
                // Roaming stalker: wanders as normal; a player nearby who is not looking at it starts the stalk.
                if (Plugin.TreeStalk.Value && Time.time - cs.Since > 20f)
                {
                    bool outd = Cocoon.SpiderOutdoors(s);
                    PlayerControllerB cand = null; float cd = outd ? Plugin.StalkTriggerMeters.Value : Plugin.IndoorStalkTriggerMeters.Value;
                    Vector3 sp0 = s.transform.position;
                    PlayerControllerB[] ps = StartOfRound.Instance.allPlayerScripts;
                    for (int i = 0; i < ps.Length; i++)
                    {
                        PlayerControllerB p = ps[i];
                        if (p == null || !s.PlayerIsTargetable(p)) continue;
                        Vector3 pp = p.transform.position;
                        float d = Vector2.Distance(new Vector2(pp.x, pp.z), new Vector2(sp0.x, sp0.z));
                        if (d >= cd) continue;
                        if (!outd && Mathf.Abs(pp.y - sp0.y) > 3f) continue; // inside: another floor does not count
                        float ang;
                        if (SeenBy(p, sp0 + Vector3.up * 0.7f, out ang)) continue;
                        cd = d; cand = p;
                    }
                    if (cand != null)
                    {
                        cs.StalkTarget = cand; cs.StalkLogged = false; cs.Meter = 0f; cs.Frozen = false; cs.CloseSince = -1f;
                        cs.M = Mode.Stalk; cs.Since = Time.time;
                        Plugin.Log.LogInfo("Roaming spider: starts stalking player " + cand.playerClientId + " (" + cd.ToString("0.0") + " m away).");
                    }
                }
                if (cs.M == Mode.Walk) return true;
            }
            if (cs.M == Mode.Web)
            {
                s.destination = s.transform.position;
                s.movingTowardsTargetPlayer = false;
                if (!s.inSpecialAnimation && s.agent.enabled && s.agent.isOnNavMesh) s.agent.SetDestination(s.destination);
                s.SyncPositionToClients();
                return false;
            }
            if (cs.M == Mode.Stalk)
            {
                if (cs.StalkTarget == null || cs.StalkTarget.isPlayerDead) { Plugin.Log.LogInfo("Stalk: target gone (interval)."); cs.M = Mode.Walk; cs.HasNode = false; return true; }
                s.destination = cs.Frozen ? s.transform.position : cs.StalkTarget.transform.position;
                s.movingTowardsTargetPlayer = false;
                if (!s.inSpecialAnimation && s.agent.enabled && s.agent.isOnNavMesh) s.agent.SetDestination(s.destination);
                s.SyncPositionToClients();
                return false;
            }
            if (cs.Skip)
            {
                if (Time.time - cs.Since > 60f) Roll(cs);
                return true;
            }
            if (cs.M == Mode.Walk)
            {
                if (!cs.HasNode && Time.time >= cs.NextPick)
                {
                    cs.NextPick = Time.time + 1f;
                    PickNode(s, cs);
                }
                if (cs.HasNode)
                {
                    s.destination = cs.Node;
                    s.agent.speed = 4.25f; s.spiderSpeed = 4.25f;
                    bool agentThere = Vector3.Distance(s.transform.position, cs.Node) < 1.6f;
                    bool timedOut = Time.time - cs.Since > 45f;
                    float modelGap = 0f;
                    if (s.meshContainer != null)
                    {
                        Vector3 mp = s.meshContainer.position, tp = s.transform.position;
                        modelGap = Vector2.Distance(new Vector2(mp.x, mp.z), new Vector2(tp.x, tp.z));
                    }
                    if (agentThere || timedOut)
                    {
                        // The visible model trails behind the real spider and can get stuck (on a door, say).
                        // Bring it onto the real spider before the climb starts.
                        if (modelGap > 0.3f && s.meshContainer != null)
                        {
                            Vector3 tp = s.transform.position;
                            s.meshContainerPosition = tp;
                            s.meshContainer.position = tp;
                            s.meshContainerTarget = tp;
                            Plugin.Log.LogInfo("Ceiling spider: its body was " + modelGap.ToString("0.0") + " m behind; moved it onto the spider before the climb.");
                        }
                        float goal = cs.Tree ? TreeGoal(s, cs) : GoalAbove(s.transform.position);
                        if (goal >= 0.5f)
                        {
                            cs.Goal = goal; cs.Goal0 = goal; cs.CeilY = lastCeilY; cs.M = Mode.Rising; cs.Since = Time.time;
                            Plugin.Log.LogInfo("Ceiling spider: climbing up " + goal.ToString("0.0") + " m to the ceiling.");
                        }
                        else { cs.HasNode = false; cs.Since = Time.time; }
                    }
                }
            }
            if (cs.M == Mode.Rising || cs.M == Mode.Waiting || cs.M == Mode.Dropping)
            {
                s.destination = s.transform.position;
                s.agent.speed = 0f; s.spiderSpeed = 0f;
            }
            s.movingTowardsTargetPlayer = false;
            if (!s.inSpecialAnimation && s.agent.enabled && s.agent.isOnNavMesh) s.agent.SetDestination(s.destination);
            s.SyncPositionToClients();
            bool up = cs.M == Mode.Rising || cs.M == Mode.Waiting || cs.M == Mode.Dropping;
            if (up) s.meshContainerTarget = s.meshContainerPosition; // the path would start from the raised model and drag it upward
            else if (s.navigateMeshTowardsPosition && CalcMeshPath != null && !((cs.Tree || cs.Roam) && (cs.M == Mode.Walk || cs.M == Mode.Stalk || cs.M == Mode.Web))) CalcMeshPath.Invoke(s, null);
            return false;
        }

        // Outdoors: the "ceiling" is the underside of a tree's branches. Pick a tree, then walk to its foot.
        private static bool PickTree(SandSpiderAI s, CS cs)
        {
            GameObject[] ts = TreePerch.Trees();
            if (ts.Length == 0 || RoundManager.Instance == null) return false;
            Vector3 here = s.transform.position;
            for (int tries = 0; tries < 12; tries++)
            {
                GameObject tr = ts[UnityEngine.Random.Range(0, ts.Length)];
                if (tr == null) continue;
                float d = Vector3.Distance(tr.transform.position, here);
                if (d < 6f || d > 50f) continue;
                Vector3 np = RoundManager.Instance.GetNavMeshPosition(tr.transform.position, default(UnityEngine.AI.NavMeshHit), 5f, s.agent.areaMask);
                if (np == Vector3.zero) continue;
                if (s.SetDestinationToPosition(np, true))
                {
                    cs.Node = np; cs.HasNode = true; cs.M = Mode.Walk; cs.Since = Time.time; cs.TreePos = tr.transform.position;
                    Plugin.Log.LogInfo("Tree spider: heading to a tree " + d.ToString("0") + " m away ('" + tr.name + "').");
                    return true;
                }
            }
            return false;
        }

        // Outdoors: a ray from the spider, level with a given height, straight at the tree it chose (tree layer only).
        private static bool FindTrunk(SandSpiderAI s, CS cs, float h, out RaycastHit hit)
        {
            Vector3 dir = cs.TreePos - s.transform.position; dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = s.transform.forward;
            dir.Normalize();
            return Physics.Raycast(new Ray(s.transform.position + Vector3.up * h, dir), out hit, 9f, 33554432, QueryTriggerInteraction.Ignore);
        }

        // Outdoors: the trunk is what it clings to. Returns how far to climb, or -1 when there is no trunk here.
        private static float TreeGoal(SandSpiderAI s, CS cs)
        {
            RaycastHit hit;
            if (!FindTrunk(s, cs, 1.5f, out hit))
            {
                Plugin.Dbg("Tree spider: no trunk to cling to at that spot; picking another tree.");
                return -1f;
            }
            Vector3 n = hit.normal; n.y = 0f;
            if (n.sqrMagnitude < 0.01f) { n = s.transform.position - hit.point; n.y = 0f; }
            cs.WallP = hit.point; cs.WallN = n.normalized; cs.HasWall = true; cs.Stand = 1f; cs.Turn = 0f;
            Plugin.Dbg("Tree spider: trunk '" + hit.collider.name + "' (layer " + hit.collider.gameObject.layer + ", tag " + hit.collider.tag
                + ") " + hit.distance.ToString("0.0") + " m away; climbing " + Plugin.TreeHeight.Value.ToString("0.0") + " m up it.");
            lastCeilY = -99999f;
            return Plugin.TreeHeight.Value;
        }

        // Puts each foot where it grips the trunk. First the feet are spread in or out (TreeLegSpread), then each one is
        // sent along the trunk's curve: a ray from outside, aimed at the tree's middle, finds the surface to plant it on,
        // so the outer feet wrap round the sides instead of sticking out past them. f = how far it has gone onto the trunk.
        private static void LegsOnTrunk(SandSpiderAI s, CS cs, float f)
        {
            try
            {
                if (s.legTargets == null || s.legDefaultPositions == null || s.meshContainer == null) return;
                Transform mc = s.meshContainer;
                Vector3 up = mc.up;
                Vector3 pivot = mc.position + up;
                float spread = Mathf.Lerp(1f, Plugin.TreeLegSpread.Value / 100f, f);
                Vector3 n = cs.HasWall ? cs.WallN : up;
                for (int i = 0; i < s.legTargets.Length && i < s.legDefaultPositions.Length; i++)
                {
                    Transform lt = s.legTargets[i]; Transform df = s.legDefaultPositions[i];
                    if (lt == null || df == null) continue;
                    Vector3 d = df.position - pivot;
                    Vector3 dn = up * Vector3.Dot(d, up);
                    Vector3 foot = pivot + dn + (d - dn) * spread;
                    Vector3 res = foot;
                    if (f > 0.01f && cs.HasWall)
                    {
                        Vector3 o = foot + n * 2f;
                        Vector3 dir = new Vector3(cs.TreePos.x, o.y, cs.TreePos.z) - o; dir.y = 0f;
                        if (dir.sqrMagnitude > 0.01f)
                        {
                            dir.Normalize();
                            RaycastHit hit;
                            if (Physics.Raycast(new Ray(o, dir), out hit, 5f, 33554432, QueryTriggerInteraction.Ignore))
                                res = hit.point - dir * Plugin.TreeGap.Value;
                        }
                    }
                    lt.position = Vector3.Lerp(foot, res, f);
                }
            }
            catch (Exception) { }
        }

        // Keeps the trunk's surface point up to date at the height the body is at (trunks lean and taper).
        private static void UpdateWall(SandSpiderAI s, CS cs)
        {
            RaycastHit hit;
            if (!FindTrunk(s, cs, cs.Height + 1f, out hit)) return;
            cs.WallP = hit.point;
            Vector3 n = hit.normal; n.y = 0f;
            if (n.sqrMagnitude > 0.01f) cs.WallN = n.normalized;
            cs.HasWall = true;
        }

        // Head up, back away from the trunk, feet on it. f = 0 is standing normally, 1 = fully on the trunk.
        // The body turns about a point 1 m above where it stands, and that point slides out to the trunk as it climbs.
        private static void ApplyTrunk(SandSpiderAI s, CS cs, float f, Vector3 basePos)
        {
            Transform mc = s.meshContainer;
            if (mc == null) return;
            f = Mathf.Clamp01(f);
            RollHolder h = Rolls.GetOrCreateValue(s);
            h.Pre = mc.rotation; h.Has = true;
            Vector3 n = cs.HasWall ? cs.WallN : (s.transform.position - cs.TreePos);
            n.y = 0f;
            n = n.sqrMagnitude > 0.01f ? n.normalized : Vector3.forward;
            Quaternion level = Quaternion.Euler(0f, mc.eulerAngles.y, 0f);
            if (cs.M == Mode.Rising)
                cs.Turn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((cs.Height - cs.Goal0 * 0.8f) / Mathf.Max(0.01f, cs.Goal0 * 0.2f)));
            else if (cs.M == Mode.Waiting) cs.Turn = 1f;
            float g = cs.M == Mode.Dropping ? cs.Turn * f : cs.Turn;
            // Turning about the trunk's own normal keeps the feet on the trunk: the head swings round the side to point down.
            Vector3 fwd = Quaternion.AngleAxis(180f * g, n) * Vector3.up;
            Quaternion target = Quaternion.LookRotation(fwd, n);
            Quaternion r = Quaternion.Slerp(level, target, f);
            Vector3 pivotLevel = basePos + Vector3.up * (1f + cs.Height);
            Vector3 pivotWall = (cs.HasWall ? cs.WallP : pivotLevel) + n * cs.Stand;
            pivotWall.y = pivotLevel.y;
            Vector3 pivot = Vector3.Lerp(pivotLevel, pivotWall, f);
            mc.rotation = r;
            mc.position = pivot - r * Vector3.up;
        }

        // How far the feet stand off the trunk surface (average, in metres). NaN if there is nothing to measure.
        private static float FootDot(SandSpiderAI s, CS cs)
        {
            float sum = 0f; int n = 0;
            try
            {
                if (s.legDefaultPositions != null && cs.HasWall)
                    foreach (Transform lt in s.legDefaultPositions)
                        if (lt != null) { sum += Vector3.Dot(lt.position - cs.WallP, cs.WallN); n++; }
            }
            catch (Exception) { }
            return n > 0 ? sum / n : float.NaN;
        }

        // Once nearly up, nudge the body in or out until the feet just touch the trunk.
        private static void FitTrunk(SandSpiderAI s, CS cs)
        {
            if ((cs.M != Mode.Rising && cs.M != Mode.Waiting) || cs.Height < cs.Goal0 * 0.75f) return;
            float d = FootDot(s, cs);
            if (float.IsNaN(d)) return;
            cs.Stand = Mathf.Clamp(cs.Stand - (d - Plugin.TreeGap.Value) * 0.5f, -2f, 4f);
        }

        private static bool PickNode(SandSpiderAI s, CS cs)
        {
            cs.Tree = Cocoon.SpiderOutdoors(s);
            if (cs.Tree) return PickTree(s, cs);
            if (s.allAINodes == null || s.allAINodes.Length == 0) return false;
            Vector3 here = s.transform.position;
            for (int tries = 0; tries < 12; tries++)
            {
                GameObject n = s.allAINodes[UnityEngine.Random.Range(0, s.allAINodes.Length)];
                if (n == null) continue;
                float d = Vector3.Distance(n.transform.position, here);
                if (d < 6f || d > 50f) continue;
                if (GoalAbove(n.transform.position) < 0.5f) continue;
                if (s.SetDestinationToPosition(n.transform.position, true))
                {
                    cs.Node = n.transform.position; cs.HasNode = true; cs.M = Mode.Walk; cs.Since = Time.time;
                    Plugin.Log.LogInfo("Ceiling spider: heading to a spot " + d.ToString("0") + " m away to climb the ceiling.");
                    return true;
                }
            }
            if (Time.time - lastNoSpot > 10f)
            {
                lastNoSpot = Time.time;
                Plugin.Dbg("Ceiling spider: found no spot with a ceiling within reach (outdoors: " + s.isOutside + ", nodes: " + s.allAINodes.Length + ", at " + here + "); it stays put.");
            }
            return false;
        }

        private static float lastNoSpot = -100f;

        internal static void Frame(SandSpiderAI s)
        {
            if (s == null || s.isEnemyDead || !s.IsOwner) return;
            CS cs = Get(s, false);
            if (cs == null || !Applies(s)) return;
            float dt = Time.deltaTime;

            // The model must stay where it is while it is up there (otherwise it keeps walking after its goal).
            if (cs.M == Mode.Rising || cs.M == Mode.Waiting || cs.M == Mode.Dropping) s.meshContainerTarget = s.meshContainerPosition;

            // Something else took over (it was hit, say): come down at once.
            if ((cs.M == Mode.Rising || cs.M == Mode.Waiting) && s.currentBehaviourStateIndex != 0)
            {
                cs.M = Mode.Dropping; cs.FallSpeed = 0f; cs.Target = null; SetHidden(s, cs, false);
            }
            if ((cs.M == Mode.Stalk || cs.M == Mode.Web) && s.currentBehaviourStateIndex != 0)
            {
                Plugin.Log.LogInfo("Stalk: ended because the spider's behaviour state is now " + s.currentBehaviourStateIndex + ".");
                if (cs.M == Mode.Web) SpiderWeb.Stop(s);
                cs.M = Mode.Done;
            }
            switch (cs.M)
            {
                case Mode.Rising:
                    cs.Height = Mathf.MoveTowards(cs.Height, cs.Goal, Plugin.CeilingRiseSpeed.Value * dt);
                    if (Mathf.Abs(cs.Height - cs.Goal) < 0.03f && cs.Height >= cs.Goal0 * 0.75f)
                    {
                        cs.M = Mode.Waiting; cs.Since = Time.time;
                        Plugin.Log.LogInfo("Ceiling spider: in place, waiting (highest foot at " + TopY(s).ToString("0.00") + ", ceiling at " + cs.CeilY.ToString("0.00") + ").");
                    }
                    break;
                case Mode.Waiting:
                {
                    PlayerControllerB hit = null; float best = Plugin.CeilingTriggerMeters.Value;
                    PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
                    Vector3 sp = s.transform.position;
                    for (int i = 0; i < players.Length; i++)
                    {
                        PlayerControllerB p = players[i];
                        if (p == null || !s.PlayerIsTargetable(p)) continue;
                        Vector3 pp = p.transform.position;
                        float dy = pp.y - sp.y;
                        if (dy < -1.5f || dy > cs.Height) continue;
                        float d = Vector2.Distance(new Vector2(pp.x, pp.z), new Vector2(sp.x, sp.z));
                        if (d >= best) continue;
                        Vector3 eye = p.gameplayCamera != null ? p.gameplayCamera.transform.position : pp + Vector3.up * 1.6f;
                        if (Physics.Linecast(eye, sp + Vector3.up * (cs.Height + 1f), 256, QueryTriggerInteraction.Ignore)) continue;
                        best = d; hit = p;
                    }
                    if (hit == null && cs.Tree && Plugin.TreeStalk.Value)
                    {
                        PlayerControllerB cand = null; float cd = Plugin.StalkTriggerMeters.Value;
                        Vector3 top = sp + Vector3.up * cs.Height;
                        for (int i = 0; i < players.Length; i++)
                        {
                            PlayerControllerB p = players[i];
                            if (p == null || !s.PlayerIsTargetable(p)) continue;
                            Vector3 pp = p.transform.position;
                            float d = Vector2.Distance(new Vector2(pp.x, pp.z), new Vector2(sp.x, sp.z));
                            if (d >= cd) continue;
                            float ang;
                            if (SeenBy(p, top, out ang)) continue;
                            cd = d; cand = p;
                        }
                        if (cand != null)
                        {
                            cs.Stalk = true; cs.Target = cand; cs.M = Mode.Dropping; cs.FallSpeed = 0f; cs.Since = Time.time;
                            SetHidden(s, cs, false);
                            Plugin.Log.LogInfo("Tree spider: comes down to stalk player " + cand.playerClientId + " (" + cd.ToString("0.0") + " m from the tree).");
                            break;
                        }
                    }
                    if (hit != null)
                    {
                        cs.Target = hit; cs.M = Mode.Dropping; cs.FallSpeed = 0f; cs.Since = Time.time;
                        SetHidden(s, cs, false);
                        Plugin.Log.LogInfo("Ceiling spider: drops on player " + hit.playerClientId + " (" + best.ToString("0.0") + " m beneath it).");
                    }
                    else if (Time.time - cs.Since > Plugin.CeilingRelocateSeconds.Value)
                    {
                        cs.Target = null; cs.M = Mode.Dropping; cs.FallSpeed = 0f; cs.Since = Time.time;
                        SetHidden(s, cs, false);
                        Plugin.Log.LogInfo("Ceiling spider: nobody came; coming down to find another spot.");
                    }
                    break;
                }
                case Mode.Dropping:
                    if (cs.DropFrom <= 0f) { cs.DropFrom = Mathf.Max(cs.Height, 0.01f); cs.DropRoll = cs.Roll; }
                    cs.FallSpeed += 22f * dt;
                    cs.Height = Mathf.Max(0f, cs.Height - cs.FallSpeed * dt);
                    if (cs.Height <= 0.01f)
                    {
                        cs.Height = 0f; cs.DropFrom = 0f;
                        SetHidden(s, cs, false);
                        bool stalkIt = cs.Stalk; cs.Stalk = false;
                        if (stalkIt && cs.Target != null && !cs.Target.isPlayerDead)
                        {
                            cs.StalkTarget = cs.Target; cs.StalkLogged = false; cs.Meter = 0f; cs.Frozen = false; cs.CloseSince = -1f; cs.M = Mode.Stalk;
                            Plugin.Log.LogInfo("Tree spider: down from the tree, stalking player " + cs.Target.playerClientId + ".");
                        }
                        else if (cs.Target != null && !cs.Target.isPlayerDead)
                        {
                            s.targetPlayer = cs.Target;
                            if (WatchF != null) WatchF.SetValue(s, false);
                            if (ChaseTimerF != null) ChaseTimerF.SetValue(s, 12.5f);
                            s.SwitchToBehaviourState(2);
                            cs.M = Mode.Done;
                        }
                        else { cs.M = Mode.Walk; cs.HasNode = false; }
                        cs.Since = Time.time; cs.Target = null;
                    }
                    break;
                case Mode.Stalk:
                    StalkFrame(s, cs, dt);
                    break;
                case Mode.Web:
                    WebFrame(s, cs, dt);
                    break;
                default:
                    if (cs.Height > 0f) cs.Height = Mathf.Max(0f, cs.Height - 5f * dt);
                    break;
            }
            // Roll over as it climbs (done by 3/4 of the way up), fade out between 30% and 85% of the climb.
            float frac = cs.Goal0 > 0.01f ? Mathf.Clamp01(cs.Height / (cs.Goal0 * (cs.Tree ? 0.2f : 0.75f))) : 0f;
            cs.Roll = 180f * Mathf.SmoothStep(0f, 1f, frac);
            if (cs.M == Mode.Dropping && cs.DropFrom > 0.01f)
            {
                float pr = Mathf.Clamp01((1f - cs.Height / cs.DropFrom) / 0.25f);
                cs.Roll = cs.DropRoll * (1f - Mathf.SmoothStep(0f, 1f, pr));
            }
            float fadeFrac = cs.Goal0 > 0.01f ? Mathf.Clamp01((cs.Height / cs.Goal0 - 0.3f) / 0.55f) : 0f;
            float endOpacity = Mathf.Clamp01(Plugin.WaitingOpacityPercent.Value / 100f);
            cs.Opacity = Plugin.CeilingCloak.Value ? 1f - fadeFrac * (1f - endOpacity) : 1f;
            if (cs.Height > 0.001f) cs.FadeActive = true;
            {
                bool cloak = Plugin.TreeCloak.Value && (cs.Tree || cs.Roam) && (cs.M == Mode.Waiting || cs.M == Mode.Stalk);
                if (cloak)
                {
                    Vector3 spot = cs.M == Mode.Stalk || s.meshContainer == null ? s.transform.position + Vector3.up * 0.7f : s.meshContainer.position;
                    bool facing = false;
                    Vector3 vel = (s.agent != null && s.agent.enabled) ? s.agent.velocity : Vector3.zero;
                    vel.y = 0f;
                    Vector3 ahead = spot + vel * Plugin.TreeCloakStepLead.Value;
                    PlayerControllerB[] cps = StartOfRound.Instance.allPlayerScripts;
                    for (int i = 0; i < cps.Length; i++)
                    {
                        PlayerControllerB cp = cps[i];
                        if (cp == null || !s.PlayerIsTargetable(cp) || cp.gameplayCamera == null) continue;
                        if (CloakWithin(cs, cp, spot, ahead)) facing = true;
                    }
                    float end = Mathf.Clamp01(Plugin.TreeCloakOpacityPercent.Value / 100f);
                    float want = facing ? end : 1f;
                    cs.CloakOp = Mathf.MoveTowards(cs.CloakOp, want, (facing ? 15f : 1.5f) * dt);
                    cs.Opacity = cs.CloakOp;
                    cs.FadeActive = true; cs.Cloaked = true;
                }
                else if (cs.Cloaked)
                {
                    cs.CloakOp = Mathf.MoveTowards(cs.CloakOp, 1f, 6f * dt);
                    cs.Opacity = cs.CloakOp;
                    cs.FadeActive = true;
                    if (cs.CloakOp >= 0.99f) { cs.CloakOp = 1f; cs.Opacity = 1f; cs.Cloaked = false; }
                }
            }
            if (cs.Height > 0f && s.meshContainer != null)
            {
                Vector3 p = s.meshContainer.position;
                if (cs.Tree)
                {
                    if (cs.M != Mode.Dropping) UpdateWall(s, cs);
                    ApplyTrunk(s, cs, cs.Roll / 180f, p);
                }
                else
                {
                    s.meshContainer.position = new Vector3(p.x, p.y + cs.Height, p.z);
                    ApplyRoll(s, cs.Roll);
                }
                if (cs.Tree) { LegsOnTrunk(s, cs, cs.Roll / 180f); FitTrunk(s, cs); }
                else LegsFollow(s);
                if (cs.M == Mode.Rising && cs.CeilY > -1000f && cs.Goal0 > 0.01f && cs.Height >= cs.Goal0 * 0.75f)
                {
                    float top = TopY(s);
                    if (top > -1000f)
                    {
                        float err = (cs.CeilY - Plugin.CeilingGap.Value) - top;
                        cs.Goal = Mathf.Clamp(cs.Height + err, 0.5f, cs.Goal0 + 3f);
                    }
                }
            }
            if (cs.Height > 0.001f && Plugin.DebugLogging.Value && Time.time >= cs.NextLog)
            {
                cs.NextLog = Time.time + 1f;
                LegDebug(s, cs);
            }
        }

        private sealed class RollHolder { public Quaternion Pre; public bool Has; }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, RollHolder> Rolls
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, RollHolder>();

        // Rolls the model over about its middle. The game's own turning code starts from the model's current
        // rotation every frame, so the roll is taken off again (UndoRoll) before the game runs.
        internal static void ApplyRoll(SandSpiderAI s, float angle)
        {
            Transform mc = s.meshContainer;
            if (mc == null || angle < 0.5f) return;
            RollHolder h = Rolls.GetOrCreateValue(s);
            h.Pre = mc.rotation; h.Has = true;
            mc.rotation = Quaternion.Euler(0f, mc.eulerAngles.y, 0f); // start from level, so 180 is exactly upside down
            mc.RotateAround(mc.position + mc.up * 1f, mc.forward, angle);
        }

        internal static void UndoRoll(SandSpiderAI s)
        {
            RollHolder h;
            if (s == null || !Rolls.TryGetValue(s, out h) || !h.Has) return;
            h.Has = false;
            if (s.meshContainer != null) s.meshContainer.rotation = h.Pre;
        }

        // Is the spot inside (or about to be inside) this player's view cone for the dissolve? Looks ahead by TreeCloakLead
        // seconds using how fast the angle to the spider has been shrinking, and needs a clear line to the spot.
        private static bool CloakWithin(CS cs, PlayerControllerB p, Vector3 spot, Vector3 ahead)
        {
            Transform cam = p.gameplayCamera.transform;
            Vector3 to = spot - cam.position;
            float ang = to.sqrMagnitude < 0.04f ? 0f : Vector3.Angle(cam.forward, to);
            int key = (int)p.playerClientId;
            float rate = 0f; Vector2 prev;
            float now = Time.time;
            if (cs.PrevAng.TryGetValue(key, out prev) && now > prev.y && now - prev.y < 0.2f)
                rate = Mathf.Clamp((prev.x - ang) / (now - prev.y), 0f, 720f);
            cs.PrevAng[key] = new Vector2(ang, now);
            int mask = StartOfRound.Instance.collidersAndRoomMaskAndDefault | 33554432;
            float lead = Plugin.TreeCloakLead.Value, max = Plugin.TreeCloakAngle.Value;
            // where the spider is now
            if (Mathf.Min(ang, ang - rate * lead) <= max && !Physics.Linecast(cam.position, spot, mask, QueryTriggerInteraction.Ignore)) return true;
            // where it is about to be (round a corner, out from behind something)
            Vector3 d2 = ahead - spot;
            if (d2.sqrMagnitude > 0.04f)
            {
                Vector3 to2 = ahead - cam.position;
                float ang2 = to2.sqrMagnitude < 0.04f ? 0f : Vector3.Angle(cam.forward, to2);
                if (Mathf.Min(ang2, ang2 - rate * lead) <= max && !Physics.Linecast(cam.position, ahead, mask, QueryTriggerInteraction.Ignore)) return true;
            }
            return false;
        }

        // Can this player see the spot (inside about 67 degrees of where they look, with nothing solid in between)?
        internal static bool SeenBy(PlayerControllerB p, Vector3 spot, out float angle)
        {
            angle = 180f;
            if (p == null || p.gameplayCamera == null) return false;
            Transform cam = p.gameplayCamera.transform;
            Vector3 to = spot - cam.position;
            if (to.sqrMagnitude < 0.04f) { angle = 0f; return true; }
            angle = Vector3.Angle(cam.forward, to);
            if (angle > 67f) return false;
            int mask = StartOfRound.Instance.collidersAndRoomMaskAndDefault | 33554432;
            return !Physics.Linecast(cam.position, spot, mask, QueryTriggerInteraction.Ignore);
        }

        private static bool WebClear(SandSpiderAI s, PlayerControllerB tg)
        {
            if (tg == null || tg.gameplayCamera == null) return false;
            RaycastHit bh;
            return !WebBlocker(s, SpiderWeb.Mouth(s, tg.transform.position), tg.gameplayCamera.transform.position - Vector3.up * 0.3f, out bh);
        }

        // Like a linecast, but ignores the spider's own body (the line starts inside it) and anything within 0.6 m of the mouth.
        private static bool WebBlocker(SandSpiderAI s, Vector3 from, Vector3 to, out RaycastHit hit)
        {
            int mask = StartOfRound.Instance.collidersAndRoomMaskAndDefault | 33554432;
            Vector3 d = to - from; float len = d.magnitude;
            hit = default(RaycastHit);
            if (len < 0.01f) return false;
            // Every layer, so doors count too (a closed door is not always on a "wall" layer); living things and items never block.
            RaycastHit[] hs = Physics.RaycastAll(from, d / len, len, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue; bool any = false;
            for (int i = 0; i < hs.Length; i++)
            {
                Collider c = hs[i].collider;
                if (c == null || hs[i].distance < 0.6f) continue;
                if (c.GetComponentInParent<SandSpiderAI>() != null || c.GetComponentInParent<PlayerControllerB>() != null
                    || c.GetComponentInParent<EnemyAI>() != null || c.GetComponentInParent<GrabbableObject>() != null
                    || c.GetComponentInParent<DeadBodyInfo>() != null) continue;
                bool solid = ((1 << c.gameObject.layer) & mask) != 0;
                if (!solid && DoorType != null && c.GetComponentInParent(DoorType) != null) solid = true;
                if (!solid) continue;
                if (hs[i].distance < best) { best = hs[i].distance; hit = hs[i]; any = true; }
            }
            return any;
        }

        private static readonly Type DoorType = AccessTools.TypeByName("DoorLock");

        // Before shooting: the line from the spider's mouth AND a straight line from its body to the player's body must both be free of
        // walls and doors.
        private static bool WebShotClear(SandSpiderAI s, PlayerControllerB tg)
        {
            if (!WebClear(s, tg)) return false;
            RaycastHit bh;
            if (WebBlocker(s, s.transform.position + Vector3.up * 0.8f, tg.transform.position + Vector3.up * 0.9f, out bh)) return false;
            if (WebBlocker(s, s.transform.position + Vector3.up * 0.8f, tg.gameplayCamera.transform.position, out bh)) return false;
            return true;
        }

        private static void BeginWeb(SandSpiderAI s, CS cs, PlayerControllerB tg)
        {
            cs.M = Mode.Web; cs.WebT = 0f; cs.WebBlocked = 0f; cs.Frozen = false; cs.Since = Time.time;
            s.destination = s.transform.position;
            SpiderWeb.Start(s, tg);
            Plugin.Log.LogInfo("Spider web: shoots a web line at player " + tg.playerClientId + " from " + Vector3.Distance(s.transform.position, tg.transform.position).ToString("0.0") + " m.");
        }

        // The spider stands and reels. Ends in the normal chase (bite) when you arrive, or lets go if you break the line of sight.
        private static void WebFrame(SandSpiderAI s, CS cs, float dt)
        {
            PlayerControllerB tg = cs.StalkTarget;
            if (tg == null || tg.isPlayerDead || !s.PlayerIsTargetable(tg))
            {
                SpiderWeb.Stop(s);
                cs.M = Mode.Walk; cs.HasNode = false; cs.Since = Time.time - 10f;
                return;
            }
            cs.WebT += dt;
            Vector3 tp = tg.transform.position, sp = s.transform.position;
            if (LookAtF != null && s.meshContainer != null)
                LookAtF.Invoke(s, new object[] { new Vector3(tp.x, s.meshContainer.position.y, tp.z) });
            float dist = Vector2.Distance(new Vector2(tp.x, tp.z), new Vector2(sp.x, sp.z));
            if (dist < 2.4f)
            {
                SpiderWeb.Stop(s);
                s.targetPlayer = tg;
                if (WatchF != null) WatchF.SetValue(s, false);
                if (ChaseTimerF != null) ChaseTimerF.SetValue(s, 12.5f);
                s.SwitchToBehaviourState(2);
                cs.M = Mode.Done; cs.Since = Time.time;
                Plugin.Log.LogInfo("Spider web: reeled player " + tg.playerClientId + " in; the spider moves to bite.");
                return;
            }
            if (WebClear(s, tg)) cs.WebBlocked = 0f;
            else
            {
                if (cs.WebBlocked <= 0f)
                {
                    RaycastHit bh;
                    Vector3 from = SpiderWeb.Mouth(s, tp);
                    Vector3 to = tg.gameplayCamera.transform.position - Vector3.up * 0.3f;
                    if (WebBlocker(s, from, to, out bh))
                        Plugin.Log.LogInfo("Spider web: line blocked by '" + bh.collider.name + "' (layer " + bh.collider.gameObject.layer + ", " + bh.distance.ToString("0.00") + " m from the mouth; mouth " + from + ", player " + to + ").");
                }
                cs.WebBlocked += dt;
            }
            if (Time.time >= cs.NextLog)
            {
                cs.NextLog = Time.time + 0.5f;
                if (Plugin.VerboseLogs.Value) Plugin.Log.LogInfo("Spider web: reeling, distance " + dist.ToString("0.0") + " m after " + cs.WebT.ToString("0.0") + " s, blocked " + cs.WebBlocked.ToString("0.00") + " s.");
            }
            if (dist > Plugin.WebBreakMeters.Value)
            {
                SpiderWeb.Stop(s);
                cs.M = Mode.Walk; cs.HasNode = false; cs.Since = Time.time - 10f;
                Plugin.Log.LogInfo("Spider web: the line snapped; the player stretched it to " + dist.ToString("0.0") + " m.");
                return;
            }
            if (cs.WebBlocked > 0f || cs.WebT > 7f)
            {
                SpiderWeb.Stop(s);
                cs.M = Mode.Walk; cs.HasNode = false; cs.Since = Time.time - 10f;
                Plugin.Log.LogInfo("Spider web: the line was broken (" + (cs.WebBlocked > 0f ? "blocked" : "too long") + "); the spider lets go.");
            }
        }

        // Feiopar-style stalk, once per frame on the host: frozen while anyone looks at it, creeping when nobody does.
        private static void StalkFrame(SandSpiderAI s, CS cs, float dt)
        {
            PlayerControllerB tg = cs.StalkTarget;
            if (tg == null || tg.isPlayerDead || !s.PlayerIsTargetable(tg))
            {
                Plugin.Log.LogInfo("Stalk: target lost (null " + (tg == null) + ", dead " + (tg != null && tg.isPlayerDead) + ", targetable " + (tg != null && s.PlayerIsTargetable(tg)) + ").");
                cs.M = Mode.Walk; cs.HasNode = false; cs.Frozen = false; return;
            }
            if (!cs.StalkLogged) { cs.StalkLogged = true; Plugin.Verbose("Stalk: first frame of the stalk."); }
            Vector3 sp = s.transform.position;
            Vector3 spot = sp + Vector3.up * 0.7f;
            bool seen = false; float seenAngle = 180f;
            PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
            for (int i = 0; i < players.Length; i++)
            {
                PlayerControllerB p = players[i];
                if (p == null || !s.PlayerIsTargetable(p)) continue;
                float a;
                if (SeenBy(p, spot, out a)) { seen = true; seenAngle = Mathf.Min(seenAngle, a); }
            }
            Vector3 tp = tg.transform.position;
            float dist = Vector3.Distance(new Vector3(tp.x, sp.y, tp.z), sp);
            cs.Frozen = seen;
            if (seen) cs.Meter += dt * (dist < 8f ? 2f : 1f);
            else cs.Meter = Mathf.Max(0f, cs.Meter - dt * 0.5f);
            if (!seen && dist < 6f) { if (cs.CloseSince < 0f) cs.CloseSince = Time.time; } else cs.CloseSince = -1f;

            if (Time.time >= cs.NextLog)
            {
                cs.NextLog = Time.time + 1f;
                UnityEngine.AI.NavMeshAgent ag = s.agent;
                if (Plugin.VerboseLogs.Value) Plugin.Log.LogInfo("Stalk: dist " + dist.ToString("0.0") + " seen=" + seen + " angle " + seenAngle.ToString("0") + " meter " + cs.Meter.ToString("0.0")
                    + " agentSpeed " + (ag != null ? ag.speed.ToString("0.0") : "-") + " vel " + (ag != null ? ag.velocity.magnitude.ToString("0.0") : "-")
                    + " stopped " + (ag != null && ag.isStopped) + " enabled " + (ag != null && ag.enabled) + " onMesh " + (ag != null && ag.isOnNavMesh)
                    + " path " + (ag != null && ag.enabled && ag.isOnNavMesh ? ag.pathStatus.ToString() : "-") + " special " + s.inSpecialAnimation
                    + " spiderSpeed " + s.spiderSpeed.ToString("0.0"));
            }
            if (Plugin.WebPull.Value && (!seen || Plugin.WebEvenIfWatched.Value) && dist <= Plugin.WebRange.Value && dist > 3.2f && WebShotClear(s, tg))
            {
                BeginWeb(s, cs, tg);
                return;
            }
            bool charge = (!seen && dist < 2.5f) || (seen && dist < 3.5f && seenAngle < 25f)
                || (cs.CloseSince > 0f && Time.time - cs.CloseSince > 1.5f);
            if (charge)
            {
                s.targetPlayer = tg;
                if (WatchF != null) WatchF.SetValue(s, false);
                if (ChaseTimerF != null) ChaseTimerF.SetValue(s, 12.5f);
                s.SwitchToBehaviourState(2);
                cs.M = Mode.Done; cs.Frozen = false; cs.Since = Time.time;
                Plugin.Log.LogInfo("Tree spider: charges player " + tg.playerClientId + " from " + dist.ToString("0.0") + " m.");
                return;
            }
            if (cs.Meter > 16f)
            {
                cs.M = Mode.Walk; cs.HasNode = false; cs.Frozen = false; cs.Since = Time.time;
                Plugin.Log.LogInfo("Tree spider: was watched too long; gives up the stalk and goes back to a tree.");
                return;
            }
            if (seen && LookAtF != null && s.meshContainer != null)
                LookAtF.Invoke(s, new object[] { new Vector3(tp.x, s.meshContainer.position.y, tp.z) });
        }

        // Runs after the game's own Update (which resets the speed every frame in this state).
        internal static void StalkSpeed(SandSpiderAI s)
        {
            if (s == null || !s.IsOwner || s.isEnemyDead || s.currentBehaviourStateIndex != 0) return;
            CS cs = Get(s, false);
            if (cs == null || (cs.M != Mode.Stalk && cs.M != Mode.Web) || cs.StalkTarget == null) return;
            Vector3 sp = s.transform.position, tp = cs.StalkTarget.transform.position;
            float d = Vector2.Distance(new Vector2(sp.x, sp.z), new Vector2(tp.x, tp.z));
            float v = (cs.Frozen || cs.M == Mode.Web) ? 0f : (d > 18f ? 7f : d > 10f ? 5f : d > 5f ? 4f : 3.2f);
            if (s.agent != null) s.agent.speed = v;
            s.spiderSpeed = v;
        }

        // 1 normally; the quiet fraction while this spider stalks.
        internal static float StalkQuiet(SandSpiderAI s)
        {
            CS cs = Get(s, false);
            return (cs != null && cs.M == Mode.Stalk) ? Mathf.Clamp01(Plugin.StalkFootstepPercent.Value / 100f) : 1f;
        }

        // Just before the game moves the model each frame: while it is up there, its walking target is where it is.
        internal static void PinModel(SandSpiderAI s)
        {
            if (s == null || !s.IsOwner) return;
            CS cs = Get(s, false);
            if (cs != null && (cs.M == Mode.Rising || cs.M == Mode.Waiting || cs.M == Mode.Dropping))
                s.meshContainerTarget = s.meshContainerPosition;
            // Walking to a tree outdoors: the visible body has no path of its own (the game's path for it fails on slopes and
            // it falls behind, then jumps to the tree). It simply follows the real spider, a little faster so it keeps up.
            if (cs != null && (cs.Tree || cs.Roam) && (cs.M == Mode.Walk || cs.M == Mode.Stalk || cs.M == Mode.Web) && s.meshContainer != null)
            {
                s.meshContainerTarget = s.transform.position;
                if (s.spiderSpeed > 0.01f)
                {
                    // The body must always be faster than the real spider (also during a web-alert burst of speed), otherwise it
                    // falls behind and then cuts straight across to it, through walls.
                    float ghost = (s.agent != null && s.agent.enabled) ? Mathf.Max(s.agent.speed, s.agent.velocity.magnitude) : 0f;
                    // Steady pace: the body moves as fast as the real spider does, plus a little more for each metre it is behind, so
                    // it always catches up but never rushes ahead and stops (those stop-and-go jumps made a joiner's legs jitter).
                    Vector3 gap = s.transform.position - s.meshContainerPosition; gap.y = 0f;
                    float body = Mathf.Max(ghost, cs.M == Mode.Stalk ? Mathf.Min(s.spiderSpeed, ghost + 0.5f) : 0f) + gap.magnitude * 3f;
                    s.spiderSpeed = Mathf.Max(body, 0.6f);
                }
                Vector3 v = (s.agent != null && s.agent.enabled) ? s.agent.desiredVelocity : Vector3.zero;
                v.y = 0f;
                if (v.sqrMagnitude > 0.04f && LookAtF != null)
                    LookAtF.Invoke(s, new object[] { s.meshContainer.position + v.normalized * 2f });
            }
        }

        internal static Quaternion PreRotation(SandSpiderAI s, Quaternion fallback)
        {
            RollHolder h;
            return (Rolls.TryGetValue(s, out h) && h.Has) ? h.Pre : fallback;
        }

        // The game moves the feet before the body is raised, so put them back on the body.
        internal static void LegsFollow(SandSpiderAI s)
        {
            try
            {
                if (s.legTargets == null || s.legDefaultPositions == null) return;
                for (int i = 0; i < s.legTargets.Length && i < s.legDefaultPositions.Length; i++)
                    if (s.legTargets[i] != null && s.legDefaultPositions[i] != null) s.legTargets[i].position = s.legDefaultPositions[i].position;
            }
            catch (Exception) { }
        }

        // For the see-through effect: how opaque the spider must be right now, or -1 to leave it to the normal fade.
        internal static float ForcedOpacity(SandSpiderAI s)
        {
            if (s == null) return -1f;
            if (s.IsOwner)
            {
                CS cs = Get(s, false);
                if (cs == null || !cs.FadeActive) return -1f;
                if (cs.Height <= 0.001f && !cs.Cloaked) cs.FadeActive = false;
                return cs.Opacity;
            }
            Unity.Netcode.NetworkObject no = s.NetworkObject;
            Trapdoor.RemoteInfo r;
            if (no == null || !Trapdoor.Remote.TryGetValue(no.NetworkObjectId, out r) || !r.Ceil) return -1f;
            return r.OpCur;
        }

        // Height of the highest foot right now (after raising, rolling and moving the feet), or a huge negative number.
        private static float TopY(SandSpiderAI s)
        {
            float top = -99999f;
            try
            {
                if (s.legTargets != null)
                    foreach (Transform lt in s.legTargets) if (lt != null && lt.position.y > top) top = lt.position.y;
            }
            catch (Exception) { }
            return top;
        }

        // DebugLogging only: where the legs are while the body is up, and which way up the body is.
        private static void LegDebug(SandSpiderAI s, CS cs)
        {
            try
            {
                Transform mc = s.meshContainer;
                Vector3 body = mc.position;
                int n = s.legTargets != null ? s.legTargets.Length : 0;
                float minDy = float.MaxValue, maxDy = float.MinValue, sumDy = 0f, maxReach = 0f, sumLag = 0f;
                int counted = 0, offFloor = 0, behind = 0;
                for (int i = 0; i < n; i++)
                {
                    Transform lt = s.legTargets[i];
                    if (lt == null) continue;
                    Vector3 lp = lt.position;
                    float dy = lp.y - body.y;
                    minDy = Mathf.Min(minDy, dy); maxDy = Mathf.Max(maxDy, dy); sumDy += dy;
                    maxReach = Mathf.Max(maxReach, Vector3.Distance(lp, body));
                    if (s.legDefaultPositions != null && i < s.legDefaultPositions.Length && s.legDefaultPositions[i] != null)
                        sumLag += Vector3.Distance(lp, s.legDefaultPositions[i].position);
                    if (dy < -(cs.Height * 0.5f)) offFloor++;
                    counted++;
                }
                float up = Vector3.Dot(mc.up, Vector3.up);
                if (cs.Tree)
                    Plugin.Dbg("Tree debug [" + cs.M + "]: pivot stand-off " + cs.Stand.ToString("0.00") + " m, feet stand " + FootDot(s, cs).ToString("0.00")
                        + " m off the trunk, trunk normal " + cs.WallN + ", body forward-dot-up " + Vector3.Dot(mc.forward, Vector3.up).ToString("0.00") + " (1 = head straight up).");
                Plugin.Dbg("Ceiling debug [" + cs.M + "]: height " + cs.Height.ToString("0.00") + " m, body y " + body.y.ToString("0.00")
                    + ", spider y " + s.transform.position.y.ToString("0.00") + ", body up-dot " + up.ToString("0.00")
                    + " (1 = upright, -1 = upside down), hidden renderers " + cs.Hidden.Count);
                if (counted > 0)
                    Plugin.Dbg("Ceiling debug legs: " + counted + " legs, foot height rel. body min/avg/max "
                        + minDy.ToString("0.00") + "/" + (sumDy / counted).ToString("0.00") + "/" + maxDy.ToString("0.00")
                        + " m, longest reach " + maxReach.ToString("0.00") + " m, avg distance from where the body wants them "
                        + (sumLag / counted).ToString("0.00") + " m, " + offFloor + " of " + counted + " still down near the floor");
            }
            catch (Exception e) { Plugin.Dbg("Ceiling leg debug failed: " + e.Message); }
        }
    }

    // The game's own position sync must send the surface position, never the lowered one (the depth has its own message).
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.SyncMeshContainerPositionToClients))]
    internal static class MeshSyncSurfacePatch
    {
        internal sealed class St { public bool Restore; public Vector3 Pos; public Quaternion Rot; public float Off; }

        private static void Prefix(SandSpiderAI __instance, out St __state)
        {
            __state = null;
            try
            {
                Transform mc = __instance.meshContainer;
                if (mc == null) return;
                Trapdoor.TD td = Trapdoor.Get(__instance, false);
                Ceiling.CS cs = Ceiling.Get(__instance, false);
                bool ceil = cs != null && cs.Height > 0f;
                float off = td != null ? td.Depth : 0f;
                if (!ceil && off <= 0f) return;
                __state = new St();
                if (ceil)
                {
                    __state.Restore = true; __state.Pos = mc.position; __state.Rot = mc.rotation;
                    mc.position = __instance.meshContainerPosition;
                    mc.rotation = Ceiling.PreRotation(__instance, mc.rotation);
                }
                else
                {
                    __state.Off = off;
                    Vector3 p = mc.position;
                    mc.position = new Vector3(p.x, p.y + off, p.z);
                }
            }
            catch (Exception) { }
        }

        private static void Postfix(SandSpiderAI __instance, St __state)
        {
            try
            {
                if (__state == null || __instance.meshContainer == null) return;
                Transform mc = __instance.meshContainer;
                if (__state.Restore) { mc.position = __state.Pos; mc.rotation = __state.Rot; }
                else { Vector3 p = mc.position; mc.position = new Vector3(p.x, p.y - __state.Off, p.z); }
            }
            catch (Exception) { }
        }
    }

    // Takes the roll off the model before the game's own per-frame turning runs (see Ceiling.ApplyRoll).
    [HarmonyPatch(typeof(SandSpiderAI), "LateUpdate")]
    internal static class CeilingUndoRollPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(SandSpiderAI __instance) { try { Ceiling.UndoRoll(__instance); Ceiling.PinModel(__instance); } catch (Exception) { } }
    }

    // The owner tells every other game how deep each spider is and whether it is hidden.
    internal static class DepthSync
    {
        internal const string Name = "SpiderCocoon.Depth";
        private static float next;
        private static readonly HashSet<ulong> Active = new HashSet<ulong>();
        private static float nextLog;

        internal static void Tick()
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || nm.CustomMessagingManager == null || !CocoonNet.Ready) return;
            if (Time.time < next) return;
            next = Time.time + 0.1f;
            if (nm.ConnectedClientsIds.Count < 2) return;
            List<ulong> ids = new List<ulong>(); List<float> depths = new List<float>(); List<bool> hid = new List<bool>();
            List<float> rolls = new List<float>(); List<float> ops = new List<float>();
            foreach (Trapdoor.TD td in Trapdoor.AllStates())
            {
                SandSpiderAI s = td.Spider;
                if (s == null || !s.IsOwner || s.NetworkObject == null || !s.NetworkObject.IsSpawned) continue;
                ulong id = s.NetworkObject.NetworkObjectId;
                if (td.Depth > 0.001f) { Active.Add(id); }
                else if (!Active.Remove(id)) continue; // one last zero when it has surfaced
                ids.Add(id); depths.Add(td.Depth); hid.Add(td.Hidden.Count > 0); rolls.Add(0f); ops.Add(1f);
                if (Time.time >= nextLog) { nextLog = Time.time + 1f; Plugin.Log.LogInfo("Depth message sent: spider " + id + " depth " + td.Depth.ToString("F2") + ", hidden " + (td.Hidden.Count > 0)); }
            }
            foreach (Ceiling.CS cs in Ceiling.AllStates())
            {
                SandSpiderAI s = cs.Spider;
                if (s == null || !s.IsOwner || s.NetworkObject == null || !s.NetworkObject.IsSpawned) continue;
                ulong id = s.NetworkObject.NetworkObjectId;
                if (cs.Height > 0.001f) { Active.Add(id); }
                else if (!Active.Remove(id)) continue;
                ids.Add(id); depths.Add(-cs.Height); hid.Add(cs.Hidden.Count > 0); rolls.Add(cs.Roll); ops.Add(cs.Opacity);
            }
            if (ids.Count == 0) return;
            using (Unity.Netcode.FastBufferWriter w = new Unity.Netcode.FastBufferWriter(16 + 32 * ids.Count, Unity.Collections.Allocator.Temp))
            {
                w.WriteValueSafe(ids.Count);
                for (int i = 0; i < ids.Count; i++) { w.WriteValueSafe(ids[i]); w.WriteValueSafe(depths[i]); w.WriteValueSafe(hid[i]); w.WriteValueSafe(rolls[i]); w.WriteValueSafe(ops[i]); }
                nm.CustomMessagingManager.SendNamedMessageToAll(Name, w);
            }
        }

        internal static void OnMessage(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                int n;
                reader.ReadValueSafe(out n);
                for (int i = 0; i < n; i++)
                {
                    ulong id; float depth; bool hidden; float roll; float op;
                    reader.ReadValueSafe(out id); reader.ReadValueSafe(out depth); reader.ReadValueSafe(out hidden);
                    reader.ReadValueSafe(out roll); reader.ReadValueSafe(out op);
                    Trapdoor.RemoteInfo r;
                    if (!Trapdoor.Remote.TryGetValue(id, out r)) { r = new Trapdoor.RemoteInfo(); Trapdoor.Remote[id] = r; }
                    if (Mathf.Abs(r.Target - depth) > 0.05f && (Mathf.Abs(depth) <= 0.001f || Mathf.Abs(r.Target) <= 0.001f))
                        Plugin.Log.LogInfo("Depth message received: spider " + id + " depth " + depth.ToString("F2") + ", hidden " + hidden);
                    r.Target = depth; r.Hidden = hidden; r.Stamp = Time.time; r.RollTarget = roll; r.OpTarget = op;
                }
            }
            catch (Exception e) { Plugin.Log.LogError("Depth sync failed: " + e); }
        }
    }

    // The spawn key marks a spider as outdoors on the host's game only. Every other game must agree, or the game's own
    // bite check (which compares the player's side with the spider's flag) never lets that spider hurt a joiner.
    // The host says which spiders are outdoors (OutdoorSync); this patch is the backup, judging by position.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderSideSyncPatch
    {
        private static readonly HashSet<int> Reported = new HashSet<int>();

        private static void Prefix(SandSpiderAI __instance)
        {
            try
            {
                if (__instance == null || __instance.isOutside || __instance.isEnemyDead) return;
                if (!Cocoon.SpiderOutdoors(__instance))
                {
                    if (Reported.Add(__instance.GetInstanceID()) && RoundManager.Instance != null)
                    {
                        Vector3 pos = __instance.transform.position;
                        Plugin.Verbose("Spider side check: at " + pos + " this spider counts as indoors (nearest inside node "
                            + Cocoon.NearestNodeDistance(RoundManager.Instance.insideAINodes, pos).ToString("F0") + " m, nearest outside node "
                            + Cocoon.NearestNodeDistance(RoundManager.Instance.outsideAINodes, pos).ToString("F0") + " m).");
                    }
                    return;
                }
                __instance.isOutside = true;
                Plugin.Log.LogInfo("Spider side sync: this spider stands outdoors, so it is now treated as an outdoor monster on this game too.");
            }
            catch (Exception) { }
        }
    }

    // The host tells every other game which spiders are outdoors, so their bite check treats those spiders correctly.
    internal static class OutdoorSync
    {
        internal const string Name = "SpiderCocoon.Outdoor";
        private static float next;

        internal static void Tick()
        {
            Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || !nm.IsServer || nm.CustomMessagingManager == null || !CocoonNet.Ready) return;
            if (Time.time < next) return;
            next = Time.time + 2f;
            if (nm.ConnectedClientsIds.Count < 2) return; // nobody else to tell
            List<ulong> ids = new List<ulong>();
            foreach (SandSpiderAI s in UnityEngine.Object.FindObjectsOfType<SandSpiderAI>())
            {
                if (s == null || s.isEnemyDead || !s.isOutside) continue;
                Unity.Netcode.NetworkObject no = s.NetworkObject;
                if (no != null && no.IsSpawned) ids.Add(no.NetworkObjectId);
            }
            if (ids.Count == 0) return;
            using (Unity.Netcode.FastBufferWriter w = new Unity.Netcode.FastBufferWriter(16 + 8 * ids.Count, Unity.Collections.Allocator.Temp))
            {
                w.WriteValueSafe(ids.Count);
                foreach (ulong id in ids) w.WriteValueSafe(id);
                nm.CustomMessagingManager.SendNamedMessageToAll(Name, w);
            }
        }

        internal static void OnMessage(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || nm.IsServer || nm.SpawnManager == null) return;
                int n;
                reader.ReadValueSafe(out n);
                for (int i = 0; i < n; i++)
                {
                    ulong id;
                    reader.ReadValueSafe(out id);
                    Unity.Netcode.NetworkObject no;
                    if (!nm.SpawnManager.SpawnedObjects.TryGetValue(id, out no) || no == null) continue;
                    SandSpiderAI s = no.GetComponent<SandSpiderAI>();
                    if (s != null && !s.isOutside)
                    {
                        s.isOutside = true;
                        Plugin.Log.LogInfo("Outdoor sync: the host says this spider is outdoors, so it is now treated as an outdoor monster on this game too.");
                    }
                }
            }
            catch (Exception e) { Plugin.Log.LogError("Outdoor sync failed: " + e); }
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.DoAIInterval))]
    internal static class TrapdoorIntervalPatch
    {
        [HarmonyPriority(Priority.First)]
        private static bool Prefix(SandSpiderAI __instance)
        {
            try { return Trapdoor.Interval(__instance) && Ceiling.Interval(__instance); }
            catch (Exception e) { Plugin.Log.LogError("Trapdoor interval failed: " + e); return true; }
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), "LateUpdate")]
    internal static class TrapdoorFramePatch
    {
        private static void Postfix(SandSpiderAI __instance)
        {
            try { Trapdoor.Frame(__instance); Ceiling.Frame(__instance); SpiderWeb.Tick(__instance); Ceiling.Diag(__instance); }
            catch (Exception e) { Plugin.Log.LogError("Trapdoor frame failed: " + e); }
        }
    }

    // A hiding spider must not notice players from afar the way the game's patrol does...
    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.CheckLineOfSightForPosition))]
    internal static class TrapdoorNoSpottingPatch
    {
        private static bool Prefix(EnemyAI __instance, ref bool __result)
        {
            SandSpiderAI sp = __instance as SandSpiderAI;
            if (sp != null && (Trapdoor.Ambushing(sp) || Ceiling.Ambushing(sp) || Ceiling.Stalker(sp))) { __result = false; return false; }
            return true;
        }
    }

    // Hides the advanced and testing settings from the in-game LethalConfig menu (they stay in the .cfg file). LethalConfig is an
    // optional extra: when it is not installed, none of this runs and its code is never loaded.
    internal static class AdvancedHider
    {
        internal static void Apply(ConfigFile cfg)
        {
            try
            {
                if (Plugin.ShowAdvancedInMenu != null && Plugin.ShowAdvancedInMenu.Value) return;
                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig")) return;
                Hide(cfg);
            }
            catch (Exception e) { Plugin.Log.LogWarning("Could not hide advanced settings from the LethalConfig menu: " + e.Message); }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void Hide(ConfigFile cfg)
        {
            int hidden = 0, shown = 0;
            foreach (ConfigDefinition def in cfg.Keys)
            {
                ConfigEntryBase e = cfg[def];
                string d = e.Description != null ? e.Description.Description : "";
                if (def.Section == "Testing" || d.StartsWith("Advanced. ") || d.Contains("Testing only")) { LethalConfig.LethalConfigManager.SkipAutoGenFor(e); hidden++; }
                else shown++;
            }
            Plugin.Log.LogInfo("LethalConfig menu: " + shown + " settings shown, " + hidden + " advanced/testing settings hidden (set ShowAdvancedInMenu = true to show them).");
        }
    }

    // Keeps a spider's webs apart: a new web too close to one it already laid is simply not made.
    internal static class WebSpacing
    {
        internal sealed class Laid { public List<Vector3[]> Strands = new List<Vector3[]>(); }
        internal static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Laid> Table
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Laid>();

        internal static bool TooClose(SandSpiderAI s, Vector3 a, Vector3 b)
        {
            float min = Plugin.WebMinSpacing.Value;
            if (min <= 0.01f || s == null) return false;
            Laid l;
            if (!Table.TryGetValue(s, out l)) return false;
            Vector3 mid = (a + b) * 0.5f;
            for (int i = 0; i < l.Strands.Count; i++)
            {
                Vector3 oa = l.Strands[i][0], ob = l.Strands[i][1], om = (oa + ob) * 0.5f;
                if (Vector3.Distance(mid, om) < min) return true;
                float endMin = min * 0.5f;
                if (Vector3.Distance(a, oa) < endMin || Vector3.Distance(a, ob) < endMin || Vector3.Distance(b, oa) < endMin || Vector3.Distance(b, ob) < endMin) return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.SpawnWebTrapServerRpc))]
    internal static class WebSpacingPatch
    {
        private static bool Prefix(SandSpiderAI __instance, Vector3 startPosition, Vector3 endPosition)
        {
            try { if (__instance != null && __instance.IsOwner && WebSpacing.TooClose(__instance, startPosition, endPosition)) return false; }
            catch (Exception) { }
            return true;
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.SpawnWebTrapClientRpc))]
    internal static class WebSpacingRecordPatch
    {
        private static void Postfix(SandSpiderAI __instance, Vector3 startPosition, Vector3 endPosition)
        {
            try
            {
                if (__instance == null) return;
                WebSpacing.Laid l = WebSpacing.Table.GetValue(__instance, k => new WebSpacing.Laid());
                l.Strands.Add(new Vector3[] { startPosition, endPosition });
                // A strand hung from the ceiling is steep. Give it its own trigger running the whole length of the strand, on the
                // same object as the game's web script, so walking or jumping through ANY part of it trips it like a floor web.
                if (Mathf.Abs(startPosition.y - endPosition.y) > 2f && __instance.webTraps != null && __instance.webTraps.Count > 0)
                {
                    SandSpiderWebTrap trap = __instance.webTraps[__instance.webTraps.Count - 1];
                    if (trap != null && trap.GetComponent<CapsuleCollider>() == null)
                    {
                        CapsuleCollider cc = trap.gameObject.AddComponent<CapsuleCollider>();
                        cc.isTrigger = true; cc.direction = 2; cc.radius = 0.45f; cc.height = 4f; cc.center = new Vector3(0f, 0f, 2f);
                        Plugin.Dbg("Ceiling web: added a trigger along the whole strand (" + Vector3.Distance(startPosition, endPosition).ToString("0.0") + " m).");
                    }
                }
            }
            catch (Exception) { }
        }
    }

    // Ceiling webs: besides the game's floor-to-wall trip lines, a patrolling spider sometimes hangs a strand from the ceiling down
    // to the floor, across where you walk. The game's own web code never aims upward.
    [HarmonyPatch(typeof(SandSpiderAI), "AttemptPlaceWebTrap")]
    internal static class CeilingWebPatch
    {
        private static void Postfix(SandSpiderAI __instance)
        {
            try
            {
                SandSpiderAI s = __instance;
                if (s == null || s.isEnemyDead || !s.IsOwner || s.abdomen == null) return;
                float chance = Plugin.CeilingWebChance.Value;
                if (chance <= 0.01f) return;
                if (Trapdoor.Ambushing(s) || Ceiling.Ambushing(s)) return;
                if (UnityEngine.Random.Range(0f, 100f) >= chance) return;
                int mask = StartOfRound.Instance.collidersAndRoomMask;
                Vector3 o = s.abdomen.position;
                for (int i = 0; i < s.webTraps.Count; i++)
                {
                    SandSpiderWebTrap w = s.webTraps[i];
                    if (w == null || w.centerOfWeb == null) continue;
                    Vector3 wc = w.centerOfWeb.position;
                    if (Vector2.Distance(new Vector2(wc.x, wc.z), new Vector2(o.x, o.z)) < 1.5f) return;
                }
                RaycastHit up;
                if (!Physics.Raycast(o + Vector3.up * 0.4f, Vector3.up, out up, 9f, mask)) return;
                if (up.distance < 2.5f) return;
                Vector3 ceil = up.point + Vector3.down * 0.05f;
                float ang = UnityEngine.Random.Range(0f, 360f), dist = UnityEngine.Random.Range(1.5f, 4f);
                Vector3 flat = Quaternion.Euler(0f, ang, 0f) * Vector3.forward * dist;
                Vector3 probe = new Vector3(o.x + flat.x, ceil.y - 0.3f, o.z + flat.z);
                RaycastHit down;
                if (!Physics.Raycast(probe, Vector3.down, out down, ceil.y - o.y + 6f, mask)) return;
                Vector3 end = down.point + Vector3.up * 0.2f;
                RaycastHit block;
                if (Physics.Linecast(ceil, end, out block, mask))
                {
                    if (block.distance < 2f) return;
                    end = block.point;
                }
                if (Vector3.Distance(ceil, end) < 2.5f) return;
                s.SpawnWebTrapServerRpc(ceil, end);
                Plugin.Dbg("Spider web: hung a strand from the ceiling (" + Vector3.Distance(ceil, end).ToString("0.0") + " m long).");
            }
            catch (Exception) { }
        }
    }

    // ...and must not lay webs that give its spot away.
    [HarmonyPatch(typeof(SandSpiderAI), "AttemptPlaceWebTrap")]
    internal static class TrapdoorNoWebPatch
    {
        private static bool Prefix(SandSpiderAI __instance, ref bool __result)
        {
            if (Trapdoor.Ambushing(__instance) || Ceiling.Ambushing(__instance)) { __result = false; return false; }
            return true;
        }
    }

    // The game's web frees any player who is not flagged as inside the facility, so webs never hold anyone on the
    // surface. For a spider that lives outdoors (testing only so far) keep the web holding them as normal.
    [HarmonyPatch(typeof(SandSpiderWebTrap), "CallPlayerLeaveWebOnDeath")]
    internal static class OutdoorWebHoldPatch
    {
        private static bool Prefix(SandSpiderWebTrap __instance)
        {
            try
            {
                SandSpiderAI sp = __instance.mainScript;
                if (sp == null || !Cocoon.SpiderOutdoors(sp)) return true;
                PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
                if (me != null && !me.isPlayerDead) return false; // stay stuck; the death case still runs the game's own code
            }
            catch (Exception) { }
            return true;
        }
    }

    // STAGE 4: the spider already tells every player's game when it hangs a body up.
    // We listen in, and that is when the death countdown starts.
    // (HangBodyFromCeiling is private, so we name it as text.)
    [HarmonyPatch(typeof(SandSpiderAI), "HangBodyFromCeiling")]
    internal static class SpiderHangPatch
    {
        private static bool Prefix(SandSpiderAI __instance, ref DeadBodyInfo ___currentlyHeldBody, ref bool ___decidedChanceToHangBodyEarly)
        {
            try
            {
                // Outdoors there is no ceiling to hang from: the spider drops the cocoon where it stands, and the victim
                // has to struggle free (or be cut down). Nothing is hung up.
                if (Cocoon.SpiderOutdoors(__instance))
                {
                    if (Trapdoor.Applies(__instance))
                    {
                        // Trapdoor spider: drag the catch underground (every game also marks the cocoon as hung).
                        if (__instance.IsOwner) Trapdoor.BeginPull(__instance, ___currentlyHeldBody);
                        else { Trapdoor.NoCollide(___currentlyHeldBody); Cocoon.OnBodyHung(___currentlyHeldBody, __instance); Trapdoor.MarkBuried(___currentlyHeldBody, __instance); }
                    }
                    else if ((TreePerch.Chosen(__instance) || TreePerch.Roams(__instance)) && TreePerch.TryHang(__instance, ref ___currentlyHeldBody, ref ___decidedChanceToHangBodyEarly)) { }
                    else Cocoon.OutdoorDrop(___currentlyHeldBody, __instance);
                    return false;
                }

                Cocoon.OnBodyHung(___currentlyHeldBody, __instance);

                // Same as the game's own hanging, but the victim hangs at a set height above the floor.
                float height = Plugin.HangHeightAboveGround.Value;
                DeadBodyInfo held = ___currentlyHeldBody;
                if (height <= 0f || held == null || held.bodyParts == null || held.bodyParts.Length <= 6 || __instance.hangBodyPhysicsPrefab == null) return true;

                if (!Physics.Raycast(__instance.abdomen.position, Vector3.up, out RaycastHit hit, 25f, StartOfRound.Instance.collidersAndRoomMaskAndDefault)) return true;
                Vector3 bodyPos = held.bodyParts[6].transform.position;
                if (!Physics.Raycast(bodyPos + Vector3.up * 0.3f, Vector3.down, out RaycastHit floor, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)) return true;

                // Tight spaces (stacked catwalks, low ceilings): there is no room to lift the body, so let the game hang it its own way.
                if (hit.point.y - floor.point.y < 3.2f) return true;

                // Hang at a set height above the floor, but keep a little web showing under the ceiling.
                float y = Mathf.Min(floor.point.y + height, hit.point.y - 0.5f);
                y = Mathf.Max(y, floor.point.y + 0.5f);
                y = Mathf.Max(y, bodyPos.y); // never lower than where the spider was carrying them (stops victims ending up under a grate)

                SetLineRendererPoints line = UnityEngine.Object.Instantiate(__instance.hangBodyPhysicsPrefab, hit.point, Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform).GetComponent<SetLineRendererPoints>();
                line.target.position = new Vector3(bodyPos.x, y, bodyPos.z);
                held.attachedTo = line.target;
                ___decidedChanceToHangBodyEarly = false;
                held.bodyAudio.volume = 0.8f;
                held.bodyAudio.PlayOneShot(__instance.hangPlayerSFX);
                ___currentlyHeldBody = null;
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SpiderHangPatch failed: " + e);
                return true;
            }
        }
    }

    // The spider speeds up as it closes in on the player it is chasing, but only while it can see them.
    // The spider's movement is worked out on the host (its "owner"), so this only runs there and
    // the other players just see the spider move faster.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderSpeedPatch
    {
        private sealed class Boost { public float Multiplier = 1f; }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Boost> Boosts
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Boost>();

        private sealed class Alert { public float Until; public Vector3 Spot; }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Alert> Alerts
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Alert>();

        // Called when a player touches one of this spider's webs.
        internal static void RaiseAlert(SandSpiderAI spider, Vector3 spot)
        {
            Alert a = Alerts.GetOrCreateValue(spider);
            a.Until = Time.time + Mathf.Max(0.5f, Plugin.WebAlertSeconds.Value);
            a.Spot = spot;
        }

        // Burst of speed towards a disturbed web. Does not apply while carrying or wrapping a victim.
        private static float ApplyWebAlert(SandSpiderAI spider, DeadBodyInfo held, bool spooling)
        {
            if (!Plugin.WebAlertEnabled.Value) return 1f;
            if (!Alerts.TryGetValue(spider, out Alert a) || Time.time >= a.Until) return 1f;
            if (held != null || spooling || spider.stunNormalizedTimer > 0f || spider.spiderSpeed <= 1f) return 1f;
            float dist = Vector3.Distance(spider.meshContainer.position, a.Spot);
            if (dist < 2.5f) { a.Until = 0f; return 1f; }
            float far = Mathf.Max(1f, Plugin.WebAlertFarMeters.Value);
            float extra = Mathf.Max(0f, Plugin.WebAlertPercent.Value) + Mathf.Max(0f, Plugin.WebAlertFarBonusPercent.Value) * Mathf.Clamp01(dist / far);
            float mult = 1f + extra / 100f;
            spider.agent.speed *= mult;
            spider.spiderSpeed *= mult;
            return mult;
        }

        // A faster spider also needs to accelerate and turn faster, otherwise the path-follower brakes hard
        // for every corner and the spider seems to lose all its speed when turning.
        private sealed class Handling { public float Accel; public float Turn; public bool Captured; }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Handling> HandlingStates
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Handling>();

        private static void UpdateHandling(SandSpiderAI spider, float total)
        {
            Handling h = HandlingStates.GetOrCreateValue(spider);
            if (total > 1.001f)
            {
                if (!h.Captured)
                {
                    h.Accel = spider.agent.acceleration;
                    h.Turn = spider.agent.angularSpeed;
                    h.Captured = true;
                }
                spider.agent.acceleration = h.Accel * total * 1.5f;
                spider.agent.angularSpeed = h.Turn * total * 1.5f;
            }
            else if (h.Captured)
            {
                spider.agent.acceleration = h.Accel;
                spider.agent.angularSpeed = h.Turn;
                h.Captured = false;
            }
        }

        private static void Postfix(SandSpiderAI __instance, bool ___watchFromDistance, float ___timeSinceHittingPlayer, DeadBodyInfo ___currentlyHeldBody, bool ___spoolingPlayerBody)
        {
            try
            {
                if (!__instance.IsOwner || __instance.isEnemyDead) return;
                float total = ApplyWebAlert(__instance, ___currentlyHeldBody, ___spoolingPlayerBody);
                if (!Plugin.ChaseBoostEnabled.Value) { UpdateHandling(__instance, total); return; }
                Boost boost = Boosts.GetOrCreateValue(__instance);

                PlayerControllerB prey = __instance.targetPlayer;
                // Only while actually running at someone: not watching from a distance, not stunned,
                // not in the short pause after a bite, and not once the prey is already down.
                bool chasing = __instance.currentBehaviourStateIndex == 2
                    && prey != null && !prey.isPlayerDead
                    && !___watchFromDistance
                    && __instance.stunNormalizedTimer <= 0f
                    && ___timeSinceHittingPlayer >= 0.5f
                    && __instance.spiderSpeed > 1f;

                float maxExtra = Mathf.Max(0f, Plugin.ChaseBoostMaxPercent.Value) / 100f;
                float target = 1f;
                if (chasing)
                {
                    Vector3 from = __instance.meshContainer.position;
                    Vector3 eyes = prey.gameplayCamera.transform.position;
                    // "Direct line of sight": nothing solid between the spider and the player's eyes.
                    bool canSee = !Physics.Linecast(from, eyes, StartOfRound.Instance.collidersAndRoomMaskAndDefault);
                    if (canSee)
                    {
                        float near = Plugin.ChaseBoostNearMeters.Value;
                        float far = Mathf.Max(near + 0.1f, Plugin.ChaseBoostFarMeters.Value);
                        float closeness = 1f - Mathf.Clamp01((Vector3.Distance(from, eyes) - near) / (far - near));
                        target = 1f + maxExtra * closeness;
                    }
                }

                // Build up gradually, but let go quickly once the spider loses sight of its prey.
                float ramp = Mathf.Max(0.1f, Plugin.ChaseBoostRampSeconds.Value);
                float perSecond = maxExtra / ramp;
                float step = (target > boost.Multiplier ? perSecond : perSecond * 2f) * Time.deltaTime;
                boost.Multiplier = Mathf.MoveTowards(boost.Multiplier, target, step);

                if (chasing && boost.Multiplier > 1.001f)
                {
                    // The spider has two speed values: the invisible path-follower and the visible body.
                    __instance.agent.speed *= boost.Multiplier;
                    __instance.spiderSpeed *= boost.Multiplier;
                    total *= boost.Multiplier;
                }
                UpdateHandling(__instance, total);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SpiderSpeedPatch failed: " + e);
            }
        }
    }

    // When a player steps in a web, tell the spider (on the host) where it happened.
    [HarmonyPatch(typeof(SandSpiderAI), "PlayerTripWebClientRpc")]
    internal static class SpiderWebAlertPatch
    {
        private static void Postfix(SandSpiderAI __instance, int trapID)
        {
            try
            {
                if (!__instance.IsOwner || __instance.isEnemyDead) return;
                if (trapID < 0 || trapID >= __instance.webTraps.Count) return;
                SpiderSpeedPatch.RaiseAlert(__instance, __instance.webTraps[trapID].centerOfWeb.position);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SpiderWebAlertPatch failed: " + e);
            }
        }
    }

    // The game only hangs a victim once the spider is within 0.1 m of its chosen wall spot. A spider that
    // bumps into a wall, railing or grate and shuffles about may never get that close, so accept a little less.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderWallReachPatch
    {
        private static void Postfix(SandSpiderAI __instance, ref bool ___reachedWallPosition, bool ___gotWallPositionInLOS, bool ___onWall)
        {
            try
            {
                float reach = Plugin.WallReachMeters.Value;
                if (reach <= 0.1f || !__instance.IsOwner || __instance.isEnemyDead) return;
                if (___reachedWallPosition || !__instance.lookingForWallPosition || !___gotWallPositionInLOS || !___onWall) return;
                float d = Vector3.Distance(__instance.meshContainer.position, __instance.wallPosition);
                if (d < reach)
                {
                    ___reachedWallPosition = true;
                    if (d >= 0.1f) Plugin.Dbg("Spider wall spot accepted at " + d.ToString("0.00") + " m (the game alone would have wanted 0.10 m).");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SpiderWallReachPatch failed: " + e);
            }
        }
    }

    // More webs, placed more often, over a wider area around where the spider started.
    // The spider's patrolling is worked out on the host, so only the host's settings matter.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Start))]
    internal static class SpiderWebCountPatch
    {
        private static void Postfix(SandSpiderAI __instance)
        {
            try
            {
                float m = Plugin.WebCountMultiplier.Value;
                if (m > 0f && Mathf.Abs(m - 1f) > 0.01f)
                {
                    __instance.maxWebTrapsToPlace = Mathf.Max(1, Mathf.RoundToInt(__instance.maxWebTrapsToPlace * m));
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SpiderWebCountPatch failed: " + e);
            }
        }
    }

    // Turrets and spike traps both look for the local player's real body, which stays where it was (hidden) while the
    // player is wrapped up. A wrapped player is neither shot nor crushed.
    internal static class HazardIgnorePatch
    {
        internal static void TurretPostfix(ref PlayerControllerB __result)
        {
            try { if (__result != null && Cocoon.IsCocooned(__result)) __result = null; }
            catch (Exception) { }
        }

        internal static void TurretLosPrefix(PlayerControllerB ___targetPlayerWithRotation, ref bool ___targetingDeadPlayer)
        {
            try { if (___targetPlayerWithRotation != null && Cocoon.IsCocooned(___targetPlayerWithRotation)) ___targetingDeadPlayer = true; }
            catch (Exception) { }
        }

        // false = skip the trap's own code
        internal static bool SpikePrefix(Collider other)
        {
            try
            {
                PlayerControllerB pc = other != null ? other.gameObject.GetComponent<PlayerControllerB>() : null;
                if (pc != null && Cocoon.IsCocooned(pc)) return false;
            }
            catch (Exception) { }
            return true;
        }
    }

    // The game's landmine counts two things as "the local player": their real body (which this mod teleports around
    // while they are wrapped, and the mine reads a teleport as stepping off) and their stand-in body (the ragdoll the
    // spider carries and hangs). Neither may set a mine off while the player is wrapped.
    internal static class LandmineIgnorePatch
    {
        private static System.Reflection.FieldInfo onMine;
        private static bool onMineSearched;

        private static bool IsWrapped(Collider other)
        {
            if (other == null) return false;
            if (other.CompareTag("Player"))
            {
                PlayerControllerB pc = other.gameObject.GetComponent<PlayerControllerB>();
                return pc != null && Cocoon.IsCocooned(pc);
            }
            if (other.tag.StartsWith("PlayerRagdoll"))
            {
                DeadBodyInfo db = other.GetComponent<DeadBodyInfo>();
                return db != null && db.playerScript != null && Cocoon.IsCocooned(db.playerScript);
            }
            return false;
        }

        // false = skip the mine's own code
        internal static bool Prefix(Collider other)
        {
            try { return !IsWrapped(other); }
            catch (Exception) { return true; }
        }

        // The mine also sets itself off when the local player is "on" it and gets teleported: forget they are on it.
        internal static void UpdatePrefix(object __instance)
        {
            try
            {
                PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
                if (me == null || !Cocoon.IsCocooned(me)) return;
                if (!onMineSearched)
                {
                    onMineSearched = true;
                    onMine = AccessTools.Field(__instance.GetType(), "localPlayerOnMine");
                }
                if (onMine != null) onMine.SetValue(__instance, false);
            }
            catch (Exception) { }
        }
    }

    // DebugLogging only: while the local player is wrapped in a cocoon (and for 6 s after), write down every sound
    // that starts or stops playing anywhere in the game, and every one-shot sound, with where it comes from.
    // Extra rolls while a victim is being wrapped. The game's wrap animation turns the body for about the first 2 s of its 4 s and then
    // holds it still. We keep turning it round the same axis for the rest of the wrap, ending on a whole number of turns.
    internal static class WrapSpin
    {
        private sealed class St
        {
            public SandSpiderAI S; public DeadBodyInfo Body; public float T0;
            public Quaternion Last; public bool Have; public Vector3 Sum, Axis;
            public float TStop = -1f, StillFor, Extra;
        }
        private static readonly Dictionary<int, St> BySpider = new Dictionary<int, St>();
        private static readonly Dictionary<DeadBodyInfo, St> ByBody = new Dictionary<DeadBodyInfo, St>();
        private static readonly FieldInfo Held = AccessTools.Field(typeof(SandSpiderAI), "currentlyHeldBody");

        internal static void Begin(SandSpiderAI s)
        {
            if (s == null || Plugin.WrapExtraRolls.Value <= 0.01f) return;
            int id = s.GetInstanceID();
            if (BySpider.ContainsKey(id)) return;
            CocoonRunner.Run(Drive(s, id));
        }

        private static IEnumerator Drive(SandSpiderAI s, int id)
        {
            St st = new St { S = s };
            BySpider[id] = st;
            float wait = 0f;
            while (s != null && Held.GetValue(s) == null && wait < 3f) { wait += Time.deltaTime; yield return null; }
            st.T0 = Time.time;
            const float total = 4.0f;
            while (s != null && !s.isEnemyDead && Time.time - st.T0 < total + 0.3f)
            {
                DeadBodyInfo b = Held.GetValue(s) as DeadBodyInfo;
                Transform mt = s.mouthTarget;
                if (b == null || mt == null) break;
                if (st.Body != b) { if (st.Body != null) ByBody.Remove(st.Body); st.Body = b; ByBody[b] = st; }
                float el = Time.time - st.T0, dt = Mathf.Max(Time.deltaTime, 0.001f);
                if (st.Have)
                {
                    Quaternion d = mt.rotation * Quaternion.Inverse(st.Last);
                    float ang; Vector3 ax; d.ToAngleAxis(out ang, out ax);
                    if (ang > 180f) ang -= 360f;
                    float speed = Mathf.Abs(ang) / dt;
                    if (el < 2.6f && Mathf.Abs(ang) > 0.3f) st.Sum += ax * ang;
                    if (st.TStop < 0f && el > 0.8f)
                    {
                        if (speed < 8f) { st.StillFor += dt; if (st.StillFor > 0.25f) st.TStop = el - 0.25f; }
                        else st.StillFor = 0f;
                    }
                }
                st.Last = mt.rotation; st.Have = true;
                if (st.TStop < 0f && el > 2.4f) st.TStop = el;
                if (st.TStop >= 0f && el < total)
                {
                    float u = Mathf.Clamp01((el - st.TStop) / Mathf.Max(0.3f, total - st.TStop));
                    u = u * u * (3f - 2f * u);
                    st.Extra = 360f * Plugin.WrapExtraRolls.Value * u;
                    if (st.Axis == Vector3.zero)
                    {
                        st.Axis = st.Sum.sqrMagnitude > 0.01f ? st.Sum.normalized : mt.up;
                        Plugin.Log.LogInfo("Wrap spin: the animation stopped rolling at " + st.TStop.ToString("0.0") + " s; adding " + Plugin.WrapExtraRolls.Value.ToString("0.#") + " extra rolls round " + st.Axis.ToString("0.00") + ".");
                    }
                }
                else if (el >= total) st.Extra = 0f;
                yield return null;
            }
            if (st.Body != null) ByBody.Remove(st.Body);
            BySpider.Remove(id);
        }

        internal static void Apply(DeadBodyInfo body)
        {
            if (ByBody.Count == 0) return;
            St st;
            if (!ByBody.TryGetValue(body, out st) || st.Extra <= 0.01f || st.Axis == Vector3.zero) return;
            Rigidbody l = body.attachedLimb;
            if (l == null || body.attachedTo == null) return;
            Quaternion q = Quaternion.AngleAxis(st.Extra, st.Axis) * body.attachedTo.rotation;
            l.rotation = q;
            l.transform.rotation = q;
        }
    }

    [HarmonyPatch(typeof(DeadBodyInfo), "LateUpdate")]
    internal static class WrapSpinPatch
    {
        private static void Postfix(DeadBodyInfo __instance)
        {
            try { WrapSpin.Apply(__instance); } catch (Exception) { }
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.SpiderTurnBodyIntoWebClientRpc))]
    internal static class WrapSpinStartPatch
    {
        private static void Postfix(SandSpiderAI __instance)
        {
            try { WrapSpin.Begin(__instance); } catch (Exception) { }
        }
    }

    // Muffles the voice of anyone who is wrapped up, with the same settings the centipede uses (low-pass filter on their voice source).
    // On a joiner's game the game moves the spider's body in steps: the host only sends a new position once the spider has moved
    // half a metre, and the joiner nearly snaps to each one. The legs follow every step, which looks like jitter. Here the position the
    // game steers towards is smoothed first, so the body (and so the legs) glides.
    internal static class JoinerSmooth
    {
        internal sealed class JS { public Vector3 Received, Smoothed, Vel; public bool Has; }
        internal static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, JS> Table
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, JS>();
        private static bool logged;

        internal static void Received(SandSpiderAI s)
        {
            if (s == null || s.IsOwner || !Plugin.SmoothJoinerSpider.Value) return;
            JS j = Table.GetValue(s, k => new JS());
            j.Received = s.meshContainerServerPosition;
            if (!j.Has) { j.Smoothed = s.meshContainer != null ? s.meshContainer.position : j.Received; j.Has = true; }
        }

        internal static void Before(SandSpiderAI s)
        {
            if (s == null || s.IsOwner || s.isEnemyDead || !Plugin.SmoothJoinerSpider.Value) return;
            JS j;
            if (!Table.TryGetValue(s, out j) || !j.Has) return;
            if (Vector3.Distance(j.Smoothed, j.Received) > 6f) { j.Smoothed = j.Received; j.Vel = Vector3.zero; }
            else j.Smoothed = Vector3.SmoothDamp(j.Smoothed, j.Received, ref j.Vel, 0.2f);
            s.meshContainerServerPosition = j.Smoothed;
            if (!logged) { logged = true; Plugin.Log.LogInfo("Joiner spider smoothing is active on this game."); }
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.SyncMeshContainerPositionClientRpc))]
    internal static class JoinerSmoothReceivePatch
    {
        private static void Postfix(SandSpiderAI __instance) { try { JoinerSmooth.Received(__instance); } catch (Exception) { } }
    }

    [HarmonyPatch(typeof(SandSpiderAI), "LateUpdate")]
    internal static class JoinerSmoothFramePatch
    {
        private static void Prefix(SandSpiderAI __instance) { try { JoinerSmooth.Before(__instance); } catch (Exception) { } }
    }

    // The web's "stuck" loop is started on every player's game when someone trips it, but the game only stops it for the
    // player it believes is held (which is never set on other players' games, and our own release code clears without
    // stopping it). So it could loop for ever. Here: while the loop plays, check a few times a second that a living,
    // un-cocooned player is really inside the web, and stop it if not.
    [HarmonyPatch(typeof(SandSpiderWebTrap), "Update")]
    internal static class WebLoopWatchdogPatch
    {
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderWebTrap, float[]> State
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderWebTrap, float[]>();

        private static void Postfix(SandSpiderWebTrap __instance)
        {
            try
            {
                AudioSource au = __instance.webAudio;
                if (au == null) return;
                float[] st = State.GetValue(__instance, k => new float[2]); // [0] playing since, [1] next check
                if (!au.isPlaying) { st[0] = 0f; return; }
                if (st[0] <= 0f) { st[0] = Time.time; st[1] = Time.time + 0.75f; return; }
                if (Time.time < st[1]) return;
                st[1] = Time.time + 0.25f;
                Collider area = __instance.GetComponent<Collider>();
                StartOfRound sor = StartOfRound.Instance;
                bool someoneInside = false;
                if (area != null && sor != null)
                {
                    foreach (PlayerControllerB p in sor.allPlayerScripts)
                    {
                        if (p == null || !p.isPlayerControlled || p.isPlayerDead || p.thisController == null) continue;
                        if (Cocoon.IsCocooned(p)) continue;
                        if (area.bounds.Intersects(p.thisController.bounds)) { someoneInside = true; break; }
                    }
                }
                if (!someoneInside)
                {
                    au.Stop();
                    st[0] = 0f;
                    Plugin.Dbg("Web stuck-sound was still looping with nobody in the web: stopped it.");
                }
            }
            catch (Exception) { }
        }
    }

    // Every game hangs the body for itself, from wherever its own copy of the body happened to be, so the cocoon could end up
    // up to a couple of metres apart (and inside a wall) on a joiner. The spider's owner says where its hang point ended up
    // and the other games move theirs to the same place.
    internal static class HangSync
    {
        internal const string Name = "HungToDry.HangPos";

        internal static void Announce(CocoonState st)
        {
            if (st == null || st.Spider == null || !st.Spider.IsOwner) return;
            CocoonRunner.Run(Send(st));
        }

        // The rope's end is a physics object, so each game's copy swings and settles a little differently (and can
        // catch on a wall). While the cocoon is hung the owner streams where its rope end is and the other games hold
        // theirs there.
        private static IEnumerator Send(CocoonState st)
        {
            yield return new WaitForSeconds(0.3f);
            int sent = 0;
            while (true)
            {
                try
                {
                    Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                    SandSpiderAI sp = st.Spider;
                    if (nm == null || !nm.IsListening || nm.CustomMessagingManager == null || sp == null || sp.NetworkObject == null || !sp.NetworkObject.IsSpawned || !st.Hung) yield break;
                    if (nm.ConnectedClientsIds.Count >= 2)
                    {
                        DeadBodyInfo b = st.StandIn != null ? st.StandIn : (st.Player != null ? st.Player.deadBody : null);
                        if (b == null || b.attachedTo == null) yield break;
                        Vector3 pos = b.attachedTo.position;
                        using (Unity.Netcode.FastBufferWriter w = new Unity.Netcode.FastBufferWriter(48, Unity.Collections.Allocator.Temp))
                        {
                            w.WriteValueSafe(sp.NetworkObject.NetworkObjectId);
                            w.WriteValueSafe(st.Player != null ? (int)st.Player.playerClientId : -1);
                            w.WriteValueSafe(pos.x); w.WriteValueSafe(pos.y); w.WriteValueSafe(pos.z);
                            nm.CustomMessagingManager.SendNamedMessageToAll(Name, w);
                        }
                        if (sent++ == 0) Plugin.Dbg("Hang position sent: " + pos.ToString("F2") + " (and every 0.2 s while hung)");
                    }
                }
                catch (Exception e) { Plugin.Log.LogWarning("Hang sync: could not send: " + e.Message); yield break; }
                yield return new WaitForSeconds(0.2f);
            }
        }

        private sealed class Follow { public Vector3 Latest; public float Stamp; public bool Running; }
        private static readonly Dictionary<ulong, Follow> Following = new Dictionary<ulong, Follow>();

        internal static void OnMessage(ulong sender, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                ulong id; int pid; float x, y, z;
                reader.ReadValueSafe(out id); reader.ReadValueSafe(out pid); reader.ReadValueSafe(out x); reader.ReadValueSafe(out y); reader.ReadValueSafe(out z);
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                Unity.Netcode.NetworkObject no;
                if (nm == null || nm.SpawnManager == null || !nm.SpawnManager.SpawnedObjects.TryGetValue(id, out no) || no == null) return;
                SandSpiderAI sp = no.GetComponent<SandSpiderAI>();
                if (sp == null || sp.IsOwner) return;
                Follow f;
                ulong key = id * 16UL + (ulong)(pid & 15);
                if (!Following.TryGetValue(key, out f)) { f = new Follow(); Following[key] = f; }
                f.Latest = new Vector3(x, y, z); f.Stamp = Time.time;
                if (!f.Running) { f.Running = true; CocoonRunner.Run(Hold(sp, pid, f)); }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Hang sync: bad message: " + e.Message); }
        }

        private static List<DeadBodyInfo> BodiesOf(SandSpiderAI sp, int pid)
        {
            List<DeadBodyInfo> list = new List<DeadBodyInfo>();
            foreach (KeyValuePair<int, CocoonState> kv in Cocoon.ActiveStates)
            {
                CocoonState st = kv.Value;
                if (st.Spider != sp || !st.Hung || kv.Key != pid) continue;
                if (st.StandIn != null && st.StandIn.attachedTo != null) list.Add(st.StandIn);
                if (st.Player != null && st.Player.deadBody != null && st.Player.deadBody != st.StandIn && st.Player.deadBody.attachedTo != null) list.Add(st.Player.deadBody);
            }
            return list;
        }

        // Holds this game's rope end at the owner's position until the cocoon is gone or the messages stop.
        private static IEnumerator Hold(SandSpiderAI sp, int pid, Follow f)
        {
            Dictionary<Transform, Vector3> vel = new Dictionary<Transform, Vector3>();
            HashSet<Transform> done = new HashSet<Transform>();
            float firstSeen = -1f;
            while (sp != null && Time.time - f.Stamp < 3f)
            {
                try
                {
                    foreach (DeadBodyInfo b in BodiesOf(sp, pid))
                    {
                        Transform t = b.attachedTo;
                        // Only the hang rope's end: before this game has hung its own copy the body is still attached to the spider's mouth.
                        if (t.GetComponentInParent<SetLineRendererPoints>() == null) continue;
                        Rigidbody rb = t.GetComponent<Rigidbody>();
                        if (rb != null && !rb.isKinematic) rb.isKinematic = true;
                        Vector3 v; vel.TryGetValue(t, out v);
                        if (done.Add(t))
                        {
                            Plugin.Log.LogInfo("Hang position sync: holding the rope end at the spider owner's position (was " + Vector3.Distance(t.position, f.Latest).ToString("0.00") + " m away"
                                + (rb != null ? ", it was a physics object" : ", no physics object") + ").");
                            if (firstSeen < 0f) firstSeen = Time.time;
                        }
                        t.position = Vector3.SmoothDamp(t.position, f.Latest, ref v, 0.15f);
                        vel[t] = v;
                    }
                }
                catch (Exception e) { Plugin.Log.LogWarning("Hang sync: could not apply: " + e.Message); break; }
                yield return null;
            }
            f.Running = false;
        }
    }

    internal static class VoiceMuffle
    {
        private static float nextRefresh;
        private static readonly HashSet<int> On = new HashSet<int>();
        private static readonly HashSet<int> NoSrcLogged = new HashSet<int>();
        private static float next;

        internal static void Tick()
        {
            try
            {
                if (!Plugin.MuffleVoices.Value && On.Count == 0) return;
                if (Time.time < next) return;
                next = Time.time + 0.25f;
                StartOfRound sor = StartOfRound.Instance;
                if (sor == null || sor.allPlayerScripts == null) return;
                for (int i = 0; i < sor.allPlayerScripts.Length; i++)
                {
                    PlayerControllerB p = sor.allPlayerScripts[i];
                    if (p == null) continue;
                    int id = (int)p.playerClientId;
                    bool want = Plugin.MuffleVoices.Value && Cocoon.IsCocooned(p);
                    bool has = On.Contains(id);
                    if (!want && !has) continue;
                    AudioSource src = p.currentVoiceChatAudioSource;
                    if (src == null && Time.time >= nextRefresh) { nextRefresh = Time.time + 3f; sor.RefreshPlayerVoicePlaybackObjects(); src = p.currentVoiceChatAudioSource; } // the game logs a line each time, so not every tick
                    if (src == null)
                    {
                        if (want && NoSrcLogged.Add(id)) Plugin.Log.LogInfo("Voice muffle: player " + id + " is wrapped but has no voice source in this game (normal for your own player when playing alone).");
                        if (!want) { On.Remove(id); NoSrcLogged.Remove(id); p.voiceMuffledByEnemy = false; }
                        continue;
                    }
                    NoSrcLogged.Remove(id);
                    AudioLowPassFilter lp = src.GetComponent<AudioLowPassFilter>();
                    OccludeAudio oc = src.GetComponent<OccludeAudio>();
                    if (want)
                    {
                        if (lp != null) lp.lowpassResonanceQ = Plugin.MuffleResonance.Value;
                        if (oc != null) { oc.overridingLowPass = true; oc.lowPassOverride = Plugin.MuffleCutoff.Value; }
                        p.voiceMuffledByEnemy = true;
                        if (On.Add(id)) Plugin.Log.LogInfo("Voice muffle: player " + id + " is muffled while wrapped.");
                    }
                    else
                    {
                        if (lp != null) lp.lowpassResonanceQ = 1f;
                        if (oc != null) { oc.overridingLowPass = false; oc.lowPassOverride = 20000f; }
                        p.voiceMuffledByEnemy = false;
                        On.Remove(id);
                        Plugin.Log.LogInfo("Voice muffle: player " + id + " sounds normal again.");
                    }
                }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Voice muffle failed: " + e.Message); }
        }
    }

    internal static class SoundDebug
    {
        internal static bool Active;
        private static float lastCocoon = -100f, next, windowStart;
        private static int windowCount, suppressed;
        private static readonly Dictionary<int, string> Playing = new Dictionary<int, string>();

        private static string PathOf(Transform t)
        {
            string s = t != null ? t.name : "?";
            Transform p = t != null ? t.parent : null;
            for (int i = 0; i < 4 && p != null; i++) { s = p.name + "/" + s; p = p.parent; }
            return s;
        }

        private static string Describe(AudioSource a, Vector3 ear, string clipName)
        {
            string mix = a.outputAudioMixerGroup != null ? a.outputAudioMixerGroup.name : "-";
            return PathOf(a.transform) + " | clip=" + clipName + " | loop=" + a.loop + " | vol=" + a.volume.ToString("0.00")
                + " | 3D=" + a.spatialBlend.ToString("0.0") + " | " + Vector3.Distance(a.transform.position, ear).ToString("0.0") + " m away | mixer=" + mix
                + (a.mute ? " | MUTED" : "");
        }

        internal static void Tick()
        {
            try
            {
                PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
                if (me == null || !Plugin.DebugLogging.Value || !Plugin.VerboseLogs.Value) { Active = false; return; }
                if (Cocoon.IsCocooned(me)) lastCocoon = Time.time;
                bool was = Active;
                Active = Time.time - lastCocoon < 6f || (Plugin.LogAllSounds != null && Plugin.LogAllSounds.Value);
                if (!Active) { if (Playing.Count > 0) Playing.Clear(); return; }
                if (!was) Plugin.Log.LogInfo("Sound debug: watching every sound while you are wrapped up (and for 6 s afterwards).");
                if (Time.time < next) return;
                next = Time.time + 0.25f;
                Vector3 ear = me.gameplayCamera != null ? me.gameplayCamera.transform.position : me.transform.position;
                HashSet<int> now = new HashSet<int>();
                foreach (AudioSource a in UnityEngine.Object.FindObjectsOfType<AudioSource>())
                {
                    if (a == null || !a.isPlaying) continue;
                    int id = a.GetInstanceID();
                    now.Add(id);
                    if (Playing.ContainsKey(id)) continue;
                    string d = Describe(a, ear, a.clip != null ? a.clip.name : "(one-shot)");
                    Playing[id] = d;
                    Plugin.Log.LogInfo("Sound start: " + d);
                }
                List<int> gone = null;
                foreach (KeyValuePair<int, string> kv in Playing)
                    if (!now.Contains(kv.Key)) { if (gone == null) gone = new List<int>(); gone.Add(kv.Key); }
                if (gone != null)
                    foreach (int id in gone) { Plugin.Log.LogInfo("Sound stop: " + Playing[id]); Playing.Remove(id); }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Sound debug failed: " + e.Message); }
        }

        internal static void OneShot(AudioSource src, AudioClip clip, float volume)
        {
            if (!Plugin.VerboseLogs.Value) return;
            try
            {
                if (!Active) return;
                if (Time.time - windowStart > 1f) { windowStart = Time.time; windowCount = 0; if (suppressed > 0) { Plugin.Log.LogInfo("Sound one-shot: " + suppressed + " more in the last second not shown."); suppressed = 0; } }
                if (++windowCount > 15) { suppressed++; return; }
                Vector3 ear = Camera.main != null ? Camera.main.transform.position : src.transform.position;
                Plugin.Log.LogInfo("Sound one-shot: " + Describe(src, ear, clip.name) + " | scale=" + volume.ToString("0.00"));
            }
            catch (Exception) { }
        }
    }

    // Spider footsteps played as one-shot sounds: scale them by the footstep volume setting.
    internal static class SpiderStepSoundPatch
    {
        static int logged;
        internal static void Prefix(AudioSource __instance, AudioClip clip, ref float volumeScale)
        {
            try
            {
                if (clip == null || __instance == null) return;
                SoundDebug.OneShot(__instance, clip, volumeScale);
                string n = clip.name;
                bool stepLike = n.IndexOf("step", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("foot", StringComparison.OrdinalIgnoreCase) >= 0;
                if (!stepLike && !Plugin.DebugLogging.Value) return;
                if (__instance.GetComponentInParent<SandSpiderAI>() == null) return;
                if (Plugin.DebugLogging.Value && Plugin.VerboseLogs.Value && logged < 30)
                {
                    logged++;
                    Plugin.Dbg("Spider one-shot sound: clip='" + n + "' source='" + __instance.gameObject.name + "' scale=" + volumeScale.ToString("0.00") + " stepLike=" + stepLike);
                }
            }
            catch (Exception) { }
        }
    }

    // Stops any monster picking a wrapped-up player's hidden body as the answer to "who is nearest / who can I see".
    internal static class EnemyPickPlayerPatch
    {
        internal static void Postfix(ref PlayerControllerB __result)
        {
            try
            {
                if (__result != null && Cocoon.IsCocooned(__result)) __result = null;
            }
            catch (Exception) { }
        }
    }

    // Snare fleas and thumpers: if one has locked on to a wrapped-up player anyway, make it let go.
    internal static class EnemyDropTargetPatch
    {
        // A hiding snare flea drops when the local player is below it. If this game's own player is wrapped up
        // (their hidden body is still where they were caught), it must not fall. Run before AND after the
        // flea's own update so there is no frame where the check can fire.
        internal static void HoldFlea(EnemyAI e)
        {
            try
            {
                CentipedeAI flea = e as CentipedeAI;
                if (flea == null) return;
                PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
                FieldInfo tf = TriggeredFallField;
                if (me == null || tf == null) return;
                if (Cocoon.IsCocooned(me)) { tf.SetValue(flea, true); FleaHeld.Add(flea); }
                else if (FleaHeld.Remove(flea)) tf.SetValue(flea, false);
            }
            catch (Exception) { }
        }

        internal static void Prefix(EnemyAI __instance) { HoldFlea(__instance); }

        // Looked up once (this runs every frame for every snare flea).
        private static readonly FieldInfo TriggeredFallField = typeof(CentipedeAI).GetField("triggeredFall", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        static float nextLog;
        static readonly HashSet<CentipedeAI> FleaHeld = new HashSet<CentipedeAI>();
        internal static void Postfix(EnemyAI __instance)
        {
            try
            {
                if (!(__instance is CentipedeAI) && !(__instance is CrawlerAI)) return;
                HoldFlea(__instance);
                PlayerControllerB t = __instance.targetPlayer;
                if (t == null || !Cocoon.IsCocooned(t)) return;
                __instance.targetPlayer = null;
                __instance.movingTowardsTargetPlayer = false;
                if (Time.time >= nextLog)
                {
                    nextLog = Time.time + 5f;
                    Plugin.Dbg(__instance.GetType().Name + " dropped its target: that player is wrapped up.");
                }
            }
            catch (Exception) { }
        }
    }

    // Turns the spider's footsteps down. Finds the spider's footstep audio sources once (by name), remembers
    // each one's original volume, and keeps them at the chosen fraction of it. Only affects this game's audio.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderQuietStepsPatch
    {
        private sealed class Found
        {
            public List<AudioSource> Sources = new List<AudioSource>();
            public List<float> BaseVolumes = new List<float>();
            public List<float> BaseMax = new List<float>();
            public List<float> BaseMin = new List<float>();
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Found> Table
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, Found>();

        private static readonly HashSet<AudioSource> Handled = new HashSet<AudioSource>();
        internal static bool IsHandled(AudioSource a) { return a != null && Handled.Contains(a); }

        private static bool LooksLikeSteps(string n)
        {
            if (string.IsNullOrEmpty(n)) return false;
            n = n.ToLowerInvariant();
            return n.Contains("foot") || n.Contains("step");
        }

        private static void Postfix(SandSpiderAI __instance)
        {
            try
            {
                Found f;
                if (!Table.TryGetValue(__instance, out f))
                {
                    f = new Found();
                    Table.Add(__instance, f);
                    HashSet<AudioSource> seen = new HashSet<AudioSource>();
                    // 1. fields of the spider script with "foot"/"step" in the name
                    foreach (FieldInfo fi in typeof(SandSpiderAI).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (fi.FieldType != typeof(AudioSource) || !LooksLikeSteps(fi.Name)) continue;
                        AudioSource a = fi.GetValue(__instance) as AudioSource;
                        if (a != null && seen.Add(a)) { f.Sources.Add(a); f.BaseVolumes.Add(a.volume); f.BaseMax.Add(a.maxDistance); f.BaseMin.Add(a.minDistance); Handled.Add(a); }
                    }
                    // 2. audio sources on the spider whose own name has "foot"/"step" in it
                    foreach (AudioSource a in __instance.GetComponentsInChildren<AudioSource>(true))
                    {
                        if (a != null && LooksLikeSteps(a.gameObject.name) && seen.Add(a)) { f.Sources.Add(a); f.BaseVolumes.Add(a.volume); f.BaseMax.Add(a.maxDistance); f.BaseMin.Add(a.minDistance); Handled.Add(a); }
                    }
                    if (Plugin.DebugLogging.Value)
                    {
                        foreach (AudioSource a in __instance.GetComponentsInChildren<AudioSource>(true))
                        {
                            Plugin.Verbose("Spider audio source: '" + a.gameObject.name + "' volume=" + a.volume.ToString("0.00") + " min=" + a.minDistance.ToString("0.0") + " max=" + a.maxDistance.ToString("0.0") + " rolloff=" + a.rolloffMode + " spatial=" + a.spatialBlend.ToString("0.0") +
                                " clip=" + (a.clip != null ? a.clip.name : "-") + " quietened=" + seen.Contains(a));
                        }
                    }
                    Plugin.Verbose("Spider footsteps: found " + f.Sources.Count + " audio source(s) to turn down.");
                }
                for (int i = 0; i < f.Sources.Count; i++)
                {
                    if (f.Sources[i] == null) continue;
                    float q = Ceiling.StalkQuiet(__instance);
                    f.Sources[i].volume = f.BaseVolumes[i] * q;
                    float range = Mathf.Clamp(Plugin.SpiderFootstepRange.Value, 0.1f, 1f);
                    f.Sources[i].maxDistance = f.BaseMax[i] * range * Mathf.Max(q, 0.25f);
                    f.Sources[i].minDistance = Mathf.Min(f.BaseMin[i], f.Sources[i].maxDistance);
                }
            }
            catch (Exception) { }
        }
    }

    // The spider's web line, like the fox's tongue. The host decides when it starts and stops and tells every game.
    // Every game draws the line; only the pulled player's own game applies the pull (players move on their own game).
    internal static class SpiderWeb
    {
        internal const string Name = "SpiderCocoon.Web";

        private sealed class Entry
        {
            public SandSpiderAI Spider; public PlayerControllerB Target; public LineRenderer Line; public float Start, NextFear, NextSpool; public Transform Anchor;
        }

        private static readonly Dictionary<ulong, Entry> Active = new Dictionary<ulong, Entry>();

        private static ulong Id(SandSpiderAI s)
        {
            Unity.Netcode.NetworkObject no = s.NetworkObject;
            return no != null ? no.NetworkObjectId : (ulong)(uint)s.GetInstanceID();
        }

        // Where the line starts: the middle of the body, a little towards the player (the spider faces the player while it shoots,
        // so this does not depend on which way the model's own "forward" points).
        internal static Vector3 Mouth(SandSpiderAI s, Vector3 toward)
        {
            Transform m = s.meshContainer != null ? s.meshContainer : s.transform;
            Vector3 c;
            SkinnedMeshRenderer r = s.spiderNormalMesh;
            // The spider's own abdomen position (the game uses it for placing webs); fall back to the middle of the model.
            if (s.abdomen != null) c = s.abdomen.position;
            else if (r != null) c = r.bounds.center; else c = m.position + Vector3.up * 1f;
            Vector3 d = toward - c; d.y = 0f;
            if (d.sqrMagnitude < 0.01f) { d = m.forward; d.y = 0f; }
            d.Normalize();
            return c + d * Plugin.WebOriginForward.Value + Vector3.up * Plugin.WebOriginUp.Value;
        }

        internal static void Start(SandSpiderAI s, PlayerControllerB target)
        {
            Begin(Id(s), s, target);
            Send(true, Id(s), (int)target.playerClientId);
        }

        internal static void Stop(SandSpiderAI s)
        {
            if (s == null) return;
            End(Id(s));
            Send(false, Id(s), 0);
        }

        private static void Send(bool start, ulong id, int playerId)
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsListening || !nm.IsServer || nm.CustomMessagingManager == null || !CocoonNet.Ready || nm.ConnectedClientsIds.Count < 2) return;
                using (Unity.Netcode.FastBufferWriter w = new Unity.Netcode.FastBufferWriter(32, Unity.Collections.Allocator.Temp))
                {
                    w.WriteValueSafe(start);
                    w.WriteValueSafe(id);
                    w.WriteValueSafe(playerId);
                    nm.CustomMessagingManager.SendNamedMessageToAll(Name, w);
                }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Spider web: could not send: " + e.Message); }
        }

        internal static void OnMessage(ulong senderClientId, Unity.Netcode.FastBufferReader reader)
        {
            try
            {
                bool start; ulong id; int pid;
                reader.ReadValueSafe(out start);
                reader.ReadValueSafe(out id);
                reader.ReadValueSafe(out pid);
                if (!start) { End(id); return; }
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                Unity.Netcode.NetworkObject no;
                if (nm == null || nm.SpawnManager == null || !nm.SpawnManager.SpawnedObjects.TryGetValue(id, out no) || no == null) return;
                SandSpiderAI s = no.GetComponent<SandSpiderAI>();
                PlayerControllerB[] ps = StartOfRound.Instance.allPlayerScripts;
                if (s == null || pid < 0 || pid >= ps.Length) return;
                Begin(id, s, ps[pid]);
            }
            catch (Exception e) { Plugin.Log.LogWarning("Spider web: bad message: " + e.Message); }
        }

        private static void Begin(ulong id, SandSpiderAI s, PlayerControllerB target)
        {
            End(id);
            List<ulong> dead = null;
            foreach (KeyValuePair<ulong, Entry> kv in Active) if (kv.Value.Spider == null) { if (dead == null) dead = new List<ulong>(); dead.Add(kv.Key); }
            if (dead != null) foreach (ulong d in dead) End(d);

            GameObject go = new GameObject("SpiderWebLine");
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true; lr.positionCount = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            LineRenderer src = null;
            if (s.hangBodyPhysicsPrefab != null) src = s.hangBodyPhysicsPrefab.GetComponentInChildren<LineRenderer>(true);
            if (src != null)
            {
                lr.sharedMaterial = src.sharedMaterial; lr.startWidth = src.startWidth; lr.endWidth = src.endWidth; lr.textureMode = src.textureMode;
            }
            else
            {
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startWidth = 0.05f; lr.endWidth = 0.05f;
            }
            Vector3 m = Mouth(s, target.transform.position);
            lr.SetPosition(0, m); lr.SetPosition(1, m);
            // The start of the line is pinned to the spider itself: a point fixed to its abdomen that moves and turns with it.
            Transform anchor = null;
            try
            {
                if (s.abdomen != null)
                {
                    GameObject ag = new GameObject("SpiderWebAnchor");
                    anchor = ag.transform;
                    anchor.position = m;
                    anchor.SetParent(s.abdomen, true);
                }
            }
            catch (Exception) { }
            Active[id] = new Entry { Spider = s, Target = target, Line = lr, Start = Time.time, NextSpool = Time.time + 4f, Anchor = anchor };
            Spool(s);
            if (!hooked) { hooked = true; Application.onBeforeRender += BeforeRender; }
        }

        private static bool hooked;

        // Runs just before the frame is drawn, after the spider's animation and every other update, so the line start never lags behind the body.
        private static void BeforeRender()
        {
            try
            {
                if (Active.Count == 0) return;
                foreach (KeyValuePair<ulong, Entry> kv in Active) UpdateLine(kv.Value);
            }
            catch (Exception) { }
        }

        private static void UpdateLine(Entry e)
        {
            if (e == null || e.Line == null || e.Spider == null || e.Target == null) return;
            PlayerControllerB local = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
            Vector3 a = e.Anchor != null ? e.Anchor.position : Mouth(e.Spider, e.Target.transform.position);
            Transform spine = (e.Target == local) ? e.Target.upperSpineLocalPoint : e.Target.upperSpine;
            Vector3 b = spine != null ? spine.position : e.Target.transform.position + Vector3.up * 1.2f;
            float reach = Mathf.Clamp01((Time.time - e.Start) / 0.3f);
            e.Line.SetPosition(0, a);
            e.Line.SetPosition(1, Vector3.Lerp(a, b, reach));
        }

        // The same animation the spider plays when it wraps a body (its "spool" animation, about 4 s).
        private static void Spool(SandSpiderAI s)
        {
            try
            {
                if (s == null || s.creatureAnimator == null) return;
                s.overrideAnimation = 4.05f;
                s.creatureAnimator.SetTrigger("spool");
            }
            catch (Exception) { }
        }

        private static void End(ulong id)
        {
            Entry e;
            if (!Active.TryGetValue(id, out e)) return;
            Active.Remove(id);
            if (e.Line != null) UnityEngine.Object.Destroy(e.Line.gameObject);
            if (e.Anchor != null) UnityEngine.Object.Destroy(e.Anchor.gameObject);
        }

        // Each frame, on every game, for the spider that owns the line.
        internal static bool IsActive(SandSpiderAI s) { return s != null && Active.ContainsKey(Id(s)); }

        internal static void Tick(SandSpiderAI s)
        {
            if (s == null || Active.Count == 0) return;
            ulong id = Id(s);
            Entry e;
            if (!Active.TryGetValue(id, out e)) return;
            if (e.Target == null || e.Target.isPlayerDead || Time.time - e.Start > 9f || s.isEnemyDead) { End(id); return; }

            PlayerControllerB local = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
            Vector3 a = Mouth(s, e.Target.transform.position);
            Transform spine = (e.Target == local) ? e.Target.upperSpineLocalPoint : e.Target.upperSpine;
            Vector3 b = spine != null ? spine.position : e.Target.transform.position + Vector3.up * 1.2f;
            float el = Time.time - e.Start;
            float reach = Mathf.Clamp01(el / 0.3f);
            UpdateLine(e);
            if (Time.time >= e.NextSpool) { e.NextSpool = Time.time + 4f; Spool(s); }

            if (e.Target == local && el > 0.35f)
            {
                Vector3 to = a - e.Target.transform.position;
                if (to.magnitude > 1.8f)
                {
                    float ru = Mathf.Clamp01((el - 0.35f) / Mathf.Max(0.5f, Plugin.WebRampSeconds.Value));
                    ru = ru * ru * (3f - 2f * ru);
                    float mult = Mathf.Lerp(0.8f, Plugin.WebPeakMultiplier.Value, ru);
                    Vector3 f = to.normalized * 7f * Plugin.WebStrength.Value * mult;
                    f.y = Mathf.Max(f.y, 0f);
                    e.Target.externalForces += f;
                }
                if (Time.time >= e.NextFear) { e.NextFear = Time.time + 1f; try { e.Target.JumpToFearLevel(1f); } catch (Exception) { } }
            }
        }
    }

    // Puts the bunker spider on the level's list of outdoor monsters, so it can turn up outside in a normal game.
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
    internal static class OutdoorSpawnPatch
    {
        private static void Prefix(SelectableLevel newLevel)
        {
            try
            {
                if (newLevel == null || newLevel.OutsideEnemies == null) return;
                int r = Mathf.RoundToInt(Plugin.OutdoorSpawnRarity.Value);
                EnemyType et = null;
                if (newLevel.Enemies != null)
                    foreach (SpawnableEnemyWithRarity e in newLevel.Enemies)
                        if (e != null && e.enemyType != null && e.enemyType.enemyPrefab != null && e.enemyType.enemyPrefab.GetComponent<SandSpiderAI>() != null) { et = e.enemyType; break; }
                if (et == null)
                    foreach (EnemyType e in Resources.FindObjectsOfTypeAll<EnemyType>())
                        if (e != null && e.enemyPrefab != null && e.enemyPrefab.GetComponent<SandSpiderAI>() != null) { et = e; break; }
                if (et == null) { Plugin.Log.LogWarning("Outdoor spawning: could not find the bunker spider's enemy type."); return; }
                SpawnableEnemyWithRarity existing = null;
                foreach (SpawnableEnemyWithRarity e in newLevel.OutsideEnemies) if (e != null && e.enemyType == et) { existing = e; break; }
                if (r <= 0)
                {
                    if (existing != null) newLevel.OutsideEnemies.Remove(existing);
                    return;
                }
                if (existing != null) existing.rarity = r;
                else newLevel.OutsideEnemies.Add(new SpawnableEnemyWithRarity(et, r));
                Plugin.Log.LogInfo("Outdoor spawning: bunker spiders can now spawn outside on '" + newLevel.PlanetName + "' (rarity " + r + ").");
            }
            catch (Exception e) { Plugin.Log.LogWarning("Outdoor spawning: " + e.Message); }
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderStalkSpeedPatch
    {
        private static int fixes;

        private static void Postfix(SandSpiderAI __instance, ref bool ___onWall, ref bool ___reachedWallPosition, ref bool ___gotWallPositionInLOS, DeadBodyInfo ___currentlyHeldBody)
        {
            try
            {
                Ceiling.StalkSpeed(__instance);
                // A roaming spider never climbs walls, and the game only clears its "on a wall" flag when the visible body reaches a
                // wall spot, which the pinned body never does. Left set, the spider cannot bite anyone (the game ignores touches while
                // "on a wall"), keeps its body turned to a wall, and sits on the player. Clear it.
                Ceiling.CS cs = Ceiling.Get(__instance, false);
                if (cs != null && cs.Roam && !__instance.isEnemyDead
                    && (cs.M == Ceiling.Mode.Walk || cs.M == Ceiling.Mode.Stalk || cs.M == Ceiling.Mode.Web || __instance.currentBehaviourStateIndex == 2)
                    && (___onWall || ___reachedWallPosition || ___gotWallPositionInLOS || __instance.lookingForWallPosition)
                    && ___currentlyHeldBody == null)
                {
                    if (___onWall && fixes++ < 20) Plugin.Log.LogInfo("Roaming spider: cleared a stuck \"on a wall\" flag (it could not bite while that was set).");
                    ___onWall = false; ___reachedWallPosition = false; ___gotWallPositionInLOS = false;
                    __instance.lookingForWallPosition = false;
                }
            }
            catch (Exception) { }
        }
    }

    // While the venom works, the spider that bit is slowed. Runs last each frame, after every other speed decision.
    // A player who is wrapped cannot be taken over by a haunted comedy/tragedy mask: the mask comes off and the takeover is cancelled
    // (otherwise the mask hangs in the air and the hidden body wanders off). The game itself strips the mask item's model when it
    // clamps on, so nothing is left lying on the floor; only the floating head mask and the takeover are removed.
    internal static class MaskCancel
    {
        private static readonly Type MaskType = AccessTools.TypeByName("HauntedMaskItem");
        private static readonly FieldInfo HeldBy = MaskType != null ? AccessTools.Field(MaskType, "previousPlayerHeldBy") : null;
        private static readonly FieldInfo Attaching = MaskType != null ? AccessTools.Field(MaskType, "attaching") : null;
        private static readonly FieldInfo Clamped = MaskType != null ? AccessTools.Field(MaskType, "clampedToHead") : null;
        private static readonly FieldInfo HeadMask = MaskType != null ? AccessTools.Field(MaskType, "currentHeadMask") : null;
        private static readonly MethodInfo Cancel = MaskType != null ? AccessTools.Method(MaskType, "CancelAttachToPlayerOnLocalClient") : null;

        internal static void Run(PlayerControllerB victim)
        {
            try
            {
                if (MaskType == null || HeldBy == null || Cancel == null || victim == null) return;
                foreach (UnityEngine.Object o in UnityEngine.Object.FindObjectsOfType(MaskType))
                {
                    if (o == null || HeldBy.GetValue(o) != (object)victim) continue;
                    bool on = (Attaching != null && (bool)Attaching.GetValue(o)) || (Clamped != null && (bool)Clamped.GetValue(o))
                        || (HeadMask != null && HeadMask.GetValue(o) as Transform != null);
                    if (!on) continue;
                    Cancel.Invoke(o, null);
                    if (Clamped != null) Clamped.SetValue(o, false);
                    Plugin.Log.LogInfo("Player " + victim.playerClientId + " was wrapped while a mask was taking them over: the takeover is cancelled and the mask comes off.");
                }
            }
            catch (Exception e) { Plugin.Dbg("Mask cancel failed: " + e.Message); }
        }
    }

    // The game logs "Player is not targetable" every time a monster touches a wrapped player, which floods the log.
    // Remove just that one log call (unless verbose logging is on). The rest of the method is untouched.
    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.MeetsStandardPlayerCollisionConditions))]
    internal static class QuietNotTargetablePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            try
            {
                if (Plugin.VerboseLogs != null && Plugin.VerboseLogs.Value) return list;
                for (int i = 0; i + 1 < list.Count; i++)
                {
                    if (list[i].opcode == System.Reflection.Emit.OpCodes.Ldstr && (list[i].operand as string) == "Player is not targetable"
                        && list[i + 1].opcode == System.Reflection.Emit.OpCodes.Call)
                    {
                        // keep any jump labels that pointed at the removed load
                        list[i + 1].opcode = System.Reflection.Emit.OpCodes.Nop; list[i + 1].operand = null;
                        list[i].opcode = System.Reflection.Emit.OpCodes.Nop; list[i].operand = null;
                        break;
                    }
                }
            }
            catch (Exception) { }
            return list;
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderVenomSlowPatch
    {
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, float[]> last = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, float[]>();
        private static bool logged;

        [HarmonyPriority(Priority.Last)]
        private static void Postfix(SandSpiderAI __instance)
        {
            try
            {
                if (__instance == null || !__instance.IsOwner || __instance.isEnemyDead) return;
                float pct = Plugin.VenomSpiderSlowPercent.Value;
                if (pct < 1f || __instance.agent == null || !Cocoon.SpiderVenomActive(__instance)) return;
                float k = 1f - pct / 100f;
                float[] l = last.GetValue(__instance, k0 => new float[2]); // [0] = speed we set, [1] = body speed we set
                if (Mathf.Abs(__instance.agent.speed - l[0]) > 0.001f || l[0] == 0f) { __instance.agent.speed *= k; l[0] = __instance.agent.speed; }
                if (Mathf.Abs(__instance.spiderSpeed - l[1]) > 0.001f || l[1] == 0f) { __instance.spiderSpeed *= k; l[1] = __instance.spiderSpeed; }
                if (!logged) { logged = true; Plugin.Log.LogInfo("Venom working: spider slowed to " + (100f - pct).ToString("0") + "% speed."); }
            }
            catch (Exception) { }
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderWebSpreadPatch
    {
        private static void Postfix(SandSpiderAI __instance, ref float ___timeSincePlacingWebTrap)
        {
            try
            {
                if (!__instance.IsOwner || __instance.isEnemyDead || __instance.currentBehaviourStateIndex != 0) return;

                float rate = Plugin.WebPlaceRateMultiplier.Value;
                if (rate > 1f && ___timeSincePlacingWebTrap <= 4f)
                {
                    ___timeSincePlacingWebTrap += Time.deltaTime * (rate - 1f); // the game adds the normal amount itself
                }

                float radius = Plugin.PatrolRadiusMultiplier.Value;
                if (radius > 0f && __instance.patrolHomeBase != null)
                {
                    __instance.patrolHomeBase.searchWidth = 17f * radius;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("SpiderWebSpreadPatch failed: " + e);
            }
        }
    }

    // Makes a waiting (motionless) spider partly see-through. The game draws the spider with an opaque
    // material that cannot be turned see-through at runtime, so this borrows one of the game's own
    // see-through materials, gives each spider a copy with the spider's own texture, and fades that.
    // If no suitable material exists the effect switches itself off and says so in the log.
    // This is purely visual and runs on every player's own game.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.Update))]
    internal static class SpiderFadePatch
    {
        private sealed class FadeState
        {
            public float Opacity = 1f;
            public float StillFor;
            public Vector3 LastPos; public bool HasLast; public float Speed; // for a joiner, which never gets the game's own averageVelocity
            public bool Swapped;
            public Material[] Originals;
            public Material[] Faded;
            public bool Dissolve;
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, FadeState> States
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, FadeState>();

        private static float nextFadeLog;
        private static int fadeSuppressed;

        // One of the see-through materials is no longer valid (destroyed, or built from a destroyed donor):
        // put the spider's own materials back and start again, looking for fresh donors.
        private static void ResetFade(FadeState st, SkinnedMeshRenderer r)
        {
            try { if (st.Originals != null && r != null) r.sharedMaterials = st.Originals; } catch (Exception) { }
            st.Originals = null; st.Faded = null; st.Swapped = false; st.Dissolve = false;
            donor = null; dissolveDonor = null;
        }

        // 1 = faded out, 0 = visible, -1 = this spider is not being faded (fade off or not set up).
        internal static int HiddenState(SandSpiderAI spider)
        {
            FadeState st;
            if (spider == null || !Plugin.WaitingFadeEnabled.Value || !States.TryGetValue(spider, out st)) return -1;
            float target = Mathf.Clamp01(Plugin.WaitingOpacityPercent.Value / 100f);
            return st.Opacity <= target + 0.05f ? 1 : 0;
        }

        private static Material donor;
        private static bool donorSearched;
        private static Material dissolveDonor;
        private static bool dissolveSearched;
        private static readonly Dictionary<Texture, Texture2D> NoisyTextures = new Dictionary<Texture, Texture2D>();

        // The Barber's material has an "alpha cutoff" that the Barber uses to dissolve itself.
        private static Material FindDissolveDonor()
        {
            if (dissolveSearched) return dissolveDonor;
            dissolveSearched = true;
            try
            {
                Material found = null;
                foreach (ClaySurgeonAI barber in Resources.FindObjectsOfTypeAll<ClaySurgeonAI>())
                {
                    if (barber != null && barber.scissorGuyMat != null && barber.scissorGuyMat.HasProperty("_AlphaCutoff")) { found = barber.scissorGuyMat; break; }
                }
                if (found == null)
                {
                    foreach (Material m in Resources.FindObjectsOfTypeAll<Material>())
                    {
                        if (m == null || !m.HasProperty("_AlphaCutoff") || !m.HasProperty("_BaseColorMap")) continue;
                        string n = m.name.ToLowerInvariant();
                        if (n.Contains("scissor") || n.Contains("barber") || n.Contains("clay")) { found = m; break; }
                    }
                }
                dissolveDonor = found;
                if (found != null) Plugin.Dbg("Spider dissolve: using the Barber's material '" + found.name + "' (shader " + found.shader.name + ").");
                else Plugin.Log.LogWarning("Spider dissolve: the Barber's material is not loaded, using the smooth fade instead.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Spider dissolve: search failed: " + e);
                dissolveDonor = null;
            }
            return dissolveDonor;
        }

        // Copy of the spider's texture with random speckle in its alpha channel, so the alpha cutoff
        // removes random pixels instead of nothing (the spider's own texture is fully opaque).
        private static Texture2D MakeNoisyTexture(Texture src)
        {
            if (src == null) return null;
            if (NoisyTextures.TryGetValue(src, out Texture2D cached)) return cached;
            Texture2D result = null;
            try
            {
                int w = Mathf.Min(src.width, 1024), h = Mathf.Min(src.height, 1024);
                RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                Graphics.Blit(src, rt);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;
                result = new Texture2D(w, h, TextureFormat.RGBA32, false);
                result.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
                Color32[] px = result.GetPixels32();
                System.Random rng = new System.Random(12345);
                for (int i = 0; i < px.Length; i++) px[i].a = (byte)(rng.Next(0, 256));
                result.SetPixels32(px);
                result.filterMode = FilterMode.Point;
                result.wrapMode = src.wrapMode;
                result.Apply(false, false);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Spider dissolve: could not build the speckle texture: " + e);
                result = null;
            }
            NoisyTextures[src] = result;
            return result;
        }

        private static Material FindDonor()
        {
            if (donorSearched) return donor;
            donorSearched = true;
            try
            {
                Material best = null;
                int bestKeywords = int.MaxValue;
                foreach (Material m in Resources.FindObjectsOfTypeAll<Material>())
                {
                    if (m == null || m.shader == null || m.shader.name != "HDRP/Lit") continue;
                    if (!m.HasProperty("_SurfaceType") || m.GetFloat("_SurfaceType") < 0.5f) continue; // must be see-through
                    if (!m.HasProperty("_BaseColorMap") || !m.HasProperty("_BaseColor")) continue;
                    if (m.renderQueue < 2900) continue;
                    string[] kw = m.shaderKeywords;
                    bool odd = false;
                    foreach (string k in kw)
                    {
                        if (k.Contains("REFRACTION") || k.Contains("DISTORTION") || k.Contains("ALPHATEST") || k.Contains("DISPLACEMENT") || k.Contains("PIXEL_DISPLACEMENT")) odd = true;
                    }
                    if (odd) continue;
                    if (kw.Length < bestKeywords)
                    {
                        best = m;
                        bestKeywords = kw.Length;
                    }
                }
                donor = best;
                if (donor != null) Plugin.Dbg("Spider fade: using the game's '" + donor.name + "' as the see-through material.");
                else Plugin.Log.LogWarning("Spider fade: the game has no suitable see-through material, so the waiting-spider fade is switched off.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Spider fade: material search failed, fade switched off: " + e);
                donor = null;
            }
            return donor;
        }

        private static void Apply(FadeState st, SkinnedMeshRenderer r, float opacity)
        {
            if (opacity >= 0.995f)
            {
                if (st.Swapped)
                {
                    r.sharedMaterials = st.Originals;
                    st.Swapped = false;
                }
                return;
            }
            if (!st.Swapped)
            {
                if (st.Originals == null)
                {
                    st.Originals = r.sharedMaterials;
                    st.Faded = new Material[st.Originals.Length];
                    for (int i = 0; i < st.Originals.Length; i++)
                    {
                        Material orig = st.Originals[i];
                        Texture origTex = (orig != null && orig.HasProperty("_BaseColorMap")) ? orig.GetTexture("_BaseColorMap") : null;
                        Texture2D noisy = (Plugin.WaitingDissolve.Value && FindDissolveDonor() != null) ? MakeNoisyTexture(origTex) : null;
                        if (noisy != null)
                        {
                            Material dm = new Material(dissolveDonor);
                            dm.name = (orig != null ? orig.name : "spider") + " (dissolve)";
                            dm.SetTexture("_BaseColorMap", noisy);
                            dm.SetTextureScale("_BaseColorMap", orig.GetTextureScale("_BaseColorMap"));
                            dm.SetTextureOffset("_BaseColorMap", orig.GetTextureOffset("_BaseColorMap"));
                            if (dm.HasProperty("_BaseColor")) dm.SetColor("_BaseColor", Color.white);
                            if (dm.HasProperty("_AlphaCutoffEnable")) dm.SetFloat("_AlphaCutoffEnable", 1f);
                            dm.EnableKeyword("_ALPHATEST_ON");
                            st.Faded[i] = dm;
                            st.Dissolve = true;
                            continue;
                        }
                        Material copy = new Material(donor);
                        copy.name = (orig != null ? orig.name : "spider") + " (faded)";
                        if (orig != null && orig.HasProperty("_BaseColorMap"))
                        {
                            copy.SetTexture("_BaseColorMap", orig.GetTexture("_BaseColorMap"));
                            copy.SetTextureScale("_BaseColorMap", orig.GetTextureScale("_BaseColorMap"));
                            copy.SetTextureOffset("_BaseColorMap", orig.GetTextureOffset("_BaseColorMap"));
                        }
                        st.Faded[i] = copy;
                    }
                }
                r.sharedMaterials = st.Faded;
                st.Swapped = true;
            }
            for (int i = 0; i < st.Faded.Length; i++)
            {
                if (st.Faded[i] == null) { ResetFade(st, r); return; }
            }
            for (int i = 0; i < st.Faded.Length; i++)
            {
                if (st.Faded[i].name.EndsWith("(dissolve)"))
                {
                    st.Faded[i].SetFloat("_AlphaCutoff", Mathf.Clamp(1f - opacity, 0f, 0.99f));
                    continue;
                }
                Material orig = st.Originals[i];
                Color c = (orig != null && orig.HasProperty("_BaseColor")) ? orig.GetColor("_BaseColor") : Color.white;
                c.a = opacity;
                st.Faded[i].SetColor("_BaseColor", c);
            }
        }

        private static void Postfix(SandSpiderAI __instance, Vector3 ___averageVelocity, DeadBodyInfo ___currentlyHeldBody, bool ___spoolingPlayerBody)
        {
            try
            {
                if (!Plugin.WaitingFadeEnabled.Value && !States.TryGetValue(__instance, out _) && Ceiling.ForcedOpacity(__instance) < 0f) return;
                SkinnedMeshRenderer body = __instance.spiderNormalMesh;
                if (body == null) return;
                if (FindDonor() == null) return;

                FadeState st = States.GetOrCreateValue(__instance);
                float forced = Ceiling.ForcedOpacity(__instance);
                if (forced >= 0f) { st.Opacity = forced; Apply(st, body, forced); return; }

                // "Waiting": motionless for a moment, and not chasing, carrying or wrapping anyone.
                // The game only works out averageVelocity on the spider's owner, so on a joiner's game it stays zero and a
                // walking spider looked "motionless" (and dissolved) all the time. There, measure how far the body really moves.
                float moving = ___averageVelocity.magnitude;
                bool remoteStill = false;
                if (!__instance.IsOwner && __instance.meshContainer != null)
                {
                    Vector3 pnow = __instance.meshContainer.position;
                    float dtm = Mathf.Max(Time.deltaTime, 0.0001f);
                    float inst = st.HasLast ? (pnow - st.LastPos).magnitude / dtm : 0f;
                    st.LastPos = pnow; st.HasLast = true;
                    st.Speed = Mathf.Lerp(st.Speed, inst, Mathf.Clamp01(8f * Time.deltaTime));
                    remoteStill = st.Speed <= 0.15f;
                }
                bool still = !__instance.isEnemyDead
                    && __instance.currentBehaviourStateIndex != 2
                    && ___currentlyHeldBody == null
                    && !___spoolingPlayerBody
                    && (__instance.IsOwner ? moving <= 0.001f : remoteStill);
                st.StillFor = still ? st.StillFor + Time.deltaTime : 0f;

                float target = 1f;
                if (Plugin.WaitingFadeEnabled.Value && st.StillFor > 1.5f)
                {
                    target = Mathf.Clamp01(Plugin.WaitingOpacityPercent.Value / 100f);
                    PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
                    float reveal = Plugin.WaitingRevealMeters.Value;
                    if (me != null && reveal > 0f && Vector3.Distance(me.transform.position, __instance.transform.position) < reveal) target = 1f;
                }

                float fadeSeconds = Mathf.Max(0.1f, Plugin.WaitingFadeSeconds.Value);
                float perSecond = (1f - Mathf.Clamp01(Plugin.WaitingOpacityPercent.Value / 100f)) / fadeSeconds;
                if (perSecond < 0.05f) perSecond = 0.05f;
                float step = (target < st.Opacity ? perSecond : perSecond * 4f) * Time.deltaTime;
                st.Opacity = Mathf.MoveTowards(st.Opacity, target, step);
                Apply(st, body, st.Opacity);
            }
            catch (Exception e)
            {
                // This runs every frame, so say it once in a while, not every time.
                fadeSuppressed++;
                if (Time.time >= nextFadeLog)
                {
                    nextFadeLog = Time.time + 10f;
                    Plugin.Log.LogError("SpiderFadePatch failed (" + fadeSuppressed + " times since the last report): " + e.GetType().Name + ": " + e.Message);
                    fadeSuppressed = 0;
                }
                try
                {
                    FadeState bad;
                    if (States.TryGetValue(__instance, out bad)) ResetFade(bad, __instance.spiderNormalMesh);
                }
                catch (Exception) { }
            }
        }
    }

    // On other players' games the "wrap it" and "hang it" messages can arrive before that game has
    // finished handing the spider the body (the game then logs "could not be found" and skips the
    // wrap or hang, so the victim never gets hung up). If the body is not there yet, wait for it.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.SpiderTurnBodyIntoWebClientRpc))]
    internal static class SpiderWrapRpcPatch
    {
        private static bool Prefix(SandSpiderAI __instance, DeadBodyInfo ___currentlyHeldBody)
        {
            try
            {
                if (__instance.IsOwner || ___currentlyHeldBody != null) return true;
                CocoonRunner.Run(Cocoon.WrapWhenReady(__instance));
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.SpiderHangBodyClientRpc))]
    internal static class SpiderHangRpcPatch
    {
        private static bool Prefix(SandSpiderAI __instance, DeadBodyInfo ___currentlyHeldBody)
        {
            try
            {
                if (__instance.IsOwner || ___currentlyHeldBody != null) return true;
                CocoonRunner.Run(Cocoon.HangWhenReady(__instance));
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
