using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityReusables.ScriptableObjects.Variables;
using UnityReusables.Utils.Extensions;
using Random = UnityEngine.Random;

namespace UnityReusables.Managers.Audio_Manager
{
    public class AudioManager : SingletonMB<AudioManager>
    {
        public AudioMixerGroup mixerGroup;
        [Title("Sounds")]
        [Range(0.0f, 1.0f)] public float soundVolume = 1.0f;
        public Sound[] sounds;
        
        [Title("Musics")]
        [Range(0.0f, 1.0f)] public float musicVolume = 1.0f;
        public Sound[] musics;
        public bool musicAutoPlayStart;
        public bool musicAutoPlayRandomClip;
        public bool musicAutoPlayNext;
        public float musicFadeOutDuration;
        
        private AudioSource _currentMusic;
        [SerializeField]
        private BoolVariable isAudio;

        protected override void OnAwake()
        {
            InitSoundArray(sounds);
            InitSoundArray(musics);
            AutoPlayMusic();
        }

        private void OnEnable()
        {
            if (isAudio != null) isAudio.AddOnChangeCallback(SetAudio);
        }

        private void OnDisable()
        {
            if (isAudio != null) isAudio.RemoveOnChangeCallback(SetAudio);
        }

        private void AutoPlayMusic()
        {
            if (musicAutoPlayStart && musics.Length > 0)
                PlayMusic(musics[0].name, musicAutoPlayRandomClip ? Random.Range(0, musics[0].clips.Count) : 0);
        }

        [Title("Tests")]
        [Button("Play Music")]
        private void PlayMusic(string name, int clipId = 0)
        {
            if (!isAudio.v) return;
            if (_currentMusic && _currentMusic.isPlaying)
            {
                StartCoroutine(MusicFadeOut(name));
                return;
            }

            var m = Array.Find(musics, item => item.name == name);
            if (m == null)
            {
                Debug.LogWarning($"Play music: {name} not found!");
                return;
            }

            var clip = m.clips[clipId];
            _currentMusic = m.source;
            _currentMusic.clip = clip;
            _currentMusic.volume = m.volume * musicVolume;
            _currentMusic.pitch = 1;
            _currentMusic.Play();
            //Debug.Log($"Music {name} starts");
            if (musicAutoPlayNext)
                StartCoroutine(MusicAutoPlayNext(clip.length, Array.IndexOf(musics, m), clipId));
        }

        private IEnumerator MusicAutoPlayNext(float length, int mIndex, int clipId)
        {
            yield return new WaitForSeconds(length);
            clipId++;
            PlayMusic(musics[mIndex].name, clipId % musics[mIndex].clips.Count);
        }

        private IEnumerator MusicFadeOut(string name)
        {
            float elapsed = 0.0f;
            while (elapsed <= musicFadeOutDuration)
            {
                yield return new WaitForEndOfFrame();
                elapsed += Time.deltaTime;
                _currentMusic.volume = (musicFadeOutDuration - elapsed) / musicFadeOutDuration * musicVolume;
            }

            _currentMusic.Stop();
            PlayMusic(name);
        }

        void InitSoundArray(Sound[] soundArray)
        {
            if (soundArray == null) return; // scenes without audio manager are creating empty one
            foreach (Sound s in soundArray)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clips.Count > 0 ? s.clips.GetRandom() : s.clips[0];
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = mixerGroup;
            }
        }

        [Button("PlaySound")]
        public void Play(string sound)
        {
            if (!isAudio.v) return;
            Sound s = Array.Find(sounds, item => item.name == sound);
            if (s == null)
            {
                Debug.LogWarning($"Play sound: {sound} not found!");
                return;
            }

            s.source.clip = s.clips.Count > 0 ? s.clips.GetRandom() : s.clips[0];
            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f)) *
                              soundVolume;
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            s.source.Play();
        }

        public void Stop(string sound)
        {
            Sound s = Array.Find(sounds, item => item.name == sound);
            if (s == null)
            {
                Debug.LogWarning($"Stop sound: {sound} not found!");
                return;
            }

            s.source.Stop();
        }

        public void SetAudio()
        {
            if (isAudio.v)
            {
                AudioManager.instance.Play("AudioEnabled");
                AutoPlayMusic();
            }
            AudioListener.pause = !isAudio.v;
        }
    }
}