# Milestone 3D - Player Core HUD Foundation

## 2. Goal Line

```text
Player enters gameplay scene
-> HUD shows player icon
-> HUD shows health, stamina, mana
-> HUD shows current level
-> HUD shows character XP placeholder
-> HUD shows score
-> UI updates when player stats change
-> system is ready for future character menu, inventory menu, skill XP popups, and save integration
```

This milestone is mostly visual and system plumbing. We are building the player dashboard before the deeper RPG math arrives.

---

## 3. Naming / Scope Notes

For this checkpoint:

```text
Health = real player survival stat, but damage sources can come later.
Stamina = placeholder resource for sprinting, jumping, climbing, dodging, etc.
Mana = placeholder resource for future abilities.
Level = character level placeholder.
Character XP = placeholder. Later, only enemy kills give real character XP.
Score = global/player score display.
PlayerIcon = profile/HUD identity icon.
```

Important future notes to preserve:

```text
Weapon durability is coming later.
Everything should eventually be able to break.
Skill XP popups should eventually reuse the save notification / toast UI location.
```

Weapon durability should not live as mutable data directly inside `WeaponData`. Later, use a runtime or persistent weapon state object.

Future structure idea:

```text
WeaponData
- maxDurability
- durabilityLossPerShot

WeaponRuntimeState
- weaponId
- currentDurability
- maxDurability

PlayerWeaponDurabilityController
- reduces durability on use
- blocks or weakens broken weapons
- repairs weapons
```

---

## 4. System Pieces

```text
1. PlayerProfileData ScriptableObject
2. PlayerStatsController runtime component
3. PlayerCoreHUD display component
4. HUD canvas layout
5. Optional TargetRangeTracker score hookup
6. Inspector test flow
7. Future notes for weapon durability and skill XP popup system
```

---

## 5. Folder Setup

Create or confirm these folders:

```text
Assets/_EchoSystemsLab/Scripts/Player/
Assets/_EchoSystemsLab/Scripts/UI/HUD/
Assets/_EchoSystemsLab/ScriptableObjects/Player/
Assets/_EchoSystemsLab/Prefabs/UI/HUD/
Assets/_EchoSystemsLab/Textures/UI/Icons/Player/
```

If your project is currently using `Assets/Scripts/` instead of `Assets/_EchoSystemsLab/Scripts/`, keep using the existing structure for consistency.

---

## 6. Numbered Implementation Steps

## Step 1 - Create PlayerProfileData

### File Path

```text
Assets/_EchoSystemsLab/Scripts/Player/PlayerProfileData.cs
```

### Script Name

```text
PlayerProfileData.cs
```

### Why This Step Matters

This gives the player HUD a clean ScriptableObject source for identity and starting values.

```csharp
//-----PlayerProfileData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerProfileData_NewProfile",
    menuName = "Echo Systems Lab/Player/Player Profile Data")]
public class PlayerProfileData : ScriptableObject
{
    [Header("Identity")]
    public string playerId = "player_echo";
    public string displayName = "Echo";
    public Sprite playerIcon;

    [Header("Starting Stats")]
    public int startingLevel = 1;
    public int startingScore = 0;

    [Header("Health")]
    public int maxHealth = 100;

    [Header("Stamina")]
    public int maxStamina = 100;

    [Header("Mana")]
    public int maxMana = 100;

    [Header("Experience")]
    public int startingExperience = 0;
    public int startingExperienceToNextLevel = 100;
    public float levelUpXpMultiplier = 1.25f;
}

//-----PlayerProfileData.cs END-----
```

### Inspector Setup

Create:

```text
PlayerProfileData_Echo
```

Suggested values:

```text
Player Id: player_echo
Display Name: Echo
Starting Level: 1
Starting Score: 0
Max Health: 100
Max Stamina: 100
Max Mana: 100
Starting XP: 0
XP To Next Level: 100
Level Up XP Multiplier: 1.25
```

Assign a player icon if one exists. Otherwise, leave it blank for now.

---

## Step 2 - Create PlayerStatsController

### File Path

```text
Assets/_EchoSystemsLab/Scripts/Player/PlayerStatsController.cs
```

### Script Name

```text
PlayerStatsController.cs
```

### Why This Step Matters

This becomes the runtime source of truth for core player variables.

