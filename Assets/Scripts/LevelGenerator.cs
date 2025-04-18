using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject[] levelPrefabs; 
    public Transform spawnPoint;

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        Vector3 currentPosition = spawnPoint.position;
        
        for (int i = 0; i < 5; i++)
        {
            int prefabIndex = Random.Range(0, levelPrefabs.Length);
            GameObject levelSegment = Instantiate(levelPrefabs[prefabIndex], currentPosition, Quaternion.identity);
            
            currentPosition.x += levelSegment.GetComponent<Collider2D>().bounds.size.x;
        }
    }
}
