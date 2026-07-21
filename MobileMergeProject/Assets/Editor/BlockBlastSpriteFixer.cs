using _Code.Block;
using _Code.Field;
using _Code.Manager;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using FieldCell = _Code.Field.Field;
using MouseView = _Code.Mouse.Mouse;

namespace _Code.Editor
{
    [InitializeOnLoad]
    public static class BlockBlastSpriteFixer
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string RootName = "BlockBlastGame";
        private const string SquareSpritePath = "Assets/_Asset/_Image/BlockSquare.png";
        private const string CatBlockSpritePath = "Assets/Resources/BlockBlast/BlackCatBlockSprite.png";
        private const string MouseSpritePath = "Assets/Resources/BlockBlast/MouseSprite.png";
        private const string CleanCatHomeBackgroundSpritePath = "Assets/Resources/BlockBlast/CleanCatHomeBackground.png";
        private const string MouseHoleFrameSpritePath = "Assets/Resources/BlockBlast/MouseHoleFrameSprite.png";
        private const string BackgroundName = "CleanCatHomeBackground";
        private const string CatTowerFrameName = "CatTowerFrame";
        private const string TitleName = "TitleText";
        private const string GameTitle = "\uC950\uAD6C\uBA4D \uB9C8\uC6B0\uC2A4\uD321";
        private const float MouseCornerPadding = 0.78f;
        private const float MouseScale = 0.9f;
        private const float MouseCushionCenterYOffset = 0.3f;
        private const float CatTowerCornerCenterRatio = 0.35f;

        static BlockBlastSpriteFixer()
        {
            EditorApplication.delayCall += FixOpenSceneMissingSprites;
        }

        [MenuItem("Tools/Block Blast/Fix Missing Square Sprites")]
        public static void FixOpenSceneMissingSprites()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            GameObject root = GameObject.Find(RootName);
            if (root == null)
                return;

            Sprite squareSprite = GetSquareSprite();
            if (squareSprite == null)
                return;

