using UnityEngine;

public abstract class Hero : Unit
{
    [Header("Характеристики героя")]
    [SerializeField] protected float enemySearchRadius = 5f;
    
    private bool isInCombat = false;
    
    protected override void Start()
    {
        base.Start();
        gameObject.tag = "Hero"; // Встановлюємо тег для героя
    }

// Енум для типів героїв (опціонально)
public enum HeroType
{
    Melee,
    Ranged,
}
    
    protected override void Update()
    {
        if (isDead) return;
        
        // Використовуємо базову логіку з системою висот
        base.Update();
        
        // Оновлюємо стан бою
        isInCombat = (currentTarget != null && !currentTarget.isDead);
    }
    
    protected override void FindTarget()
    {
        currentTarget = CombatUtils.FindClosestTarget(transform.position, "Enemy", enemySearchRadius);
    }
    
    // Перевизначаємо для 2D повороту героя
    protected override void MoveTowards(Vector3 targetPosition)
    {
        // Спочатку викликаємо базову логіку (яка вже враховує висоти та поворот спрайта)
        base.MoveTowards(targetPosition);
    }
    
    // Перевизначаємо логіку переходу на спільну висоту для героїв
    protected override bool ShouldMoveToCommonHeight()
    {
        if (currentTarget == null) return false;
        
        if (!CombatUtils.ShouldMoveToCommonHeight(this, currentTarget)) return false;
        
        float horizontalDistance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(currentTarget.transform.position.x, 0, currentTarget.transform.position.z)
        );

        // Герой переходить на висоту ворога якщо той в радіусі пошуку
        return horizontalDistance <= enemySearchRadius;
    }
    
    protected override void Die()
    {
        base.Die();
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Показуємо радіус пошуку ворогів
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemySearchRadius);
    }
    
    // Публічні методи
    public bool IsInCombat()
    {
        return isInCombat;
    }
    
    public float GetHealthPercentage()
    {
        return Mathf.Clamp01((float)currentHealth / (float)maxHealth);
    }
    
    // Метод для примусового переходу на певну висоту (для геймплейних механік)
    public void ForceHeightChange(float newHeight)
    {
        MoveToHeight(newHeight);
    }
    
    // Віртуальні методи для спеціальних здібностей (можуть перевизначатися в підкласах)
    public virtual void UseSpecialAbility()
    {
        Debug.Log($"{unitName} використовує спеціальну здібність!");
    }
    
    public virtual bool CanUseSpecialAbility()
    {
        return !isDead && !isRepositioning;
    }
}