using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraState
{
    public Vector3 position;
    public bool isOrthographic;
}


[System.Serializable]
public class SavedLevelSegment
{
    public int prefabIndex;
    public Vector2Int gridPosition;
    public Direction[] availableDirections;
}

[System.Serializable]
public class LevelState
{
    public List<SavedLevelSegment> segments = new List<SavedLevelSegment>();
    public Vector2Int currentGridPos;
    public Direction currentDirection;
    public List<SegmentSpawnState> spawnStates = new List<SegmentSpawnState>();
}

[System.Serializable]
public class SpawnedEnemyState
{
    public int spawnPointIndex; 
    public string prefabName; 
    public WorldType worldType;
    public string uniqueID;
}

[System.Serializable]
public class SpawnedLootState
{
    public int spawnPointIndex; 
    public string prefabName;  
    public WorldType worldType; 
    public string uniqueID;   
    public bool isCollected;   
}

[System.Serializable]
public class SegmentSpawnState
{
    public Vector2Int segmentPosition;
    public List<SpawnedEnemyState> enemies = new List<SpawnedEnemyState>();
    public List<SpawnedLootState> loot = new List<SpawnedLootState>();
}

[System.Serializable]
public class GameState
{
    public bool enemyDefeated;
    public PandaDialogueState pandaState;
    public bool isInSpiritWorld;
    public Vector3 playerPosition = new Vector3(-7.7f, 0, 0);
    public List<string> collectedItems = new List<string>();
    
    public bool hasPlayedIntro = false;
    public int totalCoins = 0;
    public int totalPotions = 0;
    public int damageBoostCount = 0;
    public int invincibilityCount = 0;
    public bool pandaRewardGiven;
    
    
    public bool isPlayerDead = false;
    public bool hasRune = false;
    
    public CameraState cameraState;
    public Vector3 lastCameraPosition;
    
    public List<SavedLevelSegment> levelSegments = new();
    public LevelState levelState = new LevelState();

    public string currentDialogueId = ""; 
    public string currentDialogueFile = "";
    
    public int fallStatus = 0;
    
    public bool isExitPlankOpen = false;
    public bool isEntrancePlankClosed = false;
    
    public bool isBossDead = false;
    
    public int currentHealth = 500;
    public int maxHealth = 500;
}