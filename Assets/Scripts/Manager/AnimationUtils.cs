using UnityEngine;

// Утилітний клас для централізованого управління анімаціями
public static class AnimationUtils
{
    // Анімації руху
    public static void SetRunning(Unit unit, bool isRunning)
    {
        if (unit.animator != null)
        {
            unit.animator.SetBool("isRunning", isRunning);
        }
    }
    
    public static void SetRunning(Animator animator, bool isRunning)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", isRunning);
        }
    }
    
    // Анімації атаки
    public static void TriggerAttack(Unit unit)
    {
        if (unit.animator != null)
        {
            unit.animator.SetTrigger("isAttacking");
        }
    }
    
    public static void TriggerAttack(Animator animator)
    {
        if (animator != null)
        {
            animator.SetTrigger("isAttacking");
        }
    }
    
    public static void TriggerMeleeAttack(Animator animator)
    {
        if (animator != null)
        {
            animator.SetTrigger("isMeleeAttacking");
        }
    }
    
    public static void TriggerRangedAttack(Animator animator)
    {
        if (animator != null)
        {
            animator.SetTrigger("isRangedAttacking");
        }
    }
    
    // Анімація смерті
    public static void TriggerDeath(Unit unit)
    {
        if (unit.animator != null)
        {
            unit.animator.SetTrigger("isDead");
        }
    }
    
    public static void TriggerDeath(Animator animator)
    {
        if (animator != null)
        {
            animator.SetTrigger("isDead");
        }
    }
    
    // Скидання анімацій
    public static void ResetCombatAnimations(Unit unit)
    {
        if (unit.animator != null)
        {
            unit.animator.SetBool("isRunning", false);
            unit.animator.ResetTrigger("isAttacking");
        }
    }
    
    public static void ResetCombatAnimations(Animator animator)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.ResetTrigger("isAttacking");
        }
    }
    
    // Комплексні анімації
    public static void PlayCombatSequence(Unit unit, bool isRunning, bool isAttacking)
    {
        if (unit.animator != null)
        {
            unit.animator.SetBool("isRunning", isRunning);
            if (isAttacking)
            {
                unit.animator.SetTrigger("isAttacking");
            }
        }
    }
    
    public static void PlayDeathSequence(Unit unit)
    {
        if (unit.animator != null)
        {
            unit.animator.SetBool("isRunning", false);
            unit.animator.ResetTrigger("isAttacking");
            unit.animator.SetTrigger("isDead");
        }
    }
    
    // Перевірка наявності аніматора
    public static bool HasAnimator(Unit unit)
    {
        return unit.animator != null;
    }
    
    public static bool HasAnimator(Animator animator)
    {
        return animator != null;
    }
} 