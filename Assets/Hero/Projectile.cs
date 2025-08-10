using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Налаштування снаряду")]
    public float speed = 10f;
    public float lifetime = 5f; // Максимальний час життя снаряду
    
    private Unit target;
    private int damage;
    private bool isCritical;
    private string attackerName;
    private Vector3 direction;
    private bool isInitialized = false;
    private float timeAlive = 0f;

    public void Initialize(Unit targetUnit, int damageAmount, float projectileSpeed, string attacker, bool critical = false)
    {
        target = targetUnit;
        damage = damageAmount;
        speed = projectileSpeed;
        attackerName = attacker;
        isCritical = critical;
        
        // Встановлюємо непрозорість для спрайту
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f; // Повністю непрозорий
            spriteRenderer.color = color;
        }
        
        if (target != null)
        {
            // Розраховуємо напрямок до цілі
            direction = (target.transform.position - transform.position).normalized;
            
            // Поворот для 2D (по Z-осі)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;
        
        timeAlive += Time.deltaTime;
        
        // Знищуємо снаряд якщо він живе занадто довго
        if (timeAlive >= lifetime)
        {
            DestroyProjectile();
            return;
        }
        
        // Перевіряємо чи ціль ще існує
        if (target == null || target.isDead)
        {
            // Продовжуємо рухатися в тому ж напрямку
            transform.position += direction * speed * Time.deltaTime;
            return;
        }
        
        // Оновлюємо напрямок до рухомої цілі (легке наведення)
        Vector3 newDirection = (target.transform.position - transform.position).normalized;
        direction = Vector3.Slerp(direction, newDirection, Time.deltaTime * 2f); // М'яке наведення
        
        // Рухаємо снаряд
        transform.position += direction * speed * Time.deltaTime;
        
        // Повертаємо снаряд по напрямку руху (2D поворот)
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // Перевіряємо відстань до цілі
        if (target != null && Vector3.Distance(transform.position, target.transform.position) < 0.5f)
        {
            HitTarget();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Перевіряємо чи снаряд влучив в ціль
        Unit hitUnit = other.GetComponent<Unit>();
        if (hitUnit != null && hitUnit == target)
        {
            HitTarget();
        }
        // Перевіряємо чи снаряд влучив в перешкоду
        else if (other.CompareTag("Obstacle") || other.CompareTag("Wall"))
        {
            DestroyProjectile();
        }
    }

    private void HitTarget()
    {
        if (target != null && !target.isDead)
        {
            // Завдаємо шкоди
            target.TakeDamage(damage);
            
            if (isCritical)
            {
                Debug.Log($"Снаряд від {attackerName} КРИТИЧНО влучив в {target.unitName}!");
            }
        }
        
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    // Метод для зміни цілі в польоті (якщо потрібно)
    public void SetNewTarget(Unit newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
    }

    // Метод для збільшення швидкості снаряду
    public void BoostSpeed(float multiplier)
    {
        speed *= multiplier;
    }

    private void OnDrawGizmosSelected()
    {
        // Показуємо напрямок руху снаряду
        if (isInitialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + direction * 2f);
        }
        
        // Показуємо ціль
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }
}