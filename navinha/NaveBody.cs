using UnityEngine;

namespace Navinha
{
    public class NaveBody : OWRigidbody
    {
        private OWRigidbody _playerBody;
        private bool _isPlayerAtFlightConsole;

        public override void Awake()
        {
            base.Awake();
            GlobalMessenger<OWRigidbody>.AddListener("EnterNaveFlightConsole", OnEnterFlightConsole);
            GlobalMessenger.AddListener("ExitNaveFlightConsole", OnExitFlightConsole);
            _playerBody = gameObject.GetTaggedComponent<OWRigidbody>("Player");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            GlobalMessenger<OWRigidbody>.RemoveListener("EnteNaverFlightConsole", OnEnterFlightConsole);
            GlobalMessenger.RemoveListener("ExitNaveFlightConsole", OnExitFlightConsole);
        }

        public override void SetPosition(Vector3 worldPosition)
        {
           if (_isPlayerAtFlightConsole)
            {
                base.SetPosition(worldPosition);
                Locator.GetCenterOfTheUniverse().RecenterUniverseAroundPlayer();
            }
            else
            {
                base.SetPosition(worldPosition);
            }
        }

        private void OnEnterFlightConsole(OWRigidbody shipBody)
        {
            _isPlayerAtFlightConsole = true;
        }

        private void OnExitFlightConsole()
        {
            _isPlayerAtFlightConsole = false;
        }
    }
}
