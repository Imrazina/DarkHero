using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSwitcher : MonoBehaviour
{
    public GameObject normalWorld;
    public GameObject spiritWorld;
    public Transform player;

    private bool isInSpiritWorld;

    private void Start()
    {
        // Восстановить состояние мира
        isInSpiritWorld = GameStateManager.Instance.CurrentState.isInSpiritWorld;

        Vector3 savedPosition = GameStateManager.Instance.CurrentState.playerPosition;
        if (savedPosition != Vector3.zero)
        {
            player.position = savedPosition;
        }

        UpdateWorldState();
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
        isInSpiritWorld = !isInSpiritWorld;
        GameStateManager.Instance.CurrentState.isInSpiritWorld = isInSpiritWorld;

        GameStateManager.Instance.CurrentState.playerPosition = player.position;

        UpdateWorldState();

        FindObjectOfType<SubtitleManager>().ShowSubtitle(
            isInSpiritWorld ? "WORLD OF SPIRITS" : "WORLD OF PEOPLE", 3f);
    }

    void UpdateWorldState()
    {
        normalWorld.SetActive(!isInSpiritWorld);
        spiritWorld.SetActive(isInSpiritWorld);
    }
}
