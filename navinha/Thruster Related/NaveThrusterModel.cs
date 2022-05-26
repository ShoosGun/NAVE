using UnityEngine;

namespace Navinha
{
    public class NaveThrusterModel : ThrusterModel
    {
        public override void Awake()
        {
            base.Awake();
            _maxTranslationalThrust = 50f;
			_maxRotationalThrust = 20f; //5f eh o padrao
        }

        public override void FireTranslationalThrusters()
        {
            //Só há um foguete, que aponta para frente. Então iremos usar o z do _translationalInput para saber quanto será o impulso
            float aceleracao = Mathf.Clamp(_translationalInput.z * _maxTranslationalThrust, 0f, _maxTranslationalThrust);
            _localAcceleration = Vector3.forward * aceleracao;
            if (aceleracao > 0f)
            {
                _isTranslationalFiring = true;
                _owRigidbody.AddLocalAcceleration(_localAcceleration, _owRigidbody.GetCenterOfMass());
            }
            else
            {
                _isTranslationalFiring = false;
            }
        }
    }

}
