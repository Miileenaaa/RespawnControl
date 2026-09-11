using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[assembly: AssemblyVersion("1.1.0")]
[assembly: AssemblyInformationalVersion("1.1.0")]

namespace RespawnControl
{
    [BepInPlugin("com.Milena.RespawnControl", "RespawnControl", "1.1.0")]
    public class RespawnControlMod : BaseUnityPlugin
    {
        public static ConfigEntry<KeyCode> InstantRespawnKey;
        public static ConfigEntry<bool> InstantRetryOnDeath;
        public static ConfigEntry<float> DelayDeath;
        public static ConfigEntry<float> DramaticDeath;
        public static ConfigEntry<bool> DramaticDeathKey;
        public static ConfigEntry<KeyCode> RespawnKey;
        public static ConfigEntry<float> FastRespawn;
        public static ConfigEntry<float> StartInvincibility;
        public static ConfigEntry<KeyCode> HoldInvincibility;
        public static ConfigEntry<bool> CannonRespawn;

        void Awake()
        {
            InstantRespawnKey = Config.Bind("Challenge", "Instant Respawn", KeyCode.B, "By pressing this key, you will instantly respawn in challenge mode");
            InstantRetryOnDeath = Config.Bind("Challenge", "Instant Retry On Death", false, "By enabling this option, you will instantly retry on death! (no scoreboard popup)");
            DelayDeath = Config.Bind("Challenge", "Delay Auto Retry by", 0f, "Useful to have more time trying to figure out what and where you died (max delay = 3 seconds)");
            DramaticDeath = Config.Bind("Challenge", "No Auto Retry after", 60f, "If the in-game Timer is higher than this number and you die, it won't auto retry (useful to not miss poss-mortems while still having a fast respawntime at the start of the level, or have more time to figure out what you died to)");
            DramaticDeathKey = Config.Bind("Challenge", "No Accidental Instant Respawn", false, "If true, this will make it so you don't die when you press your instant respawn key after the ingame timer is higher than 'No Auto Retry after' (useful if you accidently press your instant respawn key while on a far run)");

            RespawnKey = Config.Bind("Freeplay", "Respawn Key for Freeplay", KeyCode.None, "By pressing this key, you will instantly respawn in Freeplay");
            FastRespawn = Config.Bind("Freeplay", "Respawn Time for Freeplay", 0.5f, "Change the time that takes to respawn in freeplay, do note that values lower than 1 are currently unstable in multiplayer lobby!");
            StartInvincibility = Config.Bind("Freeplay", "Invincibility on Spawn", 1f, "Change how long the spawn invincibility lasts");
            HoldInvincibility = Config.Bind("Freeplay", "Invincibility Key", KeyCode.None, "While you hold this key, you will have invincibility");
            CannonRespawn = Config.Bind("Freeplay", "Respawn When Dead in Cannon", true, "When you enter a cannon while dead, you will automatically get respawned, also useful to not get teleported back to the cannon after respawn in multiplayer lobbies, disable this if you need to enter a cannon while dead");

            Debug.Log("InstantRespawn mod loaded!");
            new Harmony("com.Milena.RespawnControl").PatchAll();
        }

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
        private static bool CheckChat()
        {
            try {
                var Chat = GameObject.Find("ChatSystemPrefab(Clone)");
                var Display = Chat?.GetComponent<ChatDisplay>();
                return Display.ChatMode;
            } catch { return false; }
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
                    if (CheckChat()) return;
                    if (__instance.LocalPlayer == null) return;
                    if (__instance.Success) return;
                    if (__instance.Paused) return;
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
                if (CheckTimer()) return;
                bool dead = !__instance.Success && (__instance.Dead || __instance.Dying || __instance.LocallyDead);
                if (dead && !__instance.NetworkWantsToRetry)
                {
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
                if (__instance.Dead || __instance.Dying)
                {
                    if (!InFreeplayMode())
                    {
                        __instance.maxDeathDelay = 5f;
                    }
                    else
                    {
                        if (__instance.LastDeath != "FPSuicide")
                        {
                            __instance.maxDeathDelay = FastRespawn.Value;
                        }
                    }
                }
                bool dead = !__instance.Success && (__instance.Dead || __instance.Dying || __instance.LocallyDead);
                if (dead && __instance.inCannon == true)
                {
                    if (!InFreeplayMode()) return;
                    if (!CannonRespawn.Value) return;
                    __instance.Respawn();
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
                        __instance.LocallyDead = true;
                        __instance.Networkdead = true;
                    }
                    else
                    {
                        __instance.LastDeath = "FPSuicide";
                        __instance.maxDeathDelay = 0.2f;
                        __instance.dying = true;
                        __instance.Networkdying = true;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.StartInvincibleTimer))]
        public class SpawnInvincibility
        {
            static void Postfix(Character __instance)
            {
                if (!InFreeplayMode()) return;
                if (__instance.LocalPlayer == null) return;
                __instance.invincibleTimer = StartInvincibility.Value;
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class InvincibilityKey
        {
            static void Prefix(Character __instance)
            {
                if (__instance.LocalPlayer == null) return;
                if (Input.GetKeyDown(HoldInvincibility.Value))
                {
                    if (!InFreeplayMode()) return;
                    __instance.invincibleTimer = 1000000f;
                }
                if (Input.GetKeyUp(HoldInvincibility.Value))
                {
                    __instance.invincibleTimer = 0f;
                }
            }
        }
    }
}    