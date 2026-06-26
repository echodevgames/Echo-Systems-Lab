//-----MissionComputer.cs START-----

using UnityEngine;

public class MissionComputer : MonoBehaviour, IInteractable
{
    [SerializeField] private MissionTerminalUI terminalUI;
    [SerializeField] private string promptText = "Press E to open Mission Terminal";

    private void Awake()
    {
        if (terminalUI == null)
            terminalUI = GetComponentInChildren<MissionTerminalUI>(true);
    }

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (terminalUI == null)
        {
            Debug.LogWarning($"{name} has no MissionTerminalUI assigned or found.");
            return;
        }

        terminalUI.Open(interactor);
    }
}
//-----MissionComputer.cs END-----