---
title: Milestone 1C - Target Range HUD, Weapon Visuals, and Completion Feedback
project: Echo Systems Lab
type: checkpoint-build-plan
tags:
  - unity
  - systems-design
  - target-range
  - weapons
  - hud
  - portfolio
---

# Milestone 1C - Target Range HUD, Weapon Visuals, and Completion Feedback

This checkpoint builds on the current Target Range Trial MVP.

Current state:

```text
1. Enter TargetRangeTrial scene from the Hub terminal. DONE
2. Look at pistol pedestal. DONE
3. Prompt says "Press E to equip Pistol." DONE
4. Press E. DONE
5. Pistol equips. PARTIAL
6. Left click fires projectile. DONE
7. Projectile hits bullseye target. DONE
8. Target loses health. DONE, but needs feedback
9. Target is destroyed after enough hits. DONE
10. Tracker logs score, hits, shots, and weapon XP. UNKNOWN / needs feedback
11. Destroying all targets marks CombatTrial complete. NOT VISIBLE YET
12. Press R to return to Hub. DONE
13. Hub terminal now shows completed/unlocked progression. NEEDS FINAL TEST
```

The firing loop works, but the data is still mostly invisible. This phase makes the system readable through HUD feedback, tracker events, weapon visuals, and mission completion messaging.

---

# Goal Line

```text
In TargetRangeTrial:
Interact with pistol pedestal
→ Pistol visual disappears from pedestal
→ Pistol view model appears in player view
→ Left click fires projectiles
→ Targets update tracked data
→ HUD shows weapon, targets left, score, shots, hits, accuracy, weapon XP
→ Destroying all targets shows completion message
→ Press R to return to Hub
→ Hub terminal marks Target Range Trial complete
→ Combat Trial unlocks next
```

---

# Naming / Scope Notes

The current trial now has its own mission asset:

```text
MissionData_TargetRangeTrial
```

So the tracker should complete this mission ID:

```text
TargetRangeTrial
```

The next mission, `MissionData_CombatTrial`, should require:

```text
TargetRangeTrial
```

This creates the progression chain:

```text
Target Range Trial → Combat Trial
```

For now, `CombatTrial` can point to a blank scene or placeholder scene while Target Range gets polished.

---

# System Pieces

```text
1. TargetRangeTracker event hooks
2. TargetRangeHUD
3. Weapon pedestal visual hiding
4. Player weapon view model attaching
5. View model muzzle point support
6. Target completion feedback
7. Mission unlock polish
8. Console/debug validation
```

---

# Step 1 - Update MissionData Unlock Rules

In Unity, select:

```text
MissionData_TargetRangeTrial
```

Set:

```text
Mission Id: TargetRangeTrial
Display Name: Target Range Trial
Scene Name: TargetRangeTrial
Unlocked By Default: true
```

Then select:

```text
MissionData_CombatTrial
```

Set:

```text
Mission Id: CombatTrial
Display Name: Combat Trial
Scene Name: CombatTrial
Unlocked By Default: false
Required Completed Mission Ids:
  Element 0: TargetRangeTrial
```

On the `TargetRangeTracker` object in the scene, set:

```text
Mission Id To Complete = TargetRangeTrial
```

Important:

```text
The tracker should complete TargetRangeTrial, not CombatTrial.
```

---

# Step 2 - Make the Tracker Broadcast Data

The tracker stores data, but the HUD needs a clean way to update when data changes. Add events for stat changes and trial completion.

