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
    public Animator animator;
    public Unit currentTarget;
    
    // Змінні для системи висот
    protected bool isMovingToHeight = false;
    protected float targetHeight;
    protected Vector3 combatPosition; // Позиція для бою
    public bool isRepositioning = false; // Публічна змінна для перевірки іншими юнітами

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        
        // Встановлюємо початковий напрямок спрайта
        SpriteRotationUtils.SetInitialDirection(transform, gameObject.CompareTag("Hero"));
        
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
        if (!CombatUtils.ShouldMoveToCommonHeight(this, currentTarget, isStaticUnit)) return false;
        
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
            
                    AnimationUtils.SetRunning(this, true);
        }
        else
        {
            isRepositioning = false;
            AnimationUtils.SetRunning(this, false);
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
        
        AnimationUtils.SetRunning(this, isMovingToHeight);
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
                    AnimationUtils.SetRunning(this, false);

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
        CombatUtils.MoveTowardsTarget(this, targetPosition, moveSpeed);
    }

    protected virtual void Attack(Unit target)
    {
        if (target == null || target.isDead) return;

        int finalDamage = CombatUtils.CalculateDamage(damage, critChance, critMultiplier, out bool isCritical);
        CombatUtils.PerformAttack(this, target, finalDamage, isCritical);
    }

    protected virtual void ResetCombatAnimations()
    {
        AnimationUtils.ResetCombatAnimations(this);
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"{unitName} отримав {damageAmount} пошкоджень. HP: {currentHealth}/{maxHealth}");

        // Якщо герой атакований під час руху до зони, не зупиняємо рух
        // ZoneMovement сам керує своєю поведінкою

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        DeathHandler.HandleDeath(this);
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