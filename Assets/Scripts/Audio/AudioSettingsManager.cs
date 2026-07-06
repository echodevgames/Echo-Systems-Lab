//-----AudioSettingsManager.cs START-----

using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    public event Action OnAudioSettingsChanged;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Exposed Mixer Parameters")]
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";
    [SerializeField] private string weaponsVolumeParameter = "WeaponsVolume";
    [SerializeField] private string uiVolumeParameter = "UIVolume";
    [SerializeField] private string ambienceVolumeParameter = "AmbienceVolume";

    [Header("Defaults")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMasterVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultSfxVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultWeaponsVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultUiVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultAmbienceVolume = 0.8f;

    [Header("Decibel Range")]
    [SerializeField] private float mutedDecibels = -80f;

    private const string MasterVolumeKey = "EchoSystemsLab_Audio_MasterVolume";
    private const string MusicVolumeKey = "EchoSystemsLab_Audio_MusicVolume";
    private const string SfxVolumeKey = "EchoSystemsLab_Audio_SFXVolume";
    private const string WeaponsVolumeKey = "EchoSystemsLab_Audio_WeaponsVolume";
    private const string UiVolumeKey = "EchoSystemsLab_Audio_UIVolume";
    private const string AmbienceVolumeKey = "EchoSystemsLab_Audio_AmbienceVolume";

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;
    private float weaponsVolume;
    private float uiVolume;
    private float ambienceVolume;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;
    public float WeaponsVolume => weaponsVolume;
    public float UiVolume => uiVolume;
    public float AmbienceVolume => ambienceVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyAllMixerVolumes();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = ClampVolume(value);
        ApplyMixerVolume(masterVolumeParameter, masterVolume);
        SaveVolume(MasterVolumeKey, masterVolume);
        NotifyChanged();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = ClampVolume(value);
        ApplyMixerVolume(musicVolumeParameter, musicVolume);
        SaveVolume(MusicVolumeKey, musicVolume);
        NotifyChanged();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = ClampVolume(value);
        ApplyMixerVolume(sfxVolumeParameter, sfxVolume);
        SaveVolume(SfxVolumeKey, sfxVolume);
        NotifyChanged();
    }

    public void SetWeaponsVolume(float value)
    {
        weaponsVolume = ClampVolume(value);
        ApplyMixerVolume(weaponsVolumeParameter, weaponsVolume);
        SaveVolume(WeaponsVolumeKey, weaponsVolume);
        NotifyChanged();
    }

    public void SetUiVolume(float value)
    {
        uiVolume = ClampVolume(value);
        ApplyMixerVolume(uiVolumeParameter, uiVolume);
        SaveVolume(UiVolumeKey, uiVolume);
        NotifyChanged();
    }

    public void SetAmbienceVolume(float value)
    {
        ambienceVolume = ClampVolume(value);
        ApplyMixerVolume(ambienceVolumeParameter, ambienceVolume);
        SaveVolume(AmbienceVolumeKey, ambienceVolume);
        NotifyChanged();
    }

    public void ResetToDefaults()
    {
        SetMasterVolume(defaultMasterVolume);
        SetMusicVolume(defaultMusicVolume);
        SetSfxVolume(defaultSfxVolume);
        SetWeaponsVolume(defaultWeaponsVolume);
        SetUiVolume(defaultUiVolume);
        SetAmbienceVolume(defaultAmbienceVolume);

        PlayerPrefs.Save();

        Debug.Log("Audio settings reset to defaults.");
    }

    public void ApplyAllMixerVolumes()
    {
        ApplyMixerVolume(masterVolumeParameter, masterVolume);
        ApplyMixerVolume(musicVolumeParameter, musicVolume);
        ApplyMixerVolume(sfxVolumeParameter, sfxVolume);
        ApplyMixerVolume(weaponsVolumeParameter, weaponsVolume);
        ApplyMixerVolume(uiVolumeParameter, uiVolume);
        ApplyMixerVolume(ambienceVolumeParameter, ambienceVolume);
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, defaultMasterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);
        weaponsVolume = PlayerPrefs.GetFloat(WeaponsVolumeKey, defaultWeaponsVolume);
        uiVolume = PlayerPrefs.GetFloat(UiVolumeKey, defaultUiVolume);
        ambienceVolume = PlayerPrefs.GetFloat(AmbienceVolumeKey, defaultAmbienceVolume);

        masterVolume = ClampVolume(masterVolume);
        musicVolume = ClampVolume(musicVolume);
        sfxVolume = ClampVolume(sfxVolume);
        weaponsVolume = ClampVolume(weaponsVolume);
        uiVolume = ClampVolume(uiVolume);
        ambienceVolume = ClampVolume(ambienceVolume);
    }

    private void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, ClampVolume(value));
        PlayerPrefs.Save();
    }

    private void ApplyMixerVolume(string parameterName, float normalizedVolume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioSettingsManager has no AudioMixer assigned.");
            return;
        }

        if (string.IsNullOrWhiteSpace(parameterName))
            return;

        float decibels = NormalizedVolumeToDecibels(normalizedVolume);

        bool parameterFound = audioMixer.SetFloat(parameterName, decibels);

        if (!parameterFound)
            Debug.LogWarning($"AudioMixer parameter not found or not exposed: {parameterName}");
    }

    private float NormalizedVolumeToDecibels(float normalizedVolume)
    {
        normalizedVolume = ClampVolume(normalizedVolume);

        if (normalizedVolume <= 0.0001f)
            return mutedDecibels;

        return Mathf.Log10(normalizedVolume) * 20f;
    }

    private float ClampVolume(float value)
    {
        return Mathf.Clamp01(value);
    }

    private void NotifyChanged()
    {
        OnAudioSettingsChanged?.Invoke();
    }
}

//-----AudioSettingsManager.cs END-----