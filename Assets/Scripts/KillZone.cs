using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter triggered, colliding with: {other.name} (Tag: {other.tag})");

        Character character = other.GetComponentInParent<Character>();
        if (character != null)
        {
            character.TakeDamage(9999); 
        }
    }
}