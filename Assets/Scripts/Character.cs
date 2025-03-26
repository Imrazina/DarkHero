using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float speed = 3f;
    public float runSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;
    
    private Transform spriteTransform; // Ссылка на объект со спрайтом
    private Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ищем дочерний объект со спрайтом и аниматором
        spriteTransform = transform.Find("Sprite"); 
        if (spriteTransform != null)
        {
            animator = spriteTransform.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("Не найден дочерний объект Sprite! Убедись, что в Samurai есть объект Sprite с аниматором.");
        }
    }
    
    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0)
        {
            float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : speed;
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

            // Поворачиваем спрайт в нужную сторону
            spriteTransform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);

            // Устанавливаем анимацию ходьбы или бега
            animator.SetInteger("State", Input.GetKey(KeyCode.LeftShift) ? 2 : 1);
        }
        else
        {
            // Анимация бездействия
            animator.SetInteger("State", 0);
        }

        // Прыжок
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetInteger("State", 3);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
