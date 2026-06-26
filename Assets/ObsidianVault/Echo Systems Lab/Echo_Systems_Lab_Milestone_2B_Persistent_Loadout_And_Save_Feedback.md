---
title: Milestone 2B - Persistent Loadout, Weapon Ownership, Pedestal State, and Save Feedback
project: Echo Systems Lab
type: checkpoint-build-plan
tags:
  - unity
  - systems-design
  - save-system
  - weapons
  - persistence
  - loadout
  - portfolio
---

# Milestone 2B - Persistent Loadout, Weapon Ownership, Pedestal State, and Save Feedback

This milestone fixes the current issue where the player has to pick up the pistol again after returning to the TargetRangeTrial.

The goal is to turn weapons from temporary scene props into persistent player progression.

---

# Goal Line

```text
New Game starts fresh
→ Player enters TargetRangeTrial
→ Player picks up pistol
→ Pistol is added to persistent player data
→ Pistol pedestal visual disappears
→ Save records owned pistol and active weapon
→ Returning to Hub keeps the pistol equipped
→ Re-entering TargetRangeTrial keeps the pistol owned
→ Pistol pedestal remains empty/gone
→ Load Game restores owned pistol, active weapon, XP, and mission progress
→ Save feedback appears in UI when saving
→ Quit works in builds and stops Play Mode in the Unity Editor
```

---

# Naming / Scope Notes

This is **not** the full inventory system yet.

This milestone creates:

```text
Owned weapon IDs
Active weapon ID
Saved pedestal state derived from ownership
Auto-equip owned weapon on scene load
Save notification UI
Editor-safe quit behavior
```

This milestone does **not** create yet:

```text
Inventory UI
Dropping weapons
Weapon slots
Ammo inventory
Item stacks
Multiple save slots
Equipment screen
Weapon swapping UI
```

For now:

```text
If the player owns a weapon, it is saved permanently.
If the weapon was picked up from a pedestal, that pedestal hides its weapon visual forever.
If the player has an active weapon, it auto-equips when a scene loads.
```

---

# Current Bug Being Fixed

Current issue:

```text
Player completes TargetRangeTrial
→ Returns to Hub
→ Re-enters TargetRangeTrial
→ Has to pick up pistol again
```

New desired behavior:

```text
Player picks up pistol once
→ Pistol is owned forever
→ Pistol auto-equips in Hub and other gameplay scenes
→ TargetRangeTrial pistol pedestal stays empty if pistol is already owned
```

---

# System Pieces

```text
1. SaveData weapon ownership fields
2. PlayerProgress owned weapon methods
3. WeaponDatabase ScriptableObject
4. PlayerWeaponController auto-equip support
5. WeaponPedestal ownership-aware state
6. SaveManager save/load events
7. SaveNotificationUI
8. Editor-safe quit utility behavior
9. Mission terminal prefab note for all scenes
10. Test flow and commit
```

---

# Folder Setup

Create:

```text
Assets/_EchoSystemsLab/Scripts/Weapons/
Assets/_EchoSystemsLab/Scripts/UI/
Assets/_EchoSystemsLab/ScriptableObjects/Weapons/
Assets/_EchoSystemsLab/Prefabs/UI/System/
```

Most of these probably already exist. Add only what is missing.

---

# Step 1 - Update SaveData for Weapon Ownership

Replace `SaveData.cs` with this expanded version:

```csharp
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string saveVersion = "0.2.0";

    public List<string> completedMissionIds = new List<string>();

    public List<WeaponTypeXpEntry> weaponTypeXpEntries = new List<WeaponTypeXpEntry>();

    public List<string> ownedWeaponIds = new List<string>();

    public string activeWeaponId;

    public string lastSceneName = "Hub";

    public bool hasStartedGame;
}

[Serializable]
public class WeaponTypeXpEntry
{
    public string weaponType;
    public int xp;

    public WeaponTypeXpEntry(string weaponType, int xp)
    {
        this.weaponType = weaponType;
        this.xp = xp;
    }
}
```

New persistent fields:

```text
ownedWeaponIds
activeWeaponId
```

This keeps the save file simple and readable.

---

# Step 2 - Update PlayerProgress for Owned Weapons

Replace `PlayerProgress.cs` with this:

