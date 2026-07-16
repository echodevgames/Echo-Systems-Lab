//-----AmbienceLoopData.cs START-----

using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(
    fileName = "AmbienceLoopData_NewLoop",
    menuName = "Echo Systems Lab/Audio/Ambience Loop Data")]
public class AmbienceLoopData : ScriptableObject
{
    [Header("Loop")]
    public AudioClip loopClip;
    public AudioMixerGroup outputMixerGroup;

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 1f;
    public Vector2 pitchRange = new Vector2(1f, 1f);
    public bool loop = true;
    public bool playOnStart = true;
    public bool ignoreListenerPause = false;

    [Header("Fades")]
    public float fadeInTime = 1f;
    public float fadeOutTime = 1f;

    [Header("Spatial")]
    [Range(0f, 1f)] public float spatialBlend = 0f;
    public float minDistance = 1f;
    public float maxDistance = 30f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    public float GetRandomPitch()
    {
        float min = Mathf.Min(pitchRange.x, pitchRange.y);
        float max = Mathf.Max(pitchRange.x, pitchRange.y);

        return Random.Range(min, max);
    }
}

//-----AmbienceLoopData.cs END-----