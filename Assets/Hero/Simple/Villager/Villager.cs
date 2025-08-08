using UnityEngine;
using System.Collections;
// Селянин
public class Villager : Hero
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack(Unit target)
    {
        Debug.Log($"{unitName} б'є сокирою!");
        base.Attack(target);
    }
}
