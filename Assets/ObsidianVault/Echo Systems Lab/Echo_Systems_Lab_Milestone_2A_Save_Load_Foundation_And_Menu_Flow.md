---
title: Milestone 2A - Save Load Foundation, Main Menu, and Replayable Mission Progress
project: Echo Systems Lab
type: checkpoint-build-plan
tags:
  - unity
  - systems-design
  - save-system
  - menus
  - progression
  - portfolio
---

# Milestone 2A - Save / Load Foundation, Main Menu, and Replayable Mission Progress

This milestone turns Echo Systems Lab from a scene demo into a real game shell with memory, menus, and persistent progression.

Current foundation:

```text
Hub mission terminal ✅
Scene loading ✅
Replayable system trial structure ✅
Weapon pickup ✅
Projectile shooting ✅
Target damage ✅
HUD feedback ✅
Mission completion ✅
Mission unlock chain ✅
```

---

# Goal Line

```text
Launch game
→ Main Menu appears
→ New Game starts fresh progress
→ Load Game restores saved progress
→ Hub mission terminal shows completed and unlocked missions correctly
→ Completed missions are replayable
→ Pause menu can save, load, resume, return to main menu, or quit
→ Progress persists after quitting and reopening the game
```

---

# Naming / Scope Notes

This milestone is about **persistent game state**, not full inventory or character systems yet.

We are saving:

```text
Mission completion
Mission unlock progression
Weapon type XP
Current scene or last hub position, optional
Settings stubs, optional
```

We are **not** saving yet:

```text
Target Range session score
Shots fired in a specific run
Hits in a specific run
Accuracy for one target range attempt
Temporary scene targets destroyed
Inventory
Character stats screen
Full settings/keybind data
```

Target Range stats are currently **instance/session data**.

Weapon XP and mission completion are **persistent profile data**.

---

# Current Design Decisions

## Completed missions should be replayable

Current behavior may disable completed missions.

New rule:

```text
Locked missions cannot be clicked.
Unlocked missions can be clicked.
Completed missions can still be clicked and replayed.
```

Mission status remains:

```text
LOCKED
AVAILABLE
COMPLETED
```

But both `AVAILABLE` and `COMPLETED` should be interactable.

---

## XP should be usage-based

Current weapon XP:

```text
Hit = XP
Target destroyed = XP
```

New direction:

```text
Firing / using the weapon = XP
Hits and kills can give bonus XP later
```

For now:

```text
Every shot gives weapon type XP.
```

Later:

```text
Hit target = bonus XP
Destroy target = bonus XP
Special objective = bonus XP
```

---

## XP belongs to weapon type

Current weapon ID:

```text
pistol_basic
```

Useful for specific weapon tracking later.

For this stage, persistent XP should be based on:

```text
Pistol
Shotgun
SMG
Crossbow
Bow
RPG
```

Track:

```text
weaponTypeXp["Pistol"] = 123
```

instead of only:

```text
weaponXp["pistol_basic"] = 123
```

Specific weapon XP can come later.

---

# System Pieces

```text
1. SaveData class
2. SaveManager singleton
3. MissionProgress refactor
4. PlayerProgress / XP storage
5. Mission terminal replay rule update
6. Main Menu scene
7. MainMenuUI controller
8. Pause Menu UI
9. PauseMenuUI controller
10. Save/load flow testing
11. Quit/reboot persistence test
```

---

# Folder Setup

Create:

```text
Assets/_EchoSystemsLab/Scripts/Save/
Assets/_EchoSystemsLab/Scripts/Menus/
Assets/_EchoSystemsLab/Scripts/Progression/
Assets/_EchoSystemsLab/Scenes/MainMenu.unity
Assets/_EchoSystemsLab/Prefabs/UI/Menus/
```

Optional documentation:

```text
Assets/_Documentation/SaveSystemNotes.md
```

---

# Step 1 - Create SaveData

Create:

```text
Assets/_EchoSystemsLab/Scripts/Save/SaveData.cs
```

