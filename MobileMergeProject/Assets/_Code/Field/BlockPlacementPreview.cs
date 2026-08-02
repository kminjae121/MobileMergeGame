using System.Collections.Generic;
using _Code.Block;
using UnityEngine;

namespace _Code.Field
{
    public sealed class BlockPlacementPreview : MonoBehaviour
    {
        [SerializeField] private Color _canInstallColor = new Color(0.35f, 0.85f, 1f, 0.7f);
        [SerializeField, Min(0.1f)] private float _cellSizeMultiplier = 1f;
        [SerializeField] private int _sortingOrder = 12;

        private const int PreviewTextureSize = 32;
        private const int PreviewBorderSize = 3;

        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>(9);
        private Sprite _previewSprite;

        public void Show(BlockField blockField, BlockPiece piece)
        {
            if (blockField == null || piece == null || !blockField.TryGetAnchorFor(piece, out Vector2Int anchor))
            {
                Hide();
                return;
            }

            if (!blockField.CanInstall(piece, anchor))
            {
                Hide();
                return;
            }

            EnsureRenderers(piece.Cells.Count);

            Vector3 anchorPosition = blockField.GetWorldPosition(anchor);
            Vector3 horizontalStep = GetHorizontalStep(blockField);
            Vector3 verticalStep = GetVerticalStep(blockField);
            float cellWorldSize = Mathf.Min(horizontalStep.magnitude, verticalStep.magnitude) * _cellSizeMultiplier;
            Sprite previewSprite = GetPreviewSprite(piece);
            Vector3 previewScale = GetPreviewScale(previewSprite, cellWorldSize);
            Color previewColor = GetPreviewColor(previewSprite);

            for (int i = 0; i < _renderers.Count; i++)
            {
                bool isActive = i < piece.Cells.Count;
                SpriteRenderer renderer = _renderers[i];
                renderer.gameObject.SetActive(isActive);

                if (!isActive)
                    continue;

                Vector2Int cell = piece.Cells[i];
                Vector2Int point = anchor + cell;
                Vector3 position = blockField.TryGetField(point, out Field field)
                    ? field.transform.position
                    : anchorPosition + horizontalStep * cell.x + verticalStep * cell.y;

                renderer.transform.position = position;
                renderer.transform.localScale = previewScale;
                renderer.sprite = previewSprite;
                renderer.color = previewColor;
                renderer.sortingOrder = _sortingOrder;
            }
        }

        public void Hide()
        {
            foreach (SpriteRenderer renderer in _renderers)
            {
                if (renderer != null)
                    renderer.gameObject.SetActive(false);
            }
        }

        private void EnsureRenderers(int count)
        {
            if (_previewSprite == null)
                _previewSprite = CreatePreviewSprite();

            while (_renderers.Count < count)
            {
                GameObject previewCell = new GameObject($"PlacementPreviewCell_{_renderers.Count + 1}");
                previewCell.transform.SetParent(transform, false);

                SpriteRenderer renderer = previewCell.AddComponent<SpriteRenderer>();
                renderer.sprite = _previewSprite;
                renderer.sortingOrder = _sortingOrder;
                renderer.gameObject.SetActive(false);
                _renderers.Add(renderer);
            }
        }

        private Sprite GetPreviewSprite(BlockPiece piece)
        {
            Sprite sprite = piece.BlockSprite;
            if (sprite != null)
                return sprite;

            if (_previewSprite == null)
                _previewSprite = CreatePreviewSprite();

            return _previewSprite;
        }

        private Color GetPreviewColor(Sprite sprite)
        {
            if (sprite == _previewSprite)
                return _canInstallColor;

            return new Color(1f, 1f, 1f, _canInstallColor.a);
        }

        private static Vector3 GetPreviewScale(Sprite sprite, float cellWorldSize)
        {
            if (sprite == null)
                return Vector3.one * cellWorldSize;

            float spriteSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            if (spriteSize <= Mathf.Epsilon)
                return Vector3.one * cellWorldSize;

            return Vector3.one * (cellWorldSize / spriteSize);
        }

        private static Vector3 GetHorizontalStep(BlockField blockField)
        {
            if (blockField.Width > 1)
                return blockField.GetWorldPosition(Vector2Int.right) - blockField.GetWorldPosition(Vector2Int.zero);

            return blockField.transform.right * blockField.CellSize * blockField.transform.lossyScale.x;
        }

        private static Vector3 GetVerticalStep(BlockField blockField)
        {
            if (blockField.Height > 1)
                return blockField.GetWorldPosition(Vector2Int.up) - blockField.GetWorldPosition(Vector2Int.zero);

            return blockField.transform.up * blockField.CellSize * blockField.transform.lossyScale.y;
        }

        private static Sprite CreatePreviewSprite()
        {
            Texture2D texture = new Texture2D(PreviewTextureSize, PreviewTextureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < PreviewTextureSize; y++)
            {
                for (int x = 0; x < PreviewTextureSize; x++)
                {
                    bool isBorder = x < PreviewBorderSize ||
                                    y < PreviewBorderSize ||
                                    x >= PreviewTextureSize - PreviewBorderSize ||
                                    y >= PreviewTextureSize - PreviewBorderSize;
                    float alpha = isBorder ? 1f : 0.28f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, PreviewTextureSize, PreviewTextureSize), new Vector2(0.5f, 0.5f), PreviewTextureSize);
        }
    }
}
