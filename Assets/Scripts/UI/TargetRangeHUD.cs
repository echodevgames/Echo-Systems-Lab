using TMPro;
using UnityEngine;

public class TargetRangeHUD : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private TMP_Text targetsText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text shotsText;
    [SerializeField] private TMP_Text hitsText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text weaponXpText;
    [SerializeField] private TMP_Text completionText;

    private TargetRangeTracker tracker;

    private void Start()
    {
        tracker = TargetRangeTracker.Instance;

        if (tracker == null)
        {
            Debug.LogWarning("TargetRangeHUD could not find TargetRangeTracker.");
            return;
        }

        tracker.OnStatsChanged += Refresh;
        tracker.OnTrialCompleted += ShowCompletion;

        if (completionText != null)
            completionText.gameObject.SetActive(false);

        Refresh();
    }

    private void OnDestroy()
    {
        if (tracker == null)
            return;

        tracker.OnStatsChanged -= Refresh;
        tracker.OnTrialCompleted -= ShowCompletion;
    }

    private void Refresh()
    {
        if (tracker == null)
            return;

        string weaponName = string.IsNullOrWhiteSpace(tracker.CurrentWeaponId)
            ? "None"
            : tracker.CurrentWeaponId;

        if (weaponText != null)
            weaponText.text = $"Weapon: {weaponName}";

        if (targetsText != null)
            targetsText.text = $"Targets: {tracker.TargetsRemaining} / {tracker.TotalTargets}";

        if (scoreText != null)
            scoreText.text = $"Score: {tracker.Score}";

        if (shotsText != null)
            shotsText.text = $"Shots: {tracker.ShotsFired}";

        if (hitsText != null)
            hitsText.text = $"Hits: {tracker.HitsLanded}";

        if (accuracyText != null)
            accuracyText.text = $"Accuracy: {tracker.AccuracyPercent:0.0}%";

        if (weaponXpText != null)
        {
            int xp = tracker.GetWeaponXp(tracker.CurrentWeaponId);
            weaponXpText.text = $"Weapon XP: {xp}";
        }
    }

    private void ShowCompletion()
    {
        if (completionText != null)
        {
            completionText.gameObject.SetActive(true);
            completionText.text = "TRIAL COMPLETE\nPress R to return to Hub";
        }

        Refresh();
    }
}