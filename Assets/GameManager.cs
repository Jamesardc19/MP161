using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameManager Instance { get; private set; }

    // References to other managers
    [Header("References")]
    public UIManager uiManager;
    public PauseMenuManager pauseMenuManager;
    private MonoBehaviour inventoryComponent;
    
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenuScene";
    public string gameSceneName = "GameScene";

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Initialize references if needed
        FindReferences();
    }

    // Find references to other managers in the scene
    public void FindReferences()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }

        if (pauseMenuManager == null)
        {
            pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        }
        
        // Find the inventory component
        if (inventoryComponent == null)
        {
            GameObject inventoryObject = GameObject.FindWithTag("InventoryManager");
            if (inventoryObject != null)
            {
                inventoryComponent = inventoryObject.GetComponent<MonoBehaviour>();
            }
        }
    }

    // Save the current game state
    public void SaveGame()
    {
        // Save current scene
        PlayerPrefs.SetString("SavedGameScene", SceneManager.GetActiveScene().name);
        
        // Set a flag indicating that a saved game exists
        PlayerPrefs.SetInt("SavedGame", 1);
        
        // Save player position if available
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerPosX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerPosY", player.transform.position.y);
            PlayerPrefs.SetFloat("PlayerPosZ", player.transform.position.z);
        }
        
        // Save player health if available
        HealthManager healthManager = player?.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            PlayerPrefs.SetFloat("PlayerHealth", healthManager.currentHealth);
            PlayerPrefs.SetFloat("PlayerMaxHealth", healthManager.maxHealth);
        }
        
        // Save player stats if available
        CharacterStats characterStats = player?.GetComponent<CharacterStats>();
        if (characterStats != null)
        {
            PlayerPrefs.SetInt("PlayerLevel", characterStats.level);
            PlayerPrefs.SetInt("PlayerAttack", characterStats.attack);
            PlayerPrefs.SetInt("PlayerDefense", characterStats.defense);
            PlayerPrefs.SetInt("PlayerSpeed", characterStats.speed);
        }
        
        // Save inventory data if available
        SaveInventoryData();
        
        // Save all PlayerPrefs changes to disk
        PlayerPrefs.Save();
        
        Debug.Log("Game saved successfully!");
    }
    
    // Save inventory data using reflection
    private void SaveInventoryData()
    {
        if (inventoryComponent != null)
        {
            // Try to find the SaveInventory method on the inventory component
            var saveMethod = inventoryComponent.GetType().GetMethod("SaveInventory");
            if (saveMethod != null)
            {
                // Call the SaveInventory method
                saveMethod.Invoke(inventoryComponent, null);
                Debug.Log("Inventory data saved successfully!");
            }
            else
            {
                // If no SaveInventory method exists, try to save the items manually
                var itemsField = inventoryComponent.GetType().GetField("items");
                if (itemsField != null)
                {
                    var items = itemsField.GetValue(inventoryComponent) as System.Collections.ICollection;
                    if (items != null)
                    {
                        // Save the number of items
                        PlayerPrefs.SetInt("InventoryCount", items.Count);
                        
                        // Save each item by name
                        int index = 0;
                        foreach (var item in items)
                        {
                            var nameProperty = item.GetType().GetProperty("name");
                            if (nameProperty != null)
                            {
                                string itemName = nameProperty.GetValue(item) as string;
                                if (!string.IsNullOrEmpty(itemName))
                                {
                                    PlayerPrefs.SetString("InventoryItem_" + index, itemName);
                                    index++;
                                }
                            }
                        }
                        
                        Debug.Log("Inventory data saved manually: " + items.Count + " items");
                    }
                }
            }
        }
    }

    // Load a saved game
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("SavedGame"))
        {
            // Load the saved scene
            string savedScene = PlayerPrefs.GetString("SavedGameScene", gameSceneName);
            
            // If we're not already in the saved scene, load it
            if (SceneManager.GetActiveScene().name != savedScene)
            {
                SceneManager.LoadScene(savedScene);
            }
            
            // We need to wait for the scene to load before restoring player state
            // This is handled in the OnSceneLoaded method
        }
        else
        {
            Debug.LogWarning("No saved game found!");
        }
    }

    // Called after a scene is loaded
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find references in the new scene
        FindReferences();
        
        // If this is a load operation, restore player state
        if (PlayerPrefs.HasKey("SavedGame") && scene.name == PlayerPrefs.GetString("SavedGameScene", gameSceneName))
        {
            RestorePlayerState();
        }
    }

    // Restore player state from saved data
    private void RestorePlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && PlayerPrefs.HasKey("PlayerPosX"))
        {
            // Restore position
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            float z = PlayerPrefs.GetFloat("PlayerPosZ");
            player.transform.position = new Vector3(x, y, z);
            
            // Restore health
            HealthManager healthManager = player.GetComponent<HealthManager>();
            if (healthManager != null && PlayerPrefs.HasKey("PlayerHealth"))
            {
                healthManager.maxHealth = (int)PlayerPrefs.GetFloat("PlayerMaxHealth");
                healthManager.currentHealth = (int)PlayerPrefs.GetFloat("PlayerHealth");
                healthManager.RefreshHealthBar();
            }
            
            // Restore stats
            CharacterStats characterStats = player.GetComponent<CharacterStats>();
            if (characterStats != null && PlayerPrefs.HasKey("PlayerLevel"))
            {
                characterStats.level = PlayerPrefs.GetInt("PlayerLevel");
                characterStats.attack = PlayerPrefs.GetInt("PlayerAttack");
                characterStats.defense = PlayerPrefs.GetInt("PlayerDefense");
                characterStats.speed = PlayerPrefs.GetInt("PlayerSpeed");
                characterStats.UpdateStatsUI();
            }
            
            // Restore inventory data
            LoadInventoryData();
            
            Debug.Log("Player state restored successfully!");
        }
    }
    
    // Load inventory data using reflection
    private void LoadInventoryData()
    {
        // Find the inventory component if not already found
        if (inventoryComponent == null)
        {
            FindReferences();
        }
        
        if (inventoryComponent != null)
        {
            // Try to find the LoadInventory method on the inventory component
            var loadMethod = inventoryComponent.GetType().GetMethod("LoadInventory");
            if (loadMethod != null)
            {
                // Call the LoadInventory method
                loadMethod.Invoke(inventoryComponent, null);
                Debug.Log("Inventory data loaded successfully!");
            }
            else
            {
                // If no LoadInventory method exists, try to load the items manually
                var itemsField = inventoryComponent.GetType().GetField("items");
                if (itemsField != null)
                {
                    // Get the current items list
                    var itemsList = itemsField.GetValue(inventoryComponent);
                    
                    // Find the Clear method on the list
                    var clearMethod = itemsList.GetType().GetMethod("Clear");
                    if (clearMethod != null)
                    {
                        // Clear the current items
                        clearMethod.Invoke(itemsList, null);
                        
                        // Get the number of saved items
                        int count = PlayerPrefs.GetInt("InventoryCount", 0);
                        
                        if (count > 0)
                        {
                            // Find the Add method on the list
                            var addMethod = itemsList.GetType().GetMethod("Add");
                            if (addMethod != null)
                            {
                                // Load each item
                                for (int i = 0; i < count; i++)
                                {
                                    string itemName = PlayerPrefs.GetString("InventoryItem_" + i, "");
                                    if (!string.IsNullOrEmpty(itemName))
                                    {
                                        // Try to load the item from Resources
                                        var resourcesType = typeof(UnityEngine.Resources);
                                        var loadMethod2 = resourcesType.GetMethod("Load", new[] { typeof(string) });
                                        if (loadMethod2 != null)
                                        {
                                            var item = loadMethod2.Invoke(null, new object[] { "Items/" + itemName });
                                            if (item != null)
                                            {
                                                addMethod.Invoke(itemsList, new[] { item });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Trigger inventory UI update
                        var onChangedField = inventoryComponent.GetType().GetField("onInventoryChanged");
                        if (onChangedField != null)
                        {
                            var onChanged = onChangedField.GetValue(inventoryComponent);
                            if (onChanged != null)
                            {
                                var invokeMethod = onChanged.GetType().GetMethod("Invoke");
                                if (invokeMethod != null)
                                {
                                    invokeMethod.Invoke(onChanged, null);
                                }
                            }
                        }
                        
                        Debug.Log("Inventory data loaded manually: " + count + " items");
                    }
                }
            }
        }
    }

    // Register for scene loading events
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Unregister from scene loading events
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
