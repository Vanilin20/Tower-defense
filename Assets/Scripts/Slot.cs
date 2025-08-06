using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Компонент для слота (тільки для карт)
public class Slot : MonoBehaviour, IPointerClickHandler
{
    [Header("Слот налаштування")]
    public GameObject currentCard; // Поточна карта в слоті
    
    void Start()
    {
        // Початкова ініціалізація (якщо потрібна)
    }
    
    // Для 3D об'єктів
    void OnMouseDown()
    {
        HandleSlotClick();
    }
    
    // Для UI елементів
    public void OnPointerClick(PointerEventData eventData)
    {
        HandleSlotClick();
    }
    
    private void HandleSlotClick()
    {
        // Перевіряємо режим гри
        if (GameManager.Instance != null && GameManager.Instance.IsPrepareMode())
        {
            // В режимі підготовки можна видаляти карти зі слота
            if (currentCard != null)
            {
                Debug.Log("Слот зайнятий картою: " + currentCard.name);
                RemoveCard(); // Видаляємо карту зі слота
                return;
            }
        }
        
        // В ігровому режимі слоти не реагують на кліки
        if (GameManager.Instance != null && GameManager.Instance.IsGameMode())
        {
            Debug.Log("В ігровому режимі слоти не активні для взаємодії!");
            return;
        }
    }
    
    // Публічний метод для прямого розміщення карти (викликається з карти)
    public void PlaceCardDirectly(Card card)
    {
        if (currentCard != null)
        {
            Debug.Log("Слот вже зайнятий!");
            return;
        }
        
        currentCard = card.gameObject;
        card.SetInSlot(true);
        
        // Переміщуємо карту до слота
        card.transform.position = transform.position + Vector3.up * 0.1f;
        card.transform.SetParent(transform);
        
        Debug.Log("Карту розміщено в слоті: " + gameObject.name);
    }
    
    public void RemoveCard()
    {
        if (currentCard != null)
        {
            Card cardComponent = currentCard.GetComponent<Card>();
            if (cardComponent != null)
            {
                cardComponent.SetInSlot(false);
            }
            
            currentCard.transform.SetParent(null);
            currentCard = null;
            
            Debug.Log("Карту видалено зі слота: " + gameObject.name);
        }
    }
    
    public bool IsEmpty()
    {
        return currentCard == null;
    }
    
    public GameObject GetCurrentCard()
    {
        return currentCard;
    }
    
    public Card GetCurrentCardComponent()
    {
        if (currentCard != null)
        {
            return currentCard.GetComponent<Card>();
        }
        return null;
    }
}