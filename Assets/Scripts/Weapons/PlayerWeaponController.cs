//-----PlayerWeaponController.cs START-----
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform fallbackMuzzlePoint;

    [Header("Input")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;

    private WeaponData currentWeapon;
    private GameObject currentViewModel;
    private Transform currentMuzzlePoint;
    private float nextFireTime;

    public WeaponData CurrentWeapon => currentWeapon;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (currentWeapon == null)
            return;

        if (Input.GetKey(fireKey))
            TryFire();
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("Tried to equip null weapon data.");
            return;
        }

        currentWeapon = weaponData;

        SpawnViewModel();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterWeaponEquipped(currentWeapon.weaponId, currentWeapon.weaponType);

        Debug.Log($"Equipped weapon: {currentWeapon.displayName}");
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + currentWeapon.fireRate;

        if (currentWeapon.fireMode == WeaponFireMode.Projectile)
            FireProjectile();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterShot(currentWeapon.weaponId, currentWeapon.weaponType);
    }

    private void FireProjectile()
    {
        if (currentWeapon.projectilePrefab == null)
        {
            Debug.LogWarning($"Weapon '{currentWeapon.displayName}' has no projectile prefab assigned.");
            return;
        }

        Transform spawnPoint = GetMuzzlePoint();

        GameObject projectileObject = Instantiate(
            currentWeapon.projectilePrefab,
            spawnPoint.position,
            playerCamera.transform.rotation);

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogWarning("Projectile prefab is missing Projectile script.");
            return;
        }

        projectile.Initialize(
            currentWeapon.damage,
            currentWeapon.weaponId,
            currentWeapon.weaponType,
            gameObject,
            currentWeapon.projectileSpeed,
            currentWeapon.projectileLifetime);
    }

    private Transform GetMuzzlePoint()
    {
        if (currentMuzzlePoint != null)
            return currentMuzzlePoint;

        if (fallbackMuzzlePoint != null)
            return fallbackMuzzlePoint;

        return playerCamera.transform;
    }

    private void SpawnViewModel()
    {
        currentMuzzlePoint = null;

        if (currentViewModel != null)
            Destroy(currentViewModel);

        if (currentWeapon.viewModelPrefab == null || weaponHolder == null)
            return;

        currentViewModel = Instantiate(currentWeapon.viewModelPrefab, weaponHolder);

        currentViewModel.transform.localPosition = currentWeapon.viewLocalPosition;
        currentViewModel.transform.localEulerAngles = currentWeapon.viewLocalEulerAngles;
        currentViewModel.transform.localScale = currentWeapon.viewLocalScale;

        Transform muzzle = currentViewModel.transform.Find("MuzzlePoint");

        if (muzzle != null)
            currentMuzzlePoint = muzzle;
        else
            Debug.LogWarning($"{currentWeapon.displayName} view model has no child named MuzzlePoint. Using fallback muzzle.");
    }
}
//-----PlayerWeaponController.cs END-----   