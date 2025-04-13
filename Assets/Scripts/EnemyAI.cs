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
    private bool isHitAnimating = false;
    private bool hasDealtDamage = false;
    private bool isAttacking = false;

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
        if (isDead || target == null)
        {
            animator.SetBool("Run", false);
            return;
        }

        if (isInHitStun)
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
        if (distanceToPlayer <= attackRange && canAttack && !isAttacking)
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
        isAttacking = true;
        hasDealtDamage = false;

        // 1. Проигрываем Charge-анимацию
        animator.SetTrigger("Charge");

        // 2. Ждем длину Charge (например, 0.5 сек)
        float chargeDuration = 0.5f;
        yield return new WaitForSeconds(chargeDuration);

        // 3. Проигрываем атаку
        animator.SetTrigger("Attack");

        // 4. Ждём чуть-чуть, чтобы попасть в нужный момент удара (например, 0.2 сек после старта Attack-анимации)
        yield return new WaitForSeconds(0.2f);

        if (!hasDealtDamage && target != null && Vector2.Distance(transform.position, target.position) <= attackRange)
        {
            var playerAttack = target.GetComponent<AttackController>();
            var playerCharacter = target.GetComponent<Character>();

            if (playerCharacter != null && !playerCharacter.isDead)
            {
                if (playerAttack != null && playerAttack.IsBlockingTowards(transform.position))
                {
                    Debug.Log("Атака заблокирована!");
                    Vector2 pushDir = (transform.position - target.position).normalized;
                    pushDir.y = 0.3f;

                    rb.velocity = Vector2.zero;
                    rb.AddForce(pushDir * 15f, ForceMode2D.Impulse);
                    StartCoroutine(BlockStun(0.3f));
                }
                else
                {
                    playerCharacter.TakeDamage(attackDamage);
                }

                hasDealtDamage = true;
            }
        }

        // 5. Ждём остаток кулдауна
        float cooldownLeft = attackCooldown - chargeDuration;
        yield return new WaitForSeconds(cooldownLeft);

        canAttack = true;
        isAttacking = false;
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
        if (isDead || isInHitStun || isHitAnimating) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            GameStateManager.Instance.CurrentState.pandaState = PandaDialogueState.AfterEnemyDefeated;
            GameStateManager.Instance.SaveGame();
            Die();
        }

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName("hitEnemy")) return; // уже в анимации удара

        StartCoroutine(PlayHitAnimation());
        StartCoroutine(HitStun());
    }

    IEnumerator PlayHitAnimation()
    {
        isHitAnimating = true;
        animator.SetTrigger("Hit");

        // Ожидаем пока анимация закончится
        yield return new WaitForSeconds(0.4f); 

        isHitAnimating = false;
    }

    IEnumerator HitStun()
    {
        isInHitStun = true;

        // Прерываем движение и атаку
        rb.velocity = Vector2.zero;
        isAttacking = false; // <== предотвращает баг с застреванием в состоянии атаки

        // Отталкиваем от игрока
        if (target != null)
        {
            Vector2 pushDir = (transform.position - target.position).normalized;
            pushDir.y = 0.3f;
            rb.AddForce(pushDir * 10f, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(hitStunDuration);

        isInHitStun = false;
    }

    void Die()
    {
        if (isDead) return; 
    
        isDead = true;

        StopAllCoroutines();

        animator.ResetTrigger("Hit");
        animator.ResetTrigger("Charge");
        animator.ResetTrigger("Attack");

        animator.SetTrigger("Death");

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