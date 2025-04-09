using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("Movement")]
    public float speed = 2f;
    public float detectionRange = 5f;
    public float minFollowDistance = 1f;
    private bool facingRight = true;
    private float lastFlipTime;
    public float flipCooldown = 1f;
    
    [Header("Damage Settings")]
    public float hitStunDuration = 0.5f; 
    private bool isInHitStun = false;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;

    [Header("Ground Check")]
    public Vector2 frontGroundCheck = new Vector2(0.5f, -0.5f);
    public Vector2 backGroundCheck = new Vector2(-0.5f, -0.5f);
    public float groundCheckDistance = 0.28f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.2f;

    [Header("References")]
    private Animator animator;
    private Transform target;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        if (isDead || target == null || isInHitStun) 
        {
            animator.SetBool("Run", false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        Vector2 directionToPlayer = (target.position - transform.position).normalized;

        // Определяем нужно ли двигаться к игроку
        bool shouldMove = distanceToPlayer <= detectionRange && 
                         distanceToPlayer > minFollowDistance;

        // Определяем правильное направление взгляда
        bool shouldFaceRight = directionToPlayer.x > 0;
        if (shouldFaceRight != facingRight && Time.time > lastFlipTime + flipCooldown)
        {
            Flip();
            lastFlipTime = Time.time;
        }

        // Атака если игрок в радиусе
        if (distanceToPlayer <= attackRange && canAttack)
        {
            StartCoroutine(Attack());
        }
        // Движение если можно
        else if (shouldMove && CanMoveTowards(directionToPlayer))
        {
            MoveTowardsPlayer(directionToPlayer);
        }
        else
        {
            animator.SetBool("Run", false);
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        DebugDrawRays();
    }

    bool CanMoveTowards(Vector2 direction)
    {
        // Проверка земли впереди
        Vector2 checkPos = (Vector2)transform.position + 
                         (facingRight ? frontGroundCheck : backGroundCheck);
        
        bool hasGround = Physics2D.Raycast(
            checkPos,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        // Проверка стены перед собой
        bool hasWall = Physics2D.Raycast(
            (Vector2)transform.position,
            facingRight ? Vector2.right : Vector2.left,
            wallCheckDistance,
            groundLayer
        );

        return hasGround && !hasWall;
    }

    void MoveTowardsPlayer(Vector2 direction)
    {
        animator.SetBool("Run", true);
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    IEnumerator Attack()
    {
        canAttack = false;
        animator.SetTrigger("Charge");
        yield return new WaitForSeconds(0.5f);

        animator.SetTrigger("Attack");
    
        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange)
        {
            AttackController playerAttack = target.GetComponent<AttackController>();
            Character playerCharacter = target.GetComponent<Character>(); 
        
            if (playerCharacter != null && !playerCharacter.isDead) // Добавляем проверку на isDead
            {
                // Проверяем, защищается ли игрок
                if (playerAttack != null && playerAttack.IsBlockingTowards(transform.position))
                {
                    // Эффект блокировки (оставляем без изменений)
                    Debug.Log("Атака заблокирована!");
                    Vector2 pushDirection = (transform.position - target.position).normalized;
                    pushDirection.y = 0.3f;
            
                    if (rb != null && !rb.isKinematic)
                    {
                        rb.velocity = Vector2.zero;
                        rb.AddForce(pushDirection * 15f, ForceMode2D.Impulse);
                    }
            
                    StartCoroutine(BlockStun(0.3f));
                }
                else
                {
                    playerCharacter.TakeDamage(attackDamage); // Вызываем TakeDamage из Character
                }
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator BlockStun(float duration)
    {
        isInHitStun = true;
        yield return new WaitForSeconds(duration);
        isInHitStun = false;
    }
    
    void DebugDrawRays()
    {
        // Проверка земли впереди
        Vector2 frontCheck = (Vector2)transform.position + frontGroundCheck;
        Vector2 backCheck = (Vector2)transform.position + backGroundCheck;
        
        Debug.DrawRay(frontCheck, Vector2.down * groundCheckDistance, Color.green);
        Debug.DrawRay(backCheck, Vector2.down * groundCheckDistance, Color.cyan);
        
        // Проверка стены
        Debug.DrawRay(
            transform.position,
            (facingRight ? Vector2.right : Vector2.left) * wallCheckDistance,
            Color.red
        );
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInHitStun) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
            return; // Важно: выходим сразу после смерти
        }
        animator.SetTrigger("Hit");
        StartCoroutine(HitStun());
    }

    IEnumerator HitStun()
    {
        isInHitStun = true;
        rb.velocity = Vector2.zero; // Останавливаем движение при получении урона
        yield return new WaitForSeconds(hitStunDuration);
        isInHitStun = false;
    }

    void Die()
    {
        if (isDead) return; // Защита от повторного вызова
    
        isDead = true;
    
        // Останавливаем все корутины
        StopAllCoroutines();
    
        // Сбрасываем все триггеры аниматора
        animator.ResetTrigger("Hit");
        animator.ResetTrigger("Charge");
        animator.ResetTrigger("Attack");
    
        // Проигрываем анимацию смерти
        animator.SetTrigger("Death");
    
        // Отключаем физику и коллайдеры
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    
        Destroy(gameObject, 2f);
    }
}