using System;
using System.Collections;
using System.Text;
using _Code.Manager;
using _Code.Server;
using UnityEngine;
using UnityEngine.Networking;

namespace _Code.Auth
{
    public sealed class GoogleAuthClient : MonoBehaviour
    {
        [SerializeField] private string _baseUrl;
        [SerializeField] private string _googleLoginPath = "/auth/google";
        [SerializeField] private ServerScoreClient _serverScoreClient;
        [SerializeField] private JsonManager _jsonManager;
        [SerializeField, Min(1)] private int _timeoutSeconds = 10;
        [SerializeField] private bool _logRequests;

        public void Configure(ServerScoreClient serverScoreClient, JsonManager jsonManager)
        {
            if (_serverScoreClient == null)
                _serverScoreClient = serverScoreClient;

            if (_jsonManager == null)
                _jsonManager = jsonManager;
        }

        public void LoginWithGoogleIdToken(string idToken, Action<GoogleLoginResult> completed, Action<string> failed = null)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                failed?.Invoke("Google ID token is empty.");
                return;
            }

            if (!TryBuildUrl(out string url))
            {
                failed?.Invoke("Google login server URL is not configured.");
                return;
            }

            if (_jsonManager != null)
            {
                _jsonManager.Load();
                _jsonManager.ApplyDailyGoldRewardIfAvailable();
            }

            GoogleLoginRequestDto payload = new GoogleLoginRequestDto(
                idToken,
                PlayerIdProvider.GuestPlayerId,
                _jsonManager != null ? _jsonManager.MaxScore : 0,
                _jsonManager != null ? _jsonManager.Gold : 0);

            StartCoroutine(LoginRoutine(url, payload, completed, failed));
        }

        private IEnumerator LoginRoutine(string url, GoogleLoginRequestDto payload, Action<GoogleLoginResult> completed
            ,Action<string> failed)
        {
            string json = JsonUtility.ToJson(payload);
            byte[] body = Encoding.UTF8.GetBytes(json);

            if (_logRequests)
                Debug.Log($"Google login request: {url} {json}");

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = _timeoutSeconds;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                UnityWebRequestAsyncOperation operation;

                try
                {
                    operation = request.SendWebRequest();
                }
                catch (InvalidOperationException exception)
                {
                    failed?.Invoke(exception.Message);
                    yield break;
                }

                yield return operation;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    failed?.Invoke(request.error);
                    yield break;
                }

                GoogleLoginResponseDto response = ParseResponse(request.downloadHandler.text);
                if (response == null || string.IsNullOrWhiteSpace(response.playerId))
                {
                    failed?.Invoke("Failed to parse Google login response.");
                    yield break;
                }

                completed?.Invoke(response.ToResult());
            }
        }

        private bool TryBuildUrl(out string url)
        {
            string baseUrl = _baseUrl;

            if (string.IsNullOrWhiteSpace(baseUrl) && _serverScoreClient != null)
                baseUrl = _serverScoreClient.BaseUrl;

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                url = string.Empty;
                return false;
            }

            string path = string.IsNullOrWhiteSpace(_googleLoginPath) ? "/auth/google" : _googleLoginPath;
            url = $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
            return true;
        }

        private static GoogleLoginResponseDto ParseResponse(string json)
        {
            try
            {
                return JsonUtility.FromJson<GoogleLoginResponseDto>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to parse Google login response. {exception.Message}");
                return null;
            }
        }

        public readonly struct GoogleLoginResult
        {
            public GoogleLoginResult(
                string playerId,
                string email,
                string displayName,
                bool emailVerified,
                bool hasPlayerData,
                PlayerData playerData)
            {
                PlayerId = playerId;
                Email = email;
                DisplayName = displayName;
                EmailVerified = emailVerified;
                HasPlayerData = hasPlayerData;
                PlayerData = playerData;
            }

            public string PlayerId { get; }
            public string Email { get; }
            public string DisplayName { get; }
            public bool EmailVerified { get; }
            public bool HasPlayerData { get; }
            public PlayerData PlayerData { get; }
        }

        public readonly struct PlayerData
        {
            public PlayerData(int maxScore, int gold, string lastDailyGoldRewardDate)
            {
                MaxScore = Mathf.Max(0, maxScore);
                Gold = Mathf.Max(0, gold);
                LastDailyGoldRewardDate = lastDailyGoldRewardDate ?? string.Empty;
            }

            public int MaxScore { get; }
            public int Gold { get; }
            public string LastDailyGoldRewardDate { get; }
        }

        [Serializable]
        private sealed class GoogleLoginRequestDto
        {
            public string idToken;
            public string guestPlayerId;
            public int localBestScore;
            public int localGold;

            public GoogleLoginRequestDto(string idToken, string guestPlayerId, int localBestScore, int localGold)
            {
                this.idToken = idToken;
                this.guestPlayerId = guestPlayerId;
                this.localBestScore = Mathf.Max(0, localBestScore);
                this.localGold = Mathf.Max(0, localGold);
            }
        }

        [Serializable]
        private sealed class GoogleLoginResponseDto
        {
            public string playerId;
            public string email;
            public string displayName;
            public bool emailVerified;
            public PlayerScoreDto playerData;

            public GoogleLoginResult ToResult()
            {
                bool hasPlayerData = playerData != null;
                PlayerData data = hasPlayerData
                    ? new PlayerData(playerData.GetScore(), playerData.GetGold(), playerData.lastDailyGoldRewardDate)
                    : new PlayerData(0, 0, string.Empty);

                return new GoogleLoginResult(playerId, email, displayName, emailVerified, hasPlayerData, data);
            }
        }

        [Serializable]
        private sealed class PlayerScoreDto
        {
            public string playerId;
            public int score;
            public int maxScore;
            public int bestScore;
            public int gold;
            public string lastDailyGoldRewardDate;

            public int GetScore()
            {
                return Mathf.Max(0, score, maxScore, bestScore);
            }

            public int GetGold()
            {
                return Mathf.Max(0, gold);
            }
        }
    }
}
