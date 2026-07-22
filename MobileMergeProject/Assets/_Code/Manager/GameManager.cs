using _Code.Block;
using _Code.Field;
using TMPro;
using UnityEngine;
using MouseView = _Code.Mouse.Mouse;

namespace _Code.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BlockField _blockField;
        [SerializeField] private RandomBlockManager _randomBlockManager;
        [SerializeField] private BlockPiece[] _pieces;
        [SerializeField] private MouseView _mouse;
        [SerializeField] private BlockBlastEnvironmentView _environmentView;
        [SerializeField] private JsonManager _jsonManager;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _bestScoreText;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField, Min(20f)] private float _swipeMinDistance = 75f;
        [SerializeField] private bool _enableKeyboardInput = true;

        private readonly SwipeInputReader _swipeInputReader = new SwipeInputReader();
        private int _score;
        private int _maxScore;
        private bool _isGameOver;

        private void Awake()
        {
            DisableLegacyVisuals();

            if (_blockField == null)
            {
                enabled = false;
                return;
            }

            if (_randomBlockManager == null)
                _randomBlockManager = GetComponent<RandomBlockManager>();

            ResolveMouse();
            ResolveEnvironmentView();
            ResolveJsonManager();
        }

        private void Start()
        {
            if (_randomBlockManager == null)
                return;

            _blockField.Rebuild();
            _blockField.ClearAll();
            if (_environmentView != null)
                _environmentView.Configure(_blockField, Camera.main);

            _randomBlockManager.Initialize(_pieces);
            if (_mouse != null)
                _mouse.Initialize(_blockField);

            SubscribePieces();
            LoadMaxScore();
            UpdateScoreText();
            UpdateBestScoreText();

            if (!_randomBlockManager.GiveNewSet(_blockField))
            {
                EndGame();
                return;
            }

            SetMessage(string.Empty);
        }

        private void Update()
        {
            if (_isGameOver)
                return;

            if (BlockPiece.IsAnyDragging)
            {
                _swipeInputReader.Cancel();
                return;
            }

            if (_swipeInputReader.TryReadDirection(_swipeMinDistance, _enableKeyboardInput, out Vector2Int direction))
                ShiftBoard(direction);
        }

        private void OnDestroy()
        {
            UnsubscribePieces();
        }

        private void HandlePieceReleased(BlockPiece piece)
        {
            if (_isGameOver)
            {
                piece.ReturnToSlot();
                return;
            }

            if (!_blockField.TryGetAnchorFor(piece, out Vector2Int anchor) || !_blockField.CanInstall(piece, anchor))
            {
                piece.ReturnToSlot();
                return;
            }

            piece.SnapTo(_blockField.GetWorldPosition(anchor));
            _blockField.Install(piece, anchor);
            AddScore(piece.CellCount * 10);

            int clearedLines = _blockField.ClearCompletedLines();
            if (clearedLines > 0)
                AddScore(clearedLines * 100 + clearedLines * clearedLines * 50);

            piece.MarkPlaced();

            if (_randomBlockManager.AreAllPiecesPlaced() && !_randomBlockManager.GiveNewSet(_blockField))
            {
                EndGame();
                return;
            }

            if (!_randomBlockManager.HasAnyAvailablePlacement(_blockField))
            {
                if (_blockField.HasAnyCompactMove())
                    SetMessage("Swipe to Shift");
                else
                    EndGame();

                return;
            }

            SetMessage(string.Empty);
        }

        private void ShiftBoard(Vector2Int direction)
        {
            if (_randomBlockManager == null)
                return;

            if (_mouse != null && !_mouse.TryMove(direction, _blockField))
            {
                SetMessage("Edge");
                return;
            }

            bool moved = _blockField.Compact(direction);
            int clearedLines = _blockField.ClearCompletedLines();

            if (clearedLines > 0)
                AddScore(clearedLines * 90 + clearedLines * clearedLines * 40);

            if (!_randomBlockManager.HasAnyAvailablePlacement(_blockField))
            {
                if (_blockField.HasAnyCompactMove())
                    SetMessage("Swipe to Shift");
                else
                    EndGame();

                return;
            }

            SetMessage(moved || clearedLines > 0 ? "Shift" : string.Empty);
        }

        private void SubscribePieces()
        {
            foreach (BlockPiece piece in _pieces)
                piece.Released += HandlePieceReleased;
        }

        private void UnsubscribePieces()
        {
            if (_pieces == null)
                return;

            foreach (BlockPiece piece in _pieces)
            {
                if (piece != null)
                    piece.Released -= HandlePieceReleased;
            }
        }

        private void AddScore(int value)
        {
            _score += value;
            UpdateScoreText();
            TryUpdateMaxScore();
        }

        private void UpdateScoreText()
        {
            if (_scoreText != null)
                _scoreText.text = $"Score {_score}";
        }

        private void UpdateBestScoreText()
        {
            if (_bestScoreText != null)
                _bestScoreText.text = $"Best {_maxScore}";
        }

        private void LoadMaxScore()
        {
            if (_jsonManager == null)
                return;

            _jsonManager.Load();
            _maxScore = _jsonManager.MaxScore;
        }

        private void TryUpdateMaxScore()
        {
            if (_jsonManager == null || !_jsonManager.TrySaveMaxScore(_score))
                return;

            _maxScore = _jsonManager.MaxScore;
            UpdateBestScoreText();
        }

        private void EndGame()
        {
            _isGameOver = true;
            TryUpdateMaxScore();
            SetMessage("Game Over");
        }

        private void ResolveMouse()
        {
            if (_mouse != null)
                return;

            _mouse = FindFirstObjectByType<MouseView>();

            if (_mouse != null)
                return;

            GameObject mouseObject = GameObject.Find("Mouse");

            if (mouseObject != null)
                _mouse = mouseObject.AddComponent<MouseView>();
        }

        private void ResolveEnvironmentView()
        {
            if (_environmentView != null)
                return;

            _environmentView = GetComponent<BlockBlastEnvironmentView>();
        }

        private void ResolveJsonManager()
        {
            if (_jsonManager != null)
                return;

            _jsonManager = GetComponent<JsonManager>();

            if (_jsonManager == null)
                _jsonManager = gameObject.AddComponent<JsonManager>();
        }

        private void DisableLegacyVisuals()
        {
            Transform root = transform.parent;
            string[] legacyNames = { "Square", "Square (1)", "Square (2)", "Square (3)" };
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (Transform target in transforms)
            {
                if (root != null && target.IsChildOf(root))
                    continue;

                foreach (string legacyName in legacyNames)
                {
                    if (target.name == legacyName)
                    {
                        target.gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }

        private void SetMessage(string message)
        {
            if (_messageText != null)
                _messageText.text = message;
        }

        private sealed class SwipeInputReader
        {
            private Vector2 _startPosition;
            private bool _isTracking;

            public bool TryReadDirection(float minDistance, bool enableKeyboardInput, out Vector2Int direction)
            {
                if (enableKeyboardInput && TryReadKeyboard(out direction))
                    return true;

                if (Input.touchCount > 0)
                    return TryReadTouch(minDistance, out direction);

                return TryReadMouse(minDistance, out direction);
            }

            public void Cancel()
            {
                _isTracking = false;
            }

            private bool TryReadTouch(float minDistance, out Vector2Int direction)
            {
                direction = Vector2Int.zero;
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    _startPosition = touch.position;
                    _isTracking = true;
                    return false;
                }

                if (!_isTracking)
                    return false;

                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                    return false;

                Vector2 delta = touch.position - _startPosition;
                _isTracking = false;
                return TryConvertDelta(delta, minDistance, out direction);
            }

            private bool TryReadMouse(float minDistance, out Vector2Int direction)
            {
                direction = Vector2Int.zero;

                if (Input.GetMouseButtonDown(0))
                {
                    _startPosition = Input.mousePosition;
                    _isTracking = true;
                    return false;
                }

                if (!_isTracking || !Input.GetMouseButtonUp(0))
                    return false;

                Vector2 delta = (Vector2)Input.mousePosition - _startPosition;
                _isTracking = false;
                return TryConvertDelta(delta, minDistance, out direction);
            }

            private static bool TryReadKeyboard(out Vector2Int direction)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    direction = Vector2Int.up;
                    return true;
                }

                if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    direction = Vector2Int.down;
                    return true;
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    direction = Vector2Int.left;
                    return true;
                }

                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    direction = Vector2Int.right;
                    return true;
                }

                direction = Vector2Int.zero;
                return false;
            }

            private static bool TryConvertDelta(Vector2 delta, float minDistance, out Vector2Int direction)
            {
                if (delta.sqrMagnitude < minDistance * minDistance)
                {
                    direction = Vector2Int.zero;
                    return false;
                }

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    direction = delta.x > 0f ? Vector2Int.right : Vector2Int.left;
                else
                    direction = delta.y > 0f ? Vector2Int.up : Vector2Int.down;

                return true;
            }
        }
    }
}
