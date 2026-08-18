using _Code.Server;
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
            UpdateButtonState();
        }

        private void OnEnable()
        {
            UpdateButtonState();
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(StartStage);
        }

        private void StartStage()
        {
            if (!PlayerIdProvider.CanPlay)
            {
                Debug.LogWarning("로그인 전에는 게임을 시작할 수 없습니다.");
                UpdateButtonState();
                return;
            }

            StageRunContext.SelectStage(stageNumber);
            SceneManager.LoadScene(gameSceneName);
        }

        private void UpdateButtonState()
        {
            if (_button != null)
                _button.interactable = PlayerIdProvider.CanPlay;
        }
    }
}
