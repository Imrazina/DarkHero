using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunePickup : MonoBehaviour
{
    public string uniqueID = "Rune_1";
    public GameObject runeVisual; 
    public GameObject pickupEffect;
    public AudioClip pickupSound;
    public Collider2D pickupCollider;

    private void Start()
    {
        if (GameStateManager.Instance.CurrentState.collectedItems.Contains(uniqueID))
        {
            SetRuneActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && 
            !GameStateManager.Instance.CurrentState.collectedItems.Contains(uniqueID))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        GameStateManager.Instance.CurrentState.collectedItems.Add(uniqueID);
        FindObjectOfType<PlayerInventory>().PickUpRune();
        
        if (pickupEffect) Instantiate(pickupEffect, transform.position, Quaternion.identity);
        if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        
        SetRuneActive(false);
        
        FindObjectOfType<SubtitleManager>().ShowSubtitle("What was that..?", 3f);
    }

    public void SetRuneActive(bool isActive)
    {
        if (runeVisual != null)
        {
            runeVisual.SetActive(isActive);
            pickupCollider.enabled = isActive; // Отключаем коллайдер
        }
    }
}
