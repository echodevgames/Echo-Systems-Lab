//-----PlayerWeaponController.cs START-----

using System.Collections;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform fallbackMuzzlePoint;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerAmmoInventory ammoInventory;

    [Header("Loadout")]
    [SerializeField] private bool autoEquipSavedWeapon = true;

    [Header("Projectile Spawn")]
    [SerializeField] private float muzzleForwardOffset = 0.15f;

    private WeaponData currentWeapon;
    private GameObject currentViewModel;
    private Transform currentMuzzlePoint;

    private float nextFireTime;
    private bool inputEnabled = true;

    private int currentAmmoInClip;
    private bool isReloading;
    private bool reloadPromptShown;

    public event System.Action OnWeaponAmmoChanged;
    public event System.Action<WeaponData> OnWeaponChanged;

    public WeaponData CurrentWeapon => currentWeapon;
    public int CurrentAmmoInClip => currentAmmoInClip;
    public int CurrentClipAmmo => currentAmmoInClip;
    public int CurrentClipSize => currentWeapon != null ? currentWeapon.clipSize : 0;
    public bool IsReloading => isReloading;

    public bool UsesInfiniteReserveAmmo =>
        currentWeapon != null && currentWeapon.infiniteReserveAmmo;



    public int CurrentReserveAmmo
    {
        get
        {
            if (ammoInventory == null || currentWeapon == null || currentWeapon.defaultAmmo == null)
                return 0;

            return ammoInventory.GetReserveAmmo(currentWeapon.defaultAmmo);
        }
    }

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();

        if (ammoInventory == null)
            ammoInventory = GetComponent<PlayerAmmoInventory>();
    }

    private void OnEnable()
    {
        if (ammoInventory != null)
            ammoInventory.OnAmmoChanged += HandleAmmoInventoryChanged;
    }

    private void OnDisable()
    {
        if (ammoInventory != null)
            ammoInventory.OnAmmoChanged -= HandleAmmoInventoryChanged;
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

        if (inputReader == null)
            return;

        if (currentWeapon == null)
            return;

        if (inputReader.ReloadPressed)
            TryReload();

        if (isReloading)
            return;

        if (currentWeapon.isAutomatic)
        {
            if (inputReader.FireHeld)
                TryFire();
        }
        else
        {
            if (inputReader.FirePressed)
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

        StopAllCoroutines();

        currentWeapon = weaponData;
        isReloading = false;
        reloadPromptShown = false;

        PlayerProgress.SetActiveWeapon(currentWeapon.weaponId);

        SpawnViewModel();
        FillClip();

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterWeaponEquipped(currentWeapon.weaponId, currentWeapon.weaponType);

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log($"Equipped weapon: {currentWeapon.displayName}");
        Debug.Log($"{currentWeapon.displayName} ammo: {currentAmmoInClip}/{currentWeapon.clipSize}");

        OnWeaponAmmoChanged?.Invoke();
        OnWeaponChanged?.Invoke(currentWeapon);

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

        EquipWeapon(savedWeapon);
        Debug.Log($"Auto-equipped saved weapon: {savedWeapon.displayName}");

        OnWeaponChanged?.Invoke(currentWeapon);
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

        if (currentAmmoInClip <= 0)
        {
            ShowReloadPrompt();
            OnWeaponAmmoChanged?.Invoke();
            return;
        }

        nextFireTime = Time.time + currentWeapon.fireRate;

        bool firedSuccessfully = false;

        if (currentWeapon.fireMode == WeaponFireMode.Projectile)
            firedSuccessfully = FireProjectilePattern();

        if (!firedSuccessfully)
            return;

        currentAmmoInClip--;
        reloadPromptShown = false;

        AwardWeaponUseXp(currentWeapon.defaultAmmo);

        TargetRangeTracker tracker = TargetRangeTracker.Instance;
        if (tracker != null)
            tracker.RegisterShot(currentWeapon.weaponId, currentWeapon.weaponType);

        Debug.Log($"{currentWeapon.displayName} ammo: {currentAmmoInClip}/{currentWeapon.clipSize}");

        if (currentAmmoInClip <= 0)
            ShowReloadPrompt();

        OnWeaponAmmoChanged?.Invoke();
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
        bool spawnedAnyProjectile = false;

        Debug.Log($"Firing {currentWeapon.displayName}: {projectileCount} projectile(s) using {ammo.displayName}");

        for (int i = 0; i < projectileCount; i++)
        {
            bool spawnedProjectile = FireSingleProjectile(ammo);

            if (spawnedProjectile)
                spawnedAnyProjectile = true;
        }

        return spawnedAnyProjectile;
    }

    private bool FireSingleProjectile(AmmoData ammo)
    {
        Transform spawnPoint = GetMuzzlePoint();

        if (spawnPoint == null)
        {
            Debug.LogWarning("No muzzle point or player camera available.");
            return false;
        }

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
            return false;
        }

        projectile.Initialize(
            ammo.damage,
            currentWeapon.weaponId,
            currentWeapon.weaponType,
            gameObject,
            ammo.projectileSpeed,
            ammo.projectileLifetime);

        return true;
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

        if (playerCamera != null)
            return playerCamera.transform;

        return null;
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
            currentAmmoInClip = 0;
            OnWeaponAmmoChanged?.Invoke();
            return;
        }

        currentAmmoInClip = Mathf.Max(1, currentWeapon.clipSize);
        isReloading = false;
        reloadPromptShown = false;

        OnWeaponAmmoChanged?.Invoke();
    }

    private void TryReload()
    {
        if (currentWeapon == null)
            return;

        if (isReloading)
            return;

        if (currentAmmoInClip >= currentWeapon.clipSize)
        {
            Debug.Log($"{currentWeapon.displayName} clip is already full.");
            return;
        }

        if (!currentWeapon.infiniteReserveAmmo)
        {
            if (ammoInventory == null)
            {
                Debug.LogWarning("No PlayerAmmoInventory assigned.");
                return;
            }

            if (!ammoInventory.HasAmmo(currentWeapon.defaultAmmo))
            {
                Debug.Log($"No reserve ammo for {currentWeapon.defaultAmmo.displayName}.");
                ShowReloadPrompt();
                OnWeaponAmmoChanged?.Invoke();
                return;
            }
        }

        StartCoroutine(ReloadRoutine(currentWeapon, currentWeapon.defaultAmmo));
    }

    private IEnumerator ReloadRoutine(WeaponData weaponAtReloadStart, AmmoData ammoAtReloadStart)
    {
        isReloading = true;
        reloadPromptShown = false;

        Debug.Log($"Reloading {weaponAtReloadStart.displayName}...");

        OnWeaponAmmoChanged?.Invoke();

        yield return new WaitForSeconds(weaponAtReloadStart.reloadTime);

        if (currentWeapon != weaponAtReloadStart)
        {
            isReloading = false;
            OnWeaponAmmoChanged?.Invoke();
            yield break;
        }

        int neededAmmo = weaponAtReloadStart.clipSize - currentAmmoInClip;

        if (neededAmmo <= 0)
        {
            isReloading = false;
            OnWeaponAmmoChanged?.Invoke();
            yield break;
        }

        if (weaponAtReloadStart.infiniteReserveAmmo)
        {
            currentAmmoInClip += neededAmmo;
        }
        else
        {
            if (ammoInventory == null)
            {
                isReloading = false;
                OnWeaponAmmoChanged?.Invoke();
                yield break;
            }

            int loadedAmmo = ammoInventory.RemoveAmmo(ammoAtReloadStart, neededAmmo);
            currentAmmoInClip += loadedAmmo;
        }

        currentAmmoInClip = Mathf.Clamp(currentAmmoInClip, 0, weaponAtReloadStart.clipSize);
        isReloading = false;

        Debug.Log($"{weaponAtReloadStart.displayName} reloaded: {currentAmmoInClip}/{weaponAtReloadStart.clipSize}");

        OnWeaponAmmoChanged?.Invoke();
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

    private void HandleAmmoInventoryChanged()
    {
        OnWeaponAmmoChanged?.Invoke();
    }
}

//-----PlayerWeaponController.cs END-----