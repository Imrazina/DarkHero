using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public int potionPrice = 1;
    public int damageBoostPrice = 2;
    public int invincibilityPrice = 3;
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
    public void BuyDamageBoost()
    {
        int playerGold = GameStateManager.Instance.CurrentState.totalCoins;
        if (playerGold >= damageBoostPrice)
        {
            StatsManager.Instance.SetCoins(playerGold - damageBoostPrice);
            playerInventory.AddDamageBoost(1); 
            Debug.Log("Буст урона добавлен в инвентарь!");
        }
        else
        {
            Debug.Log("Недостаточно золота!");
        }
    }

    public void BuyInvincibility()
    {
        int playerGold = GameStateManager.Instance.CurrentState.totalCoins;
        if (playerGold >= invincibilityPrice)
        {
            StatsManager.Instance.SetCoins(playerGold - invincibilityPrice);
            playerInventory.AddInvincibility(1); 
            Debug.Log("Неуязвимость добавлена в инвентарь!");
        }
        else
        {
            Debug.Log("Недостаточно золота!");
        }
    }
}