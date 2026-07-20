//-----AudioSettingsMenuUI.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsMenuUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject settingsRoot;

    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider weaponsVolumeSlider;
    [SerializeField] private Slider uiVolumeSlider;
    [SerializeField] private Slider ambienceVolumeSlider;

    [Header("Value Labels")]
    [SerializeField] private TMP_Text masterVolumeValueText;
    [SerializeField] private TMP_Text musicVolumeValueText;
    [SerializeField] private TMP_Text sfxVolumeValueText;
    [SerializeField] private TMP_Text weaponsVolumeValueText;
    [SerializeField] private TMP_Text uiVolumeValueText;
    [SerializeField] private TMP_Text ambienceVolumeValueText;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button resetDefaultsButton;

    [Header("References")]
    [SerializeField] private AudioSettingsManager audioSettingsManager;

    private GameObject returnPanel;
    private bool hasInitialized;

    private void Awake()
    {
        Initialize();

        if (settingsRoot != null)
            settingsRoot.SetActive(false);
    }

    private void OnEnable()
    {
        Initialize();

        if (audioSettingsManager != null)
            audioSettingsManager.OnAudioSettingsChanged += RefreshValueLabels;
    }

    private void OnDisable()
    {
        if (audioSettingsManager != null)
            audioSettingsManager.OnAudioSettingsChanged -= RefreshValueLabels;
    }

    public void Open()
    {
        OpenFrom(null);
    }

    public void OpenFrom(GameObject panelToReturnTo)
    {
        Initialize();

        returnPanel = panelToReturnTo;

        if (returnPanel != null)
            returnPanel.SetActive(false);

        if (settingsRoot != null)
            settingsRoot.SetActive(true);

        SyncSlidersToSettings();
        RefreshValueLabels();
    }

    public void Close()
    {
        ForceClose(true);
    }

    public void ForceClose(bool restoreReturnPanel)
    {
        if (settingsRoot != null)
            settingsRoot.SetActive(false);

        if (restoreReturnPanel && returnPanel != null)
            returnPanel.SetActive(true);

        returnPanel = null;
    }

    private void Initialize()
    {
        if (hasInitialized)
            return;

        if (audioSettingsManager == null)
            audioSettingsManager = AudioSettingsManager.Instance;

        if (audioSettingsManager == null)
            audioSettingsManager = FindFirstObjectByType<AudioSettingsManager>();

        HookSlider(masterVolumeSlider, HandleMasterVolumeChanged);
        HookSlider(musicVolumeSlider, HandleMusicVolumeChanged);
        HookSlider(sfxVolumeSlider, HandleSfxVolumeChanged);
        HookSlider(weaponsVolumeSlider, HandleWeaponsVolumeChanged);
        HookSlider(uiVolumeSlider, HandleUiVolumeChanged);
        HookSlider(ambienceVolumeSlider, HandleAmbienceVolumeChanged);

        if (backButton != null)
            backButton.onClick.AddListener(Close);

        if (resetDefaultsButton != null)
            resetDefaultsButton.onClick.AddListener(ResetDefaults);

        hasInitialized = true;
    }

    private void HookSlider(Slider slider, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        slider.onValueChanged.AddListener(callback);
    }

    private void SyncSlidersToSettings()
    {
        if (audioSettingsManager == null)
        {
            Debug.LogWarning("AudioSettingsMenuUI could not find AudioSettingsManager.");
            return;
        }

        SetSliderWithoutNotify(masterVolumeSlider, audioSettingsManager.MasterVolume);
        SetSliderWithoutNotify(musicVolumeSlider, audioSettingsManager.MusicVolume);
        SetSliderWithoutNotify(sfxVolumeSlider, audioSettingsManager.SfxVolume);
        SetSliderWithoutNotify(weaponsVolumeSlider, audioSettingsManager.WeaponsVolume);
        SetSliderWithoutNotify(uiVolumeSlider, audioSettingsManager.UiVolume);
        SetSliderWithoutNotify(ambienceVolumeSlider, audioSettingsManager.AmbienceVolume);
    }

    private void SetSliderWithoutNotify(Slider slider, float value)
    {
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    private void HandleMasterVolumeChanged(float value)
    {
        if (audioSettingsManager != null)
            audioSettingsManager.SetMasterVolume(value);

        SetPercentText(masterVolumeValueText, value);
    }

    private void HandleMusicVolumeChanged(float value)
    {
        if (audioSettingsManager != null)
            audioSettingsManager.SetMusicVolume(value);

        SetPercentText(musicVolumeValueText, value);
    }

    private void HandleSfxVolumeChanged(float value)
    {
        if (audioSettingsManager != null)
            audioSettingsManager.SetSfxVolume(value);

        SetPercentText(sfxVolumeValueText, value);
    }

    private void HandleWeaponsVolumeChanged(float value)
    {
        if (audioSettingsManager != null)
            audioSettingsManager.SetWeaponsVolume(value);

        SetPercentText(weaponsVolumeValueText, value);
    }

    private void HandleUiVolumeChanged(float value)
    {
        if (audioSettingsManager != null)
            audioSettingsManager.SetUiVolume(value);

        SetPercentText(uiVolumeValueText, value);
    }

    private void HandleAmbienceVolumeChanged(float value)
    {
        if (audioSettingsManager != null)
            audioSettingsManager.SetAmbienceVolume(value);

        SetPercentText(ambienceVolumeValueText, value);
    }

    private void ResetDefaults()
    {
        if (audioSettingsManager == null)
            return;

        audioSettingsManager.ResetToDefaults();
        SyncSlidersToSettings();
        RefreshValueLabels();
    }

    private void RefreshValueLabels()
    {
        if (audioSettingsManager == null)
            return;

        SetPercentText(masterVolumeValueText, audioSettingsManager.MasterVolume);
        SetPercentText(musicVolumeValueText, audioSettingsManager.MusicVolume);
        SetPercentText(sfxVolumeValueText, audioSettingsManager.SfxVolume);
        SetPercentText(weaponsVolumeValueText, audioSettingsManager.WeaponsVolume);
        SetPercentText(uiVolumeValueText, audioSettingsManager.UiVolume);
        SetPercentText(ambienceVolumeValueText, audioSettingsManager.AmbienceVolume);
    }

    private void SetPercentText(TMP_Text text, float value)
    {
        if (text == null)
            return;

        int percent = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
        text.text = $"{percent}%";
    }
}

//-----AudioSettingsMenuUI.cs END-----