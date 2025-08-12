using UnityEngine;

// Клас для обробки смерті юнітів
public static class DeathHandler
{
    // Подія для повідомлення про смерть героя
    public static System.Action<Unit> OnHeroDeath;
    
    public static void HandleDeath(Unit unit, string customMessage = null)
    {
        if (unit.isDead) return;
        
        unit.isDead = true;
        
        string deathMessage = customMessage ?? $"{unit.unitName} загинув!";
        
        // Перевіряємо чи увімкнені debug логи
        bool shouldLog = CombatSystemManager.Instance == null || CombatSystemManager.Instance.IsDebugLogsEnabled();
        if (shouldLog)
        {
            Debug.Log(deathMessage);
        }
        
        // Скидаємо анімації бою
        AnimationUtils.PlayDeathSequence(unit);
        
        // Повідомляємо про смерть героя
        if (unit.gameObject.CompareTag("Hero"))
        {
            OnHeroDeath?.Invoke(unit);
        }
        
        // Вимікаємо колайдер
        Collider col = unit.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Знищуємо об'єкт через 3 секунди
        Object.Destroy(unit.gameObject, 3f);
    }
}