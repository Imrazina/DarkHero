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

    private void Start()
    {
        potionCount = GameStateManager.Instance.CurrentState.totalPotions;

        hasRune = GameStateManager.Instance.CurrentState.collectedItems.Contains("Rune_1");
        if (runeIcon != null)
            runeIcon.color = hasRune ? Color.white : Color.black;
        
        damageBoostCount = GameStateManager.Instance.CurrentState.damageBoostCount;
        invincibilityCount = GameStateManager.Instance.CurrentState.invincibilityCount;
    
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
        if (runeIcon != null)
            runeIcon.color = Color.white;
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

        UpdatePotionsUI();
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