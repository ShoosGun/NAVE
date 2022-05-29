using UnityEngine;

namespace Navinha
{
    class NaveFlightConsole : ControlledVanishObject
    {
        public OWRigidbody naveBody;

        private InteractVolume interactVolume;

        private PlayerAttachPoint attachPoint;

        //private OWRigidbody attachedBody;

        private AudioSource playerAudioSource;

        private ScreenPrompt promptDeAtivar;

        private ScreenPrompt promptDeReduzirPotencia;

        private ScreenPrompt promptDeAumentarPotencia;

        public ScreenPrompt valorDaPotencia;
        public NaveThrusterController naveThrusterController;

        private string TurnOnPrompt = "Turn On";
        private string LowerPowerPrompt = "Lower Power";
        private string IncreasePowerPrompt = "Increase Power";
        private string PowerPrompt = "Power: ";
        public override void  OnVanish() 
        {
            GlobalMessenger<DeathType>.FireEvent("TriggerPlayerDeath", DeathType.Energy);
        }
        private void Awake()
        {
            enabled = false;
            promptDeAtivar = new ScreenPrompt(XboxButton.LeftStick, TurnOnPrompt, 1);
            promptDeReduzirPotencia = new ScreenPrompt(XboxButton.LeftTrigger, LowerPowerPrompt, 1);
            promptDeAumentarPotencia = new ScreenPrompt(XboxButton.RightTrigger, IncreasePowerPrompt, 1);
            valorDaPotencia = new ScreenPrompt(PowerPrompt, 1);

            attachPoint = this.GetRequiredComponent<PlayerAttachPoint>();
            interactVolume = this.GetRequiredComponent<InteractVolume>();
            playerAudioSource = gameObject.GetTaggedComponent<AudioSource>("Player");

            interactVolume.OnPressInteract += OnPressInteract;
        }

        private void OnDestroy()
        {
            interactVolume.OnPressInteract -= OnPressInteract;
        }

        private void OnPressInteract()
        {
            if (!enabled)
            {
                attachPoint.AttachPlayer();
                enabled = true;
                GlobalMessenger<OWRigidbody>.FireEvent("EnterNaveFlightConsole", naveBody);
                
                Locator.GetPromptManager().AddScreenPrompt(promptDeAtivar, PromptPosition.left, true);
                Locator.GetPromptManager().AddScreenPrompt(promptDeReduzirPotencia, PromptPosition.left, true);
                Locator.GetPromptManager().AddScreenPrompt(promptDeAumentarPotencia, PromptPosition.left, true);
                Locator.GetPromptManager().AddScreenPrompt(valorDaPotencia, PromptPosition.bottom, true);
                
            }
        }

        private void Update()
        {
            if (OWInput.GetButtonUp(InterfaceInput.cancel))
            {
                attachPoint.DetachPlayer();
                interactVolume.ResetInteraction();
                enabled = false;
                GlobalMessenger.FireEvent("ExitNaveFlightConsole");
                Locator.GetPromptManager().RemoveScreenPrompt(promptDeAtivar);
                Locator.GetPromptManager().RemoveScreenPrompt(promptDeReduzirPotencia);
                Locator.GetPromptManager().RemoveScreenPrompt(promptDeAumentarPotencia);
                Locator.GetPromptManager().RemoveScreenPrompt(valorDaPotencia);
            }
            if (naveThrusterController != null)
                valorDaPotencia.SetText(PowerPrompt + naveThrusterController.Potencia / 10 + '%');
        }
    }
}
