# Milestone 3C - Weapon Ammo HUD and Ammo Pickup Foundation

## 2. Goal Line

```text
Player equips weapon
-> HUD shows weapon name and weapon icon
-> HUD shows active ammo type and ammo icon
-> HUD shows current clip ammo / clip size
-> HUD shows reserve ammo
-> firing updates HUD
-> reloading updates HUD
-> ammo pickups update reserve ammo
-> empty clip gives visible reload prompt
```

---

## 3. Naming / Scope Notes

Use this split:

```text
WeaponData = weapon identity, icon, clip size, reload time
AmmoData = ammo identity, projectile, damage, caliber, ammo icon
AmmoPickupData = pickup box definition
PlayerAmmoInventory = reserve ammo storage
PlayerWeaponController = current weapon, clip ammo, reload state
WeaponAmmoHUD = visual display only
```

HUD should not decide weapon logic.

HUD should only listen and display.

This milestone is also preparing for future ammo swapping:

```text
Regular Ammo
Incendiary Ammo
Armor Piercing Ammo
Explosive Ammo
Stun Ammo
```

For now, each weapon uses its `defaultAmmo`, but the HUD should already be able to show whichever ammo is currently active.

---

## 4. System Pieces

```text
1. WeaponData HUD icon field
2. AmmoData HUD icon field
3. AmmoPickupData ScriptableObject
4. AmmoPickup interactable prefab
5. PlayerAmmoInventory reserve ammo storage
6. PlayerWeaponController clip / reload / reserve connection
7. WeaponAmmoHUD script
8. Weapon icon display
9. Ammo icon display
10. Reload prompt display
11. TargetRangeTrial scene test
```

---

## 5. Folder Setup

Create or confirm these folders:

```text
Assets/Scripts/Weapons/
Assets/Scripts/UI/HUD/
Assets/ScriptableObjects/Ammo/
Assets/ScriptableObjects/Pickups/
Assets/Prefabs/Pickups/
Assets/Prefabs/UI/HUD/
Assets/Textures/UI/Icons/Weapons/
Assets/Textures/UI/Icons/Ammo/
```

---

## 6. Numbered Implementation Steps

## Step 1 - Update WeaponData HUD Fields

### File Path

```text
Assets/Scripts/Weapons/WeaponData.cs
```

### Script Name

```text
WeaponData.cs
```

### Required HUD Fields

```csharp
[Header("HUD")]
public string displayName;
public string hudDisplayName;
public Sprite weaponIcon;
```

### Why This Step Matters

This lets the HUD show a clean weapon name and a weapon icon without hardcoding UI behavior into the weapon controller.

---

## Step 2 - Update AmmoData HUD Fields

### File Path

```text
Assets/Scripts/Weapons/AmmoData.cs
```

### Script Name

```text
AmmoData.cs
```

### Full Script

```csharp
//-----AmmoData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "AmmoData_NewAmmo",
    menuName = "Echo Systems Lab/Weapons/Ammo Data")]
public class AmmoData : ScriptableObject
{
    [Header("Identity")]
    public string ammoId;
    public string displayName;
    public string caliberLabel;

    [TextArea(2, 4)]
    public string description;

    [Header("HUD")]
    public string hudDisplayName;
    public Sprite ammoIcon;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public int damage = 10;
    public float projectileSpeed = 30f;
    public float projectileLifetime = 10f;

    [Header("Progression")]
    public int xpPerUse = 10;
}

//-----AmmoData.cs END-----
```

### Why This Step Matters

Ammo is its own data identity. The HUD should display ammo visually because later one weapon may support multiple ammo types.

---

## Step 3 - Create AmmoPickupData

### File Path

```text
Assets/Scripts/Weapons/AmmoPickupData.cs
```

### Script Name

```text
AmmoPickupData.cs
```

```csharp
//-----AmmoPickupData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "AmmoPickupData_NewPickup",
    menuName = "Echo Systems Lab/Weapons/Ammo Pickup Data")]
public class AmmoPickupData : ScriptableObject
{
    [Header("Identity")]
    public string pickupId;
    public string displayName;

    [TextArea(2, 4)]
    public string description;

    [Header("Ammo")]
    public AmmoData ammoData;
    public int amount = 24;

    [Header("Pickup Behavior")]
    public bool destroyOnPickup = true;
}

//-----AmmoPickupData.cs END-----
```

### Inspector Setup

Create:

```text
Assets/ScriptableObjects/Pickups/AmmoPickupData_9mm_Box
```

Suggested values:

