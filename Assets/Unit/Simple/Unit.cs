using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitStats stats;

    private Vector3 targetPosition;
    private GameObject enemyTarget;
    private float attackTimer;

    private int currentHealth;
    private ZoneController myZone; // 🎯 Посилання на зону
    private List<GameObject> inZoneEnemies = new List<GameObject>();

    // 🆕 ДОДАНО: Посилання на карту-власника для системи кулдауну
    private CardDragSpawner ownerCard;

    void Start()
    {
        currentHealth = stats.maxHealth;
    }

    void Update()
    {
        // 🔁 Якщо втрачено ціль — вибрати іншого ворога, якщо є
        if (enemyTarget == null)
        {
            RemoveNullsFromZone();
            if (inZoneEnemies.Count > 0)
            {
                enemyTarget = inZoneEnemies[0];
            }
        }

        if (enemyTarget != null)
        {
            float dist = Vector2.Distance(transform.position, enemyTarget.transform.position);
            if (dist > stats.attackRange)
            {
                // 🎯 Рух до ворога з вирівнюванням по висоті
                MoveTowardsEnemy();
            }
            else
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0f)
                {
                    Attack();
                    attackTimer = stats.attackCooldown;
                }
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                MoveTowards(targetPosition);
            }
        }
    }

    void MoveTowardsEnemy()
    {
        if (enemyTarget == null) return;

        Vector3 enemyPos = enemyTarget.transform.position;
        Vector3 currentPos = transform.position;
        
        // 📐 Вирівнювання по Y-осі - рухаємося до однієї висоти з ворогом
        float enemyY = enemyPos.y;
        float currentY = currentPos.y;
        
        Vector2 direction = Vector2.zero;
        
        // Якщо різниця по Y більша за певний поріг, спочатку вирівнюємо висоту
        float yDifference = Mathf.Abs(enemyY - currentY);
        if (yDifference > 0.1f)
        {
            // Рух по Y-осі для вирівнювання
            direction.y = enemyY > currentY ? 1f : -1f;
            
            // Також рухаємося по X, але повільніше
            if (enemyPos.x > currentPos.x)
            {
                direction.x = 0.5f;
            }
        }
        else
        {
            // Коли висота вирівняна, рухаємося в бік ворога
            direction = (enemyPos - currentPos).normalized;
        }
        
        transform.Translate(direction * stats.moveSpeed * Time.deltaTime);
    }

    void MoveTowards(Vector3 position)
    {
        Vector2 dir = (position - transform.position).normalized;
        transform.Translate(dir * stats.moveSpeed * Time.deltaTime);
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    // 📥 Викликається ззовні, щоб прив'язати юніта до зони
    public void AssignZone(ZoneController zone)
    {
        myZone = zone;
    }

    // 🆕 ДОДАНО: Метод для встановлення карти-власника
    public void SetOwnerCard(CardDragSpawner card)
    {
        ownerCard = card;
        Debug.Log($"Unit {gameObject.name} assigned to card {card.gameObject.name}");
    }

    // 🆕 ДОДАНО: Метод для розриву зв'язку з картою (якщо потрібно)
    public void RemoveOwnerCard()
    {
        ownerCard = null;
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            Die(); // 🔄 Викликаємо метод смерті
        }
    }

    // 🆕 ДОДАНО: Централізований метод смерті
    public void Die()
    {
        // 🔓 Звільняємо зону, якщо юніт прив'язаний
        if (myZone != null)
        {
            myZone.hasUnit = false;
        }

        // 🆕 Повідомляємо карту що юніт помер (знімаємо кулдаун)
        if (ownerCard != null)
        {
            ownerCard.OnUnitDestroyed();
        }

        // Знищуємо юніт
        Destroy(gameObject);
    }

    // 🆕 ДОДАНО: Викликається коли юніт знищується (додаткова перевірка)
    void OnDestroy()
    {
        // Повідомляємо карту що юніт помер (якщо ще не повідомили)
        if (ownerCard != null)
        {
            ownerCard.OnUnitDestroyed();
        }
    }

    void Attack()
    {
        if (enemyTarget != null && enemyTarget.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.TakeDamage(stats.damage);

            if (enemy == null || enemy.IsDead())
            {
                enemyTarget = null;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!inZoneEnemies.Contains(other.gameObject))
                inZoneEnemies.Add(other.gameObject);

            if (enemyTarget == null)
            {
                enemyTarget = other.gameObject;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            inZoneEnemies.Remove(other.gameObject);

            if (enemyTarget == other.gameObject)
            {
                enemyTarget = null;
            }
        }
    }

    void RemoveNullsFromZone()
    {
        inZoneEnemies.RemoveAll(enemy => enemy == null);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}