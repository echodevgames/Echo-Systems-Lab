//------MainMenuController.cs START-----
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private CreditsMenuUI creditsMenuUI;

    [Header("Scenes")]
    [SerializeField] private string hubSceneName = "Hub";

    private void Awake()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettingsStub);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(OpenCredits);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
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

    private void OpenSettingsStub()
    {
        Debug.Log("Settings menu stub.");
    }

    private void OpenCredits()
    {
        if (creditsMenuUI != null)
            creditsMenuUI.OpenFrom(mainMenuPanel);
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