using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code.Manager
{
    public sealed class GameOverView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreValueText;
        [SerializeField] private TextMeshProUGUI bestScoreValueText;
        [SerializeField] private Button restartButton;

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
            if (scoreValueText != null)
                scoreValueText.text = score.ToString();

            if (bestScoreValueText != null)
                bestScoreValueText.text = bestScore.ToString();

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
