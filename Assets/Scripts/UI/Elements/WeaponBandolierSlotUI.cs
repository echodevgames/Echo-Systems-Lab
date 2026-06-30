//-----WeaponBandolierSlotUI.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBandolierSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject selectedRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    public void Setup(WeaponData weaponData, bool isSelected)
    {
        if (weaponData == null)
            return;

        if (iconImage != null)
        {
            iconImage.sprite = weaponData.weaponIcon;
            iconImage.enabled = weaponData.weaponIcon != null;
        }

        if (nameText != null)
        {
            string displayName = !string.IsNullOrWhiteSpace(weaponData.hudDisplayName)
                ? weaponData.hudDisplayName
                : weaponData.displayName;

            nameText.text = displayName;
        }

        SetSelected(isSelected);
    }

    public void SetSelected(bool selected)
    {
        if (selectedRoot != null)
            selectedRoot.SetActive(selected);

        if (canvasGroup != null)
            canvasGroup.alpha = selected ? 1f : 0.55f;
    }
}

//-----WeaponBandolierSlotUI.cs END-----