using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using BepInEx.Logging;
using System.Globalization;
using System.Runtime.InteropServices;
using GameEvent;
using System.Runtime.CompilerServices;

[assembly: AssemblyVersion("1.2.0")]
[assembly: AssemblyInformationalVersion("1.2.0")]

namespace RespawnControl
{
    [BepInPlugin("com.Milena.RespawnControl", "RespawnControl", "1.2.0")]
    public class RespawnControlMod : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        // Challenge
        public static ConfigEntry<KeyCode> InstantRespawnKey;
        public static ConfigEntry<bool> InstantRetryOnDeath;
        public static ConfigEntry<float> DelayDeath;
        public static ConfigEntry<float> DramaticDeath;
        public static ConfigEntry<bool> DramaticDeathKey;
        public static ConfigEntry<float> ChallengeDeathFreeze;
        // Freeplay
        public static ConfigEntry<KeyCode> RespawnKey;
        public static ConfigEntry<float> FastRespawn;
        public static ConfigEntry<float> StartInvincibility;
        public static ConfigEntry<KeyCode> HoldInvincibility;
        public static ConfigEntry<bool> CannonRespawn;
        public static ConfigEntry<bool> PortalRespawn;
        public static ConfigEntry<float> SwitchTime;
        public static ConfigEntry<float> DeathFreeze;
        public static ConfigEntry<bool> CyclePersistence;
        public static ConfigEntry<bool> ResetCrumbleOnDeath;
        public static ConfigEntry<bool> SmartInvincibility;
        // Multiplayer
        public static ConfigEntry<int> CountDown;
        public static ConfigEntry<bool> Deathlink;
        public static ConfigEntry<bool> PlayerCollisionFix;
        public static ConfigEntry<bool> MultiplayerRespawnFix;
        public static ConfigEntry<bool> ManualPartyRespawn;
        // Other
        public static ConfigEntry<bool> NoSuicideNote;

