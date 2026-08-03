using System.Collections.Generic;
using _Code.Block;
using _Code.Effects;
using _Code.Field;
using _Code.Server;
using _Code.Stage;
using Code.Core.Events.Bus;
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
        [SerializeField] private ServerScoreClient serverScoreClient;
        [SerializeField] private HapticFeedback hapticFeedback;
        [SerializeField] private BlockPlacementPreview placementPreview;
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private LineClearPawParticleEffect lineClearParticleEffect;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField, Min(20f)] private float swipeMinDistance = 75f;
        [SerializeField] private bool enableKeyboardInput = true;

        private readonly List<Vector3> _clearedBlockPositions = new List<Vector3>(36);
        private readonly SwipeInputReader _swipeInputReader = new SwipeInputReader();
        private const string ScoreSuffix = "\uC810";
        private const string BestScoreLabel = "\uCD5C\uACE0\uC810\uC218 : ";
        private const string StageLabel = "\uC2A4\uD14C\uC774\uC9C0";
        private const string GoalLabel = "\uBAA9\uD45C";
        private const int GoldScoreInterval = 100;
        private int _score;
        private int _maxScore;
        private int _gold;
        private bool _isGameOver;
        private bool _isStageMode;
        private StageDefinition _stageDefinition;

        private void Awake()
        {
            if (blockField == null)
            {
                enabled = false;
                return;
            }

            if (randomBlockManager == null)
                randomBlockManager = GetComponent<RandomBlockManager>();

            if (serverScoreClient == null)
                serverScoreClient = GetComponent<ServerScoreClient>();

            if (hapticFeedback == null)
                hapticFeedback = GetComponent<HapticFeedback>();

            if (placementPreview == null)
                placementPreview = GetComponent<BlockPlacementPreview>();

            if (placementPreview == null)
                placementPreview = gameObject.AddComponent<BlockPlacementPreview>();

            if (lineClearParticleEffect == null)
                lineClearParticleEffect = GetComponentInChildren<LineClearPawParticleEffect>(true);
        }

        private void Start()
        {
            if (randomBlockManager == null)
                return;

            blockField.Rebuild();
            blockField.ClearAll();
            InitializeStageMode();

            randomBlockManager.Initialize(pieces);
            if (mouse != null)
                mouse.Initialize(blockField);

            SubscribePieces();
            LoadPlayerData();
            UpdateScoreText();
            UpdateBestScoreText();
            gameOverView?.Hide();

            if (!randomBlockManager.GiveNewSet(blockField))
            {
                EndGame();
                return;
            }

            SetMessage(GetStartMessage());
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

        private void LateUpdate()
        {
            UpdatePlacementPreview();
        }

        private void OnDestroy()
        {
            UnsubscribePieces();
        }

        private void HandlePieceReleased(BlockPiece piece)
        {
            placementPreview?.Hide();

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

            int clearedLines = blockField.ClearCompletedLines(_clearedBlockPositions);
            if (clearedLines > 0)
            {
                AddScore(clearedLines * 100 + clearedLines * clearedLines * 50);
                PlayLineClearEffects(clearedLines, _clearedBlockPositions);
            }

            piece.MarkPlaced();

            if (TryCompleteStage())
                return;

            if (randomBlockManager.AreAllPiecesPlaced() && !randomBlockManager.GiveNewSet(blockField))
            {
                EndGame();
                return;
            }

            if (!randomBlockManager.CanPlaceAllRemainingPieces(blockField))
            {
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
                return;

            Debug.Log("CatBlast");

            bool moved = blockField.Compact(direction);
            int clearedLines = blockField.ClearCompletedLines(_clearedBlockPositions);

            if (clearedLines > 0)
            {
                AddScore(clearedLines * 90 + clearedLines * clearedLines * 40);
                PlayLineClearEffects(clearedLines, _clearedBlockPositions);
            }

            if (TryCompleteStage())
                return;

            if (!randomBlockManager.CanPlaceAllRemainingPieces(blockField))
            {
                EndGame();
                return;
            }

            SetMessage(moved || clearedLines > 0 ? "Shift" : string.Empty);
        }

        private void InitializeStageMode()
        {
            _isStageMode = StageRunContext.TryGetSelectedStage(out _stageDefinition);

            if (!_isStageMode)
                return;

            int groupId = 10000 + _stageDefinition.Number * 100;

            foreach (Vector2Int point in _stageDefinition.StartingCells)
            {
                if (blockField.TryGetField(point, out _Code.Field.Field field))
                    field.SetObject(gameObject, Color.white, BlockBlastSpriteLibrary.GetRandomCatBlockSprite(), groupId++);
            }
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
            int previousScore = _score;
            _score += value;
            int earnedGold = Mathf.Max(0, _score / GoldScoreInterval - previousScore / GoldScoreInterval);

            if (earnedGold > 0)
                AddGold(earnedGold);

            UpdateScoreText();
            bool updatedMaxScore = TryUpdateMaxScore();

            if (earnedGold > 0 && !updatedMaxScore)
                SyncPlayerDataToServer();
        }

        private bool TryCompleteStage()
        {
            if (!_isStageMode || _isGameOver || _score < _stageDefinition.TargetScore)
                return false;

            CompleteStage();
            return true;
        }

        private void CompleteStage()
        {
            _isGameOver = true;
            placementPreview?.Hide();
            TryUpdateMaxScore();
            SetMessage($"{StageLabel} {_stageDefinition.Number} \uD074\uB9AC\uC5B4!");

            if (gameOverView != null)
                gameOverView.ShowStageClear(_score, _maxScore);
        }

        private void PlayLineClearEffects(int clearedLines, IReadOnlyList<Vector3> clearedBlockPositions)
        {
            Bus<CamShakeEvent>.Raise(new CamShakeEvent(0.2f));

            if (lineClearParticleEffect != null)
                lineClearParticleEffect.PlayAtPositions(clearedBlockPositions, clearedLines);

            if (hapticFeedback != null)
                hapticFeedback.PlayLineClear();
        }

        private void UpdateScoreText()
        {
            if (scoreText != null)
                scoreText.text = _isStageMode ? $"{_score}/{_stageDefinition.TargetScore}{ScoreSuffix}" : $"{_score}{ScoreSuffix}";
        }

        private void UpdateBestScoreText()
        {
            if (bestScoreText != null)
                bestScoreText.text = $"{BestScoreLabel}{_maxScore}";
        }

        private void LoadPlayerData()
        {
            if (jsonManager != null)
            {
                jsonManager.Load();
                jsonManager.ApplyDailyGoldRewardIfAvailable();
                _maxScore = jsonManager.MaxScore;
                _gold = jsonManager.Gold;
            }

            if (serverScoreClient != null)
                serverScoreClient.FetchPlayerData(ApplyServerPlayerData, KeepLocalPlayerData);
        }

        private bool TryUpdateMaxScore()
        {
            if (_score <= _maxScore)
                return false;

            _maxScore = _score;

            if (jsonManager != null)
                jsonManager.SetMaxScore(_maxScore);

            UpdateBestScoreText();

            SyncPlayerDataToServer();
            return true;
        }

        private void ApplyServerPlayerData(ServerScoreClient.PlayerData serverData)
        {
            int localScore = _maxScore;
            int localGold = _gold;

            if (jsonManager != null)
            {
                jsonManager.MergePlayerData(serverData.MaxScore, serverData.Gold, serverData.LastDailyGoldRewardDate);
                _maxScore = jsonManager.MaxScore;
                _gold = jsonManager.Gold;
            }
            else
            {
                _maxScore = Mathf.Max(_maxScore, serverData.MaxScore);
                _gold = Mathf.Max(_gold, serverData.Gold);
            }

            UpdateBestScoreText();

            if (localScore > serverData.MaxScore || localGold > serverData.Gold)
                SyncPlayerDataToServer();
        }

        private void KeepLocalPlayerData()
        {
            UpdateBestScoreText();
        }

        private void AddGold(int amount)
        {
            if (amount <= 0)
                return;

            _gold = Mathf.Max(0, _gold + amount);

            if (jsonManager != null)
                jsonManager.SetGold(_gold);
        }

        private void SyncPlayerDataToServer()
        {
            if (serverScoreClient != null)
                serverScoreClient.SubmitPlayerData(_maxScore, _gold);
        }

        private void EndGame()
        {
            _isGameOver = true;
            placementPreview?.Hide();
            TryUpdateMaxScore();
            SetMessage(string.Empty);

            if (gameOverView != null)
                gameOverView.Show(_score, _maxScore);
            else
                SetMessage("Game Over");
        }

        private void UpdatePlacementPreview()
        {
            if (placementPreview == null)
                return;

            BlockPiece activePiece = BlockPiece.ActivePiece;
            if (_isGameOver || activePiece == null)
            {
                placementPreview.Hide();
                return;
            }

            placementPreview.Show(blockField, activePiece);
        }

        private void SetMessage(string message)
        {
            if (messageText != null)
                messageText.text = message;
        }

        private string GetStartMessage()
        {
            if (!_isStageMode)
                return string.Empty;

            return $"{StageLabel} {_stageDefinition.Number} / {GoalLabel} {_stageDefinition.TargetScore}{ScoreSuffix}";
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
