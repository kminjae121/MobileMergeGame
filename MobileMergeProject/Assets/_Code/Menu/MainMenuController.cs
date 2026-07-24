using System;
using _Code.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code.Menu
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private Button startBtn;
        [SerializeField] private JsonManager jsonManager;
        [SerializeField] private TextMeshProUGUI maxScoreTxt;
        private void Awake()
        {
            startBtn.onClick.AddListener(StartGame);
            jsonManager.Load();
            maxScoreTxt.text = $"최대점수 : {jsonManager.MaxScore}";
        }

        private void OnDestroy()
        {
            startBtn.onClick.RemoveAllListeners();
        }

        public void StartGame()
        {
            if (string.IsNullOrEmpty(gameSceneName) == false)
            {
                SceneManager.LoadScene(gameSceneName);
                return;
            }

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}
