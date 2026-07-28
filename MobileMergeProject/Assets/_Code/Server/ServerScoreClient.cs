using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace _Code.Server
{
    public sealed class ServerScoreClient : MonoBehaviour
    {
        [SerializeField] private string _baseUrl;
        [SerializeField] private string _fetchScorePath = "/players/{playerId}/score";
        [SerializeField] private string _submitScorePath = "/players/{playerId}/score";
        [SerializeField] private string _submitMethod = "POST";
        [SerializeField, Min(1)] private int _timeoutSeconds = 10;
        [SerializeField] private bool _logRequests;

        public string PlayerId => PlayerIdProvider.PlayerId;
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_baseUrl);

        public void FetchScore(Action<int> completed)
        {
            if (!IsConfigured)
                return;

            StartCoroutine(FetchScoreRoutine(completed));
        }

        public void SubmitScore(int score)
        {
            if (!IsConfigured)
                return;

            StartCoroutine(SubmitScoreRoutine(Mathf.Max(0, score)));
        }

        private System.Collections.IEnumerator FetchScoreRoutine(Action<int> completed)
        {
            string url = BuildUrl(_fetchScorePath);
            if (_logRequests)
                Debug.Log($"Fetch score from server: {url}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = _timeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Failed to fetch score. {request.error}");
                    yield break;
                }

                PlayerScoreDto response = JsonUtility.FromJson<PlayerScoreDto>(request.downloadHandler.text);
                if (response == null)
                {
                    Debug.LogWarning("Failed to parse server score response.");
                    yield break;
                }

                completed?.Invoke(response.GetScore());
            }
        }

        private System.Collections.IEnumerator SubmitScoreRoutine(int score)
        {
            string url = BuildUrl(_submitScorePath);
            PlayerScoreDto payload = new PlayerScoreDto(PlayerId, score);
            string json = JsonUtility.ToJson(payload);
            byte[] body = Encoding.UTF8.GetBytes(json);

            if (_logRequests)
                Debug.Log($"Submit score to server: {url} {json}");

            using (UnityWebRequest request = new UnityWebRequest(url, string.IsNullOrWhiteSpace(_submitMethod) ? "POST" : _submitMethod))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = _timeoutSeconds;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.LogWarning($"Failed to submit score. {request.error}");
            }
        }

        private string BuildUrl(string path)
        {
            string playerId = UnityWebRequest.EscapeURL(PlayerId);
            string expandedPath = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace("{playerId}", playerId);
            return $"{_baseUrl.TrimEnd('/')}/{expandedPath.TrimStart('/')}";
        }

        [Serializable]
        private sealed class PlayerScoreDto
        {
            public string playerId;
            public int score;
            public int maxScore;
            public int bestScore;

            public PlayerScoreDto(string playerId, int score)
            {
                this.playerId = playerId;
                this.score = score;
                maxScore = score;
                bestScore = score;
            }

            public int GetScore()
            {
                return Mathf.Max(0, score, maxScore, bestScore);
            }
        }
    }
}
