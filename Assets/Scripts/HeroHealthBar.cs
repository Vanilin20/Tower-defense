using UnityEngine;
using UnityEngine.UI;

// Компонент HP бару для героїв (ручне налаштування)
public class HeroHealthBar : MonoBehaviour
{
    [Header("HP Bar компоненти")]
    [SerializeField] private Image healthFillImage; // Зображення заповнення HP (перетягніть з дочірніх об'єктів)
    [SerializeField] private Image backgroundImage; // Фонове зображення (опціонально)
    
    private Hero heroUnit; // Посилання на героя
    private int lastKnownHealth = -1; // Для відстеження змін здоров'я
    
    void Start()
    {
        // Отримуємо компонент Hero з батьківського об'єкта
        heroUnit = GetComponentInParent<Hero>();
        if (heroUnit == null)
        {
            Debug.LogError("HeroHealthBar не знайшов компонент Hero у батьківських об'єктах!");
            return;
        }
        
        // Перевіряємо чи призначено заповнення HP
        if (healthFillImage == null)
        {
            Debug.LogError("Не призначено healthFillImage в HeroHealthBar!");
            return;
        }
        
        // Початкове оновлення HP бару
        lastKnownHealth = heroUnit.currentHealth;
        UpdateHealthBar();
    }
    
// Замість Update() використовуйте LateUpdate() в HeroHealthBar
    void LateUpdate()
    {
        if (heroUnit == null || healthFillImage == null) return;
        
        // Оновлюємо HP бар тільки якщо здоров'я змінилось
        if (lastKnownHealth != heroUnit.currentHealth)
        {
            lastKnownHealth = heroUnit.currentHealth;
            UpdateHealthBar();
        }
    }
    
    // Також можна викликати цей метод безпосередньо з Unit.TakeDamage()
    public void ForceUpdateHealthBar()
    {
        UpdateHealthBar();
    }
    
    private void UpdateHealthBar()
    {
        // Розраховуємо відсоток здоров'я з обмеженням від 0 до 1
        float healthPercentage = Mathf.Clamp01((float)heroUnit.currentHealth / (float)heroUnit.maxHealth);

        // Змінюємо заповнення смужки HP
        healthFillImage.fillAmount = healthPercentage;
        Debug.Log($"Health %: {healthPercentage} (HP: {heroUnit.currentHealth}/{heroUnit.maxHealth})");

        // Змінюємо колір залежно від HP
        if (healthPercentage > 0.6f)
            healthFillImage.color = Color.green;
        else if (healthPercentage > 0.3f)
            healthFillImage.color = Color.yellow;
        else
            healthFillImage.color = Color.red;
    }

    
    // Публічні методи для налаштування
    public void SetHealthBarColors(Color backgroundColor, Color healthColor)
    {
        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;
        if (healthFillImage != null)
            healthFillImage.color = healthColor;
    }
    
    public void SetHealthFillImage(Image fillImage)
    {
        healthFillImage = fillImage;
    }
    
    public void SetBackgroundImage(Image bgImage)
    {
        backgroundImage = bgImage;
    }
}