using UnityEngine;

namespace _Code.Block
{
    public class BlockCellView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
        }

        public void SetVisible(bool visible)
        {
            if (_renderer != null)
                _renderer.enabled = visible;
        }

        public void SetColor(Color color)
        {
            if (_renderer != null)
                _renderer.color = color;
        }

        public void SetSortingOrder(int order)
        {
            if (_renderer != null)
                _renderer.sortingOrder = order;
        }
    }
}
