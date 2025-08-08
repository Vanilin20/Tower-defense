using UnityEngine;
using System.Collections;

// Простий бандит
public class Bandit_Simple : Enemy
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack(Unit target)
    {
        Debug.Log($"{unitName} атакує ножем!");
        base.Attack(target);
    }
}

// Менеджер для керування юнітами