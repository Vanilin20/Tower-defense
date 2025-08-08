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
                // Ігровий режим - вибираємо карту для спавну героя
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
        
        // Вибираємо цю карту
        isSelected = true;
        
        Debug.Log("Карту вибрано: " + gameObject.name);
    }
    
    public void DeselectCard()
    {
        isSelected = false;
    }
    
    public GameObject GetHeroPrefab()
    {
        return heroPrefab;
    }
    
    public void SetInSlot(bool inSlot)
    {
        isInSlot = inSlot;
        if (isInSlot)
        {
            isSelected = false; // Автоматично скидаємо вибір коли карта в слоті
        }
    }
}