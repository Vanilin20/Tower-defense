using UnityEngine;

// Утилітний клас для логіки бою
public static class CombatUtils
{
    // Розраховує фінальну шкоду з урахуванням критичних ударів
    public static int CalculateDamage(int baseDamage, float critChance, float critMultiplier, out bool isCritical)
    {
        isCritical = Random.Range(0f, 1f) < critChance;
        return isCritical ? Mathf.RoundToInt(baseDamage * critMultiplier) : baseDamage;
    }
    
    // Виконує атаку з логуванням
    public static void PerformAttack(Unit attacker, Unit target, int damage, bool isCritical, string attackType = "атакує")
    {
        if (target == null || target.isDead) return;
        
        // Перевіряємо чи увімкнені debug логи
        bool shouldLog = CombatSystemManager.Instance == null || CombatSystemManager.Instance.IsDebugLogsEnabled();
        
        if (shouldLog)
        {
            if (isCritical)
            {
                Debug.Log($"{attacker.unitName} завдає критичний {attackType} {target.unitName} на {damage} пошкоджень!");
            }
            else
            {
                Debug.Log($"{attacker.unitName} {attackType} {target.unitName} на {damage} пошкоджень!");
            }
        }
        
        AnimationUtils.TriggerAttack(attacker);
        target.TakeDamage(damage);
    }
    
    // Знаходить найближчу ціль
    public static Unit FindClosestTarget(Vector3 position, string targetTag, float searchRadius)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        Unit closestTarget = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (GameObject targetObj in targets)
        {
            Unit target = targetObj.GetComponent<Unit>();
            if (target == null || target.isDead) continue;
            
            float distance = Vector3.Distance(position, target.transform.position);
            if (distance < closestDistance && distance <= searchRadius)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }
        
        return closestTarget;
    }
    
    // Перевіряє чи потрібно переміщуватися на спільну висоту
    public static bool ShouldMoveToCommonHeight(Unit unit, Unit target, bool isStaticUnit = false)
    {
        if (isStaticUnit || target == null) return false;
        
        float heightDiff = Mathf.Abs(unit.transform.position.y - target.transform.position.y);
        return heightDiff > HeightManager.Instance.heightTolerance;
    }
    
    // Виконує рух до цілі з збереженням висоти
    public static void MoveTowardsTarget(Unit unit, Vector3 targetPosition, float moveSpeed)
    {
        Vector3 direction = new Vector3(
            targetPosition.x - unit.transform.position.x, 
            0, 
            targetPosition.z - unit.transform.position.z
        ).normalized;
        
        Vector3 newPosition = unit.transform.position + direction * moveSpeed * Time.deltaTime;
        newPosition.y = unit.transform.position.y; // Зберігаємо поточну висоту
        
        unit.transform.position = newPosition;
        
        // Повертаємо спрайт відповідно до напрямку руху
        SpriteRotationUtils.RotateSpriteByUnit(unit, direction);
        
        AnimationUtils.SetRunning(unit, true);
    }
} 