using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float speed = 3f;
    public float runSpeed = 5f;
    public float dashSpeed = 12f;
    public float dashTime = 0.15f;
    private bool isDashing = false;

    public float jumpForce = 8f;
    private Rigidbody2D rb;
    private bool isGrounded;

    private Transform spriteTransform;
    private Animator animator;
    private AttackController attackController;

    public int maxHealth = 100;
    private int currentHealth;
    public bool isDead = false;

    [Header("Wall Jump")]
    public float wallSlideSpeed = -1.5f;
    public float wallJumpHorizontalForce = 20f;
    public float wallJumpVerticalForce = 17f;
    public float wallCoyoteTime = 0.2f;

    private float wallCoyoteTimer = 0f;
    private bool isWallJumping = false;

    public float wallStickTime = 0.5f;
    private bool isTouchingWall = false;
    private bool isWallSliding = false;
    private float wallStickTimer = 0f;
    private Vector2 wallNormal;

    public float wallJumpControlDelay = 0.2f;
    private int currentAnimState = -1;
    private bool justWallJumped = false;

    private MovingPlatform currentPlatform;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        attackController = GetComponent<AttackController>();

        spriteTransform = transform.Find("Sprite");
        if (spriteTransform != null)
        {
            animator = spriteTransform.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("Не найден дочерний объект Sprite!");
        }
    }

void Update()
{
    if (isDead) return;

    float moveInput = Input.GetAxisRaw("Horizontal");
    bool isRunning = Input.GetKey(KeyCode.LeftShift);
    float moveSpeed = isRunning ? runSpeed : speed;

    if (Input.GetKeyDown(KeyCode.O) && !isDashing)
    {
        StartDashFromAnimation(); // твой метод
    }
    
    if (isDashing) return;

    // -------------------- Wall Slide --------------------
    if (isTouchingWall && !isGrounded && moveInput != 0 && Mathf.Sign(moveInput) == -Mathf.Sign(wallNormal.x) && !isWallJumping && !justWallJumped)
    {
        if (!isWallSliding)
        {
            isWallSliding = true;
            SetAnimState(4);
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, wallSlideSpeed)); // замедление падения
        wallCoyoteTimer = wallCoyoteTime;
    }
    else
    {
        if (isWallSliding)
            isWallSliding = false;

        if (wallCoyoteTimer > 0)
            wallCoyoteTimer -= Time.deltaTime;
    }

    // -------------------- Движение --------------------
    if (!isWallSliding && !isWallJumping)
    {
        float adjustedSpeed = moveInput * moveSpeed;

        if (currentPlatform != null && isGrounded)
        {
            adjustedSpeed += currentPlatform.PlatformVelocity.x;
        }

        rb.velocity = new Vector2(adjustedSpeed, rb.velocity.y);
    }
    else if (isWallJumping && wallJumpControlDelay > 0)
    {
        rb.velocity = new Vector2(
            Mathf.Lerp(rb.velocity.x, moveInput * moveSpeed * 1.5f, Time.deltaTime * 10),
            rb.velocity.y
        );
    }

    // -------------------- Отражение спрайта --------------------
    if (isTouchingWall)
    {
        if (wallNormal.x > 0)
        {
            spriteTransform.localScale = new Vector3(Mathf.Abs(spriteTransform.localScale.x), spriteTransform.localScale.y, spriteTransform.localScale.z);
        }
        else if (wallNormal.x < 0)
        {
            spriteTransform.localScale = new Vector3(-Mathf.Abs(spriteTransform.localScale.x), spriteTransform.localScale.y, spriteTransform.localScale.z);
        }
    }
    else if (moveInput != 0)
    {
        spriteTransform.localScale = new Vector3(
            Mathf.Abs(spriteTransform.localScale.x) * Mathf.Sign(moveInput),
            spriteTransform.localScale.y,
            spriteTransform.localScale.z
        );
    }

    // -------------------- Анимации --------------------
    if (!attackController.isAttacking)
    {
        // Проверяем на wall sliding только если персонаж не был в прыжке от стены
        if (isWallSliding && !isWallJumping && !justWallJumped)
        {
            SetAnimState(4); // WallSlide
        }
        else if (isGrounded)
        {
            if (moveInput == 0)
                SetAnimState(0); // Idle
            else
                SetAnimState(isRunning ? 2 : 1); // Run
        }
        else if (!isGrounded && !isWallSliding && !isWallJumping)
        {
            SetAnimState(3); // Jump
        }
    }

    // -------------------- Прыжки --------------------
    if (Input.GetKeyDown(KeyCode.Space))
    {
        Debug.Log($"[Jump Pressed] Grounded: {isGrounded}, Wall: {isTouchingWall}, WallNormal: {wallNormal}");

        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            SetAnimState(3);
            Debug.Log("[Jump] Прыжок с земли");
            Invoke("DisableGrounded", 0.1f);
        }
        else if ((isTouchingWall || wallCoyoteTimer > 0f) && Mathf.Abs(wallNormal.x) > 0.1f)
        {
            Debug.Log("[WallJump Attempt] Условия выполнены. Начинаем прыжок от стены.");
            WallJump();
        }
        else
        {
            Debug.Log("[WallJump Attempt] Условия НЕ выполнены.");
        }
    }

    if (wallStickTimer > 0)
    {
        wallStickTimer -= Time.deltaTime;
    }
    else
    {
        isTouchingWall = false;
    }
}