```text
Pickup Id: ammo_pickup_9mm_box
Display Name: 9mm Ammo Box
Ammo Data: AmmoData_9mm
Amount: 48
Destroy On Pickup: true
```

---

## Step 4 - Confirm PlayerAmmoInventory

### File Path

```text
Assets/Scripts/Weapons/PlayerAmmoInventory.cs
```

### Script Name

```text
PlayerAmmoInventory.cs
```

### Why This Step Matters

Reserve ammo belongs to the player, not to WeaponData or AmmoData.

The player should own ammo reserves like:

```text
ammo_9mm = 48
ammo_45 = 24
ammo_12gauge = 12
ammo_bolt = 10
```

---

## Step 5 - Confirm PlayerWeaponController Ammo Runtime

### File Path

```text
Assets/Scripts/Weapons/PlayerWeaponController.cs
```

### Script Name

```text
PlayerWeaponController.cs
```

### Required Runtime Data

```text
currentWeapon
currentAmmoInClip
isReloading
CurrentReserveAmmo
OnWeaponAmmoChanged
```

### Why This Step Matters

PlayerWeaponController bridges weapon data, ammo data, clip ammo, reserve ammo, projectile spawning, reloads, and the future HUD.

---

## Step 6 - Create / Update AmmoPickup

### File Path

```text
Assets/Scripts/Weapons/AmmoPickup.cs
```

### Script Name

```text
AmmoPickup.cs
```

```csharp
//-----AmmoPickup.cs START-----

using UnityEngine;

public class AmmoPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private AmmoPickupData pickupData;
    [SerializeField] private string promptPrefix = "Press E to pick up";

    public string GetPromptText()
    {
        if (pickupData == null)
            return "No ammo assigned";

        return $"{promptPrefix} {pickupData.displayName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (pickupData == null)
        {
            Debug.LogWarning($"{name} has no AmmoPickupData assigned.");
            return;
        }

        if (pickupData.ammoData == null)
        {
            Debug.LogWarning($"{pickupData.displayName} has no AmmoData assigned.");
            return;
        }

        if (interactor == null)
        {
            Debug.LogWarning($"{name} was interacted with, but no PlayerInteractor was provided.");
            return;
        }

        PlayerAmmoInventory ammoInventory = interactor.GetComponent<PlayerAmmoInventory>();

        if (ammoInventory == null)
        {
            Debug.LogWarning("Interactor does not have PlayerAmmoInventory.");
            return;
        }

        ammoInventory.AddAmmo(pickupData.ammoData, pickupData.amount);

        Debug.Log($"Picked up {pickupData.amount} {pickupData.ammoData.displayName} from {pickupData.displayName}.");

        if (pickupData.destroyOnPickup)
            Destroy(gameObject);
    }
}

//-----AmmoPickup.cs END-----
```

---

## Step 7 - Create WeaponAmmoHUD

### File Path

```text
Assets/Scripts/UI/HUD/WeaponAmmoHUD.cs
```

### Script Name

```text
WeaponAmmoHUD.cs
```

```csharp
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

    [Header("Ammo UI")]
    [SerializeField] private TMP_Text ammoNameText;
    [SerializeField] private TMP_Text clipAmmoText;
    [SerializeField] private TMP_Text reserveAmmoText;
    [SerializeField] private TMP_Text reloadPromptText;
    [SerializeField] private Image ammoIconImage;

    [Header("Display")]
    [SerializeField] private string noWeaponText = "No Weapon";
    [SerializeField] private string noAmmoText = "No Ammo";
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

        if (currentWeapon == null)
        {
            ShowNoWeapon();
            return;
        }

        AmmoData currentAmmo = currentWeapon.defaultAmmo;

        RefreshWeaponName(currentWeapon);
        RefreshWeaponIcon(currentWeapon);
        RefreshAmmoName(currentAmmo);
        RefreshAmmoIcon(currentAmmo);
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

    private void RefreshAmmoName(AmmoData ammo)
    {
        if (ammoNameText == null)
            return;

        if (ammo == null)
        {
            ammoNameText.text = noAmmoText;
            return;
        }

        if (!string.IsNullOrWhiteSpace(ammo.hudDisplayName))
            ammoNameText.text = ammo.hudDisplayName;
        else if (!string.IsNullOrWhiteSpace(ammo.caliberLabel))
            ammoNameText.text = ammo.caliberLabel;
        else if (!string.IsNullOrWhiteSpace(ammo.displayName))
            ammoNameText.text = ammo.displayName;
        else
            ammoNameText.text = ammo.ammoId;
    }

    private void RefreshAmmoIcon(AmmoData ammo)
    {
        if (ammoIconImage == null)
            return;

        ammoIconImage.sprite = ammo != null ? ammo.ammoIcon : null;
        ammoIconImage.enabled = ammo != null && ammo.ammoIcon != null;
    }

    private void RefreshAmmo(WeaponData weapon)
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
```

