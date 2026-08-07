using _Code.Block;
using UnityEngine;

namespace _Code.Field
{
    public class Field : MonoBehaviour
    {
        [field: SerializeField] public Vector2Int Point { get; private set; }
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private SpriteRenderer _objectRenderer;

        private bool _isHaveObject;
        private GameObject _thisObject;
        private Color _currentColor = Color.white;
        private Sprite _currentSprite;
        private int _currentGroupId = -1;

        private int name;

        public bool IsHaveObject => _isHaveObject;
        public bool IsEmpty => !_isHaveObject;
        public GameObject CurrentObject => _thisObject;
        public Color CurrentColor => _currentColor;
        public Sprite CurrentSprite => _currentSprite;
        public int CurrentGroupId => _currentGroupId;

        private void Awake()
        {
            if (_backgroundRenderer == null)
                _backgroundRenderer = GetComponent<SpriteRenderer>();

            ApplyObjectSprite();
        }
        
        public void SetObject(GameObject obj)
        {
            SetObject(obj, Color.white);
        }

        public void SetObject(GameObject obj, Color color)
        {
            SetObject(obj, color, BlockBlastSpriteLibrary.CatBlockSprite, obj != null ? obj.GetInstanceID() : -1);
        }

        public void SetObject(GameObject obj, Color color, int groupId)
        {
            SetObject(obj, color, BlockBlastSpriteLibrary.CatBlockSprite, groupId);
        }

        public void SetObject(GameObject obj, Color color, Sprite sprite, int groupId)
        {
            _isHaveObject = true;
            _thisObject = obj;
            _currentColor = color;
            _currentSprite = sprite != null ? sprite : BlockBlastSpriteLibrary.CatBlockSprite;
            _currentGroupId = groupId;

            if (_objectRenderer == null)
                return;

            bool hasCatSprite = ApplyObjectSprite();
            _objectRenderer.color = hasCatSprite ? Color.white : color;
            _objectRenderer.enabled = true;
        }

        public void ClearObject()
        {
            _isHaveObject = false;
            _thisObject = null;
            _currentColor = Color.white;
            _currentSprite = null;
            _currentGroupId = -1;

            if (_objectRenderer != null)
                _objectRenderer.enabled = false;
        }

        public void Configure(Vector2Int point, SpriteRenderer backgroundRenderer, SpriteRenderer objectRenderer)
        {
            Point = point;
            _backgroundRenderer = backgroundRenderer;
            _objectRenderer = objectRenderer;

            ClearObject();
        }

        public void SetBackgroundColor(Color color)
        {
            if (_backgroundRenderer != null)
                _backgroundRenderer.color = color;
        }

        private bool ApplyObjectSprite()
        {
            Sprite sprite = _currentSprite != null ? _currentSprite : BlockBlastSpriteLibrary.CatBlockSprite;

            if (_objectRenderer == null || sprite == null)
                return false;

            _objectRenderer.sprite = sprite;
            return true;
        }
    }
}
