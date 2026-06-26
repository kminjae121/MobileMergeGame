using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Code.InputSystem
{
    [CreateAssetMenu (menuName = "Input/PlayerInput", fileName = "PlayerInput")]
    public class PlayerInput : ScriptableObject, Controls.IPlayerActions
    {
        private Controls _controls;

        public Vector2 TorchValue;
        public Action TorchPressEvent;

        private void EnsureControls()
        {
            if (_controls != null)
                return;

            _controls = new Controls();
            _controls.Player.SetCallbacks(this);
        }

        private void OnEnable()
        {
            EnsureControls();
            
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls?.Player.Disable();
        }


        public void OnTorch(InputAction.CallbackContext context)
        {
            TorchValue = context.ReadValue<Vector2>();
        }

        public void OnTorchPress(InputAction.CallbackContext context)
        {
            if(context.performed)
                TorchPressEvent?.Invoke();
        }
    }
}