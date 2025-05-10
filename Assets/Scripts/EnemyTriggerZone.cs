using UnityEngine;
using System.Collections;

public class EnemyTriggerZone : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public bool spawnOnTriggerEnter = true;
    public bool activateExistingEnemy = false;
    public string targetFloorName = "2nd Floor";
    public string targetLevelName = "Level 2";
    
    [Header("References")]
    public FloorLevelManager floorManager;
    
    private GameObject spawnedEnemy;
    private bool hasTriggered = false;
    
    private void Start()
    {
        // Find FloorLevelManager if not assigned
        if (floorManager == null)
        {
            floorManager = FindObjectOfType<FloorLevelManager>();
        }
        
        // If we're activating an existing enemy, find it
        if (activateExistingEnemy && enemyPrefab != null)
        {
            // Try to find an existing enemy with the same name as the prefab
            spawnedEnemy = GameObject.Find(enemyPrefab.name);
            
            if (spawnedEnemy != null)
            {
                // Ensure it's inactive at start
                EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.SetActive(false);
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player entering the trigger
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // Check if we're on the correct floor/level
            if (IsTargetFloorOrLevel())
            {
                if (spawnOnTriggerEnter)
                {
                    SpawnOrActivateEnemy();
                    hasTriggered = true;
                }
            }
        }
    }
    
    private bool IsTargetFloorOrLevel()
    {
        // If no floor manager, assume we're on the correct floor
        if (floorManager == null) return true;
        
        // Check if we're on the target floor or level
        foreach (FloorLevelManager.FloorData floor in floorManager.floors)
        {
            if (floor.floorName == targetFloorName || floor.floorName == targetLevelName)
            {
                // We found the target floor, now check if the player is on it
                // This is a simple implementation - you might need to adjust based on your specific setup
                return true;
            }
        }
        
        return false;
    }
    
    private void SpawnOrActivateEnemy()
    {
        if (activateExistingEnemy && spawnedEnemy != null)
        {
            // Activate existing enemy
            EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.SetActive(true);
                Debug.Log($"Activated existing enemy: {spawnedEnemy.name}");
            }
        }
        else if (enemyPrefab != null)
        {
            // Spawn new enemy
            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            spawnedEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            
            // Set up enemy components
            EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                enemyController = spawnedEnemy.AddComponent<EnemyController>();
            }
            
            EnemyStats enemyStats = spawnedEnemy.GetComponent<EnemyStats>();
            if (enemyStats == null)
            {
                enemyStats = spawnedEnemy.AddComponent<EnemyStats>();
            }
            
            // Ensure the enemy has a health manager
            HealthManager healthManager = spawnedEnemy.GetComponent<HealthManager>();
            if (healthManager == null)
            {
                healthManager = spawnedEnemy.AddComponent<HealthManager>();
                healthManager.maxHealth = enemyStats.health;
                healthManager.currentHealth = enemyStats.health;
            }
            
            // Activate the enemy
            enemyController.SetActive(true);
            
            Debug.Log($"Spawned new enemy: {spawnedEnemy.name} at {position}");
        }
    }
    
    // Method to manually trigger the enemy spawn/activation
    public void ManualTrigger()
    {
        if (!hasTriggered && IsTargetFloorOrLevel())
        {
            SpawnOrActivateEnemy();
            hasTriggered = true;
        }
    }
    
    // Reset the trigger
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
