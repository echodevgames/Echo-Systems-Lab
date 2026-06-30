# Milestone 3G - Weapon Drop, Pickup, and Flexible Pickup Spawner Foundation

## 1. Milestone Title

Milestone 3G - Weapon Drop, Pickup, and Flexible Pickup Spawner Foundation

---

## 2. Goal Line

Player can equip weapons from pedestals or pickups.

Player can drop the active weapon.

Dropped weapon spawns as a physical pickup in front of the player.

Dropped weapon lands on the ground using physics.

Player can look at the dropped weapon and press Interact to pick it back up.

Weapon ownership, active weapon, weapon HUD, bandolier HUD, and view model all update correctly.

A reusable PickupSpawner can spawn weapon pickups or ammo pickups using flexible spawn rules.

---

## 3. Naming / Scope Notes

Use Pickup as the generic world-object term.

Use:

```text
WeaponPickup = world object that grants a WeaponData
AmmoPickup = world object that grants AmmoData / reserve ammo
PickupSpawner = reusable scene spawner for pickup prefabs
```

Keep WeaponPedestal separate for now.

Pedestals are curated display/equip stations.

Pickups are loose world items.

---

## 4. System Pieces

1. WeaponPickupData ScriptableObject
2. WeaponPickup MonoBehaviour
3. PlayerWeaponDropController
4. PlayerProgress remove/clear weapon ownership helpers
5. PlayerWeaponController unequip / active weapon cleanup
6. PlayerWeaponLoadoutController safety when current weapon is dropped
7. PlayerInputReader drop input
8. PickupSpawner
9. Pickup spawn timing rules
10. Pickup spawn trigger support
11. TargetRangeTrial test setup

---

## 5. Folder Setup

```text
Assets/Scripts/Pickups/
Assets/Scripts/Spawning/
Assets/ScriptableObjects/Pickups/
Assets/Prefabs/Pickups/
Assets/Prefabs/Spawners/
```

Suggested assets:

```text
WeaponPickupData_Pistol
WeaponPickupData_Revolver
WeaponPickupData_Shotgun
WeaponPickupData_AssaultRifle
WeaponPickupData_SniperRifle
WeaponPickupData_Crossbow

Pickup_Weapon_Generic
Pickup_Ammo_Generic
PickupSpawner
```

---

## 6. Numbered Implementation Steps

### Step 1 - Add Drop input

Add a new Player action:

```text
Player / Drop
```

Suggested binding:

```text
Keyboard = G
```

Add to PlayerInputReader:

```csharp
public bool DropPressed =>
    gameplayInputEnabled &&
    inputActions.Player.Drop.WasPressedThisFrame();
```

Why this matters:

Dropping should use the same New Input System pipeline as fire, reload, interact, pause, and weapon cycling.

---

### Step 2 - Create WeaponPickupData

Path:

```text
Assets/Scripts/Pickups/WeaponPickupData.cs
```

Purpose:

WeaponPickupData defines how a weapon appears as a pickup in the world.

WeaponData remains the actual weapon definition.

WeaponPickupData controls pickup prompt, visual prefab, and pickup behavior.

Suggested fields:

```text
WeaponData weaponData
string displayName
GameObject worldVisualPrefab
float pickupImpulse
bool destroyOnPickup
bool saveOnPickup
```

---

### Step 3 - Create WeaponPickup

Path:

```text
Assets/Scripts/Pickups/WeaponPickup.cs
```

Purpose:

WeaponPickup is a loose world item that uses IInteractable.

When interacted with, it gives the weapon to the player and equips it.

Flow:

```text
Look at dropped gun
Prompt says "Press E to pick up Shotgun"
Press E
PlayerProgress.SetActiveWeapon(weaponId)
PlayerWeaponController.EquipWeapon(weaponData)
Weapon pickup disappears
HUD and bandolier update
```

Important:

This should use the same interaction system as pedestals, ammo pickups, and mission terminals.

---

### Step 4 - Add weapon removal helpers to PlayerProgress

Add helper methods:

```csharp
public static void RemoveOwnedWeapon(string weaponId)
{
    if (string.IsNullOrWhiteSpace(weaponId))
        return;

    ownedWeaponIds.Remove(weaponId);

    if (activeWeaponId == weaponId)
        activeWeaponId = null;
}

public static void ClearActiveWeapon()
{
    activeWeaponId = null;
}
```

