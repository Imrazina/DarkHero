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
    public GameObject[] backgrounds;

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
        }
    }

    void SpawnLoot()
    {
        foreach (Transform spawnPoint in lootSpawnPoints)
        {
            GameObject selectedLoot = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
            Vector3 spawnPosition = spawnPoint.position;
            
            if (selectedLoot.name.ToLower().Contains("coin"))
            {
                spawnPosition.y += 0.5f;
            }

            GameObject lootInstance = Instantiate(selectedLoot, spawnPosition, Quaternion.identity, transform);

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
    
    void ChangeBackground()
    {
        foreach (GameObject background in backgrounds)
        {
            background.SetActive(false);
        }
        int randomIndex = Random.Range(0, backgrounds.Length);
        backgrounds[randomIndex].SetActive(true);
    }
}
