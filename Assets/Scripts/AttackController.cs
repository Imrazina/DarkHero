using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    
    public HitboxController hitbox;
    private Animator animator;
    private Rigidbody2D rb;
    public bool isAttacking = false;
    private bool canCombo = false; 
    private Queue<string> comboQueue = new Queue<string>();
    private string currentAttack = null;
    private bool isGrounded;

    private Dictionary<string, float> attackCooldowns = new Dictionary<string, float>();
    private Dictionary<string, float> lastAttackTime = new Dictionary<string, float>();

    void Start()
    {
        if (hitbox == null) hitbox = GetComponentInChildren<HitboxController>(); // Ищем HitboxController среди детей
        
        if (hitbox == null) Debug.LogError("HitboxController не найден! Назначь его в инспекторе.");

        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D не найден на Samurai!");
            return;
        }

        Transform spriteTransform = transform.Find("Sprite");
        if (spriteTransform != null)
        {
            animator = spriteTransform.GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Animator не найден на дочернем объекте Sprite!");
            return;
        }

        // Инициализация атак
        attackCooldowns["LightAttack"] = 0.5f;
        attackCooldowns["MediumAttack"] = 0.7f;
        attackCooldowns["HeavyAttack"] = 1f;
        attackCooldowns["AirAttack"] = 0.6f; 

        foreach (var key in attackCooldowns.Keys)
        {
            lastAttackTime[key] = 0f;
        }
        
        foreach (var key in attackCooldowns.Keys)
        {
            lastAttackTime[key] = 0f;
            Debug.Log($"Инициализирована атака: {key}");
        }
    }

    void Update()
    {
        if (animator == null || rb == null) return;

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
        float currentTime = Time.time;

        if (Input.GetKeyDown(KeyCode.J)) QueueAttack("LightAttack", currentTime);
        if (Input.GetKeyDown(KeyCode.K)) QueueAttack("MediumAttack", currentTime);
        if (Input.GetKeyDown(KeyCode.L)) QueueAttack("HeavyAttack", currentTime);
        if (Input.GetKeyDown(KeyCode.O) && !isGrounded) QueueAttack("AirAttack", currentTime); // Атака в воздухе

        if (Input.GetKey(KeyCode.H)) Block();
        else animator.SetBool("IsBlocking", false);
    }

    void QueueAttack(string attackType, float currentTime)
    {
        if (!lastAttackTime.ContainsKey(attackType))
        {
            Debug.LogError($"Атака '{attackType}' не найдена в словаре lastAttackTime!");
            return;
        }
        if (!attackCooldowns.ContainsKey(attackType))
        {
            Debug.LogError($"Атака '{attackType}' не найдена в словаре attackCooldowns!");
            return;
        }
    
        if (currentTime - lastAttackTime[attackType] < attackCooldowns[attackType]) 
        {
            Debug.Log($"Attack {attackType} on cooldown. Time remaining: {attackCooldowns[attackType] - (currentTime - lastAttackTime[attackType])}");
            return;
        }

        if (!isAttacking)
        {
            Debug.Log($"Queueing attack {attackType}");
            StartCoroutine(PerformAttack(attackType, currentTime));
        }
        else if (canCombo)
        {
            comboQueue.Enqueue(attackType);
            Debug.Log($"Combo queued for attack {attackType}");
        }
    }

    IEnumerator PerformAttack(string attackType, float currentTime)
    {
        isAttacking = true;
        canCombo = false;
        currentAttack = attackType;
        lastAttackTime[attackType] = currentTime;

        if (attackType == "HeavyAttack" && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 6f); 
            isGrounded = false;
        }
        
        Debug.Log("Performing attack: " + attackType);
        animator.SetTrigger(attackType);

        yield return new WaitForSeconds(attackCooldowns[attackType] * 0.7f);
        canCombo = true;
        
        yield return new WaitForSeconds(attackCooldowns[attackType] * 0.3f);
        
        isAttacking = false;
        canCombo = false;

        if (comboQueue.Count > 0)
        {
            string nextAttack = comboQueue.Dequeue();
            Debug.Log($"Next attack in combo: {nextAttack}");
            StartCoroutine(PerformAttack(nextAttack, Time.time));
        }
        
        if (hitbox != null)
        {
            Debug.Log($"Activating hitbox for {attackType}");
            hitbox.ActivateHitbox(attackType);
        }
        else
        {
            Debug.LogError("Ошибка: HitboxController не найден! Назначь его в инспекторе.");
        }
    }

    void Block()
    {
        animator.SetBool("IsBlocking", true);
    }
    
    public bool IsBlocking()
    {
        return animator.GetBool("IsBlocking");
    }

    public bool IsBlockingTowards(Vector3 attackerPosition)
    {
        if (!IsBlocking()) return false;
    
        // Определяем направление к атакующему
        Vector2 directionToAttacker = (attackerPosition - transform.position).normalized;
    
        // Определяем направление взгляда игрока (предполагаем, что спрайт повёрнут вправо)
        float facingDirection = transform.localScale.x > 0 ? 1 : -1;
    
        // Блок работает только против атак спереди
        return (directionToAttacker.x * facingDirection) > 0;
    }
}
