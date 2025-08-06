using UnityEngine;
using System.Collections;


// Простий маг
public class Simple_Mage : Hero
{
    protected override void Start()
    {
        base.Start();
        unitName = "Простий Маг";
        maxHealth = 60;
        currentHealth = maxHealth;
        damage = 30;
        attackRange = 4f;
        attackSpeed = 0.6f;
        moveSpeed = 2f;
        critChance = 0.15f; // 15% шанс на крит
        critMultiplier = 2.2f;
        
        // Встановлюємо тег для героя
        gameObject.tag = "Hero";
    }

    protected override void Attack(Unit target)
    {
        Debug.Log($"{unitName} кидає магічну кулю!");
        base.Attack(target);
    }
}

