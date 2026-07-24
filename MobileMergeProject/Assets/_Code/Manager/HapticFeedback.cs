using UnityEngine;

namespace _Code.Manager
{
    public sealed class HapticFeedback : MonoBehaviour
    {
        [SerializeField] private bool _isEnabled = true;
        [SerializeField] private bool _vibrateInEditor;

        public void PlayLineClear()
        {
            if (!_isEnabled)
                return;

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#elif UNITY_EDITOR
            if (_vibrateInEditor)
                Handheld.Vibrate();
#endif
        }
    }
}
