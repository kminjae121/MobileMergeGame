using System;
using System.IO;
using UnityEngine;

namespace _Code.Manager
{
    public sealed class JsonManager : MonoBehaviour
    {
        [SerializeField] private string _fileName = "score-data.json";

        private ScoreSaveData _saveData = new ScoreSaveData();

        public int MaxScore => _saveData.MaxScore;
        public string SavePath => Path.Combine(Application.persistentDataPath, _fileName);

        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            if (!File.Exists(SavePath))
            {
                _saveData = new ScoreSaveData();
                Save();
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                ScoreSaveData loadedData = JsonUtility.FromJson<ScoreSaveData>(json);
                _saveData = loadedData ?? new ScoreSaveData();
                _saveData.MaxScore = Mathf.Max(0, _saveData.MaxScore);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load score data. A new save file will be created. {exception.Message}");
                _saveData = new ScoreSaveData();
                Save();
            }
        }

        public bool TrySaveMaxScore(int score)
        {
            if (score <= _saveData.MaxScore)
                return false;

            _saveData.MaxScore = score;
            Save();
            return true;
        }

        public void SetMaxScore(int score)
        {
            _saveData.MaxScore = Mathf.Max(0, score);
            Save();
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(SavePath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(_saveData, true);
            File.WriteAllText(SavePath, json);
        }

        [Serializable]
        private sealed class ScoreSaveData
        {
            public int MaxScore;
        }
    }
}
