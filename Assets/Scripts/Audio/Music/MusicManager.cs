//-----MusicManager.cs START-----

using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Lifetime")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private AudioSource sourceA;
    private AudioSource sourceB;

    private AudioSource activeSource;
    private AudioSource inactiveSource;

    private MusicTrackData currentTrack;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        CreateSources();
    }

    public void PlayTrack(MusicTrackData track)
    {
        PlayTrack(track, false);
    }

    public void PlayTrack(MusicTrackData track, bool forceRestart)
    {
        if (track == null || track.musicClip == null)
        {
            StopMusic(0.5f);
            return;
        }

        bool sameTrackAlreadyPlaying =
            currentTrack == track &&
            activeSource != null &&
            activeSource.isPlaying;

        if (sameTrackAlreadyPlaying && !forceRestart && !track.restartIfAlreadyPlaying)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(CrossfadeRoutine(track, forceRestart));

        if (debugLogs)
            Debug.Log($"MusicManager playing track: {track.name}");
    }

    public void StopMusic(float fadeOutTime = 1f)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(StopRoutine(fadeOutTime));
        currentTrack = null;
    }

    private IEnumerator CrossfadeRoutine(MusicTrackData nextTrack, bool forceRestart)
    {
        AudioSource oldSource = activeSource;
        AudioSource newSource = inactiveSource;

        ConfigureSource(newSource, nextTrack);
        newSource.volume = 0f;
        newSource.Play();

        float fadeInTime = Mathf.Max(0f, nextTrack.fadeInTime);
        float fadeOutTime = currentTrack != null
            ? Mathf.Max(0f, currentTrack.fadeOutTime)
            : fadeInTime;

        float oldStartVolume = oldSource != null ? oldSource.volume : 0f;
        float targetVolume = Mathf.Clamp01(nextTrack.volume);

        float duration = Mathf.Max(fadeInTime, fadeOutTime);

        if (duration <= 0f)
        {
            if (oldSource != null)
                oldSource.Stop();

            newSource.volume = targetVolume;
            SwapSources();
            currentTrack = nextTrack;
            fadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float fadeInPercent = fadeInTime <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeInTime);

            float fadeOutPercent = fadeOutTime <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeOutTime);

            newSource.volume = Mathf.Lerp(0f, targetVolume, fadeInPercent);

            if (oldSource != null)
                oldSource.volume = Mathf.Lerp(oldStartVolume, 0f, fadeOutPercent);

            yield return null;
        }

        if (oldSource != null)
            oldSource.Stop();

        newSource.volume = targetVolume;

        SwapSources();

        currentTrack = nextTrack;
        fadeRoutine = null;
    }

    private IEnumerator StopRoutine(float fadeOutTime)
    {
        AudioSource sourceToStop = activeSource;

        if (sourceToStop == null || !sourceToStop.isPlaying)
        {
            fadeRoutine = null;
            yield break;
        }

        float duration = Mathf.Max(0f, fadeOutTime);
        float startVolume = sourceToStop.volume;

        if (duration <= 0f)
        {
            sourceToStop.Stop();
            sourceToStop.volume = 0f;
            fadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(elapsed / duration);

            sourceToStop.volume = Mathf.Lerp(startVolume, 0f, percent);

            yield return null;
        }

        sourceToStop.Stop();
        sourceToStop.volume = 0f;

        fadeRoutine = null;
    }

    private void ConfigureSource(AudioSource source, MusicTrackData track)
    {
        if (source == null || track == null)
            return;

        source.clip = track.musicClip;
        source.outputAudioMixerGroup = track.outputMixerGroup;
        source.loop = track.loop;
        source.pitch = Mathf.Max(0.01f, track.pitch);
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.ignoreListenerPause = track.ignoreListenerPause;
    }

    private void SwapSources()
    {
        AudioSource previousActive = activeSource;

        activeSource = inactiveSource;
        inactiveSource = previousActive;
    }

    private void CreateSources()
    {
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;

        sourceA.loop = true;
        sourceB.loop = true;

        activeSource = sourceA;
        inactiveSource = sourceB;
    }
}

//-----MusicManager.cs END-----