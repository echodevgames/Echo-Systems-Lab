//-----PlayerWeaponController.cs START-----
using System.Collections;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform fallbackMuzzlePoint;

    [Header("Loadout")]
    [SerializeField] private bool autoEquipSavedWeapon = true;

    [Header("Input")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;

    [Header("Projectile Spawn")]
    [SerializeField] private float muzzleForwardOffset = 0.15f;

    private WeaponData currentWeapon;
    private GameObject currentViewModel;
    private Transform currentMuzzlePoint;

    private float nextFireTime;
    private bool inputEnabled = true;

    private int currentClipAmmo;
    private bool isReloading;
    private bool reloadPromptShown;

    public WeaponData CurrentWeapon => currentWeapon;
    public int CurrentClipAmmo => currentClipAmmo;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        if (autoEquipSavedWeapon)
            TryEquipSavedWeapon();
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (currentWeapon == null)
            return;

        if (Input.GetKeyDown(reloadKey))
            TryReload();

        if (isReloading)
            return;

        if (currentWeapon.isAutomatic)
        {
            if (Input.GetKey(fireKey))
                TryFire();
        }
        else
        {
            if (Input.GetKeyDown(fireKey))
                TryFire();
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("Tried to equip null weapon data.");
            return;
        }

        currentWeapon = weaponData;

        PlayerProgress.SetActiveWeapon(currentWeapon.weaponId);

        SpawnViewModel();
        FillClip();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterWeaponEquipped(currentWeapon.weaponId, currentWeapon.weaponType);

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log($"Equipped weapon: {currentWeapon.displayName}");
        Debug.Log($"{currentWeapon.displayName} ammo: {currentClipAmmo}/{currentWeapon.clipSize}");
    }

    private void TryEquipSavedWeapon()
    {
        if (SaveManager.Instance == null)
            return;

        string activeWeaponId = PlayerProgress.ActiveWeaponId;

        if (string.IsNullOrWhiteSpace(activeWeaponId))
            return;

        WeaponDatabase database = SaveManager.Instance.WeaponDatabase;

        if (database == null)
        {
            Debug.LogWarning("No WeaponDatabase assigned to SaveManager.");
            return;
        }

        WeaponData savedWeapon = database.GetWeaponById(activeWeaponId);

        if (savedWeapon == null)
        {
            Debug.LogWarning($"Could not find saved weapon with id: {activeWeaponId}");
            return;
        }

        currentWeapon = savedWeapon;

        SpawnViewModel();
        FillClip();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterWeaponEquipped(currentWeapon.weaponId, currentWeapon.weaponType);

        Debug.Log($"Auto-equipped saved weapon: {currentWeapon.displayName}");
        Debug.Log($"{currentWeapon.displayName} ammo: {currentClipAmmo}/{currentWeapon.clipSize}");
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime)
            return;

        if (currentWeapon.defaultAmmo == null)
        {
            Debug.LogWarning($"Weapon '{currentWeapon.displayName}' has no default ammo assigned.");
            return;
        }

        if (currentClipAmmo <= 0)
        {
            ShowReloadPrompt();
            return;
        }

        nextFireTime = Time.time + currentWeapon.fireRate;

        bool firedSuccessfully = false;

        if (currentWeapon.fireMode == WeaponFireMode.Projectile)
            firedSuccessfully = FireProjectilePattern();

        if (!firedSuccessfully)
            return;

        currentClipAmmo--;
        reloadPromptShown = false;

        AwardWeaponUseXp(currentWeapon.defaultAmmo);

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterShot(currentWeapon.weaponId, currentWeapon.weaponType);

        Debug.Log($"{currentWeapon.displayName} ammo: {currentClipAmmo}/{currentWeapon.clipSize}");

        if (currentClipAmmo <= 0)
            ShowReloadPrompt();
    }

    private bool FireProjectilePattern()
    {
        AmmoData ammo = currentWeapon.defaultAmmo;

        if (ammo == null)
        {
            Debug.LogWarning($"Weapon '{currentWeapon.displayName}' has no default ammo assigned.");
            return false;
        }

        if (ammo.projectilePrefab == null)
        {
            Debug.LogWarning($"Ammo '{ammo.displayName}' has no projectile prefab assigned.");
            return false;
        }

        int projectileCount = Mathf.Max(1, currentWeapon.projectilesPerShot);

        Debug.Log($"Firing {currentWeapon.displayName}: {projectileCount} projectile(s) using {ammo.displayName}");

        for (int i = 0; i < projectileCount; i++)
            FireSingleProjectile(ammo);

        return true;
    }

    private void FireSingleProjectile(AmmoData ammo)
    {
        Transform spawnPoint = GetMuzzlePoint();

        Quaternion fireRotation = GetFireRotationWithSpread();

        Vector3 spawnPosition = spawnPoint.position + fireRotation * Vector3.forward * muzzleForwardOffset;

        GameObject projectileObject = Instantiate(
            ammo.projectilePrefab,
            spawnPosition,
            fireRotation);

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogWarning("Projectile prefab is missing Projectile script.");
            Destroy(projectileObject);
            return;
        }

        projectile.Initialize(
            ammo.damage,
            currentWeapon.weaponId,
            currentWeapon.weaponType,
            gameObject,
            ammo.projectileSpeed,
            ammo.projectileLifetime);
    }

    private Quaternion GetFireRotationWithSpread()
    {
        Quaternion baseRotation = playerCamera.transform.rotation;

        if (currentWeapon.spreadAngle <= 0f)
            return baseRotation;

        float randomYaw = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle);
        float randomPitch = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle);

        return baseRotation * Quaternion.Euler(randomPitch, randomYaw, 0f);
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

    private void FillClip()
    {
        if (currentWeapon == null)
        {
            currentClipAmmo = 0;
            return;
        }

        currentClipAmmo = Mathf.Max(1, currentWeapon.clipSize);
        isReloading = false;
        reloadPromptShown = false;
    }

    private void TryReload()
    {
        if (currentWeapon == null)
            return;

        if (isReloading)
            return;

        if (currentClipAmmo >= currentWeapon.clipSize)
        {
            Debug.Log($"{currentWeapon.displayName} clip is already full.");
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        reloadPromptShown = false;

        Debug.Log($"Reloading {currentWeapon.displayName}...");

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentClipAmmo = Mathf.Max(1, currentWeapon.clipSize);
        isReloading = false;

        Debug.Log($"{currentWeapon.displayName} reloaded: {currentClipAmmo}/{currentWeapon.clipSize}");
    }

    private void ShowReloadPrompt()
    {
        if (reloadPromptShown)
            return;

        reloadPromptShown = true;

        Debug.Log($"{currentWeapon.displayName} is empty. Press R to reload.");
    }

    private void AwardWeaponUseXp(AmmoData ammo)
    {
        if (currentWeapon == null)
            return;

        int weaponXp = Mathf.Max(0, currentWeapon.xpPerUse);
        int ammoXp = ammo != null ? Mathf.Max(0, ammo.xpPerUse) : 0;

        int totalXp = weaponXp + ammoXp;

        PlayerProgress.AddWeaponTypeXp(currentWeapon.weaponType, totalXp);

        Debug.Log($"{currentWeapon.weaponType} XP gained: {totalXp}. Total: {PlayerProgress.GetWeaponTypeXp(currentWeapon.weaponType)}");
    }
}
//-----PlayerWeaponController.cs END-----