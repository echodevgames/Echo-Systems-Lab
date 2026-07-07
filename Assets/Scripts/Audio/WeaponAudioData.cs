//-----WeaponAudioData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponAudioData_NewWeapon",
    menuName = "Echo Systems Lab/Weapons/Weapon Audio Data")]
public class WeaponAudioData : ScriptableObject
{
    [Header("Core Weapon Audio")]
    public AudioEventData fireAudio;
    public AudioEventData dryFireAudio;
    public AudioEventData equipAudio;

    [Header("Reload Audio")]
    public AudioEventData reloadStartAudio;
    public AudioEventData reloadInsertAudio;
    public AudioEventData reloadEndAudio;

    [Header("Optional Mechanical Audio")]
    public AudioEventData chamberAudio;
    public AudioEventData shellEjectAudio;
    public AudioEventData safetyClickAudio;

    [Header("Debug")]
    public bool debugLogs;

    public void PlayFireAudio()
    {
        PlayAudioEvent(fireAudio, "fire");
    }

    public void PlayDryFireAudio()
    {
        PlayAudioEvent(dryFireAudio, "dry fire");
    }

    public void PlayEquipAudio()
    {
        PlayAudioEvent(equipAudio, "equip");
    }

    public void PlayReloadStartAudio()
    {
        PlayAudioEvent(reloadStartAudio, "reload start");
    }

    public void PlayReloadInsertAudio()
    {
        PlayAudioEvent(reloadInsertAudio, "reload insert");
    }

    public void PlayReloadEndAudio()
    {
        PlayAudioEvent(reloadEndAudio, "reload end");
    }

    public void PlayChamberAudio()
    {
        PlayAudioEvent(chamberAudio, "chamber");
    }

    public void PlayShellEjectAudio()
    {
        PlayAudioEvent(shellEjectAudio, "shell eject");
    }

    public void PlaySafetyClickAudio()
    {
        PlayAudioEvent(safetyClickAudio, "safety click");
    }

    private void PlayAudioEvent(AudioEventData audioEvent, string eventLabel)
    {
        if (audioEvent == null)
            return;

        if (GameAudioManager.Instance == null)
        {
            Debug.LogWarning($"Tried to play {eventLabel} audio, but no GameAudioManager exists.");
            return;
        }

        GameAudioManager.Instance.PlayOneShot(audioEvent);

        if (debugLogs)
            Debug.Log($"{name} played {eventLabel} audio event: {audioEvent.name}");
    }
}

//-----WeaponAudioData.cs END-----