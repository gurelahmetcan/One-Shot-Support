using UnityEngine;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Manages all audio in the game (music and sound effects)
    /// Singleton pattern for easy access from anywhere
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("Background Music")]
        [SerializeField] private AudioClip gameMusic;
        [SerializeField] [Range(0f, 1f)] private float defaultMusicVolume = 0.3f;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip itemEquipSound;
        [SerializeField] private AudioClip cratePurchaseSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] [Range(0f, 1f)] private float defaultSFXVolume = 0.7f;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float musicVolume = 0.3f;
        [Range(0f, 1f)] public float sfxVolume = 0.7f;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Initialize volumes
            musicVolume = defaultMusicVolume;
            sfxVolume = defaultSFXVolume;

            SetupAudioSources();
        }

        private void Start()
        {
            // Start playing background music
            PlayBackgroundMusic();
        }

        /// <summary>
        /// Setup audio sources if not assigned in inspector
        /// </summary>
        private void SetupAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.loop = false;
                voiceSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        /// <summary>
        /// Play background music
        /// </summary>
        public void PlayBackgroundMusic()
        {
            if (gameMusic != null && musicSource != null)
            {
                musicSource.clip = gameMusic;
                musicSource.volume = musicVolume;
                musicSource.Play();
                Debug.Log("[AudioManager] Playing background music");
            }
        }

        /// <summary>
        /// Stop background music
        /// </summary>
        public void StopBackgroundMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Play item equip sound effect
        /// </summary>
        public void PlayItemEquipSound()
        {
            PlaySFX(itemEquipSound);
        }

        /// <summary>
        /// Play crate purchase sound effect
        /// </summary>
        public void PlayCratePurchaseSound()
        {
            PlaySFX(cratePurchaseSound);
        }

        /// <summary>
        /// Play button click sound effect
        /// </summary>
        public void PlayButtonClickSound()
        {
            PlaySFX(buttonClickSound);
        }

        /// <summary>
        /// Play hero voiceline
        /// </summary>
        public void PlayHeroVoiceline(AudioClip voiceline)
        {
            if (voiceline != null && voiceSource != null)
            {
                voiceSource.clip = voiceline;
                voiceSource.volume = sfxVolume;
                voiceSource.Play();
                Debug.Log($"[AudioManager] Playing hero voiceline: {voiceline.name}");
            }
        }

        /// <summary>
        /// Play a one-shot sound effect
        /// </summary>
        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        /// <summary>
        /// Set SFX volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Update all audio source volumes
        /// </summary>
        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
    }
}
