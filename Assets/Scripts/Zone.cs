using UnityEngine;
using UnityEngine.EventSystems;

// Компонент для зони (працює з новими версіями Unity)
[RequireComponent(typeof(Collider2D))] // Гарантує, що зона має 2D колайдер
public class Zone : MonoBehaviour, IPointerClickHandler
{
    [Header("Зона налаштування")]
    public static Transform globalSpawnPoint; // Глобальна точка спавну для всіх героїв
    public bool hasHero = false;              // Чи є герой у зоні
    public GameObject currentHero;            // Поточний герой у зоні
    
    // НОВЕ: Система автоматичного повернення
    [Header("Автоматичне повернення")]
    public float heroStayDuration = 30f;     // Скільки секунд герой може бути в зоні
    public bool enableAutoReturn = true;     // Увімкнути/вимкнути автоматичне повернення
    private float heroEnterTime = 0f;        // Час входу героя в зону
    private bool isHeroReturning = false;    // Чи повертається герой до спавну

    void Start()
    {
        // Знаходимо глобальну точку спавну, якщо вона ще не встановлена
        if (globalSpawnPoint == null)
        {
            GameObject spawnObject = GameObject.FindGameObjectWithTag("HeroSpawnPoint");
            if (spawnObject != null)
            {
                globalSpawnPoint = spawnObject.transform;
                Debug.Log("🎯 Знайдено глобальну точку спавну: " + spawnObject.name);
            }
            else
            {
                Debug.LogWarning("⚠️ Не знайдено об'єкт з тегом 'HeroSpawnPoint'! Створіть GameObject з цим тегом для точки спавну героїв.");
            }
        }
        
        // Підписуємося на подію смерті героя
        DeathHandler.OnHeroDeath += OnHeroDeath;
    }
    
    void Update()
    {
        // НОВЕ: Перевіряємо час перебування героя в зоні
        CheckHeroStayTime();
    }
    
    // НОВИЙ МЕТОД: Перевіряє час перебування героя в зоні
    private void CheckHeroStayTime()
    {
        if (!enableAutoReturn || !hasHero || currentHero == null || isHeroReturning) return;
        
        // Перевіряємо чи не помер герой
        if (!IsHeroAlive()) return;
        
        float timeInZone = Time.time - heroEnterTime;
        
        if (timeInZone >= heroStayDuration)
        {
            Debug.Log($"⏰ Герой {currentHero.name} перебував в зоні {gameObject.name} {timeInZone:F1} сек - автоматично повертається до спавну");
            AutoReturnHeroToSpawn();
        }
    }
    
    // НОВИЙ МЕТОД: Автоматичне повернення героя до спавну
    private void AutoReturnHeroToSpawn()
    {
        if (isHeroReturning) return;
        
        isHeroReturning = true;
        
        // Отримуємо HeroController
        HeroController heroController = currentHero.GetComponent<HeroController>();
        if (heroController != null)
        {
            // Блокуємо кнопку повернення в зону
            heroController.BlockReturnToZone();
            
            // Запускаємо автоматичне повернення до спавну
            heroController.AutoReturnToSpawn();
        }
        
        // Звільняємо зону
        FreeZoneKeepHero();
        
        Debug.Log($"🔄 Зона {gameObject.name} звільнена - герой автоматично повертається до спавну");
    }
    
    void OnDestroy()
    {
        // Відписуємося від події при знищенні зони
        DeathHandler.OnHeroDeath -= OnHeroDeath;
    }
    
    // Обробник смерті героя
    private void OnHeroDeath(Unit deadHero)
    {
        // Перевіряємо чи це наш герой
        if (currentHero != null && currentHero == deadHero.gameObject)
        {
            Debug.Log($"💀 Герой {deadHero.unitName} помер, звільняємо зону {gameObject.name}");
            RemoveHero();
        }
    }

    // Метод, який викликається при кліку по зоні
    public void OnPointerClick(PointerEventData eventData)
    {
        HandleZoneClick(); // Реальна логіка кліку
    }

