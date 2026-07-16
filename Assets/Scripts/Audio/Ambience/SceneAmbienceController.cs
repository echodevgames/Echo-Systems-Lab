//-----SceneAmbienceController.cs START-----

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAmbienceController : MonoBehaviour
{
    [Header("Ambience Layers")]
    [SerializeField] private AmbienceLoopData[] ambienceLayers;

    [Header("Playback")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool stopOnDisable = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private readonly List<AudioSource> activeSources = new List<AudioSource>();
    private readonly List<Coroutine> fadeRoutines = new List<Coroutine>();

    private void Start()
    {
        if (playOnStart)
            PlayAll();
    }

    private void OnDisable()
    {
        if (stopOnDisable)
            StopAllImmediate();
    }

    public void PlayAll()
    {
        StopAllImmediate();

        if (ambienceLayers == null)
            return;

        foreach (AmbienceLoopData ambienceData in ambienceLayers)
        {
            if (ambienceData == null)
                continue;

            if (!ambienceData.playOnStart)
                continue;

            PlayLayer(ambienceData);
        }
    }

    public void StopAll()
    {
        foreach (Coroutine routine in fadeRoutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        fadeRoutines.Clear();

        foreach (AudioSource source in activeSources)
        {
            if (source == null)
                continue;

            Coroutine routine = StartCoroutine(FadeOutAndDestroyRoutine(source, GetFadeOutTime(source)));
            fadeRoutines.Add(routine);
        }

        activeSources.Clear();
    }

    public void StopAllImmediate()
    {
        foreach (Coroutine routine in fadeRoutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        fadeRoutines.Clear();

        foreach (AudioSource source in activeSources)
        {
            if (source != null)
                Destroy(source.gameObject);
        }

        activeSources.Clear();
    }

    private void PlayLayer(AmbienceLoopData ambienceData)
    {
        if (ambienceData == null || ambienceData.loopClip == null)
            return;

        GameObject audioObject = new GameObject($"Ambience_{ambienceData.name}");
        audioObject.transform.SetParent(transform);
        audioObject.transform.localPosition = Vector3.zero;
        audioObject.transform.localRotation = Quaternion.identity;

        AudioSource source = audioObject.AddComponent<AudioSource>();

        source.clip = ambienceData.loopClip;
        source.outputAudioMixerGroup = ambienceData.outputMixerGroup;
        source.loop = ambienceData.loop;
        source.pitch = Mathf.Max(0.01f, ambienceData.GetRandomPitch());
        source.volume = 0f;
        source.playOnAwake = false;
        source.ignoreListenerPause = ambienceData.ignoreListenerPause;

        source.spatialBlend = Mathf.Clamp01(ambienceData.spatialBlend);
        source.minDistance = Mathf.Max(0f, ambienceData.minDistance);
        source.maxDistance = Mathf.Max(source.minDistance, ambienceData.maxDistance);
        source.rolloffMode = ambienceData.rolloffMode;

        source.Play();

        activeSources.Add(source);

        Coroutine routine = StartCoroutine(FadeInRoutine(source, ambienceData.volume, ambienceData.fadeInTime));
        fadeRoutines.Add(routine);

        if (debugLogs)
            Debug.Log($"SceneAmbienceController started ambience layer: {ambienceData.name}");
    }

    private IEnumerator FadeInRoutine(AudioSource source, float targetVolume, float fadeTime)
    {
        if (source == null)
            yield break;

        float duration = Mathf.Max(0f, fadeTime);
        float target = Mathf.Clamp01(targetVolume);

        if (duration <= 0f)
        {
            source.volume = target;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration && source != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(elapsed / duration);

            source.volume = Mathf.Lerp(0f, target, percent);

            yield return null;
        }

        if (source != null)
            source.volume = target;
    }

    private IEnumerator FadeOutAndDestroyRoutine(AudioSource source, float fadeTime)
    {
        if (source == null)
            yield break;

        float duration = Mathf.Max(0f, fadeTime);
        float startVolume = source.volume;

        if (duration <= 0f)
        {
            Destroy(source.gameObject);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration && source != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(elapsed / duration);

            source.volume = Mathf.Lerp(startVolume, 0f, percent);

            yield return null;
        }

        if (source != null)
            Destroy(source.gameObject);
    }

    private float GetFadeOutTime(AudioSource source)
    {
        if (source == null || ambienceLayers == null)
            return 1f;

        foreach (AmbienceLoopData ambienceData in ambienceLayers)
        {
            if (ambienceData == null || ambienceData.loopClip == null)
                continue;

            if (source.clip == ambienceData.loopClip)
                return ambienceData.fadeOutTime;
        }

        return 1f;
    }
}

//-----SceneAmbienceController.cs END-----