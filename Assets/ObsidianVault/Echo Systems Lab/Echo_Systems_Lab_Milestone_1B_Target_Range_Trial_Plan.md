---
title: Echo Systems Lab - Milestone 1B Target Range Trial Plan
project: Echo Systems Lab
type: milestone-plan
status: planned
created: 2026-06-25
tags:
  - echo-systems-lab
  - unity
  - gameplay-systems
  - portfolio
  - target-range
  - weapons
  - scriptable-objects
---

# Echo Systems Lab - Milestone 1B Target Range Trial Plan

## Purpose

Milestone 1B expands the working hub into the first playable system trial: a first-person target range.

This scene replaces the earlier "Combat Trial" framing with a cleaner **Target Range Trial** concept. The goal is not to build AI yet. The goal is to demonstrate a reusable ranged weapon system, interactable weapon pedestals, target hit detection, objective completion, and mission progression.

This keeps the project focused on scalable gameplay systems and reusable templates for future games.

## Current Checkpoint

Milestone 1A, the Hub Mission Terminal MVP, is working.

Current working systems:

- [x] First-person player movement
- [x] First-person camera look
- [x] Raycast interaction
- [x] Interaction prompt
- [x] Mission terminal open/close
- [x] Mission list generated from `MissionData` assets
- [x] Locked / available mission states
- [x] Separate prompt and terminal canvases
- [x] Terminal background image
- [x] Player input/cursor control during UI

Recommended checkpoint name:

```text
Milestone 1A - Hub Mission Terminal MVP
```

Recommended commit:

```bash
git add .
git commit -m "Complete hub mission terminal MVP"
git tag milestone-1a-hub-terminal
```

## New Milestone Name

```text
Milestone 1B - Target Range Trial MVP
```

## Core Loop

```text
Click Combat Trial / Target Range Trial in the terminal
→ Load TargetRange scene
→ Interact with a weapon pedestal
→ Equip a ranged weapon
→ Shoot primitive bullseye targets
→ Complete the objective when all targets are destroyed
→ Return to hub
→ Unlock the next mission
```

## Language Change

For this scene, do not use enemy terminology.

Use:

- Target
- Bullseye
- Target Health
- Target Range
- Target Objective
- Target Group
- Target Hit Receiver

Avoid:

- Enemy
- AI
- Combatant
- Enemy Controller

Reason:

This scene is not demonstrating AI. It is demonstrating weapon data, shooting, hit detection, and objective completion.

## Scene Concept

The Target Range scene is a simple first-person shooting range.

The player enters a testing room with several weapon pedestals. Each pedestal holds a weapon. Interacting with a pedestal equips that weapon.

For the first pass, weapons are ranged only. Each weapon is defined by a ScriptableObject so new weapons can be created without rewriting core weapon logic.

The targets are primitive bullseyes built from Unity primitives. They should be simple, readable, and disposable.

## First Version Scope

Build only the smallest usable version first.

- [x] Create `TargetRange` scene
- [x] Add scene to Build Settings
- [x] Update the first mission `sceneName` from `CombatTrial` to `TargetRange`
- [x] Create a simple shooting range room
- [ ] Create a primitive bullseye target prefab
- [ ] Create a weapon pedestal prefab
- [ ] Create ranged weapon ScriptableObject data
- [ ] Allow player to interact with pedestal and equip weapon
- [ ] Allow player to shoot
- [ ] Allow projectile or raycast to hit target
- [ ] Destroy or deactivate targets when health reaches zero
- [ ] Track remaining targets
- [ ] Complete mission when all targets are destroyed
- [ ] Return to hub
- [ ] Unlock the next mission

## Systems Demonstrated

This milestone should demonstrate:

- Interaction reuse
- ScriptableObject-driven weapon data
- Ranged weapon equip flow
- Shooting input
- Projectile or hitscan firing
- Target health
- Target hit detection
- Objective tracking
- Scene loading
- Mission completion
- Mission unlock flow

## Asset Pack Import Plan

Before building the Target Range scene, import placeholder asset packs into a quarantined folder.

Planned asset packs:

