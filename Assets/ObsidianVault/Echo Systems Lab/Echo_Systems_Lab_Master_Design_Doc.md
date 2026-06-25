# Echo Systems Lab

**Master Design Doc - 3D Unity Gameplay Systems Portfolio Project**

| Field | Definition |
|---|---|
| Owner | Jesse “Echo” Adams / EchoDevGames |
| Project Type | 3D Unity portfolio framework and reusable gameplay systems library |
| Primary Goal | Populate the portfolio Systems tab with polished, cohesive demonstrations of scalable gameplay architecture. |
| Positioning | Gameplay Programmer / Gameplay Systems Engineer |
| Core Loop | Hub -> Mission Board -> System Trial -> Complete Objective -> Save Progress -> Unlock Next Trial |
| Document Version | v0.1 - Master concept baseline |

## Elevator Pitch

Echo Systems Lab is a cohesive 3D Unity gameplay framework built around a hub-and-mission structure. Each mission scene demonstrates a major reusable gameplay system, including AI, combat, dialogue, inventory, camera, save/load, procedural encounters, and debug tools. The project is both a playable portfolio piece and a template library for future games.

## 1. Vision and Portfolio Purpose

Echo Systems Lab is designed to make the portfolio Systems tab feel deliberate, technical, and recruiter-friendly. Rather than presenting disconnected mechanics, the project should demonstrate that multiple gameplay systems can be designed, built, maintained, reused, and combined into complete gameplay loops.

- **Portfolio goal:** Show scalable architecture, reusable systems, technical problem solving, and Unity gameplay engineering depth.
- **Engineering goal:** Build clean template systems that can be reused in future Unity projects.
- **Presentation goal:** Make each system understandable through playable scenes, visible debug feedback, documentation, and GitHub-ready source structure.
- **Scope goal:** Keep visual content simple and use polish where it matters: feedback, clarity, architecture, and documentation.

> **Guiding Question**  
> Does this demonstrate that I can design, build, and maintain scalable gameplay systems? If yes, it belongs in the project. If not, it is probably scope smoke.

## 2. Design Pillars

| Pillar | Meaning | Practical Rule |
|---|---|---|
| Reusable by Default | Each feature should be built as a template-friendly system. | Avoid one-off scene scripts unless they are glue code. |
| Data-Driven When Useful | Use ScriptableObjects for missions, attacks, items, dialogue, objectives, enemies, and rewards. | New content should be creatable without editing core logic. |
| Observable Systems | Make system state visible through UI, debug panels, logs, and editor gizmos. | A recruiter should understand the system within seconds. |
| Small Scenes, Strong Loops | Each scene should be compact but complete. | One polished loop is better than five half-built ideas. |
| Cohesive Framework | All demos should connect through the same hub, save state, mission manager, and UI language. | Avoid random demo room energy. |
| Portfolio First | Every feature should produce a screenshot, GIF, README section, or devlog topic. | If it cannot be explained clearly, redesign it. |

## 3. Player Experience

The player begins in a compact 3D hub. From there, they interact with a mission board to enter system trials. Completing a trial returns progress to the hub, updates saved completion data, and unlocks the next trial. The experience should feel like a clean training facility, guild hall, or systems laboratory rather than a sprawling game world.

1. Player enters the hub.
2. Player interacts with the mission board.
3. Player selects an available system trial.
4. The selected scene loads.
5. Mission objectives appear in the UI.
6. Player completes the objective using the featured system.
7. Mission completion is saved.
8. Player returns to the hub and sees updated progress.
9. New missions or rewards unlock.

> **Recommended Theme**  
> Echo Systems Lab: a clean sci-fi testing facility or gameplay systems lab where each chamber evaluates a different module. This naturally supports debug overlays, modular scenes, and a professional tech-demo tone.

## 4. Full System Overview

The overarching system is a Mission / Encounter Framework. Every scene plugs into this shared framework rather than inventing its own objective logic. This makes the project feel like one complete architecture instead of a folder of unrelated experiments.

