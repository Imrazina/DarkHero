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
        if (Input.GetKeyDown(KeyCode.E) && FindObjectOfType<PlayerInventory>().hasRune)
        {
            SwitchWorld();
        }
    }

    void SwitchWorld()
    {
        isInSpiritWorld = !isInSpiritWorld;

        // 1. Сохраняем позицию игрока перед сменой мира
        Vector3 playerPosition = player.position;

        // 2. Переключаем миры
        normalWorld.SetActive(!isInSpiritWorld);
        spiritWorld.SetActive(isInSpiritWorld);

        // 3. Восстанавливаем позицию игрока
        player.position = playerPosition;
        FindObjectOfType<SubtitleManager>().ShowSubtitle(isInSpiritWorld ? "WORLD OF SPIRITS" : "WORLD OF PEOPLE", 3f);
    }
}
