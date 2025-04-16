
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameState
{
    public bool enemyDefeated;
    public PandaDialogueState pandaState;
    public bool isInSpiritWorld;
    public Vector3 playerPosition;
    public List<string> collectedItems = new List<string>();
    
    public bool hasPlayedIntro = false;
    public int totalCoins = 0;
}