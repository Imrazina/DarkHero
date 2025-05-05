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
            gameObject.SetActive(false);
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
        GameStateManager.Instance.CurrentState.hasRune = true;
        FindObjectOfType<PlayerInventory>().PickUpRune();
        
        if (pickupEffect) Instantiate(pickupEffect, transform.position, Quaternion.identity);
        if (pickupSound) AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        
        gameObject.SetActive(false);
        
        FindObjectOfType<SubtitleManager>().ShowSubtitle("What was that..?", 3f);
        GameStateManager.Instance.SaveGame();
    }

    
}