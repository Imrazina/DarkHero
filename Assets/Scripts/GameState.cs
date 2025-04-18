
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

[System.Serializable]
public class CameraState
{
    public Vector3 position;
    public bool isOrthographic;
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
    
    public bool isPlayerDead = false;
    
    public CameraState cameraState;
    public Vector3 lastCameraPosition;
    
}