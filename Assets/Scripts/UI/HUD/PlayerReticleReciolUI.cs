//-----PlayerReticleRecoilUI.cs START-----

using UnityEngine;

public class PlayerReticleRecoilUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform reticleRoot;

    [Header("Fallback Reticle Recoil")]
    [SerializeField] private bool fallbackUseReticleRecoil = true;
    [SerializeField] private Vector2 fallbackKickXRange = new Vector2(-4f, 4f);
    [SerializeField] private Vector2 fallbackKickYRange = new Vector2(6f, 12f);
    [SerializeField] private float fallbackMaxKickOffset = 22f;
    [SerializeField] private float fallbackKickSnappiness = 28f;
    [SerializeField] private float fallbackReturnSpeed = 16f;

    [Header("Fallback Reticle Pulse")]
    [SerializeField] private bool fallbackUseScalePulse = true;
    [SerializeField] private float fallbackScaleKick = 0.12f;
    [SerializeField] private float fallbackMaxScaleKick = 0.25f;
    [SerializeField] private float fallbackScaleSnappiness = 32f;
    [SerializeField] private float fallbackScaleReturnSpeed = 18f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private Vector2 baseAnchoredPosition;
    private Vector3 baseLocalScale;

    private Vector2 targetOffset;
    private Vector2 currentOffset;

    private float targetScaleOffset;
    private float currentScaleOffset;

    private float activeKickSnappiness;
    private float activeReturnSpeed;
    private float activeScaleSnappiness;
    private float activeScaleReturnSpeed;

    private void Awake()
    {
        if (reticleRoot == null)
            reticleRoot = GetComponent<RectTransform>();

        CacheBaseValues();
        ResetActiveTuning();
        ValidateSetup();
    }

    private void OnEnable()
    {
        CacheBaseValues();
        ResetReticle();
    }

    private void Update()
    {
        if (reticleRoot == null)
            return;

        float deltaTime = Time.unscaledDeltaTime;

        targetOffset = Vector2.Lerp(
            targetOffset,
            Vector2.zero,
            activeReturnSpeed * deltaTime);

        currentOffset = Vector2.Lerp(
            currentOffset,
            targetOffset,
            activeKickSnappiness * deltaTime);

        targetScaleOffset = Mathf.Lerp(
            targetScaleOffset,
            0f,
            activeScaleReturnSpeed * deltaTime);

        currentScaleOffset = Mathf.Lerp(
            currentScaleOffset,
            targetScaleOffset,
            activeScaleSnappiness * deltaTime);

        ApplyReticleTransform();
    }

    public void PlayFireFeedback(WeaponHandlingData handlingData)
    {
        if (reticleRoot == null)
            return;

        RefreshActiveTuning(handlingData);

        bool useReticleRecoil = handlingData != null
            ? handlingData.useReticleRecoil
            : fallbackUseReticleRecoil;

        if (useReticleRecoil)
            AddReticleKick(handlingData);

        bool useScalePulse = handlingData != null
            ? handlingData.useReticleScalePulse
            : fallbackUseScalePulse;

        if (useScalePulse)
            AddScaleKick(handlingData);

        if (debugLogs)
            Debug.Log($"Reticle fire feedback played. Offset: {targetOffset}, Scale: {targetScaleOffset}");
    }

    public void ResetReticle()
    {
        targetOffset = Vector2.zero;
        currentOffset = Vector2.zero;
        targetScaleOffset = 0f;
        currentScaleOffset = 0f;

        ApplyReticleTransform();
    }

    [ContextMenu("Test Fire Feedback")]
    private void TestFireFeedback()
    {
        PlayFireFeedback(null);
    }

    private void CacheBaseValues()
    {
        if (reticleRoot == null)
            return;

        baseAnchoredPosition = reticleRoot.anchoredPosition;
        baseLocalScale = reticleRoot.localScale;
    }

    private void ResetActiveTuning()
    {
        activeKickSnappiness = Mathf.Max(0.01f, fallbackKickSnappiness);
        activeReturnSpeed = Mathf.Max(0.01f, fallbackReturnSpeed);
        activeScaleSnappiness = Mathf.Max(0.01f, fallbackScaleSnappiness);
        activeScaleReturnSpeed = Mathf.Max(0.01f, fallbackScaleReturnSpeed);
    }

    private void RefreshActiveTuning(WeaponHandlingData handlingData)
    {
        activeKickSnappiness = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticleKickSnappiness)
            : Mathf.Max(0.01f, fallbackKickSnappiness);

        activeReturnSpeed = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticleReturnSpeed)
            : Mathf.Max(0.01f, fallbackReturnSpeed);

        activeScaleSnappiness = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticleScaleSnappiness)
            : Mathf.Max(0.01f, fallbackScaleSnappiness);

        activeScaleReturnSpeed = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticleScaleReturnSpeed)
            : Mathf.Max(0.01f, fallbackScaleReturnSpeed);
    }

    private void AddReticleKick(WeaponHandlingData handlingData)
    {
        Vector2 xRange = handlingData != null
            ? handlingData.reticleKickXRange
            : fallbackKickXRange;

        Vector2 yRange = handlingData != null
            ? handlingData.reticleKickYRange
            : fallbackKickYRange;

        float maxKickOffset = handlingData != null
            ? handlingData.maxReticleKickOffset
            : fallbackMaxKickOffset;

        float minX = Mathf.Min(xRange.x, xRange.y);
        float maxX = Mathf.Max(xRange.x, xRange.y);

        float minY = Mathf.Min(yRange.x, yRange.y);
        float maxY = Mathf.Max(yRange.x, yRange.y);

        Vector2 kick = new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY));

        targetOffset += kick;
        targetOffset = Vector2.ClampMagnitude(targetOffset, Mathf.Max(0f, maxKickOffset));
    }

    private void AddScaleKick(WeaponHandlingData handlingData)
    {
        float scaleKick = handlingData != null
            ? handlingData.reticleScaleKick
            : fallbackScaleKick;

        float maxScaleKick = handlingData != null
            ? handlingData.maxReticleScaleKick
            : fallbackMaxScaleKick;

        targetScaleOffset += scaleKick;
        targetScaleOffset = Mathf.Clamp(targetScaleOffset, 0f, Mathf.Max(0f, maxScaleKick));
    }

    private void ApplyReticleTransform()
    {
        if (reticleRoot == null)
            return;

        reticleRoot.anchoredPosition = baseAnchoredPosition + currentOffset;

        float scaleMultiplier = 1f + currentScaleOffset;
        reticleRoot.localScale = baseLocalScale * scaleMultiplier;
    }

    private void ValidateSetup()
    {
        if (reticleRoot == null)
            return;

        if (reticleRoot.GetComponent<Canvas>() != null)
        {
            Debug.LogWarning(
                "PlayerReticleRecoilUI is assigned to a Canvas. Assign a small ReticleRoot RectTransform instead.");
        }
    }
}

//-----PlayerReticleRecoilUI.cs END-----