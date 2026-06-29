//-----PauseMenuController.cs START-----

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private SimpleFirstPersonController playerController;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private PlayerWeaponController playerWeaponController;
    [SerializeField] private PlayerWeaponLoadoutController playerWeaponLoadoutController;
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button keybindsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        if (inputReader == null)
            inputReader = FindFirstObjectByType<PlayerInputReader>();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (saveButton != null)
            saveButton.onClick.AddListener(Save);

        if (loadButton != null)
            loadButton.onClick.AddListener(Load);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(SettingsStub);

        if (keybindsButton != null)
            keybindsButton.onClick.AddListener(KeybindsStub);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        SetPaused(false);
    }

    private void Update()
    {
        if (inputReader == null)
            return;

        if (inputReader.PausePressed)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        SetPaused(true);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pauseRoot != null)
            pauseRoot.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        if (inputReader != null)
            inputReader.SetGameplayInputEnabled(!paused);

        if (playerController != null)
        {
            playerController.SetInputEnabled(!paused);
            playerController.SetCursorLocked(!paused);
        }

        if (playerInteractor != null)
            playerInteractor.SetInteractionEnabled(!paused);

        if (playerWeaponController != null)
            playerWeaponController.SetInputEnabled(!paused);

        if (playerWeaponLoadoutController != null)
            playerWeaponLoadoutController.SetInputEnabled(!paused);
    }

    private void Save()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    private void Load()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.LoadGame();

        Resume();
    }

    private void SettingsStub()
    {
        Debug.Log("Settings menu stub.");
    }

    private void KeybindsStub()
    {
        Debug.Log("Keybinds menu stub.");
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (GameSceneLoader.Instance != null)
        {
            GameSceneLoader.Instance.LoadMainMenu();
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void QuitGame()
    {
        if (GameSceneLoader.Instance != null)
        {
            GameSceneLoader.Instance.QuitGame();
            return;
        }

        Application.Quit();
    }
}

//-----PauseMenuController.cs END-----