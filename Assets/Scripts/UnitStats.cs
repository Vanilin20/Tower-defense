using UnityEngine;

[CreateAssetMenu(menuName = "Units/Unit Stats")]
public class UnitStats : ScriptableObject
{
    public string unitName;
    public float moveSpeed;
    public float attackRange;
    public float attackCooldown;
    public int damage;
    public int maxHealth;
    public bool isSplashDamage;      // 💥 нове поле
    public float splashRadius = 1f;  // радіус для сплеш-атаки
}