```csharp
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string saveVersion = "0.1.0";

    public List<string> completedMissionIds = new List<string>();

    public List<WeaponTypeXpEntry> weaponTypeXpEntries = new List<WeaponTypeXpEntry>();

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

Why a list instead of dictionary?

Unity’s `JsonUtility` does not serialize dictionaries cleanly. Lists are boring, reliable little pack mules.

---

# Step 2 - Create SaveManager

Create:

```text
Assets/_EchoSystemsLab/Scripts/Save/SaveManager.cs
```

```csharp
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SaveFileName = "echo_systems_lab_save.json";

    private SaveData currentSaveData = new SaveData();

    public SaveData CurrentSaveData => currentSaveData;

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame();
    }

    public void NewGame()
    {
        currentSaveData = new SaveData();
        currentSaveData.hasStartedGame = true;
        currentSaveData.lastSceneName = "Hub";

        MissionProgress.LoadFromSaveData(currentSaveData);
        PlayerProgress.LoadFromSaveData(currentSaveData);

        SaveGame();

        Debug.Log("New game created.");
    }

    public void SaveGame()
    {
        MissionProgress.WriteToSaveData(currentSaveData);
        PlayerProgress.WriteToSaveData(currentSaveData);

        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"Game saved to: {SavePath}");
    }

    public bool LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            currentSaveData = new SaveData();

            MissionProgress.LoadFromSaveData(currentSaveData);
            PlayerProgress.LoadFromSaveData(currentSaveData);

            Debug.Log("No save file found. Created fresh save data in memory.");
            return false;
        }

        string json = File.ReadAllText(SavePath);
        currentSaveData = JsonUtility.FromJson<SaveData>(json);

        if (currentSaveData == null)
            currentSaveData = new SaveData();

        MissionProgress.LoadFromSaveData(currentSaveData);
        PlayerProgress.LoadFromSaveData(currentSaveData);

        Debug.Log($"Game loaded from: {SavePath}");
        return true;
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        currentSaveData = new SaveData();

        MissionProgress.LoadFromSaveData(currentSaveData);
        PlayerProgress.LoadFromSaveData(currentSaveData);

        Debug.Log("Save file deleted.");
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }
}
```

Scene setup:

```text
_SystemBootstrap
 ├── GameSceneLoader
 └── SaveManager
```

Important:

`SaveManager` should live on a root object, just like `GameSceneLoader`.

---

# Step 3 - Refactor MissionProgress for SaveData

Replace your current `MissionProgress.cs` with this:

```csharp
using System.Collections.Generic;

public static class MissionProgress
{
    private static readonly HashSet<string> completedMissionIds = new HashSet<string>();

    public static bool IsCompleted(string missionId)
    {
        return !string.IsNullOrWhiteSpace(missionId) &&
               completedMissionIds.Contains(missionId);
    }

    public static void MarkCompleted(string missionId)
    {
        if (!string.IsNullOrWhiteSpace(missionId))
            completedMissionIds.Add(missionId);
    }

    public static bool IsUnlocked(MissionData mission)
    {
        if (mission == null)
            return false;

        if (mission.unlockedByDefault)
            return true;

        if (mission.requiredCompletedMissionIds == null || mission.requiredCompletedMissionIds.Length == 0)
            return false;

        foreach (string requiredId in mission.requiredCompletedMissionIds)
        {
            if (!completedMissionIds.Contains(requiredId))
                return false;
        }

        return true;
    }

    public static List<string> GetCompletedMissionIds()
    {
        return new List<string>(completedMissionIds);
    }

    public static void LoadFromSaveData(SaveData saveData)
    {
        completedMissionIds.Clear();

        if (saveData == null || saveData.completedMissionIds == null)
            return;

        foreach (string missionId in saveData.completedMissionIds)
        {
            if (!string.IsNullOrWhiteSpace(missionId))
                completedMissionIds.Add(missionId);
        }
    }

    public static void WriteToSaveData(SaveData saveData)
    {
        if (saveData == null)
            return;

        saveData.completedMissionIds = GetCompletedMissionIds();
    }

    public static void ResetProgress()
    {
        completedMissionIds.Clear();
    }
}
```

Now the mission system can be loaded from disk and written back to disk.

---

# Step 4 - Create PlayerProgress for XP

Create:

```text
Assets/_EchoSystemsLab/Scripts/Progression/PlayerProgress.cs
```

```csharp
using System.Collections.Generic;

public static class PlayerProgress
{
    private static readonly Dictionary<string, int> weaponTypeXp = new Dictionary<string, int>();

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

    public static void LoadFromSaveData(SaveData saveData)
    {
        weaponTypeXp.Clear();

        if (saveData == null || saveData.weaponTypeXpEntries == null)
            return;

        foreach (WeaponTypeXpEntry entry in saveData.weaponTypeXpEntries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.weaponType))
                continue;

