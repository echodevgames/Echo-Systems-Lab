# Echo Systems Lab - Milestone 4A - Weapon View Model Recoil Foundation

Tags: #EchoSystemsLab #Unity #Weapons #FPS #Milestone #WeaponHandling

---

## 1. Milestone Title

```text
Milestone 4A - Weapon View Model Recoil Foundation
```

---

## 2. Goal Line

```text
Player equips weapon -> weapon view model appears -> player fires -> projectile stays fair and reticle-based -> view model kicks back and rotates -> weapon smoothly recovers -> optional animator triggers fire/reload/equip feedback.
```

This milestone begins the professional weapon handling pass by separating **gameplay weapon logic** from **weapon presentation logic**.

---

## 3. Naming / Scope Notes

Use this architecture split:

```text
PlayerWeaponController = weapon gameplay brain
PlayerWeaponViewModelController = weapon presentation and body language
WeaponHandlingData = per-weapon feel tuning
WeaponData = weapon identity, ammo, visuals, and handling reference
Projectile = runtime projectile motion and hit delivery
```

This milestone does **not** require first-person arms yet.

The goal is weapon-only presentation first:

```text
View model recoil
Kickback
Recovery
Animator trigger hooks
Per-weapon handling profiles
```

Hands, arms, full reload animation polish, muzzle flash, fire audio, sway, bob, and advanced camera recoil are future passes.

---

## 4. System Pieces

```text
1. WeaponHandlingData ScriptableObject
2. WeaponData handling reference
3. PlayerWeaponViewModelController
4. View model kickback
5. View model rotation recoil
6. Randomized recoil variation
7. Recoil clamp limits
8. Smooth recovery
9. Fire feedback hook
10. Reload feedback hook
11. Equip feedback hook
12. PlayerWeaponController integration
13. Projectile fairness preserved
```

---

## 5. Folder Setup

Use or create:

```text
Assets/Scripts/Weapons/
Assets/Scripts/Weapons/ViewModels/
Assets/ScriptableObjects/Weapons/
Assets/ScriptableObjects/Weapons/Handling/
Assets/Prefabs/Weapons/ViewModels/
```

Suggested assets:

```text
Assets/ScriptableObjects/Weapons/Handling/WeaponHandlingData_Pistol.asset
Assets/ScriptableObjects/Weapons/Handling/WeaponHandlingData_Shotgun.asset
Assets/ScriptableObjects/Weapons/Handling/WeaponHandlingData_Rifle.asset
```

---

## 6. Numbered Implementation Steps

### Step 1 - Create `WeaponHandlingData`

**File Path**

```text
Assets/Scripts/Weapons/WeaponHandlingData.cs
```

**Purpose**

Create a ScriptableObject that controls per-weapon presentation feel:

```text
Fire position kick
Fire rotation kick
Random recoil variation
Max position offset
Max rotation offset
Position recovery
Rotation recovery
Animator trigger names
```

**Why this step matters**

Weapon feel should be data-driven. A pistol, shotgun, rifle, and heavy weapon should not share identical kickback or recovery.

---

### Step 2 - Add Handling Data to `WeaponData`

**File Path**

```text
Assets/Scripts/Weapons/WeaponData.cs
```

**Add**

```csharp
[Header("Handling")]
public WeaponHandlingData handlingData;
```

**Why this step matters**

`WeaponData` already represents the weapon. Adding a handling reference lets each weapon point to its own feel profile without bloating the weapon logic script.

---

### Step 3 - Create `PlayerWeaponViewModelController`

**File Path**

```text
Assets/Scripts/Weapons/PlayerWeaponViewModelController.cs
```

**Responsibilities**

```text
Track the active weapon view model
Store base local position / rotation / scale
Apply procedural kickback
Apply procedural rotation recoil
Recover smoothly toward base transform
Trigger Fire / Reload / Equip animator parameters
Clear itself when no view model exists
```

**Why this step matters**

This keeps presentation out of the main weapon gameplay controller.

---

### Step 4 - Patch `PlayerWeaponController`

**File Path**

```text
Assets/Scripts/Weapons/PlayerWeaponController.cs
```

**Add Reference**

```csharp
[SerializeField] private PlayerWeaponViewModelController viewModelController;
```

**Awake Setup**

```csharp
if (viewModelController == null)
    viewModelController = GetComponent<PlayerWeaponViewModelController>();
```

**Spawn View Model Hook**

When a view model is spawned:

```csharp
if (viewModelController != null)
    viewModelController.SetActiveViewModel(currentViewModel.transform, currentWeapon.handlingData);
```

**Clear Hook**

