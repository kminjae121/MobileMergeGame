using System.Collections.Generic;
using _Code.Block;
using UnityEngine;

namespace _Code.Field
{
    public class BlockField : MonoBehaviour
    {
        [SerializeField, Min(1)] private int _width = 6;
        [SerializeField, Min(1)] private int _height = 6;
        [SerializeField, Min(0.1f)] private float _cellSize = 0.72f;
        [SerializeField, Min(0.1f)] private float _snapDistance = 0.8f;

        private readonly List<Field> _fields = new List<Field>();
        private int _nextGroupId = 1;

        public Field[,] Installables { get; private set; }
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public IReadOnlyList<Field> Fields => _fields;

        private void Awake()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            Installables = new Field[_width, _height];
            _fields.Clear();

            Field[] fields = GetComponentsInChildren<Field>();

            foreach (var field in fields)
            {
                Vector2Int point = field.Point;

                if (point.x < 0 || point.x >= _width || point.y < 0 || point.y >= _height)
                    continue;

                Installables[point.x, point.y] = field;
                _fields.Add(field);
            }

            BlockCellVisualPool.Prewarm(_width * _height, GetObjectRendererTemplate());
        }

        public bool TryGetField(Vector2Int point, out Field field)
        {
            field = null;

            if (!IsInside(point))
                return false;

            field = Installables[point.x, point.y];
            return field != null;
        }

        public bool TryGetAnchorFor(BlockPiece piece, out Vector2Int point)
        {
            return TryGetClosestPoint(piece.GetReleaseAnchorWorldPosition(), out point);
        }

        public bool TryGetClosestPoint(Vector3 worldPosition, out Vector2Int point)
        {
            point = default;
            float closestDistance = _snapDistance * _snapDistance;
            Field closestField = null;

            foreach (Field field in _fields)
            {
                float distance = ((Vector2)field.transform.position - (Vector2)worldPosition).sqrMagnitude;

                if (distance > closestDistance)
                    continue;

                closestDistance = distance;
                closestField = field;
            }

            if (closestField == null)
                return false;

            point = closestField.Point;
            return true;
        }

        public Vector3 GetWorldPosition(Vector2Int point)
        {
            return TryGetField(point, out Field field) ? field.transform.position : transform.position;
        }

        public bool CanInstall(IInstallable installable, Vector2Int anchor)
        {
            return CanInstall(installable.Cells, anchor);
        }

        public bool CanInstall(IReadOnlyList<Vector2Int> cells, Vector2Int anchor)
        {
            foreach (Vector2Int cell in cells)
            {
                Vector2Int point = anchor + cell;

                if (!TryGetField(point, out Field field) || !field.IsEmpty)
                    return false;
            }

            return true;
        }

        public void Install(Blcok block, Vector2Int anchor)
        {
            int groupId = _nextGroupId++;

            foreach (Vector2Int cell in block.Cells)
            {
                Vector2Int point = anchor + cell;

                if (TryGetField(point, out Field field))
                    field.SetObject(block.gameObject, Color.white, block.BlockSprite, groupId);
            }
        }

        public bool Compact(Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
                return false;

            direction = NormalizeDirection(direction);

            bool hasMovedAny = false;
            bool hasMovedThisPass = true;
            int guard = 0;
            int maxPassCount = _width * _height;

            while (hasMovedThisPass && guard < maxPassCount)
            {
                hasMovedThisPass = false;
                guard++;

                List<BlockGroup> groups = GetBlockGroups();
                SortGroups(groups, direction);

                foreach (BlockGroup group in groups)
                {
                    if (!CanMoveGroup(group, direction))
                        continue;

                    MoveGroup(group, direction);
                    hasMovedThisPass = true;
                    hasMovedAny = true;
                }
            }

            return hasMovedAny;
        }

        public bool HasAnyCompactMove()
        {
            return CanCompact(Vector2Int.up) ||
                   CanCompact(Vector2Int.down) ||
                   CanCompact(Vector2Int.left) ||
                   CanCompact(Vector2Int.right);
        }

