//-----TargetHealth.cs START-----
using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    [Header("Target")]
    [SerializeField] private string targetId;
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int scoreValue = 100;

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
}

//-----TargetHealth.cs END-----