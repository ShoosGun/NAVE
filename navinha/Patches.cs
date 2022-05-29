using HarmonyLib;
using UnityEngine;

namespace Navinha
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestructionVolume), nameof(DestructionVolume.Vanish))]
        static bool VanishPrefix(OWRigidbody bodyToVanish) 
        {
            var vanishableObjectComponent = bodyToVanish.GetComponentInChildren<ControlledVanishObject>();
            if (vanishableObjectComponent != null)
            {
                vanishableObjectComponent.OnVanish();
                return false;
            }
            return true;
        }
    }
}
