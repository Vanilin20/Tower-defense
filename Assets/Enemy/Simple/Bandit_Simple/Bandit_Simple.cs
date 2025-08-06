using UnityEngine;
using System.Collections;

// Простий бандит
public class Bandit_Simple : Enemy
{
    protected override void Start()
    {
        base.Start();
        unitName = "Простий Бандит";
        maxHealth = 70;
        currentHealth = maxHealth;
        damage = 18;
        attackRange = 2f;
        attackSpeed = 1.2f;
        moveSpeed = 3.5f;
        aggroRange = 6f;
        critChance = 0.12f; // 12% шанс на крит
        critMultiplier = 2f;
        
        // Встановлюємо тег для ворога
        gameObject.tag = "Enemy";
    }

    protected override void Attack(Unit target)
    {
        Debug.Log($"{unitName} атакує ножем!");
        base.Attack(target);
    }
}

// Менеджер для керування юнітами