```csharp
using System.Collections.Generic;

public static class PlayerProgress
{
    private static readonly Dictionary<string, int> weaponTypeXp = new Dictionary<string, int>();
    private static readonly HashSet<string> ownedWeaponIds = new HashSet<string>();

    private static string activeWeaponId;

    public static string ActiveWeaponId => activeWeaponId;

    public static int GetWeaponTypeXp(string weaponType)
    {
        if (string.IsNullOrWhiteSpace(weaponType))
            return 0;

        return weaponTypeXp.TryGetValue(weaponType, out int xp) ? xp : 0;
    }

    public static void AddWeaponTypeXp(string weaponType, int amount)
    {
        if (string.IsNullOrWhiteSpace(weaponType))
            return;

        if (amount <= 0)
            return;

        if (!weaponTypeXp.ContainsKey(weaponType))
            weaponTypeXp.Add(weaponType, 0);

        weaponTypeXp[weaponType] += amount;
    }

    public static bool OwnsWeapon(string weaponId)
    {
        return !string.IsNullOrWhiteSpace(weaponId) &&
               ownedWeaponIds.Contains(weaponId);
    }

    public static void AddOwnedWeapon(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        ownedWeaponIds.Add(weaponId);
    }

    public static void SetActiveWeapon(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        AddOwnedWeapon(weaponId);
        activeWeaponId = weaponId;
    }

    public static List<string> GetOwnedWeaponIds()
    {
        return new List<string>(ownedWeaponIds);
    }

    public static void LoadFromSaveData(SaveData saveData)
    {
        weaponTypeXp.Clear();
        ownedWeaponIds.Clear();
        activeWeaponId = null;

        if (saveData == null)
            return;

        if (saveData.weaponTypeXpEntries != null)
        {
            foreach (WeaponTypeXpEntry entry in saveData.weaponTypeXpEntries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.weaponType))
                    continue;

                weaponTypeXp[entry.weaponType] = entry.xp;
            }
        }

        if (saveData.ownedWeaponIds != null)
        {
            foreach (string weaponId in saveData.ownedWeaponIds)
            {
                if (!string.IsNullOrWhiteSpace(weaponId))
                    ownedWeaponIds.Add(weaponId);
            }
        }

        activeWeaponId = saveData.activeWeaponId;
    }

    public static void WriteToSaveData(SaveData saveData)
    {
        if (saveData == null)
            return;

        saveData.weaponTypeXpEntries.Clear();

        foreach (KeyValuePair<string, int> pair in weaponTypeXp)
            saveData.weaponTypeXpEntries.Add(new WeaponTypeXpEntry(pair.Key, pair.Value));

        saveData.ownedWeaponIds = GetOwnedWeaponIds();
        saveData.activeWeaponId = activeWeaponId;
    }

    public static void ResetProgress()
    {
        weaponTypeXp.Clear();
        ownedWeaponIds.Clear();
        activeWeaponId = null;
    }
}
```

Now `PlayerProgress` owns:

```text
Weapon XP
Owned weapons
Currently active weapon
```

Still simple, still expandable.

---

# Step 3 - Create WeaponDatabase

Because saves store weapon IDs, the game needs a way to turn this:

```text
pistol_basic
```

back into this:

```text
WeaponData_Pistol
```

Create:

```text
Assets/_EchoSystemsLab/Scripts/Weapons/WeaponDatabase.cs
```

```csharp
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
}
```

Then create the asset:

```text
Assets/_EchoSystemsLab/ScriptableObjects/Weapons/WeaponDatabase.asset
```

Right-click:

```text
Create > Echo Systems Lab > Weapons > Weapon Database
```

Add:

```text
WeaponData_Pistol
```

to the list.

Later, this list gets:

```text
WeaponData_Shotgun
WeaponData_SMG
WeaponData_Bow
WeaponData_Crossbow
WeaponData_RPG
```

---

# Step 4 - Add WeaponDatabase to SaveManager

Update `SaveManager.cs` with a reference to the weapon database.

Add near the top:

```csharp
[Header("Databases")]
[SerializeField] private WeaponDatabase weaponDatabase;

public WeaponDatabase WeaponDatabase => weaponDatabase;
```

So the start of `SaveManager` becomes:

```csharp
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SaveFileName = "echo_systems_lab_save.json";

    [Header("Databases")]
    [SerializeField] private WeaponDatabase weaponDatabase;

    private SaveData currentSaveData = new SaveData();

    public SaveData CurrentSaveData => currentSaveData;
    public WeaponDatabase WeaponDatabase => weaponDatabase;

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
```

