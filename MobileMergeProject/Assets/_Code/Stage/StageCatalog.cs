using UnityEngine;

namespace _Code.Stage
{
    public static class StageCatalog
    {
        private const string StageSceneNameFormat = "Stage{0}Scene";

        private static readonly StageDefinition[] _stages =
        {
            ScoreStage(1, 1000),
            CheeseStage(2, 13, Cells(2, 2, 3, 3)),
            CatRemovalStage(3, StageTargetCatType.Black, 6),
            CheeseStage(4, 17, Cells(1, 1, 4, 1, 1, 4, 4, 4)),
            CatRemovalStage(5, StageTargetCatType.White, 10),
            CheeseStage(6, 21, Cells(1, 0, 4, 0, 2, 2, 3, 2, 1, 5, 4, 5)),
            ScoreStage(7, 7000),
            ScoreStage(8, 8000),
            CatRemovalStage(9, StageTargetCatType.Orange, 15),
            CheeseStage(10, 25, Cells(0, 0, 2, 0, 5, 0, 1, 2, 4, 2, 0, 5, 3, 5, 5, 5))
        };

        public static int MaxStage => _stages.Length;

        public static bool TryGetStage(int number, out StageDefinition stage)
        {
            if (number < 1 || number > _stages.Length)
            {
                stage = default;
                return false;
            }

            stage = _stages[number - 1];
            return true;
        }

        public static string GetStageSceneName(int number)
        {
            return string.Format(StageSceneNameFormat, Mathf.Clamp(number, 1, MaxStage));
        }

        public static bool TryGetStageNumberFromSceneName(string sceneName, out int stageNumber)
        {
            stageNumber = 0;

            if (string.IsNullOrEmpty(sceneName) ||
                !sceneName.StartsWith("Stage") ||
                !sceneName.EndsWith("Scene"))
                return false;

            string numberText = sceneName.Substring(5, sceneName.Length - 10);
            return int.TryParse(numberText, out stageNumber) && stageNumber >= 1 && stageNumber <= MaxStage;
        }

        private static StageDefinition ScoreStage(int number, int targetScore)
        {
            return new StageDefinition(number, StageGoalType.Score, targetScore, Cells(), Cells(), 0);
        }

        private static StageDefinition CheeseStage(int number, int maxPlacementCount, Vector2Int[] cheeseCells)
        {
            return new StageDefinition(number, StageGoalType.Cheese, 0, Cells(), cheeseCells, maxPlacementCount);
        }

        private static StageDefinition CatRemovalStage(int number, StageTargetCatType targetCatType, int targetCount)
        {
            return new StageDefinition(number, StageGoalType.CatRemoval, 0, Cells(), Cells(), 0, targetCatType, targetCount);
        }

        private static Vector2Int[] Cells(params int[] coordinates)
        {
            Vector2Int[] cells = new Vector2Int[coordinates.Length / 2];

            for (int i = 0; i < cells.Length; i++)
                cells[i] = new Vector2Int(coordinates[i * 2], coordinates[i * 2 + 1]);

            return cells;
        }
    }
}
