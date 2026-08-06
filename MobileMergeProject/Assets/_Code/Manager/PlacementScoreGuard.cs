using System;
using System.Collections.Generic;
using _Code.Block;
using _Code.Field;
using _Code.Server;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Code.Manager
{
    public sealed class PlacementScoreGuard : MonoBehaviour
    {
        [SerializeField] private JsonManager jsonManager;
        [SerializeField] private ServerScoreClient serverScoreClient;
        [SerializeField] private string resetSceneName = "MainScene";

        public void Configure(JsonManager jsonManager, ServerScoreClient serverScoreClient, string resetSceneName)
        {
            if (this.jsonManager == null)
                this.jsonManager = jsonManager;

            if (this.serverScoreClient == null)
                this.serverScoreClient = serverScoreClient;

            if (!string.IsNullOrWhiteSpace(resetSceneName))
                this.resetSceneName = resetSceneName;
        }

        public PlacementScoreSnapshot Capture(BlockField blockField, BlockPiece piece, int scoreBefore, int goldBefore)
        {
            return new PlacementScoreSnapshot(
                scoreBefore,
                goldBefore,
                blockField.Width,
                blockField.Height,
                CaptureOccupiedCells(blockField),
                CaptureBlockCells(piece));
        }

        public void Validate(
            PlacementScoreSnapshot snapshot,
            int scoreAfter,
            int bestScore,
            int goldAfter,
            Action<ServerScoreClient.PlayerData> applyServerData)
        {
            if (serverScoreClient == null)
                return;

            serverScoreClient.ValidatePlacementScore(
                snapshot.ScoreBefore,
                scoreAfter,
                bestScore,
                snapshot.GoldBefore,
                goldAfter,
                snapshot.BoardWidth,
                snapshot.BoardHeight,
                snapshot.OccupiedCells,
                snapshot.BlockCells,
                result => ApplyValidationResult(result, applyServerData));
        }

        private void ApplyValidationResult(
            ServerScoreClient.PlacementValidationResult result,
            Action<ServerScoreClient.PlayerData> applyServerData)
        {
            if (result.CheatDetected)
            {
                HandleCheatDetected();
                return;
            }

            if (result.HasPlayerData)
                applyServerData?.Invoke(result.PlayerData);
        }

        private void HandleCheatDetected()
        {
            Debug.Log("\uD575 \uAC10\uC9C0: \uACC4\uC815 \uB370\uC774\uD130\uB97C \uCD08\uAE30\uD654\uD569\uB2C8\uB2E4.");

            if (jsonManager != null)
                jsonManager.ResetSaveData();

            string sceneName = string.IsNullOrWhiteSpace(resetSceneName)
                ? SceneManager.GetActiveScene().name
                : resetSceneName;

            SceneManager.LoadScene(sceneName);
        }

        private static Vector2Int[] CaptureOccupiedCells(BlockField blockField)
        {
            List<Vector2Int> results = new List<Vector2Int>(blockField.Fields.Count);

            foreach (_Code.Field.Field field in blockField.Fields)
            {
                if (!field.IsEmpty)
                    results.Add(field.Point);
            }

            return results.ToArray();
        }

        private static Vector2Int[] CaptureBlockCells(BlockPiece piece)
        {
            Vector2Int[] results = new Vector2Int[piece.Cells.Count];

            for (int i = 0; i < piece.Cells.Count; i++)
                results[i] = piece.Cells[i];

            return results;
        }

        public readonly struct PlacementScoreSnapshot
        {
            public PlacementScoreSnapshot(
                int scoreBefore,
                int goldBefore,
                int boardWidth,
                int boardHeight,
                IReadOnlyList<Vector2Int> occupiedCells,
                IReadOnlyList<Vector2Int> blockCells)
            {
                ScoreBefore = scoreBefore;
                GoldBefore = goldBefore;
                BoardWidth = boardWidth;
                BoardHeight = boardHeight;
                OccupiedCells = occupiedCells;
                BlockCells = blockCells;
            }

            public int ScoreBefore { get; }
            public int GoldBefore { get; }
            public int BoardWidth { get; }
            public int BoardHeight { get; }
            public IReadOnlyList<Vector2Int> OccupiedCells { get; }
            public IReadOnlyList<Vector2Int> BlockCells { get; }
        }
    }
}
