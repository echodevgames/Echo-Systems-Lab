//-----TargetRangeMissionWeaponPedestal.cs START-----

using UnityEngine;

public class TargetRangeMissionWeaponPedestal : MonoBehaviour, IInteractable
{
    [Header("Weapon")]
    [SerializeField] private WeaponData weaponData;

    [Header("Visual")]
    [SerializeField] private GameObject weaponVisual;

    [Header("Prompt")]
    [SerializeField] private string armedPrompt = "Press E to start mission with";

    private TargetRangeMissionData armedMission;
    private bool isArmed;

    public WeaponData WeaponData => weaponData;

    private void Awake()
    {
        Disarm();
    }

    public void Arm(TargetRangeMissionData mission)
    {
        armedMission = mission;
        isArmed = true;

        if (weaponVisual != null)
            weaponVisual.SetActive(true);
    }

    public void Disarm()
    {
        armedMission = null;
        isArmed = false;

        if (weaponVisual != null)
            weaponVisual.SetActive(false);
    }

    public string GetPromptText()
    {
        if (!isArmed || weaponData == null)
            return "No mission weapon available";

        return $"{armedPrompt} {weaponData.displayName}";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!isArmed || armedMission == null || weaponData == null)
            return;

        PlayerWeaponController weaponController = interactor.GetComponent<PlayerWeaponController>();

        if (weaponController == null)
        {
            Debug.LogWarning("Interactor does not have PlayerWeaponController.");
            return;
        }

        weaponController.EquipTemporaryWeapon(weaponData);

        if (weaponVisual != null)
            weaponVisual.SetActive(false);

        isArmed = false;

        if (TargetRangeMissionController.Instance != null)
            TargetRangeMissionController.Instance.StartArmedMission();
    }
}

//-----TargetRangeMissionWeaponPedestal.cs END-----