    private void HandleZoneClick()
    {
        // Якщо ми в режимі підготовки — ігноруємо зону
        if (GameManager.Instance != null && GameManager.Instance.IsPrepareMode())
        {
            Debug.Log("🔒 В режимі підготовки зони не активні");
            return;
        }

        TrySpawnHero(); // Пробуємо заспавнити героя
    }

    private void TrySpawnHero()
    {
        if (hasHero)
        {
            // Перевіряємо чи герой ще живий
            if (IsHeroAlive())
            {
                Debug.Log("⚠️ В зоні вже є живий герой");
                return;
            }
            else
            {
                // Герой помер, звільняємо зону
                Debug.Log("💀 Герой в зоні помер, звільняємо зону");
                RemoveHero();
            }
        }

        if (globalSpawnPoint == null)
        {
            Debug.LogError("❌ Не встановлена глобальна точка спавну! Створіть GameObject з тегом 'HeroSpawnPoint'");
            return;
        }

        Card selectedCard = FindSelectedCard();
        if (selectedCard == null)
        {
            Debug.Log("❌ Спочатку виберіть карту в слоті");
            return;
        }

        // НОВА ЛОГІКА: Переміщення існуючого героя між зонами
        if (selectedCard.IsInUse())
        {
            GameObject linkedHero = selectedCard.GetLinkedHero();
            if (linkedHero != null)
            {
                // Перевіряємо чи герой вже в якійсь зоні
                Zone currentHeroZone = GetHeroCurrentZone(linkedHero);
                if (currentHeroZone != null)
                {
                    // Герой вже в зоні - переміщуємо його до цієї нової зони
                    MoveHeroBetweenZones(linkedHero, currentHeroZone, this);
                    selectedCard.DeselectCard();
                    return;
                }
                else
                {
                    // Герой не в зоні (можливо в спавні) - просто переміщуємо до цієї зони
                    MoveHeroToZone(linkedHero, this);
                    selectedCard.DeselectCard();
                    return;
                }
            }
        }
        
        // Перевіряємо чи карта не на перезарядці
        if (selectedCard.IsOnCooldown())
        {
            float remainingTime = selectedCard.GetCooldownRemaining();
            Debug.Log($"⏳ Карту {selectedCard.gameObject.name} ще на перезарядці. Залишилося: {remainingTime:F1} сек");
            return;
        }

        GameObject heroPrefab = selectedCard.GetHeroPrefab();
        if (heroPrefab == null)
        {
            Debug.Log("❌ У вибраної карти немає героя для спавну");
            return;
        }

        Debug.Log($"🟢 Спавнимо героя з карти: {selectedCard.gameObject.name}");

        SpawnHeroAndMoveToZone(heroPrefab, selectedCard);
        selectedCard.DeselectCard(); // Після спавну — скидаємо вибір
    }

    private Card FindSelectedCard()
    {
        // Шукаємо всі слоти і перевіряємо, чи є в них вибрана карта
        Slot[] allSlots = FindObjectsByType<Slot>(FindObjectsSortMode.None);
        foreach (Slot slot in allSlots)
        {
            if (!slot.IsEmpty())
            {
                Card cardComponent = slot.GetCurrentCardComponent();
                if (cardComponent != null && cardComponent.isSelected)
                {
                    Debug.Log($"✅ Знайдено вибрану карту: {cardComponent.gameObject.name}");
                    return cardComponent;
                }
            }
        }

        Debug.Log("⚠️ Вибрану карту не знайдено");
        return null;
    }

    private void SpawnHeroAndMoveToZone(GameObject heroPrefab, Card sourceCard)
    {
        // Спавнимо героя в глобальній точці спавну
        Vector3 spawnPosition = globalSpawnPoint.position;
        Quaternion spawnRotation = globalSpawnPoint.rotation;

        currentHero = Instantiate(heroPrefab, spawnPosition, spawnRotation);
        hasHero = true;

        // Лочимо карту на цього героя, щоб не спавнити дублікати
        if (sourceCard != null)
        {
            sourceCard.LockForHero(currentHero);
        }

        // Отримуємо компонент Hero та налаштовуємо його цільову позицію
        Hero heroComponent = currentHero.GetComponent<Hero>();
        if (heroComponent != null)
        {
            // Додаємо компонент ZoneMovement для керування рухом до зони
            ZoneMovement movement = currentHero.AddComponent<ZoneMovement>();
            movement.Initialize(transform.position, this);
            
            // Додаємо HeroController для управління кнопками
            HeroController heroController = currentHero.GetComponent<HeroController>();
            if (heroController == null)
            {
                heroController = currentHero.AddComponent<HeroController>();
            }
            
            // Прив'язуємо героя до зони
            heroController.SetCurrentZone(this);

            // НОВЕ: НЕ активуємо кнопки автоматично - чекаємо кліку гравця
            // GameManager.SelectHero(currentHero); // Закоментовано
        }
        else
        {
            Debug.LogError("❌ У префабі героя немає компонента Hero!");
        }
    }

