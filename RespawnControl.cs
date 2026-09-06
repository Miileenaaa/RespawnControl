using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;

[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]

namespace RespawnControl
{
    [BepInPlugin("com.Milena.RespawnControl", "RespawnControl", "1.0.0")]
    public class RespawnControlMod : BaseUnityPlugin
    {
        public static ConfigEntry<KeyCode> InstantRespawnKey;
        public static ConfigEntry<bool> InstantRetryOnDeath;
        public static ConfigEntry<float> DramaticDeath;
        public static ConfigEntry<bool> DramaticDeathKey;
        public static ConfigEntry<KeyCode> RespawnKey;
        public static ConfigEntry<float> FastRespawn;

        void Awake()
        {
            InstantRespawnKey = Config.Bind("Challenge", "Instant Respawn", KeyCode.B, "By pressing this key, you will instantly respawn in challenge mode");
            InstantRetryOnDeath = Config.Bind("Challenge", "Instant Retry On Death", false, "By enabling this option, you will instantly retry on death! (no scoreboard popup)");
            DramaticDeath = Config.Bind("Challenge", "No Auto Retry after", 60f, "If the in-game Timer is higher than this number and you die, it won't auto retry (useful to not miss poss-mortems while still having a fast respawntime at the start of the level, or have more time to figure out what you died to)");
            DramaticDeathKey = Config.Bind("Challenge", "No Accidental Instant Respawn", false, "If true, this will make it so you don't die when you press your instant respawn key after the ingame timer is higher than 'No Auto Retry after' (useful if you accidently press your instant respawn key while on a far run)");

            RespawnKey = Config.Bind("Freeplay", "Respawn Key for Freeplay", KeyCode.None, "By pressing this key, you will instantly respawn in Freeplay");
            FastRespawn = Config.Bind("Freeplay", "Respawn Time for Freeplay", 0.5f, "Change the time that takes to respawn in freeplay, do note that values lower than 1 are currently unstable in multiplayer lobby!");

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
        private static bool CheckTimer()
        {
            var Challenge = GameObject.Find("ChallengeController(Clone)");
            var Timer = Challenge?.GetComponent<ChallengeControl>();

            float runTime = Traverse.Create(Timer).Field("runTime").GetValue<float>();

            try {
                return runTime >= DramaticDeath.Value;
            } catch { return false; }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class CharacterUpdatePatch
        {
            static void Prefix(Character __instance)
            {
                if (Input.GetKeyDown(InstantRespawnKey.Value))
                {
                    if (!InChallengeMode()) return;
                    if (CheckTimer() && DramaticDeathKey.Value) return;
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
                    __instance.NetworkWantsToRetry = true;
                }
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Update))]
        static class FastFPRespawn
        {
            static void Prefix(Character __instance)
            {
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
                    if (__instance.Paused) return;
                    __instance.LastDeath = "FPSuicide";
                    __instance.maxDeathDelay = 0.2f;
                    __instance.Networkdying = true;
                }
            }
        }
        [HarmonyPatch(typeof(ChallengeControl), nameof(ChallengeControl.Update))]
        static class Runtime
        {
            
        }
    }
}    