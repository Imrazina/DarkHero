using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class WorldSwitcher : MonoBehaviour
{
public GameObject normalWorld;
    public GameObject spiritWorld;
    public Transform player;
    
    [Header("Sound Effects")]
    public AudioClip worldSwitchSound;
    private AudioSource audioSource;

    private void Start()
    {
        LoadWorldState();
        LoadCameraState();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && FindObjectOfType<PlayerInventory>().hasRune)
        {
            SwitchWorld();
        }
    }

    void SwitchWorld()
    {
        if (worldSwitchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(worldSwitchSound, 0.4f);
        }
    
        GameStateManager.Instance.CurrentState.isInSpiritWorld = !GameStateManager.Instance.CurrentState.isInSpiritWorld;
        GameStateManager.Instance.CurrentState.playerPosition = player.position;
        SaveCameraState();
        UpdateWorldState();
        RefreshAllSpawnedObjects();
    
        FindObjectOfType<SubtitleManager>().ShowSubtitle(
            GameStateManager.Instance.CurrentState.isInSpiritWorld ? "WORLD OF SPIRITS" : "WORLD OF PEOPLE", 3f);
    }

    void RefreshAllSpawnedObjects()
    {
        bool isSpiritWorld = GameStateManager.Instance.CurrentState.isInSpiritWorld;

        var allLoot = FindObjectsOfType<LootItem>(true).Where(l => !l.isStaticLoot);
        foreach (var loot in allLoot)
        {
            if (loot.pickedUp || GameStateManager.Instance.CurrentState.collectedItems.Contains(loot.uniqueID))
            {
                loot.gameObject.SetActive(false);
                continue;
            }

            var worldComponent = loot.GetComponent<WorldDependentObject>();
            bool isInCorrectWorld = worldComponent == null || 
                                    (isSpiritWorld && worldComponent.worldType == WorldType.SpiritWorld) ||
                                    (!isSpiritWorld && worldComponent.worldType == WorldType.PeopleWorld);

            loot.gameObject.SetActive(isInCorrectWorld);
        }
        
        var allEnemies = FindObjectsOfType<EnemyAI>(true).Where(e => !e.isStaticEnemy);
        foreach (var enemy in allEnemies)
        {
            if (enemy.IsDead() || GameStateManager.Instance.CurrentState.collectedItems.Contains(enemy.uniqueID))
            {
                enemy.gameObject.SetActive(false);
                continue;
            }

            var worldComponent = enemy.GetComponent<WorldDependentObject>();
            bool isInCorrectWorld = worldComponent == null || 
                                    (isSpiritWorld && worldComponent.worldType == WorldType.SpiritWorld) ||
                                    (!isSpiritWorld && worldComponent.worldType == WorldType.PeopleWorld);

            enemy.gameObject.SetActive(isInCorrectWorld);
        }
    }

    void UpdateWorldState()
    {
        bool isSpiritWorld = GameStateManager.Instance.CurrentState.isInSpiritWorld;
        normalWorld.SetActive(!isSpiritWorld);
        spiritWorld.SetActive(isSpiritWorld);
        
        WorldDependentObject[] worldObjects = FindObjectsOfType<WorldDependentObject>(true);
        foreach (var obj in worldObjects)
        {
            obj.UpdateVisibility(isSpiritWorld);
        }
        
        LevelSegmentSpawner[] spawners = FindObjectsOfType<LevelSegmentSpawner>();
        foreach (var spawner in spawners)
        {
            spawner.ChangeBackground();
        }
    }

    
    private void SaveCameraState()
    {
        var cam = Camera.main;
        GameStateManager.Instance.CurrentState.cameraState = new CameraState
        {
            position = cam.transform.position,
            isOrthographic = cam.orthographic
        };
    }

    private void LoadCameraState()
    {
        if (GameStateManager.Instance.CurrentState.cameraState != null)
        {
            var cam = Camera.main;
            cam.transform.position = GameStateManager.Instance.CurrentState.cameraState.position;
            cam.orthographic = GameStateManager.Instance.CurrentState.cameraState.isOrthographic;
        }
    }

    private void LoadWorldState()
    {
        UpdateWorldState();
        
        if (GameStateManager.Instance.CurrentState.playerPosition != Vector3.zero)
        {
            player.position = GameStateManager.Instance.CurrentState.playerPosition;
        }

        RefreshAllSpawnedObjects();
    }
    public void ResetWorlds()
    {
        GameStateManager.Instance.CurrentState.isInSpiritWorld = false;
        UpdateWorldState();
        
        GameStateManager.Instance.SaveGame();
    }
}
