using UnityEngine;

public class LootItem : MonoBehaviour
{
    public enum LootType { TouchPickup, AttackPickup }
    public LootType lootType;

    public int value = 1;  
    public string uniqueID;

    public bool pickedUp = false;  
    
    public GameObject pickupEffect;
    public AudioClip pickupSound;
    
    [Header("Loot Sound Settings")]
    [Range(0, 2)] public float lootSoundVolume = 1.5f;
    
    public bool isStaticLoot = false;

    private void Start()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = $"ManualLoot_{transform.GetSiblingIndex()}_{transform.position.x}_{transform.position.y}";
        }

        if (GameStateManager.Instance.CurrentState.collectedItems.Contains(uniqueID))
        {
            pickedUp = true;
            gameObject.SetActive(false);
            return;
        }

        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp) return;
        
        if (lootType == LootType.TouchPickup && other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    public void TakeHit()
    {
        if (pickedUp) return;
        
        if (lootType == LootType.AttackPickup)
        {
            Collect();
        }
    }
    
    private void Collect()
    {
        if (pickedUp) return;
        pickedUp = true;

        if (!string.IsNullOrEmpty(uniqueID));
        GameStateManager.Instance.CurrentState.collectedItems.Add(uniqueID);
    
        GameStateManager.Instance.CurrentState.totalCoins += value;
        StatsManager.Instance.AddCoins(value);

        if (pickupSound)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, lootSoundVolume);

        // Отключаем коллайдер перед скрытием
        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        gameObject.SetActive(false); 
    }
    
    public void ResetLoot()
    {
        pickedUp = false;
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true;
    }
}