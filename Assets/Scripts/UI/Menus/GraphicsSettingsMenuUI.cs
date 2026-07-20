//-----GraphicsSettingsMenuUI.cs START-----

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettingsMenuUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject graphicsRoot;

    [Header("Graphics Controls")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle windowedToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private TMP_Dropdown targetFrameRateDropdown;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Slider uiScaleSlider;


    [Header("Optional Value Labels")]
    [SerializeField] private TMP_Text resolutionValueText;
    [SerializeField] private TMP_Text windowModeValueText;
    [SerializeField] private TMP_Text qualityValueText;
    [SerializeField] private TMP_Text vSyncValueText;
    [SerializeField] private TMP_Text targetFrameRateValueText;
    [SerializeField] private TMP_Text brightnessValueText;
    [SerializeField] private TMP_Text uiScaleValueText;
    [SerializeField] private TMP_Text statusText;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button resetDefaultsButton;

    [Header("References")]
    [SerializeField] private GraphicsSettingsManager graphicsSettingsManager;
    [SerializeField] private UIScaleApplier uiScaleApplier;

    [Header("Target FPS Options")]
    [Tooltip("-1 means uncapped. If VSync is enabled, Unity may ignore target FPS.")]
    [SerializeField] private int[] targetFrameRateOptions = { -1, 30, 60, 120, 144 };

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private readonly List<int> frameRateOptions = new List<int>();

    private GameObject returnPanel;
    private bool hasInitialized;
    private bool isSyncingControls;

    private int pendingResolutionIndex;
    private bool pendingWindowed;
    private int pendingQualityLevel;
    private bool pendingVSync;
    private int pendingTargetFrameRate;
    private float pendingBrightness;
    private float pendingUiScale;

    private void Awake()
    {
        Initialize();

        if (graphicsRoot != null)
            graphicsRoot.SetActive(false);
    }

    private void OnEnable()
    {
        Initialize();

        if (graphicsSettingsManager != null)
            graphicsSettingsManager.OnGraphicsSettingsChanged += HandleGraphicsSettingsChanged;
    }

    private void OnDisable()
    {
        if (graphicsSettingsManager != null)
            graphicsSettingsManager.OnGraphicsSettingsChanged -= HandleGraphicsSettingsChanged;
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

        if (graphicsRoot != null)
            graphicsRoot.SetActive(true);

        SyncControlsToSettings();


        SetStatus("Graphics settings loaded.");
    }

    public void Close()
    {
        ForceClose(true);
    }

    public void ForceClose(bool restoreReturnPanel)
    {
        if (uiScaleApplier != null)
            uiScaleApplier.RevertToSavedScale();

        if (graphicsRoot != null)
            graphicsRoot.SetActive(false);

        if (restoreReturnPanel && returnPanel != null)
            returnPanel.SetActive(true);

        returnPanel = null;
    }

    private void Initialize()
    {
        if (hasInitialized)
            return;

        if (graphicsSettingsManager == null)
            graphicsSettingsManager = GraphicsSettingsManager.Instance;

        if (graphicsSettingsManager == null)
            graphicsSettingsManager = FindFirstObjectByType<GraphicsSettingsManager>();

        if (uiScaleApplier == null)
            uiScaleApplier = FindFirstObjectByType<UIScaleApplier>();

        BuildTargetFrameRateOptions();
        BuildDropdownOptions();
        ConfigureSliderRanges();

        HookDropdown(resolutionDropdown, HandleResolutionChanged);
        HookToggle(windowedToggle, HandleWindowedChanged);
        HookDropdown(qualityDropdown, HandleQualityChanged);
        HookToggle(vSyncToggle, HandleVSyncChanged);
        HookDropdown(targetFrameRateDropdown, HandleTargetFrameRateChanged);
        HookSlider(brightnessSlider, HandleBrightnessChanged);
        HookSlider(uiScaleSlider, HandleUiScaleChanged);

        if (applyButton != null)
            applyButton.onClick.AddListener(ApplyPendingSettings);
        if (uiScaleApplier != null)
            uiScaleApplier.ApplySavedScale();
        if (backButton != null)
            backButton.onClick.AddListener(Close);

        if (resetDefaultsButton != null)
            resetDefaultsButton.onClick.AddListener(ResetDefaults);

        hasInitialized = true;
    }

    private void BuildDropdownOptions()
    {
        BuildResolutionDropdownOptions();
        BuildQualityDropdownOptions();
        BuildTargetFrameRateDropdownOptions();
    }

    private void BuildResolutionDropdownOptions()
    {
        if (resolutionDropdown == null)
            return;

        resolutionDropdown.ClearOptions();

        if (graphicsSettingsManager == null)
        {
            resolutionDropdown.AddOptions(new List<string> { "No Graphics Manager" });
            return;
        }

        graphicsSettingsManager.RefreshResolutionOptions();

        List<string> labels = graphicsSettingsManager.GetResolutionLabels();

        if (labels == null || labels.Count == 0)
            labels = new List<string> { $"{Screen.width} x {Screen.height}" };

        resolutionDropdown.AddOptions(labels);
    }

    private void BuildQualityDropdownOptions()
    {
        if (qualityDropdown == null)
            return;

        qualityDropdown.ClearOptions();

        string[] qualityNames = QualitySettings.names;
        List<string> options = new List<string>();

        if (qualityNames != null && qualityNames.Length > 0)
        {
            options.AddRange(qualityNames);
        }
        else
        {
            options.Add("Default");
        }

        qualityDropdown.AddOptions(options);
    }

    private void BuildTargetFrameRateOptions()
    {
        frameRateOptions.Clear();

        if (targetFrameRateOptions != null)
        {
            for (int i = 0; i < targetFrameRateOptions.Length; i++)
            {
                AddFrameRateOptionIfMissing(targetFrameRateOptions[i]);
            }
        }

        AddFrameRateOptionIfMissing(-1);

        frameRateOptions.Sort((a, b) =>
        {
            if (a < 0 && b >= 0)
                return -1;

            if (a >= 0 && b < 0)
                return 1;

            return a.CompareTo(b);
        });
    }

    private void BuildTargetFrameRateDropdownOptions()
    {
        if (targetFrameRateDropdown == null)
            return;

        targetFrameRateDropdown.ClearOptions();

        List<string> labels = new List<string>();

        for (int i = 0; i < frameRateOptions.Count; i++)
        {
            labels.Add(GetFrameRateLabel(frameRateOptions[i]));
        }

        targetFrameRateDropdown.AddOptions(labels);
    }

    private void ConfigureSliderRanges()
    {
        if (graphicsSettingsManager == null)
            return;

        ConfigureSlider(
            brightnessSlider,
            graphicsSettingsManager.MinimumBrightness,
            graphicsSettingsManager.MaximumBrightness);

        ConfigureSlider(
            uiScaleSlider,
            graphicsSettingsManager.MinimumUiScale,
            graphicsSettingsManager.MaximumUiScale);
    }

    private void ConfigureSlider(Slider slider, float minValue, float maxValue)
    {
        if (slider == null)
            return;

        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = false;
    }

    private void HookDropdown(TMP_Dropdown dropdown, UnityEngine.Events.UnityAction<int> callback)
    {
        if (dropdown == null)
            return;

        dropdown.onValueChanged.AddListener(callback);
    }

    private void HookToggle(Toggle toggle, UnityEngine.Events.UnityAction<bool> callback)
    {
        if (toggle == null)
            return;

        toggle.onValueChanged.AddListener(callback);
    }

    private void HookSlider(Slider slider, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null)
            return;

        slider.onValueChanged.AddListener(callback);
    }

    private void SyncControlsToSettings()
    {
        if (graphicsSettingsManager == null)
        {
            Debug.LogWarning("GraphicsSettingsMenuUI could not find GraphicsSettingsManager.");
            SetStatus("Graphics manager missing.");
            return;
        }

        isSyncingControls = true;

        BuildDropdownOptions();
        ConfigureSliderRanges();

        pendingResolutionIndex = graphicsSettingsManager.GetCurrentResolutionIndex();
        pendingWindowed = graphicsSettingsManager.Windowed;
        pendingQualityLevel = graphicsSettingsManager.QualityLevel;
        pendingVSync = graphicsSettingsManager.VSyncEnabled;
        pendingTargetFrameRate = graphicsSettingsManager.TargetFrameRate;
        pendingBrightness = graphicsSettingsManager.Brightness;
        pendingUiScale = graphicsSettingsManager.UiScale;

        EnsureFrameRateOptionExists(pendingTargetFrameRate);

        SetDropdownWithoutNotify(resolutionDropdown, pendingResolutionIndex);
        SetToggleWithoutNotify(windowedToggle, pendingWindowed);
        SetDropdownWithoutNotify(qualityDropdown, pendingQualityLevel);
        SetToggleWithoutNotify(vSyncToggle, pendingVSync);
        SetDropdownWithoutNotify(targetFrameRateDropdown, GetFrameRateDropdownIndex(pendingTargetFrameRate));
        SetSliderWithoutNotify(brightnessSlider, pendingBrightness);
        SetSliderWithoutNotify(uiScaleSlider, pendingUiScale);

        if (uiScaleApplier != null)
            uiScaleApplier.PreviewScale(pendingUiScale);

        isSyncingControls = false;

        RefreshValueLabels();
    }

    private void HandleGraphicsSettingsChanged()
    {
        if (graphicsRoot != null && !graphicsRoot.activeInHierarchy)
            return;

        SyncControlsToSettings();
        SetStatus("Graphics settings updated.");
    }

    private void HandleResolutionChanged(int value)
    {
        if (isSyncingControls)
            return;

        pendingResolutionIndex = value;
        RefreshValueLabels();
        SetStatus("Pending graphics changes.");
    }

    private void HandleWindowedChanged(bool value)
    {
        if (isSyncingControls)
            return;

        pendingWindowed = value;
        RefreshValueLabels();
        SetStatus("Pending graphics changes.");
    }

    private void HandleQualityChanged(int value)
    {
        if (isSyncingControls)
            return;

        pendingQualityLevel = value;
        RefreshValueLabels();
        SetStatus("Pending graphics changes.");
    }

    private void HandleVSyncChanged(bool value)
    {
        if (isSyncingControls)
            return;

        pendingVSync = value;
        RefreshValueLabels();
        SetStatus(value
            ? "Pending graphics changes. VSync may override FPS cap."
            : "Pending graphics changes.");
    }

    private void HandleTargetFrameRateChanged(int value)
    {
        if (isSyncingControls)
            return;

        pendingTargetFrameRate = GetFrameRateForDropdownIndex(value);
        RefreshValueLabels();

        if (pendingVSync)
            SetStatus("Pending graphics changes. VSync may override FPS cap.");
        else
            SetStatus("Pending graphics changes.");
    }

    private void HandleBrightnessChanged(float value)
    {
        if (isSyncingControls)
            return;

        pendingBrightness = value;
        RefreshValueLabels();
        SetStatus("Pending graphics changes.");
    }

    private void HandleUiScaleChanged(float value)
    {
        if (isSyncingControls)
            return;

        pendingUiScale = value;

        if (uiScaleApplier != null)
            uiScaleApplier.PreviewScale(pendingUiScale);

        RefreshValueLabels();
        SetStatus("Pending graphics changes.");
    }
    private void ApplyPendingSettings()
    {
        if (graphicsSettingsManager == null)
        {
            Debug.LogWarning("Cannot apply graphics settings because GraphicsSettingsManager is missing.");
            SetStatus("Graphics manager missing.");
            return;
        }

        graphicsSettingsManager.ApplySettings(
            pendingResolutionIndex,
            pendingWindowed,
            pendingQualityLevel,
            pendingVSync,
            pendingTargetFrameRate,
            pendingBrightness,
            pendingUiScale);

        SyncControlsToSettings();

        if (uiScaleApplier != null)
            uiScaleApplier.ApplySavedScale();

        SetStatus("Graphics settings applied.");

        if (debugLogs)
            Debug.Log("GraphicsSettingsMenuUI applied pending settings.");
    }

    private void ResetDefaults()
    {
        if (graphicsSettingsManager == null)
        {
            Debug.LogWarning("Cannot reset graphics settings because GraphicsSettingsManager is missing.");
            SetStatus("Graphics manager missing.");
            return;
        }

        graphicsSettingsManager.ResetToDefaults();
        SyncControlsToSettings();

        if (uiScaleApplier != null)
            uiScaleApplier.ApplySavedScale();

        SetStatus("Graphics settings reset.");
    }

    private void RefreshValueLabels()
    {
        SetText(resolutionValueText, GetPendingResolutionLabel());
        SetText(windowModeValueText, pendingWindowed ? "Windowed" : "Fullscreen");
        SetText(qualityValueText, GetPendingQualityLabel());
        SetText(vSyncValueText, pendingVSync ? "On" : "Off");
        SetText(targetFrameRateValueText, GetFrameRateLabel(pendingTargetFrameRate));
        SetPercentText(brightnessValueText, pendingBrightness);
        SetPercentText(uiScaleValueText, pendingUiScale);

        RefreshControlAvailability();
    }

    private void RefreshControlAvailability()
    {
        if (resolutionDropdown == null)
            return;

        bool shouldLockResolution =
            graphicsSettingsManager != null &&
            graphicsSettingsManager.ForceNativeResolutionWhenFullscreen &&
            !pendingWindowed;

        resolutionDropdown.interactable = !shouldLockResolution;
    }

    private string GetPendingResolutionLabel()
    {
        if (graphicsSettingsManager == null)
            return "Unknown";

        Vector2Int resolution = graphicsSettingsManager.GetResolutionAtIndex(pendingResolutionIndex);
        return $"{resolution.x} x {resolution.y}";
    }

    private string GetPendingQualityLabel()
    {
        string[] qualityNames = QualitySettings.names;

        if (qualityNames == null || qualityNames.Length == 0)
            return "Default";

        int index = Mathf.Clamp(pendingQualityLevel, 0, qualityNames.Length - 1);
        return qualityNames[index];
    }

    private string GetFrameRateLabel(int frameRate)
    {
        if (frameRate < 0)
            return "Unlimited";

        return $"{frameRate} FPS";
    }

    private int GetFrameRateDropdownIndex(int frameRate)
    {
        for (int i = 0; i < frameRateOptions.Count; i++)
        {
            if (frameRateOptions[i] == frameRate)
                return i;
        }

        return 0;
    }

    private int GetFrameRateForDropdownIndex(int index)
    {
        if (frameRateOptions.Count == 0)
            return -1;

        index = Mathf.Clamp(index, 0, frameRateOptions.Count - 1);
        return frameRateOptions[index];
    }

    private void EnsureFrameRateOptionExists(int frameRate)
    {
        if (AddFrameRateOptionIfMissing(frameRate))
        {
            frameRateOptions.Sort((a, b) =>
            {
                if (a < 0 && b >= 0)
                    return -1;

                if (a >= 0 && b < 0)
                    return 1;

                return a.CompareTo(b);
            });

            BuildTargetFrameRateDropdownOptions();
        }
    }

    private bool AddFrameRateOptionIfMissing(int frameRate)
    {
        for (int i = 0; i < frameRateOptions.Count; i++)
        {
            if (frameRateOptions[i] == frameRate)
                return false;
        }

        frameRateOptions.Add(frameRate);
        return true;
    }

    private void SetDropdownWithoutNotify(TMP_Dropdown dropdown, int value)
    {
        if (dropdown == null)
            return;

        if (dropdown.options == null || dropdown.options.Count == 0)
            return;

        value = Mathf.Clamp(value, 0, dropdown.options.Count - 1);
        dropdown.SetValueWithoutNotify(value);
        dropdown.RefreshShownValue();
    }

    private void SetToggleWithoutNotify(Toggle toggle, bool value)
    {
        if (toggle == null)
            return;

        toggle.SetIsOnWithoutNotify(value);
    }

    private void SetSliderWithoutNotify(Slider slider, float value)
    {
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(value);
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text == null)
            return;

        text.text = value;
    }

    private void SetPercentText(TMP_Text text, float value)
    {
        if (text == null)
            return;

        int percent = Mathf.RoundToInt(value * 100f);
        text.text = $"{percent}%";
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        if (debugLogs && !string.IsNullOrWhiteSpace(message))
            Debug.Log(message);
    }
}

//-----GraphicsSettingsMenuUI.cs END-----