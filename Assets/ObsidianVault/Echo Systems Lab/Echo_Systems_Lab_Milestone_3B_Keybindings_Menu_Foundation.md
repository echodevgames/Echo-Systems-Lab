---
title: Milestone 3B - Keybindings Menu Foundation
project: Echo Systems Lab
type: checkpoint-build-plan
tags:
  - unity
  - systems-design
  - input-system
  - keybindings
  - ui
  - pause-menu
  - portfolio
---

# Milestone 3B - Keybindings Menu Foundation

This milestone starts the UI systems mountain by building a keybindings menu inside the pause menu.

Since Milestone 3A converted gameplay to Unity's New Input System, this is the natural next step. The project now has a rebinding-ready input foundation, so the pause menu can begin exposing that system to the player.

---

# Goal Line

```text
Pause Menu opens
→ Keybindings button opens a Keybindings panel
→ Gameplay controls stay locked while menu is open
→ Player can view current bindings
→ Player can rebind actions one at a time
→ Rebinds persist through reboot
→ Reset bindings button restores defaults
→ Back returns to Pause Menu
```

---

# Current UI Roadmap

Standing list:

```text
Keybindings menu
Credits menu
Weapon/ammo HUD
Health, score, level, XP bar
Mana/stamina placeholders
Scrolling weapon bandolier UI
Character menu
Inventory menu
```

This milestone tackles only:

```text
Keybindings menu
```

The rest stay in the backlog.

---

# System Pieces

```text
1. PlayerInputReader binding persistence methods
2. Keybindings panel in Pause Menu
3. KeybindingRowUI prefab
4. KeybindingsMenuUI controller
5. PauseMenuController submenu flow
6. Runtime rebinding
7. Binding override saving/loading
8. Reset single binding
9. Reset all bindings
10. Test pass and commit
```

---

# Folder Setup

Create or confirm these folders:

```text
Assets/_EchoSystemsLab/Scripts/UI/
Assets/_EchoSystemsLab/Scripts/Input/
Assets/_EchoSystemsLab/Prefabs/UI/Menus/
Assets/_EchoSystemsLab/Prefabs/UI/Rows/
```

---

# Step 1 - Update PlayerInputReader for Binding Persistence

Because gameplay scripts use the generated `InputSystem_Actions` instance, the keybindings menu needs access to that same runtime input asset.

Add this using statement:

```csharp
using UnityEngine.InputSystem;
```

Add these fields and methods inside `PlayerInputReader`:

```csharp
private const string BindingOverridesKey = "EchoSystemsLab_BindingOverrides";

public InputActionAsset InputActionAsset => inputActions.asset;

public InputAction FindAction(string actionPath)
{
    if (inputActions == null)
        return null;

    return inputActions.asset.FindAction(actionPath, false);
}

public void SaveBindingOverrides()
{
    string json = inputActions.asset.SaveBindingOverridesAsJson();
    PlayerPrefs.SetString(BindingOverridesKey, json);
    PlayerPrefs.Save();

    Debug.Log("Binding overrides saved.");
}

public void LoadBindingOverrides()
{
    if (!PlayerPrefs.HasKey(BindingOverridesKey))
        return;

    string json = PlayerPrefs.GetString(BindingOverridesKey);
    inputActions.asset.LoadBindingOverridesFromJson(json);

    Debug.Log("Binding overrides loaded.");
}

public void ResetBindingOverrides()
{
    inputActions.asset.RemoveAllBindingOverrides();
    PlayerPrefs.DeleteKey(BindingOverridesKey);
    PlayerPrefs.Save();

    Debug.Log("Binding overrides reset.");
}
```

Then in `Awake()`, after:

```csharp
inputActions = new InputSystem_Actions();
```

add:

```csharp
LoadBindingOverrides();
```

This gives the keybindings menu a way to:

```text
Find actions
Read bindings
Save overrides
Load overrides
Reset overrides
```

---

# Step 2 - Create Pause Menu Panel Structure

Under the existing pause menu canvas:

```text
Canvas_PauseMenu
 └── PauseRoot
      ├── PauseMainPanel
      └── KeybindingsPanel
```

Set:

```text
KeybindingsPanel = inactive by default
```

Inside `KeybindingsPanel`:

```text
KeybindingsPanel
 ├── HeaderText
 ├── Scroll View
 │    └── Viewport
 │         └── Content
 ├── ResetBindingsButton
 └── BackButton
```

The `Content` object should have:

```text
Vertical Layout Group
Content Size Fitter
```

Recommended Content Size Fitter settings:

```text
Horizontal Fit: Unconstrained
Vertical Fit: Preferred Size
```

---

# Step 3 - Create KeybindingRowUI Prefab

Prefab structure:

```text
KeybindingRowUI
 ├── ActionNameText
 ├── BindingText
 ├── RebindButton
 └── ResetButton
```

Create script:

```text
Assets/_EchoSystemsLab/Scripts/UI/KeybindingRowUI.cs
```

`KeybindingRowUI` should:

```text
Display the action name
Display the current binding
Tell KeybindingsMenuUI to start rebind
Tell KeybindingsMenuUI to reset this binding
Refresh its binding label after changes
```

Assign in the prefab:

```text
ActionNameText
BindingText
RebindButton
ResetButton
```

---

# Step 4 - Create KeybindingsMenuUI

Create:

```text
Assets/_EchoSystemsLab/Scripts/UI/KeybindingsMenuUI.cs
```

This menu should support composite bindings like WASD by using binding names:

```text
up
down
left
right
```

