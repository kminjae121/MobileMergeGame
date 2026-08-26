using System.Collections.Generic;
using UnityEngine;

namespace _Code.Stage
{
    public enum StageGoalType
    {
        Score,
        Cheese
    }

    public readonly struct StageDefinition
    {
        public StageDefinition(
            int number,
            StageGoalType goalType,
            int targetScore,
            Vector2Int[] startingCells,
            Vector2Int[] cheeseCells,
            int maxPlacementCount)
        {
            Number = number;
            GoalType = goalType;
            TargetScore = targetScore;
            StartingCells = startingCells ?? new Vector2Int[0];
            CheeseCells = cheeseCells ?? new Vector2Int[0];
            MaxPlacementCount = Mathf.Max(0, maxPlacementCount);
        }

        public int Number { get; }
        public StageGoalType GoalType { get; }
        public int TargetScore { get; }
        public IReadOnlyList<Vector2Int> StartingCells { get; }
        public IReadOnlyList<Vector2Int> CheeseCells { get; }
        public int MaxPlacementCount { get; }
        public int TargetCheeseCount => CheeseCells.Count;
    }
}
