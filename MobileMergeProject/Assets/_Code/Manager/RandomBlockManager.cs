using System.Collections.Generic;
using _Code.Block;
using _Code.Field;
using UnityEngine;

namespace _Code.Manager
{
    public class RandomBlockManager : MonoBehaviour
    {
        [SerializeField] private BlockPiece[] _pieces;

        private readonly List<BlockShape> _installableShapes = new List<BlockShape>();

        private readonly Color[] _colors =
        {
            new Color(0.98f, 0.35f, 0.33f),
            new Color(1f, 0.73f, 0.25f),
            new Color(0.27f, 0.72f, 1f),
            new Color(0.35f, 0.88f, 0.54f),
            new Color(0.74f, 0.48f, 1f),
            new Color(1f, 0.46f, 0.76f)
        };

        public IReadOnlyList<BlockPiece> Pieces => _pieces;

        public void Initialize(BlockPiece[] pieces)
        {
            if (pieces != null && pieces.Length > 0)
                _pieces = pieces;

            if (_pieces == null)
                return;

            foreach (BlockPiece piece in _pieces)
                piece.CaptureSlotPosition();
        }

        public bool GiveNewSet(BlockField blockField)
        {
            if (_pieces == null || blockField == null)
                return false;

            CacheInstallableShapes(blockField);

            if (_installableShapes.Count == 0)
                return false;

            foreach (BlockPiece piece in _pieces)
            {
                BlockShape shape = _installableShapes[Random.Range(0, _installableShapes.Count)];
                Color color = _colors[Random.Range(0, _colors.Length)];
                Sprite catSprite = BlockBlastSpriteLibrary.GetRandomCatBlockSprite();

                piece.Configure(shape, color, catSprite);
            }

            return true;
        }

        public bool AreAllPiecesPlaced()
        {
            if (_pieces == null || _pieces.Length == 0)
                return false;

            foreach (BlockPiece piece in _pieces)
            {
                if (!piece.IsPlaced)
                    return false;
            }

            return true;
        }

        public bool HasAnyAvailablePlacement(BlockField blockField)
        {
            if (_pieces == null || blockField == null)
                return false;

            foreach (BlockPiece piece in _pieces)
            {
                if (!piece.IsPlaced && blockField.HasAnyPlacement(piece))
                    return true;
            }

            return false;
        }

        public bool HasAnyInstallableShape(BlockField blockField)
        {
            if (blockField == null)
                return false;

            foreach (BlockShape shape in BlockShapeCatalog.Shapes)
            {
                if (blockField.HasAnyPlacement(shape))
                    return true;
            }

            return false;
        }

        private void CacheInstallableShapes(BlockField blockField)
        {
            _installableShapes.Clear();

            foreach (BlockShape shape in BlockShapeCatalog.Shapes)
            {
                if (blockField.HasAnyPlacement(shape))
                    _installableShapes.Add(shape);
            }
        }
    }
}