In the Inspector, assign:

```text
Weapon Database = WeaponDatabase.asset
```

on your `_SystemBootstrap` prefab.

---

# Step 5 - Update PlayerWeaponController to Save Ownership and Auto-Equip

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
        if (currentWeapon == null)
            return;

        if (Input.GetKey(fireKey))
            TryFire();
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

        nextFireTime = Time.time + currentWeapon.fireRate;

        if (currentWeapon.fireMode == WeaponFireMode.Projectile)
            FireProjectile();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterShot(currentWeapon.weaponId, currentWeapon.weaponType);
    }

    private void FireProjectile()
    {
        if (currentWeapon.projectilePrefab == null)
        {
            Debug.LogWarning($"Weapon '{currentWeapon.displayName}' has no projectile prefab assigned.");
            return;
        }

        Transform spawnPoint = GetMuzzlePoint();

        GameObject projectileObject = Instantiate(
            currentWeapon.projectilePrefab,
            spawnPoint.position,
            playerCamera.transform.rotation);

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogWarning("Projectile prefab is missing Projectile script.");
            return;
        }

        projectile.Initialize(
            currentWeapon.damage,
            currentWeapon.weaponId,
            currentWeapon.weaponType,
            gameObject,
            currentWeapon.projectileSpeed,
            currentWeapon.projectileLifetime);
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

Now picking up a weapon:

```text
Adds weapon to owned weapons
Sets active weapon
Spawns view model
Saves immediately
```

And entering a scene:

```text
Reads active weapon
Finds WeaponData in database
Equips it automatically
```

---

# Step 6 - Make WeaponPedestal Respect Ownership

Replace `WeaponPedestal.cs` with this:

```csharp
using UnityEngine;

public class WeaponPedestal : MonoBehaviour, IInteractable
{
    [Header("Weapon")]
    [SerializeField] private WeaponData weaponData;

    [Header("Pedestal Visual")]
    [SerializeField] private GameObject pedestalWeaponVisual;
    [SerializeField] private bool hideVisualWhenOwned = true;
    [SerializeField] private bool allowReequipIfOwned = true;

    [Header("Prompt")]
    [SerializeField] private string promptPrefix = "Press E to equip";

    private void Start()
    {
        RefreshVisualState();
    }

    public string GetPromptText()
    {
        if (weaponData == null)
            return "No weapon assigned";

        if (PlayerProgress.OwnsWeapon(weaponData.weaponId))
        {
            if (allowReequipIfOwned)
                return $"Press E to equip owned {weaponData.displayName}";

            return $"{weaponData.displayName} owned";
        }

        return $"{promptPrefix} {weaponData.displayName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"{name} has no WeaponData assigned.");
            return;
        }

        if (PlayerProgress.OwnsWeapon(weaponData.weaponId) && !allowReequipIfOwned)
            return;

        PlayerWeaponController weaponController = interactor.GetComponent<PlayerWeaponController>();

        if (weaponController == null)
        {
            Debug.LogWarning("Interactor does not have PlayerWeaponController.");
            return;
        }

        weaponController.EquipWeapon(weaponData);

        RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        if (weaponData == null)
            return;

        bool ownsWeapon = PlayerProgress.OwnsWeapon(weaponData.weaponId);

        if (hideVisualWhenOwned && pedestalWeaponVisual != null)
            pedestalWeaponVisual.SetActive(!ownsWeapon);
    }
}
```

This makes the pedestal state persistent **without needing a separate pedestal save ID yet**.

For now:

```text
If player owns pistol, pistol pedestal is empty.
```

Later, when scene objects need more complex memory:

```text
SceneObjectState
PedestalState
ChestState
DoorState
SwitchState
```

---

# Step 7 - Add SaveManager Events

Update `SaveManager.cs` so UI can react when saving/loading.

Add:

```csharp
using System;
```

At the top:

```csharp
using System;
using System.IO;
using UnityEngine;
```

Inside the class:

```csharp
public event Action OnGameSaved;
public event Action OnGameLoaded;
```

At the end of `SaveGame()`:

```csharp
OnGameSaved?.Invoke();
```

At the end of successful `LoadGame()`:

```csharp
OnGameLoaded?.Invoke();
```

So `SaveGame()` becomes:

```csharp
public void SaveGame()
{
    MissionProgress.WriteToSaveData(currentSaveData);
    PlayerProgress.WriteToSaveData(currentSaveData);

    string json = JsonUtility.ToJson(currentSaveData, true);
    File.WriteAllText(SavePath, json);

    Debug.Log($"Game saved to: {SavePath}");

    OnGameSaved?.Invoke();
}
```

And in `LoadGame()`, after:

```csharp
Debug.Log($"Game loaded from: {SavePath}");
```

add:

```csharp
OnGameLoaded?.Invoke();
```

---

# Step 8 - Create SaveNotificationUI

Create:

```text
Assets/_EchoSystemsLab/Scripts/UI/SaveNotificationUI.cs
```

```csharp
using System.Collections;
using TMPro;
using UnityEngine;

public class SaveNotificationUI : MonoBehaviour
{
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private float showDuration = 1.5f;

    private Coroutine hideRoutine;

    private void Start()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameSaved += ShowSaved;
            SaveManager.Instance.OnGameLoaded += ShowLoaded;
        }
    }

    private void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameSaved -= ShowSaved;
            SaveManager.Instance.OnGameLoaded -= ShowLoaded;
        }
    }

    private void ShowSaved()
    {
        ShowMessage("Saved");
    }

    private void ShowLoaded()
    {
        ShowMessage("Loaded");
    }

    private void ShowMessage(string message)
    {
        if (notificationText == null)
            return;

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(showDuration);

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }
}
```

Add to gameplay UI:

```text
Canvas_SystemMessages
 └── SaveNotificationText
```

Or place it under the existing HUD canvas.

Recommended:

```text
UI
 └── Canvas_SystemMessages
      ├── SaveNotificationUI
      └── SaveNotificationText
```

Later this can also show:

```text
Loaded
Checkpoint saved
Cannot save here
Inventory full
Mission complete
```

---

# Step 9 - Fix Quit in Editor and Build

`Application.Quit()` only works in a built game. In the Unity Editor, it does nothing.

