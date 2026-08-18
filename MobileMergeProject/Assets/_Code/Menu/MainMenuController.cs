using System;
using System.Collections;
using _Code.Auth;
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
        [SerializeField] private Button googleLoginBtn;
        [SerializeField] private JsonManager jsonManager;
        [SerializeField] private ServerScoreClient serverScoreClient;
        [SerializeField] private GoogleLoginManager googleLoginManager;
        [SerializeField] private GoogleSignInTokenProvider googleSignInTokenProvider;
        [SerializeField] private TextMeshProUGUI maxScoreTxt;
        [SerializeField] private TextMeshProUGUI goldTxt;
        [SerializeField] private bool autoGoogleLoginOnStart = true;
        [SerializeField, Min(0f)] private float autoGoogleLoginDelay = 0.2f;

        private bool _autoGoogleLoginStarted;

        private void Awake()
        {
            if (startBtn != null)
                startBtn.onClick.AddListener(StartGame);

            if (stageBtn != null)
                stageBtn.onClick.AddListener(OpenStageScene);

            if (googleLoginBtn != null)
                googleLoginBtn.onClick.AddListener(StartGoogleLogin);

            if (serverScoreClient == null)
                serverScoreClient = GetComponent<ServerScoreClient>();

            if (googleLoginManager == null)
                googleLoginManager = GetComponent<GoogleLoginManager>();

            if (googleSignInTokenProvider == null)
                googleSignInTokenProvider = GetComponent<GoogleSignInTokenProvider>();

            if (googleLoginManager != null)
                googleLoginManager.LoggedIn += HandleLoggedIn;

            UpdateGameStartState();
            RefreshPlayerData();
        }

        private void Start()
        {
            UpdateGameStartState();

            if (!autoGoogleLoginOnStart)
                return;

#if UNITY_EDITOR
            return;
#else
            StartCoroutine(AutoGoogleLoginRoutine());
#endif
        }

        private void OnDestroy()
        {
            if (startBtn != null)
                startBtn.onClick.RemoveListener(StartGame);

            if (stageBtn != null)
                stageBtn.onClick.RemoveListener(OpenStageScene);

            if (googleLoginBtn != null)
                googleLoginBtn.onClick.RemoveListener(StartGoogleLogin);

            if (googleLoginManager != null)
                googleLoginManager.LoggedIn -= HandleLoggedIn;
        }

        public void RefreshPlayerData()
        {
            UpdateGameStartState();

            if (jsonManager != null)
            {
                jsonManager.Load();
                jsonManager.ApplyDailyGoldRewardIfAvailable();
                UpdatePlayerDataText();
            }

            if (serverScoreClient != null)
                serverScoreClient.FetchPlayerData(ApplyServerPlayerData, ApplyLocalPlayerDataOnly);
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
            if (!CanStartGame())
                return;

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
            if (!CanStartGame())
                return;

            if (!string.IsNullOrEmpty(stageSceneName))
                SceneManager.LoadScene(stageSceneName);
        }

        public void StartGoogleLogin()
        {
            if (googleSignInTokenProvider == null)
            {
                Debug.LogWarning("Google sign-in token provider is missing.");
                return;
            }

            googleSignInTokenProvider.SignIn();
        }

        private IEnumerator AutoGoogleLoginRoutine()
        {
            if (_autoGoogleLoginStarted)
                yield break;

            _autoGoogleLoginStarted = true;

            if (autoGoogleLoginDelay > 0f)
                yield return new WaitForSeconds(autoGoogleLoginDelay);

            if (googleSignInTokenProvider == null)
            {
                Debug.LogWarning("Google sign-in token provider is missing.");
                yield break;
            }

            googleSignInTokenProvider.SignInAutomatically();
        }

        private void HandleLoggedIn()
        {
            UpdateGameStartState();
            RefreshPlayerData();
        }

        private bool CanStartGame()
        {
            if (PlayerIdProvider.CanPlay)
                return true;

            Debug.LogWarning("로그인 전에는 게임을 시작할 수 없습니다.");
            StartGoogleLogin();
            return false;
        }

        private void UpdateGameStartState()
        {
            bool canStart = PlayerIdProvider.CanPlay;

            if (startBtn != null)
                startBtn.interactable = canStart;

            if (stageBtn != null)
                stageBtn.interactable = canStart;
        }
    }
}
