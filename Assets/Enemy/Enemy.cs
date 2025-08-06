using UnityEngine;

// Базовий клас для ворогів з рухом вліво та атакою бази
public abstract class Enemy : Unit
{
    [Header("Характеристики ворога")]
    public float aggroRange = 5f;
    
    private GameObject homeBase;
    private bool movingToBase = false;

    protected override void Start()
    {
        base.Start();
        gameObject.tag = "Enemy"; // Встановлюємо тег для ворога
        // Знаходимо базу з тегом "Home"
        homeBase = GameObject.FindGameObjectWithTag("Home");
    }

    protected override void FindTarget()
    {
        // Спочатку шукаємо героїв в радіусі агро
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
        Unit closestHero = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject heroObj in heroes)
        {
            Unit hero = heroObj.GetComponent<Unit>();
            if (hero == null || hero.isDead) continue;

            float distance = Vector3.Distance(transform.position, hero.transform.position);
            if (distance < closestDistance && distance <= aggroRange)
            {
                closestDistance = distance;
                closestHero = hero;
            }
        }

        if (closestHero != null)
        {
            currentTarget = closestHero;
            movingToBase = false;
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
        }
    }

    protected override void MoveTowards(Vector3 targetPosition)
    {
        Vector3 actualTarget;
        
        if (movingToBase || currentTarget == homeBase?.GetComponent<Unit>())
        {
            // Для руху до бази використовуємо позицію зліва від поточної
            actualTarget = transform.position + Vector3.left;
        }
        else
        {
            // Стандартний рух до героя
            actualTarget = targetPosition;
        }
        
        // Викликаємо базову логіку руху (вона включає рух та анімацію)
        base.MoveTowards(actualTarget);
    }

    protected override void Die()
    {
        base.Die();
        // Можна додати логіку винагороди за вбивство ворога
        Debug.Log($"Ворог {unitName} знищений!");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Показуємо радіус агро
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
    
    // Публічний метод для перевірки чи йде ворог до бази
    public bool IsMovingToBase()
    {
        return movingToBase;
    }
}