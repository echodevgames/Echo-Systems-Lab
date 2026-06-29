//-----WeaponAmmoHUD.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponAmmoHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponController weaponController;

    [Header("Weapon UI")]
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private Image weaponIconImage;

    [Header("Ammo Identity UI")]
    [SerializeField] private TMP_Text ammoNameText;
    [SerializeField] private Image ammoIconImage;

    [Header("Ammo Count UI")]
    [SerializeField] private TMP_Text clipAmmoText;
    [SerializeField] private TMP_Text reserveAmmoText;
    [SerializeField] private TMP_Text reloadPromptText;

    [Header("Display")]
    [SerializeField] private string noWeaponText = "No Weapon";
    [SerializeField] private string noAmmoText = "No Ammo";
    [SerializeField] private string infiniteReserveText = "∞";
    [SerializeField] private string reloadPromptMessage = "Press R to Reload";

    private void Awake()
    {
        FindReferencesIfNeeded();
    }

    private void OnEnable()
    {
        FindReferencesIfNeeded();

        if (weaponController != null)
            weaponController.OnWeaponAmmoChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (weaponController != null)
            weaponController.OnWeaponAmmoChanged -= Refresh;
    }

    private void FindReferencesIfNeeded()
    {
        if (weaponController == null)
            weaponController = FindFirstObjectByType<PlayerWeaponController>();
    }

    public void Refresh()
    {
        if (weaponController == null)
        {
            ShowNoWeapon();
            return;
        }

        WeaponData currentWeapon = weaponController.CurrentWeapon;

        if (currentWeapon == null)
        {
            ShowNoWeapon();
            return;
        }

        AmmoData currentAmmo = currentWeapon.defaultAmmo;

        RefreshWeaponName(currentWeapon);
        RefreshWeaponIcon(currentWeapon);
        RefreshAmmoIdentity(currentAmmo);
        RefreshAmmoCounts(currentWeapon);
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

        if (ammoNameText != null)
            ammoNameText.text = noAmmoText;

        if (ammoIconImage != null)
        {
            ammoIconImage.sprite = null;
            ammoIconImage.enabled = false;
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

    private void RefreshAmmoIdentity(AmmoData ammo)
    {
        if (ammoNameText != null)
        {
            if (ammo == null)
                ammoNameText.text = noAmmoText;
            else if (!string.IsNullOrWhiteSpace(ammo.hudDisplayName))
                ammoNameText.text = ammo.hudDisplayName;
            else if (!string.IsNullOrWhiteSpace(ammo.displayName))
                ammoNameText.text = ammo.displayName;
            else
                ammoNameText.text = ammo.ammoId;
        }

        if (ammoIconImage != null)
        {
            Sprite icon = ammo != null ? ammo.ammoIcon : null;

            ammoIconImage.sprite = icon;
            ammoIconImage.enabled = icon != null;
        }
    }

    private void RefreshAmmoCounts(WeaponData weapon)
    {
        if (clipAmmoText != null)
            clipAmmoText.text = $"{weaponController.CurrentAmmoInClip} / {weaponController.CurrentClipSize}";

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
        if (reloadPromptText == null || weaponController == null)
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