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

    public int maxHealth = 500;
    private int currentHealth;
    public bool isDead = false;

    [Header("Wall Jump")]
    public float wallSlideSpeed = -1.5f;
    public float wallJumpHorizontalForce = 20f;
    public float wallJumpVerticalForce = 17f;
    public float wallCoyoteTime = 0.2f;
    
    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f; 
    private bool isKnockedBack = false; 
    
    [Header("Healing")]
    public int potionHealAmount = 200; 
    public float healingAnimationTime = 1f; 
    private bool isHealing = false;

    private float wallCoyoteTimer = 0f;
    private bool isWallJumping = false;

    public float wallStickTime = 2f;
    private bool isTouchingWall = false;
    private bool isWallSliding = false;
    private float wallStickTimer = 0f;
    private Vector2 wallNormal;

    public float wallJumpControlDelay = 0.2f;
    private int currentAnimState = -1;
    private bool justWallJumped = false;

    private MovingPlatform currentPlatform;
    public UI_HealthDisplay healthUI;
    
    [Header("Power Ups")]
    public int damageBoostAmount = 30;
    public float damageBoostDuration = 10f;
    private bool isDamageBoosted = false;

    public float invincibilityDuration = 10f;
    private bool isInvincible = false;
    
    [Header("Visual Effects")]
    public Color damageBoostColor = new Color(1f, 0.5f, 0.5f); 
    public Color invincibilityColor = new Color(0.5f, 0.5f, 1f); 
    private Color defaultColor;
    private SpriteRenderer characterRenderer;
    
    private bool isMovementLocked = false;
    
    [Header("Improved Ground Check")]
    public float groundCheckDistance = 0.5f;
    public float groundWidthCheck = 0.4f;
    public float groundAngleThreshold = 30f;
    public float groundStickForce = 5f;
    public float gravityScale = 2f;
    public float fallGravityMultiplier = 1.5f;

    private bool wasGrounded;
    private float originalGravityScale;
    
    [Header("Ground Check")]
    public float groundCheckRadius = 0.2f;
    public Vector2 groundCheckOffset = new Vector2(0, -0.5f);
    public float wallCheckDistance = 0.5f;
    public LayerMask whatIsGround;
    
    [Header("Sound Effects")]
    public AudioClip deathSound;
    public AudioClip jumpSound;
    private AudioSource audioSource;
    [Range(0, 1)] public float soundVolume = 1f;

    private void UpdateGroundAndWallStatus()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;
    
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + groundCheckOffset, 
            groundCheckRadius, 
            whatIsGround
        );
    
        foreach (Collider2D col in groundColliders)
        {
            if (col.gameObject == gameObject) continue;
        
            Vector2 closestPoint = col.ClosestPoint(transform.position);
            Vector2 groundNormal = (Vector2)transform.position - closestPoint;
            float angle = Vector2.Angle(groundNormal, Vector2.up);
        
            if (angle < groundAngleThreshold)
            {
                isGrounded = true;
                break;
            }
        }
        
        isTouchingWall = false;
        if (!isGrounded)
        {
            float direction = spriteTransform.localScale.x > 0 ? 1 : -1;
            RaycastHit2D wallHit = Physics2D.Raycast(
                transform.position,
                new Vector2(direction, 0),
                wallCheckDistance,
                whatIsGround
            );

            if (wallHit.collider != null)
            {
                float wallAngle = Vector2.Angle(wallHit.normal, Vector2.right);
                if (wallAngle > 60f && wallHit.point.y < transform.position.y - 0.3f)
                {
                    isTouchingWall = true;
                    wallNormal = wallHit.normal;
                }
            }
        }
    }

    
    void Start()
    {
        characterRenderer = GetComponentInChildren<SpriteRenderer>();
        if (characterRenderer != null)
        {
            defaultColor = characterRenderer.color;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (!GameStateManager.Instance.CurrentState.hasPlayedIntro || 
            GameStateManager.Instance.CurrentState.isPlayerDead)
        {
            transform.position = new Vector3(-6, 0, 0);
        }
        else if (GameStateManager.Instance.CurrentState.playerPosition != Vector3.zero)
        {
            transform.position = GameStateManager.Instance.CurrentState.playerPosition;
        }
        
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
        
        if (healthUI == null)
        {
            healthUI = FindObjectOfType<UI_HealthDisplay>();
        }
        
        originalGravityScale = rb.gravityScale;
        
        healthUI.UpdateHearts(currentHealth);
    }

    void Update()
    {
        if (!isDead)
        {
        
            GameStateManager.Instance.CurrentState.playerPosition = transform.position;
        }
    
        if (isDead || isKnockedBack || isHealing || isMovementLocked) return;
        
        CheckSurfaces();

        if (rb.velocity.y < 0 && !isGrounded)
        {
            rb.gravityScale = originalGravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = originalGravityScale;
        }

        float moveInput = Input.GetAxisRaw("Horizontal");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float moveSpeed = isRunning ? runSpeed : speed;

        if (Input.GetKeyDown(KeyCode.O) && !isDashing)
        {
            StartDashFromAnimation(); 
        }
    
        if (isDashing) return;

        // -------------------- Wall Slide --------------------
        bool canWallSlide =
            isTouchingWall &&
            !isGrounded &&
            moveInput != 0 &&
            Mathf.Sign(moveInput) == -Mathf.Sign(wallNormal.x) &&
            !isWallJumping &&
            !justWallJumped &&
            rb.velocity.y <= 0;

        if (canWallSlide)
        {
            if (!isWallSliding)
            {
                isWallSliding = true;
                SetAnimState(4);
            }

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, wallSlideSpeed));
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
        if (!attackController.isAttacking && !isDashing)
        {
            if (isWallSliding)
            {
                SetAnimState(4);
            }
            else if (isWallJumping || justWallJumped)
            {
                SetAnimState(3);
            }
            else if (!isGrounded && Mathf.Abs(rb.velocity.y) > 0.1f)
            {
                SetAnimState(3);
            }
            else if (isGrounded && Mathf.Abs(moveInput) > 0.01f)
            {
                SetAnimState(isRunning ? 2 : 1);
            }
            else if (isGrounded)
            {
                SetAnimState(0);
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
                PlaySound(jumpSound);
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
    
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isHealing)
        {
            TryUsePotion();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TryUseDamageBoost();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TryUseInvincibility();
        }

        if (wallStickTimer > 0)
        {
            wallStickTimer -= Time.deltaTime;
        }
        else
        {
            if (!justWallJumped) 
                isTouchingWall = false;
        }
        
        CheckGrounded();
    }

    private void TryUsePotion()
    {
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null && inventory.UsePotion())
        {
            StartCoroutine(HealingProcess());
        }
        else if (inventory != null && inventory.potionCount <= 0)
        {
            Debug.Log("Нет зелий для использования!");
        }
    }

    private IEnumerator HealingProcess()
    {
        isHealing = true;
    
        animator.SetTrigger("Healing");
    
        yield return new WaitForSeconds(0.2f);
    
        currentHealth = Mathf.Min(currentHealth + potionHealAmount, maxHealth);
    
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }

        yield return new WaitForSeconds(healingAnimationTime - 0.2f);
    
        isHealing = false;

        Debug.Log("Использовано зелье! Восстановлено " + potionHealAmount + " HP");
    }

    private void TryUseDamageBoost()
    {
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null && inventory.UseDamageBoost())
        {
            ActivateDamageBoost(); 
        }
        else
        {
            Debug.Log("Нет бустов урона!");
        }
    }

    private void ActivateDamageBoost()
    {
        if (isDamageBoosted) return;
        animator.SetTrigger("Healing");
        if (characterRenderer != null)
        {
            characterRenderer.color = damageBoostColor;
        }
        
        StartCoroutine(FlashEffect(damageBoostDuration, damageBoostColor));
        
        isDamageBoosted = true;
        Debug.Log($"Урон увеличен на {damageBoostAmount} единиц!");

        if (attackController != null && attackController.hitbox != null)
        {
            attackController.hitbox.lightAttackDamage += damageBoostAmount;
            attackController.hitbox.mediumAttackDamage += damageBoostAmount;
            attackController.hitbox.heavyAttackDamage += damageBoostAmount;
        }
        
        Invoke(nameof(ResetDamageBoost), damageBoostDuration);
    }

    private void ResetDamageBoost()
    {
        if (!isDamageBoosted) return;

        if (attackController != null && attackController.hitbox != null)
        {
            attackController.hitbox.lightAttackDamage -= damageBoostAmount;
            attackController.hitbox.mediumAttackDamage -= damageBoostAmount;
            attackController.hitbox.heavyAttackDamage -= damageBoostAmount;
        }

        isDamageBoosted = false;
        Debug.Log("Урон вернулся к исходному значению!");
    }
    
    public void TryUseInvincibility()
    {
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null && inventory.UseInvincibility())
        {
            ActivateInvincibility(); 
        }
        else
        {
            Debug.Log("Нет предметов неуязвимости!");
        }
    }
    public void ActivateInvincibility()
    {
        if (isInvincible) return;
        animator.SetTrigger("Healing");
        
        if (characterRenderer != null)
        {
            characterRenderer.color = invincibilityColor;
            StartCoroutine(FlashEffect(invincibilityDuration, invincibilityColor));
        }
    
        isInvincible = true;
        StartCoroutine(ResetInvincibility());
    }

    private IEnumerator ResetInvincibility()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        Debug.Log("Неуязвимость закончилась!");
    }

    private IEnumerator FlashEffect(float duration, Color effectColor)
    {
        float elapsed = 0f;
        float flashSpeed = 0.5f; 
    
        while (elapsed < duration)
        {
            if (characterRenderer != null)
            {
                characterRenderer.color = 
                    Mathf.PingPong(elapsed * 10f, 1f) > 0.5f 
                        ? effectColor 
                        : defaultColor;
            }
        
            elapsed += flashSpeed;
            yield return new WaitForSeconds(flashSpeed);
        }
        
        if (characterRenderer != null)
        {
            characterRenderer.color = defaultColor;
        }
    }
    
    public void SetMovementLock(bool locked)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (locked) 
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                SetAnimState(0); 
            }
            else 
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    
        isMovementLocked = locked;
    
        if (attackController != null)
        {
            attackController.enabled = !locked;
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
            StartCoroutine(ClearJustWallJumped()); 
        }
    }

    private IEnumerator ClearJustWallJumped()
    {
        yield return new WaitForSeconds(0.25f); 
        justWallJumped = false;
        isWallJumping = false;
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


    public void SetAnimState(int state)
    {
        if (state == currentAnimState) return;
        currentAnimState = state;
        Debug.Log("[Anim] Set State: " + state);
        animator.SetInteger("State", state);
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

    public void TakeDamage(int damage)
    {
        if (isDead || isDashing || isKnockedBack || isHealing || isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        animator.SetTrigger("Hurt");
    
        if (healthUI != null)
        {
            healthUI.UpdateHearts(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    

    public void Die()
    {
        if (isDead) return;
        PlaySound(deathSound);
        isDead = true;

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        rb.simulated = false;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        GetComponent<AttackController>().enabled = false;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.GameOverWithDelay(0.5f);
        }
        else
        {
            Debug.LogError("UIManager.Instance не найден!");
        }
    }
    
    public void Resurrect()
    {
        isDead = false;
        currentHealth = maxHealth;
        rb.isKinematic = false;
        rb.simulated = true;
    
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = true;
        }
    
        GetComponent<AttackController>().enabled = true;
        healthUI.UpdateHearts(currentHealth);
    }
    
    private void CheckGrounded()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + groundCheckOffset, 
            groundCheckRadius, 
            whatIsGround
        );

        isGrounded = false;
        
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == gameObject) continue;
            
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                groundCheckOffset.magnitude + groundCheckRadius,
                whatIsGround
            );
            
            if (hit.collider != null)
            {
                float surfaceAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (surfaceAngle < groundAngleThreshold)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }

    
    private void CheckSurfaces()
    {
        wasGrounded = isGrounded;
        isGrounded = false;
        isTouchingWall = false;
        
        RaycastHit2D centerHit = Physics2D.Raycast(
            transform.position + new Vector3(0, -0.1f, 0),
            Vector2.down,
            groundCheckDistance,
            whatIsGround
        );

        RaycastHit2D leftHit = Physics2D.Raycast(
            transform.position + new Vector3(-groundWidthCheck, -0.1f, 0),
            Vector2.down,
            groundCheckDistance,
            whatIsGround
        );

        RaycastHit2D rightHit = Physics2D.Raycast(
            transform.position + new Vector3(groundWidthCheck, -0.1f, 0),
            Vector2.down,
            groundCheckDistance,
            whatIsGround
        );

        CheckHit(centerHit);
        CheckHit(leftHit);
        CheckHit(rightHit);

        if (isGrounded && rb.velocity.y <= 0)
        {
            rb.AddForce(Vector2.down * groundStickForce);
        }
        if (!isGrounded)
        {
            CheckWalls();
        }
    }

    private void CheckHit(RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle < groundAngleThreshold)
            {
                isGrounded = true;
                if (hit.point.y < transform.position.y - 0.2f)
                {
                    isTouchingWall = false;
                }
            }
        }
    }

    private void CheckWalls()
    {
        float direction = spriteTransform.localScale.x > 0 ? 1 : -1;
        RaycastHit2D wallHit = Physics2D.Raycast(
            transform.position,
            new Vector2(direction, 0),
            0.6f,
            whatIsGround
        );

        if (wallHit.collider != null)
        {
            float angle = Vector2.Angle(wallHit.normal, Vector2.right);
            if (angle > 75f && wallHit.point.y < transform.position.y - 0.3f)
            {
                RaycastHit2D downwardHit = Physics2D.Raycast(
                    transform.position + new Vector3(direction * 0.3f, 0.5f, 0),
                    Vector2.down,
                    1f,
                    whatIsGround
                );
            
                if (downwardHit.collider == null)
                {
                    isTouchingWall = true;
                    wallNormal = wallHit.normal;
                }
                else
                {
                    isTouchingWall = false;
                }
            }
            else
            {
                isTouchingWall = false;
            }
        }
        else
        {
            isTouchingWall = false;
        }
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position + new Vector3(0, -0.1f, 0);
        Gizmos.DrawLine(center, center + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(center + new Vector3(-groundWidthCheck, 0, 0), 
            center + new Vector3(-groundWidthCheck, -groundCheckDistance, 0));
        Gizmos.DrawLine(center + new Vector3(groundWidthCheck, 0, 0), 
            center + new Vector3(groundWidthCheck, -groundCheckDistance, 0));
        
        Gizmos.color = Color.blue;
        float dir = spriteTransform != null ? spriteTransform.localScale.x : 1;
        Gizmos.DrawLine(transform.position, 
            transform.position + new Vector3(dir * 0.6f, 0, 0));
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }
}