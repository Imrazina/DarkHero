using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunePickup : MonoBehaviour
{
    private bool isPlayerNearby = false;
    public GameObject pickupEffect; // Эффект при поднятии
    public AudioClip pickupSound;   // Звук поднятия
    private bool isPickedUp = false;

    public SpriteRenderer runeIcon; // Иконка руны в UI (перетащи в Inspector!)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private void Update()
    {
        if (isPlayerNearby && !isPickedUp)
        {
            PickUp();
        }
    }

    void PickUp()
    {
        isPickedUp = true;
        FindObjectOfType<PlayerInventory>().hasRune = true; // Добавляем руну в инвентарь

        // 🎨 Меняем цвет иконки руны (если она указана в Inspector)
        if (runeIcon != null)
        {
            runeIcon.color = Color.white; // Или любой другой цвет
        }

        if (pickupEffect)
            Instantiate(pickupEffect, transform.position, Quaternion.identity); // Спавн эффекта

        if (pickupSound)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        FindObjectOfType<SubtitleManager>().ShowSubtitle("What was that..?", 3f);
        
        Destroy(gameObject); 
    }
}
