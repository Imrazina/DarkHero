using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public int lightAttackDamage = 10;
    public int heavyAttackDamage = 30;
    public int mediumAttackDamage = 20;
    public float activeTime = 0.2f;
    public float attackRange = 1f; 
    public LayerMask lootLayer;
    
    public BoxCollider2D hitboxCollider;
    private string currentAttackType;
    void Start()
    {
        hitboxCollider = GetComponent<BoxCollider2D>();  
        if (hitboxCollider == null)
        {
            Debug.LogError("Collider не найден в HitboxController! Проверь объект AttackHitBox.");
        }
        hitboxCollider.enabled = false;
    }
    
    public void ActivateHitbox(string attackType)
    {
        if (hitboxCollider == null)
        {
            Debug.LogError("Collider не найден в HitboxController! Проверь объект AttackHitBox.");
            return;
        }
        
        currentAttackType = attackType;
        hitboxCollider.enabled = true;
        hitboxCollider.transform.rotation = transform.rotation; 
        Debug.Log("Hitbox активирован для атаки: " + attackType);
        Invoke("DeactivateHitbox", activeTime);
    }

    public void DeactivateHitbox()
    {
        hitboxCollider.enabled = false;
        Debug.Log("Hitbox отключён.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter triggered, colliding with: {other.name} (Tag: {other.tag})");

        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyController>() ?? other.GetComponentInParent<EnemyController>();
            var enemyAI = other.GetComponent<EnemyAI>() ?? other.GetComponentInParent<EnemyAI>();
            var bossAI = other.GetComponent<BossAI>() ?? other.GetComponentInParent<BossAI>();

            int damage = 0;
            switch (currentAttackType)
            {
                case "LightAttack": damage = lightAttackDamage; break;
                case "MediumAttack": damage = mediumAttackDamage; break;
                case "HeavyAttack": damage = heavyAttackDamage; break;
            }

            if (enemy != null)
            {
                Debug.Log($"Hit {enemy.name} with {damage} damage!");
                enemy.TakeDamage(damage, currentAttackType);
            }
            else if (enemyAI != null)
            {
                Debug.Log($"Hit {enemyAI.name} with {damage} damage!");
                enemyAI.TakeDamage(damage);
            }
            else if (bossAI != null)
            {
                Debug.Log($"Hit {bossAI.name} with {damage} damage!");
                bossAI.TakeDamage(damage); // Убедись, что метод такой есть
            }
            else
            {
                Debug.LogWarning("Объект с тегом 'Enemy' не имеет подходящего компонента для получения урона.");
            }
        }
    }
}