When no prefab exists or the weapon is unequipped:

```csharp
if (viewModelController != null)
    viewModelController.ClearActiveViewModel();
```

**Fire Hook**

After a successful shot:

```csharp
if (viewModelController != null)
    viewModelController.PlayFireFeedback();
```

**Reload Hook**

When reload begins:

```csharp
if (viewModelController != null)
    viewModelController.PlayReloadFeedback();
```

**Why this step matters**

`PlayerWeaponController` decides what happened. `PlayerWeaponViewModelController` decides how that event looks and feels.

---

### Step 5 - Tune Handling Assets

Create handling data assets for each weapon family.

#### Pistol Starter Values

```text
Fire Position Kick: X 0, Y -0.01, Z -0.06
Fire Rotation Kick: X -3, Y 0, Z 0
Random Rotation Kick: X 0.5, Y 0.4, Z 0.6
Position Return Speed: 14
Rotation Return Speed: 16
Position Snappiness: 28
Rotation Snappiness: 30
```

#### Shotgun Starter Values

```text
Fire Position Kick: X 0, Y -0.025, Z -0.14
Fire Rotation Kick: X -7, Y 0, Z 0
Random Rotation Kick: X 1.2, Y 0.8, Z 1.5
Position Return Speed: 9
Rotation Return Speed: 10
Position Snappiness: 20
Rotation Snappiness: 22
```

#### Rifle Starter Values

```text
Fire Position Kick: X 0, Y -0.008, Z -0.04
Fire Rotation Kick: X -2, Y 0, Z 0
Random Rotation Kick: X 0.35, Y 0.25, Z 0.35
Position Return Speed: 18
Rotation Return Speed: 20
Position Snappiness: 34
Rotation Snappiness: 36
```

---

### Step 6 - Preserve Projectile Fairness

The projectile should still follow the reticle-based aim system.

```text
Weapon model recoil = presentation
Projectile aim = camera / reticle
```

This keeps the target range fair while weapon visuals improve.

---

## 7. Unity Setup Checklist

### Player

```text
Player
- PlayerInputReader
- SimpleFirstPersonController
- PlayerWeaponController
- PlayerWeaponLoadoutController
- PlayerWeaponViewModelController
```

### Weapon View Model Prefab

```text
Weapon_ViewModel_Pistol
- Mesh
- MuzzlePoint
- Optional Animator
```

### WeaponData

```text
WeaponData_Pistol
- View Model Prefab assigned
- View Local Position tuned
- View Local Euler Angles tuned
- View Local Scale tuned
- Handling Data assigned
```

### WeaponHandlingData

```text
WeaponHandlingData_Pistol
- Position kick tuned
- Rotation kick tuned
- Random recoil tuned
- Recovery speed tuned
- Animator trigger names set
```

---

## 8. Goal Line / Completion Checklist

This milestone is complete when:

```text
1. Player can equip a weapon.
2. Weapon view model appears correctly.
3. Shooting still fires accurately at the reticle.
4. View model kicks backward on successful fire.
5. View model rotates on successful fire.
6. View model smoothly recovers.
7. Different WeaponHandlingData assets create different weapon feel.
8. Reload feedback hook exists.
9. Equip feedback hook exists.
10. Weapon can function without an Animator.
11. No weapon view model reference remains stale after unequip.
```

---

## 9. Suggested Commit

```bash
git add .
git commit -m "Add professional weapon view model recoil foundation"
git push
```

Optional tag:

```bash
git tag milestone-4a-weapon-viewmodel-recoil
git push origin milestone-4a-weapon-viewmodel-recoil
```

---

## 10. Design Notes

This milestone starts the professional weapon handling stack.

Current structure:

```text
WeaponData
- identity
- visuals
- ammo
- firing rules
- handling profile

WeaponHandlingData
- recoil
- kickback
- recovery
- animator trigger names

PlayerWeaponController
- gameplay weapon logic

PlayerWeaponViewModelController
- visual weapon feel

Projectile
- runtime projectile motion / hit delivery
```

Important decision:

```text
Do not make PlayerWeaponController responsible for presentation details.
```

This prevents the weapon controller from becoming a tangled octopus of ammo, recoil, UI, animation, sound, projectile, and input logic.

---

## 11. Portfolio Value

This checkpoint demonstrates:

```text
- Component separation
- Data-driven weapon feel
- Professional first-person weapon architecture
- Procedural view model recoil
- Per-weapon tuning
- Animator hook readiness
- Scalable weapon presentation system
```

More importantly, this milestone changes the project mindset:

```text
Weapons are no longer just functional.
Weapons now have feel architecture.
```
