//-----WeaponPedestal.cs START-----

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

//-----WeaponPedestal.cs END-----