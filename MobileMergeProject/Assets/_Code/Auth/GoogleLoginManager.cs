using System;
using _Code.Manager;
using _Code.Server;
using TMPro;
using UnityEngine;

namespace _Code.Auth
{
    public sealed class GoogleLoginManager : MonoBehaviour
    {
        [SerializeField] private GoogleAuthClient authClient;
        [SerializeField] private JsonManager jsonManager;
        [SerializeField] private ServerScoreClient serverScoreClient;
        [SerializeField] private TextMeshProUGUI statusText;

        public event Action LoggedIn;

        private void Awake()
        {
            if (authClient == null)
                authClient = GetComponent<GoogleAuthClient>();

            if (jsonManager == null)
                jsonManager = GetComponent<JsonManager>();

            if (serverScoreClient == null)
                serverScoreClient = GetComponent<ServerScoreClient>();

            if (authClient != null)
                authClient.Configure(serverScoreClient, jsonManager);
        }

        public void LoginWithGoogleIdToken(string idToken)
        {
            if (authClient == null)
            {
                SetStatus("\uAD6C\uAE00 \uB85C\uADF8\uC778 \uD074\uB77C\uC774\uC5B8\uD2B8\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4.");
                return;
            }

            SetStatus("\uAD6C\uAE00 \uB85C\uADF8\uC778 \uC911...");
            authClient.LoginWithGoogleIdToken(idToken, ApplyLoginResult, HandleLoginFailed);
        }

        public void SignOutLocal()
        {
            PlayerIdProvider.ClearAuthenticatedPlayerId();
            SetStatus("\uB85C\uADF8\uC544\uC6C3\uB428");
        }

        private void ApplyLoginResult(GoogleAuthClient.GoogleLoginResult result)
        {
            PlayerIdProvider.SetAuthenticatedPlayerId(result.PlayerId);

            if (jsonManager != null && result.HasPlayerData)
            {
                jsonManager.MergePlayerData(
                    result.PlayerData.MaxScore,
                    result.PlayerData.Gold,
                    result.PlayerData.LastDailyGoldRewardDate);
            }

            SetStatus("\uAD6C\uAE00 \uB85C\uADF8\uC778 \uC131\uACF5");
            Debug.Log($"\uAD6C\uAE00 \uB85C\uADF8\uC778 \uC131\uACF5: {result.PlayerId}");
            LoggedIn?.Invoke();
        }

        private void HandleLoginFailed(string message)
        {
            SetStatus("\uAD6C\uAE00 \uB85C\uADF8\uC778 \uC2E4\uD328");
            Debug.LogWarning($"Google login failed. {message}");
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}
