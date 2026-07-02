---
title: Milestone 3G - PickupSpawner Setup Methods
project: Echo Systems Lab
type: systems-reference
tags:
  - unity
  - portfolio
  - systems-design
  - pickups
  - spawner
  - milestone-3g
---

# Echo Systems Lab Milestone 3G - PickupSpawner Setup Methods

## Purpose

This note documents the different ways the `PickupSpawner` can be configured and triggered.

The spawner is meant to be a reusable scene tool for:

- Weapon pickups
- Ammo pickups
- Health pickups
- Mission rewards
- Target range refills
- Debug testing
- Hidden loot
- Boss / objective rewards

---

## Core Idea

`PickupSpawner` has four main spawn modes:

```text
OnStart
OnTriggerEnter
Manual
Timed
```

The same script can be placed in many scenes and configured differently in the Inspector.

The spawner does not need to know much about the pickup itself.

It only needs:

```text
Pickup prefab
Optional WeaponPickupData
Optional spawn points
Spawn rules
Timing rules
Limit rules
```

---

# 1. OnStart Mode

## Use Case

Use `OnStart` when a pickup should appear as soon as the scene loads.

Good for:

- Basic level pickups
- Starter ammo
- Training lane weapon pickups
- Static reward objects
- Debug pickup tests

## Inspector Setup

```text
PickupSpawner
- Spawn Mode: OnStart
- Continuous Spawning: false
- Spawns Per Burst: 1
- Max Total Spawns: 1
- Max Alive Spawns: 1
- Initial Delay: 0
```

## Example

```text
Scene starts
Spawner immediately creates one pistol pickup
Player can interact with it
```

## Notes

If `Initial Delay` is greater than 0, the spawner waits before spawning.

---

# 2. OnStart Continuous Mode

## Use Case

Use `OnStart` with `Continuous Spawning` enabled when a spawner should start running automatically after the scene loads.

Good for:

- Ammo refills
- Survival mode pickups
- Periodic health pickups
- Long training trials

## Inspector Setup

```text
PickupSpawner
- Spawn Mode: OnStart
- Continuous Spawning: true
- Initial Delay: 1
- Spawn Interval: 5
- Max Total Spawns: 0
- Max Alive Spawns: 3
```

## Example

```text
Scene starts
Spawner waits 1 second
Spawner creates one ammo pickup
Every 5 seconds, it tries to create another
It never allows more than 3 alive at once
```

## Notes

`Max Total Spawns` set to `0` or less means unlimited.

`Max Alive Spawns` prevents the floor from becoming a pickup swamp.

---

# 3. OnTriggerEnter Mode

## Use Case

Use `OnTriggerEnter` when entering an area should spawn a pickup.

Good for:

- Tutorial booths
- Reward zones
- Target range lanes
- Secret rooms
- Checkpoint rewards
- “Walk here and a pickup appears” moments

## Required Scene Setup

```text
PickupSpawner_TriggerReward
- Box Collider
  - Is Trigger: true
- PickupSpawner
```

## Inspector Setup

```text
PickupSpawner
- Spawn Mode: OnTriggerEnter
- Required Tag: Player
- Spawn Only Once: true
- Continuous Spawning: false
- Spawns Per Burst: 1
- Max Total Spawns: 1
- Max Alive Spawns: 1
```

## Example

```text
Player enters trigger
Spawner creates one shotgun pickup
Spawner will not fire again
```

## Notes

The entering player object must have the matching tag, usually:

```text
Player
```

---

# 4. OnTriggerEnter Continuous Mode

## Use Case

Use trigger mode with continuous spawning when entering an area should start a timed pickup loop.

Good for:

- Combat arenas
- Target range active lanes
- Survival rooms
- Ammo refill zones
- Training challenge areas

## Inspector Setup

```text
PickupSpawner
- Spawn Mode: OnTriggerEnter
- Required Tag: Player
- Spawn Only Once: true
- Continuous Spawning: true
- Initial Delay: 1
- Spawn Interval: 5
- Max Total Spawns: 0
- Max Alive Spawns: 3
```

