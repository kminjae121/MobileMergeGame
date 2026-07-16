using UnityEngine;

namespace _Code.Block
{
    public class BlockCellView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        private Sprite _sprite;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            ApplyCatSprite();
        }

        public void SetSprite(Sprite sprite)
        {
            _sprite = sprite;
            ApplyCatSprite();
        }

        public void SetVisible(bool visible)
        {
            if (_renderer != null)
                _renderer.enabled = visible;
        }

        public void SetColor(Color color)
        {
            if (_renderer != null)
            {
                bool hasCatSprite = ApplyCatSprite();
                _renderer.color = hasCatSprite ? Color.white : color;
            }
        }

        public void SetSortingOrder(int order)
        {
            if (_renderer != null)
                _renderer.sortingOrder = order;
        }

        private bool ApplyCatSprite()
        {
            Sprite sprite = _sprite != null ? _sprite : BlockBlastSpriteLibrary.CatBlockSprite;

            if (_renderer == null || sprite == null)
                return false;

            _renderer.sprite = sprite;
            return true;
        }
    }
}
