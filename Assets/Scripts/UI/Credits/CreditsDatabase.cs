//-----CreditsDatabase.cs START-----

using System;
using UnityEngine;

public enum CreditCategory
{
    Project,
    AssetPack,
    Tool,
    Font,
    Music,
    Sound,
    SpecialThanks,
    Other
}

[CreateAssetMenu(
    fileName = "CreditsDatabase",
    menuName = "Echo Systems Lab/UI/Credits Database")]
public class CreditsDatabase : ScriptableObject
{
    [Header("Header")]
    public string creditsTitle = "Credits";

    [TextArea(2, 5)]
    public string creditsIntro =
        "Echo Systems Lab uses a mix of original systems, placeholder art, third-party assets, tools, and test content.";

    [Header("Entries")]
    public CreditEntry[] entries;
}

[Serializable]
public class CreditEntry
{
    public bool showInMenu = true;

    public CreditCategory category = CreditCategory.Other;

    [Header("Display")]
    public string title;
    public string creator;
    public string roleOrUsage;

    [Header("Source / License")]
    public string sourceName;
    public string sourceUrl;
    public string licenseNotes;

    [TextArea(2, 5)]
    public string additionalNotes;
}

//-----CreditsDatabase.cs END-----