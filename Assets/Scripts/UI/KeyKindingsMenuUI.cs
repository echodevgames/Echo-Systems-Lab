//-----KeybindingsMenuUI.cs START-----

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeybindingsMenuUI : MonoBehaviour
{
    [Serializable]
    public class BindingEntry
    {
        public string displayName;
        public string actionPath;
        public string bindingName;
    }

    [Header("References")]
    [SerializeField] private GameObject keybindingsRoot;
    [SerializeField] private Transform rowParent;
    [SerializeField] private KeybindingRowUI rowPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private Button resetAllButton;
    [SerializeField] private PauseMenuController pauseMenuController;
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Bindings")]
    [SerializeField]
    private BindingEntry[] bindingEntries =
    {
        new BindingEntry { displayName = "Move Forward", actionPath = "Player/Move", bindingName = "up" },
        new BindingEntry { displayName = "Move Backward", actionPath = "Player/Move", bindingName = "down" },
        new BindingEntry { displayName = "Move Left", actionPath = "Player/Move", bindingName = "left" },
        new BindingEntry { displayName = "Move Right", actionPath = "Player/Move", bindingName = "right" },

        new BindingEntry { displayName = "Fire", actionPath = "Player/Attack", bindingName = "" },
        new BindingEntry { displayName = "Interact", actionPath = "Player/Interact", bindingName = "" },
        new BindingEntry { displayName = "Reload / Return", actionPath = "Player/Reload", bindingName = "" },
        new BindingEntry { displayName = "Next Weapon", actionPath = "Player/Next", bindingName = "" },
        new BindingEntry { displayName = "Previous Weapon", actionPath = "Player/Previous", bindingName = "" },
        new BindingEntry { displayName = "Pause", actionPath = "Player/Pause", bindingName = "" }
    };

    private readonly List<KeybindingRowUI> rows = new List<KeybindingRowUI>();
    private InputActionRebindingExtensions.RebindingOperation currentRebind;

    private void Awake()
    {
        if (inputReader == null)
            inputReader = FindFirstObjectByType<PlayerInputReader>();

        if (backButton != null)
            backButton.onClick.AddListener(Close);

        if (resetAllButton != null)
            resetAllButton.onClick.AddListener(ResetAllBindings);

        if (keybindingsRoot != null)
            keybindingsRoot.SetActive(false);

        BuildRows();
    }

    private void OnDestroy()
    {
        currentRebind?.Dispose();
    }

    public void Open()
    {
        if (keybindingsRoot != null)
            keybindingsRoot.SetActive(true);

        RefreshRows();
    }

    public void Close()
    {
        currentRebind?.Cancel();

        if (keybindingsRoot != null)
            keybindingsRoot.SetActive(false);

        if (pauseMenuController != null)
            pauseMenuController.ShowMainPausePanel();
    }

    private void BuildRows()
    {
        if (rowParent == null || rowPrefab == null)
            return;

        ClearRows();

        foreach (BindingEntry entry in bindingEntries)
        {
            KeybindingRowUI row = Instantiate(rowPrefab, rowParent);
            row.Setup(this, entry.actionPath, entry.bindingName, entry.displayName);
            rows.Add(row);
        }
    }

    private void ClearRows()
    {
        rows.Clear();

        if (rowParent == null)
            return;

        for (int i = rowParent.childCount - 1; i >= 0; i--)
            Destroy(rowParent.GetChild(i).gameObject);
    }

    private void RefreshRows()
    {
        foreach (KeybindingRowUI row in rows)
            row.Refresh();
    }

    public string GetBindingDisplayString(string actionPath, string bindingName)
    {
        InputAction action = GetAction(actionPath);

        if (action == null)
            return "Missing Action";

        int bindingIndex = GetBindingIndex(action, bindingName);

        if (bindingIndex < 0)
            return "Missing Binding";

        return action.GetBindingDisplayString(bindingIndex);
    }

    public void StartRebind(KeybindingRowUI row)
    {
        if (row == null || inputReader == null)
            return;

        InputAction action = GetAction(row.ActionPath);

        if (action == null)
            return;

        int bindingIndex = GetBindingIndex(action, row.BindingName);

        if (bindingIndex < 0)
            return;

        currentRebind?.Dispose();

        row.SetListening();

        action.Disable();

        currentRebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnCancel(operation =>
            {
                action.Enable();
                operation.Dispose();
                currentRebind = null;
                row.Refresh();
            })
            .OnComplete(operation =>
            {
                action.Enable();
                operation.Dispose();
                currentRebind = null;

                inputReader.SaveBindingOverrides();

                RefreshRows();
            });

        currentRebind.Start();
    }

    public void ResetBinding(KeybindingRowUI row)
    {
        if (row == null || inputReader == null)
            return;

        InputAction action = GetAction(row.ActionPath);

        if (action == null)
            return;

        int bindingIndex = GetBindingIndex(action, row.BindingName);

        if (bindingIndex < 0)
            return;

        action.RemoveBindingOverride(bindingIndex);
        inputReader.SaveBindingOverrides();

        row.Refresh();
    }

    private void ResetAllBindings()
    {
        if (inputReader == null)
            return;

        inputReader.ResetBindingOverrides();
        RefreshRows();
    }

    private InputAction GetAction(string actionPath)
    {
        if (inputReader == null)
            return null;

        return inputReader.FindAction(actionPath);
    }

    private int GetBindingIndex(InputAction action, string bindingName)
    {
        if (action == null)
            return -1;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            if (binding.isComposite)
                continue;

            if (!string.IsNullOrWhiteSpace(bindingName))
            {
                if (binding.name == bindingName)
                    return i;

                continue;
            }

            if (!binding.isPartOfComposite)
                return i;
        }

        return -1;
    }
}

//-----KeybindingsMenuUI.cs END-----