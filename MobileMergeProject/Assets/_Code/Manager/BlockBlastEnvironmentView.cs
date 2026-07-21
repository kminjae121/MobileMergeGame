using _Code.Block;
using _Code.Field;
using TMPro;
using UnityEngine;

namespace _Code.Manager
{
    public class BlockBlastEnvironmentView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private SpriteRenderer _catTowerFrameRenderer;
        [SerializeField] private TextMeshPro _titleText;
        [SerializeField] private int _backgroundSortingOrder = -30;
        [SerializeField] private int _catTowerFrameSortingOrder = -2;
        [SerializeField, Min(0f)] private float _mouseCornerPadding = 0.78f;
        [SerializeField, Range(0.1f, 0.49f)] private float _catTowerCornerCenterRatio = 0.35f;

        private const string GameTitle = "\uC950\uAD6C\uBA4D \uB9C8\uC6B0\uC2A4\uD321";

        public void Configure(BlockField blockField, Camera targetCamera)
        {
            ConfigureBackground(targetCamera);
            ConfigureCatTowerFrame(blockField);
            ConfigureTitle();
        }

        private void ConfigureBackground(Camera targetCamera)
        {
            if (_backgroundRenderer == null)
                return;

            Sprite sprite = BlockBlastSpriteLibrary.CleanCatHomeBackgroundSprite;

            if (sprite != null)
                _backgroundRenderer.sprite = sprite;

            _backgroundRenderer.color = Color.white;
            _backgroundRenderer.sortingOrder = _backgroundSortingOrder;

            if (targetCamera == null || !targetCamera.orthographic || _backgroundRenderer.sprite == null)
            {
                _backgroundRenderer.transform.localScale = Vector3.one;
                return;
            }

            Vector3 cameraPosition = targetCamera.transform.position;
            _backgroundRenderer.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, 0f);

            float cameraHeight = targetCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * targetCamera.aspect;
            Vector2 spriteSize = _backgroundRenderer.sprite.bounds.size;
            float scale = Mathf.Max(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y);
            _backgroundRenderer.transform.localScale = Vector3.one * scale;
        }

        private void ConfigureCatTowerFrame(BlockField blockField)
        {
            if (_catTowerFrameRenderer == null || blockField == null)
                return;

            Sprite sprite = BlockBlastSpriteLibrary.MouseHoleFrameSprite;

            if (sprite != null)
                _catTowerFrameRenderer.sprite = sprite;

            _catTowerFrameRenderer.color = Color.white;
            _catTowerFrameRenderer.sortingOrder = _catTowerFrameSortingOrder;

            Vector3 bottomLeft = blockField.GetWorldPosition(Vector2Int.zero);
            Vector3 topRight = blockField.GetWorldPosition(new Vector2Int(blockField.Width - 1, blockField.Height - 1));
            Vector3 center = (bottomLeft + topRight) * 0.5f;
            _catTowerFrameRenderer.transform.position = new Vector3(center.x, center.y, 0f);

            if (_catTowerFrameRenderer.sprite == null)
                return;

            float mouseHalfWidth = Mathf.Abs(topRight.x - bottomLeft.x) * 0.5f + _mouseCornerPadding;
            float mouseHalfHeight = Mathf.Abs(topRight.y - bottomLeft.y) * 0.5f + _mouseCornerPadding;
            float targetWidth = mouseHalfWidth / _catTowerCornerCenterRatio;
            float targetHeight = mouseHalfHeight / _catTowerCornerCenterRatio;
            Vector2 spriteSize = _catTowerFrameRenderer.sprite.bounds.size;
            _catTowerFrameRenderer.transform.localScale = new Vector3(targetWidth / spriteSize.x, targetHeight / spriteSize.y, 1f);
        }

        private void ConfigureTitle()
        {
            if (_titleText == null)
                return;

            _titleText.text = GameTitle;
            _titleText.fontSize = 0.66f;
            _titleText.alignment = TextAlignmentOptions.Center;
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.color = new Color(0.42f, 0.23f, 0.11f);
            _titleText.rectTransform.sizeDelta = new Vector2(8.5f, 1f);
            _titleText.transform.position = new Vector3(0f, 5.35f, 0f);
        }
    }
}
