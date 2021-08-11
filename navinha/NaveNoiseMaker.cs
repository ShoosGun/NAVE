using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Navinha
{
    public class NaveNoiseMaker : NoiseMaker
    {
        private NaveThrusterModel naveThrusterModel;

        private NaveThrusterController naveThrusterController;

        private float thrustVolume = 10f;

        protected override void Awake()
        {
            base.Awake();
            naveThrusterModel = _attachedBody.GetRequiredComponent<NaveThrusterModel>();
            naveThrusterController = _attachedBody.GetRequiredComponent<NaveThrusterController>();
        }
        
        private void Update() => _netVolume = naveThrusterController.IsThrusterOn() ? naveThrusterController.Potencia / 1000 * thrustVolume : 0f;
    }

}
