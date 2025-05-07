using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour, IDialogueCallback
{
    [Header("Health")]
    public int maxHealth = 500;
    private int currentHealth;
    public bool isDead = false;
    public EnemyHealthSlider healthBar; 

    [Header("Movement")]
    public float speed = 1.5f;
    public float detectionRange = 10f;
    private bool facingRight = true;

    [Header("Combat")]
    public float attackRange = 5f;
    public int attackDamage = 20;
    public float attackCooldown = 3f;
    public float chargeTime = 1f;
    private bool canAttack = true;
    private bool isAttacking = false;
    public Collider2D attackHitbox;

    [Header("Attack Timing")]
    public float hitboxEnableDelay = 0.5f;
    public float hitboxActiveTime = 0.3f;

    [Header("Effects")]
    public ParticleSystem chargeEffect;
    public ParticleSystem attackEffect;

    [Header("Audio")]
    public AudioClip chargeSound;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    [Header("References")]
    private Animator animator;
    private Transform target;
    private Rigidbody2D rb;
    private bool isHitAnimating = false;
    private bool isInAttackFrame = false;
    public TupikController tupik;
    
    [Header("Dialogue")]
    public string dialogueFile = "BossDialogue";
    public string startDialogueID = "boss_intro";
    public bool dialogueCompleted = false;
    public bool riddleAnsweredCorrectly = false;
    public float dialogueTriggerDistance = 5f; 
    private bool dialogueStarted = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (attackHitbox != null)
            attackHitbox.enabled = false;
        
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState.isBossDead)
        {
            Die(); 
            return;
        }
    }
    
    private IEnumerator StartBossEncounter()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        animator.SetBool("Run", false); 
        animator.SetTrigger("Idle"); 

        Character player = FindObjectOfType<Character>();
        if (player != null) 
        {
            player.SetMovementLock(true); 
            player.SetAnimState(0);
        }
    
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogueFile, startDialogueID, this);
        }
        else
        {
            Debug.LogError("DialogueManager not found!");
            if (player != null) player.SetMovementLock(false); 
            StartFight();
        }

        yield return null;
    }
    
    public void OnDialogueEnd()
    {
        Debug.Log($"[BOSS_DEBUG] === OnDialogueEnd ===");
        Debug.Log($"[BOSS_DEBUG] riddleAnsweredCorrectly: {riddleAnsweredCorrectly}");

        Character player = FindObjectOfType<Character>();
        if (player != null) 
        {
            player.SetMovementLock(false);
        }

        dialogueCompleted = true;

        if (!riddleAnsweredCorrectly)
        {
            Debug.Log($"[BOSS_DEBUG] Запуск боя!");
            StartFight();
        }
        else
        {
            Debug.Log($"[BOSS_DEBUG] Босс умирает (правильный ответ)");
            Die();
        }
    }
    
    private void StartFight()
    {
        dialogueCompleted = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        isDead = false;
        canAttack = true;
        isAttacking = false;
    }

    void Update()
    {
        if (isDead || target == null) return; 
    
        if (!dialogueStarted && Vector2.Distance(transform.position, target.position) <= dialogueTriggerDistance)
        {
            dialogueStarted = true;
            StartCoroutine(StartBossEncounter());
            return; 
        }
        
        if (dialogueCompleted) 
        {
            FlipIfNeeded();
            float distance = Vector2.Distance(transform.position, target.position);
        
            if (isAttacking) return;

            if (distance <= attackRange && canAttack)
            {
                StartCoroutine(AttackRoutine());
            }
            else if (distance <= detectionRange && distance > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                StopMovement();
            }
        }
    }

    private void FlipIfNeeded()
    {
        if ((target.position.x > transform.position.x) != facingRight)
        {
            Flip();
        }
    }

    private void MoveTowardsPlayer()
    {
        if (isAttacking) return;
        
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
        animator.SetBool("Run", true);
    }

    private void StopMovement()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("Run", false);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackHitbox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackHitbox.bounds.center, attackHitbox.bounds.size);
        }
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;
        isAttacking = true;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        animator.SetBool("Run", false);
    
        if (chargeEffect != null) chargeEffect.Play();
        if (chargeSound != null) audioSource.PlayOneShot(chargeSound);
        animator.SetTrigger("Attack");

        float attackAnimLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(attackAnimLength * 0.99f);

        EnableHitbox(); 
        if (attackEffect != null) attackEffect.Play();
        if (attackSound != null) audioSource.PlayOneShot(attackSound);
        
        yield return new WaitForSeconds(attackAnimLength * 0.01f);
        isAttacking = false;
        yield return new WaitForSeconds(0.1f); 
        DisableHitbox();
        
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    private void DisableHitbox()
    {
        if (attackHitbox != null)
        {
            isInAttackFrame = false;
            attackHitbox.enabled = false;
        }
    }

    private void EnableHitbox()
    {
        if (attackHitbox != null)
        {
            isInAttackFrame = true;
            Debug.Log("[DEBUG] Включение хитбокса атаки");
            attackHitbox.enabled = true;
            DealDamage();
            StartCoroutine(DisableHitboxAfterDelay());
        }
        else
        {
            Debug.LogError("[ERROR] attackHitbox не назначен!");
        }
    }

    IEnumerator DisableHitboxAfterDelay()
    {
        yield return new WaitForSeconds(hitboxActiveTime);
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
            Debug.Log("[DEBUG] Хитбокс атаки выключен");
        }
    }
    
    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackHitbox.bounds.center, attackHitbox.bounds.size, 0f);

        Debug.Log($"[DEBUG] Хитбокс активен. Обнаружено {hits.Length} объектов.");

        foreach (var hit in hits)
        {
            Debug.Log($"[DEBUG] Найден объект: {hit.name}, тег: {hit.tag}");

            if (hit.CompareTag("Player"))
            {
                Character player = hit.GetComponentInParent<Character>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                    Debug.Log($"[УРОН] Босс ударил игрока! Урон: {attackDamage}");
                }
                else
                {
                    Debug.LogWarning("[WARNING] Компонент Character не найден на объекте Player!");
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[DEBUG] OnTriggerEnter2D вызван: {other.name}");

        if (attackHitbox != null && attackHitbox.enabled && other.CompareTag("Player"))
        {
            Character player = other.GetComponentInParent<Character>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
                Debug.Log("[УРОН] Босс ударил игрока через Trigger!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isHitAnimating || isInAttackFrame) return;

        currentHealth -= damage;
        UpdateHealthUI();

        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);

        if (currentHealth <= 0)
        {
            if (healthBar != null)
            {
                healthBar.gameObject.SetActive(false);
            }
            Die();
        }
        else
        {
            StartCoroutine(PlayHitAnimation());
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            healthBar.UpdateHealthSlider(healthPercent);
        }
    }

    IEnumerator PlayHitAnimation()
    {
        isHitAnimating = true;
        bool wasAttacking = isAttacking;
        if (!wasAttacking)
        {
            animator.SetTrigger("Hit");
        }

        yield return new WaitForSeconds(0.3f);
        isHitAnimating = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Character player = FindObjectOfType<Character>();
        if (player != null)
        {
            GameStateManager.Instance.CurrentState.playerPosition = player.transform.position;
            Debug.Log($"[BOSS] Фиксируем позицию игрока перед сохранением: {player.transform.position}");
        }
        
        GameStateManager.Instance.CurrentState.isBossDead = true;
    
        GameStateManager.Instance.SaveGame();
        
        animator.SetTrigger("Death");
        if (deathSound != null) audioSource.PlayOneShot(deathSound);

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
    
        tupik.OpenExit();
        tupik.SavePlanksStateIfBossDead();
        Destroy(gameObject, 2f);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}