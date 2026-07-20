//-----GraphicsSettingsManager.cs START-----

using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsSettingsManager : MonoBehaviour
{
    public static GraphicsSettingsManager Instance { get; private set; }

    public event Action OnGraphicsSettingsChanged;

    [Header("Lifetime")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Defaults")]
    [Tooltip("If true, the resolution detected when this manager starts becomes the default resolution.")]
    [SerializeField] private bool useStartupResolutionAsDefault = true;

    [SerializeField] private int fallbackDefaultWidth = 1920;
    [SerializeField] private int fallbackDefaultHeight = 1080;

    [Tooltip("If true, reset defaults uses Windowed mode. If false, reset defaults uses Fullscreen Window.")]
    [SerializeField] private bool defaultWindowed = true;

    [Tooltip("Use -1 to use the Unity project's current quality level at startup.")]
    [SerializeField] private int defaultQualityLevel = -1;

    [SerializeField] private bool defaultVSyncEnabled = true;

    [Tooltip("-1 means uncapped. If VSync is enabled, Unity may ignore target frame rate.")]
    [SerializeField] private int defaultTargetFrameRate = -1;

    [Header("Visual Defaults")]
    [SerializeField, Range(0.5f, 1.5f)] private float defaultBrightness = 1f;
    [SerializeField, Range(0.85f, 1.15f)] private float defaultUiScale = 1f;

    [Header("Visual Ranges")]
    [SerializeField] private float minimumBrightness = 0.5f;
    [SerializeField] private float maximumBrightness = 1.5f;
    [SerializeField] private float minimumUiScale = 0.85f;
    [SerializeField] private float maximumUiScale = 1.15f;

    [Header("Resolution Safety")]
    [SerializeField] private bool restrictToDesignedAspectRatio = true;
    [SerializeField] private Vector2 designedAspectRatio = new Vector2(16f, 9f);
    [SerializeField] private float aspectTolerance = 0.02f;

    [Tooltip("Recommended. Borderless fullscreen should use the monitor's native resolution.")]
    [SerializeField] private bool forceNativeResolutionWhenFullscreen = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private const string ResolutionWidthKey = "EchoSystemsLab_Graphics_ResolutionWidth";
    private const string ResolutionHeightKey = "EchoSystemsLab_Graphics_ResolutionHeight";
    private const string WindowedKey = "EchoSystemsLab_Graphics_Windowed";
    private const string QualityLevelKey = "EchoSystemsLab_Graphics_QualityLevel";
    private const string VSyncKey = "EchoSystemsLab_Graphics_VSync";
    private const string TargetFrameRateKey = "EchoSystemsLab_Graphics_TargetFrameRate";
    private const string BrightnessKey = "EchoSystemsLab_Graphics_Brightness";
    private const string UiScaleKey = "EchoSystemsLab_Graphics_UiScale";

    private readonly List<Vector2Int> resolutionOptions = new List<Vector2Int>();

    private Vector2Int startupResolution;
    private int startupQualityLevel;

    private int resolutionWidth;
    private int resolutionHeight;
    private bool windowed;
    private int qualityLevel;
    private bool vSyncEnabled;
    private int targetFrameRate;
    private float brightness;
    private float uiScale;

    public int ResolutionWidth => resolutionWidth;
    public int ResolutionHeight => resolutionHeight;
    public bool Windowed => windowed;
    public int QualityLevel => qualityLevel;
    public bool VSyncEnabled => vSyncEnabled;
    public int TargetFrameRate => targetFrameRate;
    public float Brightness => brightness;
    public float UiScale => uiScale;

    public int ResolutionCount => resolutionOptions.Count;
    public bool ForceNativeResolutionWhenFullscreen => forceNativeResolutionWhenFullscreen;

    public float MinimumBrightness => Mathf.Min(minimumBrightness, maximumBrightness);
    public float MaximumBrightness => Mathf.Max(minimumBrightness, maximumBrightness);
    public float MinimumUiScale => Mathf.Min(minimumUiScale, maximumUiScale);
    public float MaximumUiScale => Mathf.Max(minimumUiScale, maximumUiScale);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning($"{nameof(GraphicsSettingsManager)} has Dont Destroy On Load enabled, but it is not on a root GameObject. Put it on SystemBootstrap or disable Dont Destroy On Load.");
            }
        }

        startupResolution = GetSafeStartupResolution();
        startupQualityLevel = QualitySettings.GetQualityLevel();

        RefreshResolutionOptions();
        LoadSettings();
        ApplyCurrentSettings(false);
    }

    public void RefreshResolutionOptions()
    {
        resolutionOptions.Clear();

        Resolution[] screenResolutions = Screen.resolutions;

        if (screenResolutions != null)
        {
            foreach (Resolution resolution in screenResolutions)
            {
                AddResolutionOptionIfAllowed(resolution.width, resolution.height);
            }
        }

        AddResolutionOptionIfAllowed(1280, 720);
        AddResolutionOptionIfAllowed(1600, 900);
        AddResolutionOptionIfAllowed(1920, 1080);
        AddResolutionOptionIfAllowed(2560, 1440);
        AddResolutionOptionIfAllowed(3840, 2160);

        if (resolutionOptions.Count == 0)
        {
            AddResolutionIfMissing(1280, 720);
            AddResolutionIfMissing(1920, 1080);
        }

        resolutionOptions.Sort((a, b) =>
        {
            int widthComparison = a.x.CompareTo(b.x);

            if (widthComparison != 0)
                return widthComparison;

            return a.y.CompareTo(b.y);
        });
    }

    public List<string> GetResolutionLabels()
    {
        List<string> labels = new List<string>();

        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            Vector2Int resolution = resolutionOptions[i];
            labels.Add($"{resolution.x} x {resolution.y}");
        }

        return labels;
    }

    public Vector2Int GetResolutionAtIndex(int index)
    {
        if (resolutionOptions.Count == 0)
            return GetSafeStartupResolution();

        index = Mathf.Clamp(index, 0, resolutionOptions.Count - 1);
        return resolutionOptions[index];
    }

    public int GetCurrentResolutionIndex()
    {
        return GetClosestResolutionIndex(resolutionWidth, resolutionHeight);
    }

    public int GetClosestResolutionIndex(int width, int height)
    {
        if (resolutionOptions.Count == 0)
            return 0;

        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            Vector2Int resolution = resolutionOptions[i];

            if (resolution.x == width && resolution.y == height)
                return i;
        }

        int bestIndex = 0;
        long bestDifference = long.MaxValue;

        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            Vector2Int resolution = resolutionOptions[i];

            long widthDifference = Mathf.Abs(resolution.x - width);
            long heightDifference = Mathf.Abs(resolution.y - height);
            long totalDifference = widthDifference + heightDifference;

            if (totalDifference < bestDifference)
            {
                bestDifference = totalDifference;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public string[] GetQualityNames()
    {
        return QualitySettings.names;
    }

    public void ApplySettings(
        int resolutionIndex,
        bool useWindowedMode,
        int selectedQualityLevel,
        bool useVSync,
        int selectedTargetFrameRate,
        float selectedBrightness,
        float selectedUiScale)
    {
        Vector2Int selectedResolution = GetResolutionAtIndex(resolutionIndex);

        resolutionWidth = selectedResolution.x;
        resolutionHeight = selectedResolution.y;
        windowed = useWindowedMode;
        qualityLevel = ClampQualityLevel(selectedQualityLevel);
        vSyncEnabled = useVSync;
        targetFrameRate = selectedTargetFrameRate;
        brightness = ClampBrightness(selectedBrightness);
        uiScale = ClampUiScale(selectedUiScale);

        ApplyCurrentSettings(true);
        SaveSettings();
    }

    public void ResetToDefaults()
    {
        Vector2Int defaultResolution = GetDefaultResolution();

        resolutionWidth = defaultResolution.x;
        resolutionHeight = defaultResolution.y;
        windowed = defaultWindowed;
        qualityLevel = GetDefaultQualityLevel();
        vSyncEnabled = defaultVSyncEnabled;
        targetFrameRate = defaultTargetFrameRate;
        brightness = ClampBrightness(defaultBrightness);
        uiScale = ClampUiScale(defaultUiScale);

        RefreshResolutionOptions();
        ApplyCurrentSettings(true);
        SaveSettings();

        if (debugLogs)
            Debug.Log("Graphics settings reset to defaults.");
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt(ResolutionWidthKey, resolutionWidth);
        PlayerPrefs.SetInt(ResolutionHeightKey, resolutionHeight);
        PlayerPrefs.SetInt(WindowedKey, windowed ? 1 : 0);
        PlayerPrefs.SetInt(QualityLevelKey, qualityLevel);
        PlayerPrefs.SetInt(VSyncKey, vSyncEnabled ? 1 : 0);
        PlayerPrefs.SetInt(TargetFrameRateKey, targetFrameRate);
        PlayerPrefs.SetFloat(BrightnessKey, brightness);
        PlayerPrefs.SetFloat(UiScaleKey, uiScale);
        PlayerPrefs.Save();

        if (debugLogs)
            Debug.Log("Graphics settings saved.");
    }

    [ContextMenu("Clear Saved Graphics Settings")]
    private void ClearSavedGraphicsSettings()
    {
        PlayerPrefs.DeleteKey(ResolutionWidthKey);
        PlayerPrefs.DeleteKey(ResolutionHeightKey);
        PlayerPrefs.DeleteKey(WindowedKey);
        PlayerPrefs.DeleteKey(QualityLevelKey);
        PlayerPrefs.DeleteKey(VSyncKey);
        PlayerPrefs.DeleteKey(TargetFrameRateKey);
        PlayerPrefs.DeleteKey(BrightnessKey);
        PlayerPrefs.DeleteKey(UiScaleKey);
        PlayerPrefs.Save();

        Debug.Log("Saved graphics settings cleared.");
    }

    private void LoadSettings()
    {
        Vector2Int defaultResolution = GetDefaultResolution();

        resolutionWidth = PlayerPrefs.GetInt(ResolutionWidthKey, defaultResolution.x);
        resolutionHeight = PlayerPrefs.GetInt(ResolutionHeightKey, defaultResolution.y);
        windowed = PlayerPrefs.GetInt(WindowedKey, defaultWindowed ? 1 : 0) == 1;
        qualityLevel = PlayerPrefs.GetInt(QualityLevelKey, GetDefaultQualityLevel());
        vSyncEnabled = PlayerPrefs.GetInt(VSyncKey, defaultVSyncEnabled ? 1 : 0) == 1;
        targetFrameRate = PlayerPrefs.GetInt(TargetFrameRateKey, defaultTargetFrameRate);
        brightness = PlayerPrefs.GetFloat(BrightnessKey, defaultBrightness);
        uiScale = PlayerPrefs.GetFloat(UiScaleKey, defaultUiScale);

        qualityLevel = ClampQualityLevel(qualityLevel);
        brightness = ClampBrightness(brightness);
        uiScale = ClampUiScale(uiScale);

        AddResolutionIfMissing(resolutionWidth, resolutionHeight);
    }

    private void ApplyCurrentSettings(bool notify)
    {
        qualityLevel = ClampQualityLevel(qualityLevel);

        FullScreenMode screenMode = windowed
            ? FullScreenMode.Windowed
            : FullScreenMode.FullScreenWindow;

        int appliedWidth = Mathf.Max(1, resolutionWidth);
        int appliedHeight = Mathf.Max(1, resolutionHeight);

        if (!windowed && forceNativeResolutionWhenFullscreen)
        {
            appliedWidth = Screen.currentResolution.width;
            appliedHeight = Screen.currentResolution.height;
        }

        Screen.SetResolution(
            appliedWidth,
            appliedHeight,
            screenMode);

        QualitySettings.SetQualityLevel(qualityLevel, true);
        QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
        Application.targetFrameRate = targetFrameRate;

        if (debugLogs)
        {
            string modeLabel = windowed ? "Windowed" : "Fullscreen Window";
            Debug.Log($"Applied graphics settings: {resolutionWidth}x{resolutionHeight}, {modeLabel}, Quality {qualityLevel}, VSync {vSyncEnabled}, Target FPS {targetFrameRate}, Brightness {brightness}, UI Scale {uiScale}");
        }

        if (notify)
            OnGraphicsSettingsChanged?.Invoke();
    }

    private void AddResolutionIfMissing(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            Vector2Int existing = resolutionOptions[i];

            if (existing.x == width && existing.y == height)
                return;
        }

        resolutionOptions.Add(new Vector2Int(width, height));
    }

    private void AddResolutionOptionIfAllowed(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        if (restrictToDesignedAspectRatio && !MatchesDesignedAspectRatio(width, height))
            return;

        AddResolutionIfMissing(width, height);
    }

    private bool MatchesDesignedAspectRatio(int width, int height)
    {
        if (designedAspectRatio.x <= 0f || designedAspectRatio.y <= 0f)
            return true;

        float actualAspect = width / (float)height;
        float targetAspect = designedAspectRatio.x / designedAspectRatio.y;

        return Mathf.Abs(actualAspect - targetAspect) <= aspectTolerance;
    }

    private Vector2Int GetSafeStartupResolution()
    {
        int width = Screen.currentResolution.width;
        int height = Screen.currentResolution.height;

        if (width <= 0 || height <= 0)
        {
            width = Screen.width;
            height = Screen.height;
        }

        if (width <= 0 || height <= 0)
        {
            width = fallbackDefaultWidth;
            height = fallbackDefaultHeight;
        }

        return new Vector2Int(width, height);
    }

    private Vector2Int GetDefaultResolution()
    {
        if (useStartupResolutionAsDefault)
            return startupResolution;

        return new Vector2Int(
            Mathf.Max(1, fallbackDefaultWidth),
            Mathf.Max(1, fallbackDefaultHeight));
    }

    private int GetDefaultQualityLevel()
    {
        if (defaultQualityLevel >= 0)
            return ClampQualityLevel(defaultQualityLevel);

        return ClampQualityLevel(startupQualityLevel);
    }

    private int ClampQualityLevel(int value)
    {
        string[] names = QualitySettings.names;

        if (names == null || names.Length == 0)
            return 0;

        return Mathf.Clamp(value, 0, names.Length - 1);
    }

    private float ClampBrightness(float value)
    {
        return Mathf.Clamp(value, MinimumBrightness, MaximumBrightness);
    }

    private float ClampUiScale(float value)
    {
        return Mathf.Clamp(value, MinimumUiScale, MaximumUiScale);
    }
}

//-----GraphicsSettingsManager.cs END-----