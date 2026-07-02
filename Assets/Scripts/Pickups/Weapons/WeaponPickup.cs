//-----WeaponPickup.cs START-----

using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("Pickup Data")]
    [SerializeField] private WeaponPickupData pickupData;

    [Header("Runtime Weapon Override")]
    [SerializeField] private WeaponData weaponData;

    [Header("Visuals")]
    [SerializeField] private Transform visualRoot;

    [Header("Fallback Visuals")]
    [SerializeField] private bool useWeaponViewModelAsFallbackVisual = true;

    private GameObject spawnedVisual;

    private void Awake()
    {
        if (pickupData != null && weaponData == null)
            weaponData = pickupData.weaponData;

        RefreshVisual();
    }

    public void Initialize(WeaponData newWeaponData)
    {
        weaponData = newWeaponData;

        // Keep pickupData null for runtime dropped weapons.
        // The visual will now fall back to WeaponData visuals.
        pickupData = null;

        RefreshVisual();
    }

    public void Initialize(WeaponPickupData newPickupData)
    {
        pickupData = newPickupData;
        weaponData = pickupData != null ? pickupData.weaponData : null;

        RefreshVisual();
    }

    public string GetPromptText()
    {
        return $"Press E to pick up {GetWeaponDisplayName()}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"{name} has no WeaponData assigned.");
            return;
        }

        PlayerWeaponController weaponController = interactor.GetComponentInParent<PlayerWeaponController>();

        if (weaponController == null)
        {
            Debug.LogWarning($"{name} could not find PlayerWeaponController on interactor.");
            return;
        }

        string weaponId = GetWeaponId();

        if (!string.IsNullOrWhiteSpace(weaponId))
        {
            PlayerProgress.AddOwnedWeapon(weaponId);
            PlayerProgress.SetActiveWeapon(weaponId);
        }

        weaponController.EquipWeapon(weaponData);

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        bool shouldDestroy = pickupData == null || pickupData.destroyOnPickup;

        if (shouldDestroy)
            Destroy(gameObject);
    }

    private void RefreshVisual()
    {
        if (visualRoot == null)
            visualRoot = transform;

        ClearVisualRoot();

        GameObject visualPrefab = GetVisualPrefab();

        if (visualPrefab == null)
        {
            Debug.LogWarning($"{name} has no visual prefab for pickup weapon: {GetWeaponDisplayName()}");
            return;
        }

        spawnedVisual = Instantiate(visualPrefab, visualRoot);
        spawnedVisual.transform.localPosition = Vector3.zero;
        spawnedVisual.transform.localRotation = Quaternion.identity;
        spawnedVisual.transform.localScale = Vector3.one;
    }

    private void ClearVisualRoot()
    {
        spawnedVisual = null;

        if (visualRoot == null)
            return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
            Destroy(visualRoot.GetChild(i).gameObject);
    }

    private GameObject GetVisualPrefab()
    {
        if (pickupData != null && pickupData.worldVisualPrefab != null)
            return pickupData.worldVisualPrefab;

        if (weaponData == null)
            return null;

        // Temporary fallback:
        // Use the first-person view model as the dropped pickup visual.
        // Later we can replace this with a true world pickup mesh.
        if (useWeaponViewModelAsFallbackVisual && weaponData.viewModelPrefab != null)
            return weaponData.viewModelPrefab;

        return null;
    }

    private string GetWeaponDisplayName()
    {
        if (pickupData != null)
            return pickupData.GetDisplayName();

        if (weaponData != null)
        {
            if (!string.IsNullOrWhiteSpace(weaponData.hudDisplayName))
                return weaponData.hudDisplayName;

            if (!string.IsNullOrWhiteSpace(weaponData.displayName))
                return weaponData.displayName;

            if (!string.IsNullOrWhiteSpace(weaponData.weaponId))
                return weaponData.weaponId;
        }

        return "Weapon";
    }

    private string GetWeaponId()
    {
        if (weaponData == null)
            return string.Empty;

        return weaponData.weaponId;
    }
}

//-----WeaponPickup.cs END-----