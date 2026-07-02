//-----TargetRangeHUD.cs START-----

using TMPro;
using UnityEngine;

public class TargetRangeHUD : MonoBehaviour
{
    [Header("Mission HUD Root")]
    [SerializeField] private GameObject missionHudRoot;

    [Header("Mission Text References")]
    [SerializeField] private TMP_Text missionTitleText;
    [SerializeField] private TMP_Text missionGoalText;
    [SerializeField] private TMP_Text missionTimerText;
    [SerializeField] private TMP_Text missionStatusText;

    [Header("Mission Feedback Text References")]
    [SerializeField] private TMP_Text missionScoreText;
    [SerializeField] private TMP_Text missionShotsText;
    [SerializeField] private TMP_Text missionHitsText;
    [SerializeField] private TMP_Text missionAccuracyText;
    [SerializeField] private TMP_Text missionWeaponText;

    [Header("Completion")]
    [SerializeField] private TMP_Text completionText;

    private TargetRangeMissionController missionController;

    private void Start()
    {
        missionController = TargetRangeMissionController.Instance;

        if (missionController == null)
            missionController = FindFirstObjectByType<TargetRangeMissionController>();

        if (missionController != null)
        {
            missionController.OnMissionStateChanged += RefreshMission;
            missionController.OnMissionCompleted += ShowMissionComplete;
            missionController.OnMissionFailed += ShowMissionFailed;
        }
        else
        {
            Debug.LogWarning("TargetRangeHUD could not find TargetRangeMissionController.");
        }

        if (completionText != null)
            completionText.gameObject.SetActive(false);

        RefreshMission();
    }

    private void OnDestroy()
    {
        if (missionController != null)
        {
            missionController.OnMissionStateChanged -= RefreshMission;
            missionController.OnMissionCompleted -= ShowMissionComplete;
            missionController.OnMissionFailed -= ShowMissionFailed;
        }
    }

    private void RefreshMission()
    {
        if (missionController == null)
        {
            SetMissionHudVisible(false);
            return;
        }

        TargetRangeMissionData mission = missionController.ActiveMission;

        if (mission == null)
        {
            SetMissionHudVisible(false);
            ClearText();
            return;
        }

        SetMissionHudVisible(true);

        if (missionTitleText != null)
            missionTitleText.text = mission.displayName;

        if (missionGoalText != null)
            missionGoalText.text =
                $"Targets: {missionController.DestroyedTargetCount} / {missionController.RequiredDestroyedTargets}";

        if (missionTimerText != null)
        {
            float displayTime = missionController.IsMissionRunning
                ? missionController.RemainingTime
                : missionController.TimeLimitSeconds;

            missionTimerText.text = $"Time: {displayTime:0.0}";
        }

        if (missionScoreText != null)
            missionScoreText.text = $"Score: {missionController.MissionScore}";

        if (missionShotsText != null)
            missionShotsText.text = $"Shots: {missionController.ShotsFired}";

        if (missionHitsText != null)
            missionHitsText.text = $"Hits: {missionController.HitsLanded}";

        if (missionAccuracyText != null)
            missionAccuracyText.text = $"Accuracy: {missionController.AccuracyPercent:0.0}%";

        if (missionWeaponText != null)
        {
            string weaponName = string.IsNullOrWhiteSpace(missionController.CurrentWeaponId)
                ? "No Weapon"
                : missionController.CurrentWeaponId;

            missionWeaponText.text = $"Weapon: {weaponName}";
        }

        RefreshStatusText();
    }

    private void RefreshStatusText()
    {
        if (missionStatusText == null || missionController == null)
            return;

        switch (missionController.MissionState)
        {
            case TargetRangeMissionRuntimeState.Armed:
                missionStatusText.text = "PICK UP WEAPON TO BEGIN";
                break;

            case TargetRangeMissionRuntimeState.Running:
                missionStatusText.text = "MISSION ACTIVE";
                break;

            case TargetRangeMissionRuntimeState.Completed:
                missionStatusText.text = "MISSION COMPLETE";
                break;

            case TargetRangeMissionRuntimeState.Failed:
                missionStatusText.text = "MISSION FAILED";
                break;

            default:
                missionStatusText.text = "";
                break;
        }
    }

    private void ShowMissionComplete()
    {
        if (completionText != null)
        {
            completionText.gameObject.SetActive(true);
            completionText.text = "MISSION COMPLETE";
        }

        RefreshMission();
    }

    private void ShowMissionFailed()
    {
        if (completionText != null)
        {
            completionText.gameObject.SetActive(true);
            completionText.text = "MISSION FAILED";
        }

        RefreshMission();
    }

    private void SetMissionHudVisible(bool visible)
    {
        if (missionHudRoot != null)
            missionHudRoot.SetActive(visible);
    }

    private void ClearText()
    {
        if (missionTitleText != null)
            missionTitleText.text = "";

        if (missionGoalText != null)
            missionGoalText.text = "";

        if (missionTimerText != null)
            missionTimerText.text = "";

        if (missionStatusText != null)
            missionStatusText.text = "";

        if (missionScoreText != null)
            missionScoreText.text = "";

        if (missionShotsText != null)
            missionShotsText.text = "";

        if (missionHitsText != null)
            missionHitsText.text = "";

        if (missionAccuracyText != null)
            missionAccuracyText.text = "";

        if (missionWeaponText != null)
            missionWeaponText.text = "";
    }
}

//-----TargetRangeHUD.cs END-----