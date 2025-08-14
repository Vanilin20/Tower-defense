using UnityEngine;

/// <summary>
/// Тестовий скрипт для демонстрації системи переміщення героїв
/// </summary>
public class HeroMovementTester : MonoBehaviour
{
    [Header("Тестування")]
    public bool enableTesting = true;
    public KeyCode testKey = KeyCode.T;
    
    [Header("Демонстрація")]
    public bool showDebugInfo = true;
    
    void Update()
    {
        if (!enableTesting) return;
        
        if (Input.GetKeyDown(testKey))
        {
            TestHeroMovementSystem();
        }
    }
    
    /// <summary>
    /// Тестує систему переміщення героїв
    /// </summary>
    public void TestHeroMovementSystem()
    {
        Debug.Log("🧪 === ТЕСТУВАННЯ СИСТЕМИ ПЕРЕМІЩЕННЯ ГЕРОЇВ ===");
        
        // Тест 1: Пошук карт для переміщення
        TestFindMovementCards();
        
        // Тест 2: Пошук зон з героями
        TestFindZonesWithHeroes();
        
        // Тест 3: Перевірка стану переміщення
        TestMovementStates();
        
        Debug.Log("🧪 === ТЕСТУВАННЯ ЗАВЕРШЕНО ===");
    }
    
    /// <summary>
    /// Тестує пошук карт для переміщення
    /// </summary>
    private void TestFindMovementCards()
    {
        Debug.Log("🔍 Тест 1: Пошук карт для переміщення");
        
        Card[] allCards = FindObjectsByType<Card>(FindObjectsSortMode.None);
        int movementCards = 0;
        
        foreach (Card card in allCards)
        {
            if (card.IsSelectedForMovement())
            {
                movementCards++;
                Debug.Log($"  ✅ Карта {card.gameObject.name} готова до переміщення");
                Debug.Log($"     Інформація: {card.GetMovementInfo()}");
            }
        }
        
        if (movementCards == 0)
        {
            Debug.Log("  ℹ️ Карт для переміщення не знайдено");
        }
        else
        {
            Debug.Log($"  📊 Знайдено {movementCards} карт для переміщення");
        }
    }
    
    /// <summary>
    /// Тестує пошук зон з героями
    /// </summary>
    private void TestFindZonesWithHeroes()
    {
        Debug.Log("🔍 Тест 2: Пошук зон з героями");
        
        Zone[] allZones = FindObjectsByType<Zone>(FindObjectsSortMode.None);
        int zonesWithHeroes = 0;
        
        foreach (Zone zone in allZones)
        {
            if (zone.HasHero())
            {
                zonesWithHeroes++;
                GameObject hero = zone.GetCurrentHero();
                Debug.Log($"  ✅ Зона {zone.name} зайнята героєм {hero?.name ?? "невідомий"}");
            }
        }
        
        if (zonesWithHeroes == 0)
        {
            Debug.Log("  ℹ️ Зон з героями не знайдено");
        }
        else
        {
            Debug.Log($"  📊 Знайдено {zonesWithHeroes} зон з героями");
        }
    }
    
    /// <summary>
    /// Тестує стани переміщення
    /// </summary>
    private void TestMovementStates()
    {
        Debug.Log("🔍 Тест 3: Перевірка станів переміщення");
        
        Card[] allCards = FindObjectsByType<Card>(FindObjectsSortMode.None);
        
        foreach (Card card in allCards)
        {
            if (card.IsInSlot())
            {
                string status = card.IsSelectedForMovement() ? "ГОТОВА ДО ПЕРЕМІЩЕННЯ" : "в слоті";
                Debug.Log($"  📋 Карта {card.gameObject.name}: {status}");
                
                if (card.IsSelectedForMovement())
                {
                    Debug.Log($"     Деталі: {card.GetMovementInfo()}");
                }
            }
        }
    }
    
    /// <summary>
    /// Публічний метод для зовнішнього виклику
    /// </summary>
    public void ForceTest()
    {
        TestHeroMovementSystem();
    }
    
    /// <summary>
    /// Показує загальну статистику
    /// </summary>
    public void ShowStatistics()
    {
        if (!showDebugInfo) return;
        
        Card[] allCards = FindObjectsByType<Card>(FindObjectsSortMode.None);
        Zone[] allZones = FindObjectsByType<Zone>(FindObjectsSortMode.None);
        
        int cardsInSlots = 0;
        int cardsForMovement = 0;
        int zonesWithHeroes = 0;
        int freeZones = 0;
        
        foreach (Card card in allCards)
        {
            if (card.IsInSlot()) cardsInSlots++;
            if (card.IsSelectedForMovement()) cardsForMovement++;
        }
        
        foreach (Zone zone in allZones)
        {
            if (zone.HasHero()) zonesWithHeroes++;
            else freeZones++;
        }
        
        Debug.Log($"📊 СТАТИСТИКА: Карти в слотах: {cardsInSlots}, Готові до переміщення: {cardsForMovement}");
        Debug.Log($"📊 СТАТИСТИКА: Зони з героями: {zonesWithHeroes}, Вільні зони: {freeZones}");
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("🧪 Hero Movement Tester", GUI.skin.box);
        
        if (GUILayout.Button("Тестувати систему"))
        {
            TestHeroMovementSystem();
        }
        
        if (GUILayout.Button("Показати статистику"))
        {
            ShowStatistics();
        }
        
        GUILayout.Label($"Натисніть {testKey} для тестування");
        GUILayout.EndArea();
    }
} 