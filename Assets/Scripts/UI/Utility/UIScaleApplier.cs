//-----UIScaleApplier.cs START-----

using UnityEngine;

public class UIScaleApplier : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Assign menu roots or canvas content roots here. If empty, this object's RectTransform is used.")]
    [SerializeField] private RectTransform[] rootsToScale;

    [Header("References")]
    [SerializeField] private GraphicsSettingsManager graphicsSettingsManager;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private Vector3[] baseScales;
    private bool hasSubscribed;

    private void Awake()
    {
        CacheTargets();
    }

    private void OnEnable()
    {
        TrySubscribe();
        ApplySavedScale();
    }

    private void Start()
    {
        TrySubscribe();
        ApplySavedScale();
    }

    private void OnDisable()
    {
        if (graphicsSettingsManager != null && hasSubscribed)
            graphicsSettingsManager.OnGraphicsSettingsChanged -= ApplySavedScale;

        hasSubscribed = false;
    }

    public void PreviewScale(float scale)
    {
        ApplyScale(scale);
    }

    public void ApplySavedScale()
    {
        float scale = graphicsSettingsManager != null
            ? graphicsSettingsManager.UiScale
            : 1f;

        ApplyScale(scale);
    }

    public void RevertToSavedScale()
    {
        ApplySavedScale();
    }

    private void CacheTargets()
    {
        if (rootsToScale == null || rootsToScale.Length == 0)
        {
            RectTransform ownRectTransform = GetComponent<RectTransform>();

            if (ownRectTransform != null)
                rootsToScale = new[] { ownRectTransform };
        }

        if (rootsToScale == null)
        {
            baseScales = new Vector3[0];
            return;
        }

        baseScales = new Vector3[rootsToScale.Length];

        for (int i = 0; i < rootsToScale.Length; i++)
        {
            baseScales[i] = rootsToScale[i] != null
                ? rootsToScale[i].localScale
                : Vector3.one;
        }
    }

    private void TrySubscribe()
    {
        if (hasSubscribed)
            return;

        if (graphicsSettingsManager == null)
            graphicsSettingsManager = GraphicsSettingsManager.Instance;

        if (graphicsSettingsManager == null)
            graphicsSettingsManager = FindFirstObjectByType<GraphicsSettingsManager>();

        if (graphicsSettingsManager == null)
            return;

        graphicsSettingsManager.OnGraphicsSettingsChanged += ApplySavedScale;
        hasSubscribed = true;
    }

    private void ApplyScale(float scale)
    {
        if (rootsToScale == null || baseScales == null)
            return;

        int count = Mathf.Min(rootsToScale.Length, baseScales.Length);

        for (int i = 0; i < count; i++)
        {
            if (rootsToScale[i] == null)
                continue;

            rootsToScale[i].localScale = baseScales[i] * scale;
        }

        if (debugLogs)
            Debug.Log($"Applied UI scale preview: {scale:0.00}");
    }
}

//-----UIScaleApplier.cs END-----