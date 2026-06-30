//-----PlayerProfileData.cs START-----

using System.IO.Enumeration;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName ="PlayerProfileData_NewProfile",
    menuName ="Echo Systems Lab/Player/Player Profile Data"
    )]
    public class PlayerProfileData : ScriptableObject
{
    [Header("Identity")]
    public string playerId = "player_echo";
    public string displayName = "Echo";
    public Sprite playerIcon;

    [Header("Starting States")]
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