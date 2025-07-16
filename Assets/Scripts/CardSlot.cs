using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    private CardDragSpawner currentCard = null;
    private bool isEmpty = true;

    public bool IsEmpty()
    {
        return isEmpty;
    }

    public void PlaceCard(CardDragSpawner card)
    {
        currentCard = card;
        isEmpty = false;
    }

    public void RemoveCard()
    {
        currentCard = null;
        isEmpty = true;
    }

    // Подвійний клік для зняття карти зі слота (тільки в режимі вибору)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2 && !isEmpty && currentCard != null)
        {
            // Перевіряємо чи можна знімати карту
            if (currentCard.currentMode == CardDragSpawner.CardMode.Selection)
            {
                currentCard.RemoveFromSlot();
            }
        }
    }

    // Підтримка IDropHandler (альтернативний спосіб)
    public void OnDrop(PointerEventData eventData)
    {
        // Цей метод можна використати як альтернативу, 
        // але основна логіка вже в CardDragSpawner
    }
}