using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunePickup : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private bool isPickedUp = false;

    public GameObject pickupEffect;
    public AudioClip pickupSound;

    public string uniqueID = "Rune_1"; 
    public SpriteRenderer runeIcon;

    private void Start()
    {
        if (GameStateManager.Instance.CurrentState.collectedItems.Contains(uniqueID))
        {
            Destroy(gameObject);
        }
    }

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
        
        GameStateManager.Instance.CurrentState.collectedItems.Add(uniqueID);

        FindObjectOfType<PlayerInventory>().PickUpRune();

        if (runeIcon != null)
        {
            runeIcon.color = Color.white;
        }

        if (pickupEffect)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        FindObjectOfType<SubtitleManager>().ShowSubtitle("What was that..?", 3f);

        Destroy(gameObject);
    }
}
