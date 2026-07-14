using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Code.Block
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BlockPiece : Blcok
    {
        [SerializeField] private BlockCellView[] _cellViews;
        [SerializeField] private Camera _mainCamera;
        [SerializeField, Min(0.1f)] private float _cellSize = 0.72f;
        [SerializeField, Min(0.1f)] private float _slotCellSize = 0.46f;
        [SerializeField] private float _dragLift = 0.45f;
        [SerializeField] private int _defaultSortingOrder = 3;
        [SerializeField] private int _dragSortingOrder = 20;

        private const int MousePointerId = -1;

        private static BlockPiece _activePiece;

        private readonly List<Vector2Int> _cells = new List<Vector2Int>(9);
        private BoxCollider2D _touchCollider;
        private Color _color = Color.white;
        private Vector2 _visualCenter;
        private Vector3 _slotPosition;
        private Vector3 _dragOffset;
        private int _activePointerId = MousePointerId;
        private bool _isDragging;
        private bool _isPlaced = true;

        public event Action<BlockPiece> Released;

        public override IReadOnlyList<Vector2Int> Cells => _cells;
        public override Color BlockColor => _color;
        public bool IsPlaced => _isPlaced;
        public float CellSize => _cellSize;
        public static bool IsAnyDragging => _activePiece != null;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            if (_cellViews == null || _cellViews.Length == 0)
                _cellViews = GetComponentsInChildren<BlockCellView>(true);

            _touchCollider = GetComponent<BoxCollider2D>();
            _slotPosition = transform.position;
        }

        private void Update()
        {
            if (_isPlaced)
                return;

            if (Input.touchCount > 0)
            {
                HandleTouchInput();
                return;
            }

            if (_isDragging && _activePointerId != MousePointerId)
            {
                EndDrag();
                return;
            }

            HandleMouseInput();
        }

        private void OnDisable()
        {
            if (_isDragging)
                _isDragging = false;

            ClearActivePointer();
        }

        public void CaptureSlotPosition()
        {
            _slotPosition = transform.position;
        }

        public void Configure(BlockShape shape, Color color)
        {
            gameObject.SetActive(true);

            _cells.Clear();
            for (int i = 0; i < shape.Cells.Count; i++)
                _cells.Add(shape.Cells[i]);

            _visualCenter = shape.VisualCenter;
            _color = color;
            _isPlaced = false;
            _isDragging = false;
            transform.position = _slotPosition;
            ApplySlotView();
        }

        public Vector3 GetAnchorWorldPosition()
        {
            return transform.position - new Vector3(_visualCenter.x * _cellSize, _visualCenter.y * _cellSize, 0f);
        }

        public void SnapTo(Vector3 anchorWorldPosition)
        {
            transform.position = anchorWorldPosition + new Vector3(_visualCenter.x * _cellSize, _visualCenter.y * _cellSize, 0f);
        }

        public void ReturnToSlot()
        {
            _isDragging = false;
            ClearActivePointer();
            transform.position = _slotPosition;
            ApplySlotView();
            SetSortingOrder(_defaultSortingOrder);
        }

        public void MarkPlaced()
        {
            _isPlaced = true;
            ClearActivePointer();
            gameObject.SetActive(false);
        }

        private void HandleTouchInput()
        {
            if (_isDragging)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);

                    if (touch.fingerId != _activePointerId)
                        continue;

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        EndDrag();
                    else
                        MoveDrag(touch.position);

                return;
                }

                EndDrag();
                return;
            }

            if (_activePiece != null)
                return;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began && IsPointerInside(touch.position))
                {
                    BeginDrag(touch.fingerId, touch.position);
                    return;
                }
            }
        }

        private void HandleMouseInput()
        {
            if (_isDragging && _activePointerId == MousePointerId)
            {
                if (Input.GetMouseButtonUp(0))
                    EndDrag();
                else if (Input.GetMouseButton(0))
                    MoveDrag(Input.mousePosition);

                return;
            }

            if (_activePiece != null || !Input.GetMouseButtonDown(0) || !IsPointerInside(Input.mousePosition))
                return;

            BeginDrag(MousePointerId, Input.mousePosition);
        }

        private void BeginDrag(int pointerId, Vector3 screenPosition)
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            _activePiece = this;
            _activePointerId = pointerId;
            ApplyDragView();
            _dragOffset = transform.position - GetPointerWorldPosition(screenPosition);
            _isDragging = true;
            SetSortingOrder(_dragSortingOrder);
        }

        private void MoveDrag(Vector3 screenPosition)
        {
            transform.position = GetPointerWorldPosition(screenPosition) + _dragOffset + Vector3.up * _dragLift;
        }

        private void EndDrag()
        {
            _isDragging = false;
            ClearActivePointer();
            SetSortingOrder(_defaultSortingOrder);
            Released?.Invoke(this);
        }

        private bool IsPointerInside(Vector3 screenPosition)
        {
            if (_touchCollider == null)
                return false;

            if (_mainCamera == null)
                _mainCamera = Camera.main;

            if (_mainCamera == null)
                return false;

            return _touchCollider.OverlapPoint(GetPointerWorldPosition(screenPosition));
        }

        private Vector3 GetPointerWorldPosition(Vector3 screenPosition)
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            screenPosition.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = transform.position.z;
            return worldPosition;
        }

        private void ClearActivePointer()
        {
            if (_activePiece == this)
                _activePiece = null;

            _activePointerId = MousePointerId;
        }

        private void UpdateCellViews(float displayCellSize)
        {
            for (int i = 0; i < _cellViews.Length; i++)
            {
                bool isActive = i < _cells.Count;
                BlockCellView cellView = _cellViews[i];
                cellView.SetVisible(isActive);

                if (!isActive)
                    continue;

                Vector2Int cell = _cells[i];
                cellView.transform.localPosition = new Vector3((cell.x - _visualCenter.x) * displayCellSize, (cell.y - _visualCenter.y) * displayCellSize, 0f);
                cellView.transform.localScale = Vector3.one * (displayCellSize * 0.9f);
                cellView.SetColor(_color);
                cellView.SetSortingOrder(_defaultSortingOrder);
            }
        }

        private void UpdateCollider(float displayCellSize)
        {
            if (_touchCollider == null || _cells.Count == 0)
                return;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (Vector2Int cell in _cells)
            {
                Vector2 localPosition = new Vector2((cell.x - _visualCenter.x) * displayCellSize, (cell.y - _visualCenter.y) * displayCellSize);
                minX = Mathf.Min(minX, localPosition.x);
                maxX = Mathf.Max(maxX, localPosition.x);
                minY = Mathf.Min(minY, localPosition.y);
                maxY = Mathf.Max(maxY, localPosition.y);
            }

            _touchCollider.offset = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            _touchCollider.size = new Vector2(maxX - minX + displayCellSize, maxY - minY + displayCellSize);
            _touchCollider.isTrigger = true;
        }

        private void SetSortingOrder(int order)
        {
            foreach (BlockCellView cellView in _cellViews)
                cellView.SetSortingOrder(order);
        }

        private void ApplySlotView()
        {
            transform.localScale = Vector3.one;
            UpdateCellViews(_slotCellSize);
            UpdateCollider(_slotCellSize);
        }

        private void ApplyDragView()
        {
            transform.localScale = Vector3.one;
            UpdateCellViews(_cellSize);
            UpdateCollider(_cellSize);
        }
    }
}
