using System.Collections.Generic;
using _Code.Block;
using _Code.Field;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MouseView = _Code.Mouse.Mouse;

namespace _Code.Manager
{
    public sealed class TutorialController : MonoBehaviour
    {
        private const string TutorialCompletedKey = "CatchTheCats.MouseMoveTutorial.Completed.v3";

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
        [SerializeField] private Color guideColor = new Color(1f, 0.78f, 0.18f, 1f);
        [SerializeField] private Color guideShadowColor = new Color(0.43f, 0.21f, 0.08f, 0.72f);
        [SerializeField, Min(4f)] private float guideLineWidth = 12f;
        [SerializeField, Min(18f)] private float guidePointerSize = 46f;
        [SerializeField, Min(2f)] private float guideFocusThickness = 6f;
        [SerializeField, Min(0f)] private float guideFocusPadding = 12f;
        [SerializeField, Min(0.1f)] private float guideAnimationSpeed = 1.35f;

        private readonly List<TutorialStep> _steps = new List<TutorialStep>();
        private readonly List<FocusBox> _focusBoxes = new List<FocusBox>();
        private readonly List<Vector3> _placementTargetPositions = new List<Vector3>(9);

        private GameObject _root;
        private RectTransform _rootRect;
        private RectTransform _guideRootRect;
        private Image _guideShaft;
        private TextMeshProUGUI _guideHeadText;
        private Image _guidePointer;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _bodyText;
        private TextMeshProUGUI _progressText;
        private TextMeshProUGUI _nextButtonText;
        private Button _nextButton;
        private Button _closeButton;
        private int _stepIndex;
        private bool _isRunning;
        private bool _isManualRun;
        private bool _guideVisible;
        private Vector2 _guideStartLocal;
        private Vector2 _guideEndLocal;
        private BlockField _blockField;
        private MouseView _mouse;
        private BlockPiece[] _pieces;
        private Camera _worldCamera;
        private Sprite _guidePointerSprite;

        public bool HasPriorityMessage => _isRunning;

        private void Awake()
        {
            BuildSteps();
        }

        private void Update()
        {
            if (!_isRunning || _root == null || !_root.activeSelf)
                return;

            RefreshGuide();
            AnimateGuide();
        }

        private void OnDisable()
        {
            HideView();
            ClearMessage();
        }

        public void Configure(TextMeshProUGUI messageText)
        {
            Configure(messageText, _blockField, _mouse, _pieces);
        }

        public void Configure(TextMeshProUGUI messageText, BlockField blockField, MouseView mouse, BlockPiece[] pieces)
        {
            if (this.messageText == null)
                this.messageText = messageText;

            if (_blockField == null)
                _blockField = blockField;

            if (_mouse == null)
                _mouse = mouse;

            if (_pieces == null || _pieces.Length == 0)
                _pieces = pieces;

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
            RefreshGuide();
        }

