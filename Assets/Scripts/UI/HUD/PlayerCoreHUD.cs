//-----PlayerCoreHUD.cs START-----

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCoreHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatsController playerStats;

    [Header("Identity UI")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image playerIconImage;

    [Header("Level / Score UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Health UI")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthSlider;

    [Header("Stamina UI")]
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private Slider staminaSlider;

    [Header("Mana UI")]
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private Slider manaSlider;

    [Header("Experience UI")]
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private Slider experienceSlider;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStatsController>();
    }

    private void OnEnable()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= Refresh;
    }

    public void Refresh()
    {
        if (playerStats == null)
            return;

        RefreshIdentity();
        RefreshLevelAndScore();
        RefreshHealth();
        RefreshStamina();
        RefreshMana();
        RefreshExperience();
    }

    private void RefreshIdentity()
    {
        PlayerProfileData profile = playerStats.ProfileData;

        if (playerNameText != null)
        {
            if (profile != null && !string.IsNullOrWhiteSpace(profile.displayName))
                playerNameText.text = profile.displayName;
            else
                playerNameText.text = "Player";
        }

        if (playerIconImage != null)
        {
            Sprite icon = profile != null ? profile.playerIcon : null;
            playerIconImage.sprite = icon;
            playerIconImage.enabled = icon != null;
        }
    }

    private void RefreshLevelAndScore()
    {
        if (levelText != null)
            levelText.text = $"LVL {playerStats.CurrentLevel}";

        if (scoreText != null)
            scoreText.text = $"Score: {playerStats.CurrentScore}";
    }

    private void RefreshHealth()
    {
        if (healthText != null)
            healthText.text = $"{playerStats.CurrentHealth} / {playerStats.MaxHealth}";

        SetSlider(healthSlider, playerStats.CurrentHealth, playerStats.MaxHealth);
    }

    private void RefreshStamina()
    {
        if (staminaText != null)
            staminaText.text = $"{playerStats.CurrentStamina} / {playerStats.MaxStamina}";

        SetSlider(staminaSlider, playerStats.CurrentStamina, playerStats.MaxStamina);
    }

    private void RefreshMana()
    {
        if (manaText != null)
            manaText.text = $"{playerStats.CurrentMana} / {playerStats.MaxMana}";

        SetSlider(manaSlider, playerStats.CurrentMana, playerStats.MaxMana);
    }

    private void RefreshExperience()
    {
        if (experienceText != null)
            experienceText.text = $"EXP";//{playerStats.CurrentExperience} / {playerStats.ExperienceToNextLevel}

        SetSlider(experienceSlider, playerStats.CurrentExperience, playerStats.ExperienceToNextLevel);
    }
    private void SetSlider(Slider slider, int current, int max)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(1, max);

        float value = Mathf.Clamp(current, 0, slider.maxValue);

        slider.SetValueWithoutNotify(value);

        SetSliderFillVisible(slider, value > 0f);
    }

    private void SetSliderFillVisible(Slider slider, bool visible)
    {
        if (slider == null || slider.fillRect == null)
            return;

        Graphic fillGraphic = slider.fillRect.GetComponent<Graphic>();

        if (fillGraphic != null)
            fillGraphic.enabled = visible;
    }
}

//-----PlayerCoreHUD.cs END-----