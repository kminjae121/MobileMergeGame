using _Code.Block;
using _Code.Field;
using TMPro;
using UnityEngine;

namespace _Code.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BlockField _blockField;
        [SerializeField] private RandomBlockManager _randomBlockManager;
        [SerializeField] private BlockPiece[] _pieces;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _messageText;

        private int _score;
        private bool _isGameOver;

        private void Awake()
        {
            if (_blockField == null)
            {
                enabled = false;
                return;
            }

            if (_randomBlockManager == null)
                _randomBlockManager = GetComponent<RandomBlockManager>();
        }

        private void Start()
        {
            if (_randomBlockManager == null)
                return;

            _blockField.Rebuild();
            _blockField.ClearAll();
            _randomBlockManager.Initialize(_pieces);
            SubscribePieces();
            UpdateScoreText();

            if (!_randomBlockManager.GiveNewSet(_blockField))
            {
                EndGame();
                return;
            }

            SetMessage(string.Empty);
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
                EndGame();
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
        }

        private void UpdateScoreText()
        {
            if (_scoreText != null)
                _scoreText.text = $"Score {_score}";
        }

        private void EndGame()
        {
            _isGameOver = true;
            SetMessage("Game Over");
        }

        private void SetMessage(string message)
        {
            if (_messageText != null)
                _messageText.text = message;
            
        }
    }
}
