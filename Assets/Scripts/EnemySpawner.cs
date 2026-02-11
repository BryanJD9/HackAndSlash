using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float respawnDelay = 2f;
    public Transform spawnPoint;

    private bool isRespawning = false; // Safety flag

    private void Start()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        isRespawning = false; // Reset the flag
        GameObject newEnemyObj = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        Enemy enemyComponent = newEnemyObj.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.OnEnemyDeath += HandleEnemyDeath;
        }
    }

    private void HandleEnemyDeath()
    {
        // Only start the timer if we aren't already waiting for a respawn
        if (!isRespawning)
        {
            isRespawning = true;
            StartCoroutine(RespawnTimer());
        }
    }

    private IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnEnemy();
    }
}