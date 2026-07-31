using UnityEngine;

namespace _Code.Stage
{
    public static class StageCatalog
    {
        private static readonly StageDefinition[] _stages =
        {
            new StageDefinition(1, 300, Cells(2, 2, 3, 2, 2, 3, 3, 3)),
            new StageDefinition(2, 450, Cells(1, 1, 4, 1, 2, 2, 3, 2, 1, 4, 4, 4)),
            new StageDefinition(3, 600, Cells(0, 0, 1, 0, 4, 0, 5, 0, 2, 2, 3, 2, 2, 3, 3, 3)),
            new StageDefinition(4, 800, Cells(0, 0, 1, 0, 0, 1, 5, 0, 4, 0, 5, 1, 2, 4, 3, 4)),
            new StageDefinition(5, 950, Cells(0, 1, 2, 1, 4, 1, 1, 2, 3, 2, 5, 2, 0, 4, 2, 4, 4, 4)),
            new StageDefinition(6, 1100, Cells(0, 0, 0, 1, 0, 3, 0, 4, 5, 1, 5, 2, 5, 4, 1, 5, 2, 5, 3, 5)),
            new StageDefinition(7, 1250, Cells(1, 0, 2, 0, 3, 0, 4, 0, 1, 5, 2, 5, 3, 5, 4, 5, 0, 2, 5, 3)),
            new StageDefinition(8, 1450, Cells(0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 5, 0, 4, 1, 1, 4, 0, 5)),
            new StageDefinition(9, 1650, Cells(0, 0, 1, 0, 3, 0, 4, 0, 0, 2, 1, 2, 4, 2, 5, 2, 1, 4, 2, 4, 3, 4, 5, 4)),
            new StageDefinition(10, 1900, Cells(0, 0, 1, 0, 4, 0, 5, 0, 0, 1, 5, 1, 2, 2, 3, 2, 2, 3, 3, 3, 0, 4, 5, 4, 1, 5, 4, 5))
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

        private static Vector2Int[] Cells(params int[] coordinates)
        {
            Vector2Int[] cells = new Vector2Int[coordinates.Length / 2];

            for (int i = 0; i < cells.Length; i++)
                cells[i] = new Vector2Int(coordinates[i * 2], coordinates[i * 2 + 1]);

            return cells;
        }
    }
}
