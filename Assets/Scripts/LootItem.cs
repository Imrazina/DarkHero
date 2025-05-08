using UnityEngine;

public class LootItem : MonoBehaviour
{
    public enum LootType { TouchPickup, AttackPickup }
    public LootType lootType;

    public int value = 1;  
    public string uniqueID;  

    private bool pickedUp = false;  
    
    public GameObject pickupEffect;
    public AudioClip pickupSound;
    
    [Header("Loot Sound Settings")]
    [Range(0, 2)] public float lootSoundVolume = 1.5f;

    private void Start()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = $"ManualLoot_{transform.GetSiblingIndex()}_{transform.position.x}_{transform.position.y}";
        }

        if (GameStateManager.Instance.CurrentState.collectedItems.Contains(uniqueID))
        {
            Destroy(gameObject);
        }
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
        pickedUp = true;  
        GameStateManager.Instance.CurrentState.collectedItems.Add(uniqueID);
        
   //     var savedLoot = GameStateManager.Instance.CurrentState.lootItems.Find(x => x.uniqueID == uniqueID);
      //  if (savedLoot != null) savedLoot.isCollected = true;

        GameStateManager.Instance.CurrentState.totalCoins += value;
        StatsManager.Instance.AddCoins(value); 

        if (pickupSound)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, lootSoundVolume);

        Destroy(gameObject);
    }
}