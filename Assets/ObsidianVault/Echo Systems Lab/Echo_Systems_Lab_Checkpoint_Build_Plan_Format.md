---
title: Checkpoint Build Plan Format
project: Echo Systems Lab
type: workflow-template
tags:
  - unity
  - portfolio
  - systems-design
  - checkpoint
  - workflow
---

# Checkpoint Build Plan Format

Use this note as the source format for future Echo Systems Lab implementation checkpoints.

When asking for a new build step, use language like:

> Use the Checkpoint Build Plan format.

Or:

> Can you break this down in the same Checkpoint Build Plan format?

---

# Purpose

The Checkpoint Build Plan format is used to keep each development milestone structured, implementation-focused, expandable, and portfolio-ready.

Each checkpoint should answer:

- What are we building?
- Why does it matter for the overall system?
- What files, scripts, prefabs, and scene objects are needed?
- What order should the work happen in?
- What is the goal line?
- What should be committed when it works?
- How does this strengthen the portfolio?

---

# Standard Format

## 1. Milestone Title

Use a clear milestone name.

Example:

```text
Milestone 1B - Target Range Trial MVP
```

---

## 2. Goal Line

Start with the playable result.

Example:

```text
From Hub:
Mission Terminal -> Combat Trial / Target Range -> Load TargetRange scene

In TargetRange:
Interact with pistol pedestal -> Equip pistol -> Shoot primitive targets -> Track shots, hits, score, weapon XP -> Complete trial when all targets are destroyed -> Return to Hub -> Unlock next mission
```

---

## 3. Naming / Scope Notes

Clarify important language and boundaries.

Example:

```text
Use Target, not Enemy.
This trial is about weapon, projectile, scoring, and objective systems.
AI comes later in its own scene.
```

---

## 4. System Pieces

List the major systems being built.

Example:

```text
1. WeaponData ScriptableObject
2. AmmoData / ProjectileData ScriptableObject
3. PlayerWeaponController
4. WeaponPedestal using the existing IInteractable system
5. Runtime Projectile prefab
6. IDamageable / DamageInfo
7. TargetHealth
8. TargetRangeTracker
9. Trial completion flow
```

---

## 5. Folder Setup

Create or update project folders before adding scripts.

Example:

```text
Assets/_EchoSystemsLab/Scripts/Weapons/
Assets/_EchoSystemsLab/Scripts/Targets/
Assets/_EchoSystemsLab/Scripts/Combat/
Assets/_EchoSystemsLab/Scripts/Trials/
Assets/_EchoSystemsLab/ScriptableObjects/Weapons/
Assets/_EchoSystemsLab/ScriptableObjects/Ammo/
Assets/_EchoSystemsLab/Prefabs/Weapons/
Assets/_EchoSystemsLab/Prefabs/Projectiles/
Assets/_EchoSystemsLab/Prefabs/Targets/
Assets/_EchoSystemsLab/Prefabs/Pedestals/
```

---

## 6. Numbered Implementation Steps

Each step should include:

- File path
- Script name
- Code block when needed
- Inspector setup
- Scene setup
- Why the step matters

Example:

```text
Step 1 - Create DamageInfo and IDamageable
Step 2 - Create WeaponData
Step 3 - Create AmmoData / ProjectileData
Step 4 - Create Runtime Projectile
Step 5 - Create PlayerWeaponController
Step 6 - Create WeaponPedestal
Step 7 - Create TargetHealth
Step 8 - Create TargetRangeTracker
```

---

## 7. Unity Setup Checklist

List exact scene object requirements.

Example:

```text
Player
- CharacterController
- SimpleFirstPersonController
- PlayerInteractor
- PlayerWeaponController
- Main Camera
  - WeaponHolder
    - MuzzlePoint

PistolPedestal
- Collider
- WeaponPedestal
- Layer: Interactable
- WeaponData assigned

Target_Bullseye
- Collider
- TargetHealth
```

---

## 8. Goal Line / Completion Checklist

Define the point where the milestone is considered working.

Example:

```text
1. Enter TargetRangeTrial scene from the Hub terminal.
2. Look at pistol pedestal.
3. Prompt says "Press E to equip Pistol."
4. Press E.
5. Pistol equips.
6. Left click fires projectile.
7. Projectile hits bullseye target.
8. Target loses health.
9. Target is destroyed after enough hits.
10. Tracker logs score, hits, shots, and weapon XP.
11. Destroying all targets marks CombatTrial complete.
12. Press R to return to Hub.
13. Hub terminal now shows Combat Trial as completed and AI Trial as unlocked.
```

---

## 9. Suggested Commit

Every milestone should end with a clean Git checkpoint.

Example:

```bash
git add .
git commit -m "Build target range weapon and target objective loop"
```

Optional tag:

```bash
git tag milestone-1b-target-range-mvp
```

---

## 10. Design Notes

Include architecture decisions and future-proofing notes.

Example:

```text
WeaponData represents the weapon.
AmmoData / ProjectileData represents the projectile/ammo behavior.
The runtime Projectile MonoBehaviour is the physical object spawned into the scene.
Targets use IDamageable so the system can later support enemies, props, destructibles, or boss weak points.
```

---

## 11. Portfolio Value

End with what this milestone demonstrates.

Example:

```text
This checkpoint demonstrates:
- Reusable interaction architecture
- ScriptableObject-driven weapons
- ScriptableObject-driven ammo/projectile data
- Runtime equipment flow
- Damage interface design
- Target scoring
- Weapon XP hooks
- Mission completion integration
```

---

# Projectile / Ammo Architecture Rule

Do not make every projectile instance a ScriptableObject.

Use this split:

```text
WeaponData = shared weapon configuration
AmmoData / ProjectileData = shared ammo/projectile configuration
Projectile MonoBehaviour = runtime projectile object spawned into the scene
```

This allows one pistol to support multiple ammo types:

```text
Regular Ammo
Incendiary Ammo
Armor Piercing Ammo
Explosive Ammo
Stun Ammo
```

The weapon decides how and when to fire.

The ammo/projectile data decides what gets spawned and what effect it has.

The runtime projectile handles movement, collision, and delivery of damage/effects.

---

# Future Prompt Shortcut

Use this phrase:

```text
Use the Checkpoint Build Plan format.
```

Or:

```text
Break this into a Checkpoint Build Plan.
```
