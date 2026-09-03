using System.Collections.Generic;
using UnityEngine;

namespace _Code.Stage
{
    public enum StageGoalType
    {
        Score,
        Cheese,
        CatRemoval
    }

    public enum StageTargetCatType
    {
        None,
        Black,
        White,
        Sphynx,
        Orange
    }

    public readonly struct StageDefinition
    {
        public StageDefinition(
            int number,
            StageGoalType goalType,
            int targetScore,
            Vector2Int[] startingCells,
            Vector2Int[] cheeseCells,
            int maxPlacementCount,
            StageTargetCatType targetCatType = StageTargetCatType.None,
            int targetCatRemovalCount = 0)
        {
            Number = number;
            GoalType = goalType;
            TargetScore = targetScore;
            StartingCells = startingCells ?? new Vector2Int[0];
            CheeseCells = cheeseCells ?? new Vector2Int[0];
            MaxPlacementCount = Mathf.Max(0, maxPlacementCount);
            TargetCatType = targetCatType;
            TargetCatRemovalCount = Mathf.Max(0, targetCatRemovalCount);
        }

        public int Number { get; }
        public StageGoalType GoalType { get; }
        public int TargetScore { get; }
        public IReadOnlyList<Vector2Int> StartingCells { get; }
        public IReadOnlyList<Vector2Int> CheeseCells { get; }
        public int MaxPlacementCount { get; }
        public StageTargetCatType TargetCatType { get; }
        public int TargetCatRemovalCount { get; }
        public int TargetCheeseCount => CheeseCells.Count;
    }
}
