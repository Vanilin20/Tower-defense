using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public UnitStats stats;
    private GameObject target;
    private float attackTimer;
    public int gold = 1;
    private int currentHealth;

    private List<GameObject> inZoneUnits = new List<GameObject>();
    private Animator animator;
    private bool isDead = false;
    
    // 🎨 Компоненти для ефекту зникнення
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    
    // 💰 Префаб золота для спавну
    public GameObject goldPrefab;

    void Start()
    {
        currentHealth = stats.maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isDead) return;

        // 🔄 Очищення і вибір нової цілі
        if (target == null)
        {
            RemoveNullsFromZone();
            if (inZoneUnits.Count > 0)
            {
                target = inZoneUnits[0];
            }
        }

        if (target != null)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance > stats.attackRange)
            {
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttacking", false);

                // 🎯 Рух до цілі з вирівнюванням по Y-осі
                MoveTowardsTarget();
            }
            else
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttacking", true);

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
            // 🔄 Біг вліво, якщо нема цілі
            animator.SetBool("isRunning", true);
            animator.SetBool("isAttacking", false);

            transform.Translate(Vector2.left * stats.moveSpeed * Time.deltaTime);
        }
    }

    void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 targetPos = target.transform.position;
        Vector3 currentPos = transform.position;
        
        // 📐 Вирівнювання по Y-осі - рухаємося до однієї висоти з ціллю
        float targetY = targetPos.y;
        float currentY = currentPos.y;
        
        Vector2 direction = Vector2.zero;
        
        // Якщо різниця по Y більша за певний поріг, спочатку вирівнюємо висоту
        float yDifference = Mathf.Abs(targetY - currentY);
        if (yDifference > 0.1f)
        {
            // Рух по Y-осі для вирівнювання
            direction.y = targetY > currentY ? 1f : -1f;
            
            // Також рухаємося по X, але повільніше
            if (targetPos.x < currentPos.x)
            {
                direction.x = -0.5f;
            }
        }
        else
        {
            // Коли висота вирівняна, рухаємося в бік цілі
            direction = (targetPos - currentPos).normalized;
        }
        
        transform.Translate(direction * stats.moveSpeed * Time.deltaTime);
    }

    void Attack()
    {
        if (target == null) return;

        if (target.CompareTag("Unit") && target.TryGetComponent<Unit>(out Unit unit))
        {
            unit.TakeDamage(stats.damage);

            if (unit == null || unit.IsDead())
            {
                target = null;
            }
        }
        else if (target.CompareTag("Home"))
        {
            if (target.TryGetComponent<HomeBase>(out HomeBase home))
            {
                home.TakeDamage(stats.damage);
            }
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", true);

        // 🚫 Вимикаємо колайдер, щоб ворог не заважав
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        // 💰 Спавнимо золото замість додавання в GoldManager
        SpawnGold();
        
        // 🎭 Запускаємо корутину зникнення
        StartCoroutine(FadeOutAndDestroy());
    }

    // 💰 Метод для спавну золота
    void SpawnGold()
    {
        if (goldPrefab != null)
        {
            GameObject spawnedGold = Instantiate(goldPrefab, transform.position, Quaternion.identity);
            
            // Передаємо кількість золота в префаб
            GoldPickup goldPickup = spawnedGold.GetComponent<GoldPickup>();
            if (goldPickup != null)
            {
                goldPickup.SetGoldAmount(gold);
            }
        }
    }

    // 🌟 Корутина для плавного зникнення
    IEnumerator FadeOutAndDestroy()
    {
        float fadeSpeed = 2f; // Швидкість зникнення
        float waitTime = 1f; // Час очікування перед початком зникнення
        
        // Чекаємо трохи, щоб анімація смерті відтворилась
        yield return new WaitForSeconds(waitTime);
        
        // Поступово зменшуємо прозорість
        while (spriteRenderer.color.a > 0f)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a -= fadeSpeed * Time.deltaTime;
            spriteRenderer.color = currentColor;
            
            yield return null; // Чекаємо наступний кадр
        }
        
        // Знищуємо об'єкт після повного зникнення
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Unit"))
        {
            if (!inZoneUnits.Contains(other.gameObject))
                inZoneUnits.Add(other.gameObject);

            if (target == null)
            {
                target = other.gameObject;
            }
        }
        else if (other.CompareTag("Home") && target == null && inZoneUnits.Count == 0)
        {
            target = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Unit"))
        {
            inZoneUnits.Remove(other.gameObject);
            if (target == other.gameObject)
            {
                target = null;
            }
        }
        else if (other.CompareTag("Home") && target == other.gameObject)
        {
            target = null;
        }
    }

    void RemoveNullsFromZone()
    {
        inZoneUnits.RemoveAll(unit => unit == null);
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}