| System | Purpose | Portfolio Signal |
|---|---|---|
| Mission System | Defines mission metadata, target scene, objectives, rewards, prerequisites, and completion state. | Reusable gameplay framework and content pipeline. |
| Objective System | Tracks elimination, collection, interaction, survival, boss, and escort-style objectives. | Extensible rule architecture. |
| Interaction System | Supports prompts, NPCs, pickups, doors, mission board, terminals, and quest objects. | Clean player-to-world communication. |
| AI Framework | State-machine enemies with patrol, chase, investigate, attack, flee, and alert behaviors. | Scalable enemy architecture. |
| Combat / Ability System | ScriptableObject-driven attacks, cooldowns, hit detection, knockback, and status effects. | Data-driven gameplay programming. |
| Dialogue / Quest System | Branching dialogue connected to conditions, quest state, items, and rewards. | Narrative systems integration. |
| Inventory / Item System | Items, pickups, stackables, consumables, equipment, quest items, and item effects. | Data modeling and UI-state sync. |
| Save System | Persists mission completion, inventory, unlocked trials, NPC states, settings, and player data. | Serialization and cross-scene persistence. |
| Camera Framework | Follow, lock-on, cinematic focus, zone cameras, boss cameras, and smooth transitions. | Gameplay feel and reusable camera architecture. |
| Debug Tools | Runtime debug panel, AI state display, event log, objective tracker, spawn tools, and save inspector. | Tool-minded engineering. |

## 5. Scene and System Breakdown

### Hub Scene - Mission Board and Persistence

- Central navigation space for selecting trials.
- Shows completed, locked, and available missions.
- Demonstrates scene loading, interaction prompts, save data, player profile progress, and settings persistence.
- Acts as the anchor that ties every system demo together.

### Combat Trial - Data-Driven Abilities

- Arena with simple enemies and multiple player abilities.
- Demonstrates ScriptableObject attack definitions, hitboxes, cooldowns, damage types, knockback, animation events, enemy health, and combat feedback.
- Recommended attacks: light strike, heavy strike, dash attack, projectile, area attack, stun ability.

### AI Trial - State Machine Enemy Behaviors

- Stealth/combat arena where enemies patrol, detect, investigate, chase, attack, and return to patrol.
- Includes visible enemy debug labels showing current state, suspicion, target, and last known player position.
- Optional extension: enemy alert propagation and group behavior.

### Dialogue and Quest Trial

- NPC gives a task, tracks progress, reacts to state changes, and rewards completion.
- Demonstrates dialogue nodes, branching choices, quest conditions, objective state, NPC memory, and save integration.
- Example quest: find a missing power core, return it, and unlock a reward.

### Inventory and Item Trial

- Loot room or small survival test with pickups, inventory UI, consumables, equipment, and quest items.
- Demonstrates item definitions, stackable items, item use effects, UI updates, and persistence.
- Recommended items: health potion, keycard, damage buff, speed boots, quest item, throwable bomb.

### Procedural Encounter Trial

- Small modular encounter generated from a seed.
- Demonstrates procedural layout, spawn rules, objective placement, reward placement, and debug visualization.
- Keep this tight: 3 to 5 room modules, one objective, one reward chest, and visible seed data.

### Camera Trial

- Camera test course with follow, lock-on, cinematic trigger, room camera, boss camera, shake, and blends.
- Demonstrates camera zones, camera states, target focus, transitions, and reusable trigger configuration.

### Boss / Encounter Director Trial

- Final scene combines all major systems into one polished encounter.
- Demonstrates AI phases, combat abilities, minion spawning, boss UI, camera behavior, dialogue barks, checkpoint save, and mission completion.
- This is the fireworks room: compact, readable, and polished.

## 6. Core Architecture

The project should be organized like a reusable gameplay toolkit. Systems should depend on interfaces, events, and data assets rather than hard references wherever reasonable. The goal is not over-engineering; the goal is clean separation between data, runtime logic, presentation, and scene-specific glue.

