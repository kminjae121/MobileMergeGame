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

        public bool IsHaveObject => _isHaveObject;
        public bool IsEmpty => !_isHaveObject;
        public GameObject CurrentObject => _thisObject;

        private void Awake()
        {
            if (_backgroundRenderer == null)
                _backgroundRenderer = GetComponent<SpriteRenderer>();
        }
        
        public void SetObject(GameObject obj)
        {
            SetObject(obj, Color.white);
        }

        public void SetObject(GameObject obj, Color color)
        {
            _isHaveObject = true;
            _thisObject = obj;

            if (_objectRenderer == null)
                return;

            _objectRenderer.color = color;
            _objectRenderer.enabled = true;
        }

        public void ClearObject()
        {
            _isHaveObject = false;
            _thisObject = null;

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
    }
}