```csharp
//-----PlayerStatsController.cs START-----

using System;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
    public event Action OnStatsChanged;

    [Header("Profile")]
    [SerializeField] private PlayerProfileData profileData;

    [Header("Runtime State")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentStamina;
    [SerializeField] private int currentMana;
    [SerializeField] private int currentLevel;
    [SerializeField] private int currentExperience;
    [SerializeField] private int experienceToNextLevel;
    [SerializeField] private int currentScore;

    public PlayerProfileData ProfileData => profileData;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => profileData != null ? profileData.maxHealth : 100;

    public int CurrentStamina => currentStamina;
    public int MaxStamina => profileData != null ? profileData.maxStamina : 100;

    public int CurrentMana => currentMana;
    public int MaxMana => profileData != null ? profileData.maxMana : 100;

    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public int CurrentScore => currentScore;

    private void Awake()
    {
        InitializeFromProfile();
    }

    private void InitializeFromProfile()
    {
        currentHealth = MaxHealth;
        currentStamina = MaxStamina;
        currentMana = MaxMana;

        currentLevel = profileData != null ? Mathf.Max(1, profileData.startingLevel) : 1;
        currentExperience = profileData != null ? Mathf.Max(0, profileData.startingExperience) : 0;
        experienceToNextLevel = profileData != null ? Mathf.Max(1, profileData.startingExperienceToNextLevel) : 100;
        currentScore = profileData != null ? Mathf.Max(0, profileData.startingScore) : 0;

        NotifyStatsChanged();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        NotifyStatsChanged();
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        NotifyStatsChanged();
    }

    public bool UseStamina(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentStamina < amount)
            return false;

        currentStamina -= amount;
        NotifyStatsChanged();
        return true;
    }

    public void RestoreStamina(int amount)
    {
        if (amount <= 0)
            return;

        currentStamina = Mathf.Min(MaxStamina, currentStamina + amount);
        NotifyStatsChanged();
    }

    public bool UseMana(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentMana < amount)
            return false;

        currentMana -= amount;
        NotifyStatsChanged();
        return true;
    }

    public void RestoreMana(int amount)
    {
        if (amount <= 0)
            return;

        currentMana = Mathf.Min(MaxMana, currentMana + amount);
        NotifyStatsChanged();
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
            return;

        currentScore += amount;
        NotifyStatsChanged();
    }

    public void AddCharacterExperience(int amount)
    {
        if (amount <= 0)
            return;

        currentExperience += amount;

        while (currentExperience >= experienceToNextLevel)
            LevelUp();

        NotifyStatsChanged();
    }

    private void LevelUp()
    {
        currentExperience -= experienceToNextLevel;
        currentLevel++;

        float multiplier = profileData != null ? profileData.levelUpXpMultiplier : 1.25f;
        experienceToNextLevel = Mathf.Max(1, Mathf.RoundToInt(experienceToNextLevel * multiplier));

        Debug.Log($"Level up! New level: {currentLevel}");
    }

    private void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    [ContextMenu("Debug/Take 10 Damage")]
    private void DebugTakeDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Debug/Heal 10")]
    private void DebugHeal()
    {
        Heal(10);
    }

    [ContextMenu("Debug/Use 10 Stamina")]
    private void DebugUseStamina()
    {
        UseStamina(10);
    }

    [ContextMenu("Debug/Restore 10 Stamina")]
    private void DebugRestoreStamina()
    {
        RestoreStamina(10);
    }

    [ContextMenu("Debug/Use 10 Mana")]
    private void DebugUseMana()
    {
        UseMana(10);
    }

    [ContextMenu("Debug/Restore 10 Mana")]
    private void DebugRestoreMana()
    {
        RestoreMana(10);
    }

    [ContextMenu("Debug/Add 25 XP")]
    private void DebugAddXp()
    {
        AddCharacterExperience(25);
    }

    [ContextMenu("Debug/Add 100 Score")]
    private void DebugAddScore()
    {
        AddScore(100);
    }
}

//-----PlayerStatsController.cs END-----
```

### Inspector Setup

Add to Player:

```text
Player
- PlayerStatsController
  - Profile Data = PlayerProfileData_Echo
```

---

## Step 3 - Create PlayerCoreHUD

### File Path

```text
Assets/_EchoSystemsLab/Scripts/UI/HUD/PlayerCoreHUD.cs
```

### Script Name

```text
PlayerCoreHUD.cs
```

### Why This Step Matters

This creates the player stat HUD without mixing UI logic into the player stat system.

