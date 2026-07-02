//-----PlayerWeaponDropController.cs START-----
using UnityEngine;

public class PlayerWeaponDropController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform dropPoint;

    [Header("Drop Prefab")]
    [SerializeField] private GameObject weaponPickupPrefab;

    [Header("Drop Settings")]
    [SerializeField] private float dropForwardOffset = 1.25f;
    [SerializeField] private float dropUpOffset = 0.25f;
    [SerializeField] private float dropForce = 3f;
    [SerializeField] private float dropUpForceMultiplier = 0.35f;

    private void Awake()
    {
        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();

        if (weaponController == null)
            weaponController = GetComponent<PlayerWeaponController>();

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (inputReader == null)
            return;

        if (!inputReader.DropPressed)
            return;

        DropCurrentWeapon();
    }

    public void DropCurrentWeapon()
    {
        if (weaponController == null)
            return;

        if (!weaponController.HasWeapon)
            return;

        if (weaponPickupPrefab == null)
        {
            Debug.LogWarning($"{nameof(PlayerWeaponDropController)} has no weapon pickup prefab assigned.");
            return;
        }

        WeaponData weaponToDrop = weaponController.CurrentWeapon;

        Vector3 spawnPosition = GetDropPosition();
        Quaternion spawnRotation = GetDropRotation();

        GameObject pickupObject = Instantiate(weaponPickupPrefab, spawnPosition, spawnRotation);

        WeaponPickup weaponPickup = pickupObject.GetComponent<WeaponPickup>();

        if (weaponPickup != null)
        {
            weaponPickup.Initialize(weaponToDrop);
        }
        else
        {
            Debug.LogWarning($"{weaponPickupPrefab.name} does not have a WeaponPickup component.");
        }

        Rigidbody pickupBody = pickupObject.GetComponent<Rigidbody>();

        if (pickupBody != null)
        {
            Vector3 forceDirection = GetDropForwardDirection();
            forceDirection += Vector3.up * dropUpForceMultiplier;
            forceDirection.Normalize();

            pickupBody.AddForce(forceDirection * dropForce, ForceMode.Impulse);
        }

        string weaponId = GetWeaponId(weaponToDrop);

        if (!string.IsNullOrWhiteSpace(weaponId))
            PlayerProgress.RemoveOwnedWeapon(weaponId);

        weaponController.UnequipCurrentWeapon(false);

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    private Vector3 GetDropPosition()
    {
        if (dropPoint != null)
            return dropPoint.position;

        Transform origin = playerCamera != null ? playerCamera.transform : transform;

        return origin.position +
               origin.forward * dropForwardOffset +
               Vector3.up * dropUpOffset;
    }

    private Quaternion GetDropRotation()
    {
        Vector3 forward = GetDropForwardDirection();

        if (forward.sqrMagnitude <= 0.001f)
            forward = transform.forward;

        return Quaternion.LookRotation(forward, Vector3.up);
    }

    private Vector3 GetDropForwardDirection()
    {
        if (playerCamera != null)
            return playerCamera.transform.forward;

        return transform.forward;
    }

    private string GetWeaponId(WeaponData weaponData)
    {
        if (weaponData == null)
            return string.Empty;

        // IMPORTANT:
        // Replace this with your actual WeaponData ID field if you have one.
        // Example:
        // return weaponData.weaponId;

        return weaponData.weaponId;
    }
}

//-----PlayerWeaponDropController.cs END-----