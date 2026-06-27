---
title: Milestone 2C - Multi-Weapon Loadout, Ammo Data, and Target Range Arsenal
project: Echo Systems Lab
type: checkpoint-build-plan
tags:
  - unity
  - systems-design
  - weapons
  - ammo
  - loadout
  - target-range
  - portfolio
---

# Milestone 2C - Multi-Weapon Loadout, Ammo Data, and Target Range Arsenal

This milestone polishes the Target Range Trial into a stronger semi-finished systems showcase.

The current project already supports:

```text
Mission terminal flow
Save/load persistence
Replayable missions
Persistent pistol ownership
Persistent active weapon
Pedestal state based on ownership
Save notification UI
Editor-safe quit behavior
Reusable mission terminal prefab
```

This next phase expands the single-pistol prototype into a multi-weapon arsenal with ammo data, weapon cycling, and individual pedestals.

---

# Goal Line

```text
Player enters TargetRangeTrial
→ Multiple weapon pedestals are available
→ Player can collect Revolver, Crossbow, Shotgun, Assault Rifle, and Sniper Rifle
→ Each weapon is saved as owned
→ Each pedestal hides its weapon once owned
→ Player can press Tab to cycle owned weapons
→ Active weapon persists through scenes and reloads
→ Each weapon uses its own WeaponData
→ Each weapon uses its own AmmoData
→ Each weapon has its own view model and MuzzlePoint
→ Target Range becomes a multi-weapon systems demo
```

---

# Naming / Scope Notes

This milestone is **ranged weapons only**.

Add:

```text
Revolver
Crossbow
Shotgun
Assault Rifle
Sniper Rifle
```

Skip for now:

```text
Baseball bat
Hockey stick
Axe
Knife
```

Those will become a melee/combat systems milestone later.

---

# Current Design Direction

## WeaponData = the weapon

Examples:

```text
WeaponData_Pistol
WeaponData_Revolver
WeaponData_Crossbow
WeaponData_Shotgun
WeaponData_AssaultRifle
WeaponData_SniperRifle
```

## AmmoData = the projectile/ammo behavior

Examples:

```text
AmmoData_9mm
AmmoData_45Caliber
AmmoData_CrossbowBolt
AmmoData_12GaugeBuckshot
AmmoData_556Rifle
AmmoData_308Sniper
```

## Projectile prefab = runtime object

For now, all ammo can use the same projectile prefab/model:

```text
Projectile_PistolRound
```

Later, each ammo type can get unique visuals:

```text
Projectile_9mm
Projectile_45Caliber
Projectile_Bolt
Projectile_BuckshotPellet
Projectile_556
Projectile_308
```

---

# System Pieces

```text
1. AmmoData ScriptableObject
2. WeaponData upgrade for ammo, spread, pellets, automatic fire
3. WeaponDatabase helper methods
4. PlayerWeaponController upgrade for AmmoData and shotgun-style spread
5. PlayerWeaponLoadoutController for Tab cycling
6. Weapon ownership/save compatibility
7. New WeaponData assets
8. New AmmoData assets
9. View model prefabs with MuzzlePoint
10. Weapon pedestal setup
11. HUD/loadout validation
12. Save/load/re-enter test flow
```

---

# Folder Setup

Create or confirm these folders:

```text
Assets/_EchoSystemsLab/Scripts/Weapons/
Assets/_EchoSystemsLab/ScriptableObjects/Weapons/
Assets/_EchoSystemsLab/ScriptableObjects/Ammo/
Assets/_EchoSystemsLab/Prefabs/Weapons/ViewModels/
Assets/_EchoSystemsLab/Prefabs/Projectiles/
Assets/_EchoSystemsLab/Prefabs/Pedestals/
```

---

# Step 0 - Quick SaveManager Load Event Fix

In `SaveManager.LoadGame()`, after this line:

```csharp
Debug.Log($"Game loaded from: {SavePath}");
```

add:

```csharp
OnGameLoaded?.Invoke();
```

The end of successful `LoadGame()` should become:

```csharp
MissionProgress.LoadFromSaveData(currentSaveData);
PlayerProgress.LoadFromSaveData(currentSaveData);

Debug.Log($"Game loaded from: {SavePath}");

OnGameLoaded?.Invoke();

return true;
```

This makes the save notification system correctly show `Loaded` when loading an actual save file.

---

# Step 1 - Create AmmoData

Create:

```text
Assets/_EchoSystemsLab/Scripts/Weapons/AmmoData.cs
```

