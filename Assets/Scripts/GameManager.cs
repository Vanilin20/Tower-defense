using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button startGameButton;

    [Header("Game State")]
    public bool isGameActive = false;
    public bool isGamePaused = true; // Гра на паузі з самого початку

    [Header("Camera Settings")]
    public Camera gameCamera;
    public Transform gameModePosition; // Позиція камери в ігровому режимі
    public Transform setupModePosition; // Позиція камери в режимі налаштування
    public float cameraTransitionSpeed = 2f; // Швидкість переходу камери

    [Header("Card Panel Settings")]
    public Transform cardPanel; // Панель з картками
    public Transform cardPanelGamePosition; // Позиція панелі в ігровому режимі
    public Transform cardPanelSetupPosition; // Позиція панелі в режимі налаштування
    public float panelTransitionSpeed = 2f; // Швидкість переходу панелі

    private CardDragSpawner[] allCards;
    private Vector3 targetCameraPosition;
    private Quaternion targetCameraRotation;
    private bool isCameraMoving = false;
    
    private Vector3 targetPanelPosition;
    private Quaternion targetPanelRotation;
    private bool isPanelMoving = false;

    void Start()
    {
        // Ставимо гру на паузу з самого початку
        Time.timeScale = 0f;
        isGamePaused = true;
        
        // Знаходимо всі карти на сцені
        allCards = FindObjectsByType<CardDragSpawner>(FindObjectsSortMode.None);

        // Знаходимо камеру, якщо не вказана
        if (gameCamera == null)
            gameCamera = Camera.main;

        // Встановлюємо початкову позицію камери (режим налаштування)
        if (setupModePosition != null && gameCamera != null)
        {
            gameCamera.transform.position = setupModePosition.position;
            gameCamera.transform.rotation = setupModePosition.rotation;
        }

        // Встановлюємо початкову позицію панелі (режим налаштування)
        if (cardPanelSetupPosition != null && cardPanel != null)
        {
            cardPanel.position = cardPanelSetupPosition.position;
            cardPanel.rotation = cardPanelSetupPosition.rotation;
        }

        // Налаштовуємо кнопку
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        // Початково гра не активна
        UpdateGameState();
        
        Debug.Log("🎮 Гра на паузі. Натисніть 'Почати гру' для початку");
    }

    void Update()
    {
        // Плавний перехід камери
        if (isCameraMoving && gameCamera != null)
        {
            gameCamera.transform.position = Vector3.Lerp(gameCamera.transform.position, targetCameraPosition, cameraTransitionSpeed * Time.unscaledDeltaTime);
            gameCamera.transform.rotation = Quaternion.Lerp(gameCamera.transform.rotation, targetCameraRotation, cameraTransitionSpeed * Time.unscaledDeltaTime);

            // Перевіряємо, чи досягли цільової позиції
            if (Vector3.Distance(gameCamera.transform.position, targetCameraPosition) < 0.1f)
            {
                gameCamera.transform.position = targetCameraPosition;
                gameCamera.transform.rotation = targetCameraRotation;
                isCameraMoving = false;
            }
        }

        // Плавний перехід панелі з картками
        if (isPanelMoving && cardPanel != null)
        {
            cardPanel.position = Vector3.Lerp(cardPanel.position, targetPanelPosition, panelTransitionSpeed * Time.unscaledDeltaTime);
            cardPanel.rotation = Quaternion.Lerp(cardPanel.rotation, targetPanelRotation, panelTransitionSpeed * Time.unscaledDeltaTime);

            // Перевіряємо, чи досягли цільової позиції
            if (Vector3.Distance(cardPanel.position, targetPanelPosition) < 0.1f)
            {
                cardPanel.position = targetPanelPosition;
                cardPanel.rotation = targetPanelRotation;
                isPanelMoving = false;
            }
        }
    }

    public void StartGame()
    {
        isGameActive = true;
        isGamePaused = false;
        
        // Знімаємо гру з паузи
        Time.timeScale = 1f;
        
        Debug.Log("🎮 Гра почалася! Карти в ігровому режимі");

        // Переводимо всі карти в ігровий режим
        foreach (var card in allCards)
        {
            card.StartGame();
        }

        // Змінюємо позицію камери та панелі на ігрову
        MoveCameraToGameMode();
        MovePanelToGameMode();

        UpdateGameState();
    }

    public void StopGame()
    {
        isGameActive = false;
        isGamePaused = true;
        
        // Ставимо гру на паузу
        Time.timeScale = 0f;
        
        Debug.Log("⏹️ Гра зупинена! Карти в режимі вибору");

        // Переводимо всі карти в режим вибору
        foreach (var card in allCards)
        {
            card.StopGame();
        }

        // Змінюємо позицію камери та панелі на режим налаштування
        MoveCameraToSetupMode();
        MovePanelToSetupMode();

        UpdateGameState();
    }

    private void MoveCameraToGameMode()
    {
        if (gameModePosition != null && gameCamera != null)
        {
            targetCameraPosition = gameModePosition.position;
            targetCameraRotation = gameModePosition.rotation;
            isCameraMoving = true;
            Debug.Log("📷 Камера переходить в ігровий режим");
        }
    }

    private void MoveCameraToSetupMode()
    {
        if (setupModePosition != null && gameCamera != null)
        {
            targetCameraPosition = setupModePosition.position;
            targetCameraRotation = setupModePosition.rotation;
            isCameraMoving = true;
            Debug.Log("📷 Камера переходить в режим налаштування");
        }
    }

    private void MovePanelToGameMode()
    {
        if (cardPanelGamePosition != null && cardPanel != null)
        {
            targetPanelPosition = cardPanelGamePosition.position;
            targetPanelRotation = cardPanelGamePosition.rotation;
            isPanelMoving = true;
            Debug.Log("🃏 Панель з картками переходить в ігровий режим");
        }
    }

    private void MovePanelToSetupMode()
    {
        if (cardPanelSetupPosition != null && cardPanel != null)
        {
            targetPanelPosition = cardPanelSetupPosition.position;
            targetPanelRotation = cardPanelSetupPosition.rotation;
            isPanelMoving = true;
            Debug.Log("🃏 Панель з картками переходить в режим налаштування");
        }
    }

    private void UpdateGameState()
    {
        // Оновлюємо стан кнопки
        if (startGameButton != null)
            startGameButton.interactable = !isGameActive;
    }

    // Метод для перевірки стану гри (можна викликати з інших скриптів)
    public bool IsGameActive()
    {
        return isGameActive;
    }

    // Метод для перевірки чи гра на паузі
    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    // Метод для ручного встановлення позиції камери в ігровому режимі
    public void SetGameCameraPosition(Vector3 position, Vector3 rotation)
    {
        if (gameModePosition == null)
        {
            GameObject tempObj = new GameObject("GameModePosition");
            gameModePosition = tempObj.transform;
        }
        
        gameModePosition.position = position;
        gameModePosition.rotation = Quaternion.Euler(rotation);
    }

    // Метод для ручного встановлення позиції камери в режимі налаштування
    public void SetSetupCameraPosition(Vector3 position, Vector3 rotation)
    {
        if (setupModePosition == null)
        {
            GameObject tempObj = new GameObject("SetupModePosition");
            setupModePosition = tempObj.transform;
        }
        
        setupModePosition.position = position;
        setupModePosition.rotation = Quaternion.Euler(rotation);
    }

    // Методи для ручного встановлення позицій панелі
    public void SetCardPanelGamePosition(Vector3 position, Vector3 rotation)
    {
        if (cardPanelGamePosition == null)
        {
            GameObject tempObj = new GameObject("CardPanelGamePosition");
            cardPanelGamePosition = tempObj.transform;
        }
        
        cardPanelGamePosition.position = position;
        cardPanelGamePosition.rotation = Quaternion.Euler(rotation);
    }

    public void SetCardPanelSetupPosition(Vector3 position, Vector3 rotation)
    {
        if (cardPanelSetupPosition == null)
        {
            GameObject tempObj = new GameObject("CardPanelSetupPosition");
            cardPanelSetupPosition = tempObj.transform;
        }
        
        cardPanelSetupPosition.position = position;
        cardPanelSetupPosition.rotation = Quaternion.Euler(rotation);
    }
}