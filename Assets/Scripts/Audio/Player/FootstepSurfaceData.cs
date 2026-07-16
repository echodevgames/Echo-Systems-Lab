//-----FootstepSurfaceData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "FootstepSurfaceData_NewSurface",
    menuName = "Echo Systems Lab/Audio/Footstep Surface Data")]
public class FootstepSurfaceData : ScriptableObject
{
    [Header("Identity")]
    public string surfaceId = "Default";

    [Header("Footsteps")]
    public AudioEventData walkFootstepAudio;
    public AudioEventData runFootstepAudio;

    [Header("Jump / Land")]
    public AudioEventData jumpTakeoffAudio;
    public AudioEventData landingAudio;

    [Header("Timing")]
    public bool overrideStepIntervals = false;
    public float walkStepInterval = 0.45f;
    public float runStepInterval = 0.32f;

    public AudioEventData GetFootstepAudio(bool isRunning)
    {
        if (isRunning && runFootstepAudio != null)
            return runFootstepAudio;

        return walkFootstepAudio;
    }

    public float GetStepInterval(bool isRunning, float fallbackWalkInterval, float fallbackRunInterval)
    {
        if (!overrideStepIntervals)
            return isRunning ? fallbackRunInterval : fallbackWalkInterval;

        return isRunning
            ? Mathf.Max(0.05f, runStepInterval)
            : Mathf.Max(0.05f, walkStepInterval);
    }
}

//-----FootstepSurfaceData.cs END-----