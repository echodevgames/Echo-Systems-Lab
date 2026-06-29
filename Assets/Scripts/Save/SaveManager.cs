//-----SaveManager.cs START-----
using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public event Action OnGameSaved;
    public event Action OnGameLoaded;

    public static SaveManager Instance { get; private set; }

    private const string SaveFileName = "echo_systems_lab_save.json";

    [Header("Databases")]
    [SerializeField] private WeaponDatabase weaponDatabase;

    private SaveData currentSaveData = new SaveData();

    public SaveData CurrentSaveData => currentSaveData;
    public WeaponDatabase WeaponDatabase => weaponDatabase;

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame();
    }

    public void NewGame()
    {
        currentSaveData = new SaveData();
        currentSaveData.hasStartedGame = true;
        currentSaveData.lastSceneName = "Hub";

        MissionProgress.LoadFromSaveData(currentSaveData);
        PlayerProgress.LoadFromSaveData(currentSaveData);

        SaveGame();

        Debug.Log("New game created.");
    }

    public void SaveGame()
    {
        MissionProgress.WriteToSaveData(currentSaveData);
        PlayerProgress.WriteToSaveData(currentSaveData);

        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"Game saved to: {SavePath}");

        OnGameSaved?.Invoke();
    }

    public bool LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            currentSaveData = new SaveData();

            MissionProgress.LoadFromSaveData(currentSaveData);
            PlayerProgress.LoadFromSaveData(currentSaveData);

            Debug.Log("No save file found. Created fresh save data in memory.");

            OnGameLoaded?.Invoke();
            return false;
        }

        string json = File.ReadAllText(SavePath);
        currentSaveData = JsonUtility.FromJson<SaveData>(json);

        if (currentSaveData == null)
            currentSaveData = new SaveData();

        MissionProgress.LoadFromSaveData(currentSaveData);
        PlayerProgress.LoadFromSaveData(currentSaveData);

        Debug.Log($"Game loaded from: {SavePath}");
        OnGameLoaded?.Invoke();
        return true;
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        currentSaveData = new SaveData();

        MissionProgress.LoadFromSaveData(currentSaveData);
        PlayerProgress.LoadFromSaveData(currentSaveData);

        Debug.Log("Save file deleted.");
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }
}

//-----SaveManager.cs END-----