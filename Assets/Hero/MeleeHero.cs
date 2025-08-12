using UnityEngine;

public class MeleeHero : Hero
{
    [Header("Характеристики ближнього бою")]
    [SerializeField] private float meleeComboChance = 0.3f; // Шанс комбо-атаки
    [SerializeField] private int comboHits = 2; // Кількість ударів в комбо
    [SerializeField] private float comboDelay = 0.3f; // Затримка між ударами в комбо
    
    private bool isPerformingCombo = false;
    private int currentComboHit = 0;
    private float lastComboHitTime = 0f;

    protected override void Start()
    {
        base.Start();
        // Встановлюємо стандартні значення для ближнього бою
        if (attackRange == 2f) attackRange = 1.5f; // Трохи менший радіус атаки
    }

    protected override void Update()
    {
        if (isDead) return;

        // Обробляємо комбо-атаку
        if (isPerformingCombo)
        {
            HandleComboAttack();
            return;
        }

        base.Update();
    }

    protected override void Attack(Unit target)
    {
        if (target == null || target.isDead) return;

        // Перевіряємо чи почати комбо-атаку
        bool shouldCombo = Random.Range(0f, 1f) < meleeComboChance && !isPerformingCombo;
        
        if (shouldCombo)
        {
            StartComboAttack(target);
        }
        else
        {
            PerformSingleAttack(target);
        }
    }

    private void PerformSingleAttack(Unit target)
    {
        int finalDamage = CombatUtils.CalculateDamage(damage, critChance, critMultiplier, out bool isCritical);
        CombatUtils.PerformAttack(this, target, finalDamage, isCritical);
    }

    private void StartComboAttack(Unit target)
    {
        isPerformingCombo = true;
        currentComboHit = 0;
        currentTarget = target; // Зберігаємо ціль для комбо
        lastComboHitTime = Time.time;
        
        Debug.Log($"{unitName} починає комбо-атаку на {target.unitName}!");
        PerformComboHit();
    }

    private void HandleComboAttack()
    {
        if (currentTarget == null || currentTarget.isDead)
        {
            EndComboAttack();
            return;
        }

        // Перевіряємо чи час для наступного удару
        if (Time.time - lastComboHitTime >= comboDelay)
        {
            PerformComboHit();
        }
    }

    private void PerformComboHit()
    {
        if (currentTarget == null || currentTarget.isDead)
        {
            EndComboAttack();
            return;
        }

        currentComboHit++;
        lastComboHitTime = Time.time;

        // Розраховуємо шкоду (трохи менша за звичайну для балансу)
        int comboDamage = Mathf.RoundToInt(damage * 0.8f);
        bool isCritical = Random.Range(0f, 1f) < critChance;
        int finalDamage = isCritical ? Mathf.RoundToInt(comboDamage * critMultiplier) : comboDamage;

        Debug.Log($"{unitName} комбо удар {currentComboHit}/{comboHits} - {finalDamage} пошкоджень!");

        AnimationUtils.TriggerAttack(this);

        currentTarget.TakeDamage(finalDamage);

        // Перевіряємо чи закінчили комбо
        if (currentComboHit >= comboHits)
        {
            EndComboAttack();
        }
    }

    private void EndComboAttack()
    {
        isPerformingCombo = false;
        currentComboHit = 0;
        Debug.Log($"{unitName} закінчив комбо-атаку!");
    }

    // Переміщення в ближньому бою - більш агресивне
    protected override void MoveTowards(Vector3 targetPosition)
    {
        base.MoveTowards(targetPosition);
        
        // Додаткова логіка для ближнього бою - швидше наближення при низькому HP
        if (GetHealthPercentage() < 0.3f && currentTarget != null)
        {
            // Берсерк режим - швидше рухається при низькому HP
            Vector3 direction = new Vector3(
                targetPosition.x - transform.position.x, 
                0, 
                targetPosition.z - transform.position.z
            ).normalized;
            
            Vector3 berserkMovement = direction * moveSpeed * 0.5f * Time.deltaTime;
            berserkMovement.y = 0; // Не змінюємо висоту
            
            transform.position += berserkMovement;
        }
    }

    protected override void Die()
    {
        // Якщо помираємо під час комбо - завершуємо його
        if (isPerformingCombo)
        {
            EndComboAttack();
        }
        
        DeathHandler.HandleDeath(this);
    }

    // Візуалізація для налагодження
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Показуємо статус комбо
        if (isPerformingCombo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        }
    }

    // Публічні методи
    public bool IsPerformingCombo()
    {
        return isPerformingCombo;
    }

    public int GetCurrentComboHit()
    {
        return currentComboHit;
    }
}