```text
Assets/_EchoSystemsLab
  Core
    Bootstrapper
    SceneLoader
    GameEvents
    SaveManager
    DebugPanel
  Interaction
    Interactable
    InteractionDetector
    InteractionPromptUI
  MissionSystem
    MissionData
    ObjectiveData
    MissionManager
    ObjectiveTracker
  AI
    AIController
    AIState
    PatrolState
    InvestigateState
    ChaseState
    AttackState
    FleeState
  Combat
    Health
    Damageable
    AttackData
    Hitbox
    AbilityController
    StatusEffect
  Inventory
    ItemData
    Inventory
    InventorySlot
    ItemUseEffect
  Dialogue
    DialogueData
    DialogueNode
    DialogueRunner
    DialogueCondition
  Camera
    CameraController
    CameraZone
    CameraState
    CameraTrigger
  UI
    HUD
    MissionUI
    InventoryUI
    DialogueUI
    DebugUI
```

| Architecture Rule | Reason |
|---|---|
| Scene logic should be thin. | Scenes configure systems; systems do the work. |
| ScriptableObjects define content. | Content creation becomes fast, readable, and portfolio-friendly. |
| Events communicate state changes. | Systems stay decoupled and easier to test. |
| Interfaces define interactions. | Interactables, damageables, objectives, and item effects stay flexible. |
| Debug visibility is built in early. | Observable systems are easier to present, test, and explain. |

## 7. Data-Driven Design

The strongest portfolio value comes from showing that new content can be added through data assets rather than rewriting gameplay code. The following assets should be treated as core content templates.

| Data Asset | Example Fields | Used By |
|---|---|---|
| MissionData | Mission name, scene, objectives, prerequisites, rewards, description, icon. | Mission board, MissionManager, SaveManager, UI. |
| ObjectiveData | Objective type, target ID, required count, timer, optional marker, completion event. | ObjectiveTracker, MissionUI, EncounterDirector. |
| AttackData | Damage, range, cooldown, knockback, animation trigger, hit effect, sound effect, status effect. | AbilityController, Hitbox, AnimationEventRelay. |
| ItemData | Name, icon, stack size, item type, use effect, description, rarity. | Inventory, pickups, quest conditions, rewards. |
| DialogueData | Nodes, choices, speaker, conditions, events, quest actions. | DialogueRunner, NPCs, QuestSystem. |
| EnemyData | Health, move speed, detection stats, attack data, loot table, AI profile. | AIController, spawners, EncounterDirector. |
| CameraZoneData | Mode, target, blend speed, bounds, shake settings, priority. | CameraController and CameraTriggers. |

## 8. Debug and Developer Tools

A runtime developer panel should be treated as a core feature, not an afterthought. It will make the project easier to build and much easier to present. This is a major portfolio differentiator because it shows tool awareness and system observability.

- Current mission and objective state.
- Current scene and loaded systems.
- Player inventory contents.
- Save slot and saved completion state.
- AI state list for all active enemies.
- Spawned enemies and active encounter phase.
- Camera mode and active camera zone.
- Event log showing mission, combat, dialogue, pickup, and save events.
- Buttons for reset mission, complete objective, spawn enemy, clear inventory, and reload scene.

> **Portfolio Note**  
> The debug panel should be shown in GIFs and screenshots. It tells recruiters: “I build systems that other developers can inspect, test, and reuse.”

## 9. Milestone Roadmap

| Milestone | Build Target | Definition of Done |
|---|---|---|
| Milestone 1 | Hub + Mission Board + Scene Loading | Player can enter hub, open mission board, select Combat Trial, load scene, return to hub. |
| Milestone 2 | Mission + Objective Framework | MissionData and ObjectiveData drive at least one “Defeat 3 enemies” mission from start to completion. |
| Milestone 3 | Combat Trial | Player can use data-driven abilities to defeat enemies; mission completion saves. |
| Milestone 4 | AI Trial | Enemies patrol, detect, investigate, chase, attack, and expose current state through debug UI. |
| Milestone 5 | Dialogue + Quest Trial | NPC dialogue changes based on quest state and saved progress. |
| Milestone 6 | Inventory + Item Trial | Player collects, stores, uses, and saves items. |
| Milestone 7 | Camera Trial | Multiple camera modes work through reusable camera zones and smooth blends. |
| Milestone 8 | Procedural Encounter Trial | Seeded encounter generation creates a compact room layout, spawns enemies, places objective and reward. |
| Milestone 9 | Boss / Encounter Director Trial | Final encounter combines mission, AI, combat, camera, UI, save, and rewards. |
| Milestone 10 | Portfolio Polish Pass | README, diagrams, screenshots, GIFs, code comments, and Systems tab writeups are complete. |