Replace `TargetRangeTracker.cs` with this version:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TargetRangeTracker : MonoBehaviour
{
    public static TargetRangeTracker Instance { get; private set; }

    public event Action OnStatsChanged;
    public event Action OnTrialCompleted;

    [Header("Mission Completion")]
    [SerializeField] private string missionIdToComplete = "TargetRangeTrial";
    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private KeyCode returnToHubKey = KeyCode.R;

    [Header("State")]
    [SerializeField] private int totalTargets;
    [SerializeField] private int destroyedTargets;
    [SerializeField] private int score;
    [SerializeField] private int shotsFired;
    [SerializeField] private int hitsLanded;

    private readonly List<TargetHealth> registeredTargets = new List<TargetHealth>();
    private readonly Dictionary<string, int> weaponXp = new Dictionary<string, int>();

    private bool trialCompleted;
    private string currentWeaponId;
    private string currentWeaponType;

    public int TotalTargets => totalTargets;
    public int DestroyedTargets => destroyedTargets;
    public int TargetsRemaining => Mathf.Max(0, totalTargets - destroyedTargets);
    public int Score => score;
    public int ShotsFired => shotsFired;
    public int HitsLanded => hitsLanded;
    public bool TrialCompleted => trialCompleted;
    public string CurrentWeaponId => currentWeaponId;
    public string CurrentWeaponType => currentWeaponType;

    public float AccuracyPercent
    {
        get
        {
            if (shotsFired <= 0)
                return 0f;

            return (float)hitsLanded / shotsFired * 100f;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        NotifyStatsChanged();
    }

    private void Update()
    {
        if (!trialCompleted)
            return;

        if (Input.GetKeyDown(returnToHubKey))
            ReturnToHub();
    }

    public void RegisterTarget(TargetHealth target)
    {
        if (target == null)
            return;

        if (registeredTargets.Contains(target))
            return;

        registeredTargets.Add(target);
        totalTargets = registeredTargets.Count;

        Debug.Log($"Registered target: {target.name}. Total targets: {totalTargets}");

        NotifyStatsChanged();
    }

    public void RegisterWeaponEquipped(string weaponId, string weaponType)
    {
        currentWeaponId = weaponId;
        currentWeaponType = weaponType;

        EnsureWeaponXpEntry(weaponId);

        Debug.Log($"Weapon equipped: {weaponId} ({weaponType})");

        NotifyStatsChanged();
    }

    public void RegisterShot(string weaponId, string weaponType)
    {
        shotsFired++;

        Debug.Log($"Shot fired with {weaponId}. Total shots: {shotsFired}");

        NotifyStatsChanged();
    }

    public void RegisterHit(string weaponId, string weaponType)
    {
        hitsLanded++;

        AddWeaponXp(weaponId, 1);

        Debug.Log($"Hit landed with {weaponId}. Total hits: {hitsLanded}");

        NotifyStatsChanged();
    }

    public void RegisterTargetDamaged(TargetHealth target, DamageInfo damageInfo)
    {
        Debug.Log($"Target damaged: {target.name} for {damageInfo.damageAmount}");

        NotifyStatsChanged();
    }

    public void RegisterTargetDestroyed(TargetHealth target, DamageInfo damageInfo)
    {
        destroyedTargets++;
        score += target.ScoreValue;

        AddWeaponXp(damageInfo.weaponId, 5);

        Debug.Log($"Target destroyed: {target.name}. Score: {score}");

        NotifyStatsChanged();

        CheckTrialCompletion();
    }

    public int GetWeaponXp(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return 0;

        if (!weaponXp.ContainsKey(weaponId))
            return 0;

        return weaponXp[weaponId];
    }

    private void CheckTrialCompletion()
    {
        if (trialCompleted)
            return;

        if (totalTargets <= 0)
            return;

        if (destroyedTargets < totalTargets)
            return;

        CompleteTrial();
    }

    private void CompleteTrial()
    {
        trialCompleted = true;

        MissionProgress.MarkCompleted(missionIdToComplete);

        Debug.Log("Target Range Trial completed.");
        Debug.Log($"Completed Mission Id: {missionIdToComplete}");
        Debug.Log($"Final Score: {score}");
        Debug.Log($"Accuracy: {AccuracyPercent:0.0}%");
        Debug.Log("Press R to return to Hub.");

        OnTrialCompleted?.Invoke();
        NotifyStatsChanged();
    }

    private void AddWeaponXp(string weaponId, int amount)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        EnsureWeaponXpEntry(weaponId);

        weaponXp[weaponId] += amount;

        Debug.Log($"Weapon XP: {weaponId} = {weaponXp[weaponId]}");
    }

    private void EnsureWeaponXpEntry(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        if (!weaponXp.ContainsKey(weaponId))
            weaponXp.Add(weaponId, 0);
    }

    private void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    private void ReturnToHub()
    {
        if (GameSceneLoader.Instance != null)
        {
            GameSceneLoader.Instance.LoadHub();
            return;
        }

        SceneManager.LoadScene(hubSceneName);
    }
}
```

---

# Step 3 - Re-enable Hit Tracking in Projectile

In `Projectile.cs`, this section is currently commented out:

```csharp
//TargetRangeTracker tracker = TargetRangeTracker.Instance;
//if (tracker != null)
//tracker.RegisterHit(weaponId, weaponType);
```

Uncomment and clean it up:

```csharp
TargetRangeTracker tracker = TargetRangeTracker.Instance;
if (tracker != null)
    tracker.RegisterHit(weaponId, weaponType);
