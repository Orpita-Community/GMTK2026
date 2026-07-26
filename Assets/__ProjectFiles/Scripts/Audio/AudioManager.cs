using System;
using UnityEngine;

namespace Orpita.Audio
{
    // This creates a custom class so we can organize clips by name in the Inspector
    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        
        [Tooltip("Check this if the sound should loop (like ambient wind).")]
        public bool loop = false;
    }

    public class AudioManager : MonoBehaviour
    {
        // Singleton instance allows any script to call AudioManager.Instance
        public static AudioManager Instance;

        [Header("Audio Sources")]
        [Tooltip("Dedicated source for background music and ambient loops.")]
        [SerializeField] private AudioSource musicSource;
        
        [Tooltip("Dedicated source for one-off sound effects.")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Library")]
        [Tooltip("Add all your sound effects here (keys, doors, matches).")]
        [SerializeField] private Sound[] sfxLibrary;
        
        [Tooltip("Add all your background tracks here (eerie wind, lockdown siren).")]
        [SerializeField] private Sound[] musicLibrary;

        private void Awake()
        {
            // Standard Singleton setup to persist the audio across scene loads
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

        /// <summary>
        /// Plays a sound effect once. Multiple SFX can overlap.
        /// </summary>
        public void PlaySFX(string soundName)
        {
            Sound s = Array.Find(sfxLibrary, x => x.name == soundName);
            if (s == null)
            {
                Debug.LogWarning($"AudioManager: SFX '{soundName}' not found!");
                return;
            }
            
            sfxSource.pitch = s.pitch;
            sfxSource.PlayOneShot(s.clip, s.volume);
        }
        
        /// <summary>
        /// Plays background music or ambient loops. Replaces whatever is currently playing.
        /// </summary>
        public void PlayMusic(string musicName)
        {
            Sound s = Array.Find(musicLibrary, x => x.name == musicName);
            if (s == null)
            {
                Debug.LogWarning($"AudioManager: Music '{musicName}' not found!");
                return;
            }

            musicSource.clip = s.clip;
            musicSource.volume = s.volume;
            musicSource.pitch = s.pitch;
            musicSource.loop = s.loop;
            musicSource.Play();
        }

        /// <summary>
        /// Instantly stops the music/ambient track.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}