private void WallJump()
{
    if ((isTouchingWall || wallCoyoteTimer > 0f) && !isGrounded)
    {
        isWallJumping = true;
        justWallJumped = true;
        isWallSliding = false;
        isTouchingWall = false;
        animator.SetInteger("State", 5);

        float jumpDirection = Mathf.Sign(wallNormal.x) < 0 ? 1 : -1;
        rb.velocity = Vector2.zero;
        rb.velocity = new Vector2(jumpDirection * wallJumpHorizontalForce, wallJumpVerticalForce);

        Debug.Log($"[WallJump] Направление прыжка: {jumpDirection}, Применённая скорость: {rb.velocity}");

        StartCoroutine(ResetWallJump());
        StartCoroutine(ClearJustWallJumped()); // <-- Вызов нового корутина
    }
}

private IEnumerator ClearJustWallJumped()
{
    yield return new WaitForSeconds(0.3f); // немного времени, пока не врубим wall slide снова
    justWallJumped = false;
}
    private IEnumerator ResetWallJump()
    {
        yield return new WaitForSeconds(wallJumpControlDelay);
        isWallJumping = false;
    }

    void DisableGrounded()
    {
        isGrounded = false;
    }

    private void SetAnimState(int state)
    {
        if (currentAnimState == state) return;
        currentAnimState = state;
        animator.SetInteger("State", state);
        Debug.Log($"[Anim] Set State: {state}");
    }
    public void StartDashFromAnimation()
    {
        if (!isDashing)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
            yield break;

        animator.SetTrigger("Dash");
        isDashing = true;
        float direction = Mathf.Sign(spriteTransform.localScale.x);
        rb.velocity = new Vector2(direction * dashSpeed, rb.velocity.y);

        yield return new WaitForSeconds(dashTime);
        EndDash();
    }

    public void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero;
        animator.ResetTrigger("Dash");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            
            var platform = collision.gameObject.GetComponent<MovingPlatform>();
            if (platform != null)
            {
                currentPlatform = platform;
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<MovingPlatform>() != null)
        {
            currentPlatform = null;

            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y); 
        }
    }

    private IEnumerator DelayedPlatformReset()
    {
        yield return new WaitForSeconds(0.1f);
        currentPlatform = null;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isGrounded && collision.gameObject.CompareTag("Wall"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    wallNormal = contact.normal;
                    isTouchingWall = true;
                    wallStickTimer = wallStickTime;
                    wallCoyoteTimer = wallCoyoteTime;
                    break;
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isDashing) return;

        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        rb.simulated = false;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        GetComponent<AttackController>().enabled = false;
        this.enabled = false;

        animator.ResetTrigger("Hurt");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Dash");

        animator.Play("Death", 0, 0f);
        animator.SetTrigger("Death");

        StartCoroutine(FadeOutAndDie(1f));
    }

    private IEnumerator FadeOutAndDie(float fadeTime)
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
            sprite.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