## Example

```text
Player enters target lane
Spawner starts spawning ammo every 5 seconds
Spawner stops only if another script calls StopSpawning()
```

## Notes

With `Spawn Only Once` enabled, entering the trigger starts the loop one time.

Without a stop condition, the spawner continues until it reaches `Max Total Spawns`.

---

# 5. Timed Mode

## Use Case

Use `Timed` when the spawner should automatically spawn over time without needing a trigger.

Good for:

- Survival mode pickups
- Ammo fountains
- Repeating resource drops
- Arena refill systems

## Inspector Setup

```text
PickupSpawner
- Spawn Mode: Timed
- Initial Delay: 1
- Spawn Interval: 5
- Spawns Per Burst: 1
- Max Total Spawns: 0
- Max Alive Spawns: 3
```

## Example

```text
Scene starts
Spawner waits 1 second
Spawner spawns one pickup
Every 5 seconds, it tries again
```

## Notes

Timed mode automatically calls `StartSpawning()` from `Start()`.

---

# 6. Manual Mode

## What Manual Mode Means

Manual mode means the spawner does **nothing by itself**.

It will not spawn on scene start.

It will not spawn from trigger enter.

It will not automatically start a timed loop.

Another script, UnityEvent, terminal, button, target, mission controller, or debug tool must call one of its public methods.

## Public Methods

```csharp
public GameObject SpawnOne()
public void SpawnBurst()
public void StartSpawning()
public void StopSpawning()
public void ResetSpawner()
```

## Use Case

Manual mode is best when another system decides when the reward appears.

Good for:

- Mission completion rewards
- Target destroyed rewards
- Boss defeated rewards
- Terminal rewards
- Debug cheats
- Button-activated tests
- Trial completion rewards
- Objective-based loot

## Inspector Setup

```text
PickupSpawner
- Spawn Mode: Manual
- Spawns Per Burst: 1
- Max Total Spawns: 1
- Max Alive Spawns: 1
```

## Example Flow

```text
Player destroys all targets
TargetRangeTracker marks trial complete
Reward controller calls PickupSpawner.SpawnOne()
Weapon pickup appears
```

## Important Note

Manual mode is not broken if nothing appears.

It is waiting for another script to call it.

Tiny item gargoyle. Silent until summoned.

---

# 7. Manual Mode From Another Script

## Example Script

```csharp
using UnityEngine;

public class PickupSpawnerManualTrigger : MonoBehaviour
{
    [SerializeField] private PickupSpawner pickupSpawner;

    public void SpawnReward()
    {
        if (pickupSpawner == null)
            return;

        pickupSpawner.SpawnOne();
    }

    public void SpawnRewardBurst()
    {
        if (pickupSpawner == null)
            return;

        pickupSpawner.SpawnBurst();
    }

    public void StartRewardLoop()
    {
        if (pickupSpawner == null)
            return;

        pickupSpawner.StartSpawning();
    }

    public void StopRewardLoop()
    {
        if (pickupSpawner == null)
            return;

        pickupSpawner.StopSpawning();
    }
}
```

## Use With

```text
Mission controller
Target range tracker
Boss death script
Terminal interactable
Debug key script
Unity UI Button OnClick
```

---

# 8. Manual Mode From a Unity UI Button

## Scene Setup

```text
Canvas
- Button_SpawnPickup
  - OnClick()
    - PickupSpawnerManualTrigger.SpawnReward()
```

## Use Case

Good for debug testing in the editor.

## Example

```text
Click button
Spawner creates one weapon pickup
```

---

# 9. Manual Mode From an Interactable Terminal

## Use Case

Use this when the player presses Interact on a terminal and receives a pickup reward.

Example:

```text
Mission terminal
Player presses E
Terminal calls spawner.SpawnOne()
Reward pickup appears beside terminal
```

## Possible Script Pattern