- Pandazole - Lowpoly Asset Bundle
- Ultimate Interior Furniture Pack (Low Poly) - Household & Kitchen Props
- RPG Poly Pack - Lite
- Skybox Series Free
- Yughues Free Ground Materials

## Third-Party Asset Quarantine Structure

Use this folder structure:

```text
Assets/
 ├── _EchoSystemsLab/
 │   ├── Art/
 │   ├── Materials/
 │   ├── Prefabs/
 │   ├── Scenes/
 │   ├── ScriptableObjects/
 │   └── Scripts/
 │
 ├── _ThirdParty_Quarantine/
 │   ├── Pandazole_LowpolyAssetBundle/
 │   ├── UltimateInteriorFurniturePack/
 │   ├── RPGPolyPackLite/
 │   ├── SkyboxSeriesFree/
 │   └── YughuesFreeGroundMaterials/
 │
 └── _Documentation/
     └── ThirdPartyCredits.md
```

## Quarantine Rules

- [ ] Third-party assets stay inside `_ThirdParty_Quarantine`
- [ ] Do not directly build systems inside third-party folders
- [ ] Use prefab variants, wrappers, or duplicated materials inside `_EchoSystemsLab`
- [ ] Move imported folders inside Unity, not Windows Explorer
- [ ] Check the Console after each import
- [ ] Import one pack at a time
- [ ] Commit after each clean import

## Import Workflow

Before importing any asset packs:

```bash
git add .
git commit -m "Complete hub mission terminal MVP"
git tag milestone-1a-hub-terminal
```

For each asset pack:

1. Import one pack.
2. Move it into `_ThirdParty_Quarantine/PackName/` inside Unity.
3. Check Console for red errors.
4. Confirm no project settings were unexpectedly changed.
5. Commit.

Example commit:

```bash
git add .
git commit -m "Import quarantined placeholder asset pack: PackName"
```

## Third-Party Credits Page

Create:

```text
Assets/_Documentation/ThirdPartyCredits.md
```

Suggested content:

```markdown
# Third-Party Asset Credits

This project uses third-party assets for placeholder/environment art during development. Gameplay systems, architecture, scripting, UI behavior, and implementation are original project work unless otherwise noted.

## Asset Packs

### Pandazole - Lowpoly Asset Bundle
- Source: Unity Asset Store
- Purpose: Placeholder low-poly environment/object art
- Notes: Used for prototyping and portfolio scene dressing.

### Ultimate Interior Furniture Pack (Low Poly) - Household & Kitchen Props
- Source: Unity Asset Store
- Purpose: Placeholder interior props
- Notes: Used for hub and test scene dressing.

### RPG Poly Pack - Lite
- Source: Unity Asset Store
- Purpose: Placeholder fantasy/low-poly props
- Notes: Used for prototyping.

### Skybox Series Free
- Source: Unity Asset Store
- Purpose: Placeholder skyboxes
- Notes: Used for environment lighting/backgrounds.

### Yughues Free Ground Materials
- Source: Unity Asset Store
- Purpose: Placeholder ground/environment materials
- Notes: Used for prototyping.

## Project Authorship Note

All gameplay systems, mission flow, interaction logic, weapon data architecture, target systems, UI logic, and scene progression systems are authored as part of Echo Systems Lab.
```

## Target Range Folder Structure

```text
Assets/_EchoSystemsLab/
 ├── Scripts/
 │   ├── Weapons/
 │   ├── Targets/
 │   └── Objectives/
 │
 ├── ScriptableObjects/
 │   └── Weapons/
 │
 ├── Prefabs/
 │   ├── Weapons/
 │   ├── Targets/
 │   └── Pedestals/
 │
 └── Scenes/
     └── TargetRange.unity
```

## Planned Weapon System Architecture

First pass systems:

```text
WeaponData
RangedWeaponData
WeaponPedestal
WeaponController
Projectile or HitscanShot
TargetHealth
TargetHitReceiver
TargetRangeManager
TargetObjectiveTracker
```

## WeaponData Goals

Weapon data should be ScriptableObject-driven.

