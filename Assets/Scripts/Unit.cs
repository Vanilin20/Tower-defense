using UnityEngine;
// Розширений базовий клас Unit з підтримкою системи висот
public abstract class Unit : MonoBehaviour
{
    [Header("Основні характеристики")]
    public string unitName;
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 20;
    public float attackRange = 2f;
    public float attackSpeed = 1f;
    public float moveSpeed = 3f;
    public float critChance = 0.1f;
    public float critMultiplier = 2f;

    [Header("Система висот")]
    public float heightMoveSpeed = 5f; // Швидкість переміщення по висоті
    public bool isStaticUnit = false; // Прапор для статичних об'єктів (бази, вежі)
    
    public bool isDead = false;
    protected float lastAttackTime = 0f;
    protected Animator animator;
    protected Unit currentTarget;
    
    // Змінні для системи висот
    protected bool isMovingToHeight = false;
    protected float targetHeight;
    protected Vector3 combatPosition; // Позиція для бою
    public bool isRepositioning = false; // Публічна змінна для перевірки іншими юнітами

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        
        // Переміщуємо юніт на найближчу доступну висоту при створенні
        // ТІЛЬКИ якщо це НЕ статичний об'єкт
        if (HeightManager.Instance != null && !isStaticUnit)
        {
            float nearestHeight = HeightManager.Instance.GetNearestHeight(transform.position.y);
            transform.position = new Vector3(transform.position.x, nearestHeight, transform.position.z);
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // Спочатку обробляємо переміщення по висоті
        if (isMovingToHeight)
        {
            HandleHeightMovement();
            return;
        }
        
        // Якщо перепозиціонуємося для бою
        if (isRepositioning)
        {
            HandleRepositioning();
            return;
        }

        FindTarget();
        if (currentTarget != null && !currentTarget.isDead)
        {
            // Перевіряємо чи потрібно переміститися на спільну висоту
            if (ShouldMoveToCommonHeight())
            {
                StartHeightRepositioning();
            }
            else
            {
                HandleCombat();
            }
        }
        else
        {
            currentTarget = null;
            isRepositioning = false;
            ResetCombatAnimations();
        }
    }

    protected abstract void FindTarget();

    // Перевіряє чи потрібно переміщуватися на спільну висоту
    protected virtual bool ShouldMoveToCommonHeight()
    {
        // Статичні об'єкти ніколи не переміщуються
        if (isStaticUnit) return false;
        
        if (currentTarget == null) return false;
        
        float heightDiff = Mathf.Abs(transform.position.y - currentTarget.transform.position.y);
        
        if (heightDiff <= HeightManager.Instance.heightTolerance)
        {
            return false; // Вже на одній висоті
        }
        
        // Перевіряємо чи ціль теж не переміщується до нас (щоб уникнути циклу)
        if (IsTargetMovingToMyHeight())
        {
            return false; // Не рухаємося якщо ціль вже йде до нас
        }
        
        return true;
    }
    
    // Перевіряє чи ціль переміщується на нашу висоту
    protected virtual bool IsTargetMovingToMyHeight()
    {
        if (currentTarget == null) return false;
        
        // Перевіряємо чи ціль в процесі репозиціювання
        Unit targetUnit = currentTarget.GetComponent<Unit>();
        if (targetUnit != null && targetUnit.isRepositioning)
        {
            float myHeight = transform.position.y;
            float targetDestinationHeight = targetUnit.combatPosition.y;
            
            // Якщо ціль йде на нашу висоту, не рухаємося
            if (Mathf.Abs(targetDestinationHeight - myHeight) <= HeightManager.Instance.heightTolerance)
            {
                Debug.Log($"{unitName} чекає - {currentTarget.unitName} йде на мою висоту");
                return true;
            }
        }
        
        return false;
    }
    
    // Починає процес переміщення на спільну висоту
    protected virtual void StartHeightRepositioning()
    {
        if (HeightManager.Instance == null || currentTarget == null) return;
        
        // Визначаємо хто має переміщуватися на чию висоту
        float finalHeight = DetermineTargetHeight();
        
        targetHeight = finalHeight;
        combatPosition = new Vector3(transform.position.x, finalHeight, transform.position.z);
        isRepositioning = true;
        
        Debug.Log($"{unitName} переміщується на висоту {finalHeight} для бою з {currentTarget.unitName}");
    }
    
    // Визначає на яку висоту має переміщуватися юніт
    protected virtual float DetermineTargetHeight()
    {
        if (currentTarget == null) return transform.position.y;
        
        float myHeight = transform.position.y;
        float targetHeight = currentTarget.transform.position.y;
        
        // За замовчуванням переміщуємося на висоту цілі
        return targetHeight;
    }
    
    // Обробляє переміщення до позиції для бою
    protected virtual void HandleRepositioning()
    {
        Vector3 targetPos = combatPosition;
        float distance = Vector3.Distance(transform.position, targetPos);
        
        if (distance > 0.1f)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            if (animator != null)
                animator.SetBool("isRunning", true);
        }
        else
        {
            isRepositioning = false;
            if (animator != null)
                animator.SetBool("isRunning", false);
        }
    }

    // Обробляє рух по висоті (вертикальне переміщення)
    protected virtual void HandleHeightMovement()
    {
        float currentY = transform.position.y;
        float direction = targetHeight > currentY ? 1f : -1f;
        
        float newY = currentY + direction * heightMoveSpeed * Time.deltaTime;
        
        // Перевіряємо чи досягли цільової висоти
        if (Mathf.Abs(newY - targetHeight) <= 0.1f)
        {
            newY = targetHeight;
            isMovingToHeight = false;
        }
        
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        if (animator != null)
        {
            animator.SetBool("isRunning", isMovingToHeight);
        }
    }

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
            MoveTowards(currentTarget.transform.position);
        }
    }

    protected virtual void MoveTowards(Vector3 targetPosition)
    {
        // Рухаємося тільки по X та Z, висота вже встановлена
        Vector3 direction = new Vector3(
            targetPosition.x - transform.position.x, 
            0, 
            targetPosition.z - transform.position.z
        ).normalized;
        
        Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        newPosition.y = transform.position.y; // Зберігаємо поточну висоту
        
        transform.position = newPosition;

        if (animator != null)
            animator.SetBool("isRunning", true);
    }

    protected virtual void Attack(Unit target)
    {
        if (target == null || target.isDead) return;

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

    protected virtual void ResetCombatAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.ResetTrigger("isAttacking");
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
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

        ResetCombatAnimations();

        if (animator != null)
            animator.SetTrigger("isDead");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 3f);
    }

    // Публічні методи для роботи з висотами
    public void MoveToHeight(float height)
    {
        if (HeightManager.Instance != null && HeightManager.Instance.GetAvailableHeights().Contains(height))
        {
            targetHeight = height;
            isMovingToHeight = true;
        }
    }
    
    public float GetCurrentHeight()
    {
        return transform.position.y;
    }
    
    public bool IsOnSameHeight(Unit other)
    {
        if (other == null) return false;
        return Mathf.Abs(transform.position.y - other.transform.position.y) <= HeightManager.Instance.heightTolerance;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Показуємо радіус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Показуємо доступні висоти
        if (HeightManager.Instance != null)
        {
            Gizmos.color = Color.green;
            foreach (float height in HeightManager.Instance.GetAvailableHeights())
            {
                Vector3 heightPos = new Vector3(transform.position.x, height, transform.position.z);
                Gizmos.DrawWireCube(heightPos, new Vector3(0.5f, 0.1f, 0.5f));
            }
        }
    }
}