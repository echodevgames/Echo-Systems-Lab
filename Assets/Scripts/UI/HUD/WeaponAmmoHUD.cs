//-----WeaponAmmoHUD.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponAmmoHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponController weaponController;

    [Header("Weapon UI")]
    //Weapon
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private Image weaponIconImage;
    //Ammo
    [SerializeField] private TMP_Text ammoNameText;
    [SerializeField] private Image ammoIconImage;

    [Header("Ammo UI")]
    [SerializeField] private TMP_Text clipAmmoText;
    [SerializeField] private TMP_Text reserveAmmoText;
    [SerializeField] private TMP_Text reloadPromptText;

    [Header("Display")]
    [SerializeField] private string noWeaponText = "No Weapon";
    [SerializeField] private string infiniteReserveText = "∞";
    [SerializeField] private string reloadPromptMessage = "Press R to Reload";

    private void Awake()
    {
        if (weaponController == null)
            weaponController = FindFirstObjectByType<PlayerWeaponController>();
    }

    private void OnEnable()
    {
        if (weaponController != null)
            weaponController.OnWeaponAmmoChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (weaponController != null)
            weaponController.OnWeaponAmmoChanged -= Refresh;
    }

    private void Update()
    {
        // Tiny safety net for reload state changes during coroutine timing.
        Refresh();
    }

    public void Refresh()
    {
        if (weaponController == null)
        {
            ShowNoWeapon();
            return;
        }

        WeaponData currentWeapon = weaponController.CurrentWeapon;
        AmmoData currentAmmo = currentWeapon.defaultAmmo;

        if (currentWeapon == null)
        {
            ShowNoWeapon();
            return;
        }

        RefreshWeaponName(currentWeapon);
        RefreshWeaponIcon(currentWeapon);
        RefreshAmmo(currentWeapon);
        RefreshReloadPrompt();
    }

    private void ShowNoWeapon()
    {
        if (weaponNameText != null)
            weaponNameText.text = noWeaponText;

        if (weaponIconImage != null)
        {
            weaponIconImage.sprite = null;
            weaponIconImage.enabled = false;
        }

        if (clipAmmoText != null)
            clipAmmoText.text = "-- / --";

        if (reserveAmmoText != null)
            reserveAmmoText.text = "Reserve: --";

        if (reloadPromptText != null)
            reloadPromptText.gameObject.SetActive(false);
    }

    private void RefreshWeaponName(WeaponData weapon)
    {
        if (weaponNameText == null)
            return;

        if (!string.IsNullOrWhiteSpace(weapon.hudDisplayName))
            weaponNameText.text = weapon.hudDisplayName;
        else if (!string.IsNullOrWhiteSpace(weapon.displayName))
            weaponNameText.text = weapon.displayName;
        else
            weaponNameText.text = weapon.weaponId;
    }

    private void RefreshWeaponIcon(WeaponData weapon)
    {
        if (weaponIconImage == null)
            return;

        weaponIconImage.sprite = weapon.weaponIcon;
        weaponIconImage.enabled = weapon.weaponIcon != null;
    }

    private void RefreshAmmo(WeaponData weapon)
    {
        if (clipAmmoText != null)
        {
            clipAmmoText.text =
                $"{weaponController.CurrentAmmoInClip} / {weaponController.CurrentClipSize}";
        }

        if (reserveAmmoText != null)
        {
            if (weapon.infiniteReserveAmmo)
                reserveAmmoText.text = $"Reserve: {infiniteReserveText}";
            else
                reserveAmmoText.text = $"Reserve: {weaponController.CurrentReserveAmmo}";
        }
    }

    private void RefreshReloadPrompt()
    {
        if (reloadPromptText == null)
            return;

        bool shouldShowPrompt =
            weaponController.CurrentWeapon != null &&
            !weaponController.IsReloading &&
            weaponController.CurrentAmmoInClip <= 0;

        reloadPromptText.gameObject.SetActive(shouldShowPrompt);

        if (shouldShowPrompt)
            reloadPromptText.text = reloadPromptMessage;
    }
}

//-----WeaponAmmoHUD.cs END-----