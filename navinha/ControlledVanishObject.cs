using UnityEngine;

namespace Navinha
{
    abstract public class ControlledVanishObject : MonoBehaviour 
    {
        public virtual bool OnDestructionVanish(DestructionVolume destructionVolume) { return true; }
        public virtual bool OnBlackHoleVanish(BlackHoleVolume blackHoleVolume) { return true; }
        public virtual bool OnWhiteHoleReceiveWarped(WhiteHoleVolume whiteHoleVolume) { return true; }

        public bool DestroyComponentsOnGrow { get; protected set; } = true;
    }
}
