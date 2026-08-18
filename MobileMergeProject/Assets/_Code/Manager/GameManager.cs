using System.Collections.Generic;
using _Code.Block;
using _Code.Effects;
using _Code.Field;
using _Code.Server;
using TMPro;
using UnityEngine;
using MouseView = _Code.Mouse.Mouse;
using TutorialController = _Code.Manager.TutorialController;

namespace _Code.Manager
{
    public class GameManager : MonoBehaviour
    {
        [Header("Scene References")]
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
        [SerializeField] private string resetSceneName = "MainScene";
        [SerializeField, Min(20f)] private float swipeMinDistance = 75f;
        [SerializeField] private bool enableKeyboardInput = true;

        [Header("Responsibility Controllers")]
        [SerializeField] private PlayerProgressController playerProgressController;
        [SerializeField] private StageModeController stageModeController;
        [SerializeField] private BoardShiftController boardShiftController;
        [SerializeField] private LineClearEffectPlayer lineClearEffectPlayer;
        [SerializeField] private PlacementScoreGuard placementScoreGuard;
        [SerializeField] private TutorialController tutorialController;

        private readonly List<Vector3> _clearedBlockPositions = new List<Vector3>(36);
        private bool _isGameOver;

        private void Awake()
        {
            if (blockField == null)
            {
                enabled = false;
                return;
            }

            ResolveSceneReferences();
            ResolveControllers();
            ConfigureControllers();
        }

        private void Start()
        {
            if (randomBlockManager == null || playerProgressController == null || stageModeController == null)
                return;

            blockField.Rebuild();
            blockField.ClearAll();
            stageModeController.Initialize(blockField, gameObject);

            randomBlockManager.Initialize(pieces);
            if (mouse != null)
                mouse.Initialize(blockField);

            SubscribePieces();
            playerProgressController.Initialize(stageModeController.IsStageMode, stageModeController.TargetScore);
            gameOverView?.Hide();

            if (!randomBlockManager.GiveNewSet(blockField))
            {
                EndGame();
                return;
            }

            bool tutorialStarted = tutorialController != null && tutorialController.TryBegin(stageModeController.IsStageMode);
            if (!tutorialStarted)
                SetMessage(stageModeController.GetStartMessage());
        }

