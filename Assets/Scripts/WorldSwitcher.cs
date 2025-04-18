using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WorldSwitcher : MonoBehaviour
{
public GameObject normalWorld;
    public GameObject spiritWorld;
    public Transform player;

    private void Start()
    {
        // Загружаем состояние мира и камеры
        LoadWorldState();
        LoadCameraState();
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
        GameStateManager.Instance.CurrentState.isInSpiritWorld = !GameStateManager.Instance.CurrentState.isInSpiritWorld;
        GameStateManager.Instance.CurrentState.playerPosition = player.position;
        SaveCameraState();
        UpdateWorldState();
        
        FindObjectOfType<SubtitleManager>().ShowSubtitle(
            GameStateManager.Instance.CurrentState.isInSpiritWorld ? "WORLD OF SPIRITS" : "WORLD OF PEOPLE", 3f);
    }

    void UpdateWorldState()
    {
        bool isSpiritWorld = GameStateManager.Instance.CurrentState.isInSpiritWorld;
        normalWorld.SetActive(!isSpiritWorld);
        spiritWorld.SetActive(isSpiritWorld);
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
    }
    public void ResetWorlds()
    {
        GameStateManager.Instance.CurrentState.isInSpiritWorld = false;
        UpdateWorldState();
        
        GameStateManager.Instance.SaveGame();
    }
}
