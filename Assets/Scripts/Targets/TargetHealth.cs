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
    [SerializeField] private bool destroyOnDeath = true;

    private int currentHealth;
    private bool isDestroyed;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public string TargetId => targetId;
    public int ScoreValue => scoreValue;
    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (string.IsNullOrWhiteSpace(targetId))
            targetId = gameObject.name;
    }

    private void Start()
    {
        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterTarget(this);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDestroyed)
            return;

        currentHealth -= damageInfo.damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        AwardWeaponTypeXp(damageInfo, weaponTypeXpOnHit, "Target hit");

        Debug.Log($"{name} health: {currentHealth}/{maxHealth}");

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterTargetDamaged(this, damageInfo);

        if (currentHealth <= 0)
            DestroyTarget(damageInfo);
    }

    private void DestroyTarget(DamageInfo damageInfo)
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        AwardWeaponTypeXp(damageInfo, weaponTypeXpOnDestroyed, "Target destroyed");

        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterTargetDestroyed(this, damageInfo);

        if (destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
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