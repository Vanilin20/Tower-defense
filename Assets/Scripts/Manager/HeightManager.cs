using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Менеджер висот - синглтон для управління системою висот
public class HeightManager : MonoBehaviour
{
    public static HeightManager Instance;
    
    [Header("Налаштування висот")]
    public List<float> availableHeights = new List<float> { 0f, 2f, 4f, 6f, 8f }; // Основні висоти
    public float heightTolerance = 0.1f; // Толерантність для визначення чи юніт на висоті
    public int maxUnitsPerHeight = 10; // Максимум юнітів на одній висоті для оптимізації
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Знаходить найближчу доступну висоту до заданої позиції
    public float GetNearestHeight(float currentY)
    {
        return availableHeights.OrderBy(h => Mathf.Abs(h - currentY)).First();
    }
    
    // Знаходить найближчу спільну висоту для двох юнітів
    public float FindNearestCommonHeight(Vector3 pos1, Vector3 pos2)
    {
        float midY = (pos1.y + pos2.y) / 2f;
        return GetNearestHeight(midY);
    }
    
    // Перевіряє чи юніт знаходиться на одній з доступних висот
    public bool IsOnValidHeight(float yPosition)
    {
        return availableHeights.Any(h => Mathf.Abs(h - yPosition) <= heightTolerance);
    }
    
    // Отримує всі доступні висоти
    public List<float> GetAvailableHeights()
    {
        return new List<float>(availableHeights);
    }
    
    // Підраховує кількість живих юнітів на певній висоті
    public int GetUnitsCountOnHeight(float height, string tag = null)
    {
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        int count = 0;
        
        foreach (Unit unit in allUnits)
        {
            if (unit.isDead) continue;
            if (tag != null && !unit.gameObject.CompareTag(tag)) continue;
            if (Mathf.Abs(unit.transform.position.y - height) <= heightTolerance)
            {
                count++;
            }
        }
        
        return count;
    }
    
    // Знаходить найменш завантажену висоту поблизу цільової
    public float GetOptimalHeightNear(float targetHeight, string unitTag = null)
    {
        var heightsByDistance = availableHeights
            .OrderBy(h => Mathf.Abs(h - targetHeight))
            .ToList();
        
        // Знаходимо висоту з найменшою кількістю юнітів
        float bestHeight = heightsByDistance.First();
        int minUnits = GetUnitsCountOnHeight(bestHeight, unitTag);
        
        foreach (float height in heightsByDistance)
        {
            int unitsOnHeight = GetUnitsCountOnHeight(height, unitTag);
            if (unitsOnHeight < minUnits)
            {
                minUnits = unitsOnHeight;
                bestHeight = height;
            }
            
            // Якщо знайшли пусту висоту - використовуємо її
            if (unitsOnHeight == 0) break;
        }
        
        return bestHeight;
    }
}

