using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI компонент для відображення таймера перебування героя в зоні
/// </summary>
public class ZoneTimerUI : MonoBehaviour
{
    [Header("UI елементи")]
    public TextMeshProUGUI timerText;
    public Image timerBar;
    public GameObject timerPanel;
    
    [Header("Налаштування")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;
    public float warningThreshold = 0.3f; // 30% часу залишилося
    public float criticalThreshold = 0.1f; // 10% часу залишилося
    
    private Zone currentZone;
    private bool isActive = false;
    
    void Start()
    {
        // Спочатку приховуємо таймер
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        if (isActive && currentZone != null)
        {
            UpdateTimer();
        }
    }
    
    /// <summary>
    /// Оновлює таймер для поточної зони
    /// </summary>
    private void UpdateTimer()
    {
        if (!currentZone.HasHero()) return;
        
        float timeInZone = Time.time - currentZone.GetHeroEnterTime();
        float remainingTime = currentZone.GetHeroStayDuration() - timeInZone;
        float timePercentage = remainingTime / currentZone.GetHeroStayDuration();
        
        // Оновлюємо текст
        if (timerText != null)
        {
            if (remainingTime > 0)
            {
                timerText.text = $"⏰ {remainingTime:F1}с";
            }
            else
            {
                timerText.text = "⏰ Повернення...";
            }
        }
        
        // Оновлюємо прогрес-бар
        if (timerBar != null)
        {
            timerBar.fillAmount = Mathf.Clamp01(timePercentage);
            
            // Змінюємо колір в залежності від часу
            if (timePercentage <= criticalThreshold)
            {
                timerBar.color = criticalColor;
            }
            else if (timePercentage <= warningThreshold)
            {
                timerBar.color = warningColor;
            }
            else
            {
                timerBar.color = normalColor;
            }
        }
        
        // Приховуємо таймер якщо час вийшов
        if (remainingTime <= 0)
        {
            HideTimer();
        }
    }
    
    /// <summary>
    /// Показує таймер для зони
    /// </summary>
    public void ShowTimer(Zone zone)
    {
        if (zone == null) return;
        
        currentZone = zone;
        isActive = true;
        
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
        }
        
        Debug.Log($"⏰ Показуємо таймер для зони {zone.name}");
    }
    
    /// <summary>
    /// Приховує таймер
    /// </summary>
    public void HideTimer()
    {
        isActive = false;
        currentZone = null;
        
        if (timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
        
        if (timerText != null)
        {
            timerText.text = "";
        }
        
        if (timerBar != null)
        {
            timerBar.fillAmount = 0f;
        }
    }
    
    /// <summary>
    /// Публічний метод для зовнішнього виклику
    /// </summary>
    public void ForceUpdate()
    {
        if (isActive && currentZone != null)
        {
            UpdateTimer();
        }
    }
    
    /// <summary>
    /// Перевіряє чи активний таймер
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Отримує поточну зону
    /// </summary>
    public Zone GetCurrentZone()
    {
        return currentZone;
    }
} 