## 10. Portfolio Presentation Plan

The project should produce both a full featured project page and individual Systems tab cards. Each system card should describe the problem, the architecture, the implementation, and the result.

| Portfolio Card | Headline | Evidence to Capture |
|---|---|---|
| AI Framework | State-machine enemy architecture with patrol, chase, investigate, attack, and alert behaviors. | GIF of enemy state changes and debug labels. |
| Combat Framework | Data-driven ability system using ScriptableObjects, hit detection, cooldowns, knockback, and animation events. | GIF of attacks plus inspector screenshot of AttackData. |
| Mission Framework | Reusable objective system supporting elimination, collection, interaction, survival, and boss encounters. | Mission board screenshot and objective completion GIF. |
| Dialogue Framework | Branching NPC conversations with condition checks and quest-state integration. | GIF showing dialogue before, during, and after quest completion. |
| Inventory Framework | Data-driven items, stackable pickups, use effects, quest items, and persistence. | Inventory UI screenshot and ItemData inspector screenshot. |
| Save Framework | Persistent player progress, inventory, completed missions, unlocked scenes, and NPC states. | Before/after save demonstration GIF. |
| Camera Framework | Zone-based camera behavior with follow, lock, cinematic focus, and boss encounter support. | Camera transition GIF and camera zone inspector screenshot. |
| Debug Tools | Runtime system inspector for missions, AI, inventory, save state, event logs, and encounter data. | Debug panel screenshot in an active trial. |

## 11. Reusable Template Goals

This project should leave behind systems that can be lifted into future games with minimal rewriting. Each framework should have a small demo scene, README notes, example assets, and clear setup instructions.

- Mission framework usable for brawlers, RPGs, adventure games, and survival prototypes.
- Objective framework with modular objective types and easy extension points.
- AI state machine that supports future enemy types without rewriting the controller.
- Combat and ability framework that supports melee, ranged, area, and status-driven attacks.
- Dialogue and quest framework that can power NPC interactions in future projects.
- Inventory and item system that supports pickups, consumables, quest items, and rewards.
- Camera zone framework reusable in 2.5D, 3D, boss, and cinematic contexts.
- Save framework designed to persist cross-scene data cleanly.

## 12. Open Questions and Future Expansion

| Question | Decision Needed |
|---|---|
| Final Theme | Sci-fi lab, fantasy guild, or hybrid systems foundry presentation? |
| Player Controller Depth | Simple third-person controller, top-down controller, or modular controller showcase? |
| Visual Style | Clean prototype primitives, low-poly stylized, or modular asset pack? |
| Combat Style | Melee-focused, ranged-focused, or ability sandbox? |
| Save Format | JSON file save, binary serialization, or Unity-friendly hybrid? |
| Scene Management | Single additive hub flow or traditional scene loading per trial? |
| Editor Tooling | Should custom inspectors and validation tools be included in v1? |
| Public Repo Scope | One full repo, separate package repos, or both? |

## Final Baseline Decision

> **Recommended Starting Build**  
> Start with the smallest complete loop: Hub + Mission Board + Combat Trial + Mission Completion + Save Progress + Return to Hub. This creates the skeleton that every other system can plug into.

Once that loop exists, every new scene becomes a plug-in chamber rather than a fresh project. The final result should say: I do not just make isolated features. I build frameworks.

## Glossary

| Field | Definition |
|---|---|
| Trial | A playable scene designed to demonstrate one major gameplay system. |
| Mission | A data-defined contract that loads a scene, tracks objectives, grants rewards, and saves completion. |
| Objective | A reusable rule that can be completed through actions such as defeating enemies, collecting items, interacting, surviving, or completing dialogue. |
| Encounter Director | Runtime controller that coordinates enemy waves, phases, objective rewards, and completion flow. |
| Debug Panel | Runtime UI used to inspect current system states and speed up testing/presentation. |
