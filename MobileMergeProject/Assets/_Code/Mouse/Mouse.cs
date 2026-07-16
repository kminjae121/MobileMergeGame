using UnityEngine;
using _Code.Block;
using _Code.Field;

namespace _Code.Mouse
{
    public class Mouse : MonoBehaviour
    {
        private enum Corner
        {
            TopLeft,
            TopRight,
            BottomRight,
            BottomLeft
        }

        [SerializeField] private Corner _corner = Corner.TopLeft;
        [SerializeField, Min(0f)] private float _cornerPadding = 0.78f;
        [SerializeField] private SpriteRenderer _renderer;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            ApplySprite();
        }

        public void Initialize(BlockField blockField)
        {
            ApplySprite();
            ApplyPosition(blockField);
        }

        public bool TryMove(Vector2Int direction, BlockField blockField)
        {
            if (!TryGetNextCorner(direction, out Corner nextCorner))
                return false;

            _corner = nextCorner;
            ApplyPosition(blockField);
            return true;
        }

        public bool CanMove(Vector2Int direction)
        {
            return TryGetNextCorner(direction, out _);
        }

        private bool TryGetNextCorner(Vector2Int direction, out Corner nextCorner)
        {
            direction = NormalizeDirection(direction);
            nextCorner = _corner;

            switch (_corner)
            {
                case Corner.TopLeft:
                    if (direction == Vector2Int.right)
                    {
                        nextCorner = Corner.TopRight;
                        return true;
                    }

                    if (direction == Vector2Int.down)
                    {
                        nextCorner = Corner.BottomLeft;
                        return true;
                    }

                    return false;

                case Corner.TopRight:
                    if (direction == Vector2Int.left)
                    {
                        nextCorner = Corner.TopLeft;
                        return true;
                    }

                    if (direction == Vector2Int.down)
                    {
                        nextCorner = Corner.BottomRight;
                        return true;
                    }

                    return false;

                case Corner.BottomRight:
                    if (direction == Vector2Int.left)
                    {
                        nextCorner = Corner.BottomLeft;
                        return true;
                    }

                    if (direction == Vector2Int.up)
                    {
                        nextCorner = Corner.TopRight;
                        return true;
                    }

                    return false;

                case Corner.BottomLeft:
                    if (direction == Vector2Int.right)
                    {
                        nextCorner = Corner.BottomRight;
                        return true;
                    }

                    if (direction == Vector2Int.up)
                    {
                        nextCorner = Corner.TopLeft;
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        private void ApplyPosition(BlockField blockField)
        {
            if (blockField == null)
                return;

            Vector3 bottomLeft = blockField.GetWorldPosition(Vector2Int.zero);
            Vector3 topRight = blockField.GetWorldPosition(new Vector2Int(blockField.Width - 1, blockField.Height - 1));
            float left = bottomLeft.x - _cornerPadding;
            float right = topRight.x + _cornerPadding;
            float bottom = bottomLeft.y - _cornerPadding;
            float top = topRight.y + _cornerPadding;
            Vector3 position = transform.position;

            switch (_corner)
            {
                case Corner.TopLeft:
                    position.x = left;
                    position.y = top;
                    break;

                case Corner.TopRight:
                    position.x = right;
                    position.y = top;
                    break;

                case Corner.BottomRight:
                    position.x = right;
                    position.y = bottom;
                    break;

                case Corner.BottomLeft:
                    position.x = left;
                    position.y = bottom;
                    break;
            }

            transform.position = position;
        }

        private void ApplySprite()
        {
            if (_renderer == null || BlockBlastSpriteLibrary.MouseSprite == null)
                return;

            _renderer.sprite = BlockBlastSpriteLibrary.MouseSprite;
            _renderer.color = Color.white;
        }

        private static Vector2Int NormalizeDirection(Vector2Int direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return direction.x > 0 ? Vector2Int.right : Vector2Int.left;

            return direction.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }
}
