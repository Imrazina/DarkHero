using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public bool hasRune = false;
    
    public SpriteRenderer runeIcon;
    
    public int potionCount = 0;
    public TMP_Text potionText; 
    public Image potionIcon;
    
    public int damageBoostCount = 0;
    public int invincibilityCount = 0;
    
    public TMP_Text damageBoostText;
    public Image damageBoostIcon;
    public TMP_Text invincibilityText; 
    public Image invincibilityIcon;
    
    [Header("Sounds")]
    public AudioClip potionIncreaseSound;

    private void Start()
    {
        potionCount = GameStateManager.Instance.CurrentState.totalPotions;
        hasRune = GameStateManager.Instance.CurrentState.collectedItems.Contains("Rune_1");
        
        if (runeIcon != null)
        {
            runeIcon.color = hasRune ? Color.white : Color.black;
        }
        else
        {
            Debug.LogError("Rune Icon не назначен в инспекторе!");
        }
        
        damageBoostCount = GameStateManager.Instance.CurrentState.damageBoostCount;
        invincibilityCount = GameStateManager.Instance.CurrentState.invincibilityCount;
        Debug.Log($"Rune state: Active={gameObject.activeSelf}, Collected={GameStateManager.Instance.CurrentState.collectedItems.Contains("Rune_1")}");

        UpdateAllUI();
    }
    
    public void UpdateAllUI()
    {
        UpdatePotionsUI();
        UpdateDamageBoostUI();
        UpdateInvincibilityUI();
    }


    public void PickUpRune()
    {
        hasRune = true;
        if (potionIncreaseSound != null)
            AudioSource.PlayClipAtPoint(potionIncreaseSound, Camera.main.transform.position);
        
        if (runeIcon != null)
        {
            runeIcon.color = Color.white;
        }
        else
        {
            Debug.LogError("Rune Icon is missing! Check inspector.");
        }
        
        GameStateManager.Instance.CurrentState.hasRune = true;
        GameStateManager.Instance.SaveGame();
    }
    public void AddPotion(int amount)
    {
        Debug.Log($"Adding {amount} potion(s). Current count: {potionCount}");
        potionCount += amount;
        GameStateManager.Instance.CurrentState.totalPotions = potionCount;
        UpdatePotionsUI();
        Debug.Log($"New potion count: {potionCount}");
    }

    public void ResetInventory()
    {
        hasRune = false;
        potionCount = 0;

        if (runeIcon != null)
            runeIcon.color = Color.black;

        UpdateAllUI();
    }

    public void UpdatePotionsUI()
    {
        if (potionText != null)
            potionText.text = potionCount.ToString();

        if (potionIcon != null)
            potionIcon.gameObject.SetActive(potionCount >= 0);
    }
    
    public void UpdateDamageBoostUI()
    {
        if (damageBoostText != null)
            damageBoostText.text = damageBoostCount.ToString();

        if (damageBoostIcon != null)
            damageBoostIcon.gameObject.SetActive(damageBoostCount >= 0);
    }
    
    public void UpdateInvincibilityUI()
    {
        if (invincibilityText != null)
            invincibilityText.text = invincibilityCount.ToString();

        if (invincibilityIcon != null)
            invincibilityIcon.gameObject.SetActive(invincibilityCount >= 0);
    }
    
    public bool UsePotion()
    {
        if (potionCount > 0)
        {
            potionCount--;
            GameStateManager.Instance.CurrentState.totalPotions = potionCount;
            UpdatePotionsUI();
            return true; 
        }
        return false; 
    }
    
    public void AddDamageBoost(int amount)
    {
        damageBoostCount += amount;
        GameStateManager.Instance.CurrentState.damageBoostCount = damageBoostCount;
        GameStateManager.Instance.SaveGame();
        UpdateDamageBoostUI();
        Debug.Log($"Бустов урона: {damageBoostCount}");
    }
    
    public bool UseDamageBoost()
    {
        if (damageBoostCount > 0)
        {
            damageBoostCount--;
            GameStateManager.Instance.CurrentState.damageBoostCount = damageBoostCount;
            GameStateManager.Instance.SaveGame();
            return true;
        }
        return false;
    }

    public void AddInvincibility(int amount)
    {
        invincibilityCount += amount;
        GameStateManager.Instance.CurrentState.invincibilityCount = invincibilityCount;
        GameStateManager.Instance.SaveGame();
        UpdateInvincibilityUI();
    }

    public bool UseInvincibility()
    {
        if (invincibilityCount > 0)
        {
            invincibilityCount--;
            GameStateManager.Instance.CurrentState.invincibilityCount = invincibilityCount;
            GameStateManager.Instance.SaveGame();
            return true;
        }
        return false;
    }
    
}