```

The hit section should become:

```csharp
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
```

This should fix missing hit and weapon XP data.

---

# Step 4 - Create TargetRangeHUD

Create:

```text
Assets/_EchoSystemsLab/Scripts/UI/TargetRangeHUD.cs
```

```csharp
using TMPro;
using UnityEngine;

public class TargetRangeHUD : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text weaponText;
    [SerializeField] private TMP_Text targetsText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text shotsText;
    [SerializeField] private TMP_Text hitsText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text weaponXpText;
    [SerializeField] private TMP_Text completionText;

    private TargetRangeTracker tracker;

    private void Start()
    {
        tracker = TargetRangeTracker.Instance;

        if (tracker == null)
        {
            Debug.LogWarning("TargetRangeHUD could not find TargetRangeTracker.");
            return;
        }

        tracker.OnStatsChanged += Refresh;
        tracker.OnTrialCompleted += ShowCompletion;

        if (completionText != null)
            completionText.gameObject.SetActive(false);

        Refresh();
    }

    private void OnDestroy()
    {
        if (tracker == null)
            return;

        tracker.OnStatsChanged -= Refresh;
        tracker.OnTrialCompleted -= ShowCompletion;
    }

    private void Refresh()
    {
        if (tracker == null)
            return;

        string weaponName = string.IsNullOrWhiteSpace(tracker.CurrentWeaponId)
            ? "None"
            : tracker.CurrentWeaponId;

        if (weaponText != null)
            weaponText.text = $"Weapon: {weaponName}";

        if (targetsText != null)
            targetsText.text = $"Targets: {tracker.TargetsRemaining} / {tracker.TotalTargets}";

        if (scoreText != null)
            scoreText.text = $"Score: {tracker.Score}";

        if (shotsText != null)
            shotsText.text = $"Shots: {tracker.ShotsFired}";

        if (hitsText != null)
            hitsText.text = $"Hits: {tracker.HitsLanded}";

        if (accuracyText != null)
            accuracyText.text = $"Accuracy: {tracker.AccuracyPercent:0.0}%";

        if (weaponXpText != null)
        {
            int xp = tracker.GetWeaponXp(tracker.CurrentWeaponId);
            weaponXpText.text = $"Weapon XP: {xp}";
        }
    }

    private void ShowCompletion()
    {
        if (completionText != null)
        {
            completionText.gameObject.SetActive(true);
            completionText.text = "TRIAL COMPLETE\\nPress R to return to Hub";
        }

        Refresh();
    }
}
```

---

# Step 5 - Build the HUD Canvas

In `TargetRangeTrial`, create:

```text
Canvas_TargetRangeHUD
 ├── HUDPanel
 │    ├── WeaponText
 │    ├── TargetsText
 │    ├── ScoreText
 │    ├── ShotsText
 │    ├── HitsText
 │    ├── AccuracyText
 │    └── WeaponXPText
 └── CompletionText
```

Add `TargetRangeHUD` to:

```text
Canvas_TargetRangeHUD
```

Assign each TMP text reference in the Inspector.

Simple layout suggestion:

```text
Top Left:
Weapon
Targets
Score
Shots
Hits
Accuracy
Weapon XP

