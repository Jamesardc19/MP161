using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FloorLevelManager : MonoBehaviour
{
    [System.Serializable]
    public class FloorData
    {
        public string floorName;
        public Transform floorTransform; // Reference to the floor parent in hierarchy
        public int totalItemsOnFloor; // Will be calculated automatically
        [HideInInspector]
        public int itemsCollected = 0;
        public bool floorCompleted = false;
    }

    public List<FloorData> floors = new List<FloorData>();
    
    [Header("References")]
    public CharacterStats playerStats;
    public UIManager uiManager;
    
    [Header("Level Up Settings")]
    public float levelUpNotificationDuration = 3f;
    public AudioClip levelUpSound;
    
    private Inventory inventory;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Add an audio source component if one doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        // Find references if not set in inspector
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<CharacterStats>();
        }
        
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
        
        // Get inventory reference
        GameObject inventoryObj = GameObject.FindWithTag("InventoryManager");
        if (inventoryObj != null)
        {
            inventory = inventoryObj.GetComponent<Inventory>();
            
            // Subscribe to inventory changes
            if (inventory != null)
            {
                inventory.onInventoryChanged += OnInventoryChanged;
            }
        }
        
        // Count items on each floor
        CountItemsOnFloors();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from inventory changes
        if (inventory != null)
        {
            inventory.onInventoryChanged -= OnInventoryChanged;
        }
    }
    
    // Count all items on each floor at start
    private void CountItemsOnFloors()
    {
        foreach (FloorData floor in floors)
        {
            if (floor.floorTransform != null)
            {
                // Count all ItemPickup components that are children of this floor
                ItemPickup[] floorItems = floor.floorTransform.GetComponentsInChildren<ItemPickup>();
                floor.totalItemsOnFloor = floorItems.Length;
                
                Debug.Log($"Floor {floor.floorName} has {floor.totalItemsOnFloor} items to collect");
            }
            else
            {
                Debug.LogWarning($"Floor {floor.floorName} has no transform reference!");
            }
        }
    }
    
    // Called when inventory changes (item collected)
    private void OnInventoryChanged()
    {
        // Check if we need to update any floor progress
        CheckFloorProgress();
    }
    
    // Check if we've completed any floors
    private void CheckFloorProgress()
    {
        foreach (FloorData floor in floors)
        {
            if (floor.floorCompleted)
                continue; // Skip already completed floors
                
            if (floor.floorTransform != null)
            {
                // Get all items that were on this floor
                ItemPickup[] floorItems = floor.floorTransform.GetComponentsInChildren<ItemPickup>(true);
                
                // Count remaining items (those that haven't been collected yet)
                int remainingItems = floorItems.Length;
                
                // Calculate collected items
                floor.itemsCollected = floor.totalItemsOnFloor - remainingItems;
                
                // Check if all items on this floor have been collected
                if (floor.itemsCollected >= floor.totalItemsOnFloor && floor.totalItemsOnFloor > 0)
                {
                    // Floor completed!
                    floor.floorCompleted = true;
                    
                    // Level up the player
                    LevelUpPlayer(floor.floorName);
                }
            }
        }
    }
    
    // Level up the player when a floor is completed
    private void LevelUpPlayer(string floorName)
    {
        if (playerStats != null)
        {
            // Increment level but don't distribute stats automatically
            // Instead, show the stat distribution UI
            playerStats.level++;
            
            Debug.Log($"Player leveled up after completing floor {floorName}! New level: {playerStats.level}");
            
            // Play level up sound
            if (audioSource != null && levelUpSound != null)
            {
                audioSource.PlayOneShot(levelUpSound);
            }
            
            // Show level up notification and stat distribution UI
            if (uiManager != null)
            {
                // Show notification using existing system
                uiManager.ShowPickupNotification($"LEVEL UP! Completed {floorName}!");
                
                // Show stat distribution UI
                uiManager.ShowStatDistributionPanel();
            }
        }
    }
    
    // Get the current floor progress (for UI display)
    public string GetCurrentFloorProgress()
    {
        // Find the first incomplete floor
        FloorData currentFloor = floors.FirstOrDefault(f => !f.floorCompleted);
        
        if (currentFloor != null)
        {
            return $"{currentFloor.floorName}: {currentFloor.itemsCollected}/{currentFloor.totalItemsOnFloor} items";
        }
        
        return "All floors completed!";
    }
}