---

## Step 8 - HUD Scene Setup

Inside your gameplay HUD canvas:

```text
Canvas_HUD
└── WeaponAmmoHUDRoot
    ├── WeaponIconImage
    ├── WeaponNameText
    ├── AmmoIconImage
    ├── AmmoNameText
    ├── ClipAmmoText
    ├── ReserveAmmoText
    └── ReloadPromptText
```

Suggested layout:

```text
Bottom Right:
[Weapon Icon]  Pistol
[Ammo Icon]    9mm
               16 / 16
               Reserve: 48
               Press R to Reload
```

---

## Step 9 - WeaponData / AmmoData HUD Setup

### WeaponData_Pistol

```text
weaponId = pistol_basic
displayName = Pistol
hudDisplayName = Pistol
weaponIcon = pistol sprite
defaultAmmo = AmmoData_9mm
clipSize = 16
infiniteReserveAmmo = false
```

### AmmoData_9mm

```text
ammoId = ammo_9mm
displayName = 9mm Ammo
hudDisplayName = 9mm
caliberLabel = 9mm
ammoIcon = 9mm ammo sprite
projectilePrefab = Projectile_9mm
damage = 10
projectileSpeed = 30
projectileLifetime = 10
xpPerUse = 10
```

---

# 7. Unity Setup Checklist

## Player

```text
Player
- PlayerInputReader
- SimpleFirstPersonController
- PlayerInteractor
- PlayerWeaponController
- PlayerWeaponLoadoutController
- PlayerAmmoInventory
```

## HUD

```text
WeaponAmmoHUDRoot
- WeaponAmmoHUD script
- Weapon Controller assigned
- Weapon Name Text assigned
- Weapon Icon Image assigned
- Ammo Name Text assigned
- Ammo Icon Image assigned
- Clip Ammo Text assigned
- Reserve Ammo Text assigned
- Reload Prompt Text assigned
```

## Ammo Pickup

```text
AmmoBox_9mm
- Collider
- Layer: Interactable
- AmmoPickup
  - Pickup Data = AmmoPickupData_9mm_Box
```

---

# 8. Goal Line / Completion Checklist

```text
1. Player equips weapon.
2. HUD shows weapon name.
3. HUD shows weapon icon.
4. HUD shows active ammo name.
5. HUD shows active ammo icon.
6. HUD shows current clip ammo / clip size.
7. HUD shows reserve ammo.
8. Firing updates clip ammo.
9. Empty clip blocks firing.
10. Empty clip shows reload prompt.
11. Reload updates clip ammo.
12. Reload consumes reserve ammo when infinite reserve is false.
13. Ammo pickup adds reserve ammo.
14. Ammo pickup disappears after pickup.
15. No compile errors.
16. No input conflicts with pause or mission terminal.
```

---

# 9. Suggested Commit

```bash
git add .
git commit -m "Add weapon ammo HUD and ammo pickup foundation"
```

Optional tag:

```bash
git tag milestone-3c-weapon-ammo-hud
```

---

# 10. Design Notes

This is the correct architecture split:

```text
WeaponData says:
- What weapon is this?
- What icon does it use?
- How big is its clip?
- What ammo does it fire by default?

AmmoData says:
- What ammo type is this?
- What icon does it use?
- What projectile does it spawn?
- How much damage does it deal?
- What caliber/type is it?

PlayerAmmoInventory says:
- How much reserve ammo does the player have?

PlayerWeaponController says:
- What weapon is equipped?
- How much ammo is currently loaded?
- Is the weapon reloading?

WeaponAmmoHUD says:
- What should the player see?
```

Ammo icons are included now because later weapons will support ammo type swapping:

```text
Pistol + Regular 9mm
Pistol + Incendiary 9mm
Pistol + Armor Piercing 9mm
Shotgun + Buckshot
Shotgun + Slugs
Crossbow + Regular Bolts
Crossbow + Fire Bolts
```

---

# 11. Portfolio Value

This checkpoint demonstrates:

```text
ScriptableObject-driven HUD data
Weapon icon display
Ammo icon display
Runtime ammo tracking
Clip and reserve ammo UI
Event-driven UI refresh
Interactable pickup architecture
Separation of gameplay state from visual display
Expandable weapon/ammo inventory foundations
Future-ready ammo type swapping
```
