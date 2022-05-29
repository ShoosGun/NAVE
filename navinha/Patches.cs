using HarmonyLib;
using UnityEngine;
using System.Reflection;
namespace Navinha
{
    [HarmonyPatch]
    public static class Patches
    {
        public static void DoPatches()
        {
            Harmony harmonyInstance = new Harmony("com.locochoco.plugin.navinha");
            harmonyInstance.Patch(AccessTools.Method(typeof(WhiteHoleVolume), nameof(WhiteHoleVolume.AddToGrowQueue)), prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.AddToGrowQueuePrefix)));
            harmonyInstance.PatchAll(Assembly.GetAssembly(typeof(Patches)));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestructionVolume), nameof(DestructionVolume.Vanish))]
        static bool DestructionVanishPrefix(OWRigidbody bodyToVanish, DestructionVolume __instance)
        {
            var vanishableObjectComponent = bodyToVanish.GetComponentInChildren<ControlledVanishObject>();
            if (vanishableObjectComponent != null)
                return vanishableObjectComponent.OnDestructionVanish(__instance);

            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlackHoleVolume), nameof(BlackHoleVolume.Vanish))]
        static bool BlackHoleVanishPrefix(OWRigidbody bodyToVanish, BlackHoleVolume __instance)
        {
            var vanishableObjectComponent = bodyToVanish.GetComponent<ControlledVanishObject>();
            if(vanishableObjectComponent == null)
                vanishableObjectComponent = bodyToVanish.GetComponentInChildren<ControlledVanishObject>();

            if (vanishableObjectComponent != null)
                return vanishableObjectComponent.OnBlackHoleVanish(__instance);

            return true;
        }
        public delegate bool ConditionsForPlayerToWarp();
        public static event ConditionsForPlayerToWarp OnConditionsForPlayerToWarp;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlackHoleVolume), nameof(BlackHoleVolume.VanishPlayer))]
        static bool VanishPlayerPrefix()
        {
            bool condition = true;
            foreach (var d in OnConditionsForPlayerToWarp.GetInvocationList())
                condition &= ((ConditionsForPlayerToWarp)d).Invoke();
            return condition;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WhiteHoleVolume), nameof(WhiteHoleVolume.ReceiveWarpedBody))]
        static bool ReceiveWarpedBodyPrefix(OWRigidbody warpedBody, WhiteHoleVolume __instance)
        {
            var vanishableObjectComponent = warpedBody.GetComponent<ControlledVanishObject>();
            if (vanishableObjectComponent == null)
                vanishableObjectComponent = warpedBody.GetComponentInChildren<ControlledVanishObject>();

            if (vanishableObjectComponent != null)
                return vanishableObjectComponent.OnWhiteHoleReceiveWarped(__instance);

            return true;
        }
        //TODO PQ ISSO AQUI NAO FUNCIONA AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
        //Metade das vezes nada ocorre, e outra metade ocorre, mas para os outros objetos
        static bool AddToGrowQueuePrefix(OWRigidbody bodyToGrow)
        {
            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var vanishableObjectComponent = bodyToGrow.GetComponent<ControlledVanishObject>();
            if (vanishableObjectComponent == null)
                vanishableObjectComponent = bodyToGrow.GetComponentInChildren<ControlledVanishObject>();

            if (vanishableObjectComponent != null)
            {
                Debug.Log("BBBBBBBBBBBBBBBBB");
                return vanishableObjectComponent.DestroyComponentsOnGrow;
            }

            Debug.Log("CCCCCCCCCCCCCCCCCCC");
            return true;
        }
    }
}
