//-----AudioEventData.cs START-----

using UnityEngine;
using UnityEngine.Audio;

public enum AudioEventSelectionMode
{
    Random,
    Sequential
}

[CreateAssetMenu(
    fileName = "AudioEventData_NewEvent",
    menuName = "Echo Systems Lab/Audio/Audio Event Data")]
public class AudioEventData : ScriptableObject
{
    [Header("Clips")]
    public AudioClip[] clips;
    public AudioEventSelectionMode selectionMode = AudioEventSelectionMode.Random;

    [Header("Mixer Routing")]
    public AudioMixerGroup outputMixerGroup;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Optional extra random multiplier applied after Volume.")]
    public Vector2 volumeMultiplierRange = new Vector2(1f, 1f);

    [Header("Pitch")]
    public Vector2 pitchRange = new Vector2(1f, 1f);

    [Header("Spatial Settings")]
    [Range(0f, 1f)]
    public float spatialBlend = 0f;

    public float minDistance = 1f;
    public float maxDistance = 30f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    [Header("Playback Rules")]
    [Tooltip("Prevents this AudioEventData from playing again until this cooldown expires.")]
    public float cooldownSeconds = 0f;

    [Tooltip("Useful for pause-menu UI sounds if you later use AudioListener.pause.")]
    public bool ignoreListenerPause = false;

    [Header("Debug")]
    public bool debugLogs = false;

    [System.NonSerialized] private int nextSequentialIndex;
    [System.NonSerialized] private float nextAllowedPlayTime;

    public bool TryGetClip(out AudioClip clip)
    {
        clip = null;

        if (!CanPlay())
        {
            if (debugLogs)
                Debug.Log($"{name} audio event blocked by cooldown.");

            return false;
        }

        clip = SelectClip();

        if (clip == null)
        {
            if (debugLogs)
                Debug.LogWarning($"{name} audio event has no valid clip.");

            return false;
        }

        MarkPlayed();
        return true;
    }

    public void ApplyToAudioSource(AudioSource source, AudioMixerGroup fallbackMixerGroup)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = false;

        source.outputAudioMixerGroup = outputMixerGroup != null
            ? outputMixerGroup
            : fallbackMixerGroup;

        source.volume = GetRandomVolume();
        source.pitch = GetRandomPitch();

        source.spatialBlend = Mathf.Clamp01(spatialBlend);
        source.minDistance = Mathf.Max(0f, minDistance);
        source.maxDistance = Mathf.Max(source.minDistance, maxDistance);
        source.rolloffMode = rolloffMode;
        source.ignoreListenerPause = ignoreListenerPause;
    }

    public float GetRandomPitch()
    {
        float minPitch = Mathf.Min(pitchRange.x, pitchRange.y);
        float maxPitch = Mathf.Max(pitchRange.x, pitchRange.y);

        return Random.Range(minPitch, maxPitch);
    }

    public float GetRandomVolume()
    {
        float minMultiplier = Mathf.Min(volumeMultiplierRange.x, volumeMultiplierRange.y);
        float maxMultiplier = Mathf.Max(volumeMultiplierRange.x, volumeMultiplierRange.y);

        float multiplier = Random.Range(minMultiplier, maxMultiplier);
        return Mathf.Clamp01(volume * multiplier);
    }

    public float GetPlaybackDuration(AudioClip clip, float pitch)
    {
        if (clip == null)
            return 0f;

        float safePitch = Mathf.Max(0.01f, Mathf.Abs(pitch));
        return clip.length / safePitch;
    }

    private bool CanPlay()
    {
        if (cooldownSeconds <= 0f)
            return true;

        return Time.unscaledTime >= nextAllowedPlayTime;
    }

    private void MarkPlayed()
    {
        if (cooldownSeconds <= 0f)
            return;

        nextAllowedPlayTime = Time.unscaledTime + cooldownSeconds;
    }

    private AudioClip SelectClip()
    {
        if (clips == null || clips.Length == 0)
            return null;

        switch (selectionMode)
        {
            case AudioEventSelectionMode.Sequential:
                return SelectSequentialClip();

            case AudioEventSelectionMode.Random:
            default:
                return SelectRandomClip();
        }
    }

    private AudioClip SelectRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return null;

        for (int i = 0; i < clips.Length; i++)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];

            if (clip != null)
                return clip;
        }

        return null;
    }

    private AudioClip SelectSequentialClip()
    {
        if (clips == null || clips.Length == 0)
            return null;

        for (int i = 0; i < clips.Length; i++)
        {
            int index = nextSequentialIndex;
            nextSequentialIndex++;

            if (nextSequentialIndex >= clips.Length)
                nextSequentialIndex = 0;

            AudioClip clip = clips[index];

            if (clip != null)
                return clip;
        }

        return null;
    }
}

//-----AudioEventData.cs END-----