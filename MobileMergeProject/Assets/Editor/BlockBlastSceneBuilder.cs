using _Code.Block;
using _Code.Field;
using _Code.Manager;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Code.Editor
{
    public static class BlockBlastSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string GeneratedRootName = "BlockBlastGame";
        private const int Width = 8;
        private const int Height = 8;
        private const float CellSize = 0.72f;

        [MenuItem("Tools/Block Blast/Rebuild Sample Scene")]
        public static void RebuildSampleScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            DisableLegacyGameplayObjects();
            RemoveGeneratedRoot();

            Sprite sprite = GetDefaultSprite();
            GameObject root = new GameObject(GeneratedRootName);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0f, 0f, -10f);
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 6.2f;
                mainCamera.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
            }

            BlockField blockField = CreateBoard(root.transform, sprite);
            BlockPiece[] pieces = CreatePieces(root.transform, sprite, mainCamera);
            TextMeshPro scoreText = CreateText(root.transform, "ScoreText", "Score 0", new Vector3(0f, 4.75f, 0f), 8f, 0.52f);
            TextMeshPro messageText = CreateText(root.transform, "MessageText", string.Empty, new Vector3(0f, 4.18f, 0f), 7f, 0.42f);

            GameObject managerObject = new GameObject("BlockBlastManager");
            managerObject.transform.SetParent(root.transform);
            GameManager gameManager = managerObject.AddComponent<GameManager>();
            RandomBlockManager randomBlockManager = managerObject.AddComponent<RandomBlockManager>();

            SerializedObject randomSerializedObject = new SerializedObject(randomBlockManager);
            randomSerializedObject.FindProperty("_pieces").arraySize = pieces.Length;
            for (int i = 0; i < pieces.Length; i++)
                randomSerializedObject.FindProperty("_pieces").GetArrayElementAtIndex(i).objectReferenceValue = pieces[i];
            randomSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject gameSerializedObject = new SerializedObject(gameManager);
            gameSerializedObject.FindProperty("_blockField").objectReferenceValue = blockField;
            gameSerializedObject.FindProperty("_randomBlockManager").objectReferenceValue = randomBlockManager;
            gameSerializedObject.FindProperty("_pieces").arraySize = pieces.Length;
            for (int i = 0; i < pieces.Length; i++)
                gameSerializedObject.FindProperty("_pieces").GetArrayElementAtIndex(i).objectReferenceValue = pieces[i];
            gameSerializedObject.FindProperty("_scoreText").objectReferenceValue = scoreText;
            gameSerializedObject.FindProperty("_messageText").objectReferenceValue = messageText;
            gameSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static BlockField CreateBoard(Transform parent, Sprite sprite)
        {
            GameObject boardObject = new GameObject("Board");
            boardObject.transform.SetParent(parent);
            boardObject.transform.position = new Vector3(0f, 0.85f, 0f);

            BlockField blockField = boardObject.AddComponent<BlockField>();
            SerializedObject blockFieldSerializedObject = new SerializedObject(blockField);
            blockFieldSerializedObject.FindProperty("_width").intValue = Width;
            blockFieldSerializedObject.FindProperty("_height").intValue = Height;
            blockFieldSerializedObject.FindProperty("_cellSize").floatValue = CellSize;
            blockFieldSerializedObject.FindProperty("_snapDistance").floatValue = CellSize * 0.62f;
            blockFieldSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            Vector3 startPosition = new Vector3(-(Width - 1) * CellSize * 0.5f, -(Height - 1) * CellSize * 0.5f, 0f);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    GameObject fieldObject = new GameObject($"Field_{x}_{y}");
                    fieldObject.transform.SetParent(boardObject.transform);
                    fieldObject.transform.localPosition = startPosition + new Vector3(x * CellSize, y * CellSize, 0f);
                    fieldObject.transform.localScale = Vector3.one * (CellSize * 0.92f);

                    SpriteRenderer backgroundRenderer = fieldObject.AddComponent<SpriteRenderer>();
                    backgroundRenderer.sprite = sprite;
                    backgroundRenderer.color = new Color(0.16f, 0.2f, 0.27f);
                    backgroundRenderer.sortingOrder = 0;

                    BoxCollider2D collider = fieldObject.AddComponent<BoxCollider2D>();
                    collider.isTrigger = true;

                    GameObject fillObject = new GameObject("Fill");
                    fillObject.transform.SetParent(fieldObject.transform);
                    fillObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                    fillObject.transform.localScale = Vector3.one * 0.82f;

                    SpriteRenderer fillRenderer = fillObject.AddComponent<SpriteRenderer>();
                    fillRenderer.sprite = sprite;
                    fillRenderer.enabled = false;
                    fillRenderer.sortingOrder = 1;

                    _Code.Field.Field field = fieldObject.AddComponent<_Code.Field.Field>();
                    field.Configure(new Vector2Int(x, y), backgroundRenderer, fillRenderer);
                }
            }

            blockField.Rebuild();
            return blockField;
        }

        private static BlockPiece[] CreatePieces(Transform parent, Sprite sprite, Camera mainCamera)
        {
            Vector3[] positions =
            {
                new Vector3(-1.6f, -4.1f, 0f),
                new Vector3(0f, -3.95f, 0f),
                new Vector3(1.6f, -4.1f, 0f)
            };

            BlockPiece[] pieces = new BlockPiece[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject pieceObject = new GameObject($"Piece_{i + 1}");
                pieceObject.transform.SetParent(parent);
                pieceObject.transform.position = positions[i];

                BoxCollider2D collider = pieceObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;

                BlockPiece piece = pieceObject.AddComponent<BlockPiece>();
                BlockCellView[] cellViews = new BlockCellView[9];

                for (int j = 0; j < cellViews.Length; j++)
                {
                    GameObject cellObject = new GameObject($"Cell_{j + 1}");
                    cellObject.transform.SetParent(pieceObject.transform);
                    cellObject.transform.localPosition = Vector3.zero;

                    SpriteRenderer renderer = cellObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = 3;
                    renderer.enabled = false;

                    cellViews[j] = cellObject.AddComponent<BlockCellView>();
                }

                SerializedObject pieceSerializedObject = new SerializedObject(piece);
                pieceSerializedObject.FindProperty("_cellViews").arraySize = cellViews.Length;
                for (int j = 0; j < cellViews.Length; j++)
                    pieceSerializedObject.FindProperty("_cellViews").GetArrayElementAtIndex(j).objectReferenceValue = cellViews[j];
                pieceSerializedObject.FindProperty("_mainCamera").objectReferenceValue = mainCamera;
                pieceSerializedObject.FindProperty("_cellSize").floatValue = CellSize;
                pieceSerializedObject.FindProperty("_slotCellSize").floatValue = 0.46f;
                pieceSerializedObject.ApplyModifiedPropertiesWithoutUndo();

                pieces[i] = piece;
            }

            return pieces;
        }

        private static TextMeshPro CreateText(Transform parent, string name, string text, Vector3 position, float width, float fontSize)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent);
            textObject.transform.position = position;

            TextMeshPro textMeshPro = textObject.AddComponent<TextMeshPro>();
            textMeshPro.text = text;
            textMeshPro.fontSize = fontSize;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.color = Color.white;
            textMeshPro.rectTransform.sizeDelta = new Vector2(width, 1f);

            return textMeshPro;
        }

        private static Sprite GetDefaultSprite()
        {
            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            if (sprite == null)
                sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("Sprites/Default.psd");

            return sprite;
        }

        private static void DisableLegacyGameplayObjects()
        {
            string[] legacyNames = { "Map", "Manager", "spawningObj", "Square", "GameObject" };

            foreach (string legacyName in legacyNames)
            {
                GameObject legacyObject = GameObject.Find(legacyName);

                if (legacyObject != null)
                    legacyObject.SetActive(false);
            }
        }

        private static void RemoveGeneratedRoot()
        {
            GameObject generatedRoot = GameObject.Find(GeneratedRootName);

            if (generatedRoot != null)
                Object.DestroyImmediate(generatedRoot);
        }
    }
}
