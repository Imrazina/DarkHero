using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSwitcher : MonoBehaviour
{
    public GameObject normalWorld;  // Обычный мир
    public GameObject spiritWorld;  // Мир духов
    public Transform player;  // Персонаж
    private bool isInSpiritWorld = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && FindObjectOfType<PlayerInventory>().hasRune)
        {
            SwitchWorld();
        }
    }

    void SwitchWorld()
    {
        isInSpiritWorld = !isInSpiritWorld;
        
        Vector3 playerPosition = player.position;
        
        normalWorld.SetActive(!isInSpiritWorld);
        spiritWorld.SetActive(isInSpiritWorld);
        
        player.position = playerPosition;
        FindObjectOfType<SubtitleManager>().ShowSubtitle(isInSpiritWorld ? "WORLD OF SPIRITS" : "WORLD OF PEOPLE", 3f);
    }
}
