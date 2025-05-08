using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public int potionPrice = 1;
    public int damageBoostPrice = 2;
    public int invincibilityPrice = 3;
    public PlayerInventory playerInventory;
    
    [Header("Sound Effects")]
    public AudioClip purchaseSound;
    private AudioSource audioSource;
    [Range(0, 1)] public float purchaseSoundVolume = 0.4f;

    public void Start()
    {
        if (GameStateManager.Instance != null)
        {
            Debug.Log("ShopSystem initialized. Current gold: " + GameStateManager.Instance.CurrentState.totalCoins);
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void BuyPotion()
    {
        int playerGold = GameStateManager.Instance.CurrentState.totalCoins;

        if (playerGold >= potionPrice)
        {
            StatsManager.Instance.SetCoins(playerGold - potionPrice);
            playerInventory.AddPotion(1);
            
            if (purchaseSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(purchaseSound, purchaseSoundVolume);
            }
            
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
            if (purchaseSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(purchaseSound, purchaseSoundVolume);
            }
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
            if (purchaseSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(purchaseSound, purchaseSoundVolume);
            }
            Debug.Log("Неуязвимость добавлена в инвентарь!");
        }
        else
        {
            Debug.Log("Недостаточно золота!");
        }
    }
}