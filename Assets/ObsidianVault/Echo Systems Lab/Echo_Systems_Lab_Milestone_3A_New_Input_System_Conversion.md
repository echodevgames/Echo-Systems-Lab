---
title: Milestone 3A - New Unity Input System Conversion
project: Echo Systems Lab
type: checkpoint-build-plan
tags:
  - unity
  - systems-design
  - input-system
  - controls
  - refactor
  - portfolio
---

# Milestone 3A - New Unity Input System Conversion

This milestone converts Echo Systems Lab from Unity's old input calls to the New Unity Input System. It is a foundation refactor that makes future keybinding, controller support, accessibility, and UI input work much cleaner.

---

# Goal Line

```text
Project uses Unity New Input System
→ Player movement/look uses input actions
→ Interact uses input actions
→ Fire/reload/cycle weapon uses input actions
→ Pause uses input actions
→ Terminal and pause menus lock gameplay input correctly
→ UI still works
→ Old Input.GetKey / GetAxis calls are removed from core gameplay scripts
→ Active Input Handling can be switched from Both to Input System Package
→ Project is ready for keybinding UI later
```

---

# Starting Point

Before this milestone, Echo Systems Lab already had:

```text
TargetRangeTrial loop complete
Mission terminal flow
Save/load persistence
Persistent weapon ownership
Persistent active weapon
WeaponDatabase lookup
AmmoData ScriptableObjects
Multiple ranged weapons
Weapon pedestals
Weapon cycling
Projectile firing patterns
Shotgun multi-projectile support
Magazine / reload foundation
Target hit and destroy XP
Pause menu
Reusable mission terminal prefab
```

The old input approach still used:

```text
Input.GetAxis
Input.GetAxisRaw
Input.GetKey
Input.GetKeyDown
KeyCode fields
```

This milestone replaces those with a central input reader.

---

# Existing Input Asset Decision

Use the existing project asset:

```text
InputSystem_Actions
```

The existing asset already includes useful actions:

```text
Player / Move
Player / Look
Player / Attack
Player / Interact
Player / Previous
Player / Next
UI / Navigate
UI / Click
UI / Cancel
```

Add only the missing gameplay actions:

```text
Player / Reload
Player / Pause
```

Recommended mappings:

```text
Move = WASD / Left Stick
Look = Mouse Delta / Right Stick
Attack = Left Mouse Button
Interact = E
Reload = R
Next = 2 or Tab
Previous = 1
Pause = Escape
```

For this milestone, `Reload` can also double as the post-trial "return to hub" input.

---

# System Pieces

```text
1. Existing InputSystem_Actions asset
2. Generated InputSystem_Actions C# class
3. PlayerInputReader wrapper
4. SimpleFirstPersonController conversion
5. PlayerInteractor conversion
6. PlayerWeaponController conversion
7. PlayerWeaponLoadoutController conversion
8. PauseMenuController conversion
9. MissionTerminalUI input lock update
10. TargetRangeTracker return-to-hub conversion
11. UI EventSystem conversion
12. Active Input Handling switch
```

---

# Step 1 - Commit Current Working TargetRangeTrial

```bash
git add .
git commit -m "Complete target range arsenal loop"
```

Optional tag:

```bash
git tag milestone-2c-target-range-arsenal
```

---

# Step 2 - Enable New Input System Safely

In Unity:

```text
Edit > Project Settings > Player > Other Settings > Active Input Handling
```

Set it to:

```text
Both
```

Use `Both` during migration so old scripts still work while each file is converted.

After conversion is complete, switch to:

```text
Input System Package
```

---

# Step 3 - Prepare InputSystem_Actions

Open:

```text
InputSystem_Actions.inputactions
```

Confirm these Player actions exist:

```text
Move
Look
Attack
Interact
Next
Previous
Reload
Pause
```

If `Reload` or `Pause` is missing, add them:

```text
Reload
Type: Button
Binding: R

Pause
Type: Button
Binding: Escape
```

Then enable code generation:

```text
Generate C# Class: checked
Class Name: InputSystem_Actions
```

Click:

```text
Apply
```

Unity should generate:

```text
InputSystem_Actions.cs
```

---

# Step 4 - Create PlayerInputReader

Create:

```text
Assets/_EchoSystemsLab/Scripts/Input/PlayerInputReader.cs
```

The input reader becomes the single gameplay input doorway. It exposes:

```text
MoveInput
LookInput
InteractPressed
FirePressed
FireHeld
ReloadPressed
CycleNextWeaponPressed
CyclePreviousWeaponPressed
PausePressed
SetGameplayInputEnabled(bool)
```

