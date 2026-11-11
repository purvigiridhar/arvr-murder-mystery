using System;
using UnityEngine;

namespace MurderMystery.Audio
{
    /// <summary>
    /// Person 4 Central Audio System:
    /// Delivers subtle, atmospheric audio feedback for evidence collection, UI interactions,
    /// journal toggles, and ambient university research facility room tone.
    /// Synthesizes clean procedural audio waveforms if external audio files are not assigned,
    /// ensuring zero external dependencies and 100% portability.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioManager>();
                }
                return instance;
            }
            internal set => instance = value;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambienceSource;

        [Header("Volume Controls")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.65f;
        [Range(0f, 1f)] [SerializeField] private float ambienceVolume = 0.22f;

        [Header("Custom Audio Clips (Optional - Procedural fallbacks used if null)")]
        [SerializeField] private AudioClip customCollectClip;
        [SerializeField] private AudioClip customJournalOpenClip;
        [SerializeField] private AudioClip customJournalCloseClip;
        [SerializeField] private AudioClip customButtonClickClip;
        [SerializeField] private AudioClip customButtonHoverClip;
        [SerializeField] private AudioClip customAmbienceClip;

        // Cached procedural clips
        private AudioClip proceduralCollectClip;
        private AudioClip proceduralJournalOpenClip;
        private AudioClip proceduralJournalCloseClip;
        private AudioClip proceduralButtonClickClip;
        private AudioClip proceduralButtonHoverClip;
        private AudioClip proceduralAmbienceClip;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            EnsureAudioSources();
            GenerateProceduralClips();
            StartAmbience();
        }

        private void EnsureAudioSources()
        {
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFX_Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
                sfxSource.spatialBlend = 0f; // 2D UI audio
            }

            if (ambienceSource == null)
            {
                GameObject ambObj = new GameObject("Ambience_Source");
                ambObj.transform.SetParent(transform);
                ambienceSource = ambObj.AddComponent<AudioSource>();
                ambienceSource.playOnAwake = false;
                ambienceSource.loop = true;
                ambienceSource.spatialBlend = 0f; // 2D ambient background
            }
        }

        private void StartAmbience()
        {
            if (ambienceSource == null) return;

            AudioClip clip = customAmbienceClip != null ? customAmbienceClip : proceduralAmbienceClip;
            if (clip != null)
            {
                ambienceSource.clip = clip;
                ambienceSource.volume = ambienceVolume * masterVolume;
                ambienceSource.Play();
            }
        }

        public void PlayEvidenceCollected()
        {
            AudioClip clip = customCollectClip != null ? customCollectClip : proceduralCollectClip;
            PlayOneShot(clip, 0.85f);
        }

        public void PlayJournalOpen()
        {
            AudioClip clip = customJournalOpenClip != null ? customJournalOpenClip : proceduralJournalOpenClip;
            PlayOneShot(clip, 0.6f);
        }

        public void PlayJournalClose()
        {
            AudioClip clip = customJournalCloseClip != null ? customJournalCloseClip : proceduralJournalCloseClip;
            PlayOneShot(clip, 0.5f);
        }

        public void PlayButtonClick()
        {
            AudioClip clip = customButtonClickClip != null ? customButtonClickClip : proceduralButtonClickClip;
            PlayOneShot(clip, 0.55f);
        }

        public void PlayButtonHover()
        {
            AudioClip clip = customButtonHoverClip != null ? customButtonHoverClip : proceduralButtonHoverClip;
            PlayOneShot(clip, 0.25f);
        }

        public void PlayOneShot(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
        }

        // ==========================================
        // PROCEDURAL AUDIO SYNTHESIZERS (ZERO EXTERNAL ASSETS)
        // ==========================================

        private void GenerateProceduralClips()
        {
            try
            {
                proceduralCollectClip = GenerateChimeClip("Procedural_EvidenceCollect", 880f, 1318.5f, 0.45f);
                proceduralJournalOpenClip = GenerateWhooshSlideClip("Procedural_JournalOpen", true, 0.25f);
                proceduralJournalCloseClip = GenerateWhooshSlideClip("Procedural_JournalClose", false, 0.18f);
                proceduralButtonClickClip = GenerateClickClip("Procedural_BtnClick", 1400f, 0.04f);
                proceduralButtonHoverClip = GenerateClickClip("Procedural_BtnHover", 800f, 0.02f);
                proceduralAmbienceClip = GenerateFacilityDroneClip("Procedural_FacilityAmbience", 4.0f);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AudioManager] Procedural clip generation encountered exception: {ex.Message}");
            }
        }

        private AudioClip GenerateChimeClip(string name, float freq1, float freq2, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-7.0f * t); // Smooth exponential decay

                // Harmonic detective discovery chord (Root + Perfect Fifth with subtle vibrato)
                float s1 = Mathf.Sin(2f * Mathf.PI * freq1 * t);
                float s2 = 0.5f * Mathf.Sin(2f * Mathf.PI * freq2 * t);
                float s3 = 0.25f * Mathf.Sin(2f * Mathf.PI * (freq1 * 2f) * t);

                samples[i] = (s1 + s2 + s3) * env * 0.45f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateWhooshSlideClip(string name, bool isOpen, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / duration;
                float env = Mathf.Sin(t * Mathf.PI); // Window envelope

                // Filtered noise simulating paper / folder shuffle
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.25f;
                float toneFreq = isOpen ? Mathf.Lerp(250f, 500f, t) : Mathf.Lerp(450f, 180f, t);
                float tone = Mathf.Sin(2f * Mathf.PI * toneFreq * ((float)i / sampleRate)) * 0.2f;

                samples[i] = (noise + tone) * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateClickClip(string name, float frequency, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-65.0f * t); // Very fast click transient
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);

                samples[i] = wave * env * 0.3f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateFacilityDroneClip(string name, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Seamless low drone for research lab ventilation and server hum (55Hz / 110Hz)
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float f1 = Mathf.Sin(2f * Mathf.PI * 55.0f * t);
                float f2 = 0.35f * Mathf.Sin(2f * Mathf.PI * 110.0f * t);
                float f3 = 0.15f * Mathf.Sin(2f * Mathf.PI * 220.0f * t);

                // Very subtle pink-like gentle background whisper
                float airHiss = (UnityEngine.Random.value * 2f - 1f) * 0.04f;

                samples[i] = (f1 + f2 + f3 + airHiss) * 0.12f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

