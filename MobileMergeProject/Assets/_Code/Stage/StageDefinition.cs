using System.Collections.Generic;
using UnityEngine;

namespace _Code.Stage
{
    public readonly struct StageDefinition
    {
        public StageDefinition(int number, int targetScore, Vector2Int[] startingCells)
        {
            Number = number;
            TargetScore = targetScore;
            StartingCells = startingCells;
        }

        public int Number { get; }
        public int TargetScore { get; }
        public IReadOnlyList<Vector2Int> StartingCells { get; }
    }
}
