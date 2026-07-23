using _Code.Block;
using _Code.Field;
using TMPro;
using UnityEngine;
using MouseView = _Code.Mouse.Mouse;

namespace _Code.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BlockField blockField;
        [SerializeField] private RandomBlockManager randomBlockManager;
        [SerializeField] private BlockPiece[] pieces;
        [SerializeField] private MouseView mouse;
        [SerializeField] private JsonManager jsonManager;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField, Min(20f)] private float swipeMinDistance = 75f;
        [SerializeField] private bool enableKeyboardInput = true;

        private readonly SwipeInputReader _swipeInputReader = new SwipeInputReader();
        private const string ScoreSuffix = "\uC810";
        private const string BestScoreLabel = "\uCD5C\uACE0\uC810\uC218 : ";
        private int _score;
        private int _maxScore;
        private bool _isGameOver;

        private void Awake()
        {
            if (blockField == null)
            {
                enabled = false;
                return;
            }

            if (randomBlockManager == null)
                randomBlockManager = GetComponent<RandomBlockManager>();
        }

        private void Start()
        {
            if (randomBlockManager == null)
                return;

            blockField.Rebuild();
            blockField.ClearAll();

            randomBlockManager.Initialize(pieces);
            if (mouse != null)
                mouse.Initialize(blockField);

            SubscribePieces();
            LoadMaxScore();
            UpdateScoreText();
            UpdateBestScoreText();

            if (!randomBlockManager.GiveNewSet(blockField))
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

            if (_swipeInputReader.TryReadDirection(swipeMinDistance, enableKeyboardInput, out Vector2Int direction))
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

            if (!blockField.TryGetAnchorFor(piece, out Vector2Int anchor) || !blockField.CanInstall(piece, anchor))
            {
                piece.ReturnToSlot();
                return;
            }

            piece.SnapTo(blockField.GetWorldPosition(anchor));
            blockField.Install(piece, anchor);
            AddScore(piece.CellCount * 10);

            int clearedLines = blockField.ClearCompletedLines();
            if (clearedLines > 0)
                AddScore(clearedLines * 100 + clearedLines * clearedLines * 50);

            piece.MarkPlaced();

            if (randomBlockManager.AreAllPiecesPlaced() && !randomBlockManager.GiveNewSet(blockField))
            {
                EndGame();
                return;
            }

            if (!randomBlockManager.HasAnyAvailablePlacement(blockField))
            {
                if (blockField.HasAnyCompactMove())
                    SetMessage("Swipe to Shift");
                else
                    EndGame();

                return;
            }

            SetMessage(string.Empty);
        }

        private void ShiftBoard(Vector2Int direction)
        {
            if (randomBlockManager == null)
                return;

            if (mouse != null && !mouse.TryMove(direction, blockField))
            {
                SetMessage("Edge");
                return;
            }

            bool moved = blockField.Compact(direction);
            int clearedLines = blockField.ClearCompletedLines();

            if (clearedLines > 0)
                AddScore(clearedLines * 90 + clearedLines * clearedLines * 40);

            if (!randomBlockManager.HasAnyAvailablePlacement(blockField))
            {
                if (blockField.HasAnyCompactMove())
                    SetMessage("Swipe to Shift");
                else
                    EndGame();

                return;
            }

            SetMessage(moved || clearedLines > 0 ? "Shift" : string.Empty);
        }

        private void SubscribePieces()
        {
            foreach (BlockPiece piece in pieces)
                piece.Released += HandlePieceReleased;
        }

        private void UnsubscribePieces()
        {
            if (pieces == null)
                return;

            foreach (BlockPiece piece in pieces)
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
            if (scoreText != null)
                scoreText.text = $"{_score}{ScoreSuffix}";
        }

        private void UpdateBestScoreText()
        {
            if (bestScoreText != null)
                bestScoreText.text = $"{BestScoreLabel}{_maxScore}";
        }

        private void LoadMaxScore()
        {
            if (jsonManager == null)
                return;

            jsonManager.Load();
            _maxScore = jsonManager.MaxScore;
        }

        private void TryUpdateMaxScore()
        {
            if (jsonManager == null || !jsonManager.TrySaveMaxScore(_score))
                return;

            _maxScore = jsonManager.MaxScore;
            UpdateBestScoreText();
        }

        private void EndGame()
        {
            _isGameOver = true;
            TryUpdateMaxScore();
            SetMessage("Game Over");
        }

        private void SetMessage(string message)
        {
            if (messageText != null)
                messageText.text = message;
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
