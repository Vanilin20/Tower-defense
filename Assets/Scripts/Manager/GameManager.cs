using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI елементи")]
    public Button readyButton;
    public Text modeText;

    [Header("Налаштування")]
    public bool isPrepareMode = true;

    [Header("Об'єкти для переміщення")]
    public Transform worldObject;           // Наприклад, гравець або предмет на сцені
    public Vector3 worldTargetPosition;

    public RectTransform uiObject;          // UI-елемент (панель, картка тощо)
    public Vector2 uiTargetPosition;        // Для UI потрібен Vector2 (anchoredPosition)

    public float moveDuration = 1f;         // Скільки часу триває переміщення

    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

            // Плавне переміщення
            if (worldObject != null)
                StartCoroutine(MoveWorldObject(worldObject, worldTargetPosition, moveDuration));

            if (uiObject != null)
                StartCoroutine(MoveUIObject(uiObject, uiTargetPosition, moveDuration));

            Debug.Log("Перемикнено в ігровий режим!");
        }
        else
        {
            isPrepareMode = true;
            Debug.Log("Перемикнено в режим підготовки!");
        }

        UpdateModeDisplay();
    }

    private IEnumerator MoveWorldObject(Transform obj, Vector3 target, float duration)
    {
        Vector3 start = obj.position;
        float time = 0f;

        while (time < duration)
        {
            obj.position = Vector3.Lerp(start, target, time / duration);
            time += Time.unscaledDeltaTime; // Якщо Time.timeScale = 0
            yield return null;
        }

        obj.position = target;
    }

    private IEnumerator MoveUIObject(RectTransform rect, Vector2 target, float duration)
    {
        Vector2 start = rect.anchoredPosition;
        float time = 0f;

        while (time < duration)
        {
            rect.anchoredPosition = Vector2.Lerp(start, target, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        rect.anchoredPosition = target;
    }

    private void UpdateModeDisplay()
    {
        if (modeText != null)
        {
            modeText.text = isPrepareMode ? "Режим підготовки (Гра на паузі)" : "Ігровий режим";
        }

        Time.timeScale = isPrepareMode ? 0f : 1f;
    }

    public bool IsPrepareMode() => isPrepareMode;
    public bool IsGameMode() => !isPrepareMode;
}