    // Викликається коли герой досягає зони
    public void OnHeroReachedZone(GameObject hero)
    {
        AssignHero(hero);
    }
    
    // Призначає героя цій зоні без спавну
    public void AssignHero(GameObject hero)
    {
        currentHero = hero;
        hasHero = true;
        heroEnterTime = Time.time; // Зберігаємо час входу героя в зону
        Debug.Log($"✅ Герой {hero.name} призначений до зони {gameObject.name}");
        StartCoroutine(MonitorHeroDeath(hero));
    }
    
    // Звільняє зону, але НЕ знищує героя і не очищає його контролер
    public void FreeZoneKeepHero()
    {
        if (currentHero != null)
        {
            StopAllCoroutines();
        }
        currentHero = null;
        hasHero = false;
        heroEnterTime = 0f; // Скидаємо час входу
        isHeroReturning = false; // Скидаємо стан повернення
        Debug.Log($"🚫 Зона {gameObject.name} звільнена без видалення героя");
    }
    
    // Відстежує смерть героя і звільняє зону
    private System.Collections.IEnumerator MonitorHeroDeath(GameObject hero)
    {
        while (hero != null && !hero.GetComponent<Unit>().isDead)
        {
            yield return new WaitForSeconds(0.5f); // Перевіряємо кожні 0.5 секунди
        }
        
        // Якщо герой помер або був знищений
        if (hero == null || (hero.GetComponent<Unit>() != null && hero.GetComponent<Unit>().isDead))
        {
            Debug.Log($"💀 Герой {hero?.name ?? "невідомий"} помер, звільняємо зону {gameObject.name}");
            RemoveHero();
        }
    }

    public void RemoveHero()
    {
        if (currentHero != null)
        {
            // Зупиняємо відстеження смерті
            StopAllCoroutines();
            
            // Сповіщаємо HeroController про звільнення з зони
            HeroController heroController = currentHero.GetComponent<HeroController>();
            if (heroController != null)
            {
                heroController.ClearCurrentZone();
            }
            
            // Не видаляємо героя — тільки відв'язуємо зону
            // (герой може бути в спавні і повернутися назад)
            // currentHero НЕ знищуємо
        }

        currentHero = null;
        hasHero = false;
        heroEnterTime = 0f; // Скидаємо час входу
        isHeroReturning = false; // Скидаємо стан повернення

        Debug.Log("🗑️ Героя видалено з зони: " + gameObject.name);
    }

    public bool HasHero()
    {
        return hasHero;
    }

    public GameObject GetCurrentHero()
    {
        return currentHero;
    }
    
    // Перевіряє чи герой ще живий
    public bool IsHeroAlive()
    {
        if (currentHero == null) return false;
        
        Unit heroUnit = currentHero.GetComponent<Unit>();
        return heroUnit != null && !heroUnit.isDead;
    }
    
    // Примусово звільняє зону (наприклад, якщо герой застряг)
    public void ForceFreeZone()
    {
        if (hasHero)
        {
            Debug.Log($"🔄 Примусово звільняємо зону {gameObject.name}");
            RemoveHero();
        }
    }

    // Метод для встановлення глобальної точки спавну (можна викликати з інших скриптів)
    public static void SetGlobalSpawnPoint(Transform spawnPoint)
    {
        globalSpawnPoint = spawnPoint;
        Debug.Log("🎯 Встановлено нову глобальну точку спавну: " + spawnPoint.name);
    }
    
