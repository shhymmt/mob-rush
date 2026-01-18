using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all audio in the game.
/// ゲーム内の全てのオーディオを管理。
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float seVolume = 0.7f;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip gateAddSound;
    [SerializeField] private AudioClip gateSubtractSound;
    [SerializeField] private AudioClip gateMultiplySound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip bossAppearSound;
    [SerializeField] private AudioClip bossAttackSound;
    [SerializeField] private AudioClip bgmClip;

    [Header("Settings")]
    [SerializeField] private bool generatePlaceholderSounds = true;

    // Sound type enum for easy access
    public enum SoundType
    {
        Shoot,
        Hit,
        Explosion,
        GateAdd,
        GateSubtract,
        GateMultiply,
        GameOver,
        Victory,
        ButtonClick,
        BossAppear,
        BossAttack
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetupAudioSources();

        if (generatePlaceholderSounds)
        {
            GeneratePlaceholderSounds();
        }
    }

    void Start()
    {
        // Start BGM
        if (bgmClip != null)
        {
            PlayBGM(bgmClip);
        }

        Debug.Log("SoundManager: Initialized");
    }

    private void SetupAudioSources()
    {
        // Create BGM source if not assigned
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGMSource");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f; // 2D sound
        }

        // Create SE source if not assigned
        if (seSource == null)
        {
            GameObject seObj = new GameObject("SESource");
            seObj.transform.SetParent(transform);
            seSource = seObj.AddComponent<AudioSource>();
            seSource.loop = false;
            seSource.playOnAwake = false;
            seSource.spatialBlend = 0f; // 2D sound
        }

        UpdateVolumes();
    }

    #region Placeholder Sound Generation

    private void GeneratePlaceholderSounds()
    {
        // Generate simple synthesized sounds
        // 発射音：頻繁に鳴るため控えめに
        if (shootSound == null)
            shootSound = GenerateTone(800f, 0.05f, ToneType.Square, 0.2f);

        // ヒット音：複数同時に鳴るため控えめに
        if (hitSound == null)
            hitSound = GenerateTone(1200f, 0.03f, ToneType.Noise, 0.25f);

        if (explosionSound == null)
            explosionSound = GenerateExplosionSound();

        if (gateAddSound == null)
            gateAddSound = GenerateChime(new float[] { 523f, 659f, 784f }, 0.15f); // C5, E5, G5

        if (gateSubtractSound == null)
            gateSubtractSound = GenerateTone(200f, 0.2f, ToneType.Square, 0.5f);

        if (gateMultiplySound == null)
            gateMultiplySound = GenerateChime(new float[] { 523f, 784f, 1047f }, 0.12f); // C5, G5, C6

        if (gameOverSound == null)
            gameOverSound = GenerateGameOverSound();

        if (victorySound == null)
            victorySound = GenerateVictorySound();

        if (buttonClickSound == null)
            buttonClickSound = GenerateTone(600f, 0.05f, ToneType.Sine, 0.5f);

        // ボス出現音：低い音で威圧感
        if (bossAppearSound == null)
            bossAppearSound = GenerateBossAppearSound();

        // ボス攻撃音：警告音的な音
        if (bossAttackSound == null)
            bossAttackSound = GenerateTone(300f, 0.15f, ToneType.Square, 0.4f);

        if (bgmClip == null)
            bgmClip = GenerateSimpleBGM();

        Debug.Log("SoundManager: Placeholder sounds generated");
    }

    private AudioClip GenerateBossAppearSound()
    {
        int sampleRate = 44100;
        float duration = 0.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // 低い音が上昇していく威圧的なサウンド
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float frequency = 100f + t * 200f; // 100Hz -> 300Hz
            float envelope = Mathf.Sin(t * Mathf.PI); // 中央が最大
            float sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
            samples[i] = sample * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("BossAppear", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private enum ToneType { Sine, Square, Noise }

    private AudioClip GenerateTone(float frequency, float duration, ToneType type, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / sampleCount; // Fade out

            float sample = 0f;
            switch (type)
            {
                case ToneType.Sine:
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                    break;
                case ToneType.Square:
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t) > 0 ? 1f : -1f;
                    break;
                case ToneType.Noise:
                    sample = Random.Range(-1f, 1f);
                    break;
            }

            samples[i] = sample * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("GeneratedTone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateChime(float[] frequencies, float noteDuration)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * noteDuration * frequencies.Length);
        float[] samples = new float[totalSamples];

        int samplesPerNote = (int)(sampleRate * noteDuration);

        for (int note = 0; note < frequencies.Length; note++)
        {
            float freq = frequencies[note];
            int startSample = note * samplesPerNote;

            for (int i = 0; i < samplesPerNote && startSample + i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samplesPerNote;
                samples[startSample + i] += Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.3f;
            }
        }

        AudioClip clip = AudioClip.Create("GeneratedChime", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateExplosionSound()
    {
        int sampleRate = 44100;
        float duration = 0.3f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Pow(1f - t, 2f); // Quick decay
            float noise = Random.Range(-1f, 1f);
            float lowFreq = Mathf.Sin(2f * Mathf.PI * 80f * t / sampleRate * i);

            samples[i] = (noise * 0.7f + lowFreq * 0.3f) * envelope * 0.6f;
        }

        AudioClip clip = AudioClip.Create("Explosion", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateGameOverSound()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Descending tones
        float[] frequencies = { 400f, 350f, 300f, 250f };
        int samplesPerNote = sampleCount / frequencies.Length;

        for (int note = 0; note < frequencies.Length; note++)
        {
            float freq = frequencies[note];
            int startSample = note * samplesPerNote;

            for (int i = 0; i < samplesPerNote; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samplesPerNote;
                if (startSample + i < sampleCount)
                {
                    samples[startSample + i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
                }
            }
        }

        AudioClip clip = AudioClip.Create("GameOver", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateVictorySound()
    {
        int sampleRate = 44100;
        float duration = 1.0f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Ascending fanfare
        float[] frequencies = { 523f, 659f, 784f, 1047f }; // C5, E5, G5, C6
        float[] durations = { 0.15f, 0.15f, 0.15f, 0.4f };

        int currentSample = 0;
        for (int note = 0; note < frequencies.Length; note++)
        {
            float freq = frequencies[note];
            int noteSamples = (int)(sampleRate * durations[note]);

            for (int i = 0; i < noteSamples && currentSample + i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / noteSamples * 0.5f;
                samples[currentSample + i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
            }
            currentSample += noteSamples;
        }

        AudioClip clip = AudioClip.Create("Victory", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateSimpleBGM()
    {
        int sampleRate = 44100;
        float duration = 8f; // 8 second loop
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Simple bass line with rhythm
        float[] bassNotes = { 130.81f, 146.83f, 164.81f, 146.83f }; // C3, D3, E3, D3
        float noteDuration = duration / bassNotes.Length;
        int samplesPerNote = (int)(sampleRate * noteDuration);

        for (int note = 0; note < bassNotes.Length; note++)
        {
            float freq = bassNotes[note];
            int startSample = note * samplesPerNote;

            for (int i = 0; i < samplesPerNote; i++)
            {
                float t = (float)i / sampleRate;
                float beatEnvelope = (i % (samplesPerNote / 4) < samplesPerNote / 8) ? 1f : 0.3f;
                float fadeEnvelope = 1f - (float)(i % (samplesPerNote / 4)) / (samplesPerNote / 4) * 0.5f;

                if (startSample + i < sampleCount)
                {
                    samples[startSample + i] = Mathf.Sin(2f * Mathf.PI * freq * t) * beatEnvelope * fadeEnvelope * 0.2f;
                }
            }
        }

        AudioClip clip = AudioClip.Create("BGM", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 効果音を再生
    /// </summary>
    public void PlaySE(SoundType type)
    {
        AudioClip clip = GetClipForType(type);
        if (clip != null && seSource != null)
        {
            seSource.PlayOneShot(clip, seVolume * masterVolume);
        }
    }

    /// <summary>
    /// カスタムクリップで効果音を再生
    /// </summary>
    public void PlaySE(AudioClip clip)
    {
        if (clip != null && seSource != null)
        {
            seSource.PlayOneShot(clip, seVolume * masterVolume);
        }
    }

    /// <summary>
    /// BGMを再生
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume * masterVolume;
        bgmSource.Play();
    }

    /// <summary>
    /// BGMを停止
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// BGMを一時停止
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }

    /// <summary>
    /// BGMを再開
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }

    /// <summary>
    /// マスター音量を設定
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    /// <summary>
    /// BGM音量を設定
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    /// <summary>
    /// SE音量を設定
    /// </summary>
    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
    }

    #endregion

    #region Private Methods

    private AudioClip GetClipForType(SoundType type)
    {
        switch (type)
        {
            case SoundType.Shoot: return shootSound;
            case SoundType.Hit: return hitSound;
            case SoundType.Explosion: return explosionSound;
            case SoundType.GateAdd: return gateAddSound;
            case SoundType.GateSubtract: return gateSubtractSound;
            case SoundType.GateMultiply: return gateMultiplySound;
            case SoundType.GameOver: return gameOverSound;
            case SoundType.Victory: return victorySound;
            case SoundType.ButtonClick: return buttonClickSound;
            case SoundType.BossAppear: return bossAppearSound;
            case SoundType.BossAttack: return bossAttackSound;
            default: return null;
        }
    }

    private void UpdateVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume * masterVolume;
        }
    }

    #endregion
}