Update `GameSceneLoader.cs` like this:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameSceneLoader : MonoBehaviour
{
    public static GameSceneLoader Instance { get; private set; }

    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMissionScene(MissionData mission)
    {
        if (mission == null)
        {
            Debug.LogWarning("Tried to load a null mission.");
            return;
        }

        if (string.IsNullOrWhiteSpace(mission.sceneName))
        {
            Debug.LogWarning($"Mission '{mission.displayName}' has no scene name assigned.");
            return;
        }

        SceneManager.LoadScene(mission.sceneName);
    }

    public void LoadHub()
    {
        SceneManager.LoadScene(hubSceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
```

Then update `MainMenuController.QuitGame()` and `PauseMenuController.QuitGame()` to use `GameSceneLoader`.

In `MainMenuController`:

```csharp
private void QuitGame()
{
    if (GameSceneLoader.Instance != null)
    {
        GameSceneLoader.Instance.QuitGame();
        return;
    }

    Application.Quit();
}
```

In `PauseMenuController`:

```csharp
private void QuitGame()
{
    if (GameSceneLoader.Instance != null)
    {
        GameSceneLoader.Instance.QuitGame();
        return;
    }

    Application.Quit();
}
```

Now quit works in builds and stops Play Mode in the editor.

---

# Step 10 - Add Mission Terminal to Every Level

Do this as a setup step, not a new system yet.

Create a prefab:

```text
Assets/_EchoSystemsLab/Prefabs/UI/Terminals/MissionTerminal_Setup.prefab
```

Or, if it includes a world object:

```text
Assets/_EchoSystemsLab/Prefabs/Interactables/MissionComputer.prefab
```

Recommended structure:

```text
MissionComputer_Setup
 ├── Computer / Terminal Mesh
 ├── Collider
 ├── MissionComputer
 └── Canvas_MissionTerminal
```

For now, add it to:

```text
Hub
TargetRangeTrial
CombatTrial
CampGroundTrial
```

Later this can split into:

```text
MissionTerminalUI prefab
MissionComputer interactable prefab
MissionBoard data source
```

---

# Step 11 - Unity Setup Checklist

## Save Data

```text
SaveData has:
- completedMissionIds
- weaponTypeXpEntries
- ownedWeaponIds
- activeWeaponId
```

## System Bootstrap

```text
_SystemBootstrap prefab
- GameSceneLoader
- SaveManager
  - Weapon Database assigned
```

Place prefab in:

```text
MainMenu
Hub
TargetRangeTrial
CombatTrial
CampGroundTrial
```

## Weapon Database

```text
WeaponDatabase.asset
- WeaponData_Pistol
```

## Player

```text
Player
- PlayerWeaponController
  - Auto Equip Saved Weapon = true
  - Weapon Holder assigned
  - Fallback Muzzle Point assigned
```

## Pistol Pedestal

```text
PistolPedestal
- WeaponPedestal
  - Weapon Data = WeaponData_Pistol
  - Pedestal Weapon Visual = pistol mesh on pedestal
  - Hide Visual When Owned = true
  - Allow Reequip If Owned = true
```

## Save Notification

```text
Canvas_SystemMessages
- SaveNotificationUI
- SaveNotificationText
```

## Mission Terminal

```text
Mission terminal prefab exists
TargetRangeTrial has a mission terminal
Hub has a mission terminal
CombatTrial/CampGroundTrial can receive one next
```

---

# Goal Line / Completion Checklist

This milestone is done when:

```text
1. Start New Game.
2. Enter TargetRangeTrial.
3. Pistol pedestal shows pistol.
4. Press E to equip pistol.
5. Pistol is added to owned weapons.
6. Pistol visual disappears from pedestal.
7. Save notification appears.
8. Return to Hub.
9. Pistol remains equipped in Hub.
10. Save and quit play mode.
11. Restart game.
12. Load Game.
13. Pistol is still owned.
14. Pistol auto-equips.
15. Enter TargetRangeTrial again.
16. Pistol pedestal is still empty/gone.
17. Target Range Trial remains completed and replayable.
18. Combat Trial remains unlocked.
19. Quit button stops Play Mode in Editor.
20. Mission terminal can be placed in non-Hub levels.
```

---

# Suggested Commit

```bash
git add .
git commit -m "Add persistent weapon ownership and save feedback"
```

Optional tag:

```bash
git tag milestone-2b-persistent-loadout
```

---

# Design Notes

## Weapon ownership drives pedestal state

For now, pedestal state is derived from:

```text
Player owns weapon ID
```

So if the player owns `pistol_basic`, the pistol pedestal hides its visual.

This is enough for first-pass weapon pickup persistence.

Later, when scene objects need more individual memory, add:

```text
SceneObjectStateData
SceneStateManager
PersistentObjectId
```

That will support:

```text
Opened chests
Unlocked doors
Activated switches
Destroyed props
Collected one-time items
```

## Active weapon is saved separately

Owned weapons means:

```text
The player has this weapon available.
```

Active weapon means:

```text
This is the weapon currently equipped.
```

That separation matters when weapon swapping is added.

## Inventory later

This is not the inventory system yet. It is the **first persistent loadout bridge**.

Later inventory can upgrade this into:

```text
InventoryItemData
InventorySlot
InventoryUI
EquipmentScreen
DropItem
PickupItem
```

But this milestone keeps it focused.

---

# Small Polish Backlog After 2B

```text
Add small flash to targets on hit.
Debug projectile misses.
Add saving prompt animation.
Import fonts.
Create color palettes.
Settings UI.
Credits UI.
Keybinds UI after New Input System upgrade.
Mission terminal variant for shooting challenges.
```

---

# Portfolio Value

This milestone demonstrates:

```text
Persistent player loadout
Saved weapon ownership
ScriptableObject weapon lookup by ID
Scene-to-scene equipment persistence
Derived pedestal state from save data
Auto-save feedback UI
Editor-safe quit flow
Reusable mission terminal placement
Expansion path toward inventory and scene persistence
```

This is a strong systems-card candidate because it shows the bridge between moment-to-moment gameplay and persistent profile state.

---

# Future Portfolio Card Draft

## Persistent Loadout and Save Feedback

Implemented persistent weapon ownership and active loadout saving for Echo Systems Lab. Weapon pickups are saved into player progress, restored across scenes and game restarts, and used to drive world object state such as empty weapon pedestals. Added a ScriptableObject weapon database for resolving saved weapon IDs, auto-equipping saved weapons on scene load, save notification UI, and editor-safe quit handling. This milestone establishes the bridge between gameplay interaction, persistent progression, and future inventory systems.
