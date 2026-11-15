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
    [BepInPlugin("Militus.HungToDry", "Hung to Dry", "1.6.6")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        // All of these can be edited in BepInEx\config\Militus.HungToDry.cfg
        internal static ConfigEntry<float> HoldSeconds;      // safety net: release if the spider never hangs the body
        internal static ConfigEntry<float> HangSeconds;      // time between being hung up and dying
        internal static ConfigEntry<float> ImmunitySeconds;  // only used if a victim is released
        internal static ConfigEntry<UnityEngine.InputSystem.Key> CutDownKey;   // hold this near a hung cocoon to rescue
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
                "Seconds after a release during which the spider cannot cocoon the player again.");
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
            StuckSeconds = Config.Bind(
                "General", "StuckSeconds", 12f,
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
                "Testing only: lets the HOST press F6 to spawn a bunker spider in front of them (and F7 to log nearby labels). Leave this off for normal play.");
            DebugSpawnKey = Config.Bind(
                "Testing", "DebugSpawnKey", UnityEngine.InputSystem.Key.F6,
                "The key that spawns a bunker spider (host only, needs EnableDebugSpawnKey).");

            CocoonRunner.Ensure(); // exists from startup so the network messages are ready before the first catch
            Harmony harmony = new Harmony("Militus.HungToDry");
            harmony.PatchAll();
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
            Log.LogInfo("Hung to Dry 1.6.6 loaded (spider camera, death timer, cut-down rescue, anti-stall).");
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

        private void Update()
        {
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
                if (keyboard.f7Key.wasPressedThisFrame) DebugSpawn.DumpLabelsNearby();
            }
            Cocoon.RemoveDeadVictims();
            CocoonNet.EnsureRegistered();
            Cocoon.WatchSpiders();
        }

        private void LateUpdate()
        {
            Cocoon.UpdateCameras();
            Cocoon.UpdateRescue();
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

        internal static void SpawnSpiderInFrontOfHost()
        {
            try
            {
                Unity.Netcode.NetworkManager nm = Unity.Netcode.NetworkManager.Singleton;
                if (nm == null || !nm.IsServer)
                {
                    Plugin.Log.LogWarning("Spawn key: only the host can spawn a spider.");
                    return;
                }
                PlayerControllerB me = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
                if (me == null)
                {
                    Plugin.Log.LogWarning("Spawn key: no local player yet.");
                    return;
                }

                EnemyType spiderType = null;
                foreach (EnemyType t in Resources.FindObjectsOfTypeAll<EnemyType>())
                {
                    if (t != null && t.enemyPrefab != null && t.enemyPrefab.GetComponentInChildren<SandSpiderAI>() != null)
                    {
                        spiderType = t;
                        break;
                    }
                }
                if (spiderType == null)
                {
                    Plugin.Log.LogWarning("Spawn key: could not find the bunker spider type.");
                    return;
                }

                Vector3 pos = me.transform.position + me.transform.forward * 7f;
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 8f, UnityEngine.AI.NavMesh.AllAreas)) pos = hit.position;

                GameObject go = UnityEngine.Object.Instantiate(spiderType.enemyPrefab, pos, Quaternion.identity);
                Unity.Netcode.NetworkObject netObj = go.GetComponentInChildren<Unity.Netcode.NetworkObject>();
                if (netObj == null)
                {
                    UnityEngine.Object.Destroy(go);
                    Plugin.Log.LogWarning("Spawn key: spider prefab has no network object.");
                    return;
                }
                netObj.Spawn(true);
                EnemyAI ai = go.GetComponentInChildren<EnemyAI>();
                if (ai != null && RoundManager.Instance != null && RoundManager.Instance.SpawnedEnemies != null)
                {
                    RoundManager.Instance.SpawnedEnemies.Add(ai);
                }
                Plugin.Log.LogInfo("Spawn key: bunker spider spawned at " + pos + ".");
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

        // Used to notice a spider that has stopped moving while carrying this victim.
        public float BeganAt;
        public bool DiagLogged;
        public float LostSince = -1f;
        public Vector3 LastSpiderPos;
        public float StillSince = -1f;

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

        private static bool IsLocal(PlayerControllerB p)
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
                CocoonRunner.Run(DeathAfter(kv.Key, s, Plugin.HangSeconds.Value));
                Plugin.Log.LogInfo("Player " + kv.Key + " hung up. Death in " + Plugin.HangSeconds.Value + "s.");
                break;
            }
        }

        private static readonly MethodInfo WrapMethod = AccessTools.Method(typeof(SandSpiderAI), "turnBodyIntoWeb");

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
            yield return new WaitForSeconds(seconds);
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
            p.KillPlayer(Vector3.zero, false, CauseOfDeath.Mauling, 0);
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
        private static void MoveVictimUnderBody(PlayerControllerB p)
        {
            try
            {
                Vector3 pos = p.deadBody.bodyParts[6].transform.position;
                RaycastHit hit;
                if (Physics.Raycast(pos, Vector3.down, out hit, 25f,
                        StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
                {
                    pos = hit.point + Vector3.up * 0.3f;
                }
                p.TeleportPlayer(pos);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Could not move the freed player to the cocoon: " + e);
            }
        }

        // Removes the web line the body was hanging from.
        private static void DestroyRope(Transform hangPoint)
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
                ? "Cutting them down... " + percent + "%"
                : "[Hold " + Plugin.CutDownKey.Value + "] Cut them down";
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
                    Active.Remove(id);
                }
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

    // The game shows a player's floating name badge whenever you look at their body. A wrapped-up
    // player's real (invisible) body is still standing where they were caught, so stop the badge
    // being shown at the source instead of trying to hide it afterwards.
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ShowNameBillboard))]
    internal static class NameBadgePatch
    {
        private static bool Prefix(PlayerControllerB __instance)
        {
            return !Cocoon.IsCocooned(__instance);
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
                if (__result && __instance is SandSpiderAI && __args != null && __args.Length > 0)
                {
                    PlayerControllerB player = __args[0] as PlayerControllerB;
                    // Wrapped-up players, and freed players during their immunity, are invisible to the spider's
                    // AI. Otherwise it hovers next to a freed player and re-catches them the moment it ends.
                    if (Cocoon.IsCocooned(player) || Cocoon.IsImmune(player)) __result = false;
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
                    if (d >= 0.1f) Plugin.Log.LogInfo("Spider wall spot accepted at " + d.ToString("0.00") + " m (the game alone would have wanted 0.10 m).");
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
