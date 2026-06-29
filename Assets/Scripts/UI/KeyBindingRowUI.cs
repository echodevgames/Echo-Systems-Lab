//-----KeybindingRowUI.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeybindingRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private TMP_Text bindingText;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Button resetButton;

    private KeybindingsMenuUI owner;
    private string actionPath;
    private string bindingName;
    private string displayName;

    public void Setup(
        KeybindingsMenuUI menuOwner,
        string newActionPath,
        string newBindingName,
        string newDisplayName)
    {
        owner = menuOwner;
        actionPath = newActionPath;
        bindingName = newBindingName;
        displayName = newDisplayName;

        if (actionNameText != null)
            actionNameText.text = displayName;

        if (rebindButton != null)
        {
            rebindButton.onClick.RemoveAllListeners();
            rebindButton.onClick.AddListener(() => owner.StartRebind(this));
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(() => owner.ResetBinding(this));
        }

        Refresh();
    }

    public string ActionPath => actionPath;
    public string BindingName => bindingName;

    public void SetBindingText(string text)
    {
        if (bindingText != null)
            bindingText.text = text;
    }

    public void SetListening()
    {
        SetBindingText("Press a key...");
    }

    public void Refresh()
    {
        if (owner == null)
            return;

        SetBindingText(owner.GetBindingDisplayString(actionPath, bindingName));
    }
}

//-----KeybindingRowUI.cs END-----