            int changedCount = 0;
            changedCount += ApplyCharacterSprites(root);
            changedCount += ApplyEnvironmentSprites(root);
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer spriteRenderer in renderers)
            {
                if (spriteRenderer.sprite != null)
                    continue;

                spriteRenderer.sprite = squareSprite;
                changedCount++;
            }

            if (changedCount <= 0)
                return;

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"Updated {changedCount} Block Blast SpriteRenderers.");
        }

        [MenuItem("Tools/Block Blast/Apply Cat And Mouse Sprites")]
        public static void ApplyOpenSceneCharacterSprites()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            GameObject root = GameObject.Find(RootName);
            if (root == null)
                return;

            int changedCount = ApplyCharacterSprites(root);
            changedCount += ApplyEnvironmentSprites(root);

            if (changedCount <= 0)
                return;

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"Assigned cat and mouse sprites to {changedCount} Block Blast SpriteRenderers.");
        }

        [MenuItem("Tools/Block Blast/Apply Cat Home Environment")]
        public static void ApplyOpenSceneEnvironment()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            GameObject root = GameObject.Find(RootName);
            if (root == null)
                return;

            int changedCount = ApplyEnvironmentSprites(root);

            if (changedCount <= 0)
                return;

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"Assigned cat home environment sprites to {changedCount} Block Blast SpriteRenderers.");
        }

        public static void ApplySampleSceneEnvironment()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find(RootName);

            if (root == null)
                return;

            int changedCount = ApplyEnvironmentSprites(root);

            if (changedCount <= 0)
                return;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"Applied cat home environment to SampleScene with {changedCount} saved changes.");
        }

        private static int ApplyCharacterSprites(GameObject root)
        {
            Sprite catBlockSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CatBlockSpritePath);
            Sprite mouseSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MouseSpritePath);
            int changedCount = 0;

            if (catBlockSprite != null)
            {
                foreach (BlockCellView cellView in root.GetComponentsInChildren<BlockCellView>(true))
                    changedCount += AssignSprite(cellView.GetComponent<SpriteRenderer>(), catBlockSprite, Color.white);

                foreach (FieldCell field in root.GetComponentsInChildren<FieldCell>(true))
                {
                    Transform fill = field.transform.Find("Fill");

                    if (fill != null)
                        changedCount += AssignSprite(fill.GetComponent<SpriteRenderer>(), catBlockSprite, Color.white);
                }
            }

            if (mouseSprite == null)
                return changedCount;

            foreach (MouseView mouse in root.GetComponentsInChildren<MouseView>(true))
                changedCount += AssignSprite(mouse.GetComponent<SpriteRenderer>(), mouseSprite, Color.white);

            Transform mouseTransform = root.transform.Find("Mouse");
            if (mouseTransform != null)
                changedCount += AssignSprite(mouseTransform.GetComponent<SpriteRenderer>(), mouseSprite, Color.white);

            return changedCount;
        }

        private static int ApplyEnvironmentSprites(GameObject root)
        {
            int changedCount = 0;
            bool environmentCreated = false;
            BlockBlastEnvironmentView environmentView = EnsureEnvironmentView(root, ref environmentCreated);
            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CleanCatHomeBackgroundSpritePath);
            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MouseHoleFrameSpritePath);

            if (environmentCreated)
                changedCount++;

            if (backgroundSprite != null)
                changedCount += ConfigureBackground(root.transform, backgroundSprite);

            BlockField blockField = root.GetComponentInChildren<BlockField>(true);

            if (frameSprite != null)
                changedCount += ConfigureCatTowerFrame(root.transform, frameSprite, blockField);

            changedCount += ConfigureTitle(root.transform);
            changedCount += ConfigureStatusText(root.transform, "ScoreText", "Score 0", new Vector3(0f, 4.75f, 0f), 8f, 0.52f);
            changedCount += ConfigureStatusText(root.transform, "MessageText", string.Empty, new Vector3(0f, 4.18f, 0f), 7f, 0.42f);
            changedCount += ConfigureMouse(root, blockField);
            changedCount += AssignEnvironmentReferences(
                environmentView,
                FindRenderer(root.transform, BackgroundName),
                FindRenderer(root.transform, CatTowerFrameName),
                FindTitle(root.transform));
            changedCount += AssignGameManagerReferences(root, environmentView);
            changedCount += DisableLegacySceneVisuals(root);
            return changedCount;
        }

        private static BlockBlastEnvironmentView EnsureEnvironmentView(GameObject root, ref bool changed)
        {
            GameManager gameManager = root.GetComponentInChildren<GameManager>(true);
            GameObject targetObject = gameManager != null ? gameManager.gameObject : null;

            if (targetObject == null)
            {
                Transform managerTransform = root.transform.Find("BlockBlastManager");
                targetObject = managerTransform != null ? managerTransform.gameObject : root;
            }

            BlockBlastEnvironmentView environmentView = targetObject.GetComponent<BlockBlastEnvironmentView>();

            if (environmentView != null)
                return environmentView;

            changed = true;
            return targetObject.AddComponent<BlockBlastEnvironmentView>();
        }

        private static SpriteRenderer FindRenderer(Transform root, string objectName)
        {
            Transform child = root.Find(objectName);
            return child != null ? child.GetComponent<SpriteRenderer>() : null;
        }

        private static TextMeshPro FindTitle(Transform root)
        {
            Transform child = root.Find(TitleName);
            return child != null ? child.GetComponent<TextMeshPro>() : null;
        }

        private static TMP_Text FindText(Transform root, string objectName)
        {
            Transform child = root.Find(objectName);
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        private static int AssignEnvironmentReferences(
            BlockBlastEnvironmentView environmentView,
            SpriteRenderer backgroundRenderer,
            SpriteRenderer catTowerFrameRenderer,
            TextMeshPro titleText)
        {
            if (environmentView == null)
                return 0;

            SerializedObject serializedObject = new SerializedObject(environmentView);
            int changedCount = 0;

            changedCount += AssignObjectReference(serializedObject, "_backgroundRenderer", backgroundRenderer);
            changedCount += AssignObjectReference(serializedObject, "_catTowerFrameRenderer", catTowerFrameRenderer);
            changedCount += AssignObjectReference(serializedObject, "_titleText", titleText);
            changedCount += AssignFloat(serializedObject, "_mouseCornerPadding", MouseCornerPadding);
            changedCount += AssignFloat(serializedObject, "_catTowerCornerCenterRatio", CatTowerCornerCenterRatio);

            if (changedCount > 0)
                serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return changedCount;
        }

        private static int AssignGameManagerReferences(GameObject root, BlockBlastEnvironmentView environmentView)
        {
            GameManager gameManager = root.GetComponentInChildren<GameManager>(true);

            if (gameManager == null)
                return 0;

            SerializedObject serializedObject = new SerializedObject(gameManager);
            int changedCount = 0;

            changedCount += AssignObjectReference(serializedObject, "_environmentView", environmentView);
            changedCount += AssignObjectReference(serializedObject, "_mouse", root.GetComponentInChildren<MouseView>(true));
            changedCount += AssignObjectReference(serializedObject, "_scoreText", FindText(root.transform, "ScoreText"));
            changedCount += AssignObjectReference(serializedObject, "_messageText", FindText(root.transform, "MessageText"));

            if (changedCount > 0)
                serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return changedCount;
        }

        private static int ConfigureMouse(GameObject root, BlockField blockField)
        {
            Transform mouseTransform = root.transform.Find("Mouse");

            if (mouseTransform == null || blockField == null)
                return 0;

            int changedCount = 0;
            SpriteRenderer spriteRenderer = mouseTransform.GetComponent<SpriteRenderer>();
            MouseView mouse = mouseTransform.GetComponent<MouseView>();

            if (mouse == null)
            {
                mouse = mouseTransform.gameObject.AddComponent<MouseView>();
                changedCount++;
            }

            SerializedObject serializedObject = new SerializedObject(mouse);
            changedCount += AssignObjectReference(serializedObject, "_renderer", spriteRenderer);
            changedCount += AssignFloat(serializedObject, "_cornerPadding", MouseCornerPadding);
            changedCount += AssignFloat(serializedObject, "_cushionCenterYOffset", MouseCushionCenterYOffset);

            if (changedCount > 0)
                serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Vector3 targetScale = new Vector3(MouseScale, MouseScale, 1f);
            if (mouseTransform.localScale != targetScale)
            {
                mouseTransform.localScale = targetScale;
                changedCount++;
            }

            blockField.Rebuild();
            mouse.Initialize(blockField);
            return changedCount;
        }

        private static int ConfigureStatusText(Transform root, string name, string text, Vector3 position, float width, float fontSize)
        {
            bool changed = false;
            TextMeshPro statusText = EnsureText(root, name, ref changed);
            Color targetColor = new Color(0.23f, 0.16f, 0.11f);
            Vector2 targetSize = new Vector2(width, 1f);

            changed |= statusText.text != text ||
                       statusText.color != targetColor ||
                       statusText.transform.position != position ||
                       statusText.rectTransform.sizeDelta != targetSize ||
                       Mathf.Approximately(statusText.fontSize, fontSize) == false ||
                       statusText.alignment != TextAlignmentOptions.Center;

            statusText.text = text;
            statusText.color = targetColor;
            statusText.transform.position = position;
            statusText.rectTransform.sizeDelta = targetSize;
            statusText.fontSize = fontSize;
            statusText.alignment = TextAlignmentOptions.Center;

            return changed ? 1 : 0;
        }

        private static int DisableLegacySceneVisuals(GameObject blockBlastRoot)
        {
            string[] legacyNames = { "Map", "Manager", "spawningObj", "Square", "Square (1)", "Square (2)", "Square (3)", "GameObject" };
            int changedCount = 0;
            Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Transform target in transforms)
            {
                if (target == blockBlastRoot.transform || !Contains(legacyNames, target.name) || !target.gameObject.activeSelf)
                    continue;

                target.gameObject.SetActive(false);
                changedCount++;
            }

            return changedCount;
        }

        private static int AssignObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
        {
            if (value == null)
                return 0;

            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || property.objectReferenceValue == value)
                return 0;

            property.objectReferenceValue = value;
            return 1;
        }

        private static int AssignFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || Mathf.Approximately(property.floatValue, value))
                return 0;

            property.floatValue = value;
            return 1;
        }

        private static bool Contains(string[] values, string target)
        {
            foreach (string value in values)
            {
                if (value == target)
                    return true;
            }

            return false;
        }

        private static int ConfigureTitle(Transform root)
        {
            bool changed = false;
            TextMeshPro titleText = EnsureTitleText(root, ref changed);
            string targetText = GameTitle;
            Color targetColor = new Color(0.42f, 0.23f, 0.11f);
            Vector3 targetPosition = new Vector3(0f, 5.35f, 0f);
            Vector2 targetSize = new Vector2(8.5f, 1f);
            float targetFontSize = 0.66f;

            changed |= titleText.text != targetText ||
                       titleText.color != targetColor ||
                       titleText.transform.position != targetPosition ||
                       titleText.rectTransform.sizeDelta != targetSize ||
                       Mathf.Approximately(titleText.fontSize, targetFontSize) == false ||
                       titleText.alignment != TextAlignmentOptions.Center ||
                       titleText.fontStyle != FontStyles.Bold;

            titleText.text = targetText;
            titleText.color = targetColor;
            titleText.transform.position = targetPosition;
            titleText.rectTransform.sizeDelta = targetSize;
            titleText.fontSize = targetFontSize;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            return changed ? 1 : 0;
        }

        private static int ConfigureBackground(Transform root, Sprite sprite)
        {
            bool changed = false;
            SpriteRenderer renderer = EnsureRenderer(root, BackgroundName, ref changed);
            Camera mainCamera = Camera.main;
            Vector3 targetPosition = mainCamera != null ? new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 0f) : Vector3.zero;
            Vector3 targetScale = Vector3.one;

            if (mainCamera != null && mainCamera.orthographic)
            {
                float cameraHeight = mainCamera.orthographicSize * 2f;
                float cameraWidth = cameraHeight * mainCamera.aspect;
                Vector2 spriteSize = sprite.bounds.size;
                float scale = Mathf.Max(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y);
                targetScale = Vector3.one * scale;
            }

            changed |= ApplyRendererState(renderer, sprite, Color.white, -30, targetPosition, targetScale);
            return changed ? 1 : 0;
        }

        private static int ConfigureCatTowerFrame(Transform root, Sprite sprite, BlockField blockField)
        {
            if (blockField == null)
                return 0;

            blockField.Rebuild();

            bool changed = false;
            SpriteRenderer renderer = EnsureRenderer(root, CatTowerFrameName, ref changed);
            Vector3 bottomLeft = blockField.GetWorldPosition(Vector2Int.zero);
            Vector3 topRight = blockField.GetWorldPosition(new Vector2Int(blockField.Width - 1, blockField.Height - 1));
            Vector3 center = (bottomLeft + topRight) * 0.5f;
            float mouseHalfWidth = Mathf.Abs(topRight.x - bottomLeft.x) * 0.5f + MouseCornerPadding;
            float mouseHalfHeight = Mathf.Abs(topRight.y - bottomLeft.y) * 0.5f + MouseCornerPadding;
            float targetWidth = mouseHalfWidth / CatTowerCornerCenterRatio;
            float targetHeight = mouseHalfHeight / CatTowerCornerCenterRatio;
            Vector2 spriteSize = sprite.bounds.size;
            Vector3 scale = new Vector3(targetWidth / spriteSize.x, targetHeight / spriteSize.y, 1f);

            changed |= ApplyRendererState(renderer, sprite, Color.white, -2, new Vector3(center.x, center.y, 0f), scale);
            return changed ? 1 : 0;
        }

        private static SpriteRenderer EnsureRenderer(Transform root, string objectName, ref bool changed)
        {
            Transform child = root.Find(objectName);

            if (child == null)
            {
                GameObject visualObject = new GameObject(objectName);
                visualObject.transform.SetParent(root);
                child = visualObject.transform;
                changed = true;
            }

            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();

            if (renderer != null)
                return renderer;

            changed = true;
            return child.gameObject.AddComponent<SpriteRenderer>();
        }

        private static TextMeshPro EnsureTitleText(Transform root, ref bool changed)
        {
            return EnsureText(root, TitleName, ref changed);
        }

        private static TextMeshPro EnsureText(Transform root, string objectName, ref bool changed)
        {
            Transform child = root.Find(objectName);

            if (child == null)
            {
                GameObject titleObject = new GameObject(objectName);
                titleObject.transform.SetParent(root);
                child = titleObject.transform;
                changed = true;
            }

            TextMeshPro titleText = child.GetComponent<TextMeshPro>();

            if (titleText != null)
                return titleText;

            changed = true;
            return child.gameObject.AddComponent<TextMeshPro>();
        }

        private static bool ApplyRendererState(SpriteRenderer renderer, Sprite sprite, Color color, int sortingOrder, Vector3 position, Vector3 scale)
        {
            bool changed = renderer.sprite != sprite ||
                           renderer.color != color ||
                           renderer.sortingOrder != sortingOrder ||
                           renderer.transform.position != position ||
                           renderer.transform.localScale != scale;

            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            renderer.transform.position = position;
            renderer.transform.localScale = scale;
            return changed;
        }

        private static int AssignSprite(SpriteRenderer spriteRenderer, Sprite sprite, Color color)
        {
            if (spriteRenderer == null || sprite == null)
                return 0;

            bool changed = spriteRenderer.sprite != sprite || spriteRenderer.color != color;
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            return changed ? 1 : 0;
        }

        private static Sprite GetSquareSprite()
        {
            EnsureSquareSpriteImported();
            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);

            if (squareSprite != null)
                return squareSprite;

            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static void EnsureSquareSpriteImported()
        {
            TextureImporter importer = AssetImporter.GetAtPath(SquareSpritePath) as TextureImporter;

            if (importer == null)
                return;

            bool isDirty = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                isDirty = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                isDirty = true;
            }

            if (Mathf.Approximately(importer.spritePixelsPerUnit, 1f) == false)
            {
                importer.spritePixelsPerUnit = 1f;
                isDirty = true;
            }

            if (isDirty)
                importer.SaveAndReimport();
        }
    }
}
