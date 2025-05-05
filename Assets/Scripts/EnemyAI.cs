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
    private bool isAgro => Vector2.Distance(transform.position, target.position) <= detectionRange;


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

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        Vector2 directionToPlayer = (target.position - transform.position).normalized;

        bool shouldMove = distanceToPlayer <= detectionRange &&
                          distanceToPlayer > minFollowDistance;

        bool shouldFaceRight = directionToPlayer.x > 0;
        if (shouldFaceRight != facingRight && Time.time > lastFlipTime + flipCooldown)
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

        animator.SetTrigger("Charge");
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        float chargeDuration = 0.3f;
        yield return new WaitForSeconds(chargeDuration);

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.1f);
        DealDamage();

        float cooldownLeft = attackCooldown - chargeDuration;
        yield return new WaitForSeconds(cooldownLeft);

        canAttack = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isAttacking = false;
    }

    public void DealDamage()
    {
        if (!hasDealtDamage && target != null &&
            Vector2.Distance(transform.position, target.position) <= attackRange)
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
                    rb.AddForce(pushDir * 25f, ForceMode2D.Impulse);
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
        if (isDead || isHitAnimating) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            GameStateManager.Instance.CurrentState.pandaState = PandaDialogueState.AfterEnemyDefeated;
            GameStateManager.Instance.SaveGame();
            Die();
        }

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName("hitEnemy")) return;

        StartCoroutine(PlayHitAnimation());
    }

    IEnumerator PlayHitAnimation()
    {
        isHitAnimating = true;
        animator.SetTrigger("Hit");

        Vector2 pushDir = (transform.position - target.position).normalized;
        pushDir.y = 0.2f;
        rb.AddForce(pushDir * 40f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.3f);

        isHitAnimating = false;
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