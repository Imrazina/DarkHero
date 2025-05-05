using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 50; // Урон, который наносит босс
    private BossAI bossAI;  // Ссылка на скрипт босса (опционально)

    private void Start()
    {
        bossAI = GetComponentInParent<BossAI>(); // Автоматически находим BossAI
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Character player = other.GetComponent<Character>();
            if (player != null)
            {
                player.TakeDamage(damage); // Вызываем метод TakeDamage из Character
                Debug.Log("Босс ударил игрока! Урон: " + damage);
            }
        }
    }
}