        public bool CanCompact(Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
                return false;

            direction = NormalizeDirection(direction);
            List<BlockGroup> groups = GetBlockGroups();

            foreach (BlockGroup group in groups)
            {
                if (CanMoveGroup(group, direction))
                    return true;
            }

            return false;
        }

        public int ClearCompletedLines()
        {
            return ClearCompletedLines(null);
        }

        public int ClearCompletedLines(ICollection<Vector3> clearedWorldPositions)
        {
            return ClearCompletedLines(clearedWorldPositions, null);
        }

        public int ClearCompletedLines(ICollection<Vector3> clearedWorldPositions, ICollection<Vector2Int> clearedPoints)
        {
            HashSet<Field> fieldsToClear = new HashSet<Field>();
            int clearedLineCount = 0;
            clearedWorldPositions?.Clear();
            clearedPoints?.Clear();

            for (int y = 0; y < _height; y++)
            {
                if (!IsRowFull(y))
                    continue;

                clearedLineCount++;
                for (int x = 0; x < _width; x++)
                    fieldsToClear.Add(Installables[x, y]);
            }

            for (int x = 0; x < _width; x++)
            {
                if (!IsColumnFull(x))
                    continue;

                clearedLineCount++;
                for (int y = 0; y < _height; y++)
                    fieldsToClear.Add(Installables[x, y]);
            }

            foreach (Field field in fieldsToClear)
            {
                clearedWorldPositions?.Add(field.transform.position);
                clearedPoints?.Add(field.Point);
                field.ClearObject();
            }

            return clearedLineCount;
        }

        public void ClearAll()
        {
            foreach (Field field in _fields)
                field.ClearObject();

            _nextGroupId = 1;
        }

        public bool HasAnyPlacement(IInstallable installable)
        {
            return HasAnyPlacement(installable.Cells);
        }

        public bool HasAnyPlacement(BlockShape shape)
        {
            return HasAnyPlacement(shape.Cells);
        }

