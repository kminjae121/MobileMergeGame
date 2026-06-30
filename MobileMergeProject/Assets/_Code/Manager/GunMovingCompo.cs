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

        private bool _isHolding;

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

        private void Update()
        {
            if (_isHolding && _currentGunItem != null)
                _currentGunItem.transform.position = inputSO.TorchValue;
        }

        private void HandleTorchPress()
        {
            GunItem gunItem;

            gunItem = _currentGunItem;

            _isHolding = true;
        }
        private void HandleTorchRelease()
        {
            _currentGunItem = null;
        }
    }
}