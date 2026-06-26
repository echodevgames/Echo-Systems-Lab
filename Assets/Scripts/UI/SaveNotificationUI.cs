//-----SaveNotificationUI.cs START-----

using System.Collections;
using TMPro;
using UnityEngine;

public class SaveNotificationUI : MonoBehaviour
{
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private float showDuration = 1.5f;

    private Coroutine hideRoutine;

    private void Start()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameSaved += ShowSaved;
            SaveManager.Instance.OnGameLoaded += ShowLoaded;
        }
    }

    private void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnGameSaved -= ShowSaved;
            SaveManager.Instance.OnGameLoaded -= ShowLoaded;
        }
    }

    private void ShowSaved()
    {
        ShowMessage("Saved");
    }

    private void ShowLoaded()
    {
        ShowMessage("Loaded");
    }

    private void ShowMessage(string message)
    {
        if (notificationText == null)
            return;

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(showDuration);

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }
}

//-----SaveNotificationUI.cs END-----