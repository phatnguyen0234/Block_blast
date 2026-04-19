using UnityEngine;
using System.Collections.Generic;

public class GameAudio : MonoBehaviour
{
    private AudioSource audioSource;
    private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = 0.14f;
    }

    public void PlayPlace()
    {
        PlayTone("place", 540f, 0.05f, 0.12f);
    }

    public void PlayInvalid()
    {
        PlayTone("invalid", 220f, 0.08f, 0.14f);
    }

    public void PlayClear(int linesCleared)
    {
        float frequency = 680f + (Mathf.Clamp(linesCleared, 1, 4) - 1) * 90f;
        PlayTone($"clear_{linesCleared}", frequency, 0.12f, 0.18f);
    }

    public void PlayCombo(int combo)
    {
        float frequency = 820f + Mathf.Clamp(combo, 1, 6) * 45f;
        PlayTone($"combo_{combo}", frequency, 0.1f, 0.16f);
    }

    public void PlayGameOver()
    {
        PlayTone("gameover", 180f, 0.25f, 0.18f);
    }

    public void PlayNewGame()
    {
        PlayTone("newgame", 620f, 0.09f, 0.16f);
    }

    private void PlayTone(string key, float frequency, float duration, float volume)
    {
        if (!clipCache.TryGetValue(key, out AudioClip clip))
        {
            clip = CreateTone(key, frequency, duration);
            clipCache[key] = clip;
        }

        audioSource.PlayOneShot(clip, volume);
    }

    private AudioClip CreateTone(string key, float frequency, float duration)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = Mathf.Clamp01(1f - (i / (float)sampleCount));
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create(key, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
