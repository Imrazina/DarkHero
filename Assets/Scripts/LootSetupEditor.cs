#if UNITY_EDITOR
using UnityEngine;

public class LootSetupEditor : MonoBehaviour
{
    [ContextMenu("Auto-Setup LootPickups")]
    void SetupLootPickups()
    {
        foreach (Transform category in transform) // Coins, GoldDore, Crystals, etc.
        {
            string categoryName = category.name;

            int i = 1;
            foreach (Transform loot in category)
            {
                LootItem pickup = loot.GetComponent<LootItem>();
                if (pickup == null)
                    pickup = loot.gameObject.AddComponent<LootItem>();
                
                pickup.uniqueID = $"{categoryName}_{i}";
                i++;
                
                if (categoryName.Contains("Coin"))
                {
                    pickup.lootType = LootItem.LootType.TouchPickup;  
                    pickup.value = 1;  
                }
                else if (categoryName.Contains("GoldShape"))
                {
                    pickup.lootType = LootItem.LootType.TouchPickup;
                    pickup.value = 5;
                }
                else if (categoryName.Contains("Crystal"))
                {
                    pickup.lootType = LootItem.LootType.AttackPickup;
                    pickup.value = 20;
                }
                else if (categoryName.Contains("GoldDore"))
                {
                    pickup.lootType = LootItem.LootType.AttackPickup;
                    pickup.value = 10;
                }
                else
                {
                    Debug.LogWarning($"Unknown loot type: {categoryName}");
                }

                Debug.Log($"Set up {pickup.uniqueID} [{pickup.lootType}, +{pickup.value}]");
            }
        }

        Debug.Log("Loot setup complete!");
    }
}

#endif