using UnityEngine;
using System.Collections;
// Селянин
public class Villager : Hero
{
    protected override void Start()
    {
        base.Start();
        unitName = "Селянин";
        maxHealth = 80;
        currentHealth = maxHealth;
        damage = 15;
        attackRange = 1.5f;
        attackSpeed = 0.8f;
        moveSpeed = 2.5f;
        critChance = 0.05f; // 5% шанс на крит
        critMultiplier = 1.8f;
        
        // Встановлюємо тег для героя
        gameObject.tag = "Hero";
    }

    protected override void Attack(Unit target)
    {
        Debug.Log($"{unitName} б'є сокирою!");
        base.Attack(target);
    }
}
