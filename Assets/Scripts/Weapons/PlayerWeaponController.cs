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
    [SerializeField] private PlayerWeaponViewModelController viewModelController;
    [SerializeField] private PlayerReticleRecoilUI reticleRecoilUI;

    [Header("Loadout")]
    [SerializeField] private bool autoEquipSavedWeapon = true;

    [Header("Projectile Spawn")]
    [SerializeField] private float muzzleForwardOffset = 0.15f;

    [Header("Aim Accuracy")]
    [SerializeField] private LayerMask aimMask = ~0;
    [SerializeField] private float aimDistance = 1000f;
    [SerializeField] private bool debugAimRays = false;

    private WeaponData currentWeapon;
    private GameObject currentViewModel;
    private Transform currentMuzzlePoint;

    private float nextFireTime;
    private bool inputEnabled = true;

    private int currentAmmoInClip;
    private bool isReloading;
    private bool reloadPromptShown;

    private Coroutine reloadRoutine;
    private Coroutine emptyFireReloadRoutine;
    private float nextDryFireTime;

    public event System.Action OnWeaponAmmoChanged;
    public event System.Action<WeaponData> OnWeaponChanged;

    public event System.Action<WeaponData> OnWeaponEquipped;
    public event System.Action<WeaponData> OnWeaponFired;
    public event System.Action<WeaponData> OnWeaponDryFired;
    public event System.Action<WeaponData> OnWeaponReloadStarted;
    public event System.Action<WeaponData> OnWeaponReloadInserted;
    public event System.Action<WeaponData> OnWeaponReloadEnded;

    public bool HasWeapon => currentWeapon != null;
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

        if (viewModelController == null)
            viewModelController = GetComponent<PlayerWeaponViewModelController>();

        if (reticleRecoilUI == null)
            reticleRecoilUI = FindFirstObjectByType<PlayerReticleRecoilUI>();
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

        bool wantsToFire = currentWeapon.isAutomatic
            ? inputReader.FireHeld
            : inputReader.FirePressed;

        if (wantsToFire)
            TryFire();
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (!CanEquipOwnedWeapon(weaponData))
            return;

        EquipWeaponInternal(weaponData, true, true);
    }

    public void EquipTemporaryWeapon(WeaponData weaponData)
    {
        if (!CanEquipTemporaryWeapon(weaponData))
            return;

        EquipWeaponInternal(weaponData, false, false);
    }

    private bool CanEquipOwnedWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
            return false;

        TargetRangeMissionController missionController = TargetRangeMissionController.Instance;

        if (missionController == null)
            return true;

        if (!missionController.IsWeaponSelectionLocked)
            return true;

        Debug.Log($"Cannot equip owned weapon '{weaponData.displayName}' while target range mission weapon is locked.");
        return false;
    }

    private bool CanEquipTemporaryWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
            return false;

        TargetRangeMissionController missionController = TargetRangeMissionController.Instance;

        if (missionController == null)
            return true;

        if (!missionController.IsWeaponSelectionLocked)
            return true;

        bool allowed = missionController.IsWeaponAllowedForActiveMission(weaponData);

        if (!allowed)
            Debug.Log($"Temporary weapon '{weaponData.displayName}' is not allowed for the active target range mission.");

        return allowed;
    }

    private bool CanUseCurrentWeapon()
    {
        if (currentWeapon == null)
            return false;

        TargetRangeMissionController missionController = TargetRangeMissionController.Instance;

        if (missionController == null)
            return true;

        if (!missionController.IsWeaponSelectionLocked)
            return true;

        return missionController.IsWeaponAllowedForActiveMission(currentWeapon);
    }

    private void EquipWeaponInternal(WeaponData weaponData, bool addToInventory, bool saveAfterEquip)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("Tried to equip null weapon data.");
            return;
        }

        StopAllCoroutines();

        reloadRoutine = null;
        emptyFireReloadRoutine = null;

        currentWeapon = weaponData;
        isReloading = false;
        reloadPromptShown = false;
        nextDryFireTime = 0f;

        if (addToInventory)
            PlayerProgress.SetActiveWeapon(currentWeapon.weaponId);

        SpawnViewModel();
        FillClip();

        if (reticleRecoilUI != null)
            reticleRecoilUI.SetActiveHandlingData(currentWeapon.handlingData);

        TargetRangeMissionController missionController = TargetRangeMissionController.Instance;

        if (missionController != null)
            missionController.RegisterMissionWeaponEquipped(currentWeapon.weaponId, currentWeapon.weaponType);

        if (addToInventory && saveAfterEquip && SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log(addToInventory
            ? $"Equipped owned weapon: {currentWeapon.displayName}"
            : $"Equipped temporary mission weapon: {currentWeapon.displayName}");

        Debug.Log($"{currentWeapon.displayName} ammo: {currentAmmoInClip}/{currentWeapon.clipSize}");

        OnWeaponAmmoChanged?.Invoke();
        OnWeaponChanged?.Invoke(currentWeapon);
        OnWeaponEquipped?.Invoke(currentWeapon);
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

        EquipWeaponInternal(savedWeapon, true, false);

        Debug.Log($"Auto-equipped saved weapon: {savedWeapon.displayName}");
    }

    private void TryFire()
    {
        if (!CanUseCurrentWeapon())
        {
            Debug.Log("Current weapon cannot be used during this target range mission.");
            return;
        }

        if (Time.time < nextFireTime)
            return;

        if (currentWeapon.defaultAmmo == null)
        {
            Debug.LogWarning($"Weapon '{currentWeapon.displayName}' has no default ammo assigned.");
            return;
        }

        if (isReloading)
        {
            if (!currentWeapon.canFireDuringReload)
                return;

            if (currentAmmoInClip <= 0)
            {
                HandleEmptyFire();
                return;
            }

            if (currentWeapon.cancelReloadOnFire)
                CancelReload();
        }

        if (currentAmmoInClip <= 0)
        {
            HandleEmptyFire();
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

        TargetRangeMissionController missionController = TargetRangeMissionController.Instance;

        if (missionController != null)
            missionController.RegisterMissionShot(currentWeapon.weaponId, currentWeapon.weaponType);

        Vector3 sharedWeaponRotationKick = Vector3.zero;
        bool hasSharedWeaponRotationKick = false;

        if (viewModelController != null)
        {
            sharedWeaponRotationKick = viewModelController.PlayFireFeedback();
            hasSharedWeaponRotationKick = true;
        }

        if (reticleRecoilUI != null)
        {
            reticleRecoilUI.PlayFireFeedback(
                currentWeapon.handlingData,
                sharedWeaponRotationKick,
                hasSharedWeaponRotationKick);
        }

        OnWeaponFired?.Invoke(currentWeapon);

        Debug.Log($"{currentWeapon.displayName} ammo: {currentAmmoInClip}/{currentWeapon.clipSize}");

        if (currentAmmoInClip <= 0)
            ShowReloadPrompt();

        OnWeaponAmmoChanged?.Invoke();
    }

    private void TryDryFire()
    {
        if (currentWeapon == null)
            return;

        if (!currentWeapon.canDryFire)
        {
            ShowReloadPrompt();
            OnWeaponAmmoChanged?.Invoke();
            return;
        }

        if (Time.time < nextDryFireTime)
            return;

        nextDryFireTime = Time.time + Mathf.Max(0.01f, currentWeapon.dryFireCooldown);

        ShowReloadPrompt();

        if (viewModelController != null)
            viewModelController.PlayDryFireFeedback();

        OnWeaponDryFired?.Invoke(currentWeapon);

        Debug.Log($"{currentWeapon.displayName} dry fired.");

        OnWeaponAmmoChanged?.Invoke();
    }

    private void HandleEmptyFire()
    {
        if (currentWeapon == null)
            return;

        switch (currentWeapon.emptyFireBehavior)
        {
            case EmptyFireBehavior.ReloadOnly:
                TryReload();
                break;

            case EmptyFireBehavior.DryFireThenReload:
                TryDryFire();
                StartEmptyFireReloadDelay();
                break;

            case EmptyFireBehavior.DryFireOnly:
            default:
                TryDryFire();
                break;
        }
    }

    private void StartEmptyFireReloadDelay()
    {
        if (currentWeapon == null)
            return;

        if (isReloading)
            return;

        if (emptyFireReloadRoutine != null)
            return;

        emptyFireReloadRoutine = StartCoroutine(EmptyFireReloadDelayRoutine(currentWeapon));
    }

    private IEnumerator EmptyFireReloadDelayRoutine(WeaponData weaponAtStart)
    {
        float delay = Mathf.Max(0f, weaponAtStart.emptyFireReloadDelay);

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        emptyFireReloadRoutine = null;

        if (currentWeapon != weaponAtStart)
            yield break;

        if (currentAmmoInClip > 0)
            yield break;

        TryReload();
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

        Vector3 spawnPosition = spawnPoint.position;
        Quaternion fireRotation = GetAccurateFireRotation(spawnPosition);

        fireRotation = ApplyWeaponSpread(fireRotation);

        spawnPosition += fireRotation * Vector3.forward * muzzleForwardOffset;

        if (debugAimRays)
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 100f, Color.green, 1f);
            Debug.DrawRay(spawnPosition, fireRotation * Vector3.forward * 100f, Color.red, 1f);
        }

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

    private Quaternion GetAccurateFireRotation(Vector3 projectileSpawnPosition)
    {
        if (playerCamera == null)
            return transform.rotation;

        Vector3 aimPoint = playerCamera.transform.position + playerCamera.transform.forward * aimDistance;

        Ray aimRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(
            aimRay,
            aimDistance,
            aimMask,
            QueryTriggerInteraction.Collide);

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (IsOwnerOrOwnerChild(hit.collider.gameObject))
                    continue;

                Projectile projectile = hit.collider.GetComponentInParent<Projectile>();

                if (projectile != null)
                    continue;

                aimPoint = hit.point;
                break;
            }
        }

        Vector3 direction = aimPoint - projectileSpawnPosition;

        if (direction.sqrMagnitude <= 0.0001f)
            direction = playerCamera.transform.forward;

        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private Quaternion ApplyWeaponSpread(Quaternion baseRotation)
    {
        if (currentWeapon == null || currentWeapon.spreadAngle <= 0f)
            return baseRotation;

        float randomYaw = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle);
        float randomPitch = Random.Range(-currentWeapon.spreadAngle, currentWeapon.spreadAngle);

        return baseRotation * Quaternion.Euler(randomPitch, randomYaw, 0f);
    }

    private bool IsOwnerOrOwnerChild(GameObject otherObject)
    {
        if (otherObject == null)
            return false;

        if (otherObject == gameObject)
            return true;

        return otherObject.transform.IsChildOf(transform);
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
        {
            Destroy(currentViewModel);
            currentViewModel = null;
        }

        if (currentWeapon.viewModelPrefab == null || weaponHolder == null)
        {
            if (viewModelController != null)
                viewModelController.ClearActiveViewModel();

            return;
        }

        currentViewModel = Instantiate(currentWeapon.viewModelPrefab, weaponHolder);

        currentViewModel.transform.localPosition = currentWeapon.viewLocalPosition;
        currentViewModel.transform.localEulerAngles = currentWeapon.viewLocalEulerAngles;
        currentViewModel.transform.localScale = currentWeapon.viewLocalScale;

        if (viewModelController != null)
            viewModelController.SetActiveViewModel(currentViewModel.transform, currentWeapon.handlingData);

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

        if (!CanUseCurrentWeapon())
        {
            Debug.Log("Current weapon cannot be reloaded during this target range mission.");
            return;
        }

        if (currentWeapon.defaultAmmo == null)
        {
            Debug.LogWarning($"Weapon '{currentWeapon.displayName}' has no default ammo assigned.");
            return;
        }

        if (currentAmmoInClip >= currentWeapon.clipSize)
        {
            Debug.Log($"{currentWeapon.displayName} clip is already full.");
            return;
        }

        if (!HasReloadAmmo(currentWeapon, currentWeapon.defaultAmmo))
        {
            ShowReloadPrompt();
            OnWeaponAmmoChanged?.Invoke();
            return;
        }

        if (currentWeapon.reloadMode == WeaponReloadMode.OneRoundAtATime)
        {
            reloadRoutine = StartCoroutine(
                ReloadOneRoundAtATimeRoutine(currentWeapon, currentWeapon.defaultAmmo));
        }
        else
        {
            reloadRoutine = StartCoroutine(
                ReloadFullClipRoutine(currentWeapon, currentWeapon.defaultAmmo));
        }
    }

    private IEnumerator ReloadFullClipRoutine(WeaponData weaponAtReloadStart, AmmoData ammoAtReloadStart)
    {
        isReloading = true;
        reloadPromptShown = false;

        OnWeaponReloadStarted?.Invoke(weaponAtReloadStart);

        if (viewModelController != null)
        {
            viewModelController.PlayReloadFeedback();

            if (weaponAtReloadStart.playReloadStartFeedback)
                viewModelController.PlayReloadStartFeedback();
        }

        Debug.Log($"Reloading {weaponAtReloadStart.displayName}...");

        OnWeaponAmmoChanged?.Invoke();

        float totalReloadTime = Mathf.Max(0f, weaponAtReloadStart.reloadTime);
        float elapsedTime = 0f;

        if (weaponAtReloadStart.playReloadInsertFeedback)
        {
            float insertTime = Mathf.Clamp(
                weaponAtReloadStart.fullReloadInsertFeedbackTime,
                0f,
                totalReloadTime);

            if (insertTime > elapsedTime)
            {
                yield return new WaitForSeconds(insertTime - elapsedTime);
                elapsedTime = insertTime;
            }

            if (currentWeapon != weaponAtReloadStart)
            {
                FinishReloadState();
                yield break;
            }

            if (viewModelController != null)
                viewModelController.PlayReloadInsertFeedback();

            OnWeaponReloadInserted?.Invoke(weaponAtReloadStart);
        }

        if (weaponAtReloadStart.playReloadEndFeedback)
        {
            float endTime = Mathf.Clamp(
                totalReloadTime - Mathf.Max(0f, weaponAtReloadStart.fullReloadEndFeedbackLeadTime),
                elapsedTime,
                totalReloadTime);

            if (endTime > elapsedTime)
            {
                yield return new WaitForSeconds(endTime - elapsedTime);
                elapsedTime = endTime;
            }

            if (currentWeapon != weaponAtReloadStart)
            {
                FinishReloadState();
                yield break;
            }

            if (viewModelController != null)
                viewModelController.PlayReloadEndFeedback();

            OnWeaponReloadEnded?.Invoke(weaponAtReloadStart);
        }

        if (totalReloadTime > elapsedTime)
            yield return new WaitForSeconds(totalReloadTime - elapsedTime);

        if (currentWeapon != weaponAtReloadStart)
        {
            FinishReloadState();
            yield break;
        }

        int neededAmmo = weaponAtReloadStart.clipSize - currentAmmoInClip;

        if (neededAmmo > 0)
            LoadAmmoIntoClip(weaponAtReloadStart, ammoAtReloadStart, neededAmmo);

        FinishReloadState();

        Debug.Log($"{weaponAtReloadStart.displayName} reloaded: {currentAmmoInClip}/{weaponAtReloadStart.clipSize}");

        OnWeaponAmmoChanged?.Invoke();
    }

    private IEnumerator ReloadOneRoundAtATimeRoutine(WeaponData weaponAtReloadStart, AmmoData ammoAtReloadStart)
    {
        isReloading = true;
        reloadPromptShown = false;

        OnWeaponReloadStarted?.Invoke(weaponAtReloadStart);

        if (viewModelController != null && weaponAtReloadStart.playReloadStartFeedback)
            viewModelController.PlayReloadStartFeedback();

        Debug.Log($"Started round-by-round reload: {weaponAtReloadStart.displayName}");

        OnWeaponAmmoChanged?.Invoke();

        yield return new WaitForSeconds(Mathf.Max(0f, weaponAtReloadStart.reloadStartTime));

        while (currentWeapon == weaponAtReloadStart &&
               currentAmmoInClip < weaponAtReloadStart.clipSize &&
               HasReloadAmmo(weaponAtReloadStart, ammoAtReloadStart))
        {
            yield return new WaitForSeconds(Mathf.Max(0f, weaponAtReloadStart.reloadRoundInsertTime));

            if (currentWeapon != weaponAtReloadStart)
            {
                FinishReloadState();
                yield break;
            }

            int loadedAmmo = LoadAmmoIntoClip(weaponAtReloadStart, ammoAtReloadStart, 1);

            if (loadedAmmo <= 0)
                break;

            if (viewModelController != null && weaponAtReloadStart.playReloadInsertFeedback)
                viewModelController.PlayReloadInsertFeedback();

            if (weaponAtReloadStart.playReloadInsertFeedback)
                OnWeaponReloadInserted?.Invoke(weaponAtReloadStart);

            Debug.Log($"{weaponAtReloadStart.displayName} inserted round: {currentAmmoInClip}/{weaponAtReloadStart.clipSize}");

            OnWeaponAmmoChanged?.Invoke();
        }

        if (currentWeapon == weaponAtReloadStart &&
            weaponAtReloadStart.playReloadEndFeedback)
        {
            if (viewModelController != null)
                viewModelController.PlayReloadEndFeedback();

            OnWeaponReloadEnded?.Invoke(weaponAtReloadStart);
        }

        yield return new WaitForSeconds(Mathf.Max(0f, weaponAtReloadStart.reloadEndTime));

        FinishReloadState();

        Debug.Log($"Finished round-by-round reload: {weaponAtReloadStart.displayName} {currentAmmoInClip}/{weaponAtReloadStart.clipSize}");

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

    public void UnequipCurrentWeapon(bool clearProgress = true)
    {
        StopAllCoroutines();

        reloadRoutine = null;
        emptyFireReloadRoutine = null;

        currentWeapon = null;
        currentAmmoInClip = 0;
        isReloading = false;
        reloadPromptShown = false;
        nextFireTime = 0f;
        nextDryFireTime = 0f;

        if (currentViewModel != null)
        {
            Destroy(currentViewModel);
            currentViewModel = null;
        }

        currentMuzzlePoint = null;

        if (viewModelController != null)
            viewModelController.ClearActiveViewModel();

        if (reticleRecoilUI != null)
            reticleRecoilUI.ClearActiveHandlingData();

        if (clearProgress)
            PlayerProgress.ClearActiveWeapon();

        OnWeaponAmmoChanged?.Invoke();
        OnWeaponChanged?.Invoke(null);
    }

    private bool HasReloadAmmo(WeaponData weaponData, AmmoData ammoData)
    {
        if (weaponData == null)
            return false;

        if (weaponData.infiniteReserveAmmo)
            return true;

        if (ammoInventory == null)
        {
            Debug.LogWarning("No PlayerAmmoInventory assigned.");
            return false;
        }

        if (ammoData == null)
        {
            Debug.LogWarning($"Weapon '{weaponData.displayName}' has no ammo data assigned.");
            return false;
        }

        return ammoInventory.HasAmmo(ammoData);
    }

    private int LoadAmmoIntoClip(WeaponData weaponData, AmmoData ammoData, int requestedAmount)
    {
        if (weaponData == null)
            return 0;

        int roomInClip = weaponData.clipSize - currentAmmoInClip;
        int amountToLoad = Mathf.Min(requestedAmount, roomInClip);

        if (amountToLoad <= 0)
            return 0;

        int loadedAmount = amountToLoad;

        if (!weaponData.infiniteReserveAmmo)
        {
            if (ammoInventory == null || ammoData == null)
                return 0;

            loadedAmount = ammoInventory.RemoveAmmo(ammoData, amountToLoad);
        }

        currentAmmoInClip += loadedAmount;
        currentAmmoInClip = Mathf.Clamp(currentAmmoInClip, 0, weaponData.clipSize);

        return loadedAmount;
    }

    private void CancelReload()
    {
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
        }

        if (emptyFireReloadRoutine != null)
        {
            StopCoroutine(emptyFireReloadRoutine);
            emptyFireReloadRoutine = null;
        }

        isReloading = false;
        reloadPromptShown = false;

        Debug.Log($"{currentWeapon.displayName} reload cancelled.");

        OnWeaponAmmoChanged?.Invoke();
    }

    private void FinishReloadState()
    {
        isReloading = false;
        reloadRoutine = null;
        reloadPromptShown = false;
    }
}

//-----PlayerWeaponController.cs END-----