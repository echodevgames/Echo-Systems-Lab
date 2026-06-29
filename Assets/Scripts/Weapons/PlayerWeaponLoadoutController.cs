//-----PlayerWeaponLoadoutController.cs START-----

using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponLoadoutController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerInputReader inputReader;

    private bool inputEnabled = true;

    private void Awake()
    {
        if (weaponController == null)
            weaponController = GetComponent<PlayerWeaponController>();

        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (inputReader == null)
            return;

        if (inputReader.CycleNextWeaponPressed)
            CycleWeapon(1);

        if (inputReader.CyclePreviousWeaponPressed)
            CycleWeapon(-1);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void CycleNextWeapon()
    {
        CycleWeapon(1);
    }

    public void CyclePreviousWeapon()
    {
        CycleWeapon(-1);
    }

    private void CycleWeapon(int direction)
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

        if (currentIndex < 0)
            currentIndex = 0;

        int nextIndex = currentIndex + direction;

        if (nextIndex >= ownedWeapons.Count)
            nextIndex = 0;

        if (nextIndex < 0)
            nextIndex = ownedWeapons.Count - 1;

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