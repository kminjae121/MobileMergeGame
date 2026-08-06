using System.Collections.Generic;
using _Code.Effects;
using Code.Core.Events.Bus;
using UnityEngine;

namespace _Code.Manager
{
    public sealed class LineClearEffectPlayer : MonoBehaviour
    {
        [SerializeField] private HapticFeedback hapticFeedback;
        [SerializeField] private LineClearPawParticleEffect lineClearParticleEffect;

        public void Configure(HapticFeedback hapticFeedback, LineClearPawParticleEffect lineClearParticleEffect)
        {
            if (this.hapticFeedback == null)
                this.hapticFeedback = hapticFeedback;

            if (this.lineClearParticleEffect == null)
                this.lineClearParticleEffect = lineClearParticleEffect;
        }

        public void Play(int clearedLines, IReadOnlyList<Vector3> clearedBlockPositions)
        {
            if (clearedLines <= 0)
                return;

            Bus<CamShakeEvent>.Raise(new CamShakeEvent(0.2f));

            if (lineClearParticleEffect != null)
                lineClearParticleEffect.PlayAtPositions(clearedBlockPositions, clearedLines);

            if (hapticFeedback != null)
                hapticFeedback.PlayLineClear();
        }
    }
}
