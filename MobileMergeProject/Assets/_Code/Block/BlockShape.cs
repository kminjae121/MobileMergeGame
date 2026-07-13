using System.Collections.Generic;
using UnityEngine;

namespace _Code.Block
{
    public sealed class BlockShape
    {
        private readonly Vector2Int[] _cells;

        public BlockShape(string name, params Vector2Int[] cells)
        {
            Name = name;
            _cells = cells;
            VisualCenter = CalculateVisualCenter(cells);
        }

        public string Name { get; }
        public IReadOnlyList<Vector2Int> Cells => _cells;
        public Vector2 VisualCenter { get; }

        private static Vector2 CalculateVisualCenter(IReadOnlyList<Vector2Int> cells)
        {
            if (cells.Count == 0)
                return Vector2.zero;

            int minX = cells[0].x;
            int maxX = cells[0].x;
            int minY = cells[0].y;
            int maxY = cells[0].y;

            for (int i = 1; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                minX = Mathf.Min(minX, cell.x);
                maxX = Mathf.Max(maxX, cell.x);
                minY = Mathf.Min(minY, cell.y);
                maxY = Mathf.Max(maxY, cell.y);
            }

            return new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        }
    }
}
