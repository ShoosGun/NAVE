using UnityEngine;
namespace Navinha
{
    internal class NaveControlledVanish : ControlledVanishObject
    {
        private OWRigidbody naveBody;
        private NaveFlightConsole naveConsole;
        public void Awake() 
        {
            DestroyComponentsOnGrow = false;
            Patches.OnConditionsForPlayerToWarp += Patches_OnConditionsForPlayerToWarp;
        }
        public void Start() 
        {
            naveBody = gameObject.GetAttachedOWRigidbody();
            Debug.Log("aaa : " + naveBody == null);
            naveConsole = gameObject.GetComponentInChildren<NaveFlightConsole>();
            Debug.Log("aaa : " + naveConsole == null);
        }
        public override bool OnDestructionVanish(DestructionVolume destructionVolume)
        {
            if (naveConsole.enabled)
            {
                GlobalMessenger<DeathType>.FireEvent("TriggerPlayerDeath", DeathType.Energy);
                return false;
            }
            return true;
        }
        public override bool OnBlackHoleVanish(BlackHoleVolume blackHoleVolume)
        {
            blackHoleVolume._whiteHole.ReceiveWarpedBody(naveBody);
            return false;
        }
        private float lastNaveWarpTime = 0f;
        public override bool OnWhiteHoleReceiveWarped(WhiteHoleVolume whiteHoleVolume)
        {
            if (Time.time > lastNaveWarpTime + Time.deltaTime)
            {
                whiteHoleVolume.ForceWarp(naveBody);
                lastNaveWarpTime = Time.time;
            }
            return false;
        }
        private bool Patches_OnConditionsForPlayerToWarp()
        {
            return !naveConsole.enabled;
        }
    }
}
