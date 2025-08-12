using UnityEngine;

public class RangedHero : Hero
{
    [Header("Характеристики дальнього бою")]
    [SerializeField] private GameObject projectilePrefab; // Префаб снаряду
    [SerializeField] private Transform firePoint; // Точка пострілу
    [SerializeField] private float projectileSpeed = 10f; // Швидкість снаряду
    [SerializeField] private float minRangedDistance = 3f; // Мінімальна дистанція для дальнього бою
    [SerializeField] private float meleeRange = 1.5f; // Дистанція переходу в ближній бій
    [SerializeField] private int meleeDamage = 15; // Шкода в ближньому бою (менша ніж дальній)
    
    private bool isInMeleeMode = false; // Флаг ближнього бою

    protected override void Start()
    {
        base.Start();
        
        // Встановлюємо більший радіус атаки для дальнього бою
        if (attackRange == 2f) attackRange = 6f;
        
        // Перевіряємо наявність точки пострілу
        if (firePoint == null)
        {
            // Створюємо точку пострілу якщо її немає
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0.5f, 0.5f, 0); // Трохи попереду та вище
            firePoint = fp.transform;
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        // Визначаємо режим бою
        DetermineCombatMode();

        base.Update();
    }

    private void DetermineCombatMode()
    {
        if (currentTarget == null)
        {
            isInMeleeMode = false;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        
        // Переходимо в ближній бій якщо ворог дуже близько
        if (distanceToTarget <= meleeRange)
        {
            if (!isInMeleeMode)
            {
                Debug.Log($"{unitName} переходить в ближній бій!");
                isInMeleeMode = true;
            }
        }
        // Повертаємося до дальнього бою якщо ворог віддалився
        else if (distanceToTarget > minRangedDistance)
        {
            if (isInMeleeMode)
            {
                Debug.Log($"{unitName} повертається до дальнього бою!");
                isInMeleeMode = false;
            }
        }
    }

    protected override void Attack(Unit target)
    {
        if (target == null || target.isDead) return;

        if (isInMeleeMode)
        {
            PerformMeleeAttack(target);
        }
        else
        {
            PerformRangedAttack(target);
        }
    }

    private void PerformMeleeAttack(Unit target)
    {
        int finalDamage = CombatUtils.CalculateDamage(meleeDamage, critChance, critMultiplier, out bool isCritical);
        CombatUtils.PerformAttack(this, target, finalDamage, isCritical, "атакує в ближньому бою");
        
        AnimationUtils.TriggerMeleeAttack(animator);
    }

    private void PerformRangedAttack(Unit target)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{unitName} не має префабу снаряду! Використовую миттєву атаку.");
            PerformInstantRangedAttack(target);
            return;
        }

        // Створюємо снаряд
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Налаштовуємо снаряд
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent == null)
        {
            projectileComponent = projectile.AddComponent<Projectile>();
        }

        // Розраховуємо шкоду
        bool isCritical = Random.Range(0f, 1f) < critChance;
        int finalDamage = isCritical ? Mathf.RoundToInt(damage * critMultiplier) : damage;

        projectileComponent.Initialize(target, finalDamage, projectileSpeed, unitName, isCritical);

        if (isCritical)
        {
            Debug.Log($"{unitName} стріляє критичним снарядом в {target.unitName} на {finalDamage} пошкоджень!");
        }
        else
        {
            Debug.Log($"{unitName} стріляє в {target.unitName} на {finalDamage} пошкоджень!");
        }

        AnimationUtils.TriggerRangedAttack(animator);
    }

    private void PerformInstantRangedAttack(Unit target)
    {
        // Запасний варіант якщо немає префабу снаряду
        int finalDamage = CombatUtils.CalculateDamage(damage, critChance, critMultiplier, out bool isCritical);
        CombatUtils.PerformAttack(this, target, finalDamage, isCritical, "завдає дальній удар");
        
        AnimationUtils.TriggerRangedAttack(animator);
    }

    // Спеціальна логіка руху для дальнього бою
    protected override void MoveTowards(Vector3 targetPosition)
    {
        // Завжди рухаємося до цілі, незалежно від режиму бою
        base.MoveTowards(targetPosition);
    }

    // Перевизначаємо логіку бою для врахування дистанції
    protected override void HandleCombat()
    {
        if (currentTarget == null || currentTarget.isDead)
        {
            ResetCombatAnimations();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        
        // Логіка атаки залежно від режиму
        if (isInMeleeMode)
        {
            // Ближній бій - стандартна логіка
            if (distanceToTarget <= meleeRange)
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
        else
        {
            // Дальній бій
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
    }

    // Переміщення на спільну висоту тільки в ближньому бою
    protected override bool ShouldMoveToCommonHeight()
    {
        if (currentTarget == null) return false;
        
        // Переходимо на спільну висоту тільки якщо в ближньому бою
        if (!isInMeleeMode) return false;
        
        float heightDiff = Mathf.Abs(transform.position.y - currentTarget.transform.position.y);
        
        // Герой переходить на висоту ворога тільки в ближньому бою
        return heightDiff > HeightManager.Instance.heightTolerance;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Показуємо мінімальну дистанцію для дальнього бою
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minRangedDistance);
        
        // Показуємо дистанцію ближнього бою
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        
        // Показуємо точку пострілу
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
    
    // Публічні методи
    public bool IsInMeleeMode()
    {
        return isInMeleeMode;
    }

    public void SetProjectilePrefab(GameObject prefab)
    {
        projectilePrefab = prefab;
    }

    public void SetFirePoint(Transform point)
    {
        firePoint = point;
    }
}