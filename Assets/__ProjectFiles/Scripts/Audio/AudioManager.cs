using System;
using System.Collections;
using UnityEngine;

namespace Orpita.Audio
{
    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        
        [Tooltip("Dedicated source for the ghost/horror SFX so it can fade independently.")]
        [SerializeField] private AudioSource ghostSource; 

        [Header("Audio Library")]
        [SerializeField] private Sound[] sfxLibrary;
        [SerializeField] private Sound[] musicLibrary;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySFX(string soundName)
        {
            Sound s = Array.Find(sfxLibrary, x => x.name == soundName);
            if (s == null) return;
            
            sfxSource.pitch = s.pitch;
            sfxSource.PlayOneShot(s.clip, s.volume);
        }
        
        public void PlayMusic(string musicName)
        {
            Sound s = Array.Find(musicLibrary, x => x.name == musicName);
            if (s == null) return;

            musicSource.clip = s.clip;
            musicSource.volume = s.volume;
            musicSource.pitch = s.pitch;
            musicSource.loop = s.loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void StopSFX()
        {
            sfxSource.Stop();
        }

        /// <summary>
        /// Plays a continuous ghost/horror SFX on its own dedicated source.
        /// </summary>
        public void PlayGhostSFX(string soundName)
        {
            Sound s = Array.Find(sfxLibrary, x => x.name == soundName);
            if (s == null) return;

            ghostSource.clip = s.clip;
            ghostSource.volume = s.volume;
            ghostSource.pitch = s.pitch;
            ghostSource.loop = s.loop; // Usually true for continuous ghost sounds
            ghostSource.Play();
        }

        /// <summary>
        /// Fades out the ghost SFX smoothly over time.
        /// </summary>
        public void FadeOutGhostSFX(float duration = 1.5f)
        {
            if (ghostSource != null && ghostSource.isPlaying)
            {
                StartCoroutine(FadeOutCoroutine(duration));
            }
        }

        private IEnumerator FadeOutCoroutine(float fadeDuration)
        {
            float startVol = ghostSource.volume;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                ghostSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
                yield return null;
            }

            ghostSource.Stop();
            ghostSource.volume = startVol; // Reset volume for the next time it plays
        }
    }
}