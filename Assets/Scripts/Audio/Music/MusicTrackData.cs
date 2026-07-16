//-----MusicTrackData.cs START-----

using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(
    fileName = "MusicTrackData_NewTrack",
    menuName = "Echo Systems Lab/Audio/Music Track Data")]
public class MusicTrackData : ScriptableObject
{
    [Header("Track")]
    public AudioClip musicClip;
    public AudioMixerGroup outputMixerGroup;

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 1f;
    public float pitch = 1f;
    public bool loop = true;
    public bool restartIfAlreadyPlaying = false;
    public bool ignoreListenerPause = false;

    [Header("Fades")]
    public float fadeInTime = 1f;
    public float fadeOutTime = 1f;
}

//-----MusicTrackData.cs END-----