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
        int maxHealth = MaxHealth;
        int maxStamina = MaxStamina;
        int maxMana = MaxMana;

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentMana = maxMana;

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