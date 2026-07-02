# Milestone 3F - Weapon Bandolier HUD

## 1. Milestone Title

Milestone 3F - Weapon Bandolier HUD

---

## 2. Goal Line

Pick up multiple weapons.

Owned weapons appear in a weapon bandolier HUD.

The active weapon is highlighted.

Cycling weapons updates:

- Active weapon
- Weapon view model
- Weapon/ammo HUD
- Saved active weapon
- Bandolier highlight

Holding Left Shift opens the bandolier.

Scrolling mouse wheel while holding Left Shift cycles weapons.

Releasing Left Shift hides the bandolier.

Existing Previous and Next inputs remain available.

---

## 3. Naming / Scope Notes

Use Bandolier as the visual weapon-selection HUD.

Use Loadout as the owned/equipped weapon controller logic.

Use WeaponData for the weapon definition.

Use WeaponDatabase for ordered lookup.

This milestone is not a full inventory system yet.

The bandolier is a focused weapon selection UI.

---

## 4. System Pieces

1. Input actions for bandolier hold and mouse scroll
2. PlayerInputReader bandolier input helpers
3. WeaponBandolierHUD
4. WeaponBandolierSlotUI
5. Slot prefab
6. Bandolier root canvas/panel
7. PlayerWeaponLoadoutController scroll support
8. WeaponDatabase owned weapons lookup
9. Active weapon highlight
10. HUD refresh behavior

---

## 5. Folder Setup

```text
Assets/Scripts/UI/HUD/
Assets/Scripts/UI/Weapons/
Assets/Prefabs/UI/HUD/
Assets/Prefabs/UI/Elements/
```

Suggested prefabs:

```text
Canvas_WeaponBandolierHUD
WeaponBandolierSlotUI
```

---

## 6. Numbered Implementation Steps

### Step 1 - Add Input Actions

Add actions to the Player action map:

```text
BandolierHold
BandolierScroll
```

Suggested bindings:

```text
BandolierHold = Left Shift
BandolierScroll = Mouse Scroll / Y
```

Keep existing weapon cycling:

```text
Previous Weapon = 1
Next Weapon = 2
```

Why this matters:

The player gets two weapon selection styles:

- Quick cycling with keys
- Visual bandolier selection with hold plus scroll

---

### Step 2 - Update PlayerInputReader

Add properties:

```text
BandolierHeld
BandolierScrollY
```

Expected behavior:

```text
BandolierHeld returns true while Left Shift is held.
BandolierScrollY returns scroll wheel movement while gameplay input is enabled.
```

Why this matters:

All weapon UI input should flow through PlayerInputReader now that the project has moved to the New Input System.

---

### Step 3 - Update PlayerWeaponLoadoutController

PlayerWeaponLoadoutController should support:

```text
CycleWeapon(1)
CycleWeapon(-1)
CycleNextWeapon()
CyclePreviousWeapon()
```

Inputs:

```text
Next input cycles forward
Previous input cycles backward
Mouse wheel while bandolier is held cycles forward/backward
```

Design note:

Do not duplicate weapon cycling logic in the HUD if the loadout controller already owns that behavior.

---

### Step 4 - Create WeaponBandolierSlotUI

Path:

```text
Assets/Scripts/UI/HUD/WeaponBandolierSlotUI.cs
```

Purpose:

One slot displays one owned weapon.

Fields:

```text
Image weaponIconImage
TMP_Text weaponNameText
GameObject selectedRoot
CanvasGroup canvasGroup
```

Functions:

```text
Setup(WeaponData weaponData, bool isSelected)
SetSelected(bool selected)
```

Display rules:

```text
If weaponIcon exists, show icon.
If no icon exists, show text fallback.
Selected weapon turns on SelectedHighlight.
Non-selected weapons stay dimmer or plain.
```

---

### Step 5 - Create WeaponBandolierHUD

Path:

```text
Assets/Scripts/UI/HUD/WeaponBandolierHUD.cs
```

Purpose:

The HUD builds slots from owned weapons in WeaponDatabase order.

References:

```text
GameObject bandolierRoot
Transform slotParent
WeaponBandolierSlotUI slotPrefab
PlayerInputReader inputReader
PlayerWeaponController weaponController
```

Responsibilities:

```text
Show bandolier while BandolierHeld is true
Hide bandolier when BandolierHeld is false
Read owned weapons from WeaponDatabase
Spawn one slot per owned weapon
Highlight active weapon
Refresh after weapon changes
Handle mouse scroll cycling
```

