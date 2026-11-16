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
    [BepInPlugin("Militus.HungToDry", "Hung to Dry", "1.8.1")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        // Repetitive diagnostic detail: only written when DebugLogging is on.
        internal static void Dbg(string message)
        {
            if (Log != null && DebugLogging != null && DebugLogging.Value) Log.LogInfo(message);
        }

        // All of these can be edited in BepInEx\config\Militus.HungToDry.cfg
        internal static ConfigEntry<float> HoldSeconds;      // safety net: release if the spider never hangs the body
        internal static ConfigEntry<float> HangSeconds;      // time between being hung up and dying
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
        internal static ConfigEntry<bool> WrapOnCatch;                         // wrap the victim where they are caught, then drag
        internal static ConfigEntry<float> StuckSeconds;                        // spider standing still this long while carrying = stuck
        internal static ConfigEntry<float> CarryTimeoutSeconds;                // spider carrying this long without hanging = hang where it is
        internal static ConfigEntry<float> WallReachMeters;                    // how close to its wall spot the spider must get before it hangs a victim
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
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugSpawnBrackenKey; // testing only
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugSpawnLootBugKey; // testing only
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugClearEnemiesKey; // testing only
        internal static ConfigEntry<UnityEngine.InputSystem.Key> DebugDumpBonesKey; // testing only
        internal static ConfigEntry<bool> DebugLogging;
        internal static ConfigEntry<float> SpiderFootstepVolume;

        private void Awake()
        {
            Log = Logger;
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
            DropHeldItems = Config.Bind(
                "General", "DropHeldItems", true,
                "Victims drop the items they are holding when they are caught.");
            CutDownKey = Config.Bind(
                "Rescue", "CutDownKey", UnityEngine.InputSystem.Key.E,
                "Hold this key near a hung cocoon to cut the victim down.");
            CutDownSeconds = Config.Bind(
                "Rescue", "CutDownSeconds", 4f,
                "How many seconds the key must be held to cut a victim down.");
            CarryTimeoutSeconds = Config.Bind(
                "General", "CarryTimeoutSeconds", 30f,
                "If a spider has been carrying a victim this many seconds without hanging them (for example it cannot find a way to its lair), it hangs them where it is. 0 turns this off.");
            WallReachMeters = Config.Bind(
                "General", "WallReachMeters", 0.8f,
                "How close (in metres) the spider has to get to its hanging spot on the wall before it hangs a victim. The game wants 0.1, which a spider that bumps into a wall or grate may never reach. 0 uses the game's own value.");
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
                "Testing only: lets the HOST press F6 to spawn a bunker spider, F8 a Bracken and F10 a loot bug in front of them, F11 to remove every enemy (and F7 to log nearby labels). Set this to false for normal play.");
            DebugSpawnKey = Config.Bind(
                "Testing", "DebugSpawnKey", UnityEngine.InputSystem.Key.F6,
                "The key that spawns a bunker spider (host only, needs EnableDebugSpawnKey).");
            DebugSpawnBrackenKey = Config.Bind(
                "Testing", "DebugSpawnBrackenKey", UnityEngine.InputSystem.Key.F8,
                "The key that spawns a Bracken (host only, needs EnableDebugSpawnKey).");
            DebugSpawnLootBugKey = Config.Bind(
                "Testing", "DebugSpawnLootBugKey", UnityEngine.InputSystem.Key.F10,
                "The key that spawns a loot bug (host only, needs EnableDebugSpawnKey).");
            DebugClearEnemiesKey = Config.Bind(
                "Testing", "DebugClearEnemiesKey", UnityEngine.InputSystem.Key.F11,
                "The key that removes every enemy from the level (host only, needs EnableDebugSpawnKey). Anyone hung up is freed first.");
            SpiderFootstepVolume = Config.Bind(
                "General", "SpiderFootstepVolume", 1f,
                new ConfigDescription("How loud the bunker spider's footsteps are, as a fraction of the game's normal volume. 1 = unchanged, 0.5 = half, 0 = silent. Experimental: may not take effect on every spider. Only changes what you hear on your own game.",
                    new AcceptableValueRange<float>(0f, 1f)));
            DebugLogging = Config.Bind(
                "Testing", "DebugLogging", false,
                "Writes extra, repetitive detail to the log (carry progress, notice-range checks, back-off messages). Leave off unless you are chasing a problem.");
            DebugDumpBonesKey = Config.Bind(
                "Testing", "DebugDumpBonesKey", UnityEngine.InputSystem.Key.F4,
                "The key that writes every Bracken's bone list to the log (needs EnableDebugSpawnKey). Used to work out the arm pose for the carry.");

            CocoonRunner.Ensure(); // exists from startup so the network messages are ready before the first catch
            Harmony harmony = new Harmony("Militus.HungToDry");
            harmony.PatchAll();
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
                // Spider footsteps that the game plays as one-shot sounds: turn those down as well.
                System.Reflection.MethodBase oneShot = AccessTools.Method(typeof(AudioSource), "PlayOneShot", new Type[] { typeof(AudioClip), typeof(float) });
                if (oneShot != null) harmony.Patch(oneShot, prefix: new HarmonyMethod(typeof(SpiderStepSoundPatch), "Prefix"));
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
                        Log.LogInfo("Patched EnemyAI." + name + " so monsters skip wrapped players.");
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
            Log.LogInfo("Hung to Dry 1.8.1 loaded (spider camera, death timer, cut-down rescue, anti-stall).");
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
                if (!Plugin.ShowDeathTimer.Value || Event.current.type != EventType.Repaint) return;
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
                if (Vector3.Distance(a, b2) > 3f) Cocoon.MoveVictimUnderBody(s.Player);
            }
        }

        private void Update()
        {
            SyncGhosts();
            // Emergency release: press F9 to free yourself if anything ever goes wrong.
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.f9Key.wasPressedThisFrame)
            {
                Cocoon.EmergencyRelease();
            }
            if (Plugin.EnableDebugSpawn.Value && keyboard != null)
            {
                UnityEngine.InputSystem.Controls.KeyControl spawnKey = keyboard[Plugin.DebugSpawnKey.Value];
                if (spawnKey != null && spawnKey.wasPressedThisFrame) DebugSpawn.SpawnSpiderInFrontOfHost();
                UnityEngine.InputSystem.Controls.KeyControl brackenKey = keyboard[Plugin.DebugSpawnBrackenKey.Value];
                if (brackenKey != null && brackenKey.wasPressedThisFrame) DebugSpawn.SpawnInFrontOfHost(typeof(FlowermanAI), "Bracken");
                UnityEngine.InputSystem.Controls.KeyControl bugKey = keyboard[Plugin.DebugSpawnLootBugKey.Value];
                if (bugKey != null && bugKey.wasPressedThisFrame) DebugSpawn.SpawnInFrontOfHost(typeof(HoarderBugAI), "loot bug");
                UnityEngine.InputSystem.Controls.KeyControl clearKey = keyboard[Plugin.DebugClearEnemiesKey.Value];
                if (clearKey != null && clearKey.wasPressedThisFrame) DebugSpawn.ClearAllEnemies();
                if (keyboard.f7Key.wasPressedThisFrame) DebugSpawn.DumpLabelsNearby();
                UnityEngine.InputSystem.Controls.KeyControl bonesKey = keyboard[Plugin.DebugDumpBonesKey.Value];
                if (bonesKey != null && bonesKey.wasPressedThisFrame) DebugSpawn.DumpBrackenBones();
            }
            Cocoon.RemoveDeadVictims();
            Cocoon.TickCorpseDump();
            Cocoon.TickStandIns();
            CocoonNet.EnsureRegistered();
            Cocoon.WatchSpiders();
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

        // Testing helper: removes every enemy from the level. Anyone hung up is freed first, so nobody is left
        // stuck to a spider that is about to vanish.
        internal static void ClearAllEnemies()
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer)
                {
                    Plugin.Log.LogWarning("Clear key: only the host can remove enemies.");
                    return;
                }
                foreach (int id in new List<int>(Cocoon.ActiveStates.Keys))
                {
                    Cocoon.End(id);
                    CocoonNet.RequestRelease(id);
                }
                RoundManager rm = RoundManager.Instance;
                int removed = 0;
                foreach (EnemyAI ai in UnityEngine.Object.FindObjectsOfType<EnemyAI>())
                {
                    if (ai == null) continue;
                    if (rm != null && rm.SpawnedEnemies != null) rm.SpawnedEnemies.Remove(ai);
                    Unity.Netcode.NetworkObject no = ai.NetworkObject;
                    if (no != null && no.IsSpawned) no.Despawn(true);
                    else UnityEngine.Object.Destroy(ai.gameObject);
                    removed++;
                }
                Plugin.Log.LogInfo("Clear key: removed " + removed + " enemies from the level.");
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
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Spawn key failed: " + e);
            }
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
        public bool NetWrapped;      // the stall net already made the spider wrap this victim

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

        // True for a few seconds after a release, so the victim has time to get away.
        public static bool IsImmune(PlayerControllerB player)
        {
            float until;
            return player != null
                && ImmuneUntil.TryGetValue((int)player.playerClientId, out until)
                && Time.time < until;
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
        private static void MakeSpiderGrab(SandSpiderAI spider, PlayerControllerB victim)
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
            Plugin.Log.LogInfo("Spider grabbed player " + victim.playerClientId + "'s stand-in body.");
            if (Plugin.WrapOnCatch.Value) StartWrap(spider, "at the catch");
        }

        // Runs on EVERY player's game when a spider hit lands.
        public static void Begin(PlayerControllerB victim, SandSpiderAI spider)
        {
            if (victim == null || victim.isPlayerDead) return;
            int id = (int)victim.playerClientId;
            if (Active.ContainsKey(id)) return; // already cocooned (also guards the host running this twice)

            CocoonState state = new CocoonState { Player = victim, Spider = spider, BeganAt = Time.time };
            Active[id] = state;

            // Diagnostic for the "caught again straight after a rescue" question.
            float lastImmuneUntil;
            if (ImmuneUntil.TryGetValue(id, out lastImmuneUntil))
            {
                Plugin.Log.LogInfo("Player " + id + " caught again; their immunity ended "
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
                    Plugin.Log.LogInfo("Player " + id + ": switched off " + state.Colliders.Count + " collider(s) so nobody bumps into the hidden body.");
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
                victim.SpawnDeadBody(id, Vector3.zero, (int)CauseOfDeath.Mauling, victim, 0);
                state.StandIn = victim.deadBody;

                // 5. Hand the body to the spider (only does anything on the spider owner's game).
                MakeSpiderGrab(spider, victim);

                // 6. The victim watches from a camera that follows their body.
                if (isLocal) StartCamera(state);

                Plugin.Log.LogInfo("Player " + id + " cocooned (this is my own player: " + isLocal + ").");
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

                s.Hung = true;
                s.HungAt = Time.time;
                CocoonRunner.Run(DeathAfter(kv.Key, s, Plugin.HangSeconds.Value));
                Plugin.Log.LogInfo("Player " + kv.Key + " hung up. Death in " + Plugin.HangSeconds.Value + "s.");
                break;
            }
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
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Starting the wrap failed: " + e);
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
            if (Active.TryGetValue(id, out s) && s == owner && !s.Hung)
            {
                Plugin.Log.LogInfo("Player " + id + " was never hung up: safety release.");
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

            // spawnBody: false. The hanging stand-in body is the corpse, so no second body appears.
            // Die the normal way WITH a body: the game then makes an ordinary pick-up-able corpse. First the
            // hanging stand-in is removed and the player's body is moved to where it hung, so the real corpse
            // appears there.
            bool normalBody = false;
            try
            {
                DeadBodyInfo standIn = state.StandIn != null ? state.StandIn : p.deadBody;
                if (standIn != null && standIn.bodyParts != null && standIn.bodyParts.Length > 5)
                {
                    Vector3 hip = standIn.bodyParts[5].transform.position;
                    BrackenThief.ReleaseCarry(state, standIn);
                    standIn.attachedTo = null; standIn.attachedLimb = null; standIn.matchPositionExactly = false;
                    p.thisPlayerBody.position = hip - Vector3.up * 1.32f;
                    p.transform.position = p.thisPlayerBody.position;
                    UnityEngine.Object.Destroy(standIn.gameObject);
                    if (p.deadBody == standIn) p.deadBody = null;
                    normalBody = true;
                }
            }
            catch (Exception e) { Plugin.Log.LogWarning("Could not swap the stand-in for a normal body: " + e.Message); }
            p.KillPlayer(Vector3.zero, normalBody, CauseOfDeath.Mauling, 0);
            if (p.isPlayerDead && !normalBody) EnsureCorpseGrabbable(state); // does nothing unless this game is the host

            // If something blocked the death (Imperium's god mode, the game's "allow death" switch, another mod),
            // the player is still alive but our camera is already off: they would walk around seeing nothing.
            // Give them their own camera back and free them from the cocoon instead.
            if (!p.isPlayerDead)
            {
                Plugin.Log.LogWarning("Player " + id + " did not die: the game or another mod (for example Imperium's god mode) blocked it. Freeing them from the cocoon instead.");
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
                if (teleportToBody && state.Hung && IsLocal(p))
                {
                    MoveVictimUnderBody(p);
                }
                BrackenThief.ReleaseCarry(state, p.deadBody);
                Transform hangPoint = p.deadBody.attachedTo;
                UnityEngine.Object.Destroy(p.deadBody.gameObject);
                p.deadBody = null;
                if (state.Hung && hangPoint != null) DestroyRope(hangPoint);
            }
            Plugin.Log.LogInfo("Player " + id + " released (this is my own player: " + IsLocal(p) + ").");
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

                if (s.Hung || spider == null || spider.isEnemyDead || !spider.IsOwner || s.Player == null) continue;

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
                            Plugin.Log.LogWarning("Spider dropped player " + s.Player.playerClientId + " without hanging them: releasing them.");
                            s.LostSince = -1f;
                            CocoonNet.RequestRelease((int)s.Player.playerClientId);
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

        // Runs on EVERY player's game when someone finishes cutting a victim down.
        public static void ApplyRelease(int id)
        {
            CocoonState s;
            if (!Active.TryGetValue(id, out s)) return;
            Plugin.Log.LogInfo("Player " + id + " was cut down.");
            End(id, true);
        }

        // The freed player's real body is wherever they were hit, so bring them to the cocoon.
        internal static void MoveVictimUnderBody(PlayerControllerB p)
        {
            try
            {
                Vector3 pos = p.deadBody.bodyParts[6].transform.position;
                // Land on the nearest AI node (the points the monsters themselves walk between): they are
                // always on safe, walkable ground, unlike the floor under a carried cocoon (a pit, say).
                GameObject[] nodes = RoundManager.Instance != null
                    ? (p.isInsideFactory ? RoundManager.Instance.insideAINodes : RoundManager.Instance.outsideAINodes)
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
                    Plugin.Log.LogInfo("No AI node found near the cocoon; the hidden player stays where they are.");
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
        private static int cutTarget = -1;
        private static float cutProgress;
        private static float lastRequestTime = -10f;
        private static bool promptShown;

        public static void UpdateRescue()
        {
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
                    if (!s.Hung || s.Player == null || s.Player == me) continue;
                    DeadBodyInfo body = s.Player.deadBody;
                    if (body == null || body.bodyParts == null || body.bodyParts.Length <= 6) continue;
                    float d = Vector3.Distance(eye, body.bodyParts[6].transform.position);
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

            float need = Mathf.Max(0.5f, Plugin.CutDownSeconds.Value);
            int percent = Mathf.Min(100, Mathf.RoundToInt(cutProgress / need * 100f));
            me.cursorTip.text = held
                ? "Cutting them down... " + percent + "%" + TimeSuffix(target)
                : "[Hold " + Plugin.CutDownKey.Value + "] Cut them down" + TimeSuffix(target);
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
                            UnityEngine.Object.Destroy(s.StandIn.gameObject);
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
                        UnityEngine.Object.Destroy(s.StandIn.gameObject);
                        PendingStandIns.RemoveAt(i);
                    }
                    else if (Time.time > s.StandInDeadline)
                    {
                        PendingStandIns.RemoveAt(i);
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

    // Tiny custom messages so a rescue reaches every player's game.
    // A client asks the host; the host tells everyone (including itself) to release the victim.
    internal static class CocoonNet
    {
        private const string RequestName = "SpiderCocoon.RequestRelease";
        private const string ReleaseName = "SpiderCocoon.Release";
        private const string StealRequestName = "SpiderCocoon.RequestSteal";
        private const string StealName = "SpiderCocoon.Steal";

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
                Plugin.Log.LogInfo("Bracken noticed a hung victim but another player is within " + Plugin.BrackenScaredMeters.Value.ToString("F0") + " m of them.");
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
                Plugin.Log.LogInfo("Bracken cannot reach the hung victim.");
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
                    Plugin.Log.LogInfo("Bracken carries the cocoon in '" + style.Trim() + "' style.");
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
                    Plugin.Log.LogInfo("Bracken is already out of sight " + dHere.ToString("F1") + " m from the victim and waits there.");
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
                            Plugin.Log.LogInfo("Bracken will hide out of sight (" + found.Count + " hiding spots between " + min.ToString("F0") + " and " + max.ToString("F0") + " m) and wait for the spider to finish.");
                            return true;
                        }
                    }
                    Plugin.Log.LogInfo("Bracken hiding search " + min.ToString("F0") + "-" + max.ToString("F0") + " m: " + inRange + " nodes in range, "
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

            PlayerControllerB player = __instance.MeetsStandardPlayerCollisionConditions(other, ___spoolingPlayerBody);

            // Ignore players who are already wrapped up (or just released).
            if (player != null && !Cocoon.IsCocooned(player) && !Cocoon.IsImmune(player) && ___timeSinceHittingPlayer > 1f)
            {
                ___timeSinceHittingPlayer = 0f;
                Plugin.Log.LogInfo("Spider hit intercepted: no damage, cocooning player.");

                // Keeps the attack animation + sound, and tells every player's game about the hit.
                __instance.HitPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
            }
            return false;
        }
    }

    // STAGE 2: the game already tells every player's game "the spider hit player X"
    // through this method. We piggyback on it, so no custom networking is needed.
    [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.HitPlayerClientRpc))]
    internal static class SpiderHitSyncPatch
    {
        private static void Postfix(SandSpiderAI __instance, int playerId)
        {
            try
            {
                PlayerControllerB[] players = StartOfRound.Instance.allPlayerScripts;
                if (playerId < 0 || playerId >= players.Length) return;
                Cocoon.Begin(players[playerId], __instance);
            }
            catch (Exception e)
            {
                // Never let our code break the game's own network message.
                Plugin.Log.LogError("SpiderHitSyncPatch failed: " + e);
            }
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

    // Spider footsteps played as one-shot sounds: scale them by the footstep volume setting.
    internal static class SpiderStepSoundPatch
    {
        static int logged;
        internal static void Prefix(AudioSource __instance, AudioClip clip, ref float volumeScale)
        {
            try
            {
                if (clip == null || __instance == null) return;
                string n = clip.name;
                bool stepLike = n.IndexOf("step", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("foot", StringComparison.OrdinalIgnoreCase) >= 0;
                if (!stepLike && !Plugin.DebugLogging.Value) return;
                if (__instance.GetComponentInParent<SandSpiderAI>() == null) return;
                if (Plugin.DebugLogging.Value && logged < 30)
                {
                    logged++;
                    Plugin.Dbg("Spider one-shot sound: clip='" + n + "' source='" + __instance.gameObject.name + "' scale=" + volumeScale.ToString("0.00") + " stepLike=" + stepLike);
                }
                if (stepLike && !SpiderQuietStepsPatch.IsHandled(__instance)) volumeScale *= Mathf.Clamp01(Plugin.SpiderFootstepVolume.Value);
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
                FieldInfo tf = typeof(CentipedeAI).GetField("triggeredFall", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (me == null || tf == null) return;
                if (Cocoon.IsCocooned(me)) { tf.SetValue(flea, true); FleaHeld.Add(flea); }
                else if (FleaHeld.Remove(flea)) tf.SetValue(flea, false);
            }
            catch (Exception) { }
        }

        internal static void Prefix(EnemyAI __instance) { HoldFlea(__instance); }

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
                        if (a != null && seen.Add(a)) { f.Sources.Add(a); f.BaseVolumes.Add(a.volume); Handled.Add(a); }
                    }
                    // 2. audio sources on the spider whose own name has "foot"/"step" in it
                    foreach (AudioSource a in __instance.GetComponentsInChildren<AudioSource>(true))
                    {
                        if (a != null && LooksLikeSteps(a.gameObject.name) && seen.Add(a)) { f.Sources.Add(a); f.BaseVolumes.Add(a.volume); Handled.Add(a); }
                    }
                    if (Plugin.DebugLogging.Value)
                    {
                        foreach (AudioSource a in __instance.GetComponentsInChildren<AudioSource>(true))
                        {
                            Plugin.Dbg("Spider audio source: '" + a.gameObject.name + "' volume=" + a.volume.ToString("0.00") +
                                " clip=" + (a.clip != null ? a.clip.name : "-") + " quietened=" + seen.Contains(a));
                        }
                    }
                    Plugin.Log.LogInfo("Spider footsteps: found " + f.Sources.Count + " audio source(s) to turn down.");
                }
                float mul = Mathf.Clamp01(Plugin.SpiderFootstepVolume.Value);
                for (int i = 0; i < f.Sources.Count; i++)
                {
                    if (f.Sources[i] != null) f.Sources[i].volume = f.BaseVolumes[i] * mul;
                }
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
            public bool Swapped;
            public Material[] Originals;
            public Material[] Faded;
            public bool Dissolve;
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, FadeState> States
            = new System.Runtime.CompilerServices.ConditionalWeakTable<SandSpiderAI, FadeState>();

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
                if (found != null) Plugin.Log.LogInfo("Spider dissolve: using the Barber's material '" + found.name + "' (shader " + found.shader.name + ").");
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
                if (donor != null) Plugin.Log.LogInfo("Spider fade: using the game's '" + donor.name + "' as the see-through material.");
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
                if (!Plugin.WaitingFadeEnabled.Value && !States.TryGetValue(__instance, out _)) return;
                SkinnedMeshRenderer body = __instance.spiderNormalMesh;
                if (body == null) return;
                if (FindDonor() == null) return;

                FadeState st = States.GetOrCreateValue(__instance);

                // "Waiting": motionless for a moment, and not chasing, carrying or wrapping anyone.
                bool still = !__instance.isEnemyDead
                    && __instance.currentBehaviourStateIndex != 2
                    && ___currentlyHeldBody == null
                    && !___spoolingPlayerBody
                    && ___averageVelocity.magnitude <= 0.001f;
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
                Plugin.Log.LogError("SpiderFadePatch failed: " + e);
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
