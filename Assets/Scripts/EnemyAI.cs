using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Health")] public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("Movement")] public float speed = 2f;
    public float detectionRange = 2f;
    public float minFollowDistance = 1f;
    private bool facingRight = true;
    private float lastFlipTime;
    public float flipCooldown = 1f;

    [Header("Combat")] public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    private bool canAttack = true;

    [Header("Ground Check")] public Vector2 frontGroundCheck = new Vector2(0.5f, -0.5f);
    public Vector2 backGroundCheck = new Vector2(-0.5f, -0.5f);
    public float groundCheckDistance = 0.28f;
    public LayerMask groundLayer;
    
    [Header("Wall Check")] public float wallCheckDistance = 0.6f;

    [Header("References")] private Animator animator;
    private Transform target;
    private Rigidbody2D rb;
    private bool isHitAnimating = false;
    private bool hasDealtDamage = false;
    private bool isAttacking = false;
    
    [Header("Combat")]
    public float invulnerabilityDuration = 0.5f;
    
    private bool isInvulnerable = false;
    private float originalSpeed;
    private Vector2 movementDirection;
    
    [Header("Sound Effects")]
    public AudioClip attackSound;
    public AudioClip deathSound;
    private AudioSource audioSource;
    
    [Header("Combat Behavior Settings")]
    public bool useChargeBeforeAttack = true;
    public bool useInvulnerabilityDuringAttack = true;
    
    [Header("Combat Timing")]
    public float postAttackVulnerabilityDuration = 0.6f;
    private bool canReactToDamage = true;
    
    public int GetCurrentHealth() => currentHealth;
    public bool IsDead() => isDead;
    public string uniqueID; 
    private bool isAgro => Vector2.Distance(transform.position, target.position) <= detectionRange;

    public bool isStaticEnemy = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (!string.IsNullOrEmpty(uniqueID))
        {
            if (GameStateManager.Instance.CurrentState.collectedItems.Contains(uniqueID))
            {
                isDead = true;
                gameObject.SetActive(false);
                return;
            }
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isDead || target == null || isHitAnimating || isAttacking)
        {
            animator.SetBool("Run", false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        Vector2 directionToPlayer = (target.position - transform.position).normalized;

        bool shouldMove = distanceToPlayer <= detectionRange &&
                          distanceToPlayer > minFollowDistance;

        bool shouldFaceRight = directionToPlayer.x > 0;
        float currentCooldown = isAgro ? 0f : flipCooldown;

        if (shouldFaceRight != facingRight && Time.time > lastFlipTime + currentCooldown)
        {
            Flip();
            lastFlipTime = Time.time;
        }

        if (distanceToPlayer <= attackRange && canAttack && !isAttacking)
        {
            animator.SetBool("Run", false);
            rb.velocity = new Vector2(0, rb.velocity.y);
            StartCoroutine(Attack());
        }
        else if (isAgro && CanMoveTowards(directionToPlayer) && distanceToPlayer > minFollowDistance)
        {
            MoveTowardsPlayer(directionToPlayer);
        }
        else if (!isAgro)
        {
            Patrol();
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
        Vector2 checkPos = (Vector2)transform.position +
                           (facingRight ? frontGroundCheck : backGroundCheck);

        bool hasGround = Physics2D.OverlapCircle(checkPos, groundCheckDistance, groundLayer);
        bool hasWall = Physics2D.Raycast(
            (Vector2)transform.position,
            facingRight ? Vector2.right : Vector2.left,
            wallCheckDistance,
            groundLayer
        );

        if (!hasGround || hasWall)
        {
            animator.SetBool("Run", false);
            rb.velocity = Vector2.zero;
            return false;
        }

        return hasGround && !hasWall;
    }

    void Patrol()
    {
        Vector2 groundCheckPos = (Vector2)transform.position + (facingRight ? frontGroundCheck : backGroundCheck);
        bool hasGround = Physics2D.Raycast(groundCheckPos, Vector2.down, groundCheckDistance, groundLayer);
        bool hasWall = Physics2D.Raycast(transform.position, facingRight ? Vector2.right : Vector2.left,
            wallCheckDistance, groundLayer);

        if (!hasGround || hasWall)
        {
            Flip();
        }

        if (hasWall)
        {
            animator.SetBool("Run", false);
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            animator.SetBool("Run", true);
            float direction = facingRight ? 1f : -1f;
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }
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
        
        if (useChargeBeforeAttack)
        {
            canReactToDamage = false;
            animator.SetTrigger("Charge");
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            yield return new WaitForSeconds(0.3f);
        }

        if (useInvulnerabilityDuringAttack)
            isInvulnerable = true;

        animator.SetTrigger("Attack");
        if (attackSound != null) audioSource.PlayOneShot(attackSound);
        yield return new WaitForSeconds(0.5f); 
    
        DealDamage();
        yield return new WaitForSeconds(0.2f); 
        
        isInvulnerable = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        canReactToDamage = true;
    
        float cooldownLeft = attackCooldown - (useChargeBeforeAttack ? 0.9f : 0f);
        yield return new WaitForSeconds(cooldownLeft);
        
        canReactToDamage = false;
        isAttacking = false;
        canAttack = true;
    }
    
    IEnumerator InvulnerabilityFrame()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    public void DealDamage()
    {
        if (!hasDealtDamage && target != null &&
            Vector2.Distance(transform.position, target.position) <= attackRange)
        {
            var playerAttack = target.GetComponent<AttackController>();
            var playerCharacter = target.GetComponentInParent<Character>();

            if (playerCharacter != null && !playerCharacter.isDead)
            {
                if (playerAttack != null && playerAttack.IsBlockingTowards(transform.position))
                {
                    Debug.Log("Атака заблокирована!");
                }
                else
                {
                    playerCharacter.TakeDamage(attackDamage);
                }

                hasDealtDamage = true;
            }
        }
    }

    void DebugDrawRays()
    {
        Vector2 frontCheck = (Vector2)transform.position + frontGroundCheck;
        Vector2 backCheck = (Vector2)transform.position + backGroundCheck;

        Debug.DrawRay(frontCheck, Vector2.down * groundCheckDistance, Color.green);
        Debug.DrawRay(backCheck, Vector2.down * groundCheckDistance, Color.cyan);

        Debug.DrawRay(
            transform.position,
            (facingRight ? Vector2.right : Vector2.left) * wallCheckDistance,
            Color.red
        );
    }

    public void TakeDamage(int damage)
    {
        if (isDead || !canReactToDamage) 
        {
            Debug.Log($"[EnemyAI] REJECT HIT: Dead:{isDead} React:{canReactToDamage} Invul:{isInvulnerable}");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"[EnemyAI] TOOK DAMAGE: {damage}, HP: {currentHealth}");

        if (currentHealth <= 0) Die();
        else StartCoroutine(PlayHitAnimation());
    }
    
    IEnumerator PlayHitAnimation()
    {
        if (canReactToDamage) 
        {
            isHitAnimating = true;
            animator.SetTrigger("Hit");
        
            yield return new WaitForSeconds(0.15f);
            isHitAnimating = false;
        }
    }


    public void Die()
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
        
        StartCoroutine(PlayDeathSoundsWithDelay());

        if (!GameStateManager.Instance.CurrentState.collectedItems.Contains(uniqueID))
        {
            GameStateManager.Instance.CurrentState.collectedItems.Add(uniqueID);
        }
        
        GameStateManager.Instance.CurrentState.pandaState = PandaDialogueState.AfterEnemyDefeated;
        GameStateManager.Instance.SaveGame();

        StartCoroutine(DeactivateAfterDelay(1.5f));
    }
    
    private IEnumerator PlayDeathSoundsWithDelay()
    {
        yield return new WaitForSeconds(1f); 
        if (deathSound != null){
            audioSource.PlayOneShot(deathSound);}
    }
    
    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
    
    public void ResetEnemy()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool("Run", true);
        }
        else
        {
            Debug.LogWarning($"{name} has no valid Animator or AnimatorController!");
        }

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = true;
        }

        isDead = false;
        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
        }
        else
        {
            Debug.LogWarning($"{name} has no Rigidbody2D!");
        }
    }
}