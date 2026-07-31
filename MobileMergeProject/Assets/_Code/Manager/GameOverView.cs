using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code.Manager
{
    public sealed class GameOverView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreValueText;
        [SerializeField] private TextMeshProUGUI bestScoreValueText;
        [SerializeField] private Button restartButton;

        private const string GameOverTitle = "GAME OVER";
        private const string StageClearTitle = "STAGE CLEAR";

        private void Awake()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartCurrentScene);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveListener(RestartCurrentScene);
        }

        public void Show(int score, int bestScore)
        {
            Show(score, bestScore, GameOverTitle);
        }

        public void ShowStageClear(int score, int bestScore)
        {
            Show(score, bestScore, StageClearTitle);
        }

        private void Show(int score, int bestScore, string title)
        {
            if (scoreValueText != null)
                scoreValueText.text = score.ToString();

            if (bestScoreValueText != null)
                bestScoreValueText.text = bestScore.ToString();

            if (titleText != null)
                titleText.text = title;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static void RestartCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();

            if (currentScene.buildIndex >= 0)
                SceneManager.LoadScene(currentScene.buildIndex);
            else
                SceneManager.LoadScene(currentScene.name);
        }
    }
}
