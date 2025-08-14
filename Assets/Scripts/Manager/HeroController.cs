using UnityEngine;
using UnityEngine.UI;

// Контролер для управління героєм через кнопки
public class HeroController : MonoBehaviour
{
    [Header("UI кнопки")]
    public Button returnToSpawnButton;    // Кнопка 1: повернутися до спавну
    public Button returnToZoneButton;     // Кнопка 2: повернутися до зони
    
    [Header("Налаштування")]
    public float stopDistance = 0.1f;     // Відстань зупинки
    
    // Приватні змінні
    private Hero heroComponent;
    private Unit unitComponent;
    private Zone currentZone; // Прив'язана зона (пам'ятається навіть коли герой у спавні)
    private Transform spawnPoint;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private bool isReturningToSpawn = false;
    private float moveSpeed; // Швидкість руху з Unit компонента
    
    // НОВЕ: Система активації UI об'єкта
    [Header("UI об'єкт для активації")]
    public GameObject heroControlPanel; // UI панель з кнопками управління героєм
    
    // Система блокування кнопок
    private bool isReturnToZoneBlocked = false; // Чи заблокована кнопка повернення в зону
    private bool isAutoReturning = false;       // Чи автоматично повертається герой

    
    void Start()
    {
        // Отримуємо компоненти
        heroComponent = GetComponent<Hero>();
        unitComponent = GetComponent<Unit>();
        
        // Отримуємо швидкість руху з Unit компонента
        if (unitComponent != null)
        {
            moveSpeed = unitComponent.moveSpeed;
            Debug.Log($"🏃 Швидкість руху героя встановлена: {moveSpeed}");
        }
        else
        {
            Debug.LogWarning("⚠️ Unit компонент не знайдено, використовуємо стандартну швидкість 5");
            moveSpeed = 5f; // Резервне значення
        }
        
        // Знаходимо точку спавну
        GameObject spawnObject = GameObject.FindGameObjectWithTag("HeroSpawnPoint");
        if (spawnObject != null)
        {
            spawnPoint = spawnObject.transform;
        }
        
        // Налаштовуємо кнопки
        SetupButtons();
        
        // НОВЕ: Спочатку кнопки неактивні - чекаємо кліку на героя
        SetButtonsActive(false);
    }
    
    void Update()
    {
        // Перевіряємо чи змінилася швидкість в Unit
        if (unitComponent != null && Mathf.Abs(moveSpeed - unitComponent.moveSpeed) > 0.01f)
        {
            UpdateMoveSpeed();
        }
        
        if (isMoving)
        {
            MoveToTarget();
        }
    }
    
    private void SetupButtons()
    {
        if (returnToSpawnButton != null)
        {
            returnToSpawnButton.onClick.AddListener(ReturnToSpawn);
        }
        
        if (returnToZoneButton != null)
        {
            returnToZoneButton.onClick.AddListener(ReturnToZone);
        }
    }
    
