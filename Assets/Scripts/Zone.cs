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

        GameObject heroPrefab = selectedCard.GetHeroPrefab();
        if (heroPrefab == null)
        {
            Debug.Log("❌ У вибраної карти немає героя для спавну");
            return;
        }

        Debug.Log($"🟢 Спавнимо героя з карти: {selectedCard.gameObject.name}");

        SpawnHeroAndMoveToZone(heroPrefab);
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

    private void SpawnHeroAndMoveToZone(GameObject heroPrefab)
    {
        // Спавнимо героя в глобальній точці спавну
        Vector3 spawnPosition = globalSpawnPoint.position;
        Quaternion spawnRotation = globalSpawnPoint.rotation;

        currentHero = Instantiate(heroPrefab, spawnPosition, spawnRotation);
        hasHero = true;

        // Отримуємо компонент Hero та налаштовуємо його цільову позицію
        Hero heroComponent = currentHero.GetComponent<Hero>();
        if (heroComponent != null)
        {
            // Додаємо компонент ZoneMovement для керування рухом до зони
            ZoneMovement movement = currentHero.AddComponent<ZoneMovement>();
            movement.Initialize(transform.position, this);
        }
        else
        {
            Debug.LogError("❌ У префабі героя немає компонента Hero!");
        }

        Debug.Log($"🧍 Герой заспавнено в точці спавну і рухається до зони: {gameObject.name}");
    }

    // Викликається коли герой досягає зони
    public void OnHeroReachedZone(GameObject hero)
    {
        Debug.Log($"✅ Герой {hero.name} досягнув зони {gameObject.name}");
        
        // Додаємо відстеження смерті героя
        StartCoroutine(MonitorHeroDeath(hero));
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
            
            // Знищуємо героя якщо він ще існує
            if (currentHero != null)
            {
                Destroy(currentHero);
            }
        }

        currentHero = null;
        hasHero = false;

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
}