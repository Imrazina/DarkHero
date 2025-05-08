using System.Collections.Generic;
using UnityEngine;

public class LevelSegmentSpawner : MonoBehaviour
{
    [Header("Enemies")]
    public GameObject[] enemyPrefabs;
    public Transform[] enemySpawnPoints;

    [Header("Loot")]
    public GameObject[] lootPrefabs;
    public Transform[] lootSpawnPoints;

    [Header("Decorations")]
    public GameObject[] decorationPrefabs;
    public Transform[] decorationSpawnPoints;
    
    [Header("Backgrounds")]
    public GameObject[] peopleWorldBackgrounds;
    public GameObject[] spiritWorldBackgrounds;

    private int lootIdCounter = 1;
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private List<GameObject> spawnedLoot = new List<GameObject>();
    private Vector2Int segmentGridPosition;

    void Start()
    {
        var segmentInfo = GetComponentInParent<SegmentInfo>();
        if (segmentInfo == null)
        {
            Debug.LogError("SegmentInfo not found in parent objects!");
            return;
        }
    
        segmentGridPosition = segmentInfo.GridPosition;
    
        if (ShouldSpawnNewObjects())
        {
            SpawnEnemies();
            SpawnLoot();
            SpawnDecorations();
        }
        else
        {
            LoadSpawnedObjects();
        }
    
        ChangeBackground();
    }
    
    private bool ShouldSpawnNewObjects()
    {
        var levelState = GameStateManager.Instance.CurrentState.levelState;
        var savedState = levelState.spawnStates.Find(s => s.segmentPosition == segmentGridPosition);
        if (savedState == null) 
            return true;
        
        return savedState.enemies.Count == 0 && savedState.loot.Count == 0;
    }

    void SpawnEnemies()
    {
        spawnedEnemies.Clear();
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);

            var worldComponent = enemy.AddComponent<WorldDependentObject>();
            var spawnerWorld = spawnPoint.GetComponent<WorldDependentObject>();
            
