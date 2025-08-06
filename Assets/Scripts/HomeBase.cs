using UnityEngine;

public class HomeBase : Unit
{
    [Header("Характеристики бази")]
    public int maxBaseHealth = 500;
    
    protected override void Start()
    {
        base.Start();
        maxHealth = maxBaseHealth;
        currentHealth = maxHealth;
        unitName = "Домашня База";
        
        // База не рухається
        moveSpeed = 0f;
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
        Debug.Log("ГЕЙМ ОВЕР! База знищена!");
        // Тут можна додати логіку закінчення гри
        Time.timeScale = 0f; // Зупиняємо гру
    }

    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);
        Debug.Log($"База під атакою! Залишилось здоров'я: {currentHealth}/{maxHealth}");
    }
}
