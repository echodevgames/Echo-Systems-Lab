//-----TargetRangeTracker.cs START-----

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TargetRangeTracker : MonoBehaviour
{
    public static TargetRangeTracker Instance { get; private set; }

    [Header("Mission Completion")]
    [SerializeField] private string missionIdToComplete = "CombatTrial";
    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private KeyCode returnToHubKey = KeyCode.R;

    [Header("State")]
    [SerializeField] private int totalTargets;
    [SerializeField] private int destroyedTargets;
    [SerializeField] private int score;
    [SerializeField] private int shotsFired;
    [SerializeField] private int hitsLanded;

    private readonly List<TargetHealth> registeredTargets = new List<TargetHealth>();
    private readonly Dictionary<string, int> weaponXp = new Dictionary<string, int>();

    private bool trialCompleted;

    public int TotalTargets => totalTargets;
    public int DestroyedTargets => destroyedTargets;
    public int Score => score;
    public int ShotsFired => shotsFired;
    public int HitsLanded => hitsLanded;
    public bool TrialCompleted => trialCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!trialCompleted)
            return;

        if (Input.GetKeyDown(returnToHubKey))
            ReturnToHub();
    }

    public void RegisterTarget(TargetHealth target)
    {
        if (target == null)
            return;

        if (registeredTargets.Contains(target))
            return;

        registeredTargets.Add(target);
        totalTargets = registeredTargets.Count;
    }

    public void RegisterWeaponEquipped(string weaponId, string weaponType)
    {
        EnsureWeaponXpEntry(weaponId);

        Debug.Log($"Weapon equipped: {weaponId} ({weaponType})");
    }

    public void RegisterShot(string weaponId, string weaponType)
    {
        shotsFired++;

        Debug.Log($"Shot fired with {weaponId}. Total shots: {shotsFired}");
    }

    public void RegisterHit(string weaponId, string weaponType)
    {
        hitsLanded++;

        AddWeaponXp(weaponId, 1);

        Debug.Log($"Hit landed with {weaponId}. Total hits: {hitsLanded}");
    }

    public void RegisterTargetDamaged(TargetHealth target, DamageInfo damageInfo)
    {
        Debug.Log($"Target damaged: {target.name} for {damageInfo.damageAmount}");
    }

    public void RegisterTargetDestroyed(TargetHealth target, DamageInfo damageInfo)
    {
        destroyedTargets++;
        score += target.ScoreValue;

        AddWeaponXp(damageInfo.weaponId, 5);

        Debug.Log($"Target destroyed: {target.name}. Score: {score}");

        CheckTrialCompletion();
    }

    private void CheckTrialCompletion()
    {
        if (trialCompleted)
            return;

        if (totalTargets <= 0)
            return;

        if (destroyedTargets < totalTargets)
            return;

        CompleteTrial();
    }

    private void CompleteTrial()
    {
        trialCompleted = true;

        MissionProgress.MarkCompleted(missionIdToComplete);

        Debug.Log("Target Range Trial completed.");
        Debug.Log($"Final Score: {score}");
        Debug.Log($"Accuracy: {GetAccuracyPercent():0.0}%");
        Debug.Log("Press R to return to Hub.");
    }

    private float GetAccuracyPercent()
    {
        if (shotsFired <= 0)
            return 0f;

        return (float)hitsLanded / shotsFired * 100f;
    }

    private void AddWeaponXp(string weaponId, int amount)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        EnsureWeaponXpEntry(weaponId);

        weaponXp[weaponId] += amount;

        Debug.Log($"Weapon XP: {weaponId} = {weaponXp[weaponId]}");
    }

    private void EnsureWeaponXpEntry(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        if (!weaponXp.ContainsKey(weaponId))
            weaponXp.Add(weaponId, 0);
    }

    private void ReturnToHub()
    {
        if (GameSceneLoader.Instance != null)
        {
            GameSceneLoader.Instance.LoadHub();
            return;
        }

        SceneManager.LoadScene(hubSceneName);
    }
}

//-----TargetRangeTracker.cs END-----