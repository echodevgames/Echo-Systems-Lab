//-----PickupSpawner.cs START-----

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PickupSpawnMode
{
    OnStart,
    OnTriggerEnter,
    Manual,
    Timed
}

public class PickupSpawner : MonoBehaviour
{
    [Header("Spawn Content")]
    [SerializeField] private GameObject[] pickupPrefabs;

    [Tooltip("Optional. Used when spawning a generic WeaponPickup prefab.")]
    [SerializeField] private WeaponPickupData[] weaponPickupDataOptions;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Mode")]
    [SerializeField] private PickupSpawnMode spawnMode = PickupSpawnMode.OnStart;

    [Tooltip("Only applies to OnStart and OnTriggerEnter modes.")]
    [SerializeField] private bool continuousSpawning = false;

    [Tooltip("Only applies to trigger mode.")]
    [SerializeField] private bool spawnOnlyOnce = true;

    [Header("Spawn Amounts")]
    [SerializeField] private int spawnsPerBurst = 1;

    [Tooltip("0 or less means unlimited.")]
    [SerializeField] private int maxTotalSpawns = 1;

    [Tooltip("0 or less means unlimited.")]
    [SerializeField] private int maxAliveSpawns = 1;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 0f;
    [SerializeField] private float spawnInterval = 5f;

    [Header("Randomization")]
    [SerializeField] private bool randomizePrefab = true;
    [SerializeField] private bool randomizeSpawnPoint = true;
    [SerializeField] private bool randomizeWeaponPickupData = true;

    [Header("Physics")]
    [SerializeField] private bool applySpawnImpulse = false;
    [SerializeField] private float spawnForwardImpulse = 0f;
    [SerializeField] private float spawnUpImpulse = 1.5f;
    [SerializeField] private float randomSideImpulse = 0.5f;

    [Header("Trigger Settings")]
    [SerializeField] private string requiredTag = "Player";

    [Header("Organization")]
    [SerializeField] private bool parentSpawnedObjects = false;

    private readonly List<GameObject> aliveSpawns = new List<GameObject>();

    private int totalSpawned;
    private bool hasTriggered;
    private Coroutine spawnRoutine;
    private Coroutine delayedBurstRoutine;

    private void Start()
    {
        if (spawnMode == PickupSpawnMode.OnStart)
        {
            if (continuousSpawning)
                StartSpawning();
            else
                SpawnBurstWithDelay();
        }

        if (spawnMode == PickupSpawnMode.Timed)
            StartSpawning();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spawnMode != PickupSpawnMode.OnTriggerEnter)
            return;

        if (spawnOnlyOnce && hasTriggered)
            return;

        if (!string.IsNullOrWhiteSpace(requiredTag) && !other.CompareTag(requiredTag))
            return;

        hasTriggered = true;

