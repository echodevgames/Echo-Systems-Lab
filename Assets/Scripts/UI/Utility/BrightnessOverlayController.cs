//-----BrightnessOverlayController.cs START-----

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BrightnessOverlayController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private GraphicsSettingsManager graphicsSettingsManager;

    [Header("Overlay Colors")]
    [SerializeField] private Color dimColor = Color.black;
    [SerializeField] private Color brightenColor = Color.white;

    [Header("Overlay Strength")]
    [SerializeField, Range(0f, 1f)] private float maxDimAlpha = 0.45f;
    [SerializeField, Range(0f, 1f)] private float maxBrightenAlpha = 0.18f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private bool hasSubscribed;

    private void Awake()
    {
        if (overlayImage == null)
            overlayImage = GetComponent<Image>();

        if (overlayImage != null)
        {
            overlayImage.raycastTarget = false;
            overlayImage.color = Color.clear;
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
        ApplyCurrentBrightness();
    }

    private void Start()
    {
        TrySubscribe();
        ApplyCurrentBrightness();
    }

    private void OnDisable()
    {
        if (graphicsSettingsManager != null && hasSubscribed)
            graphicsSettingsManager.OnGraphicsSettingsChanged -= ApplyCurrentBrightness;

        hasSubscribed = false;
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

        graphicsSettingsManager.OnGraphicsSettingsChanged += ApplyCurrentBrightness;
        hasSubscribed = true;
    }

    private void ApplyCurrentBrightness()
    {
        if (overlayImage == null)
            return;

        if (graphicsSettingsManager == null)
        {
            overlayImage.color = Color.clear;
            return;
        }

        ApplyBrightness(graphicsSettingsManager.Brightness);
    }

    private void ApplyBrightness(float brightness)
    {
        if (overlayImage == null)
            return;

        float neutral = 1f;

        if (Mathf.Approximately(brightness, neutral))
        {
            overlayImage.color = Color.clear;
            return;
        }

        if (brightness < neutral)
        {
            float percent = Mathf.InverseLerp(
                neutral,
                graphicsSettingsManager.MinimumBrightness,
                brightness);

            Color color = dimColor;
            color.a = Mathf.Clamp01(percent * maxDimAlpha);
            overlayImage.color = color;

            if (debugLogs)
                Debug.Log($"Brightness overlay dim alpha: {color.a}");

            return;
        }

        float brightenPercent = Mathf.InverseLerp(
            neutral,
            graphicsSettingsManager.MaximumBrightness,
            brightness);

        Color brighten = brightenColor;
        brighten.a = Mathf.Clamp01(brightenPercent * maxBrightenAlpha);
        overlayImage.color = brighten;

        if (debugLogs)
            Debug.Log($"Brightness overlay brighten alpha: {brighten.a}");
    }
}

//-----BrightnessOverlayController.cs END-----