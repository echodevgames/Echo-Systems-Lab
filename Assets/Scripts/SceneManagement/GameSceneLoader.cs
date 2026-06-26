//-----GameSceneLoader.cs START-----

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    public static GameSceneLoader Instance { get; private set; }

    [SerializeField] private string hubSceneName = "Hub";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMissionScene(MissionData mission)
    {
        if (mission == null)
        {
            Debug.LogWarning("Tried to load a null mission.");
            return;
        }

        if (string.IsNullOrWhiteSpace(mission.sceneName))
        {
            Debug.LogWarning($"Mission '{mission.displayName}' has no scene name assigned.");
            return;
        }

        SceneManager.LoadScene(mission.sceneName);
    }

    public void LoadHub()
    {
        SceneManager.LoadScene(hubSceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}

//-----GameSceneLoader.cs END-----