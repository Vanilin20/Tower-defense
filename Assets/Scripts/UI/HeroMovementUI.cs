using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI компонент для відображення інформації про переміщення героїв
/// </summary>
public class HeroMovementUI : MonoBehaviour
{
    [Header("UI елементи")]
    public TextMeshProUGUI movementInfoText;
    public Image movementIndicator;
    public Button cancelMovementButton;
    
    [Header("Налаштування")]
    public Color movementColor = Color.cyan;
    public Color normalColor = Color.white;
    
    private Card selectedCardForMovement;
    
    void Start()
    {
        // Налаштовуємо кнопку скасування
        if (cancelMovementButton != null)
        {
            cancelMovementButton.onClick.AddListener(CancelMovement);
        }
        
        // Спочатку приховуємо індикатор
        SetMovementUI(false);
    }
    
    void Update()
    {
        UpdateMovementInfo();
    }
    
    /// <summary>
    /// Оновлює інформацію про переміщення
    /// </summary>
    private void UpdateMovementInfo()
    {
        // Шукаємо карту, вибрану для переміщення
        Card movementCard = FindMovementCard();
        
        if (movementCard != null && movementCard.IsSelectedForMovement())
        {
            selectedCardForMovement = movementCard;
            ShowMovementInfo(movementCard);
        }
        else
        {
            selectedCardForMovement = null;
            HideMovementInfo();
        }
    }
    
    /// <summary>
    /// Знаходить карту, вибрану для переміщення
    /// </summary>
    private Card FindMovementCard()
    {
        Card[] allCards = FindObjectsByType<Card>(FindObjectsSortMode.None);
        foreach (Card card in allCards)
        {
            if (card.IsSelectedForMovement())
            {
                return card;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Показує інформацію про переміщення
    /// </summary>
    private void ShowMovementInfo(Card card)
    {
        SetMovementUI(true);
        
        if (movementInfoText != null)
        {
            movementInfoText.text = card.GetMovementInfo();
        }
        
        if (movementIndicator != null)
        {
            movementIndicator.color = movementColor;
        }
        
        Debug.Log($"🎯 UI: Показуємо інформацію про переміщення - {card.GetMovementInfo()}");
    }
    
    /// <summary>
    /// Приховує інформацію про переміщення
    /// </summary>
    private void HideMovementInfo()
    {
        SetMovementUI(false);
        
        if (movementInfoText != null)
        {
            movementInfoText.text = "";
        }
        
        if (movementIndicator != null)
        {
            movementIndicator.color = normalColor;
        }
    }
    
    /// <summary>
    /// Встановлює видимість UI переміщення
    /// </summary>
    private void SetMovementUI(bool visible)
    {
        if (movementIndicator != null)
        {
            movementIndicator.gameObject.SetActive(visible);
        }
        
        if (cancelMovementButton != null)
        {
            cancelMovementButton.gameObject.SetActive(visible);
        }
    }
    
    /// <summary>
    /// Скасовує переміщення
    /// </summary>
    public void CancelMovement()
    {
        if (selectedCardForMovement != null)
        {
            selectedCardForMovement.DeselectCard();
            Debug.Log("❌ Переміщення героя скасовано");
        }
    }
    
    /// <summary>
    /// Публічний метод для зовнішнього виклику
    /// </summary>
    public void ForceUpdate()
    {
        UpdateMovementInfo();
    }
    
    void OnDestroy()
    {
        if (cancelMovementButton != null)
        {
            cancelMovementButton.onClick.RemoveAllListeners();
        }
    }
} 