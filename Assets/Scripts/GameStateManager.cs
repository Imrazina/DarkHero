using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = new GameState();

    private string saveFilePath;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Application.persistentDataPath + "/savegame.json";
        }
       
        
        if (transform.parent != null)
            Debug.LogWarning("GameStateManager НЕ корневой объект! DontDestroyOnLoad не сработает.");
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentState, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game Saved to: " + saveFilePath);
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            CurrentState = JsonUtility.FromJson<GameState>(json);
            Debug.Log("Game Loaded from: " + saveFilePath);
        }
        else
        {
            Debug.LogWarning("No save file found!");
        }
    }

    public void ResetGame()
    {
        CurrentState = new GameState();
    }
}
