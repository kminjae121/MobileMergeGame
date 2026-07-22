using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Code.Menu
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "GameScene";
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Collider2D _gameStartButtonCollider;
        [SerializeField] private SpriteRenderer _gameStartButtonRenderer;
        [SerializeField] private Color _normalButtonColor = new Color(0.94f, 0.58f, 0.28f);
        [SerializeField] private Color _pressedButtonColor = new Color(0.78f, 0.38f, 0.18f);
        [SerializeField, Min(0f)] private float _tapMaxTravel = 0.35f;

        private bool _isPressedInside;
        private Vector2 _pressWorldPosition;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            ApplyButtonColor(false);
        }

        private void Update()
        {
            if (Input.touchSupported && Input.touchCount > 0)
            {
                HandleTouch(Input.GetTouch(0));
                return;
            }

            HandleMouse();
        }

        public void StartGame()
        {
            if (!string.IsNullOrEmpty(_gameSceneName))
            {
                SceneManager.LoadScene(_gameSceneName);
                return;
            }

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextSceneIndex);
        }

        private void HandleTouch(Touch touch)
        {
            if (touch.phase == TouchPhase.Began)
            {
                _isPressedInside = IsPointerInside(touch.position);
                _pressWorldPosition = GetWorldPosition(touch.position);
                ApplyButtonColor(_isPressedInside);
                return;
            }

            if (!_isPressedInside)
                return;

            if (touch.phase == TouchPhase.Canceled)
            {
                ResetPress();
                return;
            }

            bool isInside = IsPointerInside(touch.position);
            ApplyButtonColor(isInside);

            if (touch.phase != TouchPhase.Ended)
                return;

            Vector2 releaseWorldPosition = GetWorldPosition(touch.position);
            float travelDistance = Vector2.Distance(_pressWorldPosition, releaseWorldPosition);

            if (isInside && travelDistance <= _tapMaxTravel)
                StartGame();

            ResetPress();
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isPressedInside = IsPointerInside(Input.mousePosition);
                _pressWorldPosition = GetWorldPosition(Input.mousePosition);
                ApplyButtonColor(_isPressedInside);
                return;
            }

            if (!_isPressedInside)
                return;

            bool isInside = IsPointerInside(Input.mousePosition);
            ApplyButtonColor(isInside && Input.GetMouseButton(0));

            if (!Input.GetMouseButtonUp(0))
                return;

            Vector2 releaseWorldPosition = GetWorldPosition(Input.mousePosition);
            float travelDistance = Vector2.Distance(_pressWorldPosition, releaseWorldPosition);

            if (isInside && travelDistance <= _tapMaxTravel)
                StartGame();

            ResetPress();
        }

        private bool IsPointerInside(Vector2 screenPosition)
        {
            if (_mainCamera == null || _gameStartButtonCollider == null)
                return false;

            return _gameStartButtonCollider.OverlapPoint(GetWorldPosition(screenPosition));
        }

        private Vector2 GetWorldPosition(Vector2 screenPosition)
        {
            if (_mainCamera == null)
                return Vector2.zero;

            return _mainCamera.ScreenToWorldPoint(screenPosition);
        }

        private void ResetPress()
        {
            _isPressedInside = false;
            ApplyButtonColor(false);
        }

        private void ApplyButtonColor(bool isPressed)
        {
            if (_gameStartButtonRenderer == null)
                return;

            _gameStartButtonRenderer.color = isPressed ? _pressedButtonColor : _normalButtonColor;
        }
    }
}