Why this matters:

Dropping a weapon should remove it from the player's currently owned weapon list for now.

Later, inventory can replace this with item instance logic.

---

### Step 5 - Add unequip support to PlayerWeaponController

Add a method:

```text
UnequipCurrentWeapon()
```

It should:

```text
Clear currentWeapon
Destroy currentViewModel
Clear currentMuzzlePoint
Clear active weapon if needed
Tell TargetRangeTracker weapon is now none, if needed
Notify HUD/bandolier refresh flow
```

Why this matters:

When the player drops the active weapon, the weapon should disappear from the camera view immediately.

No phantom pistol. No spectral shotgun. Clean hands, clean state.

---

### Step 6 - Create PlayerWeaponDropController

Path:

```text
Assets/Scripts/Weapons/PlayerWeaponDropController.cs
```

Responsibilities:

```text
Read DropPressed from PlayerInputReader
Check current equipped weapon
Spawn a WeaponPickup prefab in front of player/camera
Add small forward/upward force
Remove weapon from PlayerProgress owned weapons
Unequip player
Save game
Refresh weapon HUD and bandolier HUD
```

Suggested serialized fields:

```text
PlayerInputReader inputReader
PlayerWeaponController weaponController
Camera playerCamera
Transform dropPoint
GameObject weaponPickupPrefab
float dropForwardOffset
float dropUpOffset
float dropForce
```

Design note:

The drop prefab should be generic.

The spawned WeaponPickup receives WeaponData at runtime.

---

### Step 7 - Make WeaponPickup support runtime setup

WeaponPickup should work in two ways:

```text
1. Placed in scene with WeaponData assigned in Inspector.
2. Spawned at runtime and initialized with WeaponData.
```

Add method:

```csharp
public void Initialize(WeaponData newWeaponData)
```

Why this matters:

The same pickup prefab can be used for:

- Dropped weapons
- Placed pickups
- Loot rewards
- Spawner output
- Mission rewards
- Debug testing

---

### Step 8 - Update Weapon Bandolier HUD refresh points

The bandolier should refresh when:

```text
Weapon is picked up
Weapon is dropped
Weapon is cycled
Active weapon changes
```

For this milestone, acceptable approaches:

```text
Simple route:
Bandolier rebuilds while open.

Better route:
PlayerProgress exposes OnWeaponsChanged and OnActiveWeaponChanged events.
```

Recommended future direction:

```text
PlayerProgress.OnWeaponsChanged
PlayerProgress.OnActiveWeaponChanged
```

For now, use whichever path is fastest and safest.

---

### Step 9 - Create PickupSpawner

Path:

```text
Assets/Scripts/Spawning/PickupSpawner.cs
```

Core features:

```text
Spawn on Start
Spawn on Trigger Enter
Spawn manually from another script
Spawn over time
Spawn repeatedly
Max active spawned pickups
Max total spawns
Spawn delay
Spawn interval
Random spawn point support
Random prefab support
```

Suggested enum:

```csharp
public enum PickupSpawnMode
{
    OnStart,
    OnTriggerEnter,
    Manual,
    Timed
}
```

Suggested fields:

```text
GameObject[] pickupPrefabs
Transform[] spawnPoints
PickupSpawnMode spawnMode
bool spawnOnlyOnce
bool continuousSpawning
int maxTotalSpawns
int maxAliveSpawns
float initialDelay
float spawnInterval
bool randomizePrefab
bool randomizeSpawnPoint
bool parentSpawnedObjects
```

Why this matters:

PickupSpawner becomes useful for:

- Ammo crates
- Weapon pickups
- Health pickups
- Target range refills
- Hidden pickups
- Mission rewards
- Loot systems
- Debug test drops

---

### Step 10 - Add trigger spawning

Spawner setup:

```text
PickupSpawner
- Box Collider
- Is Trigger enabled
- PickupSpawner script
```

When player enters:

```text
Spawner fires once or starts timed spawning depending on settings.
```

Useful examples:

```text
Walk into target range booth -> ammo box appears
Enter training lane -> weapon pickup spawns
Complete objective -> reward pickup spawns
```

---

### Step 11 - Add manual spawn methods