```csharp
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

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public int damage = 1;
    public float projectileSpeed = 30f;
    public float projectileLifetime = 4f;

    [Header("Progression")]
    public int xpPerUse = 1;
}
```

This keeps ammo simple for now.

Later, `AmmoData` can expand into:

```text
Armor piercing
Incendiary
Explosive
Poison
Stun
Piercing count
Ricochet count
Impact effects
Tracer visuals
```

---

# Step 2 - Upgrade WeaponData

Replace `WeaponData.cs` with this expanded version:

```csharp
using UnityEngine;

public enum WeaponFireMode
{
    Projectile,
    Hitscan
}

[CreateAssetMenu(
    fileName = "WeaponData_NewWeapon",
    menuName = "Echo Systems Lab/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponId;
    public string displayName;
    public string weaponType = "Pistol";

    [TextArea(2, 4)]
    public string description;

    [Header("Visuals")]
    public GameObject viewModelPrefab;
    public Vector3 viewLocalPosition;
    public Vector3 viewLocalEulerAngles;
    public Vector3 viewLocalScale = Vector3.one;

    [Header("Firing")]
    public WeaponFireMode fireMode = WeaponFireMode.Projectile;
    public AmmoData defaultAmmo;
    public float fireRate = 0.35f;
    public bool isAutomatic;

    [Header("Projectile Pattern")]
    public int projectilesPerShot = 1;
    public float spreadAngle = 0f;

    [Header("Progression")]
    public int xpPerUse = 1;
}
```

Important changes:

```text
WeaponData no longer owns projectile speed/damage/lifetime directly.
AmmoData owns projectile behavior.
WeaponData owns firing behavior.
```

So:

```text
Shotgun = many projectiles, wide spread
Sniper Rifle = one projectile, high damage ammo, slow fire rate
Assault Rifle = automatic, fast fire rate
Crossbow = one projectile, slower projectile, slow fire rate
Revolver = one projectile, high damage, medium fire rate
```

---

# Step 3 - Create AmmoData Assets

Create these ScriptableObjects:

```text
Assets/_EchoSystemsLab/ScriptableObjects/Ammo/AmmoData_9mm.asset
Assets/_EchoSystemsLab/ScriptableObjects/Ammo/AmmoData_45Caliber.asset
Assets/_EchoSystemsLab/ScriptableObjects/Ammo/AmmoData_CrossbowBolt.asset
Assets/_EchoSystemsLab/ScriptableObjects/Ammo/AmmoData_12GaugeBuckshot.asset
Assets/_EchoSystemsLab/ScriptableObjects/Ammo/AmmoData_556Rifle.asset
Assets/_EchoSystemsLab/ScriptableObjects/Ammo/AmmoData_308Sniper.asset
```

Suggested first-pass values:

```text
AmmoData_9mm
Ammo Id: ammo_9mm
Display Name: 9mm Round
Caliber Label: 9mm
Projectile Prefab: Projectile_PistolRound
Damage: 1
Projectile Speed: 35
Projectile Lifetime: 4
XP Per Use: 1
```

```text
AmmoData_45Caliber
Ammo Id: ammo_45
Display Name: .45 Round
Caliber Label: .45
Projectile Prefab: Projectile_PistolRound
Damage: 2
Projectile Speed: 34
Projectile Lifetime: 4
XP Per Use: 1
```

```text
AmmoData_CrossbowBolt
Ammo Id: ammo_crossbow_bolt
Display Name: Crossbow Bolt
Caliber Label: Bolt
Projectile Prefab: Projectile_PistolRound for now
Damage: 3
Projectile Speed: 26
Projectile Lifetime: 5
XP Per Use: 1
```

```text
AmmoData_12GaugeBuckshot
Ammo Id: ammo_12g_buckshot
Display Name: 12 Gauge Buckshot
Caliber Label: 12 Gauge
Projectile Prefab: Projectile_PistolRound
Damage: 1
Projectile Speed: 28
Projectile Lifetime: 2.5
XP Per Use: 1
```

```text
AmmoData_556Rifle
Ammo Id: ammo_556
Display Name: 5.56 Rifle Round
Caliber Label: 5.56
Projectile Prefab: Projectile_PistolRound
Damage: 1
Projectile Speed: 45
Projectile Lifetime: 5
XP Per Use: 1
```

```text
AmmoData_308Sniper
Ammo Id: ammo_308
Display Name: .308 Sniper Round
Caliber Label: .308
Projectile Prefab: Projectile_PistolRound
Damage: 5
Projectile Speed: 60
Projectile Lifetime: 6
XP Per Use: 1
```

