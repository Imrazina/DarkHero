using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasRune = false;
    
    public SpriteRenderer runeIcon;

    public void PickUpRune()
    {
        hasRune = true;
        
        if (runeIcon != null)
        {
            runeIcon.color = Color.white; 
        }
    }
    
    public void ResetInventory()
    {
        hasRune = false;
        if (runeIcon != null)
        {
            runeIcon.color = Color.black;
        }
    }
}
