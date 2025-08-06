using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI елементи")]
    public Button readyButton; // Кнопка "Готово"
    public Text modeText; // Текст для відображення поточного режиму
    
    [Header("Налаштування")]
    public bool isPrepareMode = true; // true = режим підготовки, false = ігровий режим
    
    public static GameManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(SwitchToGameMode);
        }
        
        UpdateModeDisplay();
    }
    
    public void SwitchToGameMode()
    {
        if (isPrepareMode)
        {
            isPrepareMode = false;
            
            // Скидаємо вибір з усіх карт
            Card[] allCards = FindObjectsByType<Card>(FindObjectsSortMode.None);
            foreach (Card card in allCards)
            {
                card.DeselectCard();
            }
            
            Debug.Log("Перемикнено в ігровий режим!");
        }
        else
        {
            isPrepareMode = true;
            Debug.Log("Перемикнено в режим підготовки!");
        }
        
        UpdateModeDisplay();
    }
    
    public void SwitchToPrepareMode()
    {
        isPrepareMode = true;
        UpdateModeDisplay();
        Debug.Log("Перемикнено в режим підготовки!");
    }
    
    private void UpdateModeDisplay()
    {
        if (modeText != null)
        {
            modeText.text = isPrepareMode ? "Режим підготовки (Гра на паузі)" : "Ігровий режим";
        }
        
        // Встановлюємо паузу/знімаємо паузу
        Time.timeScale = isPrepareMode ? 0f : 1f;
    }
    
    public bool IsPrepareMode()
    {
        return isPrepareMode;
    }
    
    public bool IsGameMode()
    {
        return !isPrepareMode;
    }
}