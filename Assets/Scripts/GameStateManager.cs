using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            CurrentState.totalPotions = inventory.potionCount;
            CurrentState.damageBoostCount = inventory.damageBoostCount;
            CurrentState.invincibilityCount = inventory.invincibilityCount;
            CurrentState.hasRune = inventory.hasRune;
            
            if (inventory.hasRune && !CurrentState.collectedItems.Contains("Rune_1"))
            {
                CurrentState.collectedItems.Add("Rune_1");
            }
            else if (!inventory.hasRune && CurrentState.collectedItems.Contains("Rune_1"))
            {
                CurrentState.collectedItems.Remove("Rune_1");
            }
        }
        
        var tupikController = FindObjectOfType<TupikController>();
        if (tupikController != null)
        {
            CurrentState.isExitPlankOpen = tupikController.openExit;
            CurrentState.isEntrancePlankClosed = tupikController.closeEntrance;
        }
        
        var mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CurrentState.cameraState = new CameraState
            {
                position = mainCamera.transform.position,
                isOrthographic = mainCamera.orthographic,
            };
            Debug.Log($"Saved camera: {CurrentState.cameraState.position}");
        }
        
        var levelGenerator = FindObjectOfType<LevelGenerator>();
        if (levelGenerator != null)
        {
            levelGenerator.SaveLevelState();
        }
        
        var boss = FindObjectOfType<BossAI>();
        if (boss != null)
        {
            CurrentState.isBossDead = boss.isDead;
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
        
            var statsManager = FindObjectOfType<StatsManager>();
            if (statsManager != null)
            {
                statsManager.SetCoins(CurrentState.totalCoins);
            }
            
            var tupikController = FindObjectOfType<TupikController>();
            if (tupikController != null)
            {
                tupikController.openExit = CurrentState.isExitPlankOpen;
                tupikController.closeEntrance = CurrentState.isEntrancePlankClosed;
                tupikController.LoadPlanksState();
            }
            
            var inventory = FindObjectOfType<PlayerInventory>();
            if (inventory != null)
            {
                inventory.potionCount = CurrentState.totalPotions;
                inventory.damageBoostCount = CurrentState.damageBoostCount;
                inventory.invincibilityCount = CurrentState.invincibilityCount;
                inventory.hasRune = CurrentState.collectedItems.Contains("Rune_1");
                inventory.UpdateAllUI(); 
            
                if (inventory.runeIcon != null)
                    inventory.runeIcon.color = inventory.hasRune ? Color.white : Color.black;
            }
            
            var mainCamera = Camera.main;
            if (mainCamera != null && CurrentState.cameraState != null)
            {
                mainCamera.transform.position = CurrentState.cameraState.position;
                mainCamera.orthographic = CurrentState.cameraState.isOrthographic;
                Debug.Log($"Restored camera: {CurrentState.cameraState.position}");
            }
            
            var player = FindObjectOfType<Character>();
            if (player != null)
            {
                player.Resurrect();
            }
            
            var screenFade = FindObjectOfType<ScreenFade>();
            if (screenFade != null)
            {
                screenFade.FadeIn(0.5f);
            }
            
            var allLootItems = FindObjectsOfType<LootItem>(true); 
            foreach (var loot in allLootItems)
            {
                if (CurrentState.collectedItems.Contains(loot.uniqueID))
                {
                    loot.pickedUp = true;
                    loot.gameObject.SetActive(false);
                }
                else
                {
                    loot.pickedUp = false;
                    loot.gameObject.SetActive(true);
                    var collider = loot.GetComponent<Collider2D>();
                    if (collider != null)
                        collider.enabled = true;
                }
            }
            
            var allRunes = FindObjectsOfType<RunePickup>(true);
            foreach (var rune in allRunes)
            {
                if (CurrentState.collectedItems.Contains(rune.uniqueID))
                {
                    rune.gameObject.SetActive(false);
                }
            }
            
            var levelGenerator = FindObjectOfType<LevelGenerator>();
            if (levelGenerator != null)
            {
                levelGenerator.LoadLevelState();
            }
            
            var boss = FindObjectOfType<BossAI>();
            if (boss != null && CurrentState.isBossDead)
            {
                boss.Die(); 
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
            totalPotions = 0,
            damageBoostCount = 0,
            invincibilityCount = 0,
            isExitPlankOpen = false,
            isEntrancePlankClosed = false,
            isBossDead = false,
            levelState = new LevelState()
            {
                segments = new List<SavedLevelSegment>(),
                spawnStates = new List<SegmentSpawnState>()
            },
            fallStatus = 0,
        };
    }
    
    public void FullReset()
    {
        ResetGame(); 
        DeleteSaveFile();
        
        CurrentState.collectedItems.RemoveAll(id => id == "Rune_1");
        CurrentState.hasRune = false;
        
        var runes = FindObjectsOfType<RunePickup>(true);
        foreach (var rune in runes)
        {
            rune.gameObject.SetActive(true);
            if (rune.pickupCollider != null) 
                rune.pickupCollider.enabled = true;
        }
        
        Debug.Log($"Rune reset: In collectedItems? {CurrentState.collectedItems.Contains("Rune_1")}");
        
        var levelGenerator = FindObjectOfType<LevelGenerator>();
        if (levelGenerator != null)
        {
            levelGenerator.ClearExistingLevel();
            levelGenerator.GenerateLevel(); 
        } 
        
        var allEnemies = FindObjectsOfType<EnemyAI>(true); 
        foreach (var enemy in allEnemies)
        {
            enemy.gameObject.SetActive(true);
            enemy.ResetEnemy(); 
        }
       
        var boss = FindObjectOfType<BossAI>(true);
       if (boss != null && !CurrentState.isBossDead)
       {
           boss.gameObject.SetActive(true);
           boss.ResetBoss();
       }
       
       var allLoot = FindObjectsOfType<LootItem>(true);
       foreach (var loot in allLoot)
       {
           loot.gameObject.SetActive(true);
           loot.ResetLoot();
       }
        
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
        
        var statsManager = FindObjectOfType<StatsManager>();
        if (statsManager != null)
        {
            statsManager.SetCoins(0);  
        }
        
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0.16f, 0.95f, -10); 
        }
    }
    
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Файл сохранения удален: " + saveFilePath);
        }
        string backupPath = saveFilePath + ".bak";
        if (File.Exists(backupPath))
        {
            File.Delete(backupPath);
            Debug.Log("Резервный файл сохранения удален: " + backupPath);
        }
        CurrentState = new GameState();
        PlayerPrefs.DeleteKey("GameSaved");
        PlayerPrefs.Save();
    }
}