---

# Step 4 - Update WeaponDatabase

Replace `WeaponDatabase.cs` with this version:

```csharp
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponDatabase",
    menuName = "Echo Systems Lab/Weapons/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    [SerializeField] private WeaponData[] weapons;

    public WeaponData GetWeaponById(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return null;

        foreach (WeaponData weapon in weapons)
        {
            if (weapon == null)
                continue;

            if (weapon.weaponId == weaponId)
                return weapon;
        }

        return null;
    }

    public List<WeaponData> GetOwnedWeaponsInDatabaseOrder()
    {
        List<WeaponData> ownedWeapons = new List<WeaponData>();

        foreach (WeaponData weapon in weapons)
        {
            if (weapon == null)
                continue;

            if (PlayerProgress.OwnsWeapon(weapon.weaponId))
                ownedWeapons.Add(weapon);
        }

        return ownedWeapons;
    }
}
```

This gives the loadout system a clean way to build the player’s weapon bandolier.

---

# Step 5 - Update PlayerWeaponController for AmmoData

Replace `PlayerWeaponController.cs` with this version:

```csharp
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform fallbackMuzzlePoint;

    [Header("Loadout")]
    [SerializeField] private bool autoEquipSavedWeapon = true;

    [Header("Input")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;

    private WeaponData currentWeapon;
    private GameObject currentViewModel;
    private Transform currentMuzzlePoint;
    private float nextFireTime;
    private bool inputEnabled = true;

    public WeaponData CurrentWeapon => currentWeapon;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        if (autoEquipSavedWeapon)
            TryEquipSavedWeapon();
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (currentWeapon == null)
            return;

        if (currentWeapon.isAutomatic)
        {
            if (Input.GetKey(fireKey))
                TryFire();
        }
        else
        {
            if (Input.GetKeyDown(fireKey))
                TryFire();
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("Tried to equip null weapon data.");
            return;
        }

        currentWeapon = weaponData;

        PlayerProgress.SetActiveWeapon(currentWeapon.weaponId);

        SpawnViewModel();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterWeaponEquipped(currentWeapon.weaponId, currentWeapon.weaponType);

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log($"Equipped weapon: {currentWeapon.displayName}");
    }

    private void TryEquipSavedWeapon()
    {
        if (SaveManager.Instance == null)
            return;

        string activeWeaponId = PlayerProgress.ActiveWeaponId;

        if (string.IsNullOrWhiteSpace(activeWeaponId))
            return;

        WeaponDatabase database = SaveManager.Instance.WeaponDatabase;

        if (database == null)
        {
            Debug.LogWarning("No WeaponDatabase assigned to SaveManager.");
            return;
        }

        WeaponData savedWeapon = database.GetWeaponById(activeWeaponId);

        if (savedWeapon == null)
        {
            Debug.LogWarning($"Could not find saved weapon with id: {activeWeaponId}");
            return;
        }

        currentWeapon = savedWeapon;

        SpawnViewModel();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterWeaponEquipped(currentWeapon.weaponId, currentWeapon.weaponType);

        Debug.Log($"Auto-equipped saved weapon: {currentWeapon.displayName}");
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime)
            return;

        if (currentWeapon.defaultAmmo == null)
        {
            Debug.LogWarning($"Weapon '{currentWeapon.displayName}' has no default ammo assigned.");
            return;
        }

        nextFireTime = Time.time + currentWeapon.fireRate;

        if (currentWeapon.fireMode == WeaponFireMode.Projectile)
            FireProjectilePattern();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterShot(currentWeapon.weaponId, currentWeapon.weaponType);
    }

    private void FireProjectilePattern()
    {
        AmmoData ammo = currentWeapon.defaultAmmo;

        if (ammo.projectilePrefab == null)
        {
            Debug.LogWarning($"Ammo '{ammo.displayName}' has no projectile prefab assigned.");
            return;
        }

        int projectileCount = Mathf.Max(1, currentWeapon.projectilesPerShot);

        for (int i = 0; i < projectileCount; i++)
            FireSingleProjectile(ammo);
    }

    private void FireSingleProjectile(AmmoData ammo)
    {
        Transform spawnPoint = GetMuzzlePoint();

        Quaternion fireRotation = GetFireRotationWithSpread();

        GameObject projectileObject = Instantiate(
            ammo.projectilePrefab,
            spawnPoint.position,
            fireRotation);

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogWarning("Projectile prefab is missing Projectile script.");
            return;
        }

        projectile.Initialize(
            ammo.damage,
            currentWeapon.weaponId,
            currentWeapon.weaponType,
            gameObject,
            ammo.projectileSpeed,
            ammo.projectileLifetime);
    }

    private Quaternion GetFireRotationWithSpread()
    {
        Quaternion baseRotation = playerCamera.transform.rotation;

        if (currentWeapon.spreadAngle <= 0f)
            return baseRotation;

        float randomYaw = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle);
        float randomPitch = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle);

        return baseRotation * Quaternion.Euler(randomPitch, randomYaw, 0f);
    }

    private Transform GetMuzzlePoint()
    {
        if (currentMuzzlePoint != null)
            return currentMuzzlePoint;

        if (fallbackMuzzlePoint != null)
            return fallbackMuzzlePoint;

        return playerCamera.transform;
    }

    private void SpawnViewModel()
    {
        currentMuzzlePoint = null;

        if (currentViewModel != null)
            Destroy(currentViewModel);

        if (currentWeapon.viewModelPrefab == null || weaponHolder == null)
            return;

        currentViewModel = Instantiate(currentWeapon.viewModelPrefab, weaponHolder);

        currentViewModel.transform.localPosition = currentWeapon.viewLocalPosition;
        currentViewModel.transform.localEulerAngles = currentWeapon.viewLocalEulerAngles;
        currentViewModel.transform.localScale = currentWeapon.viewLocalScale;

        Transform muzzle = currentViewModel.transform.Find("MuzzlePoint");

        if (muzzle != null)
            currentMuzzlePoint = muzzle;
        else
            Debug.LogWarning($"{currentWeapon.displayName} view model has no child named MuzzlePoint. Using fallback muzzle.");
    }
}
```

