using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.Core.Managers
{
    public static class TMPTween
    {
        public static Tweener DoText(this TextMeshProUGUI thisTmp, string text, float duration)
        {
            int length = 0;
            
            return DOTween.To(
                () => length,
                x =>
                {
                    length = x;
                    thisTmp.text = text.Substring(0, length);
                },
                text.Length,
                duration
            ).SetEase(Ease.Linear);
        }

        public static Tween RemoveText(this TextMeshProUGUI thisTmp, float duration)
        {
            string originalText = thisTmp.text;

            return DOTween.To(
                () => originalText.Length,
                x =>
                {
                    x = Mathf.Clamp(x, 0, originalText.Length);

                    thisTmp.text = x == 0
                        ? string.Empty
                        : originalText.Substring(0, x);
                },
                0,
                duration
            ).SetEase(Ease.Linear);
        }
    }
}