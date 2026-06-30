//-----CreditsMenuUI.cs START-----

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject creditsRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text introText;
    [SerializeField] private Transform rowParent;
    [SerializeField] private CreditsRowUI rowPrefab;
    [SerializeField] private Button backButton;

    [Header("Data")]
    [SerializeField] private CreditsDatabase creditsDatabase;

    private readonly List<CreditsRowUI> spawnedRows = new List<CreditsRowUI>();
    private GameObject returnPanel;

    private void Awake()
    {
        if (backButton != null)
            backButton.onClick.AddListener(Close);

        if (creditsRoot != null)
            creditsRoot.SetActive(false);
    }

    public void Open()
    {
        OpenFrom(null);
    }

    public void OpenFrom(GameObject panelToReturnTo)
    {
        returnPanel = panelToReturnTo;

        if (returnPanel != null)
            returnPanel.SetActive(false);

        BuildCredits();

        if (creditsRoot != null)
            creditsRoot.SetActive(true);
    }

    public void Close()
    {
        if (creditsRoot != null)
            creditsRoot.SetActive(false);

        if (returnPanel != null)
            returnPanel.SetActive(true);

        returnPanel = null;
    }

    private void BuildCredits()
    {
        ClearRows();

        if (creditsDatabase == null)
        {
            if (titleText != null)
                titleText.text = "Credits";

            if (introText != null)
                introText.text = "No CreditsDatabase assigned.";

            return;
        }

        if (titleText != null)
            titleText.text = creditsDatabase.creditsTitle;

        if (introText != null)
            introText.text = creditsDatabase.creditsIntro;

        if (rowParent == null || rowPrefab == null || creditsDatabase.entries == null)
            return;

        foreach (CreditEntry entry in creditsDatabase.entries)
        {
            if (entry == null || !entry.showInMenu)
                continue;

            CreditsRowUI row = Instantiate(rowPrefab, rowParent);
            row.Setup(entry);
            spawnedRows.Add(row);
        }

        if (rowParent is RectTransform rectTransform)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void ClearRows()
    {
        spawnedRows.Clear();

        if (rowParent == null)
            return;

        for (int i = rowParent.childCount - 1; i >= 0; i--)
            Destroy(rowParent.GetChild(i).gameObject);
    }
}

//-----CreditsMenuUI.cs END-----