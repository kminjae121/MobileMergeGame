using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Manager
{
    public sealed class TutorialController : MonoBehaviour
    {
        private const string TutorialCompletedKey = "CatchTheCats.MouseMoveTutorial.Completed.v2";

        private enum StepKind
        {
            Message,
            WaitForShift,
            WaitForPiecePlaced,
            Finish
        }

        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private bool showFirstRunTutorial = true;
        [SerializeField] private Color panelColor = new Color(1f, 0.93f, 0.78f, 0.96f);
        [SerializeField] private Color panelOutlineColor = new Color(0.58f, 0.28f, 0.16f, 1f);
        [SerializeField] private Color titleColor = new Color(0.31f, 0.16f, 0.08f, 1f);
        [SerializeField] private Color bodyColor = new Color(0.24f, 0.17f, 0.12f, 1f);

        private readonly List<TutorialStep> _steps = new List<TutorialStep>();

        private GameObject _root;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _bodyText;
        private TextMeshProUGUI _progressText;
        private TextMeshProUGUI _nextButtonText;
        private Button _nextButton;
        private Button _closeButton;
        private int _stepIndex;
        private bool _isRunning;
        private bool _isManualRun;

        public bool HasPriorityMessage => _isRunning;

        private void Awake()
        {
            BuildSteps();
        }

        private void OnDisable()
        {
            HideView();
            ClearMessage();
        }

        public void Configure(TextMeshProUGUI messageText)
        {
            if (this.messageText == null)
                this.messageText = messageText;

            EnsureView();
            HideView();
        }

        public bool TryBegin(bool isStageMode)
        {
            if (!showFirstRunTutorial || PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1)
                return false;

            Begin(false);
            return true;
        }

        public void BeginManually()
        {
            Begin(true);
        }

        public void NotifyBoardShiftSucceeded(Vector2Int direction)
        {
            if (!_isRunning || CurrentStep.Kind != StepKind.WaitForShift)
                return;

            direction = NormalizeDirection(direction);

            if (direction == CurrentStep.Direction)
            {
                AdvanceStep();
                return;
            }

            SetTemporaryHint($"{DirectionToKorean(CurrentStep.Direction)} 방향으로 밀어볼게요.");
        }

        public void NotifyBoardShiftFailed(Vector2Int direction)
        {
            if (!_isRunning || CurrentStep.Kind != StepKind.WaitForShift)
                return;

            SetTemporaryHint("그쪽 쥐구멍으로는 갈 수 없어요. 화살표 방향으로 다시 밀어봐요.");
        }

        public void NotifyPiecePlaced()
        {
            if (!_isRunning || CurrentStep.Kind != StepKind.WaitForPiecePlaced)
                return;

            AdvanceStep();
        }

        private TutorialStep CurrentStep => _steps[Mathf.Clamp(_stepIndex, 0, _steps.Count - 1)];

        private void Begin(bool isManualRun)
        {
            BuildSteps();

            if (_steps.Count == 0)
                return;

            _isRunning = true;
            _isManualRun = isManualRun;
            _stepIndex = 0;
            EnsureView();
            ShowCurrentStep();
        }

        private void AdvanceStep()
        {
            if (!_isRunning)
                return;

            _stepIndex++;

            if (_stepIndex >= _steps.Count)
            {
                Complete();
                return;
            }

            ShowCurrentStep();
        }

        private void Complete()
        {
            PlayerPrefs.SetInt(TutorialCompletedKey, 1);
            PlayerPrefs.Save();

            _isRunning = false;
            HideView();
            ClearMessage();
        }

        private void Close()
        {
            if (!_isManualRun)
            {
                PlayerPrefs.SetInt(TutorialCompletedKey, 1);
                PlayerPrefs.Save();
            }

            _isRunning = false;
            HideView();
            ClearMessage();
        }

        private void ShowCurrentStep()
        {
            EnsureView();

            if (_root == null)
                return;

            TutorialStep step = CurrentStep;
            _root.SetActive(true);

            if (_titleText != null)
                _titleText.text = step.Title;

            if (_bodyText != null)
                _bodyText.text = step.Body;

            if (_progressText != null)
                _progressText.text = $"{_stepIndex + 1}/{_steps.Count}";

            if (_nextButton != null)
                _nextButton.gameObject.SetActive(step.Kind == StepKind.Message || step.Kind == StepKind.Finish);

            if (_nextButtonText != null)
                _nextButtonText.text = step.Kind == StepKind.Finish ? "끝내기" : "다음";

            if (messageText != null)
                messageText.text = step.ShortMessage;
        }

        private void SetTemporaryHint(string hint)
        {
            if (_bodyText != null)
                _bodyText.text = $"{CurrentStep.Body}\n<size=78%><color=#8A3D25>{hint}</color></size>";

            if (messageText != null)
                messageText.text = hint;
        }

        private void HideView()
        {
            if (_root != null)
                _root.SetActive(false);
        }

        private void ClearMessage()
        {
            if (messageText != null)
                messageText.text = string.Empty;
        }

        private void EnsureView()
        {
            if (_root != null)
                return;

            if (targetCanvas == null)
                targetCanvas = FindFirstObjectByType<Canvas>();

            if (targetCanvas == null)
                return;

            _root = new GameObject("TutorialOverlay", typeof(RectTransform));
            RectTransform rootRect = _root.GetComponent<RectTransform>();
            rootRect.SetParent(targetCanvas.transform, false);
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            GameObject panelObject = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.SetParent(rootRect, false);
            panelRect.anchorMin = new Vector2(0.14f, 0.77f);
            panelRect.anchorMax = new Vector2(0.86f, 0.91f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = panelColor;

            Outline panelOutline = panelObject.GetComponent<Outline>();
            panelOutline.effectColor = panelOutlineColor;
            panelOutline.effectDistance = new Vector2(4f, -4f);

            _titleText = CreateText(panelRect, "TutorialTitleTxt", new Vector2(0.05f, 0.62f), new Vector2(0.78f, 0.94f), 60f, titleColor, TextAlignmentOptions.Left);
            _bodyText = CreateText(panelRect, "TutorialBodyTxt", new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.62f), 55f, bodyColor, TextAlignmentOptions.Left);
            _progressText = CreateText(panelRect, "TutorialProgressTxt", new Vector2(0.05f, 0.03f), new Vector2(0.35f, 0.2f), 32f, bodyColor, TextAlignmentOptions.Left);

            _nextButton = CreateButton(panelRect, "TutorialNextButton", new Vector2(0.68f, 0.04f), new Vector2(0.92f, 0.22f), "다음");
            _nextButton.onClick.AddListener(AdvanceStep);
            _nextButtonText = _nextButton.GetComponentInChildren<TextMeshProUGUI>();

            _closeButton = CreateButton(panelRect, "TutorialCloseButton", new Vector2(0.88f, 0.64f), new Vector2(0.96f, 0.9f), "X");
            _closeButton.onClick.AddListener(Close);

            HideView();
        }

        private TextMeshProUGUI CreateText(
            RectTransform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            if (messageText != null && messageText.font != null)
                text.font = messageText.font;

            if (messageText != null && messageText.fontSharedMaterial != null)
                text.fontSharedMaterial = messageText.fontSharedMaterial;

            text.fontSize = fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(12f, fontSize * 0.55f);
            text.fontSizeMax = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private Button CreateButton(
            RectTransform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            string label)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(1f, 0.98f, 0.9f, 1f);

            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = panelOutlineColor;
            outline.effectDistance = new Vector2(2f, -2f);

            TextMeshProUGUI text = CreateText(rectTransform, $"{objectName}LabelTxt", Vector2.zero, Vector2.one, 36f, titleColor, TextAlignmentOptions.Center);
            text.text = label;

            return buttonObject.GetComponent<Button>();
        }

        private void BuildSteps()
        {
            if (_steps.Count > 0)
                return;

            _steps.Add(new TutorialStep(
                "쥐구멍 이동",
                "쥐는 네 모서리의 쥐구멍을 오가요. 화면을 밀면 쥐가 그 방향의 쥐구멍으로 이동하고, 고양이 블럭들도 같이 밀려요.",
                "Swipe to move the mouse.",
                StepKind.Message,
                Vector2Int.zero));

            _steps.Add(new TutorialStep(
                "오른쪽으로 밀기",
                "화면을 오른쪽으로 스와이프해 쥐를 오른쪽 쥐구멍으로 이동시켜봐요.",
                "오른쪽으로 스와이프",
                StepKind.WaitForShift,
                Vector2Int.right));

            _steps.Add(new TutorialStep(
                "아래로 내려가기",
                "이번에는 아래로 스와이프해 쥐를 아래 쥐구멍으로 내려보내요.",
                "아래로 스와이프",
                StepKind.WaitForShift,
                Vector2Int.down));

            _steps.Add(new TutorialStep(
                "왼쪽으로 밀기",
                "왼쪽으로 스와이프하면 쥐가 왼쪽 쥐구멍으로 이동해요. 블럭 움직임도 같이 확인해봐요.",
                "왼쪽으로 스와이프",
                StepKind.WaitForShift,
                Vector2Int.left));

            _steps.Add(new TutorialStep(
                "다시 위로",
                "마지막으로 위로 스와이프해 처음 위치로 돌아가봐요.",
                "위로 스와이프",
                StepKind.WaitForShift,
                Vector2Int.up));

            _steps.Add(new TutorialStep(
                "고양이 배치",
                "쥐를 움직여 공간을 만든 뒤, 아래 고양이 블럭을 잡아 빈 칸에 놓아보세요. 놓을 수 있는 칸이면 미리보기가 보여요.",
                "고양이 블럭 배치",
                StepKind.WaitForPiecePlaced,
                Vector2Int.zero));

            _steps.Add(new TutorialStep(
                "준비 완료",
                "좋아요. 이제 쥐를 움직여 판을 정리하고, 고양이 블럭을 배치해서 점수를 올리면 돼요.",
                "Tutorial Complete",
                StepKind.Finish,
                Vector2Int.zero));
        }

        private static Vector2Int NormalizeDirection(Vector2Int direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return direction.x > 0 ? Vector2Int.right : Vector2Int.left;

            return direction.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        private static string DirectionToKorean(Vector2Int direction)
        {
            if (direction == Vector2Int.right)
                return "오른쪽";

            if (direction == Vector2Int.left)
                return "왼쪽";

            if (direction == Vector2Int.up)
                return "위";

            return "아래";
        }

        private readonly struct TutorialStep
        {
            public TutorialStep(string title, string body, string shortMessage, StepKind kind, Vector2Int direction)
            {
                Title = title;
                Body = body;
                ShortMessage = shortMessage;
                Kind = kind;
                Direction = direction;
            }

            public string Title { get; }
            public string Body { get; }
            public string ShortMessage { get; }
            public StepKind Kind { get; }
            public Vector2Int Direction { get; }
        }
    }
}
