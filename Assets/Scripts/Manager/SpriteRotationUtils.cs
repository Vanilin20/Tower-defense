using UnityEngine;

// Утилітний клас для управління поворотами спрайтів юнітів
public static class SpriteRotationUtils
{
    // Повертає спрайт героя (за замовчуванням дивиться вправо)
    public static void RotateHeroSprite(Transform transform, Vector3 direction)
    {
        if (transform == null) return;
        
        // Перевіряємо чи є значущий рух по X
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            if (direction.x > 0)
            {
                // Рухаємося вправо - спрайт дивиться вправо
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x < 0)
            {
                // Рухаємося вліво - перевертаємо спрайт
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
    
    // Повертає спрайт ворога (за замовчуванням дивиться вліво)
    public static void RotateEnemySprite(Transform transform, Vector3 direction)
    {
        if (transform == null) return;
        
        // Перевіряємо чи є значущий рух по X
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            if (direction.x > 0)
            {
                // Рухаємося вправо - перевертаємо спрайт (ворог дивиться вліво)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x < 0)
            {
                // Рухаємося вліво - спрайт дивиться вліво
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
    
    // Універсальний метод для повороту спрайта
    public static void RotateSprite(Transform transform, Vector3 direction, bool isHero = true)
    {
        if (isHero)
        {
            RotateHeroSprite(transform, direction);
        }
        else
        {
            RotateEnemySprite(transform, direction);
        }
    }
    
    // Повертає спрайт на основі тегу об'єкта
    public static void RotateSpriteByTag(Transform transform, Vector3 direction)
    {
        if (transform == null) return;
        
        bool isHero = transform.CompareTag("Hero");
        RotateSprite(transform, direction, isHero);
    }
    
    // Повертає спрайт на основі компонента Unit
    public static void RotateSpriteByUnit(Unit unit, Vector3 direction)
    {
        if (unit == null) return;
        
        bool isHero = unit.gameObject.CompareTag("Hero");
        RotateSprite(unit.transform, direction, isHero);
    }
    
    // Встановлює початковий напрямок спрайта
    public static void SetInitialDirection(Transform transform, bool isHero = true)
    {
        if (transform == null) return;
        
        if (isHero)
        {
            // Герой за замовчуванням дивиться вправо
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            // Ворог за замовчуванням дивиться вліво
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    
    // Отримує поточний напрямок спрайта
    public static bool IsFacingRight(Transform transform)
    {
        if (transform == null) return true;
        return transform.localScale.x > 0;
    }
    
    // Встановлює конкретний напрямок спрайта
    public static void SetFacingDirection(Transform transform, bool faceRight, bool isHero = true)
    {
        if (transform == null) return;
        
        if (isHero)
        {
            // Для героя: true = вправо, false = вліво
            if (faceRight)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            // Для ворога: true = вліво, false = вправо (інвертовано)
            if (faceRight)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
} 