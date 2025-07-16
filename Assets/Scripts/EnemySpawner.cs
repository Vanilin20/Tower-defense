using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Місця для спавну")]
    public List<Transform> spawnPoints;

    [Header("Префаби ворогів")]
    public List<GameObject> enemyPrefabs;

    [Header("Налаштування")]
    public float spawnInterval = 3f;
    private float timer;

    void Start()
    {
        timer = spawnInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Count == 0 || enemyPrefabs.Count == 0) return;

        // Випадкове місце і випадковий ворог
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        Instantiate(prefab, point.position, Quaternion.identity);
    }
}
