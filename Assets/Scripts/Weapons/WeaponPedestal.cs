//-----WeaponPedestal.cs START-----

using UnityEngine;

public class WeaponPedestal : MonoBehaviour, IInteractable
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private string promptPrefix = "Press E to equip";

    public string GetPromptText()
    {
        if (weaponData == null)
            return "No weapon assigned";

        return $"{promptPrefix} {weaponData.displayName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
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
    }
}

//-----WeaponPedestal.cs END-----