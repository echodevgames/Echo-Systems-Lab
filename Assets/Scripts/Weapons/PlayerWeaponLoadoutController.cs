//-----PlayerWeaponLoadoutController.cs START-----


using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponLoadoutController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponController weaponController;

    [Header("Input")]
    [SerializeField] private KeyCode cycleNextKey = KeyCode.Tab;

    private bool inputEnabled = true;

    private void Awake()
    {
        if (weaponController == null)
            weaponController = GetComponent<PlayerWeaponController>();
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (Input.GetKeyDown(cycleNextKey))
            CycleNextWeapon();
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void CycleNextWeapon()
    {
        if (SaveManager.Instance == null)
            return;

        WeaponDatabase database = SaveManager.Instance.WeaponDatabase;

        if (database == null)
        {
            Debug.LogWarning("No WeaponDatabase assigned to SaveManager.");
            return;
        }

        List<WeaponData> ownedWeapons = database.GetOwnedWeaponsInDatabaseOrder();

        if (ownedWeapons.Count <= 0)
        {
            Debug.Log("No owned weapons to cycle.");
            return;
        }

        int currentIndex = GetCurrentWeaponIndex(ownedWeapons);
        int nextIndex = (currentIndex + 1) % ownedWeapons.Count;

        WeaponData nextWeapon = ownedWeapons[nextIndex];

        if (weaponController != null)
            weaponController.EquipWeapon(nextWeapon);
    }

    private int GetCurrentWeaponIndex(List<WeaponData> ownedWeapons)
    {
        string activeWeaponId = PlayerProgress.ActiveWeaponId;

        for (int i = 0; i < ownedWeapons.Count; i++)
        {
            if (ownedWeapons[i] == null)
                continue;

            if (ownedWeapons[i].weaponId == activeWeaponId)
                return i;
        }

        return -1;
    }
}

//-----PlayerWeaponLoadoutController.cs END-----