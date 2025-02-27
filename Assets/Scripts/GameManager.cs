using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public  int score;
    public GameObject enemyPrefab;
    public float spawnInterval = 5;
    public bool gameStarted;
    public Transform spawnCenter;
    public float spawnRadius = 10;
    public int spawnCount = 1;
    
    public static GameManager instance;
    public TMP_Text scoreText;
    
    // Track active enemies
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        instance = this;
        
        // Initial spawn when game starts
        if (gameStarted)
        {
            SpawnEnemies();
        }
    }

    public void PlayerDied()
    {
        foreach (var enemy in activeEnemies)
        {
            Destroy(enemy);

        }
        activeEnemies.Clear();
        score = 0;
        scoreText.text = "Score:\n" + score;
        spawnCount = 1;
        gameStarted = false;
    }
    void Update()
    {
        // Check if all enemies are destroyed and game is running
        if (gameStarted && activeEnemies.Count == 0)
        {
            SpawnEnemies();
        }
    }

    public void EnemyKilled(GameObject enemy)
    {
        score++;
        scoreText.text = "Score:\n" + score;
        
        // Remove from active enemies list
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    void SpawnEnemies()
    {
        if (gameStarted)
        {
            // Generate a random rotation offset for the entire pattern
            float patternRotation = Random.Range(0f, 360f);
            
            // Calculate angle between spawns based on current spawn count
            float angleStep = 360f / spawnCount;
            
            // Spawn enemies in a geometric pattern (line, triangle, square, pentagon, etc.)
            for (int i = 0; i < spawnCount; i++)
            {
                // Calculate position on the circle with the random rotation offset
                float angle = (i * angleStep) + patternRotation;
                Vector3 spawnPos = spawnCenter.position + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius,
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius
                );
                
                // Instantiate enemy
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                
                // Make enemy face the center
                if (enemy.transform.position != spawnCenter.position)
                {
                    Vector3 direction = spawnCenter.position - enemy.transform.position;
                    enemy.transform.rotation = Quaternion.LookRotation(direction);
                }
                
                // Add to active enemies list
                activeEnemies.Add(enemy);
            }
            
            // Increase spawn count for next wave to create more complex patterns
            spawnCount++;
        }
    }
}