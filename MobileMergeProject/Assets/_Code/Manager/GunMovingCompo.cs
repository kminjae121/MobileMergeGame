using System;
using _Code.GunData;
using _Code.InputSystem;
using UnityEngine;

namespace _Code.Manager
{
    public class GunMovingCompo : MonoBehaviour
    {
        [SerializeField] private PlayerInput inputSO;

        private GunItem _currentGunItem;

        private void Awake()
        {
            inputSO.TorchPressEvent += HandleTorchPress;
            inputSO.TorchReleaseEvent += HandleTorchRelease;
        }


        private void OnDestroy()
        { 
            inputSO.TorchPressEvent -= HandleTorchPress;
            inputSO.TorchReleaseEvent -= HandleTorchRelease;
        }

        private void HandleTorchPress()
        {
            
        }
        private void HandleTorchRelease()
        {
            
        }
    }
}