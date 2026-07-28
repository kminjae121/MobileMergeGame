using System;
using _Code.Manager;
using _Code.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code.Menu
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private Button startBtn;
        [SerializeField] private JsonManager jsonManager;
        [SerializeField] private ServerScoreClient serverScoreClient;
        [SerializeField] private TextMeshProUGUI maxScoreTxt;

        private void Awake()
        {
            if (startBtn != null)
                startBtn.onClick.AddListener(StartGame);

            if (serverScoreClient == null)
                serverScoreClient = GetComponent<ServerScoreClient>();

            if (jsonManager != null)
            {
                jsonManager.Load();
                UpdateMaxScoreText(jsonManager.MaxScore);
            }

            if (serverScoreClient != null)
                serverScoreClient.FetchScore(ApplyServerScore);
        }

        private void OnDestroy()
        {
            if (startBtn != null)
                startBtn.onClick.RemoveListener(StartGame);
        }

        private void ApplyServerScore(int serverScore)
        {
            if (jsonManager == null)
            {
                UpdateMaxScoreText(serverScore);
                return;
            }

            jsonManager.TrySaveMaxScore(serverScore);
            UpdateMaxScoreText(jsonManager.MaxScore);
        }

        private void UpdateMaxScoreText(int maxScore)
        {
            if (maxScoreTxt != null)
                maxScoreTxt.text = $"\uCD5C\uB300\uC810\uC218 : {maxScore}";
        }

        public void StartGame()
        {
            if (string.IsNullOrEmpty(gameSceneName) == false)
            {
                SceneManager.LoadScene(gameSceneName);
                return;
            }

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
