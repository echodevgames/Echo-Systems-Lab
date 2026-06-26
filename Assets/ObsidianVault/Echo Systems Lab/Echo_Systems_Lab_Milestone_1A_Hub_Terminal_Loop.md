---
title: Echo Systems Lab - Milestone 1 Hub Terminal Loop
project: Echo Systems Lab
type: design-doc
status: draft
tags:
  - unity
  - gameplay-systems
  - portfolio
  - first-person
  - missions
  - interaction-system
  - scene-loader
---

# Echo Systems Lab - Milestone 1 Hub Terminal Loop

## Purpose

This milestone creates the first working spine of **Echo Systems Lab**.

The goal is to build a simple first-person hub room where the player can walk up to a computer terminal, see an interact prompt, open a mission UI, view locked and unlocked system trials, and load the first available scene.

This becomes the foundation for the larger portfolio framework:

> Hub → Mission Board → System Trial → Complete Objective → Save Progress → Unlock Next Trial

The first version should stay lean, clean, and reusable. The point is not visual polish yet. The point is to establish a modular architecture that can become template code for future games.

---

# Milestone 1 System Stack

The first implementation should include:

- First-person player controller
- Raycast-based interaction system
- Interact prompt UI
- Mission data using ScriptableObjects
- Mission terminal UI
- Locked/unlocked mission display
- Scene loader
- Placeholder progression tracking

---

# Suggested Folder Structure

```text
Assets/_EchoSystemsLab/
 ├── Scripts/
 │   ├── Player/
 │   ├── Interaction/
 │   ├── Missions/
 │   ├── UI/
 │   └── SceneManagement/
 ├── ScriptableObjects/
 │   └── Missions/
 ├── Prefabs/
 │   └── UI/
 └── Scenes/
```

---

# Core Loop For This Milestone

```text
Player spawns in Hub
↓
Player walks to computer
↓
Interaction prompt appears
↓
Player presses E
↓
Mission terminal UI opens
↓
Available and locked missions are displayed
↓
Player selects unlocked mission
↓
Scene loader loads selected mission scene
```

---

# Scene Setup

## Scene Name

Save the first scene as:

```text
Hub
```

Create a second scene, even if it is empty for now:

```text
CombatTrial
```

Add both scenes to Build Settings:

```text
Hub
CombatTrial
```

---

# Hub Scene Objects

The hub scene should contain:

```text
- Player
- Camera
- Canvas
- EventSystem
- _SystemBootstrap
- Desk
- Computer
- Placeholder props
```

The room can be very simple. A desk, a computer, some walls, a floor, and a light are enough for the first pass.

---

# 1. Simple First-Person Player Controller

Create:

```text
Assets/_EchoSystemsLab/Scripts/Player/SimpleFirstPersonController.cs
```

```csharp
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float cameraPitch;
    private bool inputEnabled = true;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        SetCursorLocked(true);
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = move.normalized * moveSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = move;
        finalMove.y = verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
```

## Player GameObject Setup

```text
Player
 ├── Optional capsule mesh
 ├── CharacterController
 ├── SimpleFirstPersonController
 ├── PlayerInteractor
 └── Camera
```

Camera recommended local position:

```text
Position: 0, 1.6, 0
```

---

# 2. Interactable Interface

Create:

```text
Assets/_EchoSystemsLab/Scripts/Interaction/IInteractable.cs
```

```csharp
public interface IInteractable
{
    string GetPromptText();
    void Interact(PlayerInteractor interactor);
}
```

This interface becomes the shared contract for every future interactable object:

- Computers
- Doors
- NPCs
- Pickups
- Mission boards
- Switches
- Loot chests
- Debug terminals

---

# 3. Interaction Prompt UI

Create:

```text
Assets/_EchoSystemsLab/Scripts/UI/InteractionPromptUI.cs
```

```csharp
using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        if (promptRoot != null)
            promptRoot.SetActive(true);

        if (promptText != null)
            promptText.text = text;
    }

    public void Hide()
    {
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }
}
```

## UI Setup

```text
Canvas
 └── InteractionPromptRoot
      └── TMP_Text: "Press E"
```

Set `InteractionPromptRoot` inactive by default or let the script hide it on Awake.

---

# 4. Player Interactor

Create:

```text
Assets/_EchoSystemsLab/Scripts/Interaction/PlayerInteractor.cs
```

```csharp
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionPromptUI promptUI;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
            currentInteractable.Interact(this);
    }

    private void CheckForInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                promptUI.Show(currentInteractable.GetPromptText());
                return;
            }
        }

        promptUI.Hide();
    }
}
```

## PlayerInteractor Setup

Add `PlayerInteractor` to the Player.

Assign:

```text
Player Camera = Player child camera
Prompt UI = InteractionPromptUI component
Interact Distance = 3
Interact Key = E
Interact Mask = Interactable
```

Create a Unity layer:

```text
Interactable
```

Set the computer object to this layer.

---

# 5. Mission Data

Create:

```text
Assets/_EchoSystemsLab/Scripts/Missions/MissionData.cs
```

