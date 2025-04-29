using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
        potionCount = GameStateManager.Instance.CurrentState.totalPotions;

        hasRune = GameStateManager.Instance.CurrentState.collectedItems.Contains("Rune_1");
        if (runeIcon != null)
            runeIcon.color = hasRune ? Color.white : Color.black;
    
        UpdatePotionsUI();
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
}
