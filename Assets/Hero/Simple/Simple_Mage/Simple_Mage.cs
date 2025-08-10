using UnityEngine;

// Маг - дальній боєць з магічними атаками
public class Simple_Mage : RangedHero
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack(Unit target)
    {
        if (target == null || target.isDead) return;

        if (IsInMeleeMode())
        {
            Debug.Log($"{unitName} б'є посохом по {target.unitName}!");
        }
        else
        {
            Debug.Log($"{unitName} кидає магічну кулю в {target.unitName}!");
        }
        
        base.Attack(target);
    }

    protected override void Die()
    {
        Debug.Log($"Маг {unitName} загинув!");
        base.Die();
    }
}