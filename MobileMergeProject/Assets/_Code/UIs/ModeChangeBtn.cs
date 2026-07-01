using System;
using _Code.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayMode = _Code.Manager.PlayMode;

namespace _Code.UIs
{
    public class ModeChangeBtn : MonoBehaviour
    {
        [SerializeField] private Button changeBtn;
        [SerializeField] private TextMeshProUGUI changeBtnText;

        private void Awake()
        {
            changeBtn.onClick.AddListener(HandleChangeGameMode);
        }

        private void Start()
        {
            if (GameManager.Instance.PlayMode == PlayMode.Spawning)
            {
                changeBtnText.text = "Spawn";
            }
            else
            {
                changeBtnText.text = "Move";
            }
        }


        private void OnDestroy()
        {
            changeBtn.onClick.RemoveListener(HandleChangeGameMode);
        }
        private void HandleChangeGameMode()
        {
            if (GameManager.Instance.PlayMode == PlayMode.Spawning)
            {
                changeBtnText.text = "Move";
                GameManager.Instance.SetPlayMode(PlayMode.Moving);
            }
            else
            {
                changeBtnText.text = "Spawn";
                GameManager.Instance.SetPlayMode(PlayMode.Spawning);
            }
        }
    }
}