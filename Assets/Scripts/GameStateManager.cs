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
        var player = FindObjectOfType<Character>();
        if (player != null)
        {
            CurrentState.playerPosition = player.transform.position;
            Debug.Log($"Saving position: {CurrentState.playerPosition}");
        }
    
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
            Debug.Log($"Game Loaded! Position: {CurrentState.playerPosition}");
            
            CurrentState.isPlayerDead = false;
            
            var player = FindObjectOfType<Character>();
            if (player != null)
            {
                player.Resurrect();
            }
        }
    }

    public void ResetGame()
    {
        CurrentState = new GameState()
        {
            playerPosition = new Vector3(-6, 0, 0),
            enemyDefeated = false,
            isInSpiritWorld = false,
            collectedItems = new List<string>(),
            hasPlayedIntro = false,
            totalCoins = 0,
            isPlayerDead = false,
            cameraState = new CameraState(), 
            lastCameraPosition = Vector3.zero,
        };
    }
    
    public void FullReset()
    {
        ResetGame(); 
        
        var player = FindObjectOfType<Character>();
        if (player != null)
        {
            player.Resurrect();
            player.transform.position = new Vector3(-7.7f, 0, 0);
        }
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.ResetInventory();
        }
        
        var allRunes = FindObjectsOfType<RunePickup>(true); 
        foreach (var rune in allRunes)
        {
            rune.SetRuneActive(true); 
        }
        
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0.16f, 0.95f, -10); 
        }
    }
}
