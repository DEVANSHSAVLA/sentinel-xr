using UnityEngine;

namespace SentinelXR.Gameplay
{
    /// <summary>
    /// Procedural futuristic synthesizer and sound effect manager for SENTINEL-XR.
    /// Mathematically generates cyberpunk UI, combat, scanner, and feedback audio
    /// without requiring external audio asset dependencies.
    /// </summary>
    public class SentinelAudioSystem : MonoBehaviour
    {
        public static SentinelAudioSystem Instance { get; private set; }

        private AudioSource audioSource;

        private AudioClip clipUiClick;
        private AudioClip clipScanPulse;
        private AudioClip clipPatchTool;
        private AudioClip clipQuarantinePulse;
        private AudioClip clipFirewallDeploy;
        private AudioClip clipShieldHit;
        private AudioClip clipPlayerDamage;
        private AudioClip clipEnemyPurged;
        private AudioClip clipMissionSuccess;
        private AudioClip clipMissionFailed;

        private const int SAMPLE_RATE = 44100;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                InitAudioSource();
                GenerateProceduralClips();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D clean HUD audio
        }

        private void GenerateProceduralClips()
        {
            clipUiClick = GenerateBlip(0.04f, 1800f, 2400f);
            clipScanPulse = GenerateSweep(0.35f, 440f, 1200f, true);
            clipPatchTool = GenerateTone(0.20f, 750f, 0.4f);
            clipQuarantinePulse = GenerateBassDrop(0.50f, 220f, 45f);
            clipFirewallDeploy = GenerateChord(0.40f, new float[] { 330f, 440f, 660f });
            clipShieldHit = GenerateDeflectionZap(0.12f, 1600f, 400f);
            clipPlayerDamage = GenerateThud(0.25f, 110f, 30f);
            clipEnemyPurged = GeneratePurgeChime(0.28f, 1200f, 600f);
            clipMissionSuccess = GenerateSuccessArpeggio();
            clipMissionFailed = GenerateFailureTones();
        }

        public void PlayUiClick() => PlayOneShot(clipUiClick, 0.6f);
        public void PlayScanPulse() => PlayOneShot(clipScanPulse, 0.75f);
        public void PlayPatchTool() => PlayOneShot(clipPatchTool, 0.45f);
        public void PlayQuarantinePulse() => PlayOneShot(clipQuarantinePulse, 0.85f);
        public void PlayFirewallDeploy() => PlayOneShot(clipFirewallDeploy, 0.7f);
        public void PlayShieldHit() => PlayOneShot(clipShieldHit, 0.65f);
        public void PlayPlayerDamage() => PlayOneShot(clipPlayerDamage, 0.8f);
        public void PlayEnemyPurged() => PlayOneShot(clipEnemyPurged, 0.7f);
        public void PlayMissionSuccess() => PlayOneShot(clipMissionSuccess, 0.9f);
        public void PlayMissionFailed() => PlayOneShot(clipMissionFailed, 0.9f);

        private void PlayOneShot(AudioClip clip, float volume = 1.0f)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }

        private AudioClip GenerateBlip(float duration, float startFreq, float endFreq)
        {
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = 1f - t;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SAMPLE_RATE) * env;
            }
            return CreateClip("UiBlip", data);
        }

        private AudioClip GenerateSweep(float duration, float startFreq, float endFreq, bool harmonic)
        {
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, Mathf.SmoothStep(0f, 1f, t));
                float env = Mathf.Sin(t * Mathf.PI);
                float val = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SAMPLE_RATE);
                if (harmonic)
                {
                    val += 0.5f * Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * (float)i / SAMPLE_RATE);
                }
                data[i] = val * env * 0.6f;
            }
            return CreateClip("ScanSweep", data);
        }

        private AudioClip GenerateTone(float duration, float freq, float maxVol)
        {
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SAMPLE_RATE) * env * maxVol;
            }
            return CreateClip("Tone", data);
        }

        private AudioClip GenerateBassDrop(float duration, float startFreq, float endFreq)
        {
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t * t);
                float env = Mathf.Pow(1f - t, 0.7f);
                float noise = (Random.value * 2f - 1f) * 0.15f * (1f - t);
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * (float)i / SAMPLE_RATE) + noise) * env;
            }
            return CreateClip("BassDrop", data);
        }

        private AudioClip GenerateChord(float duration, float[] frequencies)
        {
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = Mathf.Sin(t * Mathf.PI * 0.5f) * (1f - t);
                float sum = 0f;
                foreach (var f in frequencies)
                {
                    sum += Mathf.Sin(2f * Mathf.PI * f * (float)i / SAMPLE_RATE);
                }
                data[i] = (sum / frequencies.Length) * env;
            }
            return CreateClip("Chord", data);
        }

        private AudioClip GenerateDeflectionZap(float duration, float startFreq, float endFreq)
        {
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = Mathf.Pow(1f - t, 1.5f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SAMPLE_RATE) * env;
            }
            return CreateClip("DeflectionZap", data);
        }

        private AudioClip GenerateThud(float duration, float startFreq, float endFreq)
        {
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = Mathf.Pow(1f - t, 2f);
                float noise = (Random.value * 2f - 1f) * 0.25f * env;
                data[i] = (Mathf.Sin(2f * Mathf.PI * freq * (float)i / SAMPLE_RATE) + noise) * env;
            }
            return CreateClip("Thud", data);
        }

        private AudioClip GeneratePurgeChime(float duration, float startFreq, float endFreq)
        {
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = (1f - t);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (float)i / SAMPLE_RATE) * env;
            }
            return CreateClip("PurgeChime", data);
        }

        private AudioClip GenerateSuccessArpeggio()
        {
            float duration = 1.2f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            float noteDur = duration / notes.Length;

            for (int i = 0; i < samples; i++)
            {
                float timeSec = (float)i / SAMPLE_RATE;
                int noteIdx = Mathf.Clamp((int)(timeSec / noteDur), 0, notes.Length - 1);
                float freq = notes[noteIdx];
                float noteT = (timeSec % noteDur) / noteDur;
                float env = Mathf.Sin(noteT * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * timeSec) * env * 0.7f;
            }
            return CreateClip("SuccessArp", data);
        }

        private AudioClip GenerateFailureTones()
        {
            float duration = 1.0f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];
            float[] notes = { 349.23f, 311.13f, 261.63f, 196.00f }; // F4, Eb4, C4, G3
            float noteDur = duration / notes.Length;

            for (int i = 0; i < samples; i++)
            {
                float timeSec = (float)i / SAMPLE_RATE;
                int noteIdx = Mathf.Clamp((int)(timeSec / noteDur), 0, notes.Length - 1);
                float freq = notes[noteIdx];
                float noteT = (timeSec % noteDur) / noteDur;
                float env = (1f - noteT);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * timeSec) * env * 0.7f;
            }
            return CreateClip("FailureTones", data);
        }

        private AudioClip CreateClip(string name, float[] samples)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SAMPLE_RATE, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
