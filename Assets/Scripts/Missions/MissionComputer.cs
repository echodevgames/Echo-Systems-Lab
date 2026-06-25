//-----MissionComputer.cs START-----

using UnityEngine;

public class MissionComputer : MonoBehaviour, IInteractable
{
    [SerializeField] private MissionTerminalUI terminalUI;
    [SerializeField] private string promptText = "Press E to open Mission Terminal";

    public string GetPromptText()
    {
        return promptText;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (terminalUI != null)
            terminalUI.Open();
    }
}
//-----MissionComputer.cs END-----