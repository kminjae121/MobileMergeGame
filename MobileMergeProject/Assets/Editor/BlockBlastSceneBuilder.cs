using _Code.Block;
using _Code.Field;
using _Code.Manager;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MouseView = _Code.Mouse.Mouse;

namespace _Code.Editor
{
    public static class BlockBlastSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string GeneratedRootName = "BlockBlastGame";
        private const int Width = 8;
        private const int Height = 8;
        private const float CellSize = 0.72f;
        private const string CatBlockSpritePath = "Assets/Resources/BlockBlast/BlackCatBlockSprite.png";
        private const string MouseSpritePath = "Assets/Resources/BlockBlast/MouseSprite.png";
        private const string CleanCatHomeBackgroundSpritePath = "Assets/Resources/BlockBlast/CleanCatHomeBackground.png";
        private const string MouseHoleFrameSpritePath = "Assets/Resources/BlockBlast/MouseHoleFrameSprite.png";
        private const string BackgroundName = "CleanCatHomeBackground";
        private const string CatTowerFrameName = "CatTowerFrame";
        private const string TitleName = "TitleText";
        private const float MouseCornerPadding = 0.78f;
        private const float MouseScale = 0.9f;
        private const float MouseCushionCenterYOffset = 0.3f;
        private const float CatTowerCornerCenterRatio = 0.35f;

        [MenuItem("Tools/Block Blast/Rebuild Sample Scene")]
        public static void RebuildSampleScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            DisableLegacyGameplayObjects();
            RemoveGeneratedRoot();

