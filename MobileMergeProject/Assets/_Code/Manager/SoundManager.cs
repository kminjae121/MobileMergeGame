 using System.Collections;
using System.Collections.Generic;
using _Code.SO;
using UnityEngine;

namespace Code.Core
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        [SerializeField] private List<SoundClipSO> clips;

        [SerializeField] private AudioSource audioSourcePrefab;
        [SerializeField] private int sourceCnt = 10;

        private readonly Stack<AudioSource> audioSources = new Stack<AudioSource>();
        private readonly List<AudioSource> _usingSources = new List<AudioSource>();

        private readonly Dictionary<string, SoundClipSO> _clipDictionary = new Dictionary<string, SoundClipSO>();
        private readonly Dictionary<string, SoundClipSO> _loopingClipDictionary = new Dictionary<string, SoundClipSO>();

        protected override void Awake()
        {
            base.Awake();

            if (audioSourcePrefab == null)
            {
                Debug.LogError("[SoundManager] AudioSource Prefab이 비어있습니다.");
                return;
            }

            foreach (var audio in clips)
            {
                if (audio == null || audio.Clip == null || string.IsNullOrEmpty(audio.AudioName))
                    continue;

                if (audio.IsLooping)
                {
                    if (!_loopingClipDictionary.ContainsKey(audio.AudioName))
                        _loopingClipDictionary.Add(audio.AudioName, audio);
                }
                else
                {
                    if (!_clipDictionary.ContainsKey(audio.AudioName))
                        _clipDictionary.Add(audio.AudioName, audio);
                }
            }

            for (int i = 0; i < sourceCnt; ++i)
            {
                AudioSource src = Instantiate(audioSourcePrefab, transform);
                ResetSource(src);
                src.gameObject.SetActive(false);
                audioSources.Push(src);
            }
        }

        public void PlayClip(string name)
        {
            if (!_clipDictionary.TryGetValue(name, out var clip))
            {
                Debug.LogWarning($"[SoundManager] 효과음 '{name}'을 찾을 수 없습니다.");
                return;
            }

            AudioSource source = GetSource();
            if (source == null) return;

            source.clip = clip.Clip;
            source.loop = false;
            source.volume = clip.Volume;
            source.gameObject.SetActive(true);
            source.Play();

            _usingSources.Add(source);
            StartCoroutine(ReturnSource(source));
        }

        public void PlayLooping(string name)
        {
            if (!_loopingClipDictionary.TryGetValue(name, out var clip))
            {
                Debug.LogWarning($"[SoundManager] 루프 사운드 '{name}'을 찾을 수 없습니다.");
                return;
            }

            if (IsLoopingAlreadyPlaying(clip.Clip))
                return;

            AudioSource source = GetSource();
            if (source == null) return;

            source.clip = clip.Clip;
            source.loop = true;
            source.volume = clip.Volume;
            source.gameObject.SetActive(true);
            source.Play();

            _usingSources.Add(source);
        }

        public void PlayBGMSound(string name)
        {
            StopAllLooping();
            PlayLooping(name);
        }

        public void StopAllLooping()
        {
            for (int i = _usingSources.Count - 1; i >= 0; --i)
            {
                AudioSource source = _usingSources[i];

                if (source == null)
                {
                    _usingSources.RemoveAt(i);
                    continue;
                }

                if (!source.loop)
                    continue;

                ReleaseSource(source);
            }
        }

        public void StopLooping(string name)
        {
            if (!_loopingClipDictionary.TryGetValue(name, out var clip))
                return;

            StopSourceByClip(clip.Clip, true);
        }
        
        public void StopClip(string name)
        {
            if (!_clipDictionary.TryGetValue(name, out var clip))
                return;

            StopSourceByClip(clip.Clip, false);
        }

        private IEnumerator ReturnSource(AudioSource source)
        {
            if (source == null || source.loop)
                yield break;

            while (source != null && source.gameObject.activeSelf && source.isPlaying)
                yield return null;

            if (source == null)
                yield break;

            if (!_usingSources.Contains(source))
                yield break;

            ReleaseSource(source);
        }

        private AudioSource GetSource()
        {
            if (audioSources.Count == 0)
            {
                Debug.LogWarning("[SoundManager] 사용 가능한 AudioSource가 없습니다. sourceCnt를 늘려주세요.");
                return null;
            }

            return audioSources.Pop();
        }

        private void StopSourceByClip(AudioClip clip, bool isLooping)
        {
            for (int i = _usingSources.Count - 1; i >= 0; --i)
            {
                AudioSource source = _usingSources[i];

                if (source == null)
                {
                    _usingSources.RemoveAt(i);
                    continue;
                }

                if (source.loop != isLooping)
                    continue;

                if (source.clip != clip)
                    continue;

                ReleaseSource(source);
            }
        }

        private void ReleaseSource(AudioSource source)
        {
            if (source == null)
                return;

            source.Stop();
            ResetSource(source);
            source.gameObject.SetActive(false);

            _usingSources.Remove(source);

            if (!audioSources.Contains(source))
                audioSources.Push(source);
        }

        private static void ResetSource(AudioSource source)
        {
            source.clip = null;
            source.loop = false;
            source.playOnAwake = false;
        }

        private bool IsLoopingAlreadyPlaying(AudioClip clip)
        {
            foreach (var source in _usingSources)
            {
                if (source == null)
                    continue;

                if (source.loop && source.clip == clip && source.isPlaying)
                    return true;
            }

            return false;
        }
    }
}