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
        if (isDashing) return; 

        float moveInput = Input.GetAxisRaw("Horizontal");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float moveSpeed = isRunning ? runSpeed : speed;

        if (moveInput != 0)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
            spriteTransform.localScale = new Vector3(Mathf.Abs(spriteTransform.localScale.x) * Mathf.Sign(moveInput), spriteTransform.localScale.y, spriteTransform.localScale.z);

            if (!attackController.isAttacking) 
            {
                animator.SetInteger("State", isRunning ? 2 : 1);
            }
        }
        else if (isGrounded && !attackController.isAttacking)
        {
            animator.SetInteger("State", 0);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetInteger("State", 3);
            Invoke("DisableGrounded", 0.1f);
        }

        if (Input.GetKeyDown(KeyCode.O) && !isDashing)
        {
            StartDashFromAnimation();
        }

        if (currentPlatform != null && isGrounded && !isDashing)
        {
         //   rb.velocity = new Vector2(rb.velocity.x + currentPlatform.PlatformVelocity.x, rb.velocity.y);
        }
    }

    void DisableGrounded()
    {
        isGrounded = false;
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

        // Плавное исчезновение
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
