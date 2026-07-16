//-----UIAudioBinder.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAudioBinder : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private UIAudioData uiAudioData;

    [Header("Binding")]
    [SerializeField] private bool bindOnAwake = true;
    [SerializeField] private bool includeInactiveChildren = true;

    [Header("Events")]
    [SerializeField] private bool bindButtons = true;
    [SerializeField] private bool bindSliders = true;
    [SerializeField] private bool bindToggles = true;
    [SerializeField] private bool bindDropdowns = true;

    [Header("Menu Open / Close")]
    [SerializeField] private bool playMenuOpenOnEnable = false;
    [SerializeField] private bool playMenuCloseOnDisable = false;

    [Header("Slider Throttle")]
    [SerializeField] private float sliderAudioInterval = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private bool hasBound;
    private bool hasPlayedOpenOnce;

    private void Awake()
    {
        if (bindOnAwake)
            Bind();
    }

    private void OnEnable()
    {
        if (!hasBound && bindOnAwake)
            Bind();

        if (playMenuOpenOnEnable && hasPlayedOpenOnce)
            PlayMenuOpen();

        hasPlayedOpenOnce = true;
    }

    private void OnDisable()
    {
        if (playMenuCloseOnDisable)
            PlayMenuClose();
    }

    [ContextMenu("Bind UI Audio")]
    public void Bind()
    {
        if (hasBound)
            return;

        if (uiAudioData == null)
        {
            Debug.LogWarning($"{name} has no UIAudioData assigned.");
            return;
        }

        if (bindButtons)
            BindAllButtons();

        if (bindSliders)
            BindAllSliders();

        if (bindToggles)
            BindAllToggles();

        if (bindDropdowns)
            BindAllDropdowns();

        hasBound = true;

        if (debugLogs)
            Debug.Log($"{name} bound UI audio.");
    }

    public void PlayMenuOpen()
    {
        if (uiAudioData != null)
            uiAudioData.PlayMenuOpenAudio();
    }

    public void PlayMenuClose()
    {
        if (uiAudioData != null)
            uiAudioData.PlayMenuCloseAudio();
    }

    public void PlayMenuBack()
    {
        if (uiAudioData != null)
            uiAudioData.PlayMenuBackAudio();
    }

    private void BindAllButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(includeInactiveChildren);

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            button.onClick.AddListener(() =>
            {
                if (uiAudioData != null)
                    uiAudioData.PlayButtonClickAudio();
            });

            AddHoverAudio(button.gameObject);
        }
    }

    private void BindAllSliders()
    {
        Slider[] sliders = GetComponentsInChildren<Slider>(includeInactiveChildren);

        foreach (Slider slider in sliders)
        {
            if (slider == null)
                continue;

            float nextAllowedAudioTime = 0f;

            slider.onValueChanged.AddListener(_ =>
            {
                if (Time.unscaledTime < nextAllowedAudioTime)
                    return;

                nextAllowedAudioTime = Time.unscaledTime + Mathf.Max(0f, sliderAudioInterval);

                if (uiAudioData != null)
                    uiAudioData.PlaySliderChangedAudio();
            });

            AddHoverAudio(slider.gameObject);
        }
    }

    private void BindAllToggles()
    {
        Toggle[] toggles = GetComponentsInChildren<Toggle>(includeInactiveChildren);

        foreach (Toggle toggle in toggles)
        {
            if (toggle == null)
                continue;

            toggle.onValueChanged.AddListener(_ =>
            {
                if (uiAudioData != null)
                    uiAudioData.PlayToggleChangedAudio();
            });

            AddHoverAudio(toggle.gameObject);
        }
    }

    private void BindAllDropdowns()
    {
        TMP_Dropdown[] tmpDropdowns = GetComponentsInChildren<TMP_Dropdown>(includeInactiveChildren);

        foreach (TMP_Dropdown dropdown in tmpDropdowns)
        {
            if (dropdown == null)
                continue;

            dropdown.onValueChanged.AddListener(_ =>
            {
                if (uiAudioData != null)
                    uiAudioData.PlayDropdownChangedAudio();
            });

            AddHoverAudio(dropdown.gameObject);
        }

        Dropdown[] legacyDropdowns = GetComponentsInChildren<Dropdown>(includeInactiveChildren);

        foreach (Dropdown dropdown in legacyDropdowns)
        {
            if (dropdown == null)
                continue;

            dropdown.onValueChanged.AddListener(_ =>
            {
                if (uiAudioData != null)
                    uiAudioData.PlayDropdownChangedAudio();
            });

            AddHoverAudio(dropdown.gameObject);
        }
    }

    private void AddHoverAudio(GameObject targetObject)
    {
        if (targetObject == null)
            return;

        EventTrigger eventTrigger = targetObject.GetComponent<EventTrigger>();

        if (eventTrigger == null)
            eventTrigger = targetObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };

        pointerEnterEntry.callback.AddListener(_ =>
        {
            if (uiAudioData != null)
                uiAudioData.PlayButtonHoverAudio();
        });

        eventTrigger.triggers.Add(pointerEnterEntry);
    }
}

//-----UIAudioBinder.cs END-----