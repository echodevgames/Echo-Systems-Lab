//-----Projectile.cs START-----
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private int damage;
    private string weaponId;
    private string weaponType;
    private GameObject owner;
    private float lifeTime;
    private bool hasInitialized;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(
        int damageAmount,
        string sourceWeaponId,
        string sourceWeaponType,
        GameObject sourceOwner,
        float speed,
        float projectileLifeTime)
    {
        damage = damageAmount;
        weaponId = sourceWeaponId;
        weaponType = sourceWeaponType;
        owner = sourceOwner;
        lifeTime = projectileLifeTime;

        hasInitialized = true;

        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasInitialized)
            return;

        if (other.gameObject == owner)
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            DamageInfo damageInfo = new DamageInfo(
                damage,
                weaponId,
                weaponType,
                owner,
                transform.position);

            damageable.TakeDamage(damageInfo);

            TargetRangeTracker tracker = TargetRangeTracker.Instance;
            if (tracker != null)
               tracker.RegisterHit(weaponId, weaponType);
        }

        Destroy(gameObject);
    }
}

//-----Projectile.cs END-----
