using System;
using _Code.Manager;
using _Code.Server;
using _Code.Stage;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code.Menu
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private string stageSceneName = "StageScene";
        [SerializeField] private Button startBtn;
        [SerializeField] private Button stageBtn;
        [SerializeField] private JsonManager jsonManager;
        [SerializeField] private ServerScoreClient serverScoreClient;
        [SerializeField] private TextMeshProUGUI maxScoreTxt;
        [SerializeField] private TextMeshProUGUI goldTxt;

        private void Awake()
        {
            if (startBtn != null)
                startBtn.onClick.AddListener(StartGame);

            if (stageBtn != null)
                stageBtn.onClick.AddListener(OpenStageScene);

            if (serverScoreClient == null)
                serverScoreClient = GetComponent<ServerScoreClient>();

            if (jsonManager != null)
            {
                jsonManager.Load();
                jsonManager.ApplyDailyGoldRewardIfAvailable();
                UpdatePlayerDataText();
            }

            if (serverScoreClient != null)
                serverScoreClient.FetchPlayerData(ApplyServerPlayerData, ApplyLocalPlayerDataOnly);
        }

        private void OnDestroy()
        {
            if (startBtn != null)
                startBtn.onClick.RemoveListener(StartGame);

            if (stageBtn != null)
                stageBtn.onClick.RemoveListener(OpenStageScene);
        }

        private void ApplyServerPlayerData(ServerScoreClient.PlayerData serverData)
        {
            if (jsonManager == null)
            {
                UpdateMaxScoreText(serverData.MaxScore);
                UpdateGoldText(serverData.Gold);
                return;
            }

            int localScore = jsonManager.MaxScore;
            int localGold = jsonManager.Gold;
            jsonManager.MergePlayerData(serverData.MaxScore, serverData.Gold, serverData.LastDailyGoldRewardDate);
            UpdatePlayerDataText();

            if (localScore > serverData.MaxScore || localGold > serverData.Gold)
                serverScoreClient.SubmitPlayerData(jsonManager.MaxScore, jsonManager.Gold);
        }

        private void ApplyLocalPlayerDataOnly()
        {
            if (jsonManager != null)
                UpdatePlayerDataText();
        }

        private void UpdatePlayerDataText()
        {
            if (jsonManager == null)
                return;

            UpdateGoldText(jsonManager.Gold);
            UpdateMaxScoreText(jsonManager.MaxScore);
        }

        private void UpdateMaxScoreText(int maxScore)
        {
            if (maxScoreTxt != null)
                maxScoreTxt.text = $"\uCD5C\uB300\uC810\uC218 : {maxScore}";
        }

        private void UpdateGoldText(int gold)
        {
            if (goldTxt != null)
                goldTxt.text = $"\uACE8\uB4DC : {gold}";
        }

        public void StartGame()
        {
            StageRunContext.SelectInfiniteMode();

            if (string.IsNullOrEmpty(gameSceneName) == false)
            {
                SceneManager.LoadScene(gameSceneName);
                return;
            }

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextSceneIndex);
        }

        public void OpenStageScene()
        {
            if (!string.IsNullOrEmpty(stageSceneName))
                SceneManager.LoadScene(stageSceneName);
        }
    }
}
