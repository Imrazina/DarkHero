using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float speed = 3f;
    public float runSpeed = 5f;
    public float dashSpeed = 12f;  // Сделал чуть быстрее для резкости
    public float dashTime = 0.15f;  // Чуть короче, чтобы было резче
    private bool isDashing = false;
    public float jumpForce = 8f;
    private Rigidbody2D rb;
    private bool isGrounded;

    private Transform spriteTransform;
    private Animator animator;
    private AttackController attackController; 

    void Start()
    {
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
        
        if (isDashing) return; // Блокируем управление во время дэша

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
        }
    }
}
