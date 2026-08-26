using System;
using System.Collections.Generic;
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
        [SerializeField] private string _validatePlacementPath = "/players/{playerId}/score/validate-placement";
        [SerializeField] private string _submitMethod = "POST";
        [SerializeField, Min(1)] private int _timeoutSeconds = 10;
        [SerializeField] private bool _logRequests;
        [SerializeField] private bool _logConnectionSuccess = true;
        [SerializeField] private bool _logConnectionFailure = true;
        [SerializeField] private bool _logConnectionFailures;

        private bool _hasLoggedConnectionSuccess;
        private bool _hasLoggedConnectionFailure;

        public string PlayerId => PlayerIdProvider.PlayerId;
        public string BaseUrl => _baseUrl;
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_baseUrl);

        public void FetchScore(Action<int> completed)
        {
            FetchScore(completed, null);
        }

        public void FetchScore(Action<int> completed, Action failed)
        {
            FetchPlayerData(data => completed?.Invoke(data.MaxScore), failed);
        }

        public void FetchPlayerData(Action<PlayerData> completed)
        {
            FetchPlayerData(completed, null);
        }

        public void FetchPlayerData(Action<PlayerData> completed, Action failed)
        {
            if (!IsConfigured)
            {
                LogConnectionFailure("Server score client is not configured.");
                failed?.Invoke();
                return;
            }

            StartCoroutine(FetchPlayerDataRoutine(completed, failed));
        }

        public void SubmitScore(int score)
        {
            SubmitPlayerData(score, 0);
        }

        public void SubmitPlayerData(int maxScore, int gold)
        {
            if (!IsConfigured)
            {
                LogConnectionFailure("Server score client is not configured.");
                return;
            }

            StartCoroutine(SubmitPlayerDataRoutine(Mathf.Max(0, maxScore), Mathf.Max(0, gold)));
        }

        public void ValidatePlacementScore(
            int scoreBefore,
            int scoreAfter,
            int bestScore,
            int goldBefore,
            int goldAfter,
            int boardWidth,
            int boardHeight,
            IReadOnlyList<Vector2Int> occupiedCells,
            IReadOnlyList<Vector2Int> blockCells,
            Action<PlacementValidationResult> completed,
            Action failed = null)
        {
            if (!IsConfigured)
            {
                LogConnectionFailure("Server score client is not configured.");
                failed?.Invoke();
                return;
            }

            StartCoroutine(ValidatePlacementScoreRoutine(
                Mathf.Max(0, scoreBefore),
                Mathf.Max(0, scoreAfter),
                Mathf.Max(0, bestScore),
                Mathf.Max(0, goldBefore),
                Mathf.Max(0, goldAfter),
                boardWidth,
                boardHeight,
                occupiedCells,
                blockCells,
                completed,
                failed));
        }

        private System.Collections.IEnumerator FetchPlayerDataRoutine(Action<PlayerData> completed, Action failed)
        {
            if (!TryBuildUrl(_fetchScorePath, out string url))
            {
                failed?.Invoke();
                yield break;
            }

            if (_logRequests)
                Debug.Log($"Fetch score from server: {url}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = _timeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");

                UnityWebRequestAsyncOperation operation;

                try
                {
                    operation = request.SendWebRequest();
                }
                catch (InvalidOperationException exception)
                {
                    LogConnectionFailure($"Failed to start fetch request. {exception.Message}");
                    failed?.Invoke();
                    yield break;
                }

                yield return operation;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogConnectionFailure($"Failed to fetch score. {request.error}");
                    failed?.Invoke();
                    yield break;
                }

                PlayerScoreDto response = ParseScoreResponse(request.downloadHandler.text);
                if (response == null)
                {
                    LogConnectionFailure("Failed to parse server score response.");
                    failed?.Invoke();
                    yield break;
                }

                LogConnectionSuccess();
                completed?.Invoke(response.ToPlayerData());
            }
        }

        private System.Collections.IEnumerator SubmitPlayerDataRoutine(int maxScore, int gold)
        {
            if (!TryBuildUrl(_submitScorePath, out string url))
                yield break;

            PlayerScoreDto payload = new PlayerScoreDto(PlayerId, maxScore, gold);
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

                UnityWebRequestAsyncOperation operation;

                try
                {
                    operation = request.SendWebRequest();
                }
                catch (InvalidOperationException exception)
                {
                    LogConnectionFailure($"Failed to start submit request. {exception.Message}");
                    yield break;
                }

                yield return operation;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogConnectionFailure($"Failed to submit score. {request.error}");
                    yield break;
                }

                LogConnectionSuccess();
            }
        }

        private System.Collections.IEnumerator ValidatePlacementScoreRoutine(
            int scoreBefore,
            int scoreAfter,
            int bestScore,
            int goldBefore,
            int goldAfter,
            int boardWidth,
            int boardHeight,
            IReadOnlyList<Vector2Int> occupiedCells,
            IReadOnlyList<Vector2Int> blockCells,
            Action<PlacementValidationResult> completed,
            Action failed)
        {
            if (!TryBuildUrl(_validatePlacementPath, out string url))
            {
                failed?.Invoke();
                yield break;
            }

            PlacementValidationRequestDto payload = new PlacementValidationRequestDto(
                PlayerId,
                scoreBefore,
                scoreAfter,
                bestScore,
                goldBefore,
                goldAfter,
                boardWidth,
                boardHeight,
                occupiedCells,
                blockCells);

            string json = JsonUtility.ToJson(payload);
            byte[] body = Encoding.UTF8.GetBytes(json);

            if (_logRequests)
                Debug.Log($"Validate placement score on server: {url} {json}");

            using (UnityWebRequest request = new UnityWebRequest(url, string.IsNullOrWhiteSpace(_submitMethod) ? "POST" : _submitMethod))
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
                    LogConnectionFailure($"Failed to start placement validation request. {exception.Message}");
                    failed?.Invoke();
                    yield break;
                }

                yield return operation;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogConnectionFailure($"Failed to validate placement score. {request.error}");
                    failed?.Invoke();
                    yield break;
                }

                PlacementValidationResponseDto response = ParsePlacementValidationResponse(request.downloadHandler.text);
                if (response == null)
                {
                    LogConnectionFailure("Failed to parse placement validation response.");
                    failed?.Invoke();
                    yield break;
                }

                LogConnectionSuccess();
                completed?.Invoke(response.ToResult());
            }
        }

        private bool TryBuildUrl(string path, out string url)
        {
            url = string.Empty;

            try
            {
                string playerId = UnityWebRequest.EscapeURL(PlayerId);
                string expandedPath = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace("{playerId}", playerId);
                url = $"{_baseUrl.TrimEnd('/')}/{expandedPath.TrimStart('/')}";
                return true;
            }
            catch (Exception exception)
            {
                LogConnectionFailure($"Failed to build server URL. {exception.Message}");
                return false;
            }
        }

        private PlayerScoreDto ParseScoreResponse(string json)
        {
            try
            {
                return JsonUtility.FromJson<PlayerScoreDto>(json);
            }
            catch (Exception exception)
            {
                LogConnectionFailure($"Failed to parse server score response. {exception.Message}");
                return null;
            }
        }

        private PlacementValidationResponseDto ParsePlacementValidationResponse(string json)
        {
            try
            {
                return JsonUtility.FromJson<PlacementValidationResponseDto>(json);
            }
            catch (Exception exception)
            {
                LogConnectionFailure($"Failed to parse placement validation response. {exception.Message}");
                return null;
            }
        }

        private void LogConnectionFailure(string message)
        {
            if (_logConnectionFailure && !_hasLoggedConnectionFailure)
            {
                _hasLoggedConnectionFailure = true;
                Debug.Log("\uC11C\uBC84 \uC5F0\uACB0\uC548\uB428 Json으로 저장시작.");
            }

            if (_logConnectionFailures)
                Debug.Log(message);
        }

        private void LogConnectionSuccess()
        {
            if (!_logConnectionSuccess || _hasLoggedConnectionSuccess)
                return;

            _hasLoggedConnectionSuccess = true;
            Debug.Log("\uC11C\uBC84 \uC5F0\uACB0\uB428");
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

        public readonly struct PlacementValidationResult
        {
            public PlacementValidationResult(
                bool cheatDetected,
                bool accountDeleted,
                int maxAllowedScoreGain,
                int actualScoreGain,
                int maxAllowedGoldGain,
                int actualGoldGain,
                bool hasPlayerData,
                PlayerData playerData)
            {
                CheatDetected = cheatDetected;
                AccountDeleted = accountDeleted;
                MaxAllowedScoreGain = maxAllowedScoreGain;
                ActualScoreGain = actualScoreGain;
                MaxAllowedGoldGain = maxAllowedGoldGain;
                ActualGoldGain = actualGoldGain;
                HasPlayerData = hasPlayerData;
                PlayerData = playerData;
            }

            public bool CheatDetected { get; }
            public bool AccountDeleted { get; }
            public int MaxAllowedScoreGain { get; }
            public int ActualScoreGain { get; }
            public int MaxAllowedGoldGain { get; }
            public int ActualGoldGain { get; }
            public bool HasPlayerData { get; }
            public PlayerData PlayerData { get; }
        }

        [Serializable]
        private sealed class PlacementValidationRequestDto
        {
            public string playerId;
            public int scoreBefore;
            public int scoreAfter;
            public int bestScore;
            public int goldBefore;
            public int goldAfter;
            public int boardWidth;
            public int boardHeight;
            public GridPointDto[] occupiedCells;
            public GridPointDto[] blockCells;

            public PlacementValidationRequestDto(
                string playerId,
                int scoreBefore,
                int scoreAfter,
                int bestScore,
                int goldBefore,
                int goldAfter,
                int boardWidth,
                int boardHeight,
                IReadOnlyList<Vector2Int> occupiedCells,
                IReadOnlyList<Vector2Int> blockCells)
            {
                this.playerId = playerId;
                this.scoreBefore = scoreBefore;
                this.scoreAfter = scoreAfter;
                this.bestScore = bestScore;
                this.goldBefore = goldBefore;
                this.goldAfter = goldAfter;
                this.boardWidth = boardWidth;
                this.boardHeight = boardHeight;
                this.occupiedCells = ToGridPoints(occupiedCells);
                this.blockCells = ToGridPoints(blockCells);
            }
        }

        [Serializable]
        private sealed class PlacementValidationResponseDto
        {
            public bool cheatDetected;
            public bool accountDeleted;
            public int maxAllowedScoreGain;
            public int actualScoreGain;
            public int maxAllowedGoldGain;
            public int actualGoldGain;
            public PlayerScoreDto playerData;

            public PlacementValidationResult ToResult()
            {
                bool hasPlayerData = playerData != null;
                PlayerData data = hasPlayerData ? playerData.ToPlayerData() : new PlayerData(0, 0, string.Empty);

                return new PlacementValidationResult(
                    cheatDetected,
                    accountDeleted,
                    maxAllowedScoreGain,
                    actualScoreGain,
                    maxAllowedGoldGain,
                    actualGoldGain,
                    hasPlayerData,
                    data);
            }
        }

        [Serializable]
        private sealed class GridPointDto
        {
            public int x;
            public int y;

            public GridPointDto(Vector2Int point)
            {
                x = point.x;
                y = point.y;
            }
        }

        private static GridPointDto[] ToGridPoints(IReadOnlyList<Vector2Int> points)
        {
            if (points == null || points.Count == 0)
                return Array.Empty<GridPointDto>();

            GridPointDto[] result = new GridPointDto[points.Count];

            for (int i = 0; i < points.Count; i++)
                result[i] = new GridPointDto(points[i]);

            return result;
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

            public PlayerScoreDto(string playerId, int maxScore, int gold)
            {
                this.playerId = playerId;
                this.score = maxScore;
                this.maxScore = maxScore;
                bestScore = maxScore;
                this.gold = gold;
                lastDailyGoldRewardDate = string.Empty;
            }

            public int GetScore()
            {
                return Mathf.Max(0, score, maxScore, bestScore);
            }

            public int GetGold()
            {
                return Mathf.Max(0, gold);
            }

            public PlayerData ToPlayerData()
            {
                return new PlayerData(GetScore(), GetGold(), lastDailyGoldRewardDate);
            }
        }
    }
}
