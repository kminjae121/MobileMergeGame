using _Code.Block;
using TMPro;
using UnityEngine;

namespace _Code.Manager
{
    public sealed class TutorialController : MonoBehaviour
    {
        private const string TutorialCompletedKey = "CatBlast.Tutorial.MouseMove.Completed";
        private const string StartMessage = "튜토리얼: 화면을 밀어서 쥐를 다른 쥐구멍으로 움직여봐요!";
        private const string BlockPlacementMessage = "먼저 쥐를 움직여볼게요. 화면을 크게 밀어봐요.";
        private const string FailedShiftMessage = "그 방향으로는 쥐가 못 움직여요. 다른 방향으로 밀어봐요!";
        private const string CompleteMessage = "좋아요! 쥐가 움직이면 고양이 블럭들도 같은 방향으로 밀려요.";

        [SerializeField] private bool tutorialEnabled = true;
        [SerializeField] private bool onlyInInfiniteMode = true;
        [SerializeField] private bool forceTutorialEveryPlay;
        [SerializeField, Min(0.5f)] private float completionMessageSeconds = 2.8f;
        [SerializeField] private TextMeshProUGUI messageText;

        private float _messageClearTime;
        private bool _isActive;

        public bool IsActive => _isActive;
        public bool BlocksPiecePlacement => _isActive;
        public bool BlocksPlacementPreview => _isActive;
        public bool HasPriorityMessage => _isActive || _messageClearTime > 0f;

        private void Update()
        {
            if (_messageClearTime <= 0f || Time.unscaledTime < _messageClearTime)
                return;

            _messageClearTime = 0f;
            SetMessage(string.Empty);
        }

        public void Configure(TextMeshProUGUI messageText)
        {
            if (this.messageText == null)
                this.messageText = messageText;
        }

        public bool TryBegin(bool isStageMode)
        {
            if (!ShouldStart(isStageMode))
                return false;

            Begin();
            return true;
        }

        public void BeginManually()
        {
            if (!tutorialEnabled)
                return;

            Begin();
        }

        public bool CanPlacePiece(BlockPiece piece)
        {
            if (!_isActive)
                return true;

            SetMessage(BlockPlacementMessage);
            return false;
        }

        public void NotifyBoardShiftFailed(Vector2Int direction)
        {
            if (!_isActive || direction == Vector2Int.zero)
                return;

            SetMessage(FailedShiftMessage);
        }

        public void NotifyBoardShiftSucceeded(Vector2Int direction)
        {
            if (!_isActive || direction == Vector2Int.zero)
                return;

            Complete();
        }

        [ContextMenu("Reset Tutorial")]
        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(TutorialCompletedKey);
            PlayerPrefs.Save();
        }

        private bool ShouldStart(bool isStageMode)
        {
            if (!tutorialEnabled)
                return false;

            if (onlyInInfiniteMode && isStageMode)
                return false;

            return forceTutorialEveryPlay || PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 0;
        }

        private void Begin()
        {
            _isActive = true;
            _messageClearTime = 0f;
            SetMessage(StartMessage);
        }

        private void Complete()
        {
            _isActive = false;
            PlayerPrefs.SetInt(TutorialCompletedKey, 1);
            PlayerPrefs.Save();
            SetMessage(CompleteMessage);
            _messageClearTime = Time.unscaledTime + completionMessageSeconds;
        }

        private void SetMessage(string message)
        {
            if (messageText != null)
                messageText.text = message; 

            if (!string.IsNullOrWhiteSpace(message))
                Debug.Log(message);
        }
    }
}
