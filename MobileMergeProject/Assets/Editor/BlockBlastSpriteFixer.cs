using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Code.Editor
{
    [InitializeOnLoad]
    public static class BlockBlastSpriteFixer
    {
        private const string RootName = "BlockBlastGame";
        private const string SquareSpritePath = "Assets/_Asset/_Image/BlockSquare.png";

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
            Debug.Log($"Assigned square sprites to {changedCount} Block Blast SpriteRenderers.");
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
