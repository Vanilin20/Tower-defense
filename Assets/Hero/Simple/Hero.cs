using UnityEngine;
using System.Collections;

public abstract class Hero : Unit
{
    [SerializeField] protected float enemySearchRadius = 5f; // Радіус пошуку ворогів
    private bool isInCombat = false;
    
    protected override void Start()
    {
        base.Start();
        gameObject.tag = "Hero"; // Встановлюємо тег для героя
    }
    
    protected override void Update()
    {
        if (isDead) return;
        
        // Використовуємо базову логіку, але додаємо контроль стану бою
        if (!isInCombat)
        {
            FindTarget();
        }
        
        if (currentTarget != null && !currentTarget.isDead)
        {
            isInCombat = true;
            HandleCombat(); // Використовуємо базову реалізацію
        }
        else
        {
            isInCombat = false;
            currentTarget = null;
            // Анімація зупинки руху вже обробляється в базовому HandleCombat()
        }
    }
    
    protected override void FindTarget()
    {
        // Шукаємо найближчого ворога в радіусі пошуку
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Unit closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (GameObject enemyObj in enemies)
        {
            Unit enemy = enemyObj.GetComponent<Unit>();
            if (enemy == null || enemy.isDead) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= enemySearchRadius)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        
        currentTarget = closestEnemy;
        
        if (currentTarget != null)
        {
            Debug.Log($"{unitName} знайшов ворога: {currentTarget.unitName}");
        }
    }
    
    // Перевизначаємо MoveTowards для додавання логіки повороту героя
    protected override void MoveTowards(Vector3 targetPosition)
    {
        // Викликаємо базову логіку руху та анімації
        base.MoveTowards(targetPosition);
        
        // Додаємо тільки унікальну логіку повороту для героя (2D)
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    
    protected override void Die()
    {
        base.Die(); // Використовуємо базову логіку смерті
        
        // Додаткова логіка для смерті героя
        Debug.Log($"Герой {unitName} загинув");
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // Показуємо радіус атаки з базового класу
        
        // Додаємо радіус пошуку ворогів
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemySearchRadius);
    }
    
    // Публічний метод для отримання інформації про героя
    public bool IsInCombat()
    {
        return isInCombat;
    }
}