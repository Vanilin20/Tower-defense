using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Компонент для карти
public class Card : MonoBehaviour, IPointerClickHandler
{
    [Header("Карта налаштування")]
    public GameObject heroPrefab; // Префаб героя який буде спавнитись
    public bool isSelected = false;
    public bool isInSlot = false; // Перевіряє чи карта в слоті
    
    // Заборона повторного використання, доки герой живий
    [Header("Стан використання")]
    [SerializeField] private bool isLockedInUse = false;
    [SerializeField] private GameObject linkedHeroInstance = null;
    
    [Header("Таймер перезарядки")]
    [SerializeField] private float cooldownAfterSpawn = 5f; // Час перезарядки після того, як герой пішов на спавн
    [SerializeField] private bool isOnCooldown = false;
    [SerializeField] private float cooldownEndTime = 0f;
    
    // НОВЕ: Стан для переміщення героя
    [Header("Переміщення героя")]
    [SerializeField] private bool isSelectedForMovement = false;
    
    private Image cardImage;
    private Button cardButton;
    private Vector3 originalPosition; // Зберігаємо оригінальну позицію
    private Transform originalParent; // Зберігаємо оригінального батька
    
    void Start()
    {
        cardImage = GetComponent<Image>();
        cardButton = GetComponent<Button>();
        
        // Зберігаємо оригінальну позицію та батька
        originalPosition = transform.position;
        originalParent = transform.parent;
        
        // Якщо є кнопка, додаємо обробник
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClick);
        }
    }
    
    void Update()
    {
        // Перевіряємо таймер перезарядки
        CheckCooldown();
    }
    
    private void CheckCooldown()
    {
        if (isOnCooldown && Time.time >= cooldownEndTime)
        {
            // Перезарядка завершена
            isOnCooldown = false;
            Debug.Log($"✅ Карта {gameObject.name} перезаряджена і готова до використання");
        }
    }
    
    // Для 3D об'єктів
    void OnMouseDown()
    {
        HandleCardClick();
    }
    
    // Для UI елементів (IPointerClickHandler)
    public void OnPointerClick(PointerEventData eventData)
    {
        HandleCardClick();
    }
    
    // Для UI кнопок
    public void OnCardClick()
    {
        HandleCardClick();
    }
    
    private void HandleCardClick()
    {
        // В ігровому режимі — якщо карта вже використана і герой живий,
        // ми обираємо пов'язаного героя, а сама карта стане "командою повернення" при кліку по зоні
        if (GameManager.Instance != null && GameManager.Instance.IsGameMode())
        {
            if (isLockedInUse && LinkedHeroAlive())
            {
                // Перевіряємо чи не на перезарядці
                if (isOnCooldown)
                {
                    float remainingTime = cooldownEndTime - Time.time;
                    Debug.Log($"⏳ Карту {gameObject.name} ще на перезарядці. Залишилося: {remainingTime:F1} сек");
                    return;
                }
                
                GameManager.SelectHero(linkedHeroInstance);
                Debug.Log($"ℹ️ Карту {gameObject.name} вже використано — вибрано існуючого героя. Клік по зоні поверне його в зону (без спавну нового)");
            }
        }
        
        // Далі — поточна логіка
        // Перевіряємо режим гри
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsPrepareMode())
            {
                if (isInSlot)
                {
                    // Режим підготовки - прибираємо карту зі слота на оригінальне місце
                    ReturnToOriginalPosition();
                }
                else
                {
                    // Режим підготовки - автоматично ставимо карту в слот
                    TryPlaceInSlot();
                }
            }
            else if (GameManager.Instance.IsGameMode() && isInSlot)
            {
                // Ігровий режим - вибираємо карту для дії
                // Якщо герой ще живий і прив'язаний — ця карта ініціюватиме повернення героя до зони при кліку по зоні
                SelectCard();
            }
        }
        else
        {
            // Якщо немає GameManager, поводимося як раніше
            if (isInSlot)
            {
                ReturnToOriginalPosition();
            }
            else
            {
                TryPlaceInSlot();
            }
        }
    }
    
    private void ReturnToOriginalPosition()
    {
        // Знаходимо слот де знаходиться карта і видаляємо з нього
        Slot[] allSlots = FindObjectsByType<Slot>(FindObjectsSortMode.None);
        foreach (Slot slot in allSlots)
        {
            if (slot.GetCurrentCard() == gameObject)
            {
                slot.RemoveCard();
                break;
            }
        }
        
        // Повертаємо карту на оригінальне місце
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        SetInSlot(false);
        
        Debug.Log($"Карту {gameObject.name} повернено на оригінальне місце");
    }
    
    private void TryPlaceInSlot()
    {
        // Знаходимо вільний слот з пріоритетом
        Slot freeSlot = FindFreeSlotByPriority();
        if (freeSlot != null)
        {
            // Розміщуємо карту в слоті
            freeSlot.PlaceCardDirectly(this);
            Debug.Log($"Карту {gameObject.name} автоматично розміщено в слоті {freeSlot.gameObject.name}");
        }
        else
        {
            Debug.Log("Немає вільних слотів для карти!");
        }
    }
    
    // МЕТОД 1: Пошук по пріоритету слота
    private Slot FindFreeSlotByPriority()
    {
        Slot[] allSlots = FindObjectsByType<Slot>(FindObjectsSortMode.None);
        
        // Сортуємо слоти по пріоритету (менше число = вищий пріоритет)
        System.Array.Sort(allSlots, (slot1, slot2) => 
            slot1.GetSlotPriority().CompareTo(slot2.GetSlotPriority()));
        
        // Знаходимо перший вільний слот в відсортованому списку
        foreach (Slot slot in allSlots)
        {
            if (slot.IsEmpty())
            {
                return slot;
            }
        }
        return null;
    }

    private void SelectCard()
    {
        // Скидаємо вибір з інших карт
        Card[] allCards = FindObjectsByType<Card>(FindObjectsSortMode.None);
        foreach (Card card in allCards)
        {
            card.DeselectCard();
        }
        
        // Перевіряємо чи карта не на перезарядці
        if (isOnCooldown)
        {
            float remainingTime = cooldownEndTime - Time.time;
            Debug.Log($"⏳ Карту {gameObject.name} неможливо вибрати — ще на перезарядці. Залишилося: {remainingTime:F1} сек");
            return;
        }
        
        // Завжди дозволяємо вибір карти
        isSelected = true;
        
        // НОВЕ: Встановлюємо стан для переміщення якщо герой вже існує
        if (isLockedInUse && LinkedHeroAlive())
        {
            isSelectedForMovement = true;
            Debug.Log($"🎯 Карту вибрано для переміщення: {gameObject.name} (герой {linkedHeroInstance.name} готовий до переміщення)");
        }
        else
        {
            isSelectedForMovement = false;
            Debug.Log("Карту вибрано: " + gameObject.name);
        }
    }
    
    public void DeselectCard()
    {
        isSelected = false;
        isSelectedForMovement = false; // Скидаємо стан переміщення
    }
    
    public GameObject GetHeroPrefab()
    {
        return heroPrefab;
    }
    
    // === НОВА ЛОГІКА БЛОКУВАННЯ ===
    public bool IsInUse()
    {
        // Карта вважається використаною, якщо заблокована героєм і НЕ на перезарядці
        return isLockedInUse && LinkedHeroAlive() && !isOnCooldown;
    }

    public void LockForHero(GameObject heroInstance)
    {
        linkedHeroInstance = heroInstance;
        isLockedInUse = heroInstance != null;
    }

    public void Unlock()
    {
        isLockedInUse = false;
        linkedHeroInstance = null;
    }

    public void StartCooldown()
    {
        isOnCooldown = true;
        cooldownEndTime = Time.time + cooldownAfterSpawn;
        Debug.Log($"⏳ Карта {gameObject.name} на перезарядці {cooldownAfterSpawn} сек");
    }

    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    public float GetCooldownRemaining()
    {
        if (!isOnCooldown) return 0f;
        return Mathf.Max(0f, cooldownEndTime - Time.time);
    }

    public GameObject GetLinkedHero()
    {
        return linkedHeroInstance;
    }

    public bool LinkedHeroAlive()
    {
        if (linkedHeroInstance == null) return false;
        var unit = linkedHeroInstance.GetComponent<Unit>();
        return linkedHeroInstance != null && unit != null && !unit.isDead;
    }
    // === КІНЕЦЬ НОВОЇ ЛОГІКИ ===
    
    public void SetInSlot(bool inSlot)
    {
        isInSlot = inSlot;
        if (isInSlot)
        {
            isSelected = false; // Автоматично скидаємо вибір коли карта в слоті
            isSelectedForMovement = false; // Скидаємо стан переміщення
        }
    }
    
    /// <summary>
    /// Перевіряє чи карта знаходиться в слоті
    /// </summary>
    public bool IsInSlot()
    {
        return isInSlot;
    }
    
    // НОВІ МЕТОДИ ДЛЯ ПЕРЕМІЩЕННЯ ГЕРОЯ
    
    /// <summary>
    /// Перевіряє чи карта вибрана для переміщення героя
    /// </summary>
    public bool IsSelectedForMovement()
    {
        return isSelectedForMovement && isLockedInUse && LinkedHeroAlive();
    }
    
    /// <summary>
    /// Встановлює стан переміщення
    /// </summary>
    public void SetMovementState(bool forMovement)
    {
        isSelectedForMovement = forMovement;
    }
    
    /// <summary>
    /// Отримує інформацію про переміщення
    /// </summary>
    public string GetMovementInfo()
    {
        if (!isSelectedForMovement) return "Не вибрано для переміщення";
        
        if (linkedHeroInstance != null)
        {
            Zone currentZone = GetHeroCurrentZone();
            if (currentZone != null)
            {
                return $"Герой {linkedHeroInstance.name} в зоні {currentZone.name} - готовий до переміщення";
            }
            else
            {
                return $"Герой {linkedHeroInstance.name} не в зоні - готовий до розміщення";
            }
        }
        
        return "Невідома інформація про переміщення";
    }
    
    /// <summary>
    /// Знаходить поточну зону героя
    /// </summary>
    private Zone GetHeroCurrentZone()
    {
        if (linkedHeroInstance == null) return null;
        
        Zone[] allZones = FindObjectsByType<Zone>(FindObjectsSortMode.None);
        foreach (Zone zone in allZones)
        {
            if (zone.HasHero() && zone.GetCurrentHero() == linkedHeroInstance)
            {
                return zone;
            }
        }
        return null;
    }
}