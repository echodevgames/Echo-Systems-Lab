//-----PickupSpawnerTriggerZone.cs START-----

using UnityEngine;

public enum PickupSpawnerTriggerAction
{
    SpawnOne,
    SpawnBurst,
    StartSpawning,
    StopSpawning,
    ResetSpawner
}

public class PickupSpawnerTriggerZone : MonoBehaviour
{
    [Header("Target Spawner")]
    [SerializeField] private PickupSpawner targetSpawner;

    [Header("Trigger Action")]
    [SerializeField] private PickupSpawnerTriggerAction triggerAction = PickupSpawnerTriggerAction.SpawnOne;

    [Header("Trigger Rules")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (!string.IsNullOrWhiteSpace(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (targetSpawner == null)
        {
            Debug.LogWarning($"{name} has no target PickupSpawner assigned.");
            return;
        }

        hasTriggered = true;
        FireTriggerAction();
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    private void FireTriggerAction()
    {
        switch (triggerAction)
        {
            case PickupSpawnerTriggerAction.SpawnOne:
                targetSpawner.SpawnOne();
                break;

            case PickupSpawnerTriggerAction.SpawnBurst:
                targetSpawner.SpawnBurst();
                break;

            case PickupSpawnerTriggerAction.StartSpawning:
                targetSpawner.StartSpawning();
                break;

            case PickupSpawnerTriggerAction.StopSpawning:
                targetSpawner.StopSpawning();
                break;

            case PickupSpawnerTriggerAction.ResetSpawner:
                targetSpawner.ResetSpawner();
                break;
        }
    }
}

//-----PickupSpawnerTriggerZone.cs END-----