Suggested binding entries:

```text
Move Forward = Player/Move, bindingName up
Move Backward = Player/Move, bindingName down
Move Left = Player/Move, bindingName left
Move Right = Player/Move, bindingName right
Fire = Player/Attack
Interact = Player/Interact
Reload / Return = Player/Reload
Next Weapon = Player/Next
Previous Weapon = Player/Previous
Pause = Player/Pause
```

The menu controller should support:

```text
Build rows
Refresh rows
Get binding display string
Start interactive rebind
Cancel rebind with Escape
Save binding overrides
Reset one binding
Reset all bindings
Back to pause menu
```

---

# Step 5 - Patch PauseMenuController

The Keybinds button currently calls a stub.

Replace the stub with a real submenu flow.

Add fields:

```csharp
[SerializeField] private GameObject mainPausePanel;
[SerializeField] private KeybindingsMenuUI keybindingsMenuUI;
```

Change the keybinds button listener:

```csharp
if (keybindsButton != null)
    keybindsButton.onClick.AddListener(OpenKeybindings);
```

Add methods:

```csharp
private void OpenKeybindings()
{
    if (mainPausePanel != null)
        mainPausePanel.SetActive(false);

    if (keybindingsMenuUI != null)
        keybindingsMenuUI.Open();
}

public void ShowMainPausePanel()
{
    if (mainPausePanel != null)
        mainPausePanel.SetActive(true);
}
```

Now the flow is:

```text
Pause menu
→ Keybindings
→ Back
→ Pause menu
```

---

# Step 6 - Inspector Setup

On the pause canvas:

```text
Canvas_PauseMenu
 ├── PauseMenuController
 └── KeybindingsMenuUI
```

Assign:

```text
PauseMenuController
- Pause Root = PauseRoot
- Main Pause Panel = PauseMainPanel
- Keybindings Menu UI = KeybindingsMenuUI
- Keybinds Button = KeybindsButton
```

Assign:

```text
KeybindingsMenuUI
- Keybindings Root = KeybindingsPanel
- Row Parent = Scroll View / Viewport / Content
- Row Prefab = KeybindingRowUI
- Back Button = BackButton
- Reset All Button = ResetBindingsButton
- Pause Menu Controller = PauseMenuController
- Input Reader = Player
```

Assign the row prefab:

```text
KeybindingRowUI
- Action Name Text
- Binding Text
- Rebind Button
- Reset Button
```

---

# Step 7 - First Test Pass

Test in this order:

```text
1. Press Escape.
2. Pause menu opens.
3. Click Keybindings.
4. Keybindings panel opens.
5. Rows display current bindings.
6. Click Fire rebind.
7. Press a new key.
8. Fire row updates.
9. Resume game.
10. New fire binding works.
11. Quit play mode.
12. Restart.
13. Binding persists.
14. Press Reset All.
15. Defaults return.
```

---

# Completion Checklist

```text
1. Pause menu opens.
2. Keybindings button opens the keybindings panel.
3. Main pause panel hides while keybindings are open.
4. Back button returns to main pause panel.
5. Current bindings display correctly.
6. WASD movement parts display correctly.
7. Fire, Interact, Reload, Next, Previous, and Pause display correctly.
8. Player can rebind a listed action.
9. Rebound action works in gameplay.
10. Binding override persists after reboot.
11. Single reset button restores one binding.
12. Reset all button restores all defaults.
13. Gameplay input stays locked while the menu is open.
```

---

# Suggested Commit

```bash
git add .
git commit -m "Add keybindings menu with runtime rebinding"
```

Optional tag:

```bash
git tag milestone-3b-keybindings-menu
```

---

# Design Notes

## Keybindings are a direct payoff from Milestone 3A

Milestone 3A created:

```text
InputSystem_Actions
PlayerInputReader
Gameplay input lockouts
Pause-safe input handling
```

Milestone 3B exposes that work through UI.

## PlayerPrefs for binding overrides

Binding overrides are saved using:

```text
PlayerPrefs
```

This is a good first pass for keybindings.

Later, settings can move into a dedicated settings save:

```text
SettingsData
SettingsManager
Audio settings
Graphics settings
Input binding overrides
Accessibility preferences
```

## Rebinding entries are explicit

Rather than auto-generating every possible binding, this milestone lists only the controls that matter to the current project:

```text
Move Forward
Move Backward
Move Left
Move Right
Fire
Interact
Reload / Return
Next Weapon
Previous Weapon
Pause
```

This keeps the UI focused.

---

# Small Polish Backlog After 3B

```text
Add "Listening..." overlay while rebinding.
Add duplicate binding detection.
Add gamepad binding rows.
Add mouse sensitivity setting.
Move binding save data from PlayerPrefs into SettingsData.
Add audio feedback to buttons.
Style the keybindings panel.
Add Credits menu next.
```

---

# Portfolio Value

```text
Runtime input rebinding
Persistent binding overrides
Pause menu submenu architecture
Composite input binding support
Explicit control list UI
Reset single binding
Reset all bindings
Settings-style UI foundation
New Input System feature usage
```

---

# Future Portfolio Card Draft

## Keybindings Menu and Runtime Rebinding

Built a keybindings menu for Echo Systems Lab using Unity's New Input System. The menu displays current gameplay bindings, supports runtime rebinding for movement, combat, interaction, weapon cycling, reload, and pause actions, and persists binding overrides across sessions. The system includes individual binding reset, reset-all support, pause menu submenu flow, and composite WASD binding handling, establishing the foundation for a broader settings and accessibility UI.
