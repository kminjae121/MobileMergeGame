using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code.Manager
{
    public sealed class GameOverView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreLabelText;
        [SerializeField] private TextMeshProUGUI scoreValueText;
        [SerializeField] private TextMeshProUGUI bestScoreLabelText;
        [SerializeField] private TextMeshProUGUI bestScoreValueText;
        [SerializeField] private Button restartButton;

        private const string GameOverTitle = "GAME OVER";
        private const string StageClearTitle = "\uD074\uB9AC\uC5B4 \uD558\uC168\uC2B5\uB2C8\uB2E4";
        private const string StageFailedTitle = "\uAC8C\uC784\uC624\uBC84 \uD558\uC168\uC2B5\uB2C8\uB2E4";
        private const string ScoreLabel = "\uC810\uC218";
        private const string BestScoreLabel = "\uCD5C\uACE0\uC810\uC218";
        private const string RetryButtonLabel = "\uB2E4\uC2DC\uD558\uAE30";
        private const string NextButtonLabel = "\uB2E4\uC74C";
        private const string StageSelectButtonLabel = "\uC2A4\uD14C\uC774\uC9C0";

        private TextMeshProUGUI _primaryButtonLabelText;
        private PrimaryButtonAction _primaryButtonAction = PrimaryButtonAction.RestartCurrentScene;
        private string _targetSceneName;

        private enum PrimaryButtonAction
        {
            RestartCurrentScene,
            LoadScene
        }

        private void Awake()
        {
            ResolveOptionalReferences();

            if (restartButton != null)
                restartButton.onClick.AddListener(HandlePrimaryButtonClicked);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveListener(HandlePrimaryButtonClicked);
        }

        public void Show(int score, int bestScore)
        {
            Show(score, bestScore, GameOverTitle);
        }

        public void ShowStageClear(int score, int bestScore)
        {
            Show(score, bestScore, StageClearTitle);
        }

        public void ShowStageClearPrompt(int score, int bestScore, string nextSceneName)
        {
            ShowResult(
                StageClearTitle,
                ScoreLabel,
                score.ToString(),
                "\uB2E4\uC74C\uC73C\uB85C \uB118\uC5B4\uAC00\uC2DC\uACA0\uC2B5\uB2C8\uAE4C?",
                bestScore.ToString(),
                NextButtonLabel,
                nextSceneName);
        }

        public void ShowStageFailed(int score, int bestScore, string stageSelectSceneName)
        {
            ShowResult(
                StageFailedTitle,
                ScoreLabel,
                score.ToString(),
                BestScoreLabel,
                bestScore.ToString(),
                StageSelectButtonLabel,
                stageSelectSceneName);
        }

        private void Show(int score, int bestScore, string title)
        {
            ShowResult(
                title,
                ScoreLabel,
                score.ToString(),
                BestScoreLabel,
                bestScore.ToString(),
                RetryButtonLabel,
                null);
        }

        private void ShowResult(
            string title,
            string scoreLabel,
            string scoreValue,
            string bestScoreLabel,
            string bestScoreValue,
            string buttonLabel,
            string targetSceneName)
        {
            if (scoreValueText != null)
                scoreValueText.text = scoreValue;

            if (bestScoreValueText != null)
                bestScoreValueText.text = bestScoreValue;

            if (scoreLabelText != null)
                scoreLabelText.text = scoreLabel;

            if (bestScoreLabelText != null)
                bestScoreLabelText.text = bestScoreLabel;

            if (titleText != null)
                titleText.text = title;

            if (_primaryButtonLabelText != null)
                _primaryButtonLabelText.text = buttonLabel;

            _targetSceneName = targetSceneName;
            _primaryButtonAction = string.IsNullOrEmpty(targetSceneName)
                ? PrimaryButtonAction.RestartCurrentScene
                : PrimaryButtonAction.LoadScene;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandlePrimaryButtonClicked()
        {
            if (_primaryButtonAction == PrimaryButtonAction.LoadScene && !string.IsNullOrEmpty(_targetSceneName))
            {
                SceneManager.LoadScene(_targetSceneName);
                return;
            }

            RestartCurrentScene();
        }

        private static void RestartCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.buildIndex >= 0)
                SceneManager.LoadScene(currentScene.buildIndex);
            else
                SceneManager.LoadScene(currentScene.name);
        }

        private void ResolveOptionalReferences()
        {
            if (scoreLabelText == null)
                scoreLabelText = FindText("GameOverScoreLabelTxt");

            if (bestScoreLabelText == null)
                bestScoreLabelText = FindText("GameOverBestScoreLabelTxt");

            if (_primaryButtonLabelText == null && restartButton != null)
                _primaryButtonLabelText = restartButton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (_primaryButtonLabelText == null && restartButton != null)
                _primaryButtonLabelText = CreateButtonLabel(restartButton.transform);
        }

        private TextMeshProUGUI FindText(string objectName)
        {
            Transform child = transform.Find(objectName);
            return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        }

        private TextMeshProUGUI CreateButtonLabel(Transform parent)
        {
            GameObject labelObject = new GameObject("GameOverButtonLabelTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            if (titleText != null && titleText.font != null)
                text.font = titleText.font;

            if (titleText != null && titleText.fontSharedMaterial != null)
                text.fontSharedMaterial = titleText.fontSharedMaterial;

            text.text = RetryButtonLabel;
            text.fontSize = 42f;
            text.color = new Color(0.31f, 0.16f, 0.08f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }
    }
}
