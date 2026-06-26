# Save System Notes

## Purpose

The save system is responsible for preserving long-term player progress across scenes, quits, and reboots.

This system should store persistent profile-level data, not temporary scene-specific data.

---

## Persistent Profile State

Saved across sessions:

- Completed mission IDs
    
- Mission unlock progression
    
- Weapon type XP
    
- Future skill XP
    
- Future inventory data
    
- Future character data
    
- Future unlocked systems or abilities
    

---

## Session / Scene State

Not saved permanently yet:

- Target range shots fired during one run
    
- Target range hits during one run
    
- Target range accuracy during one run
    
- Current score for a single trial attempt
    
- Temporary target destruction state inside one trial instance
    

These values reset when the trial restarts.

---

## Settings State

Settings will eventually be separated from profile progression.

Future settings data may include:

- Audio volume
    
- Graphics options
    
- Controls / keybinds
    
- Accessibility options
    
- UI preferences
    

Potential future classes:

```text
SettingsData
SettingsManager
```

---

## Current Save Architecture

```text
SaveData
- Serializable data container
- Stores completed missions and persistent XP

SaveManager
- Loads save file
- Writes save file
- Creates new game state
- Deletes save file
- Persists across scenes

MissionProgress
- Runtime static mission completion tracker
- Can load from SaveData
- Can write back to SaveData

PlayerProgress
- Runtime static player progression tracker
- Currently stores weapon type XP
- Can load from SaveData
- Can write back to SaveData
```

---

## Current Design Rules

Completed missions should remain replayable.

Locked missions cannot be launched.

Available and completed missions can be launched.

Weapon XP is currently usage-based.

Target range score, shots, hits, and accuracy are session data.

Mission completion and weapon type XP are persistent data.

---

## Future Expansion

Potential future save data:

```text
InventoryData
CharacterData
SkillProgressData
UnlockedSystemsData
SettingsData
```

Potential future XP categories:

```text
Pistol
Shotgun
SMG
Bow
Crossbow
RPG
Running
Jumping
Climbing
Swimming
Grappling
Computer Use
NPC Interaction
Lifting
Crafting
```

---

## Debug Notes

Save file path uses:

```csharp
Application.persistentDataPath
```

Current file name:

```text
echo_systems_lab_save.json
```

During development, use Console logs to verify:

- New Game creates fresh SaveData
    
- Save Game writes to disk
    
- Load Game restores completed missions
    
- Mission terminal updates after loading
    
- Weapon type XP persists after reboot