```csharp
using UnityEngine;

public class PickupRewardTerminal : MonoBehaviour, IInteractable
{
    [SerializeField] private PickupSpawner rewardSpawner;
    [SerializeField] private string promptText = "Press E to claim reward";

    private bool rewardClaimed;

    public string GetPromptText()
    {
        return rewardClaimed ? "Reward already claimed" : promptText;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (rewardClaimed)
            return;

        if (rewardSpawner != null)
            rewardSpawner.SpawnOne();

        rewardClaimed = true;
    }
}
```

---

# 10. Manual Mode From TargetRangeTrial

## Use Case

Spawn a reward after a trial condition is met.

Example:

```text
All targets destroyed
Trial complete
Reward spawner creates a weapon pickup
```

## Possible Script Pattern

```csharp
using UnityEngine;

public class TargetRangeRewardSpawner : MonoBehaviour
{
    [SerializeField] private PickupSpawner rewardSpawner;

    private TargetRangeTracker tracker;

    private void Start()
    {
        tracker = TargetRangeTracker.Instance;

        if (tracker != null)
            tracker.OnTrialCompleted += SpawnReward;
    }

    private void OnDestroy()
    {
        if (tracker != null)
            tracker.OnTrialCompleted -= SpawnReward;
    }

    private void SpawnReward()
    {
        if (rewardSpawner != null)
            rewardSpawner.SpawnOne();
    }
}
```

---

# 11. Manual Mode From Boss Death

## Use Case

Spawn a pickup after a boss dies.

Example:

```text
Boss dies
Boss death controller calls rewardSpawner.SpawnBurst()
Several pickups appear
```

## Possible Flow

```text
BossHealth.OnDeath
-> BossRewardController.SpawnRewards()
-> PickupSpawner.SpawnBurst()
```

---

# 12. Manual Mode From Debug Key

## Use Case

Good while building and tuning pickup behavior.

## Example Script

```csharp
using UnityEngine;

public class PickupSpawnerDebugInput : MonoBehaviour
{
    [SerializeField] private PickupSpawner pickupSpawner;
    [SerializeField] private KeyCode spawnKey = KeyCode.P;

    private void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            if (pickupSpawner != null)
                pickupSpawner.SpawnOne();
        }
    }
}
```

## Notes

This uses old `Input.GetKeyDown`, so it is best as a temporary debug helper.

For production input, prefer `PlayerInputReader`.

---

# 13. Random Prefab Setup

## Use Case

Use random prefabs when the spawner can create different pickup types.

Example:

```text
Ammo pickup
Health pickup
Weapon pickup
```

## Inspector Setup

```text
Pickup Prefabs
- Pickup_Ammo_Generic
- Pickup_Health_Generic
- Pickup_Weapon_Generic

Randomize Prefab: true
```

## Notes

If the random prefab is `Pickup_Weapon_Generic`, the spawner can also apply random `WeaponPickupData`.

---

# 14. Random Weapon Pickup Data Setup

## Use Case

Use one generic weapon pickup prefab, but let it become different weapons.

## Inspector Setup

```text
Pickup Prefabs
- Pickup_Weapon_Generic

Weapon Pickup Data Options
- WeaponPickupData_Pistol
- WeaponPickupData_Revolver
- WeaponPickupData_Shotgun

Randomize Weapon Pickup Data: true
```

## Example

```text
Spawner creates Pickup_Weapon_Generic
Spawner chooses WeaponPickupData_Shotgun
Pickup initializes as Shotgun
```

---

# 15. Spawn Point Setup

## Use Case

Use spawn points when pickups should appear at specific locations.

## Hierarchy Example

```text
PickupSpawner_Ammo
- SpawnPoint_01
- SpawnPoint_02
- SpawnPoint_03
```

## Inspector Setup

```text
Spawn Points
- SpawnPoint_01
- SpawnPoint_02
- SpawnPoint_03

Randomize Spawn Point: true
```

## Notes

If no spawn points are assigned, the spawner uses its own transform position.

---

# 16. Spawn Burst Setup

## Use Case