            var spriteRenderer = enemy.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 2; 
            }

            if (spawnerWorld != null)
            {
                worldComponent.worldType = spawnerWorld.worldType;
            }
            else
            {
                worldComponent.worldType = WorldType.AlwaysActive; 
            }
        }
    }

    void SpawnLoot()
    {
        spawnedLoot.Clear();
        lootIdCounter = 1;
        
        foreach (Transform spawnPoint in lootSpawnPoints)
        {
            GameObject selectedLoot = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
            Vector3 spawnPosition = spawnPoint.position;
            
            string potentialID = $"Loot_{lootIdCounter}";
            if (GameStateManager.Instance.CurrentState.collectedItems.Contains(potentialID))
            {
                lootIdCounter++;
                continue;
            }

            if (selectedLoot.name.ToLower().Contains("coin") || selectedLoot.name.ToLower().Contains("goldshape"))
            {
                spawnPosition.y += 2f;
            }

            GameObject lootInstance = Instantiate(selectedLoot, spawnPosition, Quaternion.identity, transform);
            spawnedLoot.Add(lootInstance);
            lootInstance.transform.localScale *= 1.5f;
            
            var worldComponent = lootInstance.AddComponent<WorldDependentObject>();
            var spawnerWorld = spawnPoint.GetComponent<WorldDependentObject>();
        
            var spriteRenderer = lootInstance.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 1; 
            }

            if (spawnerWorld != null)
            {
                worldComponent.worldType = spawnerWorld.worldType;
            }
            else
            {
                worldComponent.worldType = WorldType.AlwaysActive;
            }
            
            

            LootItem lootItem = lootInstance.GetComponent<LootItem>();
            if (lootItem != null)
            {
                lootItem.uniqueID = $"Loot_{lootIdCounter++}";
            }
        }
    }
    
     public void SaveSpawnedObjects()
{
    var levelState = GameStateManager.Instance.CurrentState.levelState;
    var spawnState = levelState.spawnStates.Find(s => s.segmentPosition == segmentGridPosition);
    
    if (spawnState == null)
    {
        spawnState = new SegmentSpawnState { segmentPosition = segmentGridPosition };
        levelState.spawnStates.Add(spawnState);
    }
    else
    {
        spawnState.enemies.Clear();
        spawnState.loot.Clear();
    }

    // Сохраняем врагов по их точкам спавна
    for (int i = 0; i < enemySpawnPoints.Length; i++)
    {
        if (i >= spawnedEnemies.Count || spawnedEnemies[i] == null) continue;
        
        var enemy = spawnedEnemies[i];
        var worldComponent = enemy.GetComponent<WorldDependentObject>();
        
        spawnState.enemies.Add(new SpawnedEnemyState
        {
            spawnPointIndex = i, // Сохраняем индекс точки спавна
            prefabName = GetOriginalPrefabName(enemy),
            worldType = worldComponent != null ? worldComponent.worldType : WorldType.AlwaysActive,
            uniqueID = enemy.GetComponent<EnemyAI>()?.uniqueID ?? System.Guid.NewGuid().ToString()
        });
    }

    // Сохраняем лут по точкам спавна
    for (int i = 0; i < lootSpawnPoints.Length; i++)
    {
        if (i >= spawnedLoot.Count || spawnedLoot[i] == null) continue;
        
        var loot = spawnedLoot[i];
        var worldComponent = loot.GetComponent<WorldDependentObject>();
        var lootItem = loot.GetComponent<LootItem>();
        
        spawnState.loot.Add(new SpawnedLootState
        {
            spawnPointIndex = i, // Сохраняем индекс точки спавна
            prefabName = GetOriginalPrefabName(loot),
            worldType = worldComponent != null ? worldComponent.worldType : WorldType.AlwaysActive,
            uniqueID = lootItem?.uniqueID ?? $"Loot_{lootIdCounter++}",
            isCollected = lootItem == null || GameStateManager.Instance.CurrentState.collectedItems.Contains(lootItem.uniqueID)
        });
    }
    }

    private string GetOriginalPrefabName(GameObject obj)
    {
        // Убираем "(Clone)" и номер, если есть
        return obj.name.Split('(')[0].Trim();
    }

    private void LoadSpawnedObjects()
    {
    var levelState = GameStateManager.Instance.CurrentState.levelState;
    var spawnState = levelState.spawnStates.Find(s => s.segmentPosition == segmentGridPosition);
    
    if (spawnState == null) return;

    spawnedEnemies.Clear();
    spawnedLoot.Clear();
    
    bool isSpiritWorld = GameStateManager.Instance.CurrentState.isInSpiritWorld;

    // Загружаем врагов
    foreach (var enemyState in spawnState.enemies)
    {
        if (enemyState.spawnPointIndex >= enemySpawnPoints.Length) continue;
        
        var spawnPoint = enemySpawnPoints[enemyState.spawnPointIndex];
        var prefab = System.Array.Find(enemyPrefabs, p => p.name == enemyState.prefabName);
        if (prefab == null) continue;
        
        GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, transform);
        spawnedEnemies.Add(enemy);
        
        var worldComponent = enemy.GetComponent<WorldDependentObject>();
        if (worldComponent == null)
        {
            worldComponent = enemy.AddComponent<WorldDependentObject>();
        }
        
        worldComponent.worldType = enemyState.worldType;
        worldComponent.UpdateVisibility(isSpiritWorld);
        
        var enemyComponent = enemy.GetComponent<EnemyAI>();
        if (enemyComponent != null)
        {
            enemyComponent.uniqueID = enemyState.uniqueID;
        }
    }

    // Загружаем лут
    foreach (var lootState in spawnState.loot)
    {
        if (lootState.isCollected || lootState.spawnPointIndex >= lootSpawnPoints.Length) continue;
        
        var spawnPoint = lootSpawnPoints[lootState.spawnPointIndex];
        var prefab = System.Array.Find(lootPrefabs, p => p.name == lootState.prefabName);
        if (prefab == null) continue;
        
        GameObject loot = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, transform);
        spawnedLoot.Add(loot);
        
        var worldComponent = loot.GetComponent<WorldDependentObject>();
        if (worldComponent == null)
        {
            worldComponent = loot.AddComponent<WorldDependentObject>();
        }
        
        worldComponent.worldType = lootState.worldType;
        worldComponent.UpdateVisibility(isSpiritWorld);
        
        var lootItem = loot.GetComponent<LootItem>();
        if (lootItem != null)
        {
            lootItem.uniqueID = lootState.uniqueID;
        }
    }
}

    void SpawnDecorations()
    {
        foreach (Transform spawnPoint in decorationSpawnPoints)
        {
            GameObject selectedDecoration = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
            GameObject decor = Instantiate(selectedDecoration, spawnPoint.position, Quaternion.identity, transform);
            
            if (Random.value > 0.5f)
            {
                Vector3 scale = decor.transform.localScale;
                scale.x *= -1;
                decor.transform.localScale = scale;
            }

            if (Random.value > 0.7f)
            {
                decor.transform.Rotate(Vector3.forward * Random.Range(-10f, 10f));
            }
        }
    }

    public void ChangeBackground()
    {
        bool isSpiritWorld = GameStateManager.Instance.CurrentState.isInSpiritWorld;

        // Выбираем нужный массив бэкграундов
        GameObject[] currentBackgrounds = isSpiritWorld ? spiritWorldBackgrounds : peopleWorldBackgrounds;

        // Выключаем ВСЕ бэкграунды (и людей, и духов)
        foreach (GameObject background in peopleWorldBackgrounds)
        {
            background.SetActive(false);
        }
        foreach (GameObject background in spiritWorldBackgrounds)
        {
            background.SetActive(false);
        }

        // Ставим случайный фон из правильного мира
        if (currentBackgrounds.Length > 0)
        {
            int randomIndex = Random.Range(0, currentBackgrounds.Length);
            currentBackgrounds[randomIndex].SetActive(true);
        }
    }
}