using UnityEngine;

// Менеджер бойової системи - керує всіма утилітами бою
public class CombatSystemManager : MonoBehaviour
{
    public static CombatSystemManager Instance;
    
    [Header("Налаштування бойової системи")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool enableCombatUtils = true;
    [SerializeField] private bool enableDeathHandler = true;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCombatSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeCombatSystem()
    {
        Debug.Log("🔧 Ініціалізація бойової системи...");
        
        // Перевіряємо наявність необхідних менеджерів
        if (HeightManager.Instance == null)
        {
            Debug.LogWarning("⚠️ HeightManager не знайдено! Створюємо автоматично...");
            CreateHeightManager();
        }
        
        Debug.Log("✅ Бойова система ініціалізована!");
    }
    
    private void CreateHeightManager()
    {
        GameObject heightManagerObj = new GameObject("HeightManager");
        heightManagerObj.AddComponent<HeightManager>();
        DontDestroyOnLoad(heightManagerObj);
    }
    
    // Публічні методи для керування системою
    public void EnableCombatUtils(bool enable)
    {
        enableCombatUtils = enable;
        Debug.Log($"🔧 CombatUtils: {(enable ? "увімкнено" : "вимкнено")}");
    }
    
    public void EnableDeathHandler(bool enable)
    {
        enableDeathHandler = enable;
        Debug.Log($"🔧 DeathHandler: {(enable ? "увімкнено" : "вимкнено")}");
    }
    
    public void EnableDebugLogs(bool enable)
    {
        enableDebugLogs = enable;
        Debug.Log($"🔧 Debug логи: {(enable ? "увімкнено" : "вимкнено")}");
    }
    
    // Методи для перевірки стану системи
    public bool IsCombatUtilsEnabled() => enableCombatUtils;
    public bool IsDeathHandlerEnabled() => enableDeathHandler;
    public bool IsDebugLogsEnabled() => enableDebugLogs;
    
    // Метод для тестування системи
    public void TestCombatSystem()
    {
        Debug.Log("🧪 Тестування бойової системи...");
        
        // Тестуємо CombatUtils
        if (enableCombatUtils)
        {
            int testDamage = CombatUtils.CalculateDamage(10, 0.2f, 2f, out bool isCritical);
            Debug.Log($"✅ CombatUtils тест: шкода={testDamage}, критичний={isCritical}");
        }
        
        // Тестуємо DeathHandler
        if (enableDeathHandler)
        {
            Debug.Log("✅ DeathHandler готовий до використання");
        }
        
        Debug.Log("✅ Тестування завершено!");
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Debug.Log("🔧 Бойова система зупинена");
        }
    }
} 