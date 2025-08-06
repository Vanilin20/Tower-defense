using UnityEngine;
using UnityEngine.EventSystems;

// Компонент для зони (працює з новими версіями Unity)
[RequireComponent(typeof(Collider2D))] // Гарантує, що зона має 2D колайдер
public class Zone : MonoBehaviour, IPointerClickHandler
{
    [Header("Зона налаштування")]
    public Transform spawnPoint;         // Точка спавну героя (якщо null — буде позиція зони)
    public bool hasHero = false;         // Чи є герой у зоні
    public GameObject currentHero;       // Поточний герой у зоні

    void Start()
    {
        // Якщо spawnPoint не заданий вручну — використовуємо позицію самої зони
        if (spawnPoint == null)
        {
            spawnPoint = transform;
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
            Debug.Log("⚠️ В зоні вже є герой");
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

        SpawnHero(heroPrefab);
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

    private void SpawnHero(GameObject heroPrefab)
    {
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        currentHero = Instantiate(heroPrefab, spawnPosition, spawnRotation);
        hasHero = true;

        Debug.Log("🧍 Герой заспавнено в зоні: " + gameObject.name);
    }

    public void RemoveHero()
    {
        if (currentHero != null)
        {
            Destroy(currentHero);
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
}
