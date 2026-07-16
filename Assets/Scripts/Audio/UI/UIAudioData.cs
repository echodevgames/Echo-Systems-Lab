//-----UIAudioData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "UIAudioData_NewUISet",
    menuName = "Echo Systems Lab/Audio/UI Audio Data")]
public class UIAudioData : ScriptableObject
{
    [Header("Navigation")]
    public AudioEventData buttonHoverAudio;
    public AudioEventData buttonClickAudio;
    public AudioEventData buttonDeniedAudio;

    [Header("Menus")]
    public AudioEventData menuOpenAudio;
    public AudioEventData menuCloseAudio;
    public AudioEventData menuBackAudio;

    [Header("Controls")]
    public AudioEventData sliderChangedAudio;
    public AudioEventData toggleChangedAudio;
    public AudioEventData dropdownChangedAudio;

    [Header("Debug")]
    public bool debugLogs;

    public void PlayButtonHoverAudio()
    {
        PlayAudioEvent(buttonHoverAudio, "button hover");
    }

    public void PlayButtonClickAudio()
    {
        PlayAudioEvent(buttonClickAudio, "button click");
    }

    public void PlayButtonDeniedAudio()
    {
        PlayAudioEvent(buttonDeniedAudio, "button denied");
    }

    public void PlayMenuOpenAudio()
    {
        PlayAudioEvent(menuOpenAudio, "menu open");
    }

    public void PlayMenuCloseAudio()
    {
        PlayAudioEvent(menuCloseAudio, "menu close");
    }

    public void PlayMenuBackAudio()
    {
        PlayAudioEvent(menuBackAudio, "menu back");
    }

    public void PlaySliderChangedAudio()
    {
        PlayAudioEvent(sliderChangedAudio, "slider changed");
    }

    public void PlayToggleChangedAudio()
    {
        PlayAudioEvent(toggleChangedAudio, "toggle changed");
    }

    public void PlayDropdownChangedAudio()
    {
        PlayAudioEvent(dropdownChangedAudio, "dropdown changed");
    }

    private void PlayAudioEvent(AudioEventData audioEvent, string eventLabel)
    {
        if (audioEvent == null)
            return;

        if (GameAudioManager.Instance == null)
        {
            Debug.LogWarning($"Tried to play UI {eventLabel} audio, but no GameAudioManager exists.");
            return;
        }

        GameAudioManager.Instance.PlayOneShot(audioEvent);

        if (debugLogs)
            Debug.Log($"{name} played UI {eventLabel} audio event: {audioEvent.name}");
    }
}

//-----UIAudioData.cs END-----