---

### Step 6 - Create Slot Prefab

Prefab hierarchy:

```text
WeaponBandolierSlotUI
├── Background
├── SelectedHighlight
├── WeaponIcon
└── WeaponNameText
```

Required setup:

```text
WeaponBandolierSlotUI script assigned
WeaponIcon assigned
WeaponNameText assigned
SelectedHighlight assigned
Canvas Group optional
```

Layout notes:

```text
Slot should have a fixed preferred width and height.
Slot Parent should use Horizontal Layout Group.
Slot Parent can also use Content Size Fitter if needed.
```

---

### Step 7 - Create Canvas_WeaponBandolierHUD

Suggested hierarchy:

```text
Canvas_WeaponBandolierHUD
└── WeaponBandolierRoot
    ├── Background
    └── SlotRow
        └── WeaponBandolierSlotUI(Clone)
```

Suggested placement:

```text
Bottom center of screen
Hidden by default
Visible only while holding Left Shift
```

---

### Step 8 - Hook into Player HUD prefab

HUD parent should include:

```text
Canvas_PlayerCoreHud
Canvas_WeaponAmmoHUD
Canvas_WeaponBandolierHUD
Canvas_TargetRangeHUD
Canvas_SystemMessages
```

Why this matters:

This keeps the UI modular.

Each HUD piece does one job.

---

### Step 9 - Test owned weapon list

Testing flow:

```text
Pick up Pistol
Open bandolier
See Pistol slot

Pick up Shotgun
Open bandolier
See Pistol and Shotgun slots

Press 2
Active weapon changes
Bandolier highlight changes

Hold Left Shift and scroll
Active weapon changes
Bandolier highlight changes
Release Left Shift
Bandolier hides
```

---

## 7. Unity Setup Checklist

Player:

```text
Player
- PlayerInputReader
- PlayerWeaponController
- PlayerWeaponLoadoutController
```

HUD:

```text
Canvas_WeaponBandolierHUD
- WeaponBandolierHUD

WeaponBandolierRoot
- Set inactive by default

SlotRow
- Horizontal Layout Group
- Content Size Fitter optional

WeaponBandolierSlotUI prefab
- WeaponBandolierSlotUI script
- Icon reference
- Name text reference
- Selected root reference
```

WeaponData:

```text
weaponId assigned
displayName assigned
hudDisplayName assigned if desired
weaponIcon assigned where available
```

WeaponDatabase:

```text
All weapon data assets added in desired display order
```

Input:

```text
BandolierHold = Left Shift
BandolierScroll = Mouse Scroll Y
Previous = 1
Next = 2
```

---

## 8. Goal Line / Completion Checklist

1. Player can pick up multiple weapons.
2. Owned weapons are saved.
3. Player can press 1 and 2 to cycle previous/next.
4. Player can hold Left Shift to show bandolier.
5. Bandolier displays one slot per owned weapon.
6. Active weapon slot is highlighted.
7. Mouse wheel scrolls weapons while bandolier is open.
8. Releasing Left Shift hides the bandolier.
9. Weapon/ammo HUD updates when weapon changes.
10. View model updates when weapon changes.
11. Active weapon saves correctly.
12. Reloading scenes keeps active weapon consistent.

---

## 9. Suggested Commit

```bash
git add .
git commit -m "Add weapon bandolier HUD and scroll selection"
```

Optional tag:

```bash
git tag milestone-3f-weapon-bandolier-hud
```

---

## 10. Design Notes

The bandolier is not the inventory.

The bandolier only shows owned weapons in quick-select form.

Later systems can expand this into:

- Weapon wheel
- Inventory weapon tab
- Durability display
- Ammo type selection
- Weapon mods
- Rarity or quality indicators
- Drag/drop equipment UI

For now, the bandolier is a lightweight selection layer that sits above PlayerProgress, WeaponDatabase, and PlayerWeaponController.

---

## 11. Portfolio Value

This checkpoint demonstrates:

- Modular HUD development
- Dynamic UI generation
- ScriptableObject-driven icons and names
- Runtime selection state
- New Input System integration
- Saved active weapon state
- UI feedback tied to gameplay systems
- Expandable equipment UI architecture

This is especially portfolio-friendly because it turns backend weapon ownership into readable player-facing UI.
