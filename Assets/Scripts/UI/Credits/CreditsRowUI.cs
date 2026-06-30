//-----CreditsRowUI.cs START-----

using TMPro;
using UnityEngine;

public class CreditsRowUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text categoryText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text creatorText;
    [SerializeField] private TMP_Text usageText;
    [SerializeField] private TMP_Text sourceText;
    [SerializeField] private TMP_Text licenseText;
    [SerializeField] private TMP_Text notesText;

    public void Setup(CreditEntry entry)
    {
        if (entry == null)
            return;

        if (categoryText != null)
            categoryText.text = entry.category.ToString();

        if (titleText != null)
            titleText.text = string.IsNullOrWhiteSpace(entry.title)
                ? "Untitled Credit"
                : entry.title;

        if (creatorText != null)
            creatorText.text = string.IsNullOrWhiteSpace(entry.creator)
                ? ""
                : $"By: {entry.creator}";

        if (usageText != null)
            usageText.text = string.IsNullOrWhiteSpace(entry.roleOrUsage)
                ? ""
                : $"Use: {entry.roleOrUsage}";

        if (sourceText != null)
        {
            if (!string.IsNullOrWhiteSpace(entry.sourceName) && !string.IsNullOrWhiteSpace(entry.sourceUrl))
                sourceText.text = $"Source: {entry.sourceName} | {entry.sourceUrl}";
            else if (!string.IsNullOrWhiteSpace(entry.sourceName))
                sourceText.text = $"Source: {entry.sourceName}";
            else if (!string.IsNullOrWhiteSpace(entry.sourceUrl))
                sourceText.text = $"Source: {entry.sourceUrl}";
            else
                sourceText.text = "";
        }

        if (licenseText != null)
            licenseText.text = string.IsNullOrWhiteSpace(entry.licenseNotes)
                ? ""
                : $"License: {entry.licenseNotes}";

        if (notesText != null)
            notesText.text = entry.additionalNotes;
    }
}

//-----CreditsRowUI.cs END-----