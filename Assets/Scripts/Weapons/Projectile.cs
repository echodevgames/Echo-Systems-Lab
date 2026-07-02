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
    private bool hasHit;

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
        hasHit = false;

        rb.linearVelocity = transform.forward * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasInitialized || hasHit)
            return;

        if (IsOwnerOrOwnerChild(other.gameObject))
            return;

        if (IsOtherProjectile(other))
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            damageable = other.GetComponentInChildren<IDamageable>();

        if (damageable == null)
        {
            Destroy(gameObject);
            return;
        }

        hasHit = true;

        TargetRangeMissionController missionController = TargetRangeMissionController.Instance;

        if (missionController != null)
            missionController.RegisterMissionHit(weaponId, weaponType);

        DamageInfo damageInfo = new DamageInfo(
            damage,
            weaponId,
            weaponType,
            owner,
            transform.position);

        damageable.TakeDamage(damageInfo);

        Destroy(gameObject);
    }

    private bool IsOwnerOrOwnerChild(GameObject otherObject)
    {
        if (owner == null || otherObject == null)
            return false;

        if (otherObject == owner)
            return true;

        return otherObject.transform.IsChildOf(owner.transform);
    }

    private bool IsOtherProjectile(Collider other)
    {
        if (other == null)
            return false;

        return other.GetComponentInParent<Projectile>() != null;
    }
}

//-----Projectile.cs END-----