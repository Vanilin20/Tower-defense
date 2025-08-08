using UnityEngine;
using System.Collections;

// Базовий клас для всіх юнітів
public abstract class Unit : MonoBehaviour
{
    [Header("Основні характеристики")]
    public string unitName;
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 20;
    public float attackRange = 2f;
    public float attackSpeed = 1f; // атак за секунду
    public float moveSpeed = 3f;
    public float critChance = 0.1f; // 10% шанс на критичний удар
    public float critMultiplier = 2f; // множник критичного урону

    public bool isDead = false;
    protected float lastAttackTime = 0f;
    protected Animator animator;
    protected Unit currentTarget;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        if (isDead) return;

        FindTarget();
        if (currentTarget != null && !currentTarget.isDead)
        {
            HandleCombat();
        }
        else
        {
            // Коли немає цілі - скидаємо всі анімації
            if (currentTarget != null && currentTarget.isDead)
            {
                Debug.Log($"{unitName} втратив ціль (ціль загинула)");
            }
            
            currentTarget = null;
            ResetCombatAnimations();
        }
    }

    protected abstract void FindTarget();

    protected virtual void HandleCombat()
    {
        if (currentTarget == null || currentTarget.isDead)
        {
            ResetCombatAnimations();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distanceToTarget <= attackRange)
        {
            // Зупиняємося і атакуємо
            if (animator != null)
                animator.SetBool("isRunning", false);

            if (Time.time - lastAttackTime >= 1f / attackSpeed)
            {
                Attack(currentTarget);
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // Наближаємося до цілі
            MoveTowards(currentTarget.transform.position);
        }
    }

    protected virtual void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (animator != null)
            animator.SetBool("isRunning", true);
    }

    protected virtual void Attack(Unit target)
    {
        if (target == null || target.isDead) return;

        // Перевіряємо критичний удар
        bool isCritical = Random.Range(0f, 1f) < critChance;
        int finalDamage = isCritical ? Mathf.RoundToInt(damage * critMultiplier) : damage;

        if (isCritical)
        {
            Debug.Log($"{unitName} завдає критичний удар {target.unitName} на {finalDamage} пошкоджень!");
        }
        else
        {
            Debug.Log($"{unitName} атакує {target.unitName} на {finalDamage} пошкоджень!");
        }

        if (animator != null)
            animator.SetTrigger("isAttacking");

        target.TakeDamage(finalDamage);
    }

    // Новий метод для скидання анімацій бою
    protected virtual void ResetCombatAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            // Скидаємо анімацію атаки встановивши idle стан
            animator.ResetTrigger("isAttacking");
            // Якщо у вас є параметр для idle стану, можна його встановити
            // animator.SetBool("isIdle", true);
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        
        // Обмежуємо currentHealth мінімальним значенням 0
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"{unitName} отримав {damageAmount} пошкоджень. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        Debug.Log($"{unitName} загинув!");

        // Скидаємо всі анімації перед смертю
        ResetCombatAnimations();

        if (animator != null)
            animator.SetTrigger("isDead");

        // Вимикаємо колайдер щоб інші юніти не намагались атакувати труп
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Знищуємо об'єкт через 3 секунди
        Destroy(gameObject, 3f);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Показуємо радіус атаки в редакторі
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}