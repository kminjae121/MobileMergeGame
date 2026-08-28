using System.Collections.Generic;
using UnityEngine;

namespace _Code.Field
{
    public static class BlockCellVisualPool
    {
        private const string RootName = "[BlockCellVisualPool]";

        private static readonly Stack<SpriteRenderer> _pool = new Stack<SpriteRenderer>(64);
        private static Transform _root;

        public static void Prewarm(int count, SpriteRenderer template)
        {
            if (count <= 0)
                return;

            RemoveDestroyedRenderers();
            int missingCount = Mathf.Max(0, count - _pool.Count);

            for (int i = 0; i < missingCount; i++)
                Release(CreateRenderer(template));
        }

        public static SpriteRenderer Get(
            SpriteRenderer template,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            SpriteRenderer renderer = TakeRenderer(template);
            ApplyTemplate(renderer, template);

            Transform rendererTransform = renderer.transform;
            rendererTransform.SetParent(parent, false);
            rendererTransform.localPosition = localPosition;
            rendererTransform.localRotation = localRotation;
            rendererTransform.localScale = localScale;

            renderer.gameObject.SetActive(true);
            renderer.enabled = true;
            return renderer;
        }

        public static void Release(SpriteRenderer renderer)
        {
            if (renderer == null)
                return;

            renderer.enabled = false;
            renderer.sprite = null;
            renderer.color = Color.white;

            Transform rendererTransform = renderer.transform;
            rendererTransform.SetParent(GetRoot(), false);
            rendererTransform.localPosition = Vector3.zero;
            rendererTransform.localRotation = Quaternion.identity;
            rendererTransform.localScale = Vector3.one;

            renderer.gameObject.SetActive(false);
            _pool.Push(renderer);
        }

        private static SpriteRenderer TakeRenderer(SpriteRenderer template)
        {
            while (_pool.Count > 0)
            {
                SpriteRenderer renderer = _pool.Pop();

                if (renderer != null)
                    return renderer;
            }

            return CreateRenderer(template);
        }

        private static void RemoveDestroyedRenderers()
        {
            if (_pool.Count == 0)
                return;

            List<SpriteRenderer> validRenderers = new List<SpriteRenderer>(_pool.Count);

            while (_pool.Count > 0)
            {
                SpriteRenderer renderer = _pool.Pop();

                if (renderer != null)
                    validRenderers.Add(renderer);
            }

            foreach (SpriteRenderer renderer in validRenderers)
                _pool.Push(renderer);
        }

        private static SpriteRenderer CreateRenderer(SpriteRenderer template)
        {
            GameObject visualObject = new GameObject("PooledBlockCellVisual");
            SpriteRenderer renderer = visualObject.AddComponent<SpriteRenderer>();
            ApplyTemplate(renderer, template);
            return renderer;
        }

        private static void ApplyTemplate(SpriteRenderer renderer, SpriteRenderer template)
        {
            if (renderer == null || template == null)
                return;

            renderer.sharedMaterial = template.sharedMaterial;
            renderer.sortingLayerID = template.sortingLayerID;
            renderer.sortingOrder = template.sortingOrder;
            renderer.maskInteraction = template.maskInteraction;
        }

        private static Transform GetRoot()
        {
            if (_root != null)
                return _root;

            GameObject rootObject = new GameObject(RootName);
            _root = rootObject.transform;
            return _root;
        }
    }
}
