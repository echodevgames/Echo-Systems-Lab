//-----WeaponBandoolierHUD.cs START-----


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBandolierHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bandolierRoot;
    [SerializeField] private Transform slotParent;
    [SerializeField] private WeaponBandolierSlotUI slotPrefab;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerWeaponController weaponController;

    private readonly List<WeaponBandolierSlotUI> spawnedSlots = new List<WeaponBandolierSlotUI>();
    private bool isVisible;

    private void Awake()
    {
        if (inputReader == null)
            inputReader = FindFirstObjectByType<PlayerInputReader>();

        if (weaponController == null)
            weaponController = FindFirstObjectByType<PlayerWeaponController>();

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (weaponController != null)
            weaponController.OnWeaponChanged += HandleWeaponChanged;
    }

    private void OnDisable()
    {
        if (weaponController != null)
            weaponController.OnWeaponChanged -= HandleWeaponChanged;
    }

    private void Update()
    {
        if (inputReader == null)
            return;

        bool shouldShow = inputReader.BandolierHeld;

        if (shouldShow != isVisible)
        {
            SetVisible(shouldShow);

            if (shouldShow)
                Refresh();
        }

        if (isVisible)
            RefreshSelectionOnly();
    }

    private void HandleWeaponChanged(WeaponData weaponData)
    {
        if (isVisible)
            Refresh();
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;

        if (bandolierRoot != null)
            bandolierRoot.SetActive(visible);
    }

    private void Refresh()
    {
        ClearSlots();

        WeaponDatabase database = SaveManager.Instance != null
            ? SaveManager.Instance.WeaponDatabase
            : null;

        if (database == null || slotParent == null || slotPrefab == null)
            return;

        List<WeaponData> ownedWeapons = database.GetOwnedWeaponsInDatabaseOrder();
        string activeWeaponId = PlayerProgress.ActiveWeaponId;

        foreach (WeaponData weapon in ownedWeapons)
        {
            if (weapon == null)
                continue;

            WeaponBandolierSlotUI slot = Instantiate(slotPrefab, slotParent);
            bool isSelected = weapon.weaponId == activeWeaponId;
            slot.Setup(weapon, isSelected);
            spawnedSlots.Add(slot);
        }

        if (slotParent is RectTransform rectTransform)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void RefreshSelectionOnly()
    {
        WeaponDatabase database = SaveManager.Instance != null
            ? SaveManager.Instance.WeaponDatabase
            : null;

        if (database == null)
            return;

        List<WeaponData> ownedWeapons = database.GetOwnedWeaponsInDatabaseOrder();
        string activeWeaponId = PlayerProgress.ActiveWeaponId;

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (i >= ownedWeapons.Count || ownedWeapons[i] == null)
                continue;

            spawnedSlots[i].SetSelected(ownedWeapons[i].weaponId == activeWeaponId);
        }
    }

    private void ClearSlots()
    {
        spawnedSlots.Clear();

        if (slotParent == null)
            return;

        for (int i = slotParent.childCount - 1; i >= 0; i--)
            Destroy(slotParent.GetChild(i).gameObject);
    }
}
//-----WeaponBandolierHUD.cs END-----