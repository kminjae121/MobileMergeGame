using System.Collections.Generic;
using UnityEngine;

namespace _Code.Block
{
    public static class BlockShapeCatalog
    {
        private static readonly BlockShape[] _shapes =
        {
            new BlockShape("Single", new Vector2Int(0, 0)),
            new BlockShape("Two Horizontal", new Vector2Int(0, 0), new Vector2Int(1, 0)),
            new BlockShape("Two Vertical", new Vector2Int(0, 0), new Vector2Int(0, 1)),
            new BlockShape("Three Vertical", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)),
            new BlockShape("Four Vertical", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3)),
            new BlockShape("Five Vertical", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3), new Vector2Int(0, 4)),
            new BlockShape("Square 2", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)),
            new BlockShape("Square 3", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)),
            new BlockShape("Small L", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 0)),
            new BlockShape("L Right", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 0)),
            new BlockShape("L Left", new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(0, 0)),
            new BlockShape("T", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1)),
            new BlockShape("S", new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)),
            new BlockShape("Z", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1)),
            new BlockShape("Plus", new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2))
        };

        public static IReadOnlyList<BlockShape> Shapes => _shapes;

        public static BlockShape GetRandomShape()
        {
            return _shapes[Random.Range(0, _shapes.Length)];
        }
    }
}