Main upgrades:

```text
Uses AmmoData
Supports automatic weapons
Supports shotgun pellet count
Supports spread
Can disable weapon input later when menus are open
```

---

# Step 6 - Create PlayerWeaponLoadoutController

Create:

```text
Assets/_EchoSystemsLab/Scripts/Weapons/PlayerWeaponLoadoutController.cs
```

```csharp
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponLoadoutController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponController weaponController;

    [Header("Input")]
    [SerializeField] private KeyCode cycleNextKey = KeyCode.Tab;

    private bool inputEnabled = true;

    private void Awake()
    {
        if (weaponController == null)
            weaponController = GetComponent<PlayerWeaponController>();
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (Input.GetKeyDown(cycleNextKey))
            CycleNextWeapon();
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void CycleNextWeapon()
    {
        if (SaveManager.Instance == null)
            return;

        WeaponDatabase database = SaveManager.Instance.WeaponDatabase;

        if (database == null)
        {
            Debug.LogWarning("No WeaponDatabase assigned to SaveManager.");
            return;
        }

        List<WeaponData> ownedWeapons = database.GetOwnedWeaponsInDatabaseOrder();

        if (ownedWeapons.Count <= 0)
        {
            Debug.Log("No owned weapons to cycle.");
            return;
        }

        int currentIndex = GetCurrentWeaponIndex(ownedWeapons);
        int nextIndex = (currentIndex + 1) % ownedWeapons.Count;

        WeaponData nextWeapon = ownedWeapons[nextIndex];

        if (weaponController != null)
            weaponController.EquipWeapon(nextWeapon);
    }

    private int GetCurrentWeaponIndex(List<WeaponData> ownedWeapons)
    {
        string activeWeaponId = PlayerProgress.ActiveWeaponId;

        for (int i = 0; i < ownedWeapons.Count; i++)
        {
            if (ownedWeapons[i] == null)
                continue;

            if (ownedWeapons[i].weaponId == activeWeaponId)
                return i;
        }

        return -1;
    }
}
```

Add this to the Player:

```text
Player
- PlayerWeaponController
- PlayerWeaponLoadoutController
```

Assign:

```text
Weapon Controller = PlayerWeaponController
Cycle Next Key = Tab
```

---

# Step 7 - Create WeaponData Assets

Create these assets:

```text
WeaponData_Revolver
WeaponData_Crossbow
WeaponData_Shotgun
WeaponData_AssaultRifle
WeaponData_SniperRifle
```

Suggested first-pass values:

