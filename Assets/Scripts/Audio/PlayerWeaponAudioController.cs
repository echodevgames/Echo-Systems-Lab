//-----PlayerWeaponAudioController.cs START-----

using UnityEngine;

public class PlayerWeaponAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponController weaponController;

    [Header("Audio Toggles")]
    [SerializeField] private bool playEquipAudio = true;
    [SerializeField] private bool playFireAudio = true;
    [SerializeField] private bool playDryFireAudio = true;
    [SerializeField] private bool playReloadStartAudio = true;
    [SerializeField] private bool playReloadInsertAudio = true;
    [SerializeField] private bool playReloadEndAudio = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private void Awake()
    {
        if (weaponController == null)
            weaponController = GetComponent<PlayerWeaponController>();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (weaponController == null)
            return;

        weaponController.OnWeaponEquipped += HandleWeaponEquipped;
        weaponController.OnWeaponFired += HandleWeaponFired;
        weaponController.OnWeaponDryFired += HandleWeaponDryFired;
        weaponController.OnWeaponReloadStarted += HandleWeaponReloadStarted;
        weaponController.OnWeaponReloadInserted += HandleWeaponReloadInserted;
        weaponController.OnWeaponReloadEnded += HandleWeaponReloadEnded;
    }

    private void Unsubscribe()
    {
        if (weaponController == null)
            return;

        weaponController.OnWeaponEquipped -= HandleWeaponEquipped;
        weaponController.OnWeaponFired -= HandleWeaponFired;
        weaponController.OnWeaponDryFired -= HandleWeaponDryFired;
        weaponController.OnWeaponReloadStarted -= HandleWeaponReloadStarted;
        weaponController.OnWeaponReloadInserted -= HandleWeaponReloadInserted;
        weaponController.OnWeaponReloadEnded -= HandleWeaponReloadEnded;
    }

    private void HandleWeaponEquipped(WeaponData weapon)
    {
        if (!playEquipAudio)
            return;

        WeaponAudioData audioData = GetAudioData(weapon);

        if (audioData == null)
            return;

        audioData.PlayEquipAudio();
        LogPlayed(weapon, "equip");
    }

    private void HandleWeaponFired(WeaponData weapon)
    {
        if (!playFireAudio)
            return;

        WeaponAudioData audioData = GetAudioData(weapon);

        if (audioData == null)
            return;

        audioData.PlayFireAudio();
        LogPlayed(weapon, "fire");
    }

    private void HandleWeaponDryFired(WeaponData weapon)
    {
        if (!playDryFireAudio)
            return;

        WeaponAudioData audioData = GetAudioData(weapon);

        if (audioData == null)
            return;

        audioData.PlayDryFireAudio();
        LogPlayed(weapon, "dry fire");
    }

    private void HandleWeaponReloadStarted(WeaponData weapon)
    {
        if (!playReloadStartAudio)
            return;

        WeaponAudioData audioData = GetAudioData(weapon);

        if (audioData == null)
            return;

        audioData.PlayReloadStartAudio();
        LogPlayed(weapon, "reload start");
    }

    private void HandleWeaponReloadInserted(WeaponData weapon)
    {
        if (!playReloadInsertAudio)
            return;

        WeaponAudioData audioData = GetAudioData(weapon);

        if (audioData == null)
            return;

        audioData.PlayReloadInsertAudio();
        LogPlayed(weapon, "reload insert");
    }

    private void HandleWeaponReloadEnded(WeaponData weapon)
    {
        if (!playReloadEndAudio)
            return;

        WeaponAudioData audioData = GetAudioData(weapon);

        if (audioData == null)
            return;

        audioData.PlayReloadEndAudio();
        LogPlayed(weapon, "reload end");
    }

    private WeaponAudioData GetAudioData(WeaponData weapon)
    {
        if (weapon == null)
            return null;

        return weapon.audioData;
    }

    private void LogPlayed(WeaponData weapon, string eventName)
    {
        if (!debugLogs)
            return;

        string weaponName = weapon != null ? weapon.displayName : "No Weapon";
        Debug.Log($"PlayerWeaponAudioController played {eventName} audio for {weaponName}.");
    }
}

//-----PlayerWeaponAudioController.cs END-----