Center:
CompletionText, disabled by default
```

Keep it ugly-functional first. Polish later.

---

# Step 6 - Fix Pistol View Model Equipping

The pistol fires because `currentWeapon` is set, but the pistol is not visible because `WeaponData_Pistol.viewModelPrefab` is probably empty.

In `WeaponData_Pistol`, assign:

```text
View Model Prefab = your pistol mesh prefab
```

Create a separate prefab:

```text
Assets/_EchoSystemsLab/Prefabs/Weapons/ViewModels/Pistol_ViewModel.prefab
```

Suggested structure:

```text
Pistol_ViewModel
 ├── Pistol mesh
 └── MuzzlePoint
```

Put `MuzzlePoint` at the barrel tip.

Then tune `WeaponData_Pistol`:

```text
View Local Position
View Local Euler Angles
View Local Scale
```

Example starting values:

```text
View Local Position: 0.35, -0.25, 0.55
View Local Euler Angles: 0, 180, 0
View Local Scale: 1, 1, 1
```

Adjust in play mode to find good values, then copy the values back after stopping play mode.

---

# Step 7 - Move MuzzlePoint Toward the Weapon

The player can keep a fallback muzzle point, but each weapon view model should eventually own its own muzzle.

Replace `PlayerWeaponController.cs` with this version:

```csharp
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
```

In the Inspector, the old `Muzzle Point` field will now be:

```text
Fallback Muzzle Point
```

Assign the current player muzzle there for safety.

---

# Step 8 - Hide the Pedestal Weapon Visual on Pickup

Update `WeaponPedestal.cs`:

```csharp
using UnityEngine;

public class WeaponPedestal : MonoBehaviour, IInteractable
{
    [Header("Weapon")]
    [SerializeField] private WeaponData weaponData;

    [Header("Pedestal Visual")]
    [SerializeField] private GameObject pedestalWeaponVisual;
    [SerializeField] private bool hideVisualAfterPickup = true;
    [SerializeField] private bool disableInteractionAfterPickup = true;

    [Header("Prompt")]
    [SerializeField] private string promptPrefix = "Press E to equip";

    private bool hasBeenPickedUp;

    public string GetPromptText()
    {
        if (weaponData == null)
            return "No weapon assigned";

        if (hasBeenPickedUp && disableInteractionAfterPickup)
            return $"{weaponData.displayName} equipped";

        return $"{promptPrefix} {weaponData.displayName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (hasBeenPickedUp && disableInteractionAfterPickup)
            return;

        if (weaponData == null)
        {
            Debug.LogWarning($"{name} has no WeaponData assigned.");
            return;
        }

        PlayerWeaponController weaponController = interactor.GetComponent<PlayerWeaponController>();

        if (weaponController == null)
        {
            Debug.LogWarning("Interactor does not have PlayerWeaponController.");
            return;
        }

        weaponController.EquipWeapon(weaponData);

        hasBeenPickedUp = true;

        if (hideVisualAfterPickup && pedestalWeaponVisual != null)
            pedestalWeaponVisual.SetActive(false);
    }
}
```

On the pistol pedestal, assign:

```text
Pedestal Weapon Visual = the pistol mesh sitting on the pedestal
Hide Visual After Pickup = true
Disable Interaction After Pickup = true
```

Now the pedestal pistol disappears when equipped.

---

# Step 9 - Add Simple Target Feedback

Targets are taking damage, but they need visible or console feedback.

Update `TargetHealth.cs` by adding these properties:

```csharp
public int CurrentHealth => currentHealth;
public int MaxHealth => maxHealth;
```

Inside `TakeDamage`, after reducing health, add:

```csharp
Debug.Log($"{name} health: {currentHealth}/{maxHealth}");
```

The method should look like:

```csharp
public void TakeDamage(DamageInfo damageInfo)
{
    if (isDestroyed)
        return;

    currentHealth -= damageInfo.damageAmount;

    Debug.Log($"{name} health: {currentHealth}/{maxHealth}");

    TargetRangeTracker tracker = TargetRangeTracker.Instance;
    if (tracker != null)
        tracker.RegisterTargetDamaged(this, damageInfo);

    if (currentHealth <= 0)
        DestroyTarget(damageInfo);
}
```

If no logs appear, check Unity Console filters and make sure regular logs are enabled.

---

# Step 10 - Verify Tracker Exists in Scene

In `TargetRangeTrial`, make sure there is an active object:

```text
TargetRangeTracker
 └── TargetRangeTracker.cs
