//-----Projectile.cs START-----

using UnityEngine;

public enum ProjectileMissBehavior
{
    DestroyAfterLifetime,
    DropAfterFlightTime
}

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Hit Detection")]
    [SerializeField] private bool useSweepDetection = true;
    [SerializeField] private float sweepRadius = 0.04f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private bool destroyOnNonDamageableSolidHit = true;

    [Header("Miss Behavior")]
    [SerializeField] private ProjectileMissBehavior missBehavior = ProjectileMissBehavior.DestroyAfterLifetime;

    [Tooltip("Only used when Miss Behavior is Drop After Flight Time.")]
    [SerializeField] private float activeFlightTimeBeforeDrop = 1.25f;

    [Tooltip("Only used when Miss Behavior is Drop After Flight Time.")]
    [SerializeField] private float destroyAfterDropTime = 8f;

    [SerializeField] private float droppedVelocityMultiplier = 0.15f;
    [SerializeField] private bool makeColliderSolidWhenDropped = true;

    private int damage;
    private string weaponId;
    private string weaponType;
    private GameObject owner;
    private float lifeTime;
    private bool hasInitialized;
    private bool hasHit;
    private bool hasDropped;

    private Rigidbody rb;
    private Collider projectileCollider;
    private Vector3 previousPosition;
    private float activeFlightEndTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
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
        hasDropped = false;

        previousPosition = transform.position;
        activeFlightEndTime = Time.time + activeFlightTimeBeforeDrop;

        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;

        if (missBehavior == ProjectileMissBehavior.DestroyAfterLifetime)
        {
            Destroy(gameObject, lifeTime);
        }
        else
        {
            float totalLifetime = activeFlightTimeBeforeDrop + destroyAfterDropTime;
            Destroy(gameObject, totalLifetime);
        }
    }

    private void FixedUpdate()
    {
        if (!hasInitialized || hasHit)
            return;

        if (missBehavior == ProjectileMissBehavior.DropAfterFlightTime &&
            !hasDropped &&
            Time.time >= activeFlightEndTime)
        {
            DropProjectile();
            return;
        }

        if (!useSweepDetection || hasDropped)
            return;

        SweepForHits();
    }

    private void SweepForHits()
    {
        Vector3 currentPosition = transform.position;
        Vector3 travel = currentPosition - previousPosition;
        float distance = travel.magnitude;

        if (distance <= 0.0001f)
        {
            previousPosition = currentPosition;
            return;
        }

        Vector3 direction = travel / distance;

        RaycastHit[] hits = Physics.SphereCastAll(
            previousPosition,
            sweepRadius,
            direction,
            distance,
            hitMask,
            QueryTriggerInteraction.Collide);

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (ShouldIgnoreCollider(hit.collider))
                    continue;

                ProcessHit(hit.collider, hit.point);
                return;
            }
        }

        previousPosition = currentPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDropped)
            return;

        ProcessHit(other, transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.collider == null)
            return;

        if (hasDropped)
            return;

        Vector3 hitPoint = transform.position;

        if (collision.contactCount > 0)
            hitPoint = collision.GetContact(0).point;

        ProcessHit(collision.collider, hitPoint);
    }

    private void ProcessHit(Collider other, Vector3 hitPoint)
    {
        if (!hasInitialized || hasHit || hasDropped)
            return;

        if (ShouldIgnoreCollider(other))
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            damageable = other.GetComponentInChildren<IDamageable>();

        if (damageable == null)
        {
            if (other.isTrigger)
                return;

            if (destroyOnNonDamageableSolidHit)
            {
                hasHit = true;
                Destroy(gameObject);
            }

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
            hitPoint);

        damageable.TakeDamage(damageInfo);

        Destroy(gameObject);
    }

    private void DropProjectile()
    {
        hasDropped = true;

        // Important: once dropped, this projectile is harmless.
        hasHit = true;

        rb.useGravity = true;
        rb.linearVelocity *= droppedVelocityMultiplier;
        rb.angularVelocity = Random.insideUnitSphere * 8f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (projectileCollider != null && makeColliderSolidWhenDropped)
            projectileCollider.isTrigger = false;

        Debug.Log($"{name} missed and became a dropped projectile.");
    }

    private bool ShouldIgnoreCollider(Collider other)
    {
        if (other == null)
            return true;

        if (IsOwnerOrOwnerChild(other.gameObject))
            return true;

        if (IsOtherProjectile(other))
            return true;

        return false;
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