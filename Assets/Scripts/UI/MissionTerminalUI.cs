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

    [Header("Mission List")]
    [SerializeField] private MissionData[] missions;

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
        if (isOpen)
            return;

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

        foreach (MissionData mission in missions)
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
        if (GameSceneLoader.Instance != null)
            GameSceneLoader.Instance.LoadMissionScene(mission);
    }
}

//-----MissionTerminalUI.cs END-----