using UnityEngine;

namespace _Code.Stage
{
    public static class StageProgress
    {
        private const string ClearedKeyPrefix = "CatchTheCats.StageCleared.";

        public static bool IsCleared(int stageNumber)
        {
            return PlayerPrefs.GetInt(GetClearedKey(stageNumber), 0) == 1;
        }

        public static void MarkCleared(int stageNumber)
        {
            if (stageNumber < 1)
                return;

            PlayerPrefs.SetInt(GetClearedKey(stageNumber), 1);
            PlayerPrefs.Save();
        }

        private static string GetClearedKey(int stageNumber)
        {
            return $"{ClearedKeyPrefix}{stageNumber}";
        }
    }
}
