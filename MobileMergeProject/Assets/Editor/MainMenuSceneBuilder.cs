using System.Collections.Generic;
using _Code.Menu;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Code.Editor
{
    public static class MainMenuSceneBuilder
    {
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";
        private const string GameScenePath = "Assets/Scenes/SampleScene.unity";
        private const string RootName = "MainMenu";
        private const float TargetAspect = 9f / 16f;

        private const string BackgroundSpritePath = "Assets/Resources/BlockBlast/CleanCatHomeBackground.png";
        private const string CatTowerSpritePath = "Assets/Resources/BlockBlast/CatTowerFrameSprite.png";
        private const string MouseSpritePath = "Assets/Resources/BlockBlast/MouseSprite.png";
        private const string BlackCatSpritePath = "Assets/Resources/BlockBlast/BlackCatBlockSprite.png";
        private const string WhiteCatSpritePath = "Assets/Resources/BlockBlast/WhiteCatBlockSprite.png";
        private const string SphynxCatSpritePath = "Assets/Resources/BlockBlast/SphynxCatBlockSprite.png";

        [MenuItem("Tools/Block Blast/Rebuild Main Menu Scene")]
        public static void RebuildMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject root = new GameObject(RootName);
            Camera mainCamera = CreateCamera();
            Sprite defaultSprite = GetDefaultSprite();

            SpriteRenderer backgroundRenderer = CreateSpriteRenderer(
                root.transform,
                "CleanCatHomeBackground",
                GetSprite(BackgroundSpritePath, defaultSprite),
                -30,
                Vector3.zero);
            ScaleSpriteToCamera(backgroundRenderer, mainCamera);

            SpriteRenderer catTowerRenderer = CreateSpriteRenderer(
                root.transform,
                "CatTowerPreview",
                GetSprite(CatTowerSpritePath, defaultSprite),
                -2,
                new Vector3(0f, 0.1f, 0f));
            ScaleSpriteToHeight(catTowerRenderer, 4.95f);

            CreateTitle(root.transform);
            CreateCatDecoration(root.transform);
            GameObject buttonObject = CreateGameStartButton(root.transform, defaultSprite);
            MainMenuController controller = CreateController(root.transform);

            SerializedObject controllerSerializedObject = new SerializedObject(controller);
            controllerSerializedObject.FindProperty("_gameSceneName").stringValue = "SampleScene";
            controllerSerializedObject.FindProperty("_mainCamera").objectReferenceValue = mainCamera;
            controllerSerializedObject.FindProperty("_gameStartButtonCollider").objectReferenceValue = buttonObject.GetComponent<Collider2D>();
            controllerSerializedObject.FindProperty("_gameStartButtonRenderer").objectReferenceValue = buttonObject.GetComponent<SpriteRenderer>();
            controllerSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MainScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.98f, 0.96f, 0.92f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            cameraObject.AddComponent<AudioListener>();

            return camera;
        }

        private static void CreateTitle(Transform parent)
        {
            TextMeshPro titleText = CreateText(parent, "TitleText", "냥타워 마우스팡", new Vector3(0f, 4.38f, 0f), 6.7f, 0.78f, 20);
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(0.42f, 0.23f, 0.11f);

            SpriteRenderer mouse = CreateSpriteRenderer(
                parent,
                "MouseMascot",
                GetSprite(MouseSpritePath, GetDefaultSprite()),
                12,
                new Vector3(0f, 3.55f, 0f));
            mouse.transform.localScale = Vector3.one * 1.25f;
        }

        private static void CreateCatDecoration(Transform parent)
        {
            Sprite fallback = GetDefaultSprite();

            CreateDecorationSprite(parent, "BlackCatMascot", GetSprite(BlackCatSpritePath, fallback), new Vector3(-1.35f, -2.55f, 0f), 0.82f, 9);
            CreateDecorationSprite(parent, "WhiteCatMascot", GetSprite(WhiteCatSpritePath, fallback), new Vector3(0f, -2.35f, 0f), 0.82f, 10);
            CreateDecorationSprite(parent, "SphynxCatMascot", GetSprite(SphynxCatSpritePath, fallback), new Vector3(1.35f, -2.55f, 0f), 0.82f, 9);
        }

        private static void CreateDecorationSprite(Transform parent, string name, Sprite sprite, Vector3 position, float scale, int sortingOrder)
        {
            SpriteRenderer renderer = CreateSpriteRenderer(parent, name, sprite, sortingOrder, position);
            renderer.transform.localScale = Vector3.one * scale;
        }

        private static GameObject CreateGameStartButton(Transform parent, Sprite fallbackSprite)
        {
            GameObject shadowObject = new GameObject("GameStartButtonShadow");
            shadowObject.transform.SetParent(parent);
            shadowObject.transform.position = new Vector3(0f, -3.98f, 0f);
            shadowObject.transform.localScale = new Vector3(4.18f, 1.02f, 1f);

            SpriteRenderer shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = fallbackSprite;
            shadowRenderer.color = new Color(0.36f, 0.2f, 0.1f, 0.28f);
            shadowRenderer.sortingOrder = 3;

            GameObject buttonObject = new GameObject("GameStartButton");
            buttonObject.transform.SetParent(parent);
            buttonObject.transform.position = new Vector3(0f, -3.88f, 0f);
            buttonObject.transform.localScale = new Vector3(4.05f, 0.95f, 1f);

            SpriteRenderer buttonRenderer = buttonObject.AddComponent<SpriteRenderer>();
            buttonRenderer.sprite = fallbackSprite;
            buttonRenderer.color = new Color(0.94f, 0.58f, 0.28f);
            buttonRenderer.sortingOrder = 4;

            BoxCollider2D collider = buttonObject.AddComponent<BoxCollider2D>();

            if (fallbackSprite != null)
                collider.size = fallbackSprite.bounds.size;

            TextMeshPro label = CreateText(parent, "GameStartLabel", "GameStart", new Vector3(0f, -3.86f, 0f), 4f, 0.48f, 11);
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;

            return buttonObject;
        }

        private static MainMenuController CreateController(Transform parent)
        {
            GameObject controllerObject = new GameObject("MainMenuController");
            controllerObject.transform.SetParent(parent);

            MainMenuController controller = controllerObject.AddComponent<MainMenuController>();
            controllerObject.transform.position = Vector3.zero;

            return controller;
        }

        private static SpriteRenderer CreateSpriteRenderer(Transform parent, string name, Sprite sprite, int sortingOrder, Vector3 position)
        {
            GameObject spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(parent);
            spriteObject.transform.position = position;

            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = sortingOrder;

            return spriteRenderer;
        }

        private static TextMeshPro CreateText(Transform parent, string name, string text, Vector3 position, float width, float fontSize, int sortingOrder)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent);
            textObject.transform.position = position;

            TextMeshPro textMeshPro = textObject.AddComponent<TextMeshPro>();
            textMeshPro.text = text;
            textMeshPro.fontSize = fontSize;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.rectTransform.sizeDelta = new Vector2(width, 1f);
            textMeshPro.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMeshPro;
        }

        private static void ScaleSpriteToCamera(SpriteRenderer renderer, Camera targetCamera)
        {
            if (renderer == null || renderer.sprite == null || targetCamera == null)
                return;

            float cameraHeight = targetCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * TargetAspect;
            Vector2 spriteSize = renderer.sprite.bounds.size;
            float scale = Mathf.Max(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y);
            renderer.transform.localScale = Vector3.one * scale;
        }

        private static void ScaleSpriteToHeight(SpriteRenderer renderer, float targetHeight)
        {
            if (renderer == null || renderer.sprite == null)
                return;

            float scale = targetHeight / renderer.sprite.bounds.size.y;
            renderer.transform.localScale = Vector3.one * scale;
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

        private static void EnsureBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(MainScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true)
            };

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == MainScenePath || scene.path == GameScenePath)
                    continue;

                scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