        public bool HasAnyPlacement(IReadOnlyList<Vector2Int> cells)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (CanInstall(cells, new Vector2Int(x, y)))
                        return true;
                }
            }

            return false;
        }

        private bool IsInside(Vector2Int point)
        {
            return point.x >= 0 && point.x < _width && point.y >= 0 && point.y < _height;
        }

        private SpriteRenderer GetObjectRendererTemplate()
        {
            foreach (Field field in _fields)
            {
                if (field != null && field.ObjectRendererTemplate != null)
                    return field.ObjectRendererTemplate;
            }

            return null;
        }

        private bool IsRowFull(int y)
        {
            for (int x = 0; x < _width; x++)
            {
                Field field = Installables[x, y];

                if (field == null || field.IsEmpty)
                    return false;
            }

            return true;
        }

        private bool IsColumnFull(int x)
        {
            for (int y = 0; y < _height; y++)
            {
                Field field = Installables[x, y];

                if (field == null || field.IsEmpty)
                    return false;
            }

            return true;
        }

        private List<BlockGroup> GetBlockGroups()
        {
            List<BlockGroup> groups = new List<BlockGroup>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            foreach (Field field in _fields)
            {
                if (field.IsEmpty || field.IsStageCheese || visited.Contains(field.Point))
                    continue;

                groups.Add(BuildConnectedGroup(field, visited));
            }

            return groups;
        }

        private BlockGroup BuildConnectedGroup(Field startField, HashSet<Vector2Int> visited)
        {
            int groupId = startField.CurrentGroupId;
            BlockGroup group = new BlockGroup(groupId);
            Queue<Field> queue = new Queue<Field>();

            visited.Add(startField.Point);
            queue.Enqueue(startField);

            while (queue.Count > 0)
            {
                Field field = queue.Dequeue();
                group.Add(field);

                TryEnqueueNeighbor(field.Point + Vector2Int.up, groupId, visited, queue);
                TryEnqueueNeighbor(field.Point + Vector2Int.down, groupId, visited, queue);
                TryEnqueueNeighbor(field.Point + Vector2Int.left, groupId, visited, queue);
                TryEnqueueNeighbor(field.Point + Vector2Int.right, groupId, visited, queue);
            }

            return group;
        }

        private void TryEnqueueNeighbor(Vector2Int point, int groupId, HashSet<Vector2Int> visited, Queue<Field> queue)
        {
            if (visited.Contains(point))
                return;

            if (!TryGetField(point, out Field field) || field.IsEmpty || field.CurrentGroupId != groupId)
                return;

            visited.Add(point);
            queue.Enqueue(field);
        }

        private bool CanMoveGroup(BlockGroup group, Vector2Int direction)
        {
            foreach (BlockCellState cell in group.Cells)
            {
                Vector2Int targetPoint = cell.Point + direction;

                if (!TryGetField(targetPoint, out Field targetField))
                    return false;

                if (!targetField.IsEmpty && !group.Contains(targetPoint))
                    return false;
            }

            return true;
        }

        private void MoveGroup(BlockGroup group, Vector2Int direction)
        {
            foreach (BlockCellState cell in group.Cells)
            {
                if (TryGetField(cell.Point, out Field field))
                    field.ClearObject();
            }

            foreach (BlockCellState cell in group.Cells)
            {
                Vector2Int targetPoint = cell.Point + direction;

                if (TryGetField(targetPoint, out Field field))
                    field.SetObject(cell.Object, cell.Color, cell.Sprite, group.Id, cell.VisualWorldPosition);
            }
        }

        private static Vector2Int NormalizeDirection(Vector2Int direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return direction.x > 0 ? Vector2Int.right : Vector2Int.left;

            return direction.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        private static void SortGroups(List<BlockGroup> groups, Vector2Int direction)
        {
            groups.Sort((a, b) =>
            {
                if (direction.x > 0)
                    return b.MaxX.CompareTo(a.MaxX);

                if (direction.x < 0)
                    return a.MinX.CompareTo(b.MinX);

                if (direction.y > 0)
                    return b.MaxY.CompareTo(a.MaxY);

                return a.MinY.CompareTo(b.MinY);
            });
        }

        private readonly struct BlockCellState
        {
            public BlockCellState(Field field)
            {
                Point = field.Point;
                Object = field.CurrentObject;
                Color = field.CurrentColor;
                Sprite = field.CurrentSprite;
                VisualWorldPosition = field.ObjectRendererWorldPosition;
            }

            public Vector2Int Point { get; }
            public GameObject Object { get; }
            public Color Color { get; }
            public Sprite Sprite { get; }
            public Vector3 VisualWorldPosition { get; }
        }

        private sealed class BlockGroup
        {
            private readonly List<BlockCellState> _cells = new List<BlockCellState>();

            public BlockGroup(int id)
            {
                Id = id;
                MinX = int.MaxValue;
                MaxX = int.MinValue;
                MinY = int.MaxValue;
                MaxY = int.MinValue;
            }

            public int Id { get; }
            public IReadOnlyList<BlockCellState> Cells => _cells;
            public int MinX { get; private set; }
            public int MaxX { get; private set; }
            public int MinY { get; private set; }
            public int MaxY { get; private set; }

            public void Add(Field field)
            {
                _cells.Add(new BlockCellState(field));
                MinX = Mathf.Min(MinX, field.Point.x);
                MaxX = Mathf.Max(MaxX, field.Point.x);
                MinY = Mathf.Min(MinY, field.Point.y);
                MaxY = Mathf.Max(MaxY, field.Point.y);
            }

            public bool Contains(Vector2Int point)
            {
                foreach (BlockCellState cell in _cells)
                {
                    if (cell.Point == point)
                        return true;
                }

                return false;
            }
        }
    }
}