```csharp
using UnityEngine;

[CreateAssetMenu(
    fileName = "MissionData_NewMission",
    menuName = "Echo Systems Lab/Missions/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Identity")]
    public string missionId;
    public string displayName;

    [TextArea(3, 6)]
    public string description;

    [Header("Scene")]
    public string sceneName;

    [Header("Progression")]
    public bool unlockedByDefault;
    public string[] requiredCompletedMissionIds;
}
```

## Example Mission Assets

Create these under:

```text
Assets/_EchoSystemsLab/ScriptableObjects/Missions/
```

Suggested mission assets:

```text
MissionData_CombatTrial
MissionData_AITrial
MissionData_DialogueTrial
MissionData_InventoryTrial
MissionData_CameraTrial
MissionData_BossTrial
```

## First Mission Setup

For `MissionData_CombatTrial`:

```text
missionId = CombatTrial
displayName = Combat Trial
description = Enter a training chamber and defeat all active drones.
sceneName = CombatTrial
unlockedByDefault = true
requiredCompletedMissionIds = empty
```

For later missions:

```text
unlockedByDefault = false
requiredCompletedMissionIds = Previous mission ID
```

Example:

```text
AITrial requires CombatTrial
DialogueTrial requires AITrial
InventoryTrial requires DialogueTrial
CameraTrial requires InventoryTrial
BossTrial requires CameraTrial
```

---

# 6. Temporary Mission Progress

This is not the final save system.

This is a temporary in-memory progress tracker so the UI can work now.

Create:

```text
Assets/_EchoSystemsLab/Scripts/Missions/MissionProgress.cs
```

```csharp
using System.Collections.Generic;

public static class MissionProgress
{
    private static readonly HashSet<string> completedMissionIds = new HashSet<string>();

    public static bool IsCompleted(string missionId)
    {
        return completedMissionIds.Contains(missionId);
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

    public static void ResetProgress()
    {
        completedMissionIds.Clear();
    }
}
```

Later this can be replaced by, or wrapped inside, a real `SaveManager`.

---

# 7. Scene Loader

Create:

```text
Assets/_EchoSystemsLab/Scripts/SceneManagement/GameSceneLoader.cs
```

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    public static GameSceneLoader Instance { get; private set; }

    [SerializeField] private string hubSceneName = "Hub";

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

    public void QuitGame()
    {
        Application.Quit();
    }
}
```

## Bootstrap Setup

Create an empty GameObject:

```text
_SystemBootstrap
 └── GameSceneLoader
```

This object persists between scenes.

Later, `_SystemBootstrap` can become the home for other persistent services:

- SaveManager
- AudioManager
- GameSettings
- DebugService
- SceneTransitionManager

---

# 8. Mission Button UI

Create:

```text
Assets/_EchoSystemsLab/Scripts/UI/MissionButtonUI.cs
```

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button button;

    private MissionData mission;

    public void Setup(MissionData missionData, bool unlocked, bool completed, System.Action<MissionData> onClicked)
    {
        mission = missionData;

        if (titleText != null)
            titleText.text = mission.displayName;

        if (descriptionText != null)
            descriptionText.text = mission.description;

        if (statusText != null)
        {
            if (completed)
                statusText.text = "COMPLETED";
            else if (unlocked)
                statusText.text = "AVAILABLE";
            else
                statusText.text = "LOCKED";
        }

        if (button != null)
        {
            button.interactable = unlocked && !completed;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClicked?.Invoke(mission));
        }
    }
}
```

## Mission Button Prefab Setup

Create a prefab:

```text
MissionButtonPrefab
 ├── Button
 ├── Title TMP_Text
 ├── Description TMP_Text
 └── Status TMP_Text
```

This can be visually rough for now. It only needs to be readable and functional.

---

# 9. Mission Terminal UI

Create:

```text
Assets/_EchoSystemsLab/Scripts/UI/MissionTerminalUI.cs
```

```csharp
using UnityEngine;
using UnityEngine.UI;

public class MissionTerminalUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject terminalRoot;
    [SerializeField] private Transform missionButtonParent;
    [SerializeField] private MissionButtonUI missionButtonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private SimpleFirstPersonController playerController;

    [Header("Mission List")]
    [SerializeField] private MissionData[] missions;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        Close();
    }

    public void Open()
    {
        if (terminalRoot != null)
            terminalRoot.SetActive(true);

        if (playerController != null)
        {
            playerController.SetInputEnabled(false);
            playerController.SetCursorLocked(false);
        }

        PopulateMissionList();
    }

    public void Close()
    {
        if (terminalRoot != null)
            terminalRoot.SetActive(false);

        if (playerController != null)
        {
            playerController.SetInputEnabled(true);
            playerController.SetCursorLocked(true);
        }
    }

    private void PopulateMissionList()
    {
        ClearMissionButtons();

        foreach (MissionData mission in missions)
        {
            if (mission == null)
                continue;

            MissionButtonUI buttonInstance = Instantiate(missionButtonPrefab, missionButtonParent);

            bool unlocked = MissionProgress.IsUnlocked(mission);
            bool completed = MissionProgress.IsCompleted(mission.missionId);

            buttonInstance.Setup(mission, unlocked, completed, OnMissionClicked);
        }
    }

    private void ClearMissionButtons()
    {
        for (int i = missionButtonParent.childCount - 1; i >= 0; i--)
            Destroy(missionButtonParent.GetChild(i).gameObject);
    }

    private void OnMissionClicked(MissionData mission)
    {
        GameSceneLoader.Instance.LoadMissionScene(mission);
    }
}
```