        public void NotifyBoardShiftFailed(Vector2Int direction)
        {
            if (!_isRunning || CurrentStep.Kind != StepKind.WaitForShift)
                return;

            SetTemporaryHint("그쪽 쥐구멍으로는 갈 수 없어요. 화살표 방향으로 다시 밀어봐요.");
            RefreshGuide();
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

            RefreshGuide();
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

            HideGuide();
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
            _rootRect = _root.GetComponent<RectTransform>();
            _rootRect.SetParent(targetCanvas.transform, false);
            _rootRect.anchorMin = Vector2.zero;
            _rootRect.anchorMax = Vector2.one;
            _rootRect.offsetMin = Vector2.zero;
            _rootRect.offsetMax = Vector2.zero;

            CreateGuideView(_rootRect);

            GameObject panelObject = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.SetParent(_rootRect, false);
            panelRect.anchorMin = new Vector2(0.08f, 0.76f);
            panelRect.anchorMax = new Vector2(0.92f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = panelColor;
            panelImage.raycastTarget = false;

            Outline panelOutline = panelObject.GetComponent<Outline>();
            panelOutline.effectColor = panelOutlineColor;
            panelOutline.effectDistance = new Vector2(4f, -4f);

            _titleText = CreateText(panelRect, "TutorialTitleTxt", new Vector2(0.05f, 0.56f), new Vector2(0.66f, 0.92f), 42f, titleColor, TextAlignmentOptions.Left);
            _bodyText = CreateText(panelRect, "TutorialBodyTxt", new Vector2(0.05f, 0.2f), new Vector2(0.78f, 0.58f), 34f, bodyColor, TextAlignmentOptions.Left);
            _progressText = CreateText(panelRect, "TutorialProgressTxt", new Vector2(0.05f, 0.03f), new Vector2(0.28f, 0.18f), 24f, bodyColor, TextAlignmentOptions.Left);

            _nextButton = CreateButton(panelRect, "TutorialNextButton", new Vector2(0.72f, 0.14f), new Vector2(0.94f, 0.48f), "다음");
            _nextButton.onClick.AddListener(AdvanceStep);
            _nextButtonText = _nextButton.GetComponentInChildren<TextMeshProUGUI>();

            _closeButton = CreateButton(panelRect, "TutorialCloseButton", new Vector2(0.88f, 0.58f), new Vector2(0.96f, 0.88f), "X");
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

        private void CreateGuideView(RectTransform rootRect)
        {
            GameObject guideRoot = new GameObject("TutorialGuide", typeof(RectTransform), typeof(CanvasGroup));
            _guideRootRect = guideRoot.GetComponent<RectTransform>();
            _guideRootRect.SetParent(rootRect, false);
            _guideRootRect.anchorMin = Vector2.zero;
            _guideRootRect.anchorMax = Vector2.one;
            _guideRootRect.offsetMin = Vector2.zero;
            _guideRootRect.offsetMax = Vector2.zero;

            CanvasGroup canvasGroup = guideRoot.GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            _guideShaft = CreateGuideImage(_guideRootRect, "TutorialSwipeArrowShaft", guideColor);
            _guideShaft.rectTransform.pivot = new Vector2(0f, 0.5f);

            _guideHeadText = CreateGuideText(_guideRootRect, "TutorialSwipeArrowHead", ">", 72f, guideColor);
            _guidePointer = CreateGuideImage(_guideRootRect, "TutorialSwipePointer", Color.white);
            _guidePointer.sprite = GetGuidePointerSprite();
            _guidePointer.rectTransform.sizeDelta = Vector2.one * guidePointerSize;

            HideGuide();
        }

        private Image CreateGuideImage(RectTransform parent, string objectName, Color color)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Outline));
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = Vector2.one * 40f;

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            Outline outline = imageObject.GetComponent<Outline>();
            outline.effectColor = guideShadowColor;
            outline.effectDistance = new Vector2(3f, -3f);
            return image;
        }

