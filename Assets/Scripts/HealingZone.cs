using UnityEngine;

public class HealingZone : MonoBehaviour
{
    [Header("Налаштування хілу")]
    [SerializeField] private int healAmount = 50; // Кількість хп за один хіл
    [SerializeField] private float healInterval = 1f; // Інтервал між хілами в секундах
    [SerializeField] private bool healToMaxInstantly = true; // Хілить до максимума миттєво чи поступово
    

    
    // Словник для відстеження героїв в зоні та часу останнього хілу
    private System.Collections.Generic.Dictionary<Hero, float> heroesInZone = 
        new System.Collections.Generic.Dictionary<Hero, float>();

    private void Start()
    {
        // Перевіряємо чи є 2D колайдер з тригером
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"HealingZone на {gameObject.name} потребує Collider2D компонент!");
            return;
        }
        
        if (!col.isTrigger)
        {
            Debug.LogWarning($"Collider2D на {gameObject.name} має бути налаштований як Trigger!");
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        // Обробляємо хіл для всіх героїв в зоні
        var heroesToRemove = new System.Collections.Generic.List<Hero>();
        
        foreach (var kvp in heroesInZone)
        {
            Hero hero = kvp.Key;
            float lastHealTime = kvp.Value;
            
            // Перевіряємо чи герой ще існує та не мертвий
            if (hero == null || hero.isDead)
            {
                heroesToRemove.Add(hero);
                continue;
            }
            
            // Перевіряємо чи потрібно хілити та чи минув інтервал
            if (CanHealHero(hero) && Time.time - lastHealTime >= healInterval)
            {
                HealHero(hero);
                heroesInZone[hero] = Time.time; // Оновлюємо час останнього хілу
            }
        }
        
        // Видаляємо мертвих/неіснуючих героїв зі словника
        foreach (Hero hero in heroesToRemove)
        {
            heroesInZone.Remove(hero);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Перевіряємо чи це герой
        Hero hero = other.GetComponent<Hero>();
        if (hero == null || hero.isDead) return;
        
        Debug.Log($"{hero.unitName} увійшов у хіл зону");
        
        // Додаємо героя до словника з поточним часом
        if (!heroesInZone.ContainsKey(hero))
        {
            heroesInZone.Add(hero, 0f); // 0f означає що можна хілити відразу
            
            // Миттєвий хіл при вході в зону (опціонально)
            if (healToMaxInstantly && CanHealHero(hero))
            {
                HealHeroToMax(hero);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Видаляємо героя зі словника при виході з зони
        Hero hero = other.GetComponent<Hero>();
        if (hero == null) return;
        
        Debug.Log($"{hero.unitName} вийшов з хіл зони");
        
        if (heroesInZone.ContainsKey(hero))
        {
            heroesInZone.Remove(hero);
        }
    }

    private bool CanHealHero(Hero hero)
    {
        // Хілимо тільки якщо у героя не повне хп
        return hero.currentHealth < hero.maxHealth;
    }

    private void HealHero(Hero hero)
    {
        if (hero == null || hero.isDead) return;
        
        int oldHealth = hero.currentHealth;
        int newHealth = Mathf.Min(hero.currentHealth + healAmount, hero.maxHealth);
        int actualHeal = newHealth - oldHealth;
        
        hero.currentHealth = newHealth;
        
        Debug.Log($"Хіл зона відновила {actualHeal} хп для {hero.unitName}. HP: {hero.currentHealth}/{hero.maxHealth}");
    }
    
    private void HealHeroToMax(Hero hero)
    {
        if (hero == null || hero.isDead) return;
        
        int oldHealth = hero.currentHealth;
        hero.currentHealth = hero.maxHealth;
        int actualHeal = hero.maxHealth - oldHealth;
        
        if (actualHeal > 0)
        {
            Debug.Log($"Хіл зона повністю відновила {actualHeal} хп для {hero.unitName}. HP: {hero.currentHealth}/{hero.maxHealth}");
        }
    }

    // Публічні методи для налаштування
    public void SetHealAmount(int amount)
    {
        healAmount = Mathf.Max(1, amount);
    }
    
    public void SetHealInterval(float interval)
    {
        healInterval = Mathf.Max(0.1f, interval);
    }
    
    public void SetInstantHeal(bool instant)
    {
        healToMaxInstantly = instant;
    }
    
    public int GetHeroesInZoneCount()
    {
        return heroesInZone.Count;
    }

    // Візуалізація зони в редакторі
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Напівпрозорий зелений
            
            if (col is BoxCollider2D)
            {
                BoxCollider2D boxCol = col as BoxCollider2D;
                Gizmos.matrix = transform.localToWorldMatrix;
                Vector3 size = new Vector3(boxCol.size.x, boxCol.size.y, 0.1f);
                Vector3 center = new Vector3(boxCol.offset.x, boxCol.offset.y, 0f);
                Gizmos.DrawCube(center, size);
            }
            else if (col is CircleCollider2D)
            {
                CircleCollider2D circleCol = col as CircleCollider2D;
                Gizmos.matrix = transform.localToWorldMatrix;
                Vector3 center = new Vector3(circleCol.offset.x, circleCol.offset.y, 0f);
                Gizmos.DrawSphere(center, circleCol.radius);
            }
        }
        
        // Показуємо іконку хреста над зоною
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.identity;
        Vector3 iconPos = transform.position + Vector3.up * 2f;
        Gizmos.DrawWireCube(iconPos, Vector3.one * 0.5f);
    }
}