A first-pass ranged weapon may include:

- Display name
- Description
- Damage
- Fire rate
- Range
- Projectile prefab or hitscan flag
- Projectile speed
- Ammo behavior, later
- Reticle behavior, later
- Weapon model, later
- Sound effects, later
- Visual effects, later

## Weapon Pedestal Goals

Each pedestal should:

- Implement `IInteractable`
- Hold a reference to a `WeaponData` asset
- Show an interaction prompt like `Press E to equip Pistol`
- Tell the player weapon controller to equip that weapon
- Optionally display a floating label or model preview

## Target Goals

Primitive bullseye targets should:

- Have health
- Receive damage
- Visually react when hit
- Destroy, disable, or change state when defeated
- Notify the target range manager when cleared

First version can be simple:

```text
Projectile hits target
→ TargetHealth.TakeDamage(damage)
→ health <= 0
→ target deactivates
→ TargetRangeManager checks remaining targets
```

## Objective Goals

The first objective is:

```text
Destroy all targets
```

The system should eventually support:

- Destroy all targets
- Hit targets in order
- Timed challenge
- Accuracy challenge
- Weapon-specific challenge
- Moving targets
- Multi-stage target waves

But first version only needs all-targets-destroyed.

## Mission Progression Goal

When all targets are destroyed:

```text
MissionProgress.MarkCompleted("CombatTrial")
```

or, if the mission is renamed:

```text
MissionProgress.MarkCompleted("TargetRangeTrial")
```

Then return to hub and verify the next mission unlocks.

Important: keep the `missionId` and unlock requirement strings consistent.

## Recommended Next Order

1. [ ] Commit current hub checkpoint
2. [ ] Create or update `ThirdPartyCredits.md`
3. [ ] Import/quarantine asset packs one at a time
4. [ ] Create `TargetRange` scene
5. [ ] Add `TargetRange` to Build Settings
6. [ ] Update first mission asset scene name to `TargetRange`
7. [ ] Create primitive shooting range room
8. [ ] Create primitive bullseye target prefab
9. [ ] Create `WeaponData` ScriptableObject
10. [ ] Create weapon pedestal interaction
11. [ ] Create basic ranged weapon controller
12. [ ] Create projectile or hitscan firing
13. [ ] Create target health and hit receiver
14. [ ] Create target range objective tracker
15. [ ] Complete mission and return to hub
16. [ ] Verify next mission unlocks

## Notes for Discord Dev Forum

Potential thread title:

```text
Echo Systems Lab Devlog - Milestone 1B Target Range Trial
```

Suggested first post:

```markdown
Starting Milestone 1B for Echo Systems Lab. The hub mission terminal MVP is working, so the next goal is to build the first playable system trial: a Target Range.

This will demonstrate ScriptableObject-driven ranged weapons, weapon pedestals, target hit detection, objective tracking, and mission progression.

I'm keeping the scene AI-free for now, so all destructible objects will be referred to as targets rather than enemies. The goal is to build a reusable weapon/target framework that can later be expanded into full combat systems.
```

## Portfolio Framing

This milestone should eventually be described as:

> A modular first-person target range demonstrating ScriptableObject-driven ranged weapons, interactable weapon pedestals, reusable hit detection, target health, objective tracking, and mission-based scene progression.

## Completion Checklist

Milestone 1B is complete when:

- [ ] Player can select the first mission from the hub terminal
- [ ] `TargetRange` scene loads successfully
- [ ] Player can interact with a weapon pedestal
- [ ] Player equips a ranged weapon
- [ ] Player can shoot
- [ ] Targets can receive damage
- [ ] Targets can be destroyed or cleared
- [ ] Objective tracks target completion
- [ ] Mission completion is registered
- [ ] Player can return to hub
- [ ] Next mission becomes unlocked
- [ ] No red Console errors
- [ ] Changes are committed to Git

## Related Notes

- [[Echo Systems Lab - Master Design Doc Baseline]]
- [[Echo Systems Lab - Milestone 1 Hub Terminal Loop]]
- [[ThirdPartyCredits]]
