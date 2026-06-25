//-----MissionTerminalUI.cs START-----

using UnityEngine;
using UnityEngine.UI;

public class MissionTerminalUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject terminalRoot;
    [SerializeField] private Transform missionButtonParent;
    [SerializeField] private MissionButtonUI missionButtonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private SimpleFirstPersonController playerController;

    [Header("Mission List")]
    [SerializeField] private MissionData[] missions;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        Close();
    }

    public void Open()
    {
        if (terminalRoot != null)
            terminalRoot.SetActive(true);

        if (playerController != null)
        {
            playerController.SetInputEnabled(false);
            playerController.SetCursorLocked(false);
        }

        PopulateMissionList();
    }

    public void Close()
    {
        if (terminalRoot != null)
            terminalRoot.SetActive(false);

        if (playerController != null)
        {
            playerController.SetInputEnabled(true);
            playerController.SetCursorLocked(true);
        }
    }

    private void PopulateMissionList()
    {
        ClearMissionButtons();

        foreach (MissionData mission in missions)
        {
            if (mission == null)
                continue;

            MissionButtonUI buttonInstance = Instantiate(missionButtonPrefab, missionButtonParent);

            bool unlocked = MissionProgress.IsUnlocked(mission);
            bool completed = MissionProgress.IsCompleted(mission.missionId);

            buttonInstance.Setup(mission, unlocked, completed, OnMissionClicked);
        }
    }

    private void ClearMissionButtons()
    {
        for (int i = missionButtonParent.childCount - 1; i >= 0; i--)
            Destroy(missionButtonParent.GetChild(i).gameObject);
    }

    private void OnMissionClicked(MissionData mission)
    {
        GameSceneLoader.Instance.LoadMissionScene(mission);
    }
}
//-----MissionTerminalUI.cs END-----