## Terminal UI Setup

```text
Canvas
 ├── InteractionPromptRoot
 └── MissionTerminalRoot
      ├── Header: "Mission Terminal"
      ├── Scroll View
      │    └── Content
      └── Close Button
```

Set `MissionTerminalRoot` inactive by default.

Assign in the Inspector:

```text
terminalRoot = MissionTerminalRoot
missionButtonParent = Scroll View / Viewport / Content
missionButtonPrefab = MissionButtonPrefab
closeButton = Close Button
playerController = Player
missions = all MissionData assets
```

---

# 10. Mission Computer

Create:

```text
Assets/_EchoSystemsLab/Scripts/Missions/MissionComputer.cs
```

```csharp
using UnityEngine;

public class MissionComputer : MonoBehaviour, IInteractable
{
    [SerializeField] private MissionTerminalUI terminalUI;
    [SerializeField] private string promptText = "Press E to open Mission Terminal";

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (terminalUI != null)
            terminalUI.Open();
    }
}
```

## Computer Setup

```text
Desk
 └── Computer
      ├── Collider
      ├── MissionComputer
      └── Layer: Interactable
```

Assign:

```text
terminalUI = MissionTerminalUI in Canvas
```

---

# First Scene Checklist

## Project Setup

- [ ] Create `_EchoSystemsLab` folder
- [ ] Create script folders
- [ ] Create `Hub` scene
- [ ] Create `CombatTrial` scene
- [ ] Add both scenes to Build Settings

## Player Setup

- [ ] Create Player GameObject
- [ ] Add CharacterController
- [ ] Add child Camera
- [ ] Add `SimpleFirstPersonController`
- [ ] Add `PlayerInteractor`
- [ ] Assign player camera
- [ ] Assign prompt UI
- [ ] Set interact mask to `Interactable`

## UI Setup

- [ ] Create Canvas
- [ ] Create InteractionPromptRoot
- [ ] Add TMP prompt text
- [ ] Add `InteractionPromptUI`
- [ ] Create MissionTerminalRoot
- [ ] Add Scroll View
- [ ] Add Close Button
- [ ] Add `MissionTerminalUI`
- [ ] Create `MissionButtonPrefab`

## Mission Setup

- [ ] Create `MissionData_CombatTrial`
- [ ] Create placeholder locked mission assets
- [ ] Assign all missions to `MissionTerminalUI`
- [ ] Make Combat Trial unlocked by default
- [ ] Make all other missions locked behind previous missions

## Computer Setup

- [ ] Add desk or table prop
- [ ] Add computer prop
- [ ] Add Collider to computer
- [ ] Set computer layer to `Interactable`
- [ ] Add `MissionComputer`
- [ ] Assign terminal UI

## Bootstrap Setup

- [ ] Create `_SystemBootstrap`
- [ ] Add `GameSceneLoader`
- [ ] Set hub scene name to `Hub`

---

# Expected Result

After this milestone works, the player should be able to:

- [ ] Walk around in first person
- [ ] Look at the computer
- [ ] See an interaction prompt
- [ ] Press E
- [ ] Open the mission terminal
- [ ] See available, locked, and completed states
- [ ] Select the first unlocked mission
- [ ] Load the Combat Trial scene

---

# Portfolio Value

This milestone demonstrates the foundation of several reusable gameplay systems:

- Player input and control
- Raycast interaction
- UI state management
- ScriptableObject-driven mission data
- Mission progression rules
- Scene loading
- Early service/bootstrap structure

Even before the combat trial exists, this establishes a scalable architecture.

The important portfolio message:

> I can design a gameplay framework where features are modular, data-driven, and reusable across multiple game systems.

---

# Next Milestone

## Milestone 2 - Combat Trial

Goal:

The first loaded system scene should spawn a simple combat objective.

Proposed flow:

```text
Load CombatTrial
↓
Objective appears: Defeat all training drones
↓
Three simple enemies spawn
↓
Player defeats all enemies
↓
MissionProgress.MarkCompleted("CombatTrial")
↓
Return to Hub
↓
Mission Terminal now shows Combat Trial as completed
↓
AI Trial unlocks
```

Milestone 2 should introduce:

- Health system
- Damage system
- Simple enemy target dummy or drone
- Objective tracker
- Mission completion event
- Return-to-hub logic

---

# Notes

This system is intentionally simple for the first pass.

Avoid overbuilding at this stage.

Do not add:

- Full save/load yet
- Complex animations
- Advanced UI transitions
- Procedural generation
- Full objective framework
- Multiple player abilities

First, get the loop working.

Then polish and expand.