```csharp
//-----PlayerCoreHUD.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCoreHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatsController playerStats;

    [Header("Identity UI")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image playerIconImage;

    [Header("Level / Score UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Health UI")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image healthFillImage;

    [Header("Stamina UI")]
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private Image staminaFillImage;

    [Header("Mana UI")]
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private Image manaFillImage;

    [Header("Experience UI")]
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private Image experienceFillImage;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStatsController>();
    }

    private void OnEnable()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= Refresh;
    }

    public void Refresh()
    {
        if (playerStats == null)
            return;

        RefreshIdentity();
        RefreshLevelAndScore();
        RefreshHealth();
        RefreshStamina();
        RefreshMana();
        RefreshExperience();
    }

    private void RefreshIdentity()
    {
        PlayerProfileData profile = playerStats.ProfileData;

        if (playerNameText != null)
        {
            if (profile != null && !string.IsNullOrWhiteSpace(profile.displayName))
                playerNameText.text = profile.displayName;
            else
                playerNameText.text = "Player";
        }

        if (playerIconImage != null)
        {
            Sprite icon = profile != null ? profile.playerIcon : null;
            playerIconImage.sprite = icon;
            playerIconImage.enabled = icon != null;
        }
    }

    private void RefreshLevelAndScore()
    {
        if (levelText != null)
            levelText.text = $"LVL {playerStats.CurrentLevel}";

        if (scoreText != null)
            scoreText.text = $"Score: {playerStats.CurrentScore}";
    }

    private void RefreshHealth()
    {
        if (healthText != null)
            healthText.text = $"{playerStats.CurrentHealth} / {playerStats.MaxHealth}";

        SetFill(healthFillImage, playerStats.CurrentHealth, playerStats.MaxHealth);
    }

    private void RefreshStamina()
    {
        if (staminaText != null)
            staminaText.text = $"{playerStats.CurrentStamina} / {playerStats.MaxStamina}";

        SetFill(staminaFillImage, playerStats.CurrentStamina, playerStats.MaxStamina);
    }

    private void RefreshMana()
    {
        if (manaText != null)
            manaText.text = $"{playerStats.CurrentMana} / {playerStats.MaxMana}";

        SetFill(manaFillImage, playerStats.CurrentMana, playerStats.MaxMana);
    }

    private void RefreshExperience()
    {
        if (experienceText != null)
            experienceText.text = $"{playerStats.CurrentExperience} / {playerStats.ExperienceToNextLevel} XP";

        SetFill(experienceFillImage, playerStats.CurrentExperience, playerStats.ExperienceToNextLevel);
    }

    private void SetFill(Image fillImage, int current, int max)
    {
        if (fillImage == null)
            return;

        if (max <= 0)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        fillImage.fillAmount = Mathf.Clamp01((float)current / max);
    }
}

//-----PlayerCoreHUD.cs END-----
```

---

## Step 4 - HUD Scene Setup

Create this hierarchy under your gameplay HUD canvas:

```text
Canvas_HUD
├── PlayerCoreHUDRoot
│   ├── PlayerIconImage
│   ├── PlayerNameText
│   ├── LevelText
│   ├── ScoreText
│   ├── HealthBar
│   │   ├── HealthFill
│   │   └── HealthText
│   ├── StaminaBar
│   │   ├── StaminaFill
│   │   └── StaminaText
│   ├── ManaBar
│   │   ├── ManaFill
│   │   └── ManaText
│   └── ExperienceBar
│       ├── ExperienceFill
│       └── ExperienceText
└── WeaponAmmoHUDRoot
```

Suggested placement:

```text
PlayerCoreHUDRoot = bottom left
WeaponAmmoHUDRoot = bottom right
TargetRangeHUD = upper left or top center
```

### Bar Setup

For each fill image:

```text
Image Type: Filled
Fill Method: Horizontal
Fill Origin: Left
Fill Amount: 1
```

---

## Step 5 - Assign PlayerCoreHUD References

On `PlayerCoreHUDRoot`:

```text
PlayerCoreHUD
- Player Stats = Player / PlayerStatsController
- Player Name Text = PlayerNameText
- Player Icon Image = PlayerIconImage
- Level Text = LevelText
- Score Text = ScoreText
- Health Text = HealthText
- Health Fill Image = HealthFill
- Stamina Text = StaminaText
- Stamina Fill Image = StaminaFill
- Mana Text = ManaText
- Mana Fill Image = ManaFill
- Experience Text = ExperienceText
- Experience Fill Image = ExperienceFill
```