Expose:

```csharp
public void SpawnOne()
public void StartSpawning()
public void StopSpawning()
```

Why this matters:

Other systems can later trigger spawns:

```text
Mission completion
Target destroyed
Boss defeated
Terminal reward
Debug command
```

---

### Step 12 - TargetRangeTrial setup

Add test objects:

```text
WeaponDropTest
- Player has PlayerWeaponDropController
- Drop key is G

PickupSpawner_Test
- Spawns ammo box every 5 seconds
- Max alive: 3

PickupSpawner_WeaponReward
- Manual mode
- Prefab: WeaponPickup_Generic
```

Test scene flow:

```text
Pick up pistol from pedestal.
Press G.
Pistol drops onto ground.
Weapon disappears from player view.
Bandolier removes pistol or updates active selection.
Look at dropped pistol.
Prompt appears.
Press E.
Pistol equips again.
HUD updates.
Save/load still remembers correct current weapon state.
```

---

## 7. Unity Setup Checklist

### Player

```text
Player
- PlayerInputReader
- SimpleFirstPersonController
- PlayerInteractor
- PlayerWeaponController
- PlayerWeaponLoadoutController
- PlayerWeaponDropController
- PlayerAmmoInventory
```

### Input Actions

```text
Player/Drop
- Keyboard: G
```

### Generic Weapon Pickup Prefab

```text
Pickup_Weapon_Generic
- Rigidbody
- Collider
- WeaponPickup
- Layer: Interactable
- Visual root child
```

Rigidbody setup:

```text
Use Gravity: true
Is Kinematic: false
Collision Detection: Continuous Dynamic
```

Collider setup:

```text
Not trigger for physics body
Optional child trigger later for pickup radius
```

### Pickup Spawner Prefab

```text
PickupSpawner
- PickupSpawner script
- Optional Box Collider if using trigger mode
```

---

## 8. Goal Line / Completion Checklist

1. Player equips pistol.
2. Player presses G.
3. Pistol drops in front of player.
4. Dropped pistol lands on the ground with physics.
5. Player view model disappears.
6. Weapon HUD shows no weapon or updates safely.
7. Bandolier removes or updates dropped weapon.
8. Player looks at dropped pistol.
9. Prompt says "Press E to pick up Pistol."
10. Player presses E.
11. Pistol is added back to owned weapons.
12. Pistol equips.
13. Weapon HUD updates.
14. Bandolier updates.
15. Save/load does not break current weapon state.
16. PickupSpawner can spawn at least one ammo pickup or weapon pickup.
17. PickupSpawner supports OnStart and Trigger Enter modes.

---

## 9. Suggested Commit

```bash
git add .
git commit -m "Add weapon drop pickup and flexible pickup spawner foundation"
```

Optional tag:

```bash
git tag milestone-3g-weapon-drop-pickup-spawner
```

---

## 10. Design Notes

WeaponData remains the weapon definition.

WeaponPickupData describes how a weapon appears as a world pickup.

WeaponPickup is the interactable runtime object.

PlayerWeaponDropController converts equipped weapons back into world pickups.

PickupSpawner is generic and should not know too much about weapons specifically.

Important future note:

Right now, dropping can remove a weapon from owned weapons.

Later, when inventory exists, dropping should remove from inventory instead.

Owned weapons may become discovered/unlocked weapons, while inventory becomes what the player currently carries.

Future distinction:

```text
Discovered Weapons = player has unlocked/seen this weapon
Inventory Weapons = player currently carries this exact item
Dropped Weapon = world instance with possible durability/ammo state
```

Future durability data may require item instances:

```text
weaponId
currentDurability
currentClipAmmo
loadedAmmoType
modifiers
```

For now, keep dropped weapons simple:

```text
Dropped weapon only stores WeaponData.
```

---

## 11. Portfolio Value

This checkpoint demonstrates:

- World-space item interaction
- Runtime conversion between equipped state and physical pickup state
- Reusable pickup architecture
- ScriptableObject-driven item definitions
- Input-driven equipment control
- Save-aware weapon ownership
- Flexible reusable spawning system
- Expandable loot/reward framework

This milestone connects interaction, inventory direction, runtime spawning, ScriptableObject architecture, UI refresh hooks, and player equipment flow into one tidy systems knot.
