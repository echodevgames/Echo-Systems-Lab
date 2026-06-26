//-----WeaponPedestal.cs START-----

using UnityEngine;

public class WeaponPedestal : MonoBehaviour, IInteractable
{
    [Header("Weapon")]
    [SerializeField] private WeaponData weaponData;

    [Header("Pedestal Visual")]
    [SerializeField] private GameObject pedestalWeaponVisual;
    [SerializeField] private bool hideVisualWhenOwned = true;
    [SerializeField] private bool allowReequipIfOwned = true;

    [Header("Prompt")]
    [SerializeField] private string promptPrefix = "Press E to equip";

    private void Start()
    {
        RefreshVisualState();
    }

    public string GetPromptText()
    {
        if (weaponData == null)
            return "No weapon assigned";

        if (PlayerProgress.OwnsWeapon(weaponData.weaponId))
        {
            if (allowReequipIfOwned)
                return $"Press E to equip owned {weaponData.displayName}";

            return $"{weaponData.displayName} owned";
        }

        return $"{promptPrefix} {weaponData.displayName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"{name} has no WeaponData assigned.");
            return;
        }

        if (PlayerProgress.OwnsWeapon(weaponData.weaponId) && !allowReequipIfOwned)
            return;

        PlayerWeaponController weaponController = interactor.GetComponent<PlayerWeaponController>();

        if (weaponController == null)
        {
            Debug.LogWarning("Interactor does not have PlayerWeaponController.");
            return;
        }

        weaponController.EquipWeapon(weaponData);

        RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        if (weaponData == null)
            return;

        bool ownsWeapon = PlayerProgress.OwnsWeapon(weaponData.weaponId);

        if (hideVisualWhenOwned && pedestalWeaponVisual != null)
            pedestalWeaponVisual.SetActive(!ownsWeapon);
    }
}

//-----WeaponPedestal.cs END-----