        private void Update()
        {
            if (_isGameOver)
                return;

            if (boardShiftController != null &&
                boardShiftController.TryReadDirection(BlockPiece.IsAnyDragging, out Vector2Int direction))
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

        public void StartTutorial()
        {
            if (_isGameOver || tutorialController == null)
                return;

            tutorialController.BeginManually();
        }

        private void ResolveSceneReferences()
        {
            if (randomBlockManager == null)
                randomBlockManager = GetComponent<RandomBlockManager>();

            if (jsonManager == null)
                jsonManager = GetComponent<JsonManager>();

            if (serverScoreClient == null)
                serverScoreClient = GetComponent<ServerScoreClient>();

            if (hapticFeedback == null)
                hapticFeedback = GetComponent<HapticFeedback>();

            if (gameOverView == null)
                gameOverView = GetComponentInChildren<GameOverView>(true);

            if (placementPreview == null)
                placementPreview = GetComponent<BlockPlacementPreview>();

            if (placementPreview == null)
                placementPreview = gameObject.AddComponent<BlockPlacementPreview>();

            if (lineClearParticleEffect == null)
                lineClearParticleEffect = GetComponentInChildren<LineClearPawParticleEffect>(true);
        }

        private void ResolveControllers()
        {
            if (playerProgressController == null)
                playerProgressController = GetOrAdd<PlayerProgressController>();

            if (stageModeController == null)
                stageModeController = GetOrAdd<StageModeController>();

            if (boardShiftController == null)
                boardShiftController = GetOrAdd<BoardShiftController>();

            if (lineClearEffectPlayer == null)
                lineClearEffectPlayer = GetOrAdd<LineClearEffectPlayer>();

            if (placementScoreGuard == null)
                placementScoreGuard = GetOrAdd<PlacementScoreGuard>();

            if (tutorialController == null)
                tutorialController = GetOrAdd<TutorialController>();
        }

        private void ConfigureControllers()
        {
            playerProgressController.Configure(jsonManager, serverScoreClient, scoreText, bestScoreText);
            boardShiftController.Configure(blockField, randomBlockManager, mouse, swipeMinDistance, enableKeyboardInput);
            lineClearEffectPlayer.Configure(hapticFeedback, lineClearParticleEffect);
            placementScoreGuard.Configure(jsonManager, serverScoreClient, resetSceneName);
            tutorialController.Configure(messageText);
        }

        private T GetOrAdd<T>() where T : Component
        {
            T component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private void HandlePieceReleased(BlockPiece piece)
        {
            placementPreview?.Hide();

            if (_isGameOver)
            {
                piece.ReturnToSlot();
                return;
            }

            if (tutorialController != null && !tutorialController.CanPlacePiece(piece))
            {
                piece.ReturnToSlot();
                return;
            }

            if (!blockField.TryGetAnchorFor(piece, out Vector2Int anchor) || !blockField.CanInstall(piece, anchor))
            {
                piece.ReturnToSlot();
                return;
            }

            PlacementScoreGuard.PlacementScoreSnapshot snapshot = default;
            bool hasValidationSnapshot = placementScoreGuard != null;

            if (hasValidationSnapshot)
                snapshot = placementScoreGuard.Capture(blockField, piece, playerProgressController.Score, playerProgressController.Gold);

            piece.SnapTo(blockField.GetWorldPosition(anchor));
            blockField.Install(piece, anchor);

            playerProgressController.AddScore(piece.CellCount * 10, false);

            int clearedLines = blockField.ClearCompletedLines(_clearedBlockPositions);
            if (clearedLines > 0)
            {
                playerProgressController.AddScore(clearedLines * 100 + clearedLines * clearedLines * 50, false);
                lineClearEffectPlayer.Play(clearedLines, _clearedBlockPositions);
            }

            if (hasValidationSnapshot)
            {
                placementScoreGuard.Validate(
                    snapshot,
                    playerProgressController.Score,
                    playerProgressController.MaxScore,
                    playerProgressController.Gold,
                    playerProgressController.ApplyServerData);
            }

            piece.MarkPlaced();

            if (TryCompleteStage())
                return;

            if (randomBlockManager.AreAllPiecesPlaced() && !randomBlockManager.GiveNewSet(blockField))
            {
                EndGame();
                return;
            }

            if (!randomBlockManager.HasAnyAvailablePlacement(blockField))
            {
                EndGame();
                return;
            }

            SetMessage(string.Empty);
        }

        private bool ShiftBoard(Vector2Int direction)
        {
            if (boardShiftController == null ||
                !boardShiftController.TryShift(direction, _clearedBlockPositions, out BoardShiftController.BoardShiftResult result))
            {
                tutorialController?.NotifyBoardShiftFailed(direction);
                return false;
            }

            tutorialController?.NotifyBoardShiftSucceeded(direction);

            if (result.ClearedLines > 0)
            {
                playerProgressController.AddScore(result.ClearedLines * 90 + result.ClearedLines * result.ClearedLines * 40, true);
                lineClearEffectPlayer.Play(result.ClearedLines, _clearedBlockPositions);
            }

            if (TryCompleteStage())
                return true;

            if (!result.HasAnyRemainingPlacement)
            {
                EndGame();
                return true;
            }

            if (tutorialController == null || !tutorialController.HasPriorityMessage)
                SetMessage(result.HasVisibleChange ? "Shift" : string.Empty);

            return true;
        }

        private void SubscribePieces()
        {
            if (pieces == null)
                return;

            foreach (BlockPiece piece in pieces)
            {
                if (piece != null)
                    piece.Released += HandlePieceReleased;
            }
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

        private bool TryCompleteStage()
        {
            if (_isGameOver || stageModeController == null || !stageModeController.IsComplete(playerProgressController.Score))
                return false;

            CompleteStage();
            return true;
        }

        private void CompleteStage()
        {
            _isGameOver = true;
            placementPreview?.Hide();
            playerProgressController.TryUpdateMaxScore(true);
            SetMessage(stageModeController.GetClearMessage());

            if (gameOverView != null)
                gameOverView.ShowStageClear(playerProgressController.Score, playerProgressController.MaxScore);
        }

        private void EndGame()
        {
            _isGameOver = true;
            placementPreview?.Hide();
            playerProgressController.TryUpdateMaxScore(true);
            SetMessage(string.Empty);

            if (gameOverView != null)
                gameOverView.Show(playerProgressController.Score, playerProgressController.MaxScore);
            else
                SetMessage("Game Over");
        }

        private void UpdatePlacementPreview()
        {
            if (placementPreview == null)
                return;

            BlockPiece activePiece = BlockPiece.ActivePiece;
            if (_isGameOver ||
                activePiece == null ||
                tutorialController != null && tutorialController.BlocksPlacementPreview)
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
    }
}
