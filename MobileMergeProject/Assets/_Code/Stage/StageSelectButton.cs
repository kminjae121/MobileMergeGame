using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code.Stage
{
    [RequireComponent(typeof(Button))]
    public sealed class StageSelectButton : MonoBehaviour
    {
        [SerializeField, Range(1, 10)] private int stageNumber = 1;
        [SerializeField] private string gameSceneName = "GameScene";

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(StartStage);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(StartStage);
        }

        private void StartStage()
        {
            StageRunContext.SelectStage(stageNumber);
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