            weaponTypeXp[entry.weaponType] = entry.xp;
        }
    }

    public static void WriteToSaveData(SaveData saveData)
    {
        if (saveData == null)
            return;

        saveData.weaponTypeXpEntries.Clear();

        foreach (KeyValuePair<string, int> pair in weaponTypeXp)
            saveData.weaponTypeXpEntries.Add(new WeaponTypeXpEntry(pair.Key, pair.Value));
    }

    public static void ResetProgress()
    {
        weaponTypeXp.Clear();
    }
}
```

This is the beginning of the eventual everything-has-XP system.

Later this can expand into:

```text
SkillProgress
WeaponProgress
TraversalProgress
InteractionProgress
ComputerUseProgress
DialogueProgress
```

For now, just weapon type XP.

---

# Step 5 - Change TargetRangeTracker XP to Usage-Based

In `TargetRangeTracker.RegisterShot`, add XP on shot:

```csharp
public void RegisterShot(string weaponId, string weaponType)
{
    shotsFired++;

    PlayerProgress.AddWeaponTypeXp(weaponType, 1);

    Debug.Log($"Shot fired with {weaponId}. Total shots: {shotsFired}");
    Debug.Log($"{weaponType} XP: {PlayerProgress.GetWeaponTypeXp(weaponType)}");

    NotifyStatsChanged();
}
```

Then change the HUD XP display to use weapon type XP instead of the tracker’s session dictionary.

In `TargetRangeHUD.Refresh()` replace:

```csharp
int xp = tracker.GetWeaponXp(tracker.CurrentWeaponId);
weaponXpText.text = $"Weapon XP: {xp}";
```

with:

```csharp
int xp = PlayerProgress.GetWeaponTypeXp(tracker.CurrentWeaponType);
weaponXpText.text = $"Weapon XP: {xp}";
```

For now, the tracker’s internal weapon XP dictionary can remain in place until the new save flow is proven.

---

# Step 6 - Auto-Save on Trial Completion

In `TargetRangeTracker.CompleteTrial()`, after this:

```csharp
MissionProgress.MarkCompleted(missionIdToComplete);
```

Add:

```csharp
if (SaveManager.Instance != null)
    SaveManager.Instance.SaveGame();
```

So the player completes the trial, and the completion survives a quit/reboot.

This matters because otherwise `MissionProgress` is only memory until the player manually saves.

---

# Step 7 - Make Completed Missions Replayable

In `MissionButtonUI.Setup`, find this line:

```csharp
button.interactable = unlocked && !completed;
```

Change it to:

```csharp
button.interactable = unlocked;
```

Keep the status text as:

```text
COMPLETED
AVAILABLE
LOCKED
```

Now completed trials can be replayed.

This is a small change with a big design payoff.

---

# Step 8 - Create Main Menu Scene

Create scene:

```text
Assets/_EchoSystemsLab/Scenes/MainMenu.unity
```

Add to Build Settings above Hub:

```text
0 - MainMenu
1 - Hub
2 - TargetRangeTrial
3 - CombatTrial
```

Scene hierarchy:

```text
MainMenu
 ├── EventSystem
 ├── _SystemBootstrap
 │    ├── GameSceneLoader
 │    └── SaveManager
 └── Canvas_MainMenu
      ├── TitleText
      ├── NewGameButton
      ├── LoadGameButton
      ├── SettingsButton
      ├── CreditsButton
      └── QuitButton
```

Settings and Credits can be stubbed:

```text
Settings: Debug.Log("Settings coming soon.")
Credits: Debug.Log("Credits coming soon.")
```

---

# Step 9 - Create MainMenuController

Create:

```text
Assets/_EchoSystemsLab/Scripts/Menus/MainMenuController.cs
```

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Scenes")]
    [SerializeField] private string hubSceneName = "Hub";

    private void Awake()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettingsStub);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(OpenCreditsStub);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        if (loadGameButton != null && SaveManager.Instance != null)
            loadGameButton.interactable = SaveManager.Instance.HasSaveFile();
    }

    private void NewGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.NewGame();

        SceneManager.LoadScene(hubSceneName);
    }

    private void LoadGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.LoadGame();

        SceneManager.LoadScene(hubSceneName);
    }

    private void OpenSettingsStub()
    {
        Debug.Log("Settings menu stub.");
    }

    private void OpenCreditsStub()
    {
        Debug.Log("Credits menu stub.");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
```

Attach to:

```text
Canvas_MainMenu
```

Assign buttons.

---

# Step 10 - Create Pause Menu UI

In gameplay scenes, create:

```text
Canvas_PauseMenu
 ├── PauseRoot
 │    ├── ResumeButton
 │    ├── SaveButton
 │    ├── LoadButton
 │    ├── SettingsButton
 │    ├── KeybindsButton
 │    ├── MainMenuButton
 │    └── QuitButton
```

Keep `PauseRoot` inactive by default.

---

# Step 11 - Create PauseMenuController

Create:

```text
Assets/_EchoSystemsLab/Scripts/Menus/PauseMenuController.cs
```

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private SimpleFirstPersonController playerController;
    [SerializeField] private PlayerInteractor playerInteractor;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button keybindsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (saveButton != null)
            saveButton.onClick.AddListener(Save);

        if (loadButton != null)
            loadButton.onClick.AddListener(Load);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(SettingsStub);

        if (keybindsButton != null)
            keybindsButton.onClick.AddListener(KeybindsStub);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        SetPaused(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        SetPaused(true);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pauseRoot != null)
            pauseRoot.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        if (playerController != null)
        {
            playerController.SetInputEnabled(!paused);
            playerController.SetCursorLocked(!paused);
        }

        if (playerInteractor != null)
            playerInteractor.enabled = !paused;
    }

    private void Save()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    private void Load()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.LoadGame();

        Resume();
    }

    private void SettingsStub()
    {
        Debug.Log("Settings menu stub.");
    }

    private void KeybindsStub()
    {
        Debug.Log("Keybinds menu stub.");
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
```

Attach in both:

```text
Hub
TargetRangeTrial
```

Eventually this becomes a prefab.

---

# Step 12 - Update GameSceneLoader for Main Menu

Add this field and method to `GameSceneLoader`:

```csharp
[SerializeField] private string mainMenuSceneName = "MainMenu";

public void LoadMainMenu()
{
    SceneManager.LoadScene(mainMenuSceneName);
}
```

Optional, but keeps scene loading centralized.

---

# Step 13 - Unity Setup Checklist

## MainMenu Scene

```text
_SystemBootstrap
- GameSceneLoader
- SaveManager

Canvas_MainMenu
- MainMenuController
- NewGameButton
- LoadGameButton
- SettingsButton
- CreditsButton
- QuitButton
```

## Hub Scene

```text
_SystemBootstrap
- GameSceneLoader
- SaveManager

Canvas_PauseMenu
- PauseMenuController

MissionTerminal
- Completed missions should remain clickable
```

## TargetRangeTrial Scene

```text
TargetRangeTracker
- Mission Id To Complete = TargetRangeTrial

Canvas_TargetRangeHUD
- Existing HUD

Canvas_PauseMenu
- PauseMenuController
```

## Mission Data

```text
MissionData_TargetRangeTrial
- Unlocked By Default = true

MissionData_CombatTrial
- Required Completed Mission Ids:
  - TargetRangeTrial
```

---

# Goal Line / Completion Checklist

This milestone is done when:

```text
1. Game starts at MainMenu.
2. New Game clears old progress and loads Hub.
3. Hub terminal shows Target Range Trial available.
4. Complete Target Range Trial.
5. Completion auto-saves.
6. Press R returns to Hub.
7. Hub terminal shows Target Range Trial completed.
8. Combat Trial is available.
9. Target Range Trial remains clickable and replayable.
10. Quit play mode / close build / restart test.
11. Load Game restores completed Target Range Trial.
12. Combat Trial remains unlocked after reboot.
13. Escape opens pause menu in gameplay scenes.
14. Resume closes pause menu.
15. Save button writes progress.
16. Load button reloads progress.
17. Main Menu button returns to MainMenu.
18. Quit button exits build.
```

---

# Suggested Commit

```bash
git add .
git commit -m "Add save load foundation and menu flow"
```

Optional tag:

```bash
git tag milestone-2a-save-load-foundation
```

---

# Design Notes

This milestone establishes three types of state:

```text
Persistent Profile State
- Completed missions
- Weapon type XP
- Future skill XP
- Future inventory
- Future character data

Session / Scene State
- Target range shots
- Target range hits
- Current score
- Current trial targets destroyed

Settings State
- Audio
- Graphics
- Controls
- Accessibility
```

For now, `SaveData` handles persistent profile state.

Later, settings can become:

```text
SettingsData
SettingsManager
```

And character/inventory can become:

```text
CharacterData
InventoryData
SkillProgressData
```

Do not overload one script with every future thing. Let the save file be the crate. Let each system pack its own tools into the crate.

---

# Small Polish Backlog

Keep these as post-2A tasks or quick side quests:

```text
Completed missions should stay replayable.
Add small flash to targets on hit.
Debug projectile misses with SphereCast or larger projectile trigger.
Move XP to weapon type usage-based tracking.
Import fonts.
Create color palettes.
Build Character screen.
Build Inventory system.
Add more item types.
Upgrade to New Input System.
```

---

# Portfolio Value

This milestone demonstrates:

```text
Persistent save/load architecture
Mission progression persistence
Replayable mission structure
Main menu flow
Pause menu flow
Auto-save on mission completion
Manual save/load hooks
Persistent XP foundation
Separation between persistent data and session data
Expandable UI/menu framework
```

This is the moment Echo Systems Lab stops being a set of rooms and becomes a proper framework with memory, menus, and progression.

---

# Future Portfolio Card Draft

## Save / Load Foundation and Menu Flow

Built a persistent save/load foundation for Echo Systems Lab, including profile progression, replayable mission completion, weapon type XP persistence, New Game / Load Game menu flow, and pause menu save/load hooks. The system separates persistent profile data from temporary scene/session data, creating a scalable foundation for future inventory, character progression, settings, and skill systems.
