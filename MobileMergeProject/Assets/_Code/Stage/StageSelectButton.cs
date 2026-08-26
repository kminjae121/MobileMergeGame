using _Code.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Code.Stage
{
    [RequireComponent(typeof(Button))]
    public sealed class StageSelectButton : MonoBehaviour
    {
        [SerializeField, Range(1, 10)] private int stageNumber = 1;
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Color normalButtonColor = Color.white;
        [SerializeField] private Color clearedButtonColor = new Color(1f, 0.88f, 0.38f, 1f);
        [SerializeField] private Color normalTextColor = new Color(0.24f, 0.17f, 0.12f, 1f);
        [SerializeField] private Color clearedTextColor = new Color(0.31f, 0.16f, 0.08f, 1f);

        private Button _button;
        private Image _buttonImage;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();

            if (labelText == null)
                labelText = GetComponentInChildren<TextMeshProUGUI>(true);

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
            SceneManager.LoadScene(StageCatalog.GetStageSceneName(stageNumber));
        }

        private void UpdateButtonState()
        {
            if (_button != null)
                _button.interactable = PlayerIdProvider.CanPlay;

            bool isCleared = StageProgress.IsCleared(stageNumber);

            if (_buttonImage != null)
                _buttonImage.color = isCleared ? clearedButtonColor : normalButtonColor;

            if (labelText == null)
                return;

            labelText.color = isCleared ? clearedTextColor : normalTextColor;
            labelText.text = isCleared
                ? $"{stageNumber}\n<size=45%>CLEAR</size>"
                : stageNumber.ToString();
        }
    }
}
