using System;
using UnityEngine;

namespace MurderMystery.Audio
{
    /// <summary>
    /// Person 4 Central Audio System:
    /// Provides immersive audio feedback for investigation gameplay:
    /// - Subtle university research facility ambience
    /// - Player movement footsteps
    /// - Evidence discovery and collection chimes
    /// - Journal dossier open/close handling
    /// - Dialogue interface ticks
    /// - Accusation and case outcome stingers
    /// Synthesizes clean procedural audio waveforms dynamically at runtime,
    /// guaranteeing zero external binary dependencies and 100% portability.
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
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        instance = go.AddComponent<AudioManager>();
                    }
                }
                return instance;
            }
            internal set => instance = value;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource footstepSource;

        [Header("Volume Controls")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.65f;
        [Range(0f, 1f)] [SerializeField] private float ambienceVolume = 0.22f;
        [Range(0f, 1f)] [SerializeField] private float footstepVolume = 0.32f;

        [Header("Custom Audio Clips (Optional - Procedural fallbacks used if null)")]
        [SerializeField] private AudioClip customCollectClip;
        [SerializeField] private AudioClip customJournalOpenClip;
        [SerializeField] private AudioClip customJournalCloseClip;
        [SerializeField] private AudioClip customButtonClickClip;
        [SerializeField] private AudioClip customButtonHoverClip;
        [SerializeField] private AudioClip customDialogueTickClip;
        [SerializeField] private AudioClip customAmbienceClip;
        [SerializeField] private AudioClip customFootstepClip;
        [SerializeField] private AudioClip customObjectiveClip;
        [SerializeField] private AudioClip customSolvedClip;
        [SerializeField] private AudioClip customFailedClip;
        [SerializeField] private AudioClip customInconclusiveClip;

        // Cached procedural clips
        private AudioClip proceduralCollectClip;
        private AudioClip proceduralJournalOpenClip;
        private AudioClip proceduralJournalCloseClip;
        private AudioClip proceduralButtonClickClip;
        private AudioClip proceduralButtonHoverClip;
        private AudioClip proceduralDialogueTickClip;
        private AudioClip proceduralAmbienceClip;
        private AudioClip proceduralFootstepClip;
        private AudioClip proceduralObjectiveClip;
        private AudioClip proceduralSolvedClip;
        private AudioClip proceduralFailedClip;
        private AudioClip proceduralInconclusiveClip;

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
                sfxSource.spatialBlend = 0f;
            }

            if (ambienceSource == null)
            {
                GameObject ambObj = new GameObject("Ambience_Source");
                ambObj.transform.SetParent(transform);
                ambienceSource = ambObj.AddComponent<AudioSource>();
                ambienceSource.playOnAwake = false;
                ambienceSource.loop = true;
                ambienceSource.spatialBlend = 0f;
            }

            if (footstepSource == null)
            {
                GameObject fsObj = new GameObject("Footstep_Source");
                fsObj.transform.SetParent(transform);
                footstepSource = fsObj.AddComponent<AudioSource>();
                footstepSource.playOnAwake = false;
                footstepSource.loop = false;
                footstepSource.spatialBlend = 0f;
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
            PlayOneShot(clip, 0.5f);
        }

        public void PlayButtonHover()
        {
            AudioClip clip = customButtonHoverClip != null ? customButtonHoverClip : proceduralButtonHoverClip;
            PlayOneShot(clip, 0.22f);
        }

        public void PlayDialogueTick()
        {
            AudioClip clip = customDialogueTickClip != null ? customDialogueTickClip : proceduralDialogueTickClip;
            PlayOneShot(clip, 0.35f);
        }

        public void PlayObjectiveCompleted()
        {
            AudioClip clip = customObjectiveClip != null ? customObjectiveClip : proceduralObjectiveClip;
            PlayOneShot(clip, 0.75f);
        }

        public void PlayCaseSolved()
        {
            AudioClip clip = customSolvedClip != null ? customSolvedClip : proceduralSolvedClip;
            PlayOneShot(clip, 0.9f);
        }

        public void PlayCaseFailed()
        {
            AudioClip clip = customFailedClip != null ? customFailedClip : proceduralFailedClip;
            PlayOneShot(clip, 0.85f);
        }

        public void PlayCaseInconclusive()
        {
            AudioClip clip = customInconclusiveClip != null ? customInconclusiveClip : proceduralInconclusiveClip;
            PlayOneShot(clip, 0.8f);
        }

        public void PlayFootstep()
        {
            if (footstepSource == null) return;
            AudioClip clip = customFootstepClip != null ? customFootstepClip : proceduralFootstepClip;
            if (clip == null) return;

            footstepSource.pitch = UnityEngine.Random.Range(0.88f, 1.12f);
            footstepSource.PlayOneShot(clip, footstepVolume * masterVolume);
        }

        public void PlayOneShot(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
        }

        // ==========================================
        // PROCEDURAL AUDIO SYNTHESIS
        // ==========================================

        private void GenerateProceduralClips()
        {
            try
            {
                proceduralCollectClip = GenerateChimeClip("Procedural_EvidenceCollect", 880f, 1318.5f, 0.45f);
                proceduralJournalOpenClip = GenerateWhooshSlideClip("Procedural_JournalOpen", true, 0.24f);
                proceduralJournalCloseClip = GenerateWhooshSlideClip("Procedural_JournalClose", false, 0.18f);
                proceduralButtonClickClip = GenerateClickClip("Procedural_BtnClick", 1400f, 0.035f);
                proceduralButtonHoverClip = GenerateClickClip("Procedural_BtnHover", 850f, 0.02f);
                proceduralDialogueTickClip = GenerateClickClip("Procedural_DialogueTick", 1150f, 0.015f);
                proceduralFootstepClip = GenerateFootstepClip("Procedural_Footstep", 0.09f);
                proceduralObjectiveClip = GenerateObjectiveChordClip("Procedural_Objective", 0.6f);
                proceduralSolvedClip = GenerateSolvedChordClip("Procedural_Solved", 1.4f);
                proceduralFailedClip = GenerateFailedStingerClip("Procedural_Failed", 1.2f);
                proceduralInconclusiveClip = GenerateInconclusiveStingerClip("Procedural_Inconclusive", 1.0f);
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
                float env = Mathf.Exp(-7.0f * t);

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
                float env = Mathf.Sin(t * Mathf.PI);

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
                float env = Mathf.Exp(-65.0f * t);
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);

                samples[i] = wave * env * 0.3f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateFootstepClip(string name, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-40.0f * t);
                float lowImpact = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.4f;
                float friction = (UnityEngine.Random.value * 2f - 1f) * 0.15f;

                samples[i] = (lowImpact + friction) * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateObjectiveChordClip(string name, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // 3-note ascending arpeggio (D5, A5, D6)
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-5.0f * t);

                float note1 = Mathf.Sin(2f * Mathf.PI * 587.33f * t);
                float note2 = t > 0.08f ? Mathf.Sin(2f * Mathf.PI * 880.00f * (t - 0.08f)) * Mathf.Exp(-5f * (t - 0.08f)) : 0f;
                float note3 = t > 0.16f ? Mathf.Sin(2f * Mathf.PI * 1174.66f * (t - 0.16f)) * Mathf.Exp(-5f * (t - 0.16f)) : 0f;

                samples[i] = (note1 * 0.3f + note2 * 0.3f + note3 * 0.35f) * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateSolvedChordClip(string name, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Major harmonic chord resolution (C - E - G - C)
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-2.8f * t);

                float c = Mathf.Sin(2f * Mathf.PI * 523.25f * t);
                float e = Mathf.Sin(2f * Mathf.PI * 659.25f * t);
                float g = Mathf.Sin(2f * Mathf.PI * 783.99f * t);
                float cHigh = Mathf.Sin(2f * Mathf.PI * 1046.50f * t);

                samples[i] = (c * 0.25f + e * 0.25f + g * 0.25f + cHigh * 0.25f) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateFailedStingerClip(string name, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Tense descending minor interval
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-3.2f * t);

                float pitch = Mathf.Lerp(330f, 165f, t / duration);
                float s = Mathf.Sin(2f * Mathf.PI * pitch * t);
                float diss = Mathf.Sin(2f * Mathf.PI * (pitch * 1.059f) * t) * 0.5f; // minor second clash

                samples[i] = (s + diss) * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateInconclusiveStingerClip(string name, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Unresolved suspense chord
            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-3.0f * t);

                float s1 = Mathf.Sin(2f * Mathf.PI * 440f * t);
                float s2 = Mathf.Sin(2f * Mathf.PI * 523.25f * t);
                float s3 = Mathf.Sin(2f * Mathf.PI * 622.25f * t); // diminished fifth

                samples[i] = (s1 + s2 + s3) * env * 0.35f;
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

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float f1 = Mathf.Sin(2f * Mathf.PI * 55.0f * t);
                float f2 = 0.35f * Mathf.Sin(2f * Mathf.PI * 110.0f * t);
                float f3 = 0.15f * Mathf.Sin(2f * Mathf.PI * 220.0f * t);
                float airHiss = (UnityEngine.Random.value * 2f - 1f) * 0.035f;

                samples[i] = (f1 + f2 + f3 + airHiss) * 0.11f;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