    // НОВІ ПУБЛІЧНІ МЕТОДИ ДЛЯ ТАЙМЕРА
    
    /// <summary>
    /// Отримує час входу героя в зону
    /// </summary>
    public float GetHeroEnterTime()
    {
        return heroEnterTime;
    }
    
    /// <summary>
    /// Отримує тривалість перебування героя в зоні
    /// </summary>
    public float GetHeroStayDuration()
    {
        return heroStayDuration;
    }
    
    /// <summary>
    /// Отримує залишок часу перебування героя в зоні
    /// </summary>
    public float GetRemainingTime()
    {
        if (!hasHero || currentHero == null) return 0f;
        
        float timeInZone = Time.time - heroEnterTime;
        return Mathf.Max(0f, heroStayDuration - timeInZone);
    }
    
    /// <summary>
    /// Отримує відсоток залишеного часу
    /// </summary>
    public float GetTimePercentage()
    {
        if (!hasHero || currentHero == null) return 0f;
        
        float remainingTime = GetRemainingTime();
        return remainingTime / heroStayDuration;
    }
    
    /// <summary>
    /// Перевіряє чи залишилося мало часу (для попередження)
    /// </summary>
    public bool IsTimeRunningOut()
    {
        return GetTimePercentage() <= 0.3f; // 30% часу залишилося
    }
    
    /// <summary>
    /// Перевіряє чи критично мало часу
    /// </summary>
    public bool IsTimeCritical()
    {
        return GetTimePercentage() <= 0.1f; // 10% часу залишилося
    }

    // НОВИЙ МЕТОД: Знаходить поточну зону героя
    private Zone GetHeroCurrentZone(GameObject hero)
    {
        Zone[] allZones = FindObjectsByType<Zone>(FindObjectsSortMode.None);
        foreach (Zone zone in allZones)
        {
            if (zone.HasHero() && zone.GetCurrentHero() == hero)
            {
                return zone;
            }
        }
        return null;
    }

    // НОВИЙ МЕТОД: Переміщує героя між зонами
    private void MoveHeroBetweenZones(GameObject hero, Zone fromZone, Zone toZone)
    {
        if (fromZone == toZone)
        {
            Debug.Log("⚠️ Герой вже в цій зоні");
            return;
        }

        Debug.Log($"🔄 Переміщуємо героя {hero.name} з зони {fromZone.name} до зони {toZone.name}");

        // Звільняємо стару зону
        fromZone.FreeZoneKeepHero();

        // Призначаємо героя до нової зони
        toZone.AssignHero(hero);

        // Оновлюємо HeroController
        HeroController heroController = hero.GetComponent<HeroController>();
        if (heroController != null)
        {
            heroController.SetCurrentZone(toZone);
        }

        // Запускаємо рух до нової зони
        ZoneMovement movement = hero.GetComponent<ZoneMovement>();
        if (movement == null)
        {
            movement = hero.AddComponent<ZoneMovement>();
        }
        movement.Initialize(toZone.transform.position, toZone);

        // Вибираємо героя для UI
        GameManager.SelectHero(hero);

        Debug.Log($"✅ Герой {hero.name} успішно переміщений з зони {fromZone.name} до зони {toZone.name}");
    }

    // НОВИЙ МЕТОД: Переміщує героя до зони (якщо він не в зоні)
    private void MoveHeroToZone(GameObject hero, Zone targetZone)
    {
        Debug.Log($"🎯 Переміщуємо героя {hero.name} до зони {targetZone.name}");

        // Призначаємо героя до цієї зони
        targetZone.AssignHero(hero);

        // Оновлюємо HeroController
        HeroController heroController = hero.GetComponent<HeroController>();
        if (heroController != null)
        {
            heroController.SetCurrentZone(targetZone);
        }

        // Запускаємо рух до зони
        ZoneMovement movement = hero.GetComponent<ZoneMovement>();
        if (movement == null)
        {
            movement = hero.AddComponent<ZoneMovement>();
        }
        movement.Initialize(targetZone.transform.position, targetZone);

        // Вибираємо героя для UI
        GameManager.SelectHero(hero);

        Debug.Log($"✅ Герой {hero.name} успішно переміщений до зони {targetZone.name}");
    }
}