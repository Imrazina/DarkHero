using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject[] levelPrefabs;
    public Transform spawnPoint;

    private Transform lastExitPoint;

    void Start()
    {
        lastExitPoint = null; 
        GenerateLevelRandom();
    }

    void GenerateLevelRandom()
    {
   Vector3 currentPosition = spawnPoint.position;
   currentPosition.z = 0f;

    for (int i = 0; i < 5; i++)
    {
        int prefabIndex = Random.Range(0, levelPrefabs.Length);
        GameObject prefab = levelPrefabs[prefabIndex];
        GameStateManager.Instance.CurrentState.levelPrefabIndices.Add(prefabIndex);

        GameObject temp = Instantiate(prefab);
        BoxCollider2D col = temp.GetComponent<BoxCollider2D>();
        Transform entry = temp.transform.Find("EntryPoint");
        Transform exit = temp.transform.Find("ExitPoint");

        if (col == null || entry == null || exit == null)
        {
            Debug.LogError(prefab.name + " missing components!");
            Destroy(temp);
            continue;
        }

        Vector3 spawnPos;

        if (i == 0)
        {
            Vector3 offset = col.bounds.center - temp.transform.position;
            spawnPos = currentPosition - offset;
        }
        else
        {
            Vector3 offset = entry.position - temp.transform.position;
            spawnPos = currentPosition - offset;
        }

        Destroy(temp);

        GameObject levelSegment = Instantiate(prefab, spawnPos, Quaternion.identity);
        Debug.Log("Spawned " + levelSegment.name + " at " + spawnPos);
        
        Transform newExit = levelSegment.transform.Find("ExitPoint");
        if (newExit != null)
        {
            currentPosition = newExit.position;
        }

        Transform forwardCamera = levelSegment.transform.Find("Forward");
        Transform backwardCamera = levelSegment.transform.Find("Backward");
        Transform cameraTrigger = levelSegment.transform.Find("ExitTriggerRight");

        if (cameraTrigger != null)
        {
            CameraSwitchTrigger switcher = cameraTrigger.GetComponent<CameraSwitchTrigger>();
            if (switcher != null)
            {
                switcher.cameraPositionForward = forwardCamera;
                switcher.cameraPositionBackward = backwardCamera;
            }
        }
    }
    }
    
    public void GenerateLevelFromSave(List<int> indices)
    {
        Vector3 currentPosition = spawnPoint.position;
        currentPosition.z = 0f;

        for (int i = 0; i < indices.Count; i++)
        {
            int prefabIndex = indices[i];
            GameObject prefab = levelPrefabs[prefabIndex];

            GameObject temp = Instantiate(prefab);
            BoxCollider2D col = temp.GetComponent<BoxCollider2D>();
            Transform entry = temp.transform.Find("EntryPoint");
            Transform exit = temp.transform.Find("ExitPoint");

            if (col == null || entry == null || exit == null)
            {
                Debug.LogError(prefab.name + " missing components!");
                Destroy(temp);
                continue;
            }

            Vector3 spawnPos;
            if (i == 0)
            {
                Vector3 offset = col.bounds.center - temp.transform.position;
                spawnPos = currentPosition - offset;
            }
            else
            {
                Vector3 offset = entry.position - temp.transform.position;
                spawnPos = currentPosition - offset;
            }

            Destroy(temp);

            GameObject levelSegment = Instantiate(prefab, spawnPos, Quaternion.identity);

            Transform newExit = levelSegment.transform.Find("ExitPoint");
            if (newExit != null)
            {
                currentPosition = newExit.position;
            }
            
            Transform forwardCamera = levelSegment.transform.Find("Forward");
            Transform backwardCamera = levelSegment.transform.Find("Backward");
            Transform cameraTrigger = levelSegment.transform.Find("ExitTriggerRight");

            if (cameraTrigger != null)
            {
                CameraSwitchTrigger switcher = cameraTrigger.GetComponent<CameraSwitchTrigger>();
                if (switcher != null)
                {
                    switcher.cameraPositionForward = forwardCamera;
                    switcher.cameraPositionBackward = backwardCamera;
                }
            }
        }
    }
}