    // Кнопка 1: Герой біжить до зони спавну
    public void ReturnToSpawn()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("⚠️ Точка спавну не знайдена!");
            return;
        }
        
        Debug.Log("🏃 Герой повертається до точки спавну");
        
        // НОВЕ: Деактивуємо UI панель після натискання кнопки
        DeactivateControlPanel();
        
        // Зупиняємо поточну діяльність
        StopCurrentActivity();
        
        // Встановлюємо ціль - точка спавну
        targetPosition = spawnPoint.position;
        isReturningToSpawn = true;
        isMoving = true;
        
        // Деактивуємо кнопки під час руху
        SetButtonsActive(false);
        
        // Повертаємо героя вліво (до спавну)
        RotateHeroTowards(spawnPoint.position);
    }
    
    // Кнопка 2: Герой біжить до своєї зони
    public void ReturnToZone()
    {
        // НОВЕ: Перевіряємо чи не заблокована кнопка
        if (isReturnToZoneBlocked)
        {
            Debug.LogWarning("🚫 Кнопка повернення в зону заблокована!");
            return;
        }
        
        if (currentZone == null)
        {
            Debug.LogWarning("⚠️ Герой не прив'язаний до зони!");
            return;
        }
        
        Debug.Log("🏃 Герой повертається до своєї зони");
        
        // НОВЕ: Деактивуємо UI панель після натискання кнопки
        DeactivateControlPanel();
        
        // Зупиняємо поточну діяльність
        StopCurrentActivity();
        
        // Встановлюємо ціль - поточна зона
        targetPosition = currentZone.transform.position;
        isReturningToSpawn = false;
        isMoving = true;
        
        // Деактивуємо кнопки під час руху
        SetButtonsActive(false);
        
        // Повертаємо героя до зони
        RotateHeroTowards(currentZone.transform.position);
    }
    
    private void MoveToTarget()
    {
        if (targetPosition == null) return;
        
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance <= stopDistance)
        {
            OnReachedTarget();
            return;
        }
        
        // Рухаємося до цілі
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Повертаємо спрайт в напрямку руху
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Вліво
        }
        else if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);  // Вправо
        }
    }
    
    private void OnReachedTarget()
    {
        isMoving = false;
        
        if (isReturningToSpawn)
        {
            OnReachedSpawn();
        }
        else
        {
            OnReachedZone();
        }
    }
    
    private void OnReachedSpawn()
    {
        Debug.Log("✅ Герой досягнув точки спавну");
        
        // Скидаємо стан автоматичного повернення
        isAutoReturning = false;
        
        // Звільняємо зону, але НЕ видаляємо героя і НЕ очищаємо прив'язку
        if (currentZone != null)
        {
            currentZone.FreeZoneKeepHero();
        }
        
        // Запускаємо перезарядку карти, пов'язаної з цим героєм
        StartCardCooldown();
        
        // Розблоковуємо кнопку повернення в зону
        UnblockReturnToZone();
        
        // НОВЕ: Деактивуємо кнопки після завершення дії
        SetButtonsActive(false);
        
        // Зупиняємо всі дії
        StopCurrentActivity();
    }
    
    private void OnReachedZone()
    {
        Debug.Log("✅ Герой досягнув своєї зони");
        
        // Сповіщаємо зону, що герой повернувся (оновлюємо прив'язку)
        if (currentZone != null)
        {
            currentZone.AssignHero(gameObject);
        }
        
        // Герой може знову битися
        if (unitComponent != null)
        {
            unitComponent.enabled = true;
        }
        
        // НОВЕ: Деактивуємо кнопки після завершення дії
        SetButtonsActive(false);
        
        // Зупиняємо рух
        StopCurrentActivity();
    }
    
    private void StopCurrentActivity()
    {
        // Зупиняємо рух
        isMoving = false;
        
        // Зупиняємо атаки
        if (unitComponent != null)
        {
            unitComponent.currentTarget = null;
        }
    }
    
    private void RotateHeroTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Вліво
        }
        else if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);  // Вправо
        }
    }
    
    // Викликається коли герой прив'язується до зони
    public void SetCurrentZone(Zone zone)
    {
        currentZone = zone;
        Debug.Log($"🔗 Герой {gameObject.name} прив'язаний до зони {zone.name}");
        
        // НОВЕ: Кнопки активуються тільки якщо герой вибраний гравцем
        if (IsHeroSelected())
        {
            SetButtonsActive(true);
        }
    }
    
    // НОВИЙ МЕТОД: Активує кнопки після кліку на героя
    public void ActivateButtonsOnClick()
    {
        Debug.Log($"🎯 Герой {gameObject.name} вибраний гравцем - активую кнопки");
        SetButtonsActive(true);
    }
    
    // НОВИЙ МЕТОД: Перевіряє чи герой вибраний гравцем
    private bool IsHeroSelected()
    {
        return GameManager.GetSelectedHero() == gameObject;
    }
    
    // Викликається коли герой звільняється з зони
    public void ClearCurrentZone()
    {
        currentZone = null;
        Debug.Log($"🔓 Герой {gameObject.name} звільнений з зони");
        
        // Деактивуємо кнопки
        SetButtonsActive(false);
    }
    
    private void SetButtonsActive(bool active)
    {
        if (returnToSpawnButton != null)
        {
            returnToSpawnButton.interactable = active;
        }
        
        if (returnToZoneButton != null)
        {
            // Кнопка повернення в зону активна тільки якщо не заблокована
            returnToZoneButton.interactable = active && !isReturnToZoneBlocked;
        }
    }
    
    // Публічні методи для зовнішнього виклику
    public bool IsMoving()
    {
        return isMoving;
    }
    
    public Zone GetCurrentZone()
    {
        return currentZone;
    }
    
    public bool HasZone()
    {
        return currentZone != null;
    }
    
    // Метод для оновлення швидкості руху (викликається коли змінюється в Unit)
    public void UpdateMoveSpeed()
    {
        if (unitComponent != null)
        {
            moveSpeed = unitComponent.moveSpeed;
            Debug.Log($"🏃 Швидкість руху оновлена: {moveSpeed}");
        }
    }
    
    // Запускає перезарядку карти, пов'язаної з цим героєм
    private void StartCardCooldown()
    {
        // Знаходимо всі карти і шукаємо ту, що пов'язана з цим героєм
        Card[] allCards = FindObjectsByType<Card>(FindObjectsSortMode.None);
        foreach (Card card in allCards)
        {
            if (card.GetLinkedHero() == gameObject)
            {
                card.StartCooldown();
                Debug.Log($"⏳ Запущено перезарядку карти {card.gameObject.name} для героя {gameObject.name}");
                break;
            }
        }
    }
    
    // Публічний геттер для швидкості
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    
    // НОВІ МЕТОДИ ДЛЯ АВТОМАТИЧНОГО ПОВЕРНЕННЯ
    
    /// <summary>
    /// Блокує кнопку повернення в зону
    /// </summary>
    public void BlockReturnToZone()
    {
        isReturnToZoneBlocked = true;
        Debug.Log($"🚫 Кнопка повернення в зону заблокована для героя {gameObject.name}");
        
        // Оновлюємо стан кнопок
        UpdateButtonsState();
    }
    
    /// <summary>
    /// Розблоковує кнопку повернення в зону
    /// </summary>
    public void UnblockReturnToZone()
    {
        isReturnToZoneBlocked = false;
        Debug.Log($"✅ Кнопка повернення в зону розблокована для героя {gameObject.name}");
        
        // Оновлюємо стан кнопок
        UpdateButtonsState();
    }
    
    /// <summary>
    /// Автоматично повертає героя до спавну
    /// </summary>
    public void AutoReturnToSpawn()
    {
        if (isAutoReturning) return;
        
        isAutoReturning = true;
        Debug.Log($"🔄 Герой {gameObject.name} автоматично повертається до спавну");
        
        // НОВЕ: Деактивуємо UI панель при автоматичному поверненні
        DeactivateControlPanel();
        
        // Зупиняємо поточну діяльність
        StopCurrentActivity();
        
        // Встановлюємо ціль - точка спавну
        if (spawnPoint != null)
        {
            targetPosition = spawnPoint.position;
            isReturningToSpawn = true;
            isMoving = true;
            
            // Деактивуємо кнопки під час руху
            SetButtonsActive(false);
            
            // Повертаємо героя вліво (до спавну)
            RotateHeroTowards(spawnPoint.position);
        }
        else
        {
            Debug.LogWarning("⚠️ Точка спавну не знайдена для автоматичного повернення!");
            isAutoReturning = false;
        }
    }
    
    /// <summary>
    /// Оновлює стан кнопок з урахуванням блокування
    /// </summary>
    private void UpdateButtonsState()
    {
        if (returnToZoneButton != null)
        {
            returnToZoneButton.interactable = !isReturnToZoneBlocked && HasZone();
        }
    }
    
    /// <summary>
    /// Перевіряє чи заблокована кнопка повернення в зону
    /// </summary>
    public bool IsReturnToZoneBlocked()
    {
        return isReturnToZoneBlocked;
    }
    
    /// <summary>
    /// Перевіряє чи автоматично повертається герой
    /// </summary>
    public bool IsAutoReturning()
    {
        return isAutoReturning;
    }
    
    // НОВІ МЕТОДИ ДЛЯ UI ПАНЕЛІ
    
    /// <summary>
    /// Активує UI панель після кліку на героя
    /// </summary>
    public void ActivateControlPanel()
    {
        if (heroControlPanel != null)
        {
            heroControlPanel.SetActive(true);
            Debug.Log($"🎯 UI панель активована для героя {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Деактивує UI панель після натискання кнопки
    /// </summary>
    public void DeactivateControlPanel()
    {
        if (heroControlPanel != null)
        {
            heroControlPanel.SetActive(false);
            Debug.Log($"🚫 UI панель деактивована для героя {gameObject.name}");
        }
    }
    
    void OnDestroy()
    {
        // Очищаємо підписки на кнопки
        if (returnToSpawnButton != null)
        {
            returnToSpawnButton.onClick.RemoveAllListeners();
        }
        
        if (returnToZoneButton != null)
        {
            returnToZoneButton.onClick.RemoveAllListeners();
        }
    }
} 