Important rule:

```text
PausePressed should not be gated by gameplayInputEnabled.
```

That allows Escape to unpause the game even while gameplay input is locked.

---

# Step 5 - Convert Core Scripts

Convert in this order:

```text
1. SimpleFirstPersonController
2. PlayerInteractor
3. PlayerWeaponController
4. PlayerWeaponLoadoutController
5. PauseMenuController
6. MissionTerminalUI
7. TargetRangeTracker
```

Each script should stop asking Unity input directly and instead read from `PlayerInputReader`.

Examples:

```text
SimpleFirstPersonController
- inputReader.MoveInput
- inputReader.LookInput

PlayerInteractor
- inputReader.InteractPressed

PlayerWeaponController
- inputReader.FirePressed
- inputReader.FireHeld
- inputReader.ReloadPressed

PlayerWeaponLoadoutController
- inputReader.CycleNextWeaponPressed
- inputReader.CyclePreviousWeaponPressed

PauseMenuController
- inputReader.PausePressed

TargetRangeTracker
- inputReader.ReloadPressed after trial completion
```

---

# Step 6 - Input Locking Rules

When paused:

```text
gameplay input disabled
movement disabled
interactor disabled
weapon controller disabled
weapon cycling disabled
cursor unlocked
pause UI shown
```

When terminal opens:

```text
gameplay input disabled
movement disabled
interactor disabled
weapon controller disabled
weapon cycling disabled
cursor unlocked
terminal UI shown
```

When closing either menu:

```text
gameplay input enabled
movement enabled
interactor enabled
weapon controller enabled
weapon cycling enabled
cursor locked
menu UI hidden
```

---

# Step 7 - UI EventSystem Conversion

In scenes with an EventSystem:

```text
Remove Standalone Input Module
Add Input System UI Input Module
```

This lets UI use the New Input System too.

---

# Step 8 - Switch Active Input Handling

After old calls are gone from core gameplay scripts:

```text
Edit > Project Settings > Player > Other Settings > Active Input Handling
```

Set to:

```text
Input System Package
```

Then test again.

---

# Completion Checklist

```text
1. Player moves with WASD.
2. Player looks with mouse.
3. E interacts with mission terminal and weapon pedestals.
4. Left click fires current weapon.
5. Automatic weapons fire while holding attack.
6. R reloads current weapon.
7. 2 or Tab cycles next weapon.
8. 1 cycles previous weapon.
9. Escape opens pause menu.
10. Escape closes pause menu.
11. Terminal opens and unlocks mouse.
12. Terminal blocks movement, look, fire, reload, cycle, and interact.
13. Pause menu blocks movement, look, fire, reload, cycle, and interact.
14. Trial completion still allows R to return to Hub.
15. UI buttons still click.
16. Core gameplay scripts no longer use Input.GetKey, Input.GetKeyDown, Input.GetAxis, or Input.GetAxisRaw.
17. Active Input Handling can be switched to Input System Package.
```

---

# Suggested Commit

```bash
git add .
git commit -m "Convert gameplay controls to Unity Input System"
```

Optional tag:

```bash
git tag milestone-3a-new-input-system
```

---

# Design Notes

## PlayerInputReader is the input doorway

Gameplay scripts no longer talk directly to Unity input calls.

Instead:

```text
SimpleFirstPersonController asks PlayerInputReader for MoveInput and LookInput.
PlayerInteractor asks PlayerInputReader for InteractPressed.
PlayerWeaponController asks PlayerInputReader for FirePressed, FireHeld, and ReloadPressed.
PlayerWeaponLoadoutController asks PlayerInputReader for CycleNextWeaponPressed and CyclePreviousWeaponPressed.
PauseMenuController asks PlayerInputReader for PausePressed.
TargetRangeTracker asks PlayerInputReader for ReloadPressed after completion.
```

This creates a clean bridge to keybinding, controller support, UI navigation, and accessibility features.

---

# Portfolio Value

```text
Unity New Input System migration
Input actions asset integration
Centralized input wrapper
Gameplay input locking for UI states
Pause-safe input handling
Reusable input architecture
Foundation for keybinding UI
Controller-ready control design
```

---

# Future Portfolio Card Draft

## New Input System Conversion

Converted Echo Systems Lab from legacy Unity input calls to the New Unity Input System. Built a centralized PlayerInputReader around the generated InputSystem_Actions asset and migrated movement, camera look, interaction, weapon firing, reload, weapon cycling, pause, mission terminal lockouts, and target range completion input. This milestone establishes a rebinding-ready and controller-friendly input foundation for future UI, accessibility, and keybinding systems.
