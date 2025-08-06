// Спавнер ворогів
using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPoint
    {
        public Transform spawnTransform;
        public bool isActive = true;
        public string pointName;
    }

    [Header("Налаштування спавну")]
    public GameObject[] enemyPrefabs; // Масив префабів ворогів
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>(); // Точки спавну
    public float spawnInterval = 3f; // Інтервал між спавнами
    public int maxEnemiesOnMap = 10; // Максимальна кількість ворогів на карті
    public bool autoSpawn = true; // Автоматичний спавн
    
    [Header("Хвильовий спавн")]
    public bool useWaves = false;
    public int enemiesPerWave = 5;
    public float timeBetweenWaves = 10f;
    
    private float lastSpawnTime;
    private int currentEnemyCount = 0;
    private int currentWave = 1;
    private int enemiesSpawnedThisWave = 0;
    private bool waveInProgress = false;

    private void Start()
    {
        // Ініціалізуємо точки спавну якщо вони не задані
        if (spawnPoints.Count == 0)
        {
            SpawnPoint defaultPoint = new SpawnPoint();
            defaultPoint.spawnTransform = transform;
            defaultPoint.pointName = "Default Spawn";
            spawnPoints.Add(defaultPoint);
        }
    }

    private void Update()
    {
        if (!autoSpawn) return;

        // Підраховуємо поточну кількість ворогів
        currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (useWaves)
        {
            HandleWaveSpawning();
        }
        else
        {
            HandleContinuousSpawning();
        }
    }

    private void HandleContinuousSpawning()
    {
        if (Time.time - lastSpawnTime >= spawnInterval && currentEnemyCount < maxEnemiesOnMap)
        {
            SpawnRandomEnemy();
            lastSpawnTime = Time.time;
        }
    }

    private void HandleWaveSpawning()
    {
        if (!waveInProgress && Time.time - lastSpawnTime >= timeBetweenWaves)
        {
            StartWave();
        }

        if (waveInProgress && enemiesSpawnedThisWave < enemiesPerWave && 
            Time.time - lastSpawnTime >= spawnInterval)
        {
            SpawnRandomEnemy();
            enemiesSpawnedThisWave++;
            lastSpawnTime = Time.time;

            if (enemiesSpawnedThisWave >= enemiesPerWave)
            {
                EndWave();
            }
        }
    }

    private void StartWave()
    {
        waveInProgress = true;
        enemiesSpawnedThisWave = 0;
        Debug.Log($"Розпочинається хвиля {currentWave}!");
    }

    private void EndWave()
    {
        waveInProgress = false;
        currentWave++;
        lastSpawnTime = Time.time;
        Debug.Log($"Хвиля {currentWave - 1} завершена! Наступна хвиля через {timeBetweenWaves} секунд.");
    }

    public void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Count == 0) return;

        // Вибираємо випадкового ворога
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        // Вибираємо активну точку спавну
        List<SpawnPoint> activePoints = spawnPoints.FindAll(point => point.isActive);
        if (activePoints.Count == 0) return;

        SpawnPoint selectedPoint = activePoints[Random.Range(0, activePoints.Count)];
        
        // Створюємо ворога
        GameObject newEnemy = Instantiate(enemyPrefab, selectedPoint.spawnTransform.position, 
                                        selectedPoint.spawnTransform.rotation);
        
        // Додаємо тег якщо його немає
        if (newEnemy.tag != "Enemy")
        {
            newEnemy.tag = "Enemy";
        }

        Debug.Log($"Заспавнено ворога в точці: {selectedPoint.pointName}");
    }

    public void SpawnEnemyAtPoint(int pointIndex, int enemyIndex = -1)
    {
        if (pointIndex < 0 || pointIndex >= spawnPoints.Count) return;
        if (!spawnPoints[pointIndex].isActive) return;

        GameObject enemyPrefab;
        if (enemyIndex >= 0 && enemyIndex < enemyPrefabs.Length)
        {
            enemyPrefab = enemyPrefabs[enemyIndex];
        }
        else
        {
            enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoints[pointIndex].spawnTransform.position, 
                                        spawnPoints[pointIndex].spawnTransform.rotation);
        
        if (newEnemy.tag != "Enemy")
        {
            newEnemy.tag = "Enemy";
        }
    }

    public void ToggleSpawnPoint(int index)
    {
        if (index >= 0 && index < spawnPoints.Count)
        {
            spawnPoints[index].isActive = !spawnPoints[index].isActive;
        }
    }

    public void SetAutoSpawn(bool enabled)
    {
        autoSpawn = enabled;
    }

    // Метод для ручного спавну (можна викликати з UI)
    public void ManualSpawn()
    {
        if (currentEnemyCount < maxEnemiesOnMap)
        {
            SpawnRandomEnemy();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Показуємо точки спавну в редакторі
        foreach (SpawnPoint point in spawnPoints)
        {
            if (point.spawnTransform != null)
            {
                Gizmos.color = point.isActive ? Color.green : Color.red;
                Gizmos.DrawWireSphere(point.spawnTransform.position, 0.5f);
                
                // Показуємо напрямок (вліво)
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(point.spawnTransform.position, 
                              point.spawnTransform.position + Vector3.left * 2f);
            }
        }
    }
}
