using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
namespace Navinha
{
    public static class Patches
    {
        public static void DoPatches()
        {
            Harmony harmonyInstance = new Harmony("com.locochoco.plugin.navinha");

            #region VanishVolume
            MethodInfo VanishVolumeFixedUpdate = AccessTools.Method(typeof(VanishVolume), "FixedUpdate");
            MethodInfo VanishVolumeOnTriggerEnter = AccessTools.Method(typeof(VanishVolume), "OnTriggerEnter");

            HarmonyMethod fixedUpdatePrefix = new HarmonyMethod(typeof(Patches), nameof(Patches.FixedUpdatePrefix));
            HarmonyMethod onTriggerEnterPrefix = new HarmonyMethod(typeof(Patches), nameof(Patches.OnTriggerEnterPrefix));

            harmonyInstance.Patch(VanishVolumeFixedUpdate, prefix: fixedUpdatePrefix);
            harmonyInstance.Patch(VanishVolumeOnTriggerEnter, prefix: onTriggerEnterPrefix);
            #endregion

            #region DestructionVolume
            MethodInfo DestructionVolumeVanish = AccessTools.Method(typeof(DestructionVolume), "Vanish");
            HarmonyMethod destructionVanishPrefix = new HarmonyMethod(typeof(Patches), nameof(Patches.DestructionVanishPrefix));
            harmonyInstance.Patch(DestructionVolumeVanish, prefix: destructionVanishPrefix);
            #endregion

            #region BlackHoleVolume
            MethodInfo BlackHoleVolumeVanish = AccessTools.Method(typeof(BlackHoleVolume), "Vanish");
            MethodInfo BlackHoleVolumeVanishPlayer = AccessTools.Method(typeof(BlackHoleVolume), "VanishPlayer");

            HarmonyMethod blackHoleVanishPrefix = new HarmonyMethod(typeof(Patches), nameof(Patches.BlackHoleVanishPrefix));
            HarmonyMethod vanishPlayerPrefix = new HarmonyMethod(typeof(Patches), nameof(Patches.VanishPlayerPrefix));

            harmonyInstance.Patch(BlackHoleVolumeVanish, prefix: blackHoleVanishPrefix);
            harmonyInstance.Patch(BlackHoleVolumeVanishPlayer, prefix: vanishPlayerPrefix);
            #endregion

            #region WhiteHoleVolume
            MethodInfo WhiteHoleVolumeReceiveWarpedBody = AccessTools.Method(typeof(WhiteHoleVolume), "ReceiveWarpedBody");
            MethodInfo WhiteHoleVolumeAddToGrowQueue = AccessTools.Method(typeof(WhiteHoleVolume), "AddToGrowQueue");
            
            HarmonyMethod receiveWarpedBodyPrefix = new HarmonyMethod(typeof(Patches), nameof(Patches.ReceiveWarpedBodyPrefix));
            HarmonyMethod addToGrowQueuePrefix = new HarmonyMethod(typeof(Patches), nameof(Patches.AddToGrowQueuePrefix));

            harmonyInstance.Patch(WhiteHoleVolumeReceiveWarpedBody, prefix: receiveWarpedBodyPrefix);
            harmonyInstance.Patch(WhiteHoleVolumeAddToGrowQueue, prefix: addToGrowQueuePrefix);
            #endregion
        }

        static bool DestructionVanishPrefix(OWRigidbody bodyToVanish, DestructionVolume __instance)
        {
            var vanishableObjectComponent = bodyToVanish.GetComponentInChildren<ControlledVanishObject>();
            if (vanishableObjectComponent != null)
                return vanishableObjectComponent.OnDestructionVanish(__instance);

            return true;
        }
        static bool BlackHoleVanishPrefix(OWRigidbody bodyToVanish, BlackHoleVolume __instance)
        {
            var vanishableObjectComponent = bodyToVanish.GetComponent<ControlledVanishObject>();
            if (vanishableObjectComponent == null)
                vanishableObjectComponent = bodyToVanish.GetComponentInChildren<ControlledVanishObject>();

            if (vanishableObjectComponent != null)
                return vanishableObjectComponent.OnBlackHoleVanish(__instance);

            return true;
        }
        public delegate bool ConditionsForPlayerToWarp();
        public static event ConditionsForPlayerToWarp OnConditionsForPlayerToWarp;
        static bool VanishPlayerPrefix()
        {
            bool condition = true;
            foreach (var d in OnConditionsForPlayerToWarp.GetInvocationList())
                condition &= ((ConditionsForPlayerToWarp)d).Invoke();
            return condition;
        }
        static bool ReceiveWarpedBodyPrefix(OWRigidbody warpedBody, WhiteHoleVolume __instance)
        {
            var vanishableObjectComponent = warpedBody.GetComponent<ControlledVanishObject>();
            if (vanishableObjectComponent == null)
                vanishableObjectComponent = warpedBody.GetComponentInChildren<ControlledVanishObject>();

            if (vanishableObjectComponent != null)
                return vanishableObjectComponent.OnWhiteHoleReceiveWarped(__instance);

            return true;
        }
        static bool AddToGrowQueuePrefix(OWRigidbody bodyToGrow)
        {
            var vanishableObjectComponent = bodyToGrow.GetComponent<ControlledVanishObject>();
            if (vanishableObjectComponent == null)
                vanishableObjectComponent = bodyToGrow.GetComponentInChildren<ControlledVanishObject>();

            if (vanishableObjectComponent != null)
                return vanishableObjectComponent.DestroyComponentsOnGrow;

            return true;
        }
        private static Dictionary<VanishVolume, List<ControlledVanishObject>> controlledVanishObjectVolumeList = new Dictionary<VanishVolume, List<ControlledVanishObject>>();
        public static void CheckControlledVanishObjectVolumeList()
        {
            var keys = controlledVanishObjectVolumeList.Keys;
            foreach (var key in keys)
            {
                if (key == null)
                    controlledVanishObjectVolumeList.Remove(key);
            }
        }
        static bool OnTriggerEnterPrefix(Collider hitCollider, VanishVolume __instance)
        {
            if (hitCollider.attachedRigidbody != null)
            {
                var vanishableObjectComponent = hitCollider.attachedRigidbody.GetComponent<ControlledVanishObject>();
                if (vanishableObjectComponent == null)
                    vanishableObjectComponent = hitCollider.attachedRigidbody.GetComponentInChildren<ControlledVanishObject>();
                if (vanishableObjectComponent != null)
                {
                    if (controlledVanishObjectVolumeList.TryGetValue(__instance, out var list))
                    {
                        list.Add(vanishableObjectComponent);
                    }
                    else
                    {
                        controlledVanishObjectVolumeList.Add(__instance, new List<ControlledVanishObject> { vanishableObjectComponent });
                    }
                    return false;
                }
            }
            return true;
        }
        static void FixedUpdatePrefix(VanishVolume __instance)
        {
            if (controlledVanishObjectVolumeList.TryGetValue(__instance,out var list))
            {
                foreach(var vanishObject in list) 
                {
                    __instance.Vanish(vanishObject.GetAttachedOWRigidbody());
                }
                list.Clear();
            }
        } 
    }
}
