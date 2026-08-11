using System.Collections.Generic;
using UnityEngine;

namespace _Code.Block
{
    public static class BlockBlastSpriteLibrary
    {
        private const string CatBlockSpritePath = "BlockBlast/CatBlockSprite";
        private const string BlackCatBlockSpritePath = "BlockBlast/BlackCatBlockSprite";
        private const string WhiteCatBlockSpritePath = "BlockBlast/WhiteCatBlockSprite";
        private const string SphynxCatBlockSpritePath = "BlockBlast/SphynxCatBlockSprite";
        private const string MouseSpritePath = "BlockBlast/MouseSprite";
        private const string CleanCatHomeBackgroundSpritePath = "BlockBlast/CleanCatHomeBackground";
        private const string NightCatHomeBackgroundSpritePath = "BlockBlast/NightCatHomeBackground";
        private const string CatTowerFrameSpritePath = "BlockBlast/CatTowerFrameSprite";
        private const string MouseHoleFrameSpritePath = "BlockBlast/MouseHoleFrameSprite";

        private static Sprite _catBlockSprite;
        private static Sprite _blackCatBlockSprite;
        private static Sprite _whiteCatBlockSprite;
        private static Sprite _sphynxCatBlockSprite;
        private static Sprite[] _catBlockSprites;
        private static Sprite _mouseSprite;
        private static Sprite _cleanCatHomeBackgroundSprite;
        private static Sprite _nightCatHomeBackgroundSprite;
        private static Sprite _catTowerFrameSprite;
        private static Sprite _mouseHoleFrameSprite;

        public static Sprite CatBlockSprite => CatBlockSprites.Length > 0 ? CatBlockSprites[0] : LoadSprite(CatBlockSpritePath, ref _catBlockSprite);
        public static Sprite[] CatBlockSprites => GetCatBlockSprites();
        public static Sprite MouseSprite => LoadSprite(MouseSpritePath, ref _mouseSprite);
        public static Sprite CleanCatHomeBackgroundSprite => LoadSprite(CleanCatHomeBackgroundSpritePath, ref _cleanCatHomeBackgroundSprite);
        public static Sprite NightCatHomeBackgroundSprite => LoadSprite(NightCatHomeBackgroundSpritePath, ref _nightCatHomeBackgroundSprite) ?? CleanCatHomeBackgroundSprite;
        public static Sprite CatTowerFrameSprite => LoadSprite(CatTowerFrameSpritePath, ref _catTowerFrameSprite);
        public static Sprite MouseHoleFrameSprite => LoadSprite(MouseHoleFrameSpritePath, ref _mouseHoleFrameSprite) ?? CatTowerFrameSprite;

        public static Sprite GetRandomCatBlockSprite()
        {
            Sprite[] sprites = CatBlockSprites;
            return sprites.Length > 0 ? sprites[Random.Range(0, sprites.Length)] : null;
        }

        private static Sprite[] GetCatBlockSprites()
        {
            if (_catBlockSprites != null)
                return _catBlockSprites;

            List<Sprite> sprites = new List<Sprite>
            {
                LoadSprite(BlackCatBlockSpritePath, ref _blackCatBlockSprite),
                LoadSprite(WhiteCatBlockSpritePath, ref _whiteCatBlockSprite),
                LoadSprite(SphynxCatBlockSpritePath, ref _sphynxCatBlockSprite)
            };

            sprites.RemoveAll(sprite => sprite == null);

            if (sprites.Count == 0)
            {
                Sprite fallback = LoadSprite(CatBlockSpritePath, ref _catBlockSprite);

                if (fallback != null)
                    sprites.Add(fallback);
            }

            _catBlockSprites = sprites.ToArray();
            return _catBlockSprites;
        }

        private static Sprite LoadSprite(string path, ref Sprite sprite)
        {
            if (sprite == null)
                sprite = Resources.Load<Sprite>(path);

            return sprite;
        }
    }
}
