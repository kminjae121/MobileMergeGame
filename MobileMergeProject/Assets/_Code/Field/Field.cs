using _Code.Block;
using DG.Tweening;
using UnityEngine;

namespace _Code.Field
{
    public class Field : MonoBehaviour
    {
        [field: SerializeField] public Vector2Int Point { get; private set; }
        [SerializeField] private SpriteRenderer _backgroundRenderer;
        [SerializeField] private SpriteRenderer _objectRenderer;
        [SerializeField, Min(0f)] private float _objectMoveDuration = 0.2f;
        [SerializeField] private Ease _objectMoveEase = Ease.Linear;

        private bool _isHaveObject;
        private GameObject _thisObject;
        private Color _currentColor = Color.white;
        private Sprite _currentSprite;
        private int _currentGroupId = -1;
        private bool _isStageCheese;
        private Vector3 _objectRendererHomeLocalPosition;
        private bool _hasObjectRendererHomeLocalPosition;
        private Tween _objectMoveTween;

        public bool IsHaveObject => _isHaveObject;
        public bool IsEmpty => !_isHaveObject;
        public GameObject CurrentObject => _thisObject;
        public Color CurrentColor => _currentColor;
        public Sprite CurrentSprite => _currentSprite;
        public int CurrentGroupId => _currentGroupId;
        public bool IsStageCheese => _isStageCheese;
        public Vector3 ObjectRendererWorldPosition => _objectRenderer != null ? _objectRenderer.transform.position : transform.position;

        private void Awake()
        {
            if (_backgroundRenderer == null)
                _backgroundRenderer = GetComponent<SpriteRenderer>();

            CaptureObjectRendererHomePosition();
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
            SetObject(obj, color, sprite, groupId, null);
        }

        public void SetObject(GameObject obj, Color color, Sprite sprite, int groupId, Vector3? moveFromWorldPosition)
        {
            SetObject(obj, color, sprite, groupId, moveFromWorldPosition, false);
        }

        public void SetStageCheeseObject(GameObject obj, Sprite sprite, int groupId)
        {
            SetObject(obj, Color.white, sprite, groupId, null, true);
        }

        private void SetObject(GameObject obj, Color color, Sprite sprite, int groupId, Vector3? moveFromWorldPosition, bool isStageCheese)
        {
            _isHaveObject = true;
            _thisObject = obj;
            _currentColor = color;
            _currentSprite = sprite != null ? sprite : BlockBlastSpriteLibrary.CatBlockSprite;
            _currentGroupId = groupId;
            _isStageCheese = isStageCheese;

            if (_objectRenderer == null)
                return;

            bool hasCatSprite = ApplyObjectSprite();
            _objectRenderer.color = hasCatSprite ? Color.white : color;
            _objectRenderer.enabled = true;

            MoveObjectRenderer(moveFromWorldPosition);
        }

        public void ClearObject()
        {
            _isHaveObject = false;
            _thisObject = null;
            _currentColor = Color.white;
            _currentSprite = null;
            _currentGroupId = -1;
            _isStageCheese = false;

            if (_objectRenderer != null)
            {
                StopObjectMove();
                SnapObjectRendererToHome();
                _objectRenderer.enabled = false;
            }
        }

        public void Configure(Vector2Int point, SpriteRenderer backgroundRenderer, SpriteRenderer objectRenderer)
        {
            Point = point;
            _backgroundRenderer = backgroundRenderer;
            _objectRenderer = objectRenderer;
            _hasObjectRendererHomeLocalPosition = false;
            CaptureObjectRendererHomePosition();

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

        private void MoveObjectRenderer(Vector3? moveFromWorldPosition)
        {
            if (_objectRenderer == null)
                return;

            StopObjectMove();

            Vector3 targetPosition = GetObjectRendererHomeWorldPosition();

            if (!moveFromWorldPosition.HasValue || _objectMoveDuration <= 0f)
            {
                SnapObjectRendererToHome();
                return;
            }

            Vector3 startPosition = moveFromWorldPosition.Value;
            startPosition.z = targetPosition.z;
            _objectRenderer.transform.position = startPosition;
            _objectMoveTween = _objectRenderer.transform
                .DOMove(targetPosition, _objectMoveDuration, false)
                .SetEase(_objectMoveEase)
                .OnComplete(SnapObjectRendererToHome);
        }

        private void StopObjectMove()
        {
            if (_objectMoveTween != null && _objectMoveTween.IsActive())
                _objectMoveTween.Kill();

            _objectMoveTween = null;

            if (_objectRenderer != null)
                _objectRenderer.transform.DOKill();
        }

        private void CaptureObjectRendererHomePosition()
        {
            if (_objectRenderer == null || _hasObjectRendererHomeLocalPosition)
                return;

            _objectRendererHomeLocalPosition = _objectRenderer.transform.localPosition;
            _hasObjectRendererHomeLocalPosition = true;
        }

        private Vector3 GetObjectRendererHomeWorldPosition()
        {
            if (_objectRenderer == null)
                return transform.position;

            CaptureObjectRendererHomePosition();
            Transform objectTransform = _objectRenderer.transform;
            return objectTransform.parent != null
                ? objectTransform.parent.TransformPoint(_objectRendererHomeLocalPosition)
                : _objectRendererHomeLocalPosition;
        }

        private void SnapObjectRendererToHome()
        {
            if (_objectRenderer == null)
                return;

            CaptureObjectRendererHomePosition();
            _objectRenderer.transform.localPosition = _objectRendererHomeLocalPosition;
        }
    }
}