```text
WeaponData_Revolver
Weapon Id: revolver_basic
Display Name: Revolver
Weapon Type: Revolver
Default Ammo: AmmoData_45Caliber
Fire Rate: 0.55
Is Automatic: false
Projectiles Per Shot: 1
Spread Angle: 0.5
```

```text
WeaponData_Crossbow
Weapon Id: crossbow_basic
Display Name: Crossbow
Weapon Type: Crossbow
Default Ammo: AmmoData_CrossbowBolt
Fire Rate: 1.1
Is Automatic: false
Projectiles Per Shot: 1
Spread Angle: 0
```

```text
WeaponData_Shotgun
Weapon Id: shotgun_basic
Display Name: Shotgun
Weapon Type: Shotgun
Default Ammo: AmmoData_12GaugeBuckshot
Fire Rate: 0.85
Is Automatic: false
Projectiles Per Shot: 8
Spread Angle: 5
```

```text
WeaponData_AssaultRifle
Weapon Id: assault_rifle_basic
Display Name: Assault Rifle
Weapon Type: Assault Rifle
Default Ammo: AmmoData_556Rifle
Fire Rate: 0.12
Is Automatic: true
Projectiles Per Shot: 1
Spread Angle: 1.25
```

```text
WeaponData_SniperRifle
Weapon Id: sniper_rifle_basic
Display Name: Sniper Rifle
Weapon Type: Sniper Rifle
Default Ammo: AmmoData_308Sniper
Fire Rate: 1.25
Is Automatic: false
Projectiles Per Shot: 1
Spread Angle: 0
```

Also update:

```text
WeaponData_Pistol
Default Ammo: AmmoData_9mm
Fire Rate: 0.35
Is Automatic: false
Projectiles Per Shot: 1
Spread Angle: 1
```

---

# Step 8 - Update WeaponDatabase Asset

Open:

```text
WeaponDatabase.asset
```

Add weapons in the order you want Tab to cycle:

```text
1. WeaponData_Pistol
2. WeaponData_Revolver
3. WeaponData_Crossbow
4. WeaponData_Shotgun
5. WeaponData_AssaultRifle
6. WeaponData_SniperRifle
```

This order becomes the bandolier cycle order.

---

# Step 9 - Create ViewModel Prefabs

For each imported weapon prefab, create a clean view model prefab:

```text
Pistol_ViewModel
Revolver_ViewModel
Crossbow_ViewModel
Shotgun_ViewModel
AssaultRifle_ViewModel
SniperRifle_ViewModel
```

Each should follow this structure:

```text
WeaponName_ViewModel
 ├── Weapon mesh
 └── MuzzlePoint
```

Important:

```text
MuzzlePoint must be named exactly: MuzzlePoint
```

Put it at the barrel/front firing point.

Then assign each view model to the matching `WeaponData`.

---

# Step 10 - Create Weapon Pedestals

Create individual pedestal prefabs or scene objects:

```text
Pedestal_Pistol
Pedestal_Revolver
Pedestal_Crossbow
Pedestal_Shotgun
Pedestal_AssaultRifle
Pedestal_SniperRifle
```

Each pedestal needs:

```text
Collider
Layer: Interactable
WeaponPedestal
WeaponData assigned
Pedestal weapon visual assigned
```

Example:

```text
Pedestal_Revolver
- Weapon Data = WeaponData_Revolver
- Pedestal Weapon Visual = Revolver mesh on pedestal
- Hide Visual When Owned = true
- Allow Reequip If Owned = true
```

Place them in TargetRangeTrial like an armory row:

```text
Pistol → Revolver → Crossbow → Shotgun → Assault Rifle → Sniper Rifle
```

---

# Step 11 - Pause/Terminal Input Safety

Since there is now a weapon controller and loadout controller, menus should eventually disable both.

For this milestone, add references later if needed:

```text
PauseMenuController
- PlayerWeaponController
- PlayerWeaponLoadoutController
```

And:

```text
MissionTerminalUI
- disable weapon input while terminal is open
```

This can be a small follow-up if clicking UI causes weapons to fire.

For now, test whether this is a problem. If it is, add a clean `PlayerInputModeController` later.

---

# Step 12 - Unity Setup Checklist

## Scripts

```text
AmmoData.cs created
WeaponData.cs updated
WeaponDatabase.cs updated
PlayerWeaponController.cs updated
PlayerWeaponLoadoutController.cs created
```

## Ammo Assets

```text
AmmoData_9mm
AmmoData_45Caliber
AmmoData_CrossbowBolt
AmmoData_12GaugeBuckshot
AmmoData_556Rifle
AmmoData_308Sniper
```

