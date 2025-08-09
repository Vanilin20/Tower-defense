using UnityEngine;

public class HomeBase : Unit 
{

    
    protected override void Start()
    {
        base.Start();
    }
    
    protected override void FindTarget()
    {
        // База не шукає цілі, вона тільки захищається
        currentTarget = null;
    }
    
    protected override void HandleCombat()
    {
        // База не атакує, тільки отримує пошкодження
    }
    
    protected override void Die()
    {
        base.Die();
        // Тут можна додати логіку закінчення гри
        Time.timeScale = 0f; // Зупиняємо гру
    }
    
    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);

    }
    
    // Корисні методи для UI та геймплея
    public float GetBaseHealthPercentage()
    {
        return Mathf.Clamp01((float)currentHealth / (float)maxHealth);
    }
    
    public bool IsCriticallyDamaged()
    {
        return GetBaseHealthPercentage() <= 0.25f;
    }
}