```

Inspector values:

```text
Mission Id To Complete = TargetRangeTrial
Hub Scene Name = Hub
Return To Hub Key = R
```

If this object is missing, targets can die but the trial will not complete because nothing is counting them.

---

# Step 11 - Verify All Targets Have TargetHealth

Every target that should count toward completion needs:

```text
TargetHealth
Collider
```

The target registers itself in `Start()`:

```csharp
tracker.RegisterTarget(this);
```

For this checkpoint:

```text
All shootable bullseyes should have TargetHealth.
Props, crates, decorations should not.
```

---

# Step 12 - Unity Setup Checklist

## Player

```text
Player
 ├── SimpleFirstPersonController
 ├── PlayerInteractor
 ├── PlayerWeaponController
 └── Main Camera
      └── WeaponHolder
           └── FallbackMuzzlePoint
```

`PlayerWeaponController`:

```text
Player Camera = Main Camera
Weapon Holder = WeaponHolder
Fallback Muzzle Point = FallbackMuzzlePoint
```

## Pistol View Model

```text
Pistol_ViewModel
 ├── Pistol mesh
 └── MuzzlePoint
```

`WeaponData_Pistol`:

```text
View Model Prefab = Pistol_ViewModel
Projectile Prefab = Projectile_PistolRound
```

## Pistol Pedestal

```text
PistolPedestal
 ├── Collider
 ├── WeaponPedestal
 └── PistolVisual
```

`WeaponPedestal`:

```text
Weapon Data = WeaponData_Pistol
Pedestal Weapon Visual = PistolVisual
```

## HUD

```text
Canvas_TargetRangeHUD
 ├── HUDPanel
 └── CompletionText
```

`TargetRangeHUD` assigned to the canvas.

## Tracker

```text
TargetRangeTracker
 └── Mission Id To Complete = TargetRangeTrial
```

---

# Goal Line / Completion Checklist

This phase is done when:

```text
1. Enter TargetRangeTrial from Hub terminal.
2. Look at pistol pedestal.
3. Prompt says "Press E to equip Pistol."
4. Press E.
5. Pistol visual disappears from pedestal.
6. Pistol view model appears in player camera view.
7. Left click fires projectile from the weapon/fallback muzzle.
8. HUD updates shots fired.
9. Projectile hits target.
10. HUD updates hits, score, target count, and weapon XP.
11. Target logs health or gives visible feedback.
12. Destroying all targets shows "TRIAL COMPLETE - Press R to return to Hub."
13. Press R returns to Hub.
14. Hub terminal shows Target Range Trial completed.
15. Combat Trial unlocks.
```

---

# Suggested Commit

```bash
git add .
git commit -m "Add target range HUD and weapon pickup visuals"
```

Optional tag:

```bash
git tag milestone-1c-target-range-feedback
```

---

# Design Notes

The muzzle point should eventually live on the weapon.

For now:

```text
Player has fallback muzzle point.
Weapon view model can provide its own MuzzlePoint.
```

Later:

```text
Each weapon prefab owns its muzzle point.
WeaponData owns view model config.
PlayerWeaponController only asks the current weapon where to fire from.
```

The shell/lead projectile setup is fine for this stage:

```text
Projectile_PistolRound root = runtime projectile
Visuals/Shell + Visuals/Lead = art
```

Later, ejected casings should be split:

```text
Lead projectile = damage object
Shell casing = separate visual physics object ejected from weapon
```

That will keep ammo visuals expressive without tangling damage logic.

---

# Portfolio Value

This checkpoint demonstrates:

```text
Reusable HUD data binding
Event-driven tracker updates
Mission completion feedback
ScriptableObject weapon visuals
Interactable weapon pickup
Runtime weapon view model attachment
Projectile damage loop
Target scoring and XP hooks
Mission unlock progression
Modular player architecture direction
```

This phase turns the range from “I made a gun shoot” into:

```text
I built a weapon trial framework.
```
