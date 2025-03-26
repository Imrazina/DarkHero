using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasRune = false;
    
    public SpriteRenderer runeIcon; // Ссылка на иконку руны (SpriteRenderer)

    public void PickUpRune()
    {
        hasRune = true;

        // Меняем цвет иконки на нормальный
        if (runeIcon != null)
        {
            runeIcon.color = Color.white; // Или любой другой цвет
        }
    }
}
