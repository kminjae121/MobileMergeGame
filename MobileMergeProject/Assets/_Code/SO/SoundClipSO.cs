using UnityEngine;

namespace _Code.SO
{
    [CreateAssetMenu(fileName = "Sound/SoundClip", menuName = "Sound/SoundClip", order = 0)]
    public class SoundClipSO : ScriptableObject
    {
        public AudioClip Clip;

        public string AudioName;

        public bool IsLooping;

        [Range(0, 1)] public float Volume;
    }
}