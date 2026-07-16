//-----MusicStarter.cs START-----

using System.Collections;
using UnityEngine;

public class MusicStarter : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private MusicTrackData trackToPlay;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool forceRestart = false;
    [SerializeField] private float startDelay = 0f;

    [Header("Stop")]
    [SerializeField] private bool stopMusicOnDisable = false;
    [SerializeField] private float stopFadeOutTime = 1f;

    private Coroutine startRoutine;

    private void Start()
    {
        if (playOnStart)
            Play();
    }

    private void OnDisable()
    {
        if (stopMusicOnDisable && MusicManager.Instance != null)
            MusicManager.Instance.StopMusic(stopFadeOutTime);
    }

    public void Play()
    {
        if (startRoutine != null)
            StopCoroutine(startRoutine);

        startRoutine = StartCoroutine(PlayRoutine());
    }

    public void Stop()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusic(stopFadeOutTime);
    }

    private IEnumerator PlayRoutine()
    {
        if (startDelay > 0f)
            yield return new WaitForSecondsRealtime(startDelay);

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayTrack(trackToPlay, forceRestart);

        startRoutine = null;
    }
}

//-----MusicStarter.cs END-----