        private TextMeshProUGUI CreateGuideText(RectTransform parent, string objectName, string value, float fontSize, Color color)
        {
            TextMeshProUGUI text = CreateText(parent, objectName, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), fontSize, color, TextAlignmentOptions.Center);
            text.text = value;
            text.rectTransform.sizeDelta = Vector2.one * fontSize;
            text.raycastTarget = false;
            return text;
        }

        private void RefreshGuide()
        {
            HideGuide();

            if (!_isRunning || _rootRect == null)
                return;

            TutorialStep step = CurrentStep;

            if (step.Kind == StepKind.WaitForShift)
            {
                ShowSwipeGuide(step.Direction);
                return;
            }

            if (step.Kind == StepKind.WaitForPiecePlaced)
                ShowPlacementGuide();
        }

        private void HideGuide()
        {
            _guideVisible = false;

            if (_guideRootRect != null)
                _guideRootRect.gameObject.SetActive(false);

            for (int i = 0; i < _focusBoxes.Count; i++)
            {
                if (_focusBoxes[i] != null && _focusBoxes[i].IsAlive)
                    _focusBoxes[i].SetActive(false);
            }
        }

        private void ShowSwipeGuide(Vector2Int direction)
        {
            if (_mouse == null || _blockField == null)
                return;

            if (!_mouse.TryGetMoveGuideWorldPositions(direction, _blockField, out Vector3 fromWorld, out Vector3 toWorld))
                return;

            if (!TryWorldToCanvasPoint(fromWorld, out Vector2 fromLocal) ||
                !TryWorldToCanvasPoint(toWorld, out Vector2 toLocal))
                return;

            SetGuideArrow(fromLocal, toLocal);
            ShowFocusBox(0, fromWorld, _blockField.CellSize * 1.55f);
            ShowFocusBox(1, toWorld, _blockField.CellSize * 1.75f);
        }

        private void ShowPlacementGuide()
        {
            if (!TryFindPlacementGuide(out Vector3 pieceWorldPosition, out Vector3 targetWorldPosition))
                return;

            if (!TryWorldToCanvasPoint(pieceWorldPosition, out Vector2 pieceLocal) ||
                !TryWorldToCanvasPoint(targetWorldPosition, out Vector2 targetLocal))
                return;

            SetGuideArrow(pieceLocal, targetLocal);

            for (int i = 0; i < _placementTargetPositions.Count; i++)
                ShowFocusBox(i, _placementTargetPositions[i], _blockField.CellSize * 0.96f);
        }

        private bool TryFindPlacementGuide(out Vector3 pieceWorldPosition, out Vector3 targetWorldPosition)
        {
            pieceWorldPosition = Vector3.zero;
            targetWorldPosition = Vector3.zero;
            _placementTargetPositions.Clear();

            if (_blockField == null || _pieces == null)
                return false;

            foreach (BlockPiece piece in _pieces)
            {
                if (piece == null || piece.IsPlaced || !piece.gameObject.activeInHierarchy)
                    continue;

                for (int y = 0; y < _blockField.Height; y++)
                {
                    for (int x = 0; x < _blockField.Width; x++)
                    {
                        Vector2Int anchor = new Vector2Int(x, y);

                        if (!_blockField.CanInstall(piece, anchor))
                            continue;

                        pieceWorldPosition = piece.transform.position;
                        Vector3 total = Vector3.zero;

                        foreach (Vector2Int cell in piece.Cells)
                        {
                            Vector3 cellWorldPosition = _blockField.GetWorldPosition(anchor + cell);
                            _placementTargetPositions.Add(cellWorldPosition);
                            total += cellWorldPosition;
                        }

                        targetWorldPosition = total / Mathf.Max(1, _placementTargetPositions.Count);
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetGuideArrow(Vector2 fromLocal, Vector2 toLocal)
        {
            if (_guideRootRect == null || _guideShaft == null || _guideHeadText == null || _guidePointer == null)
                return;

            Vector2 delta = toLocal - fromLocal;
            float length = delta.magnitude;

            if (length < 24f)
                return;

            Vector2 direction = delta.normalized;
            _guideStartLocal = fromLocal + direction * Mathf.Min(52f, length * 0.22f);
            _guideEndLocal = toLocal - direction * Mathf.Min(46f, length * 0.18f);
            Vector2 guideDelta = _guideEndLocal - _guideStartLocal;
            float guideLength = guideDelta.magnitude;
            float angle = Mathf.Atan2(guideDelta.y, guideDelta.x) * Mathf.Rad2Deg;

            _guideRootRect.gameObject.SetActive(true);
            _guideVisible = true;

            RectTransform shaftRect = _guideShaft.rectTransform;
            shaftRect.anchoredPosition = _guideStartLocal;
            shaftRect.sizeDelta = new Vector2(Mathf.Max(24f, guideLength - 34f), guideLineWidth);
            shaftRect.localRotation = Quaternion.Euler(0f, 0f, angle);

            RectTransform headRect = _guideHeadText.rectTransform;
            headRect.anchoredPosition = _guideEndLocal;
            headRect.localRotation = Quaternion.Euler(0f, 0f, angle);

            _guidePointer.rectTransform.anchoredPosition = _guideStartLocal;
        }

        private void AnimateGuide()
        {
            if (!_guideVisible || _guidePointer == null)
                return;

            float travel = Mathf.PingPong(Time.unscaledTime * guideAnimationSpeed, 1f);
            float easedTravel = Mathf.SmoothStep(0f, 1f, travel);
            RectTransform pointerRect = _guidePointer.rectTransform;
            pointerRect.anchoredPosition = Vector2.Lerp(_guideStartLocal, _guideEndLocal, easedTravel);

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.08f;
            pointerRect.localScale = Vector3.one * pulse;
        }

        private void ShowFocusBox(int index, Vector3 worldPosition, float worldSize)
        {
            if (!TryWorldToCanvasPoint(worldPosition, out Vector2 localPosition))
                return;

            FocusBox focusBox = GetFocusBox(index);
            Vector2 size = WorldSizeToCanvasSize(worldPosition, worldSize) + Vector2.one * guideFocusPadding;
            focusBox.Set(localPosition, size, guideColor, guideFocusThickness);
            focusBox.SetActive(true);
        }

        private FocusBox GetFocusBox(int index)
        {
            while (_focusBoxes.Count <= index)
                _focusBoxes.Add(new FocusBox(_guideRootRect, $"TutorialFocusBox{_focusBoxes.Count}"));

            return _focusBoxes[index];
        }

        private bool TryWorldToCanvasPoint(Vector3 worldPosition, out Vector2 localPoint)
        {
            localPoint = Vector2.zero;

            if (_rootRect == null)
                return false;

            if (_worldCamera == null)
                _worldCamera = Camera.main;

            if (_worldCamera == null)
                return false;

            Camera canvasCamera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? targetCanvas.worldCamera != null ? targetCanvas.worldCamera : _worldCamera
                : null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_worldCamera, worldPosition);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_rootRect, screenPoint, canvasCamera, out localPoint);
        }

        private Vector2 WorldSizeToCanvasSize(Vector3 worldPosition, float worldSize)
        {
            if (!TryWorldToCanvasPoint(worldPosition, out Vector2 center) ||
                !TryWorldToCanvasPoint(worldPosition + Vector3.right * worldSize, out Vector2 right) ||
                !TryWorldToCanvasPoint(worldPosition + Vector3.up * worldSize, out Vector2 up))
                return Vector2.one * 72f;

            return new Vector2(
                Mathf.Max(32f, Mathf.Abs(right.x - center.x)),
                Mathf.Max(32f, Mathf.Abs(up.y - center.y)));
        }

        private Sprite GetGuidePointerSprite()
        {
            if (_guidePointerSprite != null)
                return _guidePointerSprite;

            const int textureSize = 64;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                name = "TutorialGuidePointer",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
            float radius = textureSize * 0.42f;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(radius - distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            _guidePointerSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
            return _guidePointerSprite;
        }

        private void BuildSteps()
        {
            if (_steps.Count > 0)
                return;

            _steps.Add(new TutorialStep(
                "쥐 이동하기",
                "화면을 밀면 쥐가 쥐구멍 사이를 이동하고, 고양이 블럭들도 같은 방향으로 밀려요.",
                "쥐를 움직여볼까요?",
                StepKind.Message,
                Vector2Int.zero));

            _steps.Add(new TutorialStep(
                "오른쪽으로 밀기",
                "노란 화살표처럼 오른쪽으로 스와이프해 쥐를 이동시켜봐요.",
                "오른쪽으로 밀기",
                StepKind.WaitForShift,
                Vector2Int.right));

            _steps.Add(new TutorialStep(
                "아래로 내려가기",
                "이번에는 아래로 스와이프해서 아래 쥐구멍으로 이동해요.",
                "아래로 밀기",
                StepKind.WaitForShift,
                Vector2Int.down));

            _steps.Add(new TutorialStep(
                "왼쪽으로 밀기",
                "왼쪽으로 스와이프하면 고양이 블럭들이 같이 정리돼요.",
                "왼쪽으로 밀기",
                StepKind.WaitForShift,
                Vector2Int.left));

            _steps.Add(new TutorialStep(
                "다시 위로",
                "마지막으로 위로 스와이프해서 쥐를 위쪽 쥐구멍으로 보내요.",
                "위로 밀기",
                StepKind.WaitForShift,
                Vector2Int.up));

            _steps.Add(new TutorialStep(
                "고양이 배치",
                "아래 고양이 블럭을 잡고 노란 칸에 맞춰 놓아보세요.",
                "고양이 놓기",
                StepKind.WaitForPiecePlaced,
                Vector2Int.zero));

            _steps.Add(new TutorialStep(
                "준비 완료",
                "좋아요. 이제 쥐를 움직여 공간을 만들고 고양이 블럭을 배치해 점수를 올리면 돼요.",
                "튜토리얼 완료",
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

        private sealed class FocusBox
        {
            private readonly RectTransform _rectTransform;
            private readonly Image[] _edges;

            public bool IsAlive => _rectTransform != null;

            public FocusBox(RectTransform parent, string objectName)
            {
                GameObject root = new GameObject(objectName, typeof(RectTransform));
                _rectTransform = root.GetComponent<RectTransform>();
                _rectTransform.SetParent(parent, false);
                _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                _edges = new[]
                {
                    CreateEdge(_rectTransform, "Top"),
                    CreateEdge(_rectTransform, "Bottom"),
                    CreateEdge(_rectTransform, "Left"),
                    CreateEdge(_rectTransform, "Right")
                };

                SetActive(false);
            }

            public void Set(Vector2 center, Vector2 size, Color color, float thickness)
            {
                if (!IsAlive)
                    return;

                _rectTransform.anchoredPosition = center;
                _rectTransform.sizeDelta = size;

                ConfigureHorizontal(_edges[0].rectTransform, true, thickness);
                ConfigureHorizontal(_edges[1].rectTransform, false, thickness);
                ConfigureVertical(_edges[2].rectTransform, false, thickness);
                ConfigureVertical(_edges[3].rectTransform, true, thickness);

                foreach (Image edge in _edges)
                {
                    if (edge != null)
                        edge.color = color;
                }
            }

            public void SetActive(bool active)
            {
                if (!IsAlive)
                    return;

                _rectTransform.gameObject.SetActive(active);
            }

            private static Image CreateEdge(RectTransform parent, string objectName)
            {
                GameObject edgeObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
                RectTransform rectTransform = edgeObject.GetComponent<RectTransform>();
                rectTransform.SetParent(parent, false);

                Image image = edgeObject.GetComponent<Image>();
                image.raycastTarget = false;
                return image;
            }

            private static void ConfigureHorizontal(RectTransform rectTransform, bool isTop, float thickness)
            {
                rectTransform.anchorMin = new Vector2(0f, isTop ? 1f : 0f);
                rectTransform.anchorMax = new Vector2(1f, isTop ? 1f : 0f);
                rectTransform.pivot = new Vector2(0.5f, isTop ? 1f : 0f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(0f, thickness);
            }

            private static void ConfigureVertical(RectTransform rectTransform, bool isRight, float thickness)
            {
                rectTransform.anchorMin = new Vector2(isRight ? 1f : 0f, 0f);
                rectTransform.anchorMax = new Vector2(isRight ? 1f : 0f, 1f);
                rectTransform.pivot = new Vector2(isRight ? 1f : 0f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(thickness, 0f);
            }
        }
    }
}