            Sprite defaultSprite = GetDefaultSprite();
            Sprite catBlockSprite = GetSprite(CatBlockSpritePath, defaultSprite);
            Sprite mouseSprite = GetSprite(MouseSpritePath, defaultSprite);
            Sprite backgroundSprite = GetSprite(CleanCatHomeBackgroundSpritePath, defaultSprite);
            Sprite mouseHoleFrameSprite = GetSprite(MouseHoleFrameSpritePath, defaultSprite);
            GameObject root = new GameObject(GeneratedRootName);

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0f, 0f, -10f);
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 6.2f;
                mainCamera.backgroundColor = new Color(0.98f, 0.96f, 0.92f);
            }

            SpriteRenderer backgroundRenderer = CreateSpriteRenderer(root.transform, BackgroundName, backgroundSprite, -30);
            SpriteRenderer catTowerFrameRenderer = CreateSpriteRenderer(root.transform, CatTowerFrameName, mouseHoleFrameSprite, -2);
            BlockField blockField = CreateBoard(root.transform, defaultSprite, catBlockSprite);
            MouseView mouse = CreateMouse(root.transform, mouseSprite, blockField);
            BlockPiece[] pieces = CreatePieces(root.transform, catBlockSprite, mainCamera);
            CreateText(root.transform, "TitleText", "\uC950\uAD6C\uBA4D \uB9C8\uC6B0\uC2A4\uD321", new Vector3(0f, 5.35f, 0f), 8.5f, 0.66f);
            TextMeshPro scoreText = CreateText(root.transform, "ScoreText", "Score 0", new Vector3(0f, 4.75f, 0f), 8f, 0.52f);
            TextMeshPro messageText = CreateText(root.transform, "MessageText", string.Empty, new Vector3(0f, 4.18f, 0f), 7f, 0.42f);
            TextMeshPro titleText = root.transform.Find(TitleName).GetComponent<TextMeshPro>();

            GameObject managerObject = new GameObject("BlockBlastManager");
            managerObject.transform.SetParent(root.transform);
            GameManager gameManager = managerObject.AddComponent<GameManager>();
            BlockBlastEnvironmentView environmentView = managerObject.AddComponent<BlockBlastEnvironmentView>();
            RandomBlockManager randomBlockManager = managerObject.AddComponent<RandomBlockManager>();

            SerializedObject environmentSerializedObject = new SerializedObject(environmentView);
            environmentSerializedObject.FindProperty("_backgroundRenderer").objectReferenceValue = backgroundRenderer;
            environmentSerializedObject.FindProperty("_catTowerFrameRenderer").objectReferenceValue = catTowerFrameRenderer;
            environmentSerializedObject.FindProperty("_titleText").objectReferenceValue = titleText;
            environmentSerializedObject.FindProperty("_mouseCornerPadding").floatValue = MouseCornerPadding;
            environmentSerializedObject.FindProperty("_catTowerCornerCenterRatio").floatValue = CatTowerCornerCenterRatio;
            environmentSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject randomSerializedObject = new SerializedObject(randomBlockManager);
            randomSerializedObject.FindProperty("_pieces").arraySize = pieces.Length;
            for (int i = 0; i < pieces.Length; i++)
                randomSerializedObject.FindProperty("_pieces").GetArrayElementAtIndex(i).objectReferenceValue = pieces[i];
            randomSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject gameSerializedObject = new SerializedObject(gameManager);
            gameSerializedObject.FindProperty("_blockField").objectReferenceValue = blockField;
            gameSerializedObject.FindProperty("_randomBlockManager").objectReferenceValue = randomBlockManager;
            gameSerializedObject.FindProperty("_mouse").objectReferenceValue = mouse;
            gameSerializedObject.FindProperty("_environmentView").objectReferenceValue = environmentView;
            gameSerializedObject.FindProperty("_pieces").arraySize = pieces.Length;
            for (int i = 0; i < pieces.Length; i++)
                gameSerializedObject.FindProperty("_pieces").GetArrayElementAtIndex(i).objectReferenceValue = pieces[i];
            gameSerializedObject.FindProperty("_scoreText").objectReferenceValue = scoreText;
            gameSerializedObject.FindProperty("_messageText").objectReferenceValue = messageText;
            gameSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            environmentView.Configure(blockField, mainCamera);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static BlockField CreateBoard(Transform parent, Sprite backgroundSprite, Sprite blockSprite)
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
                    backgroundRenderer.sprite = backgroundSprite;
                    backgroundRenderer.color = new Color(0.16f, 0.2f, 0.27f);
                    backgroundRenderer.sortingOrder = 0;

                    BoxCollider2D collider = fieldObject.AddComponent<BoxCollider2D>();
                    collider.isTrigger = true;

                    GameObject fillObject = new GameObject("Fill");
                    fillObject.transform.SetParent(fieldObject.transform);
                    fillObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                    fillObject.transform.localScale = Vector3.one * 0.82f;

                    SpriteRenderer fillRenderer = fillObject.AddComponent<SpriteRenderer>();
                    fillRenderer.sprite = blockSprite;
                    fillRenderer.enabled = false;
                    fillRenderer.sortingOrder = 1;

                    _Code.Field.Field field = fieldObject.AddComponent<_Code.Field.Field>();
                    field.Configure(new Vector2Int(x, y), backgroundRenderer, fillRenderer);
                }
            }

            blockField.Rebuild();
            return blockField;
        }

        private static MouseView CreateMouse(Transform parent, Sprite sprite, BlockField blockField)
        {
            GameObject mouseObject = new GameObject("Mouse");
            mouseObject.transform.SetParent(parent);
            mouseObject.transform.localScale = new Vector3(MouseScale, MouseScale, 1f);

            SpriteRenderer renderer = mouseObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.black;
            renderer.sortingOrder = 5;

            MouseView mouse = mouseObject.AddComponent<MouseView>();
            SerializedObject mouseSerializedObject = new SerializedObject(mouse);
            mouseSerializedObject.FindProperty("_cornerPadding").floatValue = MouseCornerPadding;
            mouseSerializedObject.FindProperty("_cushionCenterYOffset").floatValue = MouseCushionCenterYOffset;
            mouseSerializedObject.FindProperty("_renderer").objectReferenceValue = renderer;
            mouseSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            mouse.Initialize(blockField);
            return mouse;
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

        private static SpriteRenderer CreateSpriteRenderer(Transform parent, string name, Sprite sprite, int sortingOrder)
        {
            GameObject spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(parent);

            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = sortingOrder;

            return spriteRenderer;
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
            textMeshPro.color = new Color(0.23f, 0.16f, 0.11f);
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

        private static Sprite GetSprite(string assetPath, Sprite fallback)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            return sprite != null ? sprite : fallback;
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
