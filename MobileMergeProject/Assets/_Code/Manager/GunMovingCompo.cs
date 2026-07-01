using _Code.GunData;
using _Code.InputSystem;
using UnityEngine;

namespace _Code.Manager
{
    public class GunMovingCompo : MonoBehaviour
    {
        [SerializeField] private PlayerInput inputSO;
        [SerializeField] private LayerMask whatIsGun;
        [SerializeField] private Camera mainCamera;

        private Rigidbody2D _crnGunRbCompo;
        private GunItem _currentGunItem;
        private bool _isHolding;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

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
            if (_isHolding && _currentGunItem != null && GameManager.Instance.PlayMode == PlayMode.Moving)
            {
                _currentGunItem.transform.position = GetTouchWorldPosition();
            }
        }

        private void HandleTorchPress()
        {
            if (GameManager.Instance.PlayMode == PlayMode.Spawning)
                return;
            
            Vector2 touchWorldPosition = GetTouchWorldPosition();

            Collider2D hit = Physics2D.OverlapPoint(touchWorldPosition, whatIsGun);
            
            
            if (hit == null)
            {
                _currentGunItem = null;
                _isHolding = false;
                return;
            }

            if (!hit.TryGetComponent(out GunItem gunItem))
            {
                _currentGunItem = null;
                _isHolding = false;
                return;
            }

            _currentGunItem = gunItem;
            _crnGunRbCompo = _currentGunItem.GetComponent<Rigidbody2D>();

            _crnGunRbCompo.gravityScale = 0;
            _crnGunRbCompo.linearVelocity = Vector2.zero;
            Instantiate(_currentGunItem,touchWorldPosition,Quaternion.identity);
            _isHolding = true;
        }

        private void HandleTorchRelease()
        {
            if (_crnGunRbCompo != null && _currentGunItem != null)
            {
                _isHolding = false;
                _crnGunRbCompo.linearVelocity = Vector2.zero;
                _crnGunRbCompo.gravityScale = 1;
                _crnGunRbCompo = null;
                _currentGunItem = null;   
            }
        }

        private Vector2 GetTouchWorldPosition()
        {
            Vector3 screenPosition = inputSO.TorchValue;

            screenPosition.z = -mainCamera.transform.position.z;

            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

            return worldPosition;
        }
    }
}