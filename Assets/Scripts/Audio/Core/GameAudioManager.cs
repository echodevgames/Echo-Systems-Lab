//-----GameAudioManager.cs START-----

using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Lifetime")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Fallback Routing")]
    [Tooltip("Used if an AudioEventData does not have an output mixer group assigned.")]
    [SerializeField] private AudioMixerGroup fallbackMixerGroup;

    [Header("One Shot Cleanup")]
    [SerializeField] private float cleanupPaddingSeconds = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Transform oneShotRoot;

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

        CreateOneShotRoot();
    }

    public AudioSource PlayOneShot(AudioEventData audioEvent)
    {
        return PlayOneShotInternal(
            audioEvent,
            transform.position,
            null,
            false);
    }

    public AudioSource PlayOneShotAtPosition(AudioEventData audioEvent, Vector3 worldPosition)
    {
        return PlayOneShotInternal(
            audioEvent,
            worldPosition,
            null,
            true);
    }

    public AudioSource PlayOneShotAttached(AudioEventData audioEvent, Transform parent)
    {
        Vector3 position = parent != null
            ? parent.position
            : transform.position;

        return PlayOneShotInternal(
            audioEvent,
            position,
            parent,
            parent != null);
    }

    private AudioSource PlayOneShotInternal(
        AudioEventData audioEvent,
        Vector3 worldPosition,
        Transform parent,
        bool useWorldPosition)
    {
        if (audioEvent == null)
            return null;

        if (!audioEvent.TryGetClip(out AudioClip clip))
            return null;

        GameObject audioObject = new GameObject($"AudioEvent_{audioEvent.name}");

        if (parent != null)
        {
            audioObject.transform.SetParent(parent);
            audioObject.transform.localPosition = Vector3.zero;
            audioObject.transform.localRotation = Quaternion.identity;
        }
        else
        {
            audioObject.transform.SetParent(oneShotRoot);
            audioObject.transform.position = useWorldPosition ? worldPosition : transform.position;
            audioObject.transform.rotation = Quaternion.identity;
        }

        AudioSource source = audioObject.AddComponent<AudioSource>();

        audioEvent.ApplyToAudioSource(source, fallbackMixerGroup);

        source.clip = clip;
        source.Play();

        float duration = audioEvent.GetPlaybackDuration(clip, source.pitch);
        StartCoroutine(DestroyAfterPlayback(audioObject, duration + cleanupPaddingSeconds));

        if (debugLogs)
            Debug.Log($"Played audio event: {audioEvent.name} / Clip: {clip.name}");

        return source;
    }

    private IEnumerator DestroyAfterPlayback(GameObject audioObject, float delaySeconds)
    {
        if (delaySeconds > 0f)
            yield return new WaitForSecondsRealtime(delaySeconds);

        if (audioObject != null)
            Destroy(audioObject);
    }

    private void CreateOneShotRoot()
    {
        if (oneShotRoot != null)
            return;

        GameObject rootObject = new GameObject("OneShotAudioRoot");
        rootObject.transform.SetParent(transform);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.identity;

        oneShotRoot = rootObject.transform;
    }
}

//-----GameAudioManager.cs END-----