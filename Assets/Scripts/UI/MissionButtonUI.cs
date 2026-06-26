//-----MissionButtonUI.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button button;

    private MissionData mission;

    public void Setup(MissionData missionData, bool unlocked, bool completed, System.Action<MissionData> onClicked)
    {
        mission = missionData;

        if (titleText != null)
            titleText.text = mission.displayName;

        if (descriptionText != null)
            descriptionText.text = mission.description;

        if (statusText != null)
        {
            if (completed)
                statusText.text = "COMPLETED";
            else if (unlocked)
                statusText.text = "AVAILABLE";
            else
                statusText.text = "LOCKED";
        }

        if (button != null)
        {
            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClicked?.Invoke(mission));
        }
    }
}
//-----MissionButtonUI.cs END-----