        void Awake()
        {
            Log = Logger;
            // Challenge
            InstantRespawnKey = Config.Bind("Challenge", "Instant Respawn", KeyCode.B, "By pressing this key, you will instantly respawn in challenge mode");
            InstantRetryOnDeath = Config.Bind("Challenge", "Instant Retry On Death", false, "By enabling this option, you will instantly retry on death! (no scoreboard popup)");
            DelayDeath = Config.Bind("Challenge", "Delay Auto Retry by", 0f, "Useful to have more time trying to figure out what and where you died (max delay = 3 seconds)");
            DramaticDeath = Config.Bind("Challenge", "No Auto Retry after", 60f, "If the in-game Timer is higher than this number and you die, it won't auto retry (useful to not miss poss-mortems while still having a fast respawntime at the start of the level, or have more time to figure out what you died to)");
            DramaticDeathKey = Config.Bind("Challenge", "No Accidental Instant Respawn", false, "If true, this will make it so you don't die when you press your instant respawn key after the ingame timer is higher than 'No Auto Retry after' (useful if you accidently press your instant respawn key while on a far run)");
            // Freeplay
            RespawnKey = Config.Bind("Freeplay", "Respawn Key for Freeplay", KeyCode.None, "By pressing this key, you will instantly respawn in Freeplay");
            FastRespawn = Config.Bind("Freeplay", "Respawn Time for Freeplay", 0.5f, "Change the time that takes to respawn in freeplay, do note that values lower than 1 are currently unstable in multiplayer lobby!");
            StartInvincibility = Config.Bind("Freeplay", "Invincibility on Spawn", 1f, "Change how long the spawn invincibility lasts");
            HoldInvincibility = Config.Bind("Freeplay", "Invincibility Key", KeyCode.None, "While you hold this key, you will have invincibility");
            CannonRespawn = Config.Bind("Freeplay", "Respawn When Dead in Cannon", true, "When you enter a cannon while dead, you will automatically get respawned, also useful to not get teleported back to the cannon after respawn in multiplayer lobbies, disable this if you need to enter a cannon while dead");
            PortalRespawn = Config.Bind("Freeplay", "Respawn When Dead in Portal", true, "When you enter a portal while dead, you will automatically get respawned also activates portals faster once you respawn, this may be a bit buggy but should be stable enough, disable this if buggy or you need to enter a portal while dead");
            SwitchTime = Config.Bind("Freeplay", "Switch Mode Time", 0.5f, "Change how long it takes to switch from build mode to play mode, and vice versa");
            DeathFreeze = Config.Bind("Freeplay", "Death Freeze Timer", 0.25f, "Change how long you remain frozen right after hitting a hazard (Values lower than 0.05 may cause silly things)");
            CyclePersistence = Config.Bind("Freeplay", "Cycle Persistence", false, "When enabled, prevents the cycles from getting reset every death (you can press 'Shift + " + RespawnKey.Value + "' To respawn with the cycles reset)");
            ResetCrumbleOnDeath = Config.Bind("Freeplay", "Reset Crumble on Death", true, "Only takes effect if Cycle Persistence is enabled");
            SmartInvincibility = Config.Bind("Freeplay", "Smart Invincibility", true, "If your spawn invincibility timer runs out while you still are on a hazard, you will continue invincible");
            // Multiplayer
            CountDown = Config.Bind("Multiplayer", "Challenge Countdown Time", 3, "Change how long the Challenge Countdown sequence lasts (Acceptable Range: 0-9)");
            Deathlink = Config.Bind("Multiplayer", "Challenge Deathlink", false, "If the host has this enabled, then when one player dies, everyone dies, if you're not the host and have this enabled, then whenever someone else dies, you will die ('No Auto Retry after' affects this)");
            PlayerCollisionFix = Config.Bind("Multiplayer", "Freeplay Player Collision Fix", true, "When enabled, you will not die from dead bodies (useful to practice coop maps)");
            MultiplayerRespawnFix = Config.Bind("Multiplayer", "Freeplay Respawn Fix (Experimental)", false, "When enabled, you should start respawning like normal in multiplayer freeplay, enable this if you can't naturally respawn in freeplay");
            ManualPartyRespawn = Config.Bind("Multiplayer", "Party Manual Respawn", false, "If you're the host and have this enabled, you can respawn other non-local players in an online lobby by pressing 'Ctrl + Shift + [PlayerNumber]' (You can only respawn local players if you're in a local lobby)");
            // Other
            NoSuicideNote = Config.Bind("Other", "No Suicide Note", false, "Removes the 'Hold B to retry' message");

            Debug.Log("RespawnControl mod loaded!");
            new Harmony("com.Milena.RespawnControl").PatchAll();
        }
        
        void Update()
        {
            if (FixDeath == true)
            {
                FixDeathTimer += Time.deltaTime;
            }
            else
            {
                FixDeathTimer = 0f;
            }
        }