## Weapon Assets

```text
WeaponData_Pistol
WeaponData_Revolver
WeaponData_Crossbow
WeaponData_Shotgun
WeaponData_AssaultRifle
WeaponData_SniperRifle
```

## Weapon Database

```text
WeaponDatabase.asset contains all six ranged weapons
Order matches desired Tab cycle order
SaveManager has WeaponDatabase assigned
```

## Player

```text
Player
- PlayerWeaponController
- PlayerWeaponLoadoutController
```

## View Models

```text
Each weapon view model has child named MuzzlePoint
Each WeaponData has correct View Model Prefab assigned
```

## Pedestals

```text
Each weapon pedestal has:
- Collider
- Interactable layer
- WeaponPedestal
- WeaponData assigned
- Pedestal visual assigned
```

---

# Goal Line / Completion Checklist

This milestone is done when:

```text
1. Start New Game.
2. Enter TargetRangeTrial.
3. See pedestals for Pistol, Revolver, Crossbow, Shotgun, Assault Rifle, and Sniper Rifle.
4. Pick up each weapon.
5. Each pedestal hides its weapon visual after pickup.
6. Each weapon is saved as owned.
7. Press Tab cycles through owned weapons.
8. Each cycled weapon appears in first person.
9. Each weapon fires from its own MuzzlePoint.
10. Pistol uses 9mm AmmoData.
11. Revolver uses .45 AmmoData.
12. Crossbow uses Crossbow Bolt AmmoData.
13. Shotgun fires multiple projectiles with spread.
14. Assault Rifle fires automatically while holding fire.
15. Sniper Rifle fires slowly with high damage.
16. Save and return to Hub.
17. Active weapon stays equipped.
18. Restart game and Load Game.
19. Owned weapons persist.
20. Active weapon persists.
21. Returning to TargetRangeTrial keeps collected pedestal weapons hidden.
```

---

# Suggested Commit

```bash
git add .
git commit -m "Add multi weapon loadout and ammo data"
```

Optional tag:

```bash
git tag milestone-2c-multi-weapon-loadout
```

---

# Design Notes

## Player compartments

This starts separating the player into clearer modules:

```text
PlayerInteractor = interaction
PlayerWeaponController = current weapon firing/equipping
PlayerWeaponLoadoutController = owned weapon cycling
SimpleFirstPersonController = movement/look
```

Later, this can expand into:

```text
PlayerInventoryController
PlayerTraversalController
PlayerAbilityController
PlayerStatsController
PlayerInputModeController
```

That supports the larger goal: systems can be turned on/off per mission.

---

## AmmoData makes future ammo types easy

Later, the same weapon can support:

```text
Regular ammo
Armor piercing ammo
Incendiary ammo
Explosive ammo
Stun ammo
```

The weapon stays the same. The ammo changes the behavior.

---

## Shotgun is the first pattern weapon

The shotgun proves the weapon system can do more than single bullets.

It introduces:

```text
Multiple projectiles per shot
Spread
Close-range damage style
Different weapon identity
```

This makes the Target Range much more portfolio-visible.

---

# Small Polish Backlog After 2C

```text
Add target hit flash.
Debug projectile misses.
Add reload/ammo counters later.
Add weapon name display polish.
Add weapon pickup notification.
Add shooting challenge terminal.
Add weapon-specific target lanes.
Add weapon cards to portfolio.
```

---

# Portfolio Value

This milestone demonstrates:

```text
ScriptableObject-driven weapon expansion
ScriptableObject-driven ammo definitions
Persistent owned weapon loadout
Saved active weapon
Weapon database lookup by ID
First-person weapon view model swapping
MuzzlePoint-based projectile spawning
Semi-auto and automatic fire
Shotgun-style multi-projectile spread
Interactable weapon pedestals
Expandable player module architecture
```

This is a major systems-card milestone because it shows the framework scaling from one weapon to a reusable arsenal.

---

# Future Portfolio Card Draft

## Multi-Weapon Loadout and Ammo Data

Expanded the Target Range Trial from a single pistol prototype into a modular ranged weapon framework. Added ScriptableObject-driven ammo definitions, multiple weapon data assets, persistent weapon ownership, active weapon saving, first-person view model swapping, weapon pedestals, and Tab-based weapon cycling. The system supports semi-automatic weapons, automatic fire, shotgun-style spread patterns, per-weapon muzzle points, and a scalable WeaponDatabase for restoring saved loadouts across scenes and game restarts.
