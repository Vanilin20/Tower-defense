using UnityEngine;

public abstract class Enemy : Unit
{
    [Header("Характеристики ворога")]
    public float aggroRange = 5f;
    
    private GameObject homeBase;
    private bool movingToBase = false;
    private Vector3 baseDirection = Vector3.left; // Напрямок до бази

    protected override void Start()
    {
        base.Start();
        gameObject.tag = "Enemy";
        homeBase = GameObject.FindGameObjectWithTag("Home");
        
        // Якщо база знайдена, визначаємо напрямок до неї
        if (homeBase != null)
        {
            baseDirection = (homeBase.transform.position - transform.position).normalized;
        }
    }

    protected override void FindTarget()
    {
        // Спочатку шукаємо героїв в радіусі агро
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
        Unit closestHero = null;
        float closestDistance = Mathf.Infinity;
        Unit currentlyFightingHero = null; // Герой який вже б'ється з іншими ворогами

        foreach (GameObject heroObj in heroes)
        {
            Unit hero = heroObj.GetComponent<Unit>();
            if (hero == null || hero.isDead) continue;

            float distance = Vector3.Distance(transform.position, hero.transform.position);
            
            if (distance <= aggroRange)
            {
                // Перевіряємо чи цей герой вже б'ється з іншими ворогами
                bool heroInCombat = IsHeroInCombatWithEnemies(hero);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHero = hero;
                }
                
                // Пріоритет героям які вже в бою (для групових атак)
                if (heroInCombat && currentlyFightingHero == null)
                {
                    currentlyFightingHero = hero;
                }
            }
        }

        // Вибираємо ціль: пріоритет героям в бою, потім найближчому
        if (currentlyFightingHero != null)
        {
            currentTarget = currentlyFightingHero;
            movingToBase = false;
            Debug.Log($"{unitName} приєднується до атаки героя: {currentlyFightingHero.unitName}");
        }
        else if (closestHero != null)
        {
            currentTarget = closestHero;
            movingToBase = false;
            Debug.Log($"{unitName} знайшов героя: {closestHero.unitName}");
        }
        else
        {
            // Якщо немає героїв поблизу, йдемо до бази
            if (homeBase != null)
            {
                Unit baseUnit = homeBase.GetComponent<Unit>();
                if (baseUnit != null && !baseUnit.isDead)
                {
                    currentTarget = baseUnit;
                    movingToBase = true;
                }
            }
            else
            {
                // Якщо база не знайдена, просто рухаємося вліво
                currentTarget = null;
                movingToBase = true;
            }
        }
    }
    
    // Перевіряє чи герой вже б'ється з іншими ворогами
    private bool IsHeroInCombatWithEnemies(Unit hero)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemyObj in enemies)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy == null || enemy.isDead || enemy == this) continue;
            
            // Перевіряємо чи інший ворог атакує цього героя
            if (enemy.currentTarget == hero)
            {
                float distanceBetweenEnemyAndHero = Vector3.Distance(enemy.transform.position, hero.transform.position);
                if (distanceBetweenEnemyAndHero <= enemy.attackRange * 1.5f) // Трохи більший радіус для перевірки
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // Переміщення з урахуванням руху до бази
    protected override void MoveTowards(Vector3 targetPosition)
    {
        if (movingToBase && homeBase == null)
        {
            // Рух вліво якщо немає конкретної бази
            Vector3 leftTarget = transform.position + Vector3.left * 10f;
            leftTarget.y = transform.position.y; // Зберігаємо висоту
            base.MoveTowards(leftTarget);
        }
        else if (movingToBase)
        {
            // Рух до конкретної бази
            Vector3 baseTarget = homeBase.transform.position;
            baseTarget.y = transform.position.y; // Спочатку рухаємося на своїй висоті
            base.MoveTowards(baseTarget);
        }
        else
        {
            // Стандартний рух до героя з переходом на спільну висоту
            base.MoveTowards(targetPosition);
        }
    }
    
    // Вороги переходять на висоту героя для атаки
    protected override bool ShouldMoveToCommonHeight()
    {
        if (movingToBase) return false; // Не переходимо на спільну висоту при русі до бази
        
        if (currentTarget == null) return false;
        
        float heightDiff = Mathf.Abs(transform.position.y - currentTarget.transform.position.y);
        float horizontalDistance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(currentTarget.transform.position.x, 0, currentTarget.transform.position.z)
        );
        
        // Ворог переходить на висоту героя якщо той в радіусі агро
        return heightDiff > HeightManager.Instance.heightTolerance && horizontalDistance <= aggroRange;
    }
    
    // Спеціальна логіка позиціонування для ворогів
    protected override void StartHeightRepositioning()
    {
        if (movingToBase) return; // Не перепозиціонуємося при русі до бази
        
        base.StartHeightRepositioning();
    }

    protected override void Die()
    {
        base.Die();
        Debug.Log($"Ворог {unitName} знищений! Можна отримати винагороду.");
        
        // Тут можна додати логіку винагороди
        // GameManager.Instance?.AddGold(goldReward);
        // GameManager.Instance?.AddExperience(expReward);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Показуємо радіус агро
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        
        // Показуємо напрямок до бази
        if (homeBase != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, homeBase.transform.position);
        }
    }
    
    public bool IsMovingToBase()
    {
        return movingToBase;
    }
    
    // Метод для зміни цілі бази (якщо потрібно динамічно)
    public void SetHomeBase(GameObject newBase)
    {
        homeBase = newBase;
        if (homeBase != null)
        {
            baseDirection = (homeBase.transform.position - transform.position).normalized;
        }
    }
}