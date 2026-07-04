//-----TargetHealth.cs START-----

using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    [Header("Target")]
    [SerializeField] private string targetId;
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int scoreValue = 100;

    [Header("Progression")]
    [SerializeField] private int weaponTypeXpOnHit = 10;
    [SerializeField] private int weaponTypeXpOnDestroyed = 25;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private Vector3 destroyEffectOffset;
    [SerializeField] private Vector3 destroyEffectEulerOffset;
    [SerializeField] private bool parentDestroyEffectToTarget = false;
    [SerializeField] private bool destroyOnDeath = true;

    private int currentHealth;
    private bool isDestroyed;

    private TargetRangeMissionTarget missionTarget;
    private GameObject spawnedDestroyEffect;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public string TargetId => targetId;
    public int ScoreValue => scoreValue;
    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(targetId))
            targetId = gameObject.name;

        ResetTargetHealthOnly();
    }

    public void SetMissionTarget(TargetRangeMissionTarget newMissionTarget)
    {
        missionTarget = newMissionTarget;

        if (missionTarget != null)
            Debug.Log($"{name} linked to mission target: {missionTarget.name}");
    }

    public void ClearMissionTarget(TargetRangeMissionTarget targetToClear)
    {
        if (missionTarget == targetToClear)
            missionTarget = null;
    }

    public void ResetTarget()
    {
        ClearSpawnedDestroyEffect();
        ResetTargetHealthOnly();

        Debug.Log($"{name} reset: {currentHealth}/{maxHealth}");
    }

    public void ClearSpawnedDestroyEffect()
    {
        if (spawnedDestroyEffect != null)
            Destroy(spawnedDestroyEffect);

        spawnedDestroyEffect = null;
    }

    private void ResetTargetHealthOnly()
    {
        currentHealth = maxHealth;
        isDestroyed = false;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDestroyed)
        {
            Debug.Log($"{name} ignored damage because it is already destroyed.");
            return;
        }

        currentHealth -= damageInfo.damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        AwardWeaponTypeXp(damageInfo, weaponTypeXpOnHit, "Target hit");

        Debug.Log($"{name} took {damageInfo.damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            DestroyTarget(damageInfo);
    }

    private void DestroyTarget(DamageInfo damageInfo)
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        AwardWeaponTypeXp(damageInfo, weaponTypeXpOnDestroyed, "Target destroyed");

        SpawnDestroyEffect();

        TargetRangeMissionTarget foundMissionTarget = missionTarget;

        if (foundMissionTarget == null)
            foundMissionTarget = GetComponent<TargetRangeMissionTarget>();

        if (foundMissionTarget == null)
            foundMissionTarget = GetComponentInParent<TargetRangeMissionTarget>();

        if (foundMissionTarget == null)
            foundMissionTarget = GetComponentInChildren<TargetRangeMissionTarget>(true);

        if (foundMissionTarget != null)
        {
            Debug.Log($"{name} destroyed through mission target route: {foundMissionTarget.name}");
            missionTarget = foundMissionTarget;
            missionTarget.NotifyDestroyed(this, damageInfo);
            return;
        }

        Debug.LogWarning($"{name} destroyed through non-mission fallback route. This target is not connected to TargetRangeTargetGroup.");

        if (destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void SpawnDestroyEffect()
    {
        if (destroyEffect == null)
            return;

        ClearSpawnedDestroyEffect();

        Vector3 spawnPosition = transform.position + transform.TransformDirection(destroyEffectOffset);
        Quaternion spawnRotation = transform.rotation * Quaternion.Euler(destroyEffectEulerOffset);

        Transform effectParent = parentDestroyEffectToTarget ? transform : null;

        spawnedDestroyEffect = Instantiate(
            destroyEffect,
            spawnPosition,
            spawnRotation,
            effectParent);
    }

    private void AwardWeaponTypeXp(DamageInfo damageInfo, int amount, string reason)
    {
        if (amount <= 0)
            return;

        if (string.IsNullOrWhiteSpace(damageInfo.weaponType))
            return;

        PlayerProgress.AddWeaponTypeXp(damageInfo.weaponType, amount);

        Debug.Log($"{reason}: +{amount} {damageInfo.weaponType} XP. Total: {PlayerProgress.GetWeaponTypeXp(damageInfo.weaponType)}");
    }
}

//-----TargetHealth.cs END-----