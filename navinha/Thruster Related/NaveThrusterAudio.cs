using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Navinha
{
    public class NaveThrusterAudio: MonoBehaviour
    {
        public OWAudioSource TranslationalSource;

        private AudioSource TranslationalAudioSource;

        protected NaveThrusterModel thrusterModel;

        protected NaveThrusterController thrusterController;
        

        public const float MaxVolume = 0.8f;

        protected void Awake()
        {
            thrusterModel = gameObject.GetAttachedOWRigidbody().GetComponent<NaveThrusterModel>();
            thrusterController = gameObject.GetAttachedOWRigidbody().GetComponent<NaveThrusterController>();

            thrusterModel.OnStartTranslationalThrust += OnStartTranslationalThrust;
            thrusterModel.OnStopTranslationalThrust += OnStopTranslationalThrust;
        }
        public void SetTranslationalSource(OWAudioSource translationalSource, AudioClip translationClip)
        {
            TranslationalSource = translationalSource;
            #if ALPHA_VERSION
            TranslationalSource.audio.loop = true;
            TranslationalSource.audio.playOnAwake = false;
            TranslationalSource.audio.clip = translationClip;
            #elif CURRENT_VERSION
            TranslationalSource.loop = true;
            TranslationalSource.playOnAwake = false;
            TranslationalSource.clip = translationClip;
            #endif
            TranslationalAudioSource = TranslationalSource.GetAudioSource();
            TranslationalAudioSource.volume = 0f;
        }
        protected void Update()
        {
            if (thrusterController.IsThrusterOn())
                TranslationalAudioSource.volume = thrusterController.Potencia / 1000f * MaxVolume;
        }

        protected void OnDestroy()
        {
            thrusterModel.OnStartTranslationalThrust -= OnStartTranslationalThrust;
            thrusterModel.OnStopTranslationalThrust -= OnStopTranslationalThrust;
        }

        protected void OnStartTranslationalThrust()
        {
            TranslationalSource.FadeIn(0.05f);
        }

        protected void OnStopTranslationalThrust()
        {
            TranslationalSource.FadeOut(0.1f);
        }
    }
}