Use burst spawning when several pickups should appear at once.

Good for:

- Boss reward explosions
- Ammo crate breaks
- Mission completion loot
- Secret room rewards

## Inspector Setup

```text
Spawns Per Burst: 5
Apply Spawn Impulse: true
Spawn Up Impulse: 2
Random Side Impulse: 1
```

## Example

```text
Boss dies
Spawner creates 5 pickups
Each pickup gets a small toss impulse
```

---

# 17. Limit Rules

## Max Total Spawns

Controls how many pickups this spawner can create over its lifetime.

```text
Max Total Spawns: 1
```

Means:

```text
Spawner can only create 1 pickup total
```

```text
Max Total Spawns: 0
```

Means:

```text
Unlimited total spawns
```

## Max Alive Spawns

Controls how many spawned pickups can exist at the same time.

```text
Max Alive Spawns: 3
```

Means:

```text
Spawner will not spawn more while 3 of its spawned pickups are still alive
```

When the player picks one up and it is destroyed, the spawner can spawn again.

---

# 18. Recommended Presets

## One-Time Weapon Reward

```text
Spawn Mode: Manual
Pickup Prefabs: Pickup_Weapon_Generic
Weapon Pickup Data Options: WeaponPickupData_Shotgun
Spawns Per Burst: 1
Max Total Spawns: 1
Max Alive Spawns: 1
```

## Ammo Refill Loop

```text
Spawn Mode: Timed
Pickup Prefabs: Pickup_Ammo_Generic
Initial Delay: 1
Spawn Interval: 5
Max Total Spawns: 0
Max Alive Spawns: 3
```

## Triggered Secret Pickup

```text
Spawn Mode: OnTriggerEnter
Required Tag: Player
Spawn Only Once: true
Pickup Prefabs: Pickup_Weapon_Generic
Weapon Pickup Data Options: WeaponPickupData_Crossbow
Max Total Spawns: 1
Max Alive Spawns: 1
```

## Boss Loot Burst

```text
Spawn Mode: Manual
Pickup Prefabs: Pickup_Ammo_Generic, Pickup_Weapon_Generic
Weapon Pickup Data Options: WeaponPickupData_Revolver
Spawns Per Burst: 5
Max Total Spawns: 5
Max Alive Spawns: 5
Apply Spawn Impulse: true
Spawn Up Impulse: 2
Random Side Impulse: 1.5
```

## Training Lane Starter

```text
Spawn Mode: OnStart
Pickup Prefabs: Pickup_Weapon_Generic
Weapon Pickup Data Options: WeaponPickupData_Pistol
Spawns Per Burst: 1
Max Total Spawns: 1
Max Alive Spawns: 1
```

---

# 19. Testing Checklist

1. Add PickupSpawner to an empty GameObject.
2. Assign at least one pickup prefab.
3. Assign at least one spawn point or use the spawner transform.
4. Choose a spawn mode.
5. Set max total spawns.
6. Set max alive spawns.
7. Press Play.
8. Confirm pickup appears under expected condition.
9. Pick up or destroy pickup.
10. Confirm alive limit updates correctly.
11. Test Manual mode through another script or UnityEvent.
12. Test Trigger mode with player tag.
13. Test Timed mode with max alive limit.
14. Test weapon pickup data options with generic weapon pickup prefab.

---

# 20. Design Notes

Manual mode is the most flexible mode because it lets other systems control the timing.

Use Manual mode when the pickup is a reward.

Use Timed mode when the pickup is a resource refill.

Use Trigger mode when pickup spawning is tied to entering a space.

Use OnStart mode when the pickup should simply exist at scene load.

The ideal long-term flow:

```text
Mission / Trial / Boss / Terminal
-> Calls PickupSpawner
-> PickupSpawner creates generic pickup prefab
-> Pickup receives data
-> Player interacts with pickup
-> PlayerProgress and HUD update
```

This keeps reward logic, spawning logic, pickup logic, and player equipment logic separated.

Clean systems. Fewer cursed wires.