        public static bool deathcycle;
        public static bool FixDeath = false;
        public static float FixDeathTimer;
        public static Character cha;
        public static bool ResetCycle = false;
        public static bool Smart = false;
        private static bool InChallengeMode() {
            try {
                return GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE;
            } catch { return false; }
        }
        private static bool InFreeplayMode() {
            try {
                return GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY;
            } catch { return false; }
        }
        private static bool InPartyMode() {
            try {
                return GameSettings.GetInstance().GameMode == GameState.GameMode.PARTY;
            } catch { return false; }
        }
        public static bool InTreehouse()
        {
            return SceneManager.GetActiveScene().name == "TreeHouseLobby";
        }
        private static bool CheckTimer()
        {
            var Challenge = GameObject.Find("ChallengeController(Clone)");
            var Timer = Challenge?.GetComponent<ChallengeControl>();

            float runTime = Traverse.Create(Timer).Field("runTime").GetValue<float>();

            try {
                return runTime >= DramaticDeath.Value;
            } catch { return false; }
        }
        private static bool CheckStart()
        {
            var Challenge = GameObject.Find("ChallengeController(Clone)");
            var Timer = Challenge?.GetComponent<ChallengeControl>();

            try {
                return Timer.runStarted;
            } catch { return false; }
        }
        private static bool CheckChat()
        {
            try {
                var Chat = GameObject.Find("ChatSystemPrefab(Clone)");
                var Display = Chat?.GetComponent<ChatDisplay>();
                return Display.ChatMode;
            } catch { return false; }
        }
        public static void OnFixedRespawn()
        {
            foreach (Teleporter teleporter in Teleporter.AllTeleporters)
            {
                if (teleporter != null && teleporter.ChrInTeleport != null && teleporter.ChrInTeleport == cha)
                {
                    teleporter.CancelTeleport();
                }
            }
        }
        public static bool IsLocal()
        {
            var lobby = GameObject.Find("LobbyManager");
            var check = lobby.GetComponent<LobbyManager>();
            return check.AllLocal;
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class InstantChallengeKey
        {
            static void Prefix(Character __instance)
            {
                if (Input.GetKeyDown(InstantRespawnKey.Value))
                {
                    if (!InChallengeMode()) return;
                    if (InTreehouse()) return;
                    if (CheckTimer() && DramaticDeathKey.Value) return;
                    if (!CheckStart()) return;
                    if (CheckChat()) return;
                    if (__instance.LocalPlayer == null) return;
                    if (__instance.Success) return;
                    if (__instance.Paused) return;
                    __instance.dead = true;
                    __instance.NetworkWantsToRetry = true;
                }
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class InstantRetry
        {
            static void Prefix(Character __instance)
            {
                if (!InstantRetryOnDeath.Value) return;
                if (!InChallengeMode()) return;
                if (CheckTimer())
                {
                    if (Deathlink.Value)
                    {
                        deathcycle = true;   
                    }
                    return;
                }
                bool dead = !__instance.Success && (__instance.Dead || __instance.Dying || __instance.LocallyDead);
                if (dead && deathcycle == false && Deathlink.Value)
                {
                    if (DelayDeath.Value >= 0.25f && __instance.deathSettleTimer > __instance.deathTimer && __instance.deathTimer == 0f)
                    {
                        __instance.deathTimer += __instance.deathSettleTimer;
                    }
                    if (DelayDeath.Value > 2f)
                    {
                        __instance.minDeathDelay = Mathf.Clamp(DelayDeath.Value, 0f, 3f) + 0.1f;
                    }
                    if (__instance.deathTimer >= Mathf.Clamp(DelayDeath.Value, 0f, 3f))
                    {
                        Character[] array = GameObject.FindObjectsOfType<Character>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            array[i].NetworkWantsToRetry = true;
                            array[i].Networkdead = false;
                            array[i].Networkdying = false;
                            array[i].dead = false;
                            array[i].dying = false;
                            array[i].LocallyDead = false;
                        }
                        deathcycle = true;
                    }
                }
                if (dead && !__instance.NetworkWantsToRetry)
                {
                    if (DelayDeath.Value >= 0.25f && __instance.deathSettleTimer > __instance.deathTimer && __instance.deathTimer == 0f)
                    {
                        __instance.deathTimer += __instance.deathSettleTimer;
                    }
                    if (DelayDeath.Value > 2f)
                    {
                        __instance.minDeathDelay = Mathf.Clamp(DelayDeath.Value, 0f, 3f) + 0.1f;
                    }
                    if (__instance.deathTimer >= Mathf.Clamp(DelayDeath.Value, 0f, 3f))
                    {
                        __instance.NetworkWantsToRetry = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class FastFPRespawn
        {
            static void Prefix(Character __instance)
            {
                if (__instance.LocalPlayer == null) return;
                if (!__instance.Success && (__instance.Dead || __instance.Dying || __instance.LocallyDead || __instance.Networkdead || __instance.Networkdying))
                {
                    if (!InFreeplayMode())
                    {
                        __instance.maxDeathDelay = 5f;
                        __instance.minDeathDelay = 2f;
                        __instance.minSkipDeathTime = 1f;
                    }
                    else
                    {
                        if (__instance.deathSettleTimer > __instance.deathTimer && __instance.deathTimer == 0f)
                        {
                            __instance.deathTimer += __instance.deathSettleTimer;
                        }
                        if (__instance.LastDeath != "FPSuicide")
                        {
                            __instance.maxDeathDelay = FastRespawn.Value;
                            __instance.minDeathDelay = FastRespawn.Value;
                            __instance.minSkipDeathTime = FastRespawn.Value;
                        }
                        if (MultiplayerRespawnFix.Value && (__instance.deathTimer >= FastRespawn.Value))
                        {
                            __instance.LocallyDead = true;
                            __instance.Networkdead = true;
                        }
                        if (__instance.inCannon == true)
                        {
                            if (!CannonRespawn.Value) return;
                            if (FixDeath == false)
                            {
                                FixDeathTimer = __instance.deathTimer;
                                FixDeath = true;
                            }
                        }
                        if (FixDeathTimer >= __instance.maxDeathDelay)
                        {
                            FixDeath = false;
                            OnFixedRespawn();
                            __instance.CallCmdRespawn();
                        }
                    }
                }
                if (SmartInvincibility.Value)
                {
                    if (!InFreeplayMode() || InTreehouse()) return;
                    if (!__instance.LowerBodyCollider.Hazard && !__instance.UpperBodyCollider.Hazard)
                    {
                        Smart = false;
                    }
                    else if (__instance.Invincible && __instance.invincibleTimer < 0.4f && (__instance.LowerBodyCollider.Hazard || __instance.UpperBodyCollider.Hazard))
                    {
                        Smart = true;
                    }
                    if (Smart && __instance.invincibleTimer < 0.05f && __instance.invincibleTimer > -0.1f)
                    {
                        __instance.invincibleTimer = 0.04f;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class FreeplayRespawn
        {
            static void Prefix(Character __instance)
            {
                if (Input.GetKeyDown(RespawnKey.Value))
                {
                    if (!InFreeplayMode()) return;
                    if (InTreehouse()) return;
                    if (CheckChat()) return;
                    if (__instance.LocalPlayer == null) return;
                    if (__instance.Paused) return;
                    if (__instance.Dead || __instance.Dying || __instance.LocallyDead)
                    {
                        var playercursor = __instance.associatedGamePlayer.CursorInstance;
                        var cursor = playercursor.GetComponent<PiecePlacementCursor>();
                        if (!cursor.Frozen && !cursor.Enabled)
                        {
                            __instance.LocallyDead = true;
                            __instance.Networkdead = true;
                        }
                    }
                    else
                    {
                        __instance.LastDeath = "FPSuicide";
                        __instance.maxDeathDelay = 0.2f;
                        __instance.dying = true;
                        __instance.Networkdying = true;
                    }
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        ResetCycle = true;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.StartInvincibleTimer))]
        public class SpawnInvincibility
        {
            static void Postfix(Character __instance)
            {
                if (InFreeplayMode())
                {
                    if (__instance.LocalPlayer == null) return;
                    __instance.invincibleTimer = StartInvincibility.Value;
                    __instance.freezeDeath = DeathFreeze.Value;
                    if (PortalRespawn.Value)
                    {
                        foreach (Teleporter teleporter in Teleporter.AllTeleporters)
                        {
                            if (teleporter.coolDownTimer > 0.7f)
                            teleporter.coolDownTimer = 0.7f;
                        }
                    }
                }
                if (NoSuicideNote.Value && !InFreeplayMode())
                {
                    GameControl Game = LobbyManager.instance.CurrentGameController;
                    Transform note = Game.transform.Find("UiCamera/SuicideNote(Clone)");
                    var Suicide = note.GetComponent<SuicideNote>();
                    Suicide.MaxRunTime = 360000f;
                }
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class InvincibilityKey
        {
            static void Prefix(Character __instance)
            {
                if (__instance.LocalPlayer == null) return;
                if (Input.GetKey(HoldInvincibility.Value))
                {
                    if (!InFreeplayMode()) return;
                    __instance.invincibleTimer = 1f;
                }
                if (Input.GetKeyUp(HoldInvincibility.Value))
                {
                    __instance.invincibleTimer = -1f;
                }
            }
        }
        [HarmonyPatch(typeof(ChallengeControl), nameof(ChallengeControl.startRun))]
        static class CountdownChange
        {
            static void Postfix(ChallengeControl __instance)
            {
                __instance.OnlineCountdownTime = Mathf.Clamp(CountDown.Value, 0, 9);
                deathcycle = false;
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.StartInvincibleTimer))]
        static class CollisionFix
        {
            static void Postfix()
            {
                if (PlayerCollisionFix.Value && InFreeplayMode() && !InTreehouse())
                {
                    Character[] array = GameObject.FindObjectsOfType<Character>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        Transform deadbody = array[i].transform.Find("Sprite/DeadCollider/PlayerPlayerColliderDead");
                        var oldtag = deadbody?.GetComponent<CollisionTag>();
                        int Bodytag = 65536;
                        oldtag.bitMask = (TagComparer.Tag)Bodytag;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Teleporter), nameof(Teleporter.OnTriggerEnter2D))]
        static class TeleporterUnstuck
        {
            static void Postfix(Collider2D c, Teleporter __instance)
            {
                Character chr = c.transform.parent.GetComponent<Character>();
                if (chr == null)
				{
					chr = c.transform.parent.parent.GetComponent<Character>();
				}
                if (!chr.Success && (chr.Dying || chr.Dead || chr.LocallyDead))
                {
                    if (!InFreeplayMode()) return;
                    if (!PortalRespawn.Value) return;
                    if (FixDeath == false)
                    {                  
                        FixDeath = true;
                        FixDeathTimer = chr.deathTimer;
                        cha = chr;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class ManualRespawns
        {
            static void Prefix(Character __instance)
            {
                if (__instance.LocalPlayer == null) return;
                if (__instance.networkNumber != 1) return;
                if (!InPartyMode()) return;
                if (InTreehouse()) return;
                if (!ManualPartyRespawn.Value) return;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        int number = 0;
                        for (int i = 1; i <= 9; i++)
                        {
                            if (Input.GetKeyDown((KeyCode)(49 + (i - 1))) || Input.GetKeyDown((KeyCode)(257 + (i - 1))))
                            {
                                number = i;
                                break;
                            }
                        }
                        if (number != 0)
                        {
                            Character[] array = GameObject.FindObjectsOfType<Character>();
                            for (int i = 0; i < array.Length; i++)
                            {
                                if (array[i].networkNumber == number && (array[i].dead || array[i].Networkdead || array[i].dying || array[i].Networkdying))
                                {
                                    if (array[i].LocalPlayer != null)
                                    {
                                        UserMessageManager.Instance.UserMessage("Respawning " + array[i].associatedGamePlayer.NetworkplayerName);
                                        array[i].SetupClientRespawn();
                                        array[i].CallCmdRespawn();
                                        break;
                                    }
                                    else if (IsLocal())
                                    {
                                        UserMessageManager.Instance.UserMessage("Respawning " + array[i].LocalizedName);
                                        array[i].SetupClientRespawn();
                                        array[i].CallCmdRespawn();
                                        break;
                                    }
                                    else
                                    {
                                        UserMessageManager.Instance.UserMessage("Cannot respawn local players online.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.UpdateHoldBIndicator))]
        static class QuickSwitch
        {
            static void Postfix(Character __instance)
            {
                if (!InFreeplayMode()) return;
                if (InTreehouse()) return;
                if (__instance.LocalPlayer == null) return;
                __instance.SuicideTime = SwitchTime.Value;
                var playercursor = __instance.associatedGamePlayer.CursorInstance;
                var cursor = playercursor.GetComponent<PiecePlacementCursor>();
                cursor.SwitchTime = SwitchTime.Value;
            }
        }
        [HarmonyPatch(typeof(PiecePlacementCursor), nameof(PiecePlacementCursor.Start))]
        static class CursorSwitchHotfix
        {
            static void Postfix(PiecePlacementCursor __instance)
            {
                if (!InFreeplayMode()) return;
                if (InTreehouse()) return;
                if (__instance.LocalPlayer == null) return;
                __instance.SwitchTime = SwitchTime.Value;
            }
        }
        [HarmonyPatch(typeof(GameEventManager), nameof(GameEventManager.SendEvent))]
        static class persistingthroughthecycle
        {
            static bool Prefix(object e)
            {
                if (!InFreeplayMode()) return true;
                if (InTreehouse()) return true;
                string eventname =  e.GetType().Name;
                if (eventname == "LevelResetEvent" && CyclePersistence.Value)
                {
                    if (ResetCycle == true)
                    {
                        ResetCycle = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.StartInvincibleTimer))]
        static class ForceResetCrumble
        {
            static void Prefix(Character __instance)
            {
                if (!ResetCrumbleOnDeath.Value) return;
                if (!InFreeplayMode()) return;
                if (__instance.LocalPlayer == null) return;
                CrumblingBlock[] array = GameObject.FindObjectsOfType<CrumblingBlock>();
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Active == true)
                    {
                        array[i].Reset();
                    }
                }
            }
        }
    }
}    