---

## Step 6 - Optional Score Hook for Target Range

If you want target destruction to feed the player score HUD immediately, patch `TargetRangeTracker`.

Add near the top:

```csharp
[Header("Player Score")]
[SerializeField] private PlayerStatsController playerStatsController;
```

In `Start()`:

```csharp
if (playerStatsController == null)
    playerStatsController = FindFirstObjectByType<PlayerStatsController>();
```

In `RegisterTargetDestroyed`, after:

```csharp
score += target.ScoreValue;
```

add:

```csharp
if (playerStatsController != null)
    playerStatsController.AddScore(target.ScoreValue);
```

This keeps these two score values conceptually separate:

```text
TargetRangeTracker score = trial/session score
PlayerStatsController score = player/global score
```

Do not award character XP for targets yet. Real character XP comes from enemy kills later.

---

# 7. Unity Setup Checklist

## Player

```text
Player
- PlayerInputReader
- SimpleFirstPersonController
- PlayerInteractor
- PlayerWeaponController
- PlayerWeaponLoadoutController
- PlayerAmmoInventory
- PlayerStatsController
```

## ScriptableObject

```text
PlayerProfileData_Echo
- Display Name
- Player Icon
- Max Health
- Max Stamina
- Max Mana
- Starting Level
- Starting Score
- Starting XP
- XP To Next Level
```

## HUD

```text
Canvas_HUD
- PlayerCoreHUDRoot
  - PlayerCoreHUD
- WeaponAmmoHUDRoot
  - WeaponAmmoHUD
```

## Scene References

```text
PlayerCoreHUD.PlayerStats = Player / PlayerStatsController
Optional TargetRangeTracker.PlayerStatsController = Player / PlayerStatsController
```

---

# 8. Goal Line / Completion Checklist

```text
1. PlayerCoreHUD appears in gameplay scene.
2. Player icon displays if assigned.
3. Player name displays.
4. Health shows current / max.
5. Stamina shows current / max.
6. Mana shows current / max.
7. Level displays.
8. Character XP placeholder displays.
9. Score displays.
10. Health fill updates when damage/heal debug methods are used.
11. Stamina fill updates when stamina debug methods are used.
12. Mana fill updates when mana debug methods are used.
13. XP bar updates when debug XP is added.
14. Level increases if enough XP is added.
15. Score updates when debug score is added.
16. Optional: score updates when targets are destroyed.
17. Existing weapon ammo HUD still works.
18. Existing pause/keybindings UI still works.
```

---

# 9. Suggested Commit

```bash
git add .
git commit -m "Add player core HUD foundation"
```

Optional tag:

```bash
git tag milestone-3d-player-core-hud
```

---

# 10. Design Notes

This milestone creates the player's core runtime stat layer.

The important split:

```text
PlayerProfileData = starting stats and identity
PlayerStatsController = runtime stat state
PlayerCoreHUD = visual display
```

Future systems can call into `PlayerStatsController`:

```text
Enemies -> TakeDamage()
Healing pickups -> Heal()
Sprint/climb/jump -> UseStamina()
Rest zones -> RestoreStamina()
Spells/abilities -> UseMana()
Enemy kills -> AddCharacterExperience()
Target/system scoring -> AddScore()
```

Future note: weapon durability should be its own runtime state system.

Possible later structure:

```text
WeaponData
- maxDurability
- durabilityLossPerShot

WeaponRuntimeState
- weaponId
- currentDurability

PlayerWeaponDurabilityController
- reduces durability on use
- blocks or weakens broken weapons
- repairs weapons
```

Future note: skill XP popups can reuse the existing save notification/toast location.

Possible later structure:

```text
SkillXpToastUI
- +10 Pistol XP
- +25 Climbing XP
- +5 Computer Use XP
```

This should stay separate from the main HUD so it can float, pop, and fade without cluttering the cockpit.

---

# 11. Portfolio Value

This checkpoint demonstrates:

```text
ScriptableObject-driven player profile data
Runtime player stat controller
Event-driven HUD refresh
Health/stamina/mana UI bars
Level and XP placeholder architecture
Score display
Separation of data, runtime state, and UI presentation
Foundation for character menu and inventory UI
Foundation for future RPG-style progression
```

Possible next UI milestones:

```text
Milestone 3E - Credits Menu
Milestone 3E - Scrolling Weapon Bandolier UI
Milestone 3E - Character Menu Foundation
```
