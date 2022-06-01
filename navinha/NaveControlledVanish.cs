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
            naveConsole = gameObject.GetComponentInChildren<NaveFlightConsole>();
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
            MonoBehaviour.print("black hole receieved naveBody");
            blackHoleVolume._whiteHole.ReceiveWarpedBody(naveBody);
            return false;
        }
        private float lastNaveWarpTime = 0f;
        public override bool OnWhiteHoleReceiveWarped(WhiteHoleVolume whiteHoleVolume)
        {
            if (Time.time > lastNaveWarpTime + Time.deltaTime)
            {
                unsafe 
                {
                    MonoBehaviour.print("white hole receieved naveBody");
                    whiteHoleVolume.ForceWarp(naveBody);
                    lastNaveWarpTime = Time.time; 
                }
            }
            return false;
        }
        private bool Patches_OnConditionsForPlayerToWarp()
        {
            return !naveConsole.enabled;
        }
    }
}
