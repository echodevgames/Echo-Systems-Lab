//------MainMenuController.cs START-----

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button audioSettingsButton;
    [SerializeField] private Button graphicsSettingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private AudioSettingsMenuUI audioSettingsMenuUI;
    [SerializeField] private GraphicsSettingsMenuUI graphicsSettingsMenuUI;
    [SerializeField] private CreditsMenuUI creditsMenuUI;

    [Header("Scenes")]
    [SerializeField] private string hubSceneName = "Hub";

    private void Awake()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);

        if (audioSettingsButton != null)
            audioSettingsButton.onClick.AddListener(OpenAudioSettings);

        if (graphicsSettingsButton != null)
            graphicsSettingsButton.onClick.AddListener(OpenGraphicsSettings);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(OpenCredits);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (audioSettingsMenuUI != null)
            audioSettingsMenuUI.ForceClose(false);

        if (graphicsSettingsMenuUI != null)
            graphicsSettingsMenuUI.ForceClose(false);

        if (loadGameButton != null && SaveManager.Instance != null)
            loadGameButton.interactable = SaveManager.Instance.HasSaveFile();
    }

    private void NewGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.NewGame();

        SceneManager.LoadScene(hubSceneName);
    }

    private void LoadGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.LoadGame();

        SceneManager.LoadScene(hubSceneName);
    }

    private void OpenAudioSettings()
    {
        if (audioSettingsMenuUI != null)
        {
            audioSettingsMenuUI.OpenFrom(mainMenuPanel);
            return;
        }

        Debug.LogWarning("MainMenuController has no AudioSettingsMenuUI assigned.");
    }

    private void OpenGraphicsSettings()
    {
        if (graphicsSettingsMenuUI != null)
        {
            graphicsSettingsMenuUI.OpenFrom(mainMenuPanel);
            return;
        }

        Debug.LogWarning("MainMenuController has no GraphicsSettingsMenuUI assigned.");
    }

    private void OpenCredits()
    {
        if (creditsMenuUI != null)
        {
            creditsMenuUI.OpenFrom(mainMenuPanel);
            return;
        }

        Debug.LogWarning("MainMenuController has no CreditsMenuUI assigned.");
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

//------MainMenuController.cs END-----