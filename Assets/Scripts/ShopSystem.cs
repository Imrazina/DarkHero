using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public int potionPrice = 1;
    public PlayerInventory playerInventory;

    public void Start()
    {
        if (GameStateManager.Instance != null)
        {
            Debug.Log("ShopSystem initialized. Current gold: " + GameStateManager.Instance.CurrentState.totalCoins);
        }
    }

    public void BuyPotion()
    {
        int playerGold = GameStateManager.Instance.CurrentState.totalCoins;

        if (playerGold >= potionPrice)
        {
            StatsManager.Instance.SetCoins(playerGold - potionPrice);

            playerInventory.AddPotion(1);
            Debug.Log("Покупка прошла успешно!");
        }
        else
        {
            Debug.Log("Недостаточно золота!");
        }
    }
}