        if (continuousSpawning)
            StartSpawning();
        else
            SpawnBurstWithDelay();
    }

    public GameObject SpawnOne()
    {
        CleanupAliveList();

        if (!CanSpawn())
            return null;

        GameObject prefab = GetPrefabToSpawn();

        if (prefab == null)
        {
            Debug.LogWarning($"{name} cannot spawn because no pickup prefab is assigned.");
            return null;
        }

        Transform spawnPoint = GetSpawnPoint();

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        Transform parent = parentSpawnedObjects ? transform : null;

        GameObject spawnedObject = Instantiate(prefab, spawnPosition, spawnRotation, parent);

        ApplyWeaponPickupDataIfNeeded(spawnedObject);
        ApplyImpulseIfNeeded(spawnedObject);

        aliveSpawns.Add(spawnedObject);
        totalSpawned++;

        return spawnedObject;
    }

    public void SpawnBurst()
    {
        int amount = Mathf.Max(1, spawnsPerBurst);

        for (int i = 0; i < amount; i++)
        {
            if (!CanSpawn())
                return;

            SpawnOne();
        }
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null)
            return;

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        if (delayedBurstRoutine != null)
        {
            StopCoroutine(delayedBurstRoutine);
            delayedBurstRoutine = null;
        }
    }

    public void ResetSpawner()
    {
        StopSpawning();

        totalSpawned = 0;
        hasTriggered = false;

        CleanupAliveList();
    }

    private void SpawnBurstWithDelay()
    {
        if (delayedBurstRoutine != null)
            StopCoroutine(delayedBurstRoutine);

        delayedBurstRoutine = StartCoroutine(DelayedBurstRoutine());
    }

    private IEnumerator DelayedBurstRoutine()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        SpawnBurst();

        delayedBurstRoutine = null;
    }

    private IEnumerator SpawnRoutine()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            if (ReachedTotalSpawnLimit())
                break;

            SpawnBurst();

            if (ReachedTotalSpawnLimit())
                break;

            yield return new WaitForSeconds(Mathf.Max(0.01f, spawnInterval));
        }

        spawnRoutine = null;
    }

    private bool CanSpawn()
    {
        CleanupAliveList();

        if (ReachedTotalSpawnLimit())
            return false;

        if (ReachedAliveSpawnLimit())
            return false;

        return true;
    }

    private bool ReachedTotalSpawnLimit()
    {
        if (maxTotalSpawns <= 0)
            return false;

        return totalSpawned >= maxTotalSpawns;
    }

    private bool ReachedAliveSpawnLimit()
    {
        if (maxAliveSpawns <= 0)
            return false;

        return aliveSpawns.Count >= maxAliveSpawns;
    }

    private GameObject GetPrefabToSpawn()
    {
        if (pickupPrefabs == null || pickupPrefabs.Length == 0)
            return null;

        if (!randomizePrefab)
            return pickupPrefabs[0];

        for (int i = 0; i < 10; i++)
        {
            GameObject candidate = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];

            if (candidate != null)
                return candidate;
        }

        return pickupPrefabs[0];
    }

    private Transform GetSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return transform;

        if (!randomizeSpawnPoint)
            return spawnPoints[0] != null ? spawnPoints[0] : transform;

        for (int i = 0; i < 10; i++)
        {
            Transform candidate = spawnPoints[Random.Range(0, spawnPoints.Length)];

            if (candidate != null)
                return candidate;
        }

        return transform;
    }

    private WeaponPickupData GetWeaponPickupDataToSpawn()
    {
        if (weaponPickupDataOptions == null || weaponPickupDataOptions.Length == 0)
            return null;

        if (!randomizeWeaponPickupData)
            return weaponPickupDataOptions[0];

        for (int i = 0; i < 10; i++)
        {
            WeaponPickupData candidate = weaponPickupDataOptions[Random.Range(0, weaponPickupDataOptions.Length)];

            if (candidate != null)
                return candidate;
        }

        return weaponPickupDataOptions[0];
    }

    private void ApplyWeaponPickupDataIfNeeded(GameObject spawnedObject)
    {
        if (spawnedObject == null)
            return;

        WeaponPickup weaponPickup = spawnedObject.GetComponent<WeaponPickup>();

        if (weaponPickup == null)
            return;

        WeaponPickupData pickupData = GetWeaponPickupDataToSpawn();

        if (pickupData == null)
            return;

        weaponPickup.Initialize(pickupData);
    }

    private void ApplyImpulseIfNeeded(GameObject spawnedObject)
    {
        if (!applySpawnImpulse || spawnedObject == null)
            return;

        Rigidbody body = spawnedObject.GetComponent<Rigidbody>();

        if (body == null)
            return;

        Vector3 sideDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f));

        if (sideDirection.sqrMagnitude > 0.001f)
            sideDirection.Normalize();

        Vector3 force =
            transform.forward * spawnForwardImpulse +
            Vector3.up * spawnUpImpulse +
            sideDirection * randomSideImpulse;

        body.AddForce(force, ForceMode.Impulse);
    }

    private void CleanupAliveList()
    {
        for (int i = aliveSpawns.Count - 1; i >= 0; i--)
        {
            if (aliveSpawns[i] == null)
                aliveSpawns.RemoveAt(i);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 0.25f);

        if (spawnPoints == null)
            return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
                continue;

            Gizmos.DrawWireSphere(spawnPoints[i].position, 0.2f);
            Gizmos.DrawLine(transform.position, spawnPoints[i].position);
        }
    }
}

//-----PickupSpawner.cs END-----