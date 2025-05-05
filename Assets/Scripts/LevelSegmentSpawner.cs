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

    
    void Start()
    {
        SpawnEnemies();
        SpawnLoot();
        SpawnDecorations();
        ChangeBackground();
    }

    void SpawnEnemies()
    {
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            GameObject enemy = Instantiate(
                enemyPrefabs[Random.Range(0, enemyPrefabs.Length)],
                spawnPoint.position,
                Quaternion.identity,
                transform
            );

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