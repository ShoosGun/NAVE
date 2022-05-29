using UnityEngine;
using System.Collections.Generic;

namespace Navinha
{
    public static class NaveInputs
    {
        public static readonly Axis AtivarPropulsor = new Axis(InputChannels.moveZ);
        public static readonly Axis AumentarPotencia = new Axis(InputChannels.zoomIn);
        public static readonly Axis DiminuirPotencia = new Axis(InputChannels.zoomOut);

        public static readonly Axis RodarNaVertical = new Axis(InputChannels.pitch);
        public static readonly Axis RolarOuRodarNaHorizontal = new Axis(InputChannels.yaw);
        public static readonly Button TrocarRodarHorizontalPorRolar = new Button(InputChannels.swapRollAndYaw);

        private static HashSet<InputCommand> naveInputs;

        private static HashSet<InputCommand> playerLookInputs;

        public static void InnitNaveInputs()
        {
            playerLookInputs = new HashSet<InputCommand>
            {
                PlayerCameraInput.lookX,
                PlayerCameraInput.lookY,
            };
            naveInputs = new HashSet<InputCommand>
            {
                AtivarPropulsor,
                AumentarPotencia,
                DiminuirPotencia,
                RodarNaVertical,
                RolarOuRodarNaHorizontal,
                TrocarRodarHorizontalPorRolar,

                ReferenceFrameInput.targetReferenceFrame,
                InterfaceInput.interact,
                InterfaceInput.cancel,
                InterfaceInput.toggleSettings,
                TelescopeInput.toggleTelescope,
                MapInput.toggleMap,
                ProbeInput.launchProbe,
                ProbeInput.takeSnapshot,
                ProbeInput.retrieveProbe,
                PlayerCameraInput.toggleFlashlight,
            };

            GlobalMessenger<OWRigidbody>.AddListener("EnterNaveFlightConsole", OnEnterNaveFlightConsole);
            GlobalMessenger.AddListener("ExitNaveFlightConsole", OnExitNaveFlightConsole);
        }
        private static void OnEnterNaveFlightConsole(OWRigidbody nave)
        {
            unsafe
            {
                OWInput._activeInputs = new HashSet<InputCommand>(naveInputs);
            }
        }
        private static void OnExitNaveFlightConsole()
        {
            unsafe
            {
                OWInput._activeInputs = new HashSet<InputCommand>(OWInput._characterInputs);
            }
        } 
    }
    public class NaveThrusterController : ThrusterController
    {
        public int Potencia { get; private set; } = 0; //Em "por mil"

        private bool ehParaRolar = false;
        
        public override void Awake()
        {
            base.Awake();
            enabled = false;
            GlobalMessenger<OWRigidbody>.AddListener("EnterNaveFlightConsole", OnEnterNaveFlightConsole);
            GlobalMessenger.AddListener("ExitNaveFlightConsole", OnExitNaveFlightConsole);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            GlobalMessenger<OWRigidbody>.RemoveListener("EnterNaveFlightConsole", OnEnterNaveFlightConsole);
            GlobalMessenger.RemoveListener("ExitNaveFlightConsole", OnExitNaveFlightConsole);
        }
        private void Update()
        {
            Potencia += (int)((NaveInputs.AumentarPotencia.GetInput() - NaveInputs.DiminuirPotencia.GetInput()) * 10);
            Potencia = Mathf.Clamp(Potencia, 0,1000);
            ehParaRolar = NaveInputs.TrocarRodarHorizontalPorRolar.GetPressed();            
        }
        public bool IsThrusterOn()
        {
            return NaveInputs.AtivarPropulsor.GetInput() > 0.1f;
        }
        public override Vector3 ReadTranslationalInput()
        {
            if (IsThrusterOn())
                return new Vector3(0f, 0f, Potencia / 1000f);

            return Vector3.zero;
        }

        public override Vector3 ReadRotationalInput()
        {
            //Rodar na horizontal eh 8x menos efetivo que rolar (1.25f de aceleracao)
            if (ehParaRolar)
                return new Vector3(- NaveInputs.RodarNaVertical.GetInput() / 8f * 4f, 0f, - NaveInputs.RolarOuRodarNaHorizontal.GetInput() * 4f);
            else
                return new Vector3(- NaveInputs.RodarNaVertical.GetInput() / 8f * 4f, NaveInputs.RolarOuRodarNaHorizontal.GetInput() / 8f * 4f, 0f);
        }

        private void OnEnterNaveFlightConsole(OWRigidbody NaveBody)
        {
            enabled = true;
        }

        private void OnExitNaveFlightConsole()
        {
            enabled = false;
        }
    }
}
