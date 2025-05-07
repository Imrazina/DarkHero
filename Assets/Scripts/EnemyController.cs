using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int maxHealth = 1000;
    private int currentHealth;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int damage, string attackType)
    {
        if (isDead) 
        {
            Debug.Log("Enemy is dead, no damage taken.");
            return; 
        } 

        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage from {attackType}. Current health: {currentHealth}");
        
        switch (attackType)
        {
            case "LightAttack":
                animator.SetTrigger("HitLight");
                break;
            case "MediumAttack":
                animator.SetTrigger("HitMedium");
                break;
            case "HeavyAttack":
                animator.SetTrigger("HitHeavy");
                break;
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy died.");
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        animator.SetTrigger("Death"); 
        GetComponent<Collider>().enabled = false; 
        this.enabled = false; 
    }
}