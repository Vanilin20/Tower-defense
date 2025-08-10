using UnityEngine;

// Селянин - базовий ближній боєць
public class Villager : MeleeHero
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack(Unit target)
    {
        if (target == null || target.isDead) return;

        Debug.Log($"{unitName} б'є сокирою по {target.unitName}!");
        base.Attack(target);
    }

    protected override void Die()
    {
        Debug.Log($"Селянин {unitName} загинув!");
        base.Die();
    }
}