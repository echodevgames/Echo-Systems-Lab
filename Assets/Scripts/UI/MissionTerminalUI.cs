//-----MissionTerminalUI.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionTerminalUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject terminalRoot;
    [SerializeField] private TMP_Text terminalTitleText;
    [SerializeField] private Transform missionButtonParent;
    [SerializeField] private MissionButtonUI missionButtonPrefab;
    [SerializeField] private Button closeButton;

    [Header("Default Terminal Data")]
    [SerializeField] private MissionTerminalData defaultTerminalData;

    private MissionTerminalData activeTerminalData;

    private SimpleFirstPersonController activePlayerController;
    private PlayerInteractor activePlayerInteractor;
    private PlayerWeaponController activeWeaponController;
    private PlayerWeaponLoadoutController activeWeaponLoadoutController;
    private PlayerInputReader activeInputReader;

    private bool isOpen;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (terminalRoot != null)
            terminalRoot.SetActive(false);
    }

    public void Open(PlayerInteractor interactor)
    {
        Open(interactor, defaultTerminalData);
    }

    public void Open(PlayerInteractor interactor, MissionTerminalData terminalData)
    {
        if (isOpen)
            return;

        activeTerminalData = terminalData != null ? terminalData : defaultTerminalData;

        activePlayerInteractor = interactor;

        if (activePlayerInteractor != null)
        {
            activePlayerController = activePlayerInteractor.GetComponent<SimpleFirstPersonController>();
            activeWeaponController = activePlayerInteractor.GetComponent<PlayerWeaponController>();
            activeWeaponLoadoutController = activePlayerInteractor.GetComponent<PlayerWeaponLoadoutController>();
            activeInputReader = activePlayerInteractor.GetComponent<PlayerInputReader>();
        }

        if (activePlayerController == null)
            activePlayerController = FindFirstObjectByType<SimpleFirstPersonController>();

        PopulateMissionList();

        if (terminalRoot != null)
            terminalRoot.SetActive(true);

        if (activeInputReader != null)
            activeInputReader.SetGameplayInputEnabled(false);

        if (activePlayerController != null)
        {
            activePlayerController.SetInputEnabled(false);
            activePlayerController.SetCursorLocked(false);
        }

        if (activePlayerInteractor != null)
            activePlayerInteractor.SetInteractionEnabled(false);

        if (activeWeaponController != null)
            activeWeaponController.SetInputEnabled(false);

        if (activeWeaponLoadoutController != null)
            activeWeaponLoadoutController.SetInputEnabled(false);

        isOpen = true;
    }

    public void Close()
    {
        if (terminalRoot != null)
            terminalRoot.SetActive(false);

        if (activeInputReader != null)
            activeInputReader.SetGameplayInputEnabled(true);

        if (activePlayerController != null)
        {
            activePlayerController.SetInputEnabled(true);
            activePlayerController.SetCursorLocked(true);
        }

        if (activePlayerInteractor != null)
            activePlayerInteractor.SetInteractionEnabled(true);

        if (activeWeaponController != null)
            activeWeaponController.SetInputEnabled(true);

        if (activeWeaponLoadoutController != null)
            activeWeaponLoadoutController.SetInputEnabled(true);

        activeTerminalData = null;
        activePlayerController = null;
        activePlayerInteractor = null;
        activeWeaponController = null;
        activeWeaponLoadoutController = null;
        activeInputReader = null;

        isOpen = false;
    }

    private void PopulateMissionList()
    {
        ClearMissionButtons();

        if (terminalTitleText != null)
        {
            terminalTitleText.text = activeTerminalData != null
                ? activeTerminalData.displayName
                : "Mission Terminal";
        }

        if (activeTerminalData == null)
        {
            Debug.LogWarning($"{name} has no active MissionTerminalData.");
            return;
        }

        if (activeTerminalData.missions == null)
            return;

        foreach (MissionData mission in activeTerminalData.missions)
        {
            if (mission == null)
                continue;

            MissionButtonUI buttonInstance = Instantiate(missionButtonPrefab, missionButtonParent);

            bool unlocked = MissionProgress.IsUnlocked(mission);
            bool completed = MissionProgress.IsCompleted(mission.missionId);

            buttonInstance.Setup(mission, unlocked, completed, OnMissionClicked);
        }

        if (missionButtonParent is RectTransform rectTransform)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void ClearMissionButtons()
    {
        if (missionButtonParent == null)
            return;

        for (int i = missionButtonParent.childCount - 1; i >= 0; i--)
            Destroy(missionButtonParent.GetChild(i).gameObject);
    }

    private void OnMissionClicked(MissionData mission)
    {
        if (mission == null)
            return;

        if (mission.executionMode == MissionExecutionMode.LoadScene)
        {
            if (GameSceneLoader.Instance != null)
            {
                GameSceneLoader.Instance.LoadMissionScene(mission);
                return;
            }

            Debug.LogWarning("No GameSceneLoader found.");
            return;
        }

        if (mission.executionMode == MissionExecutionMode.StartInCurrentScene)
        {
            StartInSceneMission(mission);
            return;
        }
    }

    private void StartInSceneMission(MissionData mission)
    {
        TargetRangeMissionData targetRangeMission = mission as TargetRangeMissionData;

        if (targetRangeMission == null)
        {
            Debug.LogWarning($"Mission '{mission.displayName}' is set to StartInCurrentScene but is not a TargetRangeMissionData.");
            return;
        }

        TargetRangeMissionController controller = TargetRangeMissionController.Instance;

        if (controller == null)
            controller = FindFirstObjectByType<TargetRangeMissionController>();

        if (controller == null)
        {
            Debug.LogWarning("No TargetRangeMissionController found in scene.");
            return;
        }

        controller.StartMission(targetRangeMission);

        Close();
    }
}

//-----MissionTerminalUI.cs END-----