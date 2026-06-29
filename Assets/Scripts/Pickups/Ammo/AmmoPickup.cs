//-----AmmoPickup.cs START-----

using UnityEngine;

public class AmmoPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private AmmoPickupData pickupData;
    [SerializeField] private string promptPrefix = "Press E to pick up";

    public string GetPromptText()
    {
        if (pickupData == null)
            return "No ammo assigned";

        return $"{promptPrefix} {pickupData.displayName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (pickupData == null)
        {
            Debug.LogWarning($"{name} has no AmmoPickupData assigned.");
            return;
        }

        if (pickupData.ammoData == null)
        {
            Debug.LogWarning($"{pickupData.displayName} has no AmmoData assigned.");
            return;
        }

        if (interactor == null)
        {
            Debug.LogWarning($"{name} was interacted with, but no PlayerInteractor was provided.");
            return;
        }

        PlayerAmmoInventory ammoInventory = interactor.GetComponent<PlayerAmmoInventory>();

        if (ammoInventory == null)
        {
            Debug.LogWarning("Interactor does not have PlayerAmmoInventory.");
            return;
        }

        ammoInventory.AddAmmo(pickupData.ammoData, pickupData.amount);

        Debug.Log($"Picked up {pickupData.amount} {pickupData.ammoData.displayName} from {pickupData.displayName}.");

        if (pickupData.destroyOnPickup)
            Destroy(gameObject);
    }
}

//-----AmmoPickup.cs END-----