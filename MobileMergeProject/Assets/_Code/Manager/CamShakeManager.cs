using System;
using Code.Core.Events.Bus;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.Cam
{
    public class CamShakeManager : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource source;

        private void Awake()
        {
            Bus<CamShakeEvent>.Subscribe(ShakeCam);
        }

        private void OnDestroy()
        {
            Bus<CamShakeEvent>.Unsubscribe(ShakeCam);
        }

        private void ShakeCam(CamShakeEvent evt)
        {
            if(evt.force != 0)
                source.GenerateImpulse(evt.force);  
        }
    }
}