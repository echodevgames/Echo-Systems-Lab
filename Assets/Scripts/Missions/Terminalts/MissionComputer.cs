//-----MissionComputer.cs START-----

using UnityEngine;

public class MissionComputer : MonoBehaviour, IInteractable
{
    [Header("Terminal")]
    [SerializeField] private MissionTerminalData terminalData;
    [SerializeField] private MissionTerminalUI terminalUI;

    [Header("Fallback Prompt")]
    [SerializeField] private string fallbackPromptText = "Press E to open Mission Terminal";

    private void Awake()
    {
        if (terminalUI == null)
            terminalUI = GetComponentInChildren<MissionTerminalUI>(true);
    }

    public string GetPromptText()
    {
        if (terminalData != null && !string.IsNullOrWhiteSpace(terminalData.promptText))
            return terminalData.promptText;

        return fallbackPromptText;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (terminalUI == null)
        {
            Debug.LogWarning($"{name} has no MissionTerminalUI assigned or found.");
            return;
        }

        //terminalUI.Open(interactor, terminalData);
    }
}

//-----MissionComputer.cs END-----