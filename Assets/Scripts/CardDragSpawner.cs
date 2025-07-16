using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CardDragSpawner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject unitPrefab; // 🧱 Префаб юніта, якого ця карта спавнить
    public Transform spawnPoint;  // 📌 Звідки юніт летить (зазвичай = позиція карти)

    private Vector3 originalPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Image cardImage;
    private Canvas canvas;

    public float cooldownTime = 3f;
    private bool isOnCooldown = false;
    private bool isInSlot = false;
    private Transform currentSlot = null;
    public int unitCost = 30; // 💰 Вартість цієї карти

    // 🆕 СИСТЕМА ВІДСТЕЖЕННЯ ЮНІТА
    private GameObject currentUnit = null; // Поточний юніт, створений цією картою
    private bool hasActiveUnit = false; // Чи є активний юніт

    // 🎮 РЕЖИМИ КАРТИ
    public enum CardMode
    {
        Selection,  // Режим вибору - можна перетягувати між слотами
        Gameplay    // Ігровий режим - можна використовувати для спавну
    }

    [Header("Card Mode Settings")]
    public CardMode currentMode = CardMode.Selection;
    private bool isGameActive = false; // Чи активна гра

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        cardImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        
        // Якщо немає CanvasGroup - додаємо
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        // Зберігаємо початкову позицію та батьківський об'єкт
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Встановлюємо повну непрозорість тільки через CanvasGroup
        SetCardAlpha(1f);
        
        // Починаємо в режимі вибору
        SetMode(CardMode.Selection);
    }

    // 🆕 МЕТОД ДЛЯ ОНОВЛЕННЯ СТАТУСУ ЮНІТА
    void Update()
    {
        // Перевіряємо стан юніта кожен кадр
        if (hasActiveUnit && currentUnit == null)
        {
            // Юніт був знищений - знімаємо кулдаун
            OnUnitDestroyed();
        }
    }

    // 🆕 МЕТОД ВИКЛИКАЄТЬСЯ КОЛИ ЮНІТ ПОМИРАЄ
    public void OnUnitDestroyed()
    {
        Debug.Log($"Unit destroyed for card: {gameObject.name}");
        
        hasActiveUnit = false;
        currentUnit = null;
        
        // Знімаємо кулдаун і повертаємо карту в робочий стан
        isOnCooldown = false;
        SetCardAlpha(1f);
        
        // 🔄 Оновлюємо візуальний стан
        UpdateVisualMode();
        
        // Зупиняємо всі корутини кулдауну
        StopAllCoroutines();
    }

    // 🎮 УПРАВЛІННЯ РЕЖИМАМИ
    public void SetMode(CardMode mode)
    {
        currentMode = mode;
        UpdateVisualMode();
    }

    public void StartGame()
    {
        isGameActive = true;
        // Переводимо всі карти в слотах в ігровий режим
        if (isInSlot)
        {
            SetMode(CardMode.Gameplay);
        }
    }

    public void StopGame()
    {
        isGameActive = false;
        // Повертаємо всі карти в режим вибору
        SetMode(CardMode.Selection);
        
        // 🆕 Скидаємо стан юніта при закінченні гри
        if (hasActiveUnit)
        {
            OnUnitDestroyed();
        }
    }

    private void UpdateVisualMode()
    {
        // 🔄 ОНОВЛЕНО: Візуальні зміни залежно від режиму і стану кулдауну
        if (cardImage != null)
        {
            // 🔴 ПЕРЕВІРЯЄМО КУЛДАУН ПЕРШИМ
            if (isOnCooldown || hasActiveUnit)
            {
                // Червоний колір під час кулдауну або поки є активний юніт
                cardImage.color = Color.red;
                return;
            }

            // Якщо немає кулдауну - показуємо звичайні кольори
            switch (currentMode)
            {
                case CardMode.Selection:
                    // Білий колір для режиму вибору
                    cardImage.color = Color.white;
                    break;
                case CardMode.Gameplay:
                    // Білий колір для ігрового режиму (коли немає кулдауну)
                    cardImage.color = Color.white;
                    break;
            }
        }
    }

    private void SetCardAlpha(float alpha)
    {
        // Використовуємо тільки CanvasGroup для контролю прозорості
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    // 🔄 ОНОВЛЕНИЙ МЕТОД КУЛДАУНУ
    private IEnumerator Cooldown()
    {
        isOnCooldown = true;

        // 🔴 Робимо трохи прозорішою під час кулдауну
        SetCardAlpha(0.5f);
        
        // 🔄 Оновлюємо візуальний стан (червоний колір)
        UpdateVisualMode();

        yield return new WaitForSeconds(cooldownTime);

        // 🆕 Після класичного кулдауну перевіряємо стан юніта
        if (hasActiveUnit)
        {
            Debug.Log($"Unit is still alive for card: {gameObject.name}, keeping cooldown active");
            
            // Якщо юніт ще живий - залишаємо кулдаун активним
            // Кулдаун буде знято автоматично в OnUnitDestroyed()
            yield break;
        }

        // Якщо юніта немає - знімаємо кулдаун
        isOnCooldown = false;
        SetCardAlpha(1f);
        
        // 🔄 Оновлюємо візуальний стан (білий колір)
        UpdateVisualMode();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 🆕 Блокуємо перетягування якщо є активний юніт
        if (isOnCooldown || hasActiveUnit) return;

        // 🎮 ПЕРЕВІРКА РЕЖИМУ
        if (currentMode == CardMode.Gameplay && !CanUseInGameplay())
        {
            return; // Блокуємо перетягування якщо не можна використати
        }

        canvasGroup.blocksRaycasts = false;
        
        // 🔧 ВАЖЛИВО: Переміщуємо карту на верхній рівень для видимості
        transform.SetAsLastSibling();
        
        // Під час перетягування залишаємо карту повністю видимою
        SetCardAlpha(1f);
    }

    private bool CanUseInGameplay()
    {
        // В ігровому режимі карту можна використовувати тільки якщо вона в слоті і немає активного юніта
        return isInSlot && isGameActive && !hasActiveUnit;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isOnCooldown || hasActiveUnit) return;
        
        // 🎮 ОДНАКОВЕ ПЕРЕТЯГУВАННЯ ДЛЯ ОБОХ РЕЖИМІВ
        DragCard(eventData);
    }

    private void DragCard(PointerEventData eventData)
    {
        // 🌍 ПРАВИЛЬНА КОНВЕРТАЦІЯ ДЛЯ WORLD SPACE CANVAS
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // Для World Space Canvas конвертуємо екранні координати в світові
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Camera.main.WorldToScreenPoint(transform.position).z));
            transform.position = worldPosition;
        }
        else
        {
            // Для Screen Space Canvas
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPoint
            );
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 🎮 РІЗНА ПОВЕДІНКА ЗА РЕЖИМАМИ
        if (currentMode == CardMode.Selection)
        {
            HandleSelectionDrop(eventData);
        }
        else if (currentMode == CardMode.Gameplay)
        {
            HandleGameplayDrop(eventData);
        }
    }

    private void HandleSelectionDrop(PointerEventData eventData)
    {
        // В режимі вибору працюємо тільки зі слотами
        GameObject slotObject = GetSlotUnderCursor(eventData);
        if (slotObject != null)
        {
            CardSlot slot = slotObject.GetComponent<CardSlot>();
            if (slot != null && slot.IsEmpty())
            {
                // Якщо ми були в іншому слоті - звільняємо його
                if (currentSlot != null)
                {
                    CardSlot previousSlot = currentSlot.GetComponent<CardSlot>();
                    if (previousSlot != null)
                        previousSlot.RemoveCard();
                }

                // Переміщуємо карту в новий слот
                PlaceInSlot(slot);
                return;
            }
        }

        // Якщо не в слот - повертаємося на поточну позицію
        ReturnToCurrentPosition();
        SetCardAlpha(1f);
    }

    private void HandleGameplayDrop(PointerEventData eventData)
    {
        // 🌍 ПРАВИЛЬНА КОНВЕРТАЦІЯ КООРДИНАТ ДЛЯ WORLD SPACE CANVAS
        Vector3 worldPos;
        
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // Для World Space Canvas - використовуємо глибину Canvas'а для правильної конвертації
            float canvasDistance = Camera.main.WorldToScreenPoint(canvas.transform.position).z;
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, canvasDistance));
        }
        else
        {
            // Для Screen Space Canvas
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Camera.main.nearClipPlane));
        }
        
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);
        
        // Відладочний вивід для перевірки координат
        Debug.Log($"Screen: {eventData.position}, World: {worldPos2D}, Canvas Mode: {canvas.renderMode}");
        
        // 🔥 ВИПРАВЛЕННЯ: Використовуємо спеціальні шари для райкасту
        // Створюємо маску шарів, яка включає тільки зони (виключаючи юнітів)
        int zoneMask = 1 << LayerMask.NameToLayer("Zone"); // або використовуйте номер шару
        
        // Метод 1: Використовувати райкаст з маскою шарів
        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero, Mathf.Infinity, zoneMask);
        
        // Метод 2: Альтернативний спосіб - перевіряти всі зони безпосередньо
        ZoneController targetZone = FindZoneAtPosition(worldPos2D);
        
        if (targetZone != null)
        {
            Debug.Log($"Found zone: {targetZone.name}");
            
            // Перевіряємо чи зона дійсно вільна (не враховуючи юнітів що пробігають)
            if (!targetZone.hasUnit)
            {
                if (!GoldManager.Instance.TrySpendGold(unitCost))
                {
                    Debug.Log("❌ Not enough gold!");
                    ReturnToCurrentPosition();
                    SetCardAlpha(1f);
                    return;
                }

                Vector3 zoneCenter = targetZone.transform.position;

                // 🆕 СТВОРЮЄМО ЮНІТ І ЗБЕРІГАЄМО ПОСИЛАННЯ
                GameObject unit = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
                currentUnit = unit;
                hasActiveUnit = true;
                
                if (unit.TryGetComponent<Unit>(out Unit unitScript))
                {
                    unitScript.SetTargetPosition(zoneCenter);
                    unitScript.AssignZone(targetZone);
                    
                    // 🆕 ДОДАЄМО ПОСИЛАННЯ НА КАРТУ В ЮНІТ
                    unitScript.SetOwnerCard(this);
                }

                targetZone.hasUnit = true;
                
                // 🔄 Повертаємося в слот
                ReturnToCurrentPosition();
                
                // Гарантуємо повну непрозорість перед кулдауном
                SetCardAlpha(1f);
                
                // 🆕 Оновлюємо візуальний стан
                UpdateVisualMode();
                
                StartCoroutine(Cooldown());
                return;
            }
        }
        else
        {
            Debug.Log("No zone found at position");
        }

        // Якщо не в зону - повертаємося в слот
        ReturnToCurrentPosition();
        SetCardAlpha(1f);
    }

    // 🔥 НОВИЙ МЕТОД: Знаходження зони за позицією без використання райкасту
    private ZoneController FindZoneAtPosition(Vector2 worldPosition)
    {
        // Знаходимо всі зони в сцені (оновлений метод)
        ZoneController[] allZones = FindObjectsByType<ZoneController>(FindObjectsSortMode.None);
        
        foreach (ZoneController zone in allZones)
        {
            // Перевіряємо чи точка знаходиться в межах зони
            if (IsPointInZone(worldPosition, zone))
            {
                return zone;
            }
        }
        
        return null;
    }

    // 🔥 ДОПОМІЖНИЙ МЕТОД: Перевірка чи точка знаходиться в зоні
    private bool IsPointInZone(Vector2 point, ZoneController zone)
    {
        // Якщо у зони є Collider2D
        if (zone.TryGetComponent<Collider2D>(out Collider2D zoneCollider))
        {
            return zoneCollider.bounds.Contains(point);
        }
        
        // Якщо немає колайдера, можемо використовувати Transform
        // Припускаємо, що зона має прямокутну форму
        Vector3 zonePos = zone.transform.position;
        Vector3 zoneScale = zone.transform.localScale;
        
        // Простий AABB (Axis-Aligned Bounding Box) тест
        return (point.x >= zonePos.x - zoneScale.x/2 && 
                point.x <= zonePos.x + zoneScale.x/2 &&
                point.y >= zonePos.y - zoneScale.y/2 && 
                point.y <= zonePos.y + zoneScale.y/2);
    }

    private GameObject GetSlotUnderCursor(PointerEventData eventData)
    {
        // Використовуємо EventSystem для знаходження UI елементів під курсором
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("Slot"))
            {
                return result.gameObject;
            }
        }
        return null;
    }

    private void PlaceInSlot(CardSlot slot)
    {
        // Переміщуємо карту в слот
        transform.SetParent(slot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Оновлюємо стан
        isInSlot = true;
        currentSlot = slot.transform;
        slot.PlaceCard(this);

        // Якщо гра активна - переходимо в ігровий режим
        if (isGameActive)
        {
            SetMode(CardMode.Gameplay);
        }
    }

    private void ReturnToCurrentPosition()
    {
        if (isInSlot && currentSlot != null)
        {
            // Повертаємося в поточний слот
            transform.SetParent(currentSlot);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            // Повертаємося на початкову позицію
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // Метод для зняття карти зі слота (викликається CardSlot)
    public void RemoveFromSlot()
    {
        // В ігровому режимі не можна знімати карти зі слотів
        if (currentMode == CardMode.Gameplay && isGameActive)
        {
            return;
        }

        if (currentSlot != null)
        {
            CardSlot slot = currentSlot.GetComponent<CardSlot>();
            if (slot != null)
                slot.RemoveCard();
        }

        // Повертаємо карту на початкову позицію
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        
        isInSlot = false;
        currentSlot = null;

        // Повертаємося в режим вибору
        SetMode(CardMode.Selection);
    }
}