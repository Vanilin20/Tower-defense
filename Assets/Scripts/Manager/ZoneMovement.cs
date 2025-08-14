using UnityEngine;

// Компонент для керування рухом героя до зони
public class ZoneMovement : MonoBehaviour
{
    private Vector3 targetPosition;
    private Zone targetZone;
    private bool hasReachedZone = false;
    private float moveSpeed; // Швидкість руху до зони (буде взята з Unit)
    private float stopDistance = 0.1f; // Відстань на якій герой зупиняється
    
    // Компоненти для блокування
    private Unit unitComponent;
    private Hero heroComponent;
    private bool wasUnitEnabled = true;
    private bool wasHeroEnabled = true;
    
    // Стан руху
    private bool isInCombat = false;
    private bool isMovingToZone = true;
    
    // Налаштування пріоритету
    [Header("Налаштування руху")]
    public bool allowCombatDuringMovement = true; // Дозволити бій під час руху
    public float combatResumeDelay = 1f; // Затримка перед поверненням до руху після бою
    private float combatEndTime = 0f;

    public void Initialize(Vector3 zonePosition, Zone zone)
    {
        targetPosition = zonePosition;
        targetZone = zone;
        hasReachedZone = false;

        // Отримуємо компоненти
        unitComponent = GetComponent<Unit>();
        heroComponent = GetComponent<Hero>();
        
        if (unitComponent != null)
        {
            moveSpeed = unitComponent.moveSpeed;
            // Зберігаємо поточний стан Unit компонента
            wasUnitEnabled = unitComponent.enabled;
        }
        else
        {
            moveSpeed = 5f; // Резервне значення, якщо немає Unit компонента
            Debug.LogWarning($"⚠️ {gameObject.name} не має Unit компонента, використовуємо стандартну швидкість");
        }

        // Налаштовуємо поведінку під час руху до зони
        SetupCombatDuringMovement();

        Debug.Log($"🎯 {gameObject.name} починає рух до зони на позиції {zonePosition}");
    }

    void Update()
    {
        if (hasReachedZone || targetZone == null)
            return;

        // Обчислюємо відстань до цільової позиції
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // Перевіряємо чи досягли зони
        if (distanceToTarget <= stopDistance)
        {
            OnReachedZone();
            return;
        }

        // Перевіряємо стан бою
        CheckCombatState();

        // Якщо не в бою і рухаємося до зони - рухаємося
        if (!isInCombat && isMovingToZone)
        {
            MoveTowardsZone();
        }
    }

    private void CheckCombatState()
    {
        if (!allowCombatDuringMovement) return;
        
        // Перевіряємо чи герой в бою
        bool currentlyInCombat = false;
        if (unitComponent != null)
        {
            currentlyInCombat = (unitComponent.currentTarget != null && !unitComponent.currentTarget.isDead);
        }
        
        // Якщо вступили в бій
        if (currentlyInCombat && !isInCombat)
        {
            EnterCombat();
        }
        // Якщо вийшли з бою
        else if (!currentlyInCombat && isInCombat)
        {
            ExitCombat();
        }
        
        // Перевіряємо чи можна повернутися до руху після бою
        if (!isInCombat && !isMovingToZone && Time.time >= combatEndTime)
        {
            ResumeMovementToZone();
        }
    }
    
    private void EnterCombat()
    {
        isInCombat = true;
        isMovingToZone = false;
        Debug.Log($"⚔️ {gameObject.name} вступає в бій під час руху до зони");
        
        // Зупиняємо анімацію бігу
        if (unitComponent != null)
        {
            AnimationUtils.SetRunning(unitComponent, false);
        }
    }
    
    private void ExitCombat()
    {
        isInCombat = false;
        combatEndTime = Time.time + combatResumeDelay;
        Debug.Log($"🛡️ {gameObject.name} вийшов з бою, повертається до руху через {combatResumeDelay}с");
    }
    
    private void ResumeMovementToZone()
    {
        isMovingToZone = true;
        Debug.Log($"🎯 {gameObject.name} повертається до руху до зони");
    }

    private void SetupCombatDuringMovement()
    {
        // Зберігаємо поточний стан компонентів
        if (unitComponent != null)
        {
            wasUnitEnabled = unitComponent.enabled;
        }
        
        if (heroComponent != null)
        {
            wasHeroEnabled = heroComponent.enabled;
        }
        
        Debug.Log($"🎯 {gameObject.name} - налаштовуємо рух до зони з можливістю бою");
    }

    private void MoveTowardsZone()
    {
        // Обчислюємо напрямок руху
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Рухаємося до цілі
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Повертаємо героя в сторону руху (для 2D)
        SpriteRotationUtils.RotateSpriteByTag(transform, direction);
        
        // Встановлюємо анімацію бігу
        if (unitComponent != null)
        {
            AnimationUtils.SetRunning(unitComponent, true);
        }
    }

    private void OnReachedZone()
    {
        hasReachedZone = true;

        // Зупиняємо анімацію бігу
        if (unitComponent != null)
        {
            AnimationUtils.SetRunning(unitComponent, false);
        }

        // Повідомляємо зону, що герой досягнув її
        if (targetZone != null)
        {
            targetZone.OnHeroReachedZone(gameObject);
        }

        // Сповіщаємо HeroController про досягнення зони
        HeroController heroController = GetComponent<HeroController>();
        if (heroController != null)
        {
            // HeroController тепер знає, що герой досягнув зони
            // і може активувати кнопки
        }

        Debug.Log($"🛡️ {gameObject.name} готовий до бою в зоні!");

        // Видаляємо цей компонент, оскільки рух завершено
        Destroy(this);
    }

    // Метод для зміни швидкості руху (можна викликати ззовні)
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // Метод для зміни відстані зупинки
    public void SetStopDistance(float distance)
    {
        stopDistance = distance;
    }
    
    // Метод для примусового зупинення руху (наприклад, якщо герой атакований)
    public void StopMovement()
    {
        if (!hasReachedZone)
        {
            Debug.Log($"🛑 {gameObject.name} - примусово зупиняємо рух до зони");
            Destroy(this);
        }
    }
    
    // Метод для налаштування поведінки з ворогами
    public void SetCombatBehavior(bool allowCombat, float resumeDelay)
    {
        allowCombatDuringMovement = allowCombat;
        combatResumeDelay = resumeDelay;
    }

    // Візуалізація в Scene View
    void OnDrawGizmos()
    {
        if (targetZone != null)
        {
            // Малюємо лінію до цільової позиції
            Gizmos.color = isInCombat ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
            
            // Малюємо сферу в цільовій позиції
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetPosition, stopDistance);
            
            // Показуємо стан
            if (isInCombat)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
            }
            else if (!isMovingToZone)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.3f);
            }
        }
    }
    
    // Захист від знищення об'єкта
    void OnDestroy()
    {
        // Логуємо знищення компонента
        if (!hasReachedZone)
        {
            Debug.Log($"🗑️ {gameObject.name} - ZoneMovement знищений до досягнення зони");
        }
    }
}