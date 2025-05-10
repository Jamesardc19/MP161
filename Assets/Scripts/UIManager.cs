using UnityEngine;
using UnityEngine.UI; // Includes GridLayoutGroup, Button, etc.
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Health Bar")]
    public Image healthBarImage;
    public Gradient healthBarGradient;
    
    [Header("Stats Panel")]
    public GameObject statsPanel;
    public Text attackText;
    public Text defenseText;
    public Text speedText;
    public Text healthText;
    public Text levelText;
    
    [Header("References")]
    public HealthManager playerHealth;
    public CharacterStats playerStats;
    
    [Header("Inventory UI")]
    public GameObject inventoryPanel;
    public Transform inventorySlotContainer;
    public GameObject inventorySlotPrefab;
    public KeyCode inventoryToggleKey = KeyCode.I;
    
    [Header("Notifications")]
    public GameObject pickupNotification;
    public Text pickupNotificationText;
    public float notificationDuration = 2f;
    
    [Header("Stat Distribution Panel")]
    public GameObject statDistributionPanel;
    public Text availablePointsText;
    public Button healthPlusButton;
    public Button attackPlusButton;
    public Button defensePlusButton;
    public Button speedPlusButton;
    public Button confirmButton;
    
    // Toggle for stats panel visibility
    private bool statsVisible = false;
    // Toggle for inventory panel visibility
    private bool inventoryVisible = false;
    
    void Start()
    {
        // Initialize stats panel state
        if (statsPanel != null)
        {
            statsPanel.SetActive(statsVisible);
        }
        
        // Initialize inventory panel state
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(inventoryVisible);
        }
        
        // Connect health bar to player health manager
        if (playerHealth != null && healthBarImage != null)
        {
            playerHealth.healthBarImage = healthBarImage;
        }
        
        // Connect stat texts to player stats
        if (playerStats != null)
        {
            playerStats.attackText = attackText;
            playerStats.defenseText = defenseText;
            playerStats.speedText = speedText;
            playerStats.healthText = healthText;
            playerStats.levelText = levelText;
        }
        
        // Initialize notifications and panels
        if (pickupNotification != null)
        {
            pickupNotification.SetActive(false);
        }
        
        if (statDistributionPanel != null)
        {
            statDistributionPanel.SetActive(false);
        }
        
        // Initialize character movement speed based on starting stats
        UpdateCharacterMovementSpeed();
    }
    
    void Update()
    {
        // Update health bar color based on current health percentage
        if (healthBarImage != null && playerHealth != null && healthBarGradient != null)
        {
            float healthPercentage = playerHealth.GetHealthPercentage();
            healthBarImage.color = healthBarGradient.Evaluate(healthPercentage);
        }
        
        // Toggle stats panel with Tab key (can be changed to any key)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleStatsPanel();
        }
        
        // Toggle inventory panel with I key (can be changed to any key)
        if (Input.GetKeyDown(inventoryToggleKey))
        {
            ToggleInventoryPanel();
        }
    }
    
    // Toggle stats panel visibility
    public void ToggleStatsPanel()
    {
        if (statsPanel != null)
        {
            statsVisible = !statsVisible;
            statsPanel.SetActive(statsVisible);
        }
    }
    
    // Method to trigger a level up (can be called from a button or other event)
    public void TriggerLevelUp()
    {
        if (playerStats != null)
        {
            playerStats.LevelUp();
        }
    }
    
    // Toggle inventory panel visibility
    public void ToggleInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            inventoryVisible = !inventoryVisible;
            inventoryPanel.SetActive(inventoryVisible);
            
            if (inventoryVisible)
            {
                UpdateInventoryUI();
            }
        }
    }
    
    private void OnEnable()
    {
        // Find any object with an inventory component and subscribe to its events
        var inventoryObject = GameObject.FindWithTag("InventoryManager");
        if (inventoryObject != null)
        {
            var inventoryComponent = inventoryObject.GetComponent<MonoBehaviour>();
            if (inventoryComponent != null)
            {
                var eventField = inventoryComponent.GetType().GetField("onInventoryChanged");
                if (eventField != null)
                {
                    var eventDelegate = System.Delegate.CreateDelegate(
                        eventField.FieldType, this, this.GetType().GetMethod("UpdateInventoryUI"));
                    var eventValue = eventField.GetValue(inventoryComponent);
                    
                    if (eventValue != null)
                    {
                        var addMethod = eventValue.GetType().GetMethod("Add");
                        if (addMethod != null)
                        {
                            addMethod.Invoke(eventValue, new object[] { eventDelegate });
                        }
                    }
                    else
                    {
                        eventField.SetValue(inventoryComponent, eventDelegate);
                    }
                }
            }
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from inventory events
        var inventoryObject = GameObject.FindWithTag("InventoryManager");
        if (inventoryObject != null)
        {
            var inventoryComponent = inventoryObject.GetComponent<MonoBehaviour>();
            if (inventoryComponent != null)
            {
                var eventField = inventoryComponent.GetType().GetField("onInventoryChanged");
                if (eventField != null)
                {
                    var eventValue = eventField.GetValue(inventoryComponent);
                    if (eventValue != null)
                    {
                        var removeMethod = eventValue.GetType().GetMethod("Remove");
                        if (removeMethod != null)
                        {
                            var eventDelegate = System.Delegate.CreateDelegate(
                                eventField.FieldType, this, this.GetType().GetMethod("UpdateInventoryUI"));
                            removeMethod.Invoke(eventValue, new object[] { eventDelegate });
                        }
                    }
                }
            }
        }
    }
    
    // Update the inventory UI with current items
    public void UpdateInventoryUI()
    {
        if (inventorySlotContainer == null || inventorySlotPrefab == null)
        {
            Debug.LogError("Inventory slot container or prefab is null!");
            return;
        }
            
        // Clear existing slots
        foreach (Transform child in inventorySlotContainer)
        {
            Destroy(child.gameObject);
        }
        
        // IMPORTANT: Disable any Grid Layout Group component to prevent it from interfering
        GridLayoutGroup gridLayout = inventorySlotContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            // We'll disable the Grid Layout Group and position slots manually
            gridLayout.enabled = false;
            Debug.Log("Disabled Grid Layout Group to use manual positioning");
        }
        
        // Define fixed positions for inventory slots
        // These positions will be used regardless of Grid Layout Group settings
        Vector2[] slotPositions = new Vector2[3] {
            new Vector2(36, 0),     // First slot position (left)
            new Vector2(196, 0),    // Second slot position (middle) - 36 + 100 + 60 = 196
            new Vector2(356, 0)     // Third slot position (right) - 196 + 100 + 60 = 356
        };
        
        Debug.Log("Using fixed slot positions: " + slotPositions[0] + ", " + slotPositions[1] + ", " + slotPositions[2]);

        
        // Find the inventory GameObject
        var inventoryObject = GameObject.FindWithTag("InventoryManager");
        if (inventoryObject == null)
        {
            Debug.LogError("No GameObject with InventoryManager tag found!");
            return;
        }
            
        // Get the items list through reflection to avoid direct dependencies
        var inventoryComponent = inventoryObject.GetComponent<MonoBehaviour>();
        if (inventoryComponent == null)
        {
            Debug.LogError("No MonoBehaviour component found on InventoryManager!");
            return;
        }
        
        Debug.Log("Found inventory component: " + inventoryComponent.GetType().Name);
            
        var itemsField = inventoryComponent.GetType().GetField("items");
        if (itemsField == null)
        {
            Debug.LogError("No 'items' field found in inventory component!");
            return;
        }
            
        var items = itemsField.GetValue(inventoryComponent) as IEnumerable<object>;
        if (items == null)
        {
            Debug.LogError("Items collection is null or not enumerable!");
            return;
        }
        
        int itemCount = 0;
        
        // Create a slot for each item
        foreach (var itemObj in items)
        {
            itemCount++;
            GameObject slotObject = Instantiate(inventorySlotPrefab, inventorySlotContainer);
            
            // Ensure the slot has the correct size and position
            RectTransform slotRect = slotObject.GetComponent<RectTransform>();
            if (slotRect != null)
            {
                // Set fixed size
                slotRect.sizeDelta = new Vector2(100, 100);
                
                // Set fixed position based on item index
                int slotIndex = itemCount - 1;
                if (slotIndex < slotPositions.Length)
                {
                    // Set anchors to top-left for consistent positioning
                    slotRect.anchorMin = new Vector2(0, 1);
                    slotRect.anchorMax = new Vector2(0, 1);
                    slotRect.pivot = new Vector2(0.5f, 0.5f);
                    
                    // Apply the fixed position
                    slotRect.anchoredPosition = new Vector2(slotPositions[slotIndex].x + 50, -50); // +50 to center based on pivot
                    
                    Debug.Log("Set slot " + itemCount + " position to: " + slotRect.anchoredPosition);
                }
                else
                {
                    Debug.LogWarning("Too many items for predefined positions. Item " + itemCount + " may not be positioned correctly.");
                }
            }
            
            // Try to use the InventorySlot component if available
            InventorySlot inventorySlot = slotObject.GetComponent<InventorySlot>();
            if (inventorySlot != null)
            {
                // Try to cast the itemObj to Item
                Item itemComponent = itemObj as Item;
                if (itemComponent != null)
                {
                    // Directly set the name text if it exists
                    Text slotNameText = slotObject.transform.Find("Name")?.GetComponent<Text>();
                    if (slotNameText != null)
                    {
                        slotNameText.text = itemComponent.itemName;
                        Debug.Log("Directly set name text to: " + itemComponent.itemName);
                    }
                    
                    // Set the item in the inventory slot
                    inventorySlot.SetItem(itemComponent);
                    Debug.Log("Set item using InventorySlot component: " + itemComponent.itemName);
                    continue; // Skip the reflection-based approach below
                }
            }
            
            // Fallback to reflection-based approach
            // Set the item icon through reflection
            Image iconImage = slotObject.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                var iconProperty = itemObj.GetType().GetProperty("icon");
                if (iconProperty != null)
                {
                    var icon = iconProperty.GetValue(itemObj) as Sprite;
                    if (icon != null)
                    {
                        iconImage.sprite = icon;
                        iconImage.enabled = true;
                    }
                    else
                    {
                        Debug.LogWarning("Item icon is null!");
                    }
                }
                else
                {
                    Debug.LogWarning("No 'icon' property found in item!");
                }
            }
            else
            {
                Debug.LogError("No 'Icon' Image component found in slot prefab!");
            }
            
            // Set the item name through reflection
            Text nameText = slotObject.transform.Find("Name")?.GetComponent<Text>();
            if (nameText != null)
            {
                var nameProperty = itemObj.GetType().GetProperty("itemName");
                if (nameProperty != null)
                {
                    var name = nameProperty.GetValue(itemObj) as string;
                    if (name != null)
                    {
                        nameText.text = name;
                        Debug.Log("Set item name: " + name);
                    }
                }
                else
                {
                    // Try to get name as a field instead of property
                    var nameField = itemObj.GetType().GetField("itemName");
                    if (nameField != null)
                    {
                        var name = nameField.GetValue(itemObj) as string;
                        if (name != null)
                        {
                            nameText.text = name;
                            Debug.Log("Set item name from field: " + name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No 'itemName' property or field found in item!");
                        nameText.text = "Unknown Item";
                    }
                }
            }
            else
            {
                Debug.LogError("No 'Name' Text component found in slot prefab!");
            }
        }
        
        Debug.Log("Updated inventory UI with " + itemCount + " items");
    }
    
    // Show a notification when an item is picked up
    public void ShowPickupNotification(string itemName)
    {
        if (pickupNotification != null && pickupNotificationText != null)
        {
            // More descriptive message based on item type
            string message = "";
            switch (itemName)
            {
                case "Bolt":
                    message = "Bolt collected!";
                    break;
                case "Nut":
                    message = "Nut collected!";
                    break;
                case "Transistor":
                    message = "Transistor collected!";
                    break;
                default:
                    message = "Picked up: " + itemName;
                    break;
            }
            
            pickupNotificationText.text = message;
            Debug.Log("Showing notification: " + message);
            StartCoroutine(ShowNotificationCoroutine());
        }
        else
        {
            Debug.LogError("Pickup notification or text is null!");
        }
    }
    
    // Coroutine to show the notification for a set duration
    private IEnumerator ShowNotificationCoroutine()
    {
        pickupNotification.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        pickupNotification.SetActive(false);
    }
    
    // Show the stat distribution panel after leveling up
    public void ShowStatDistributionPanel()
    {
        Debug.Log("ShowStatDistributionPanel called");
        
        // Debug checks
        if (statDistributionPanel == null)
        {
            Debug.LogError("statDistributionPanel is null! Make sure it's assigned in the inspector.");
            return;
        }
        
        if (playerStats == null)
        {
            Debug.LogError("playerStats is null! Make sure it's assigned in the inspector.");
            return;
        }
        
        // Show the panel
        Debug.Log("Setting statDistributionPanel active");
        statDistributionPanel.SetActive(true);
        
        // Reset available points
        playerStats.availablePoints = playerStats.pointsPerLevel;
        Debug.Log($"Reset available points to {playerStats.availablePoints}");
        
        // Update UI
        UpdateStatDistributionUI();
        
        // Set up button listeners
        SetupStatDistributionButtons();
        
        // Pause the game while distributing stats
        Time.timeScale = 0f;
        Debug.Log("Game paused for stat distribution (Time.timeScale = 0)");
        
        Debug.Log($"Stat distribution panel should now be visible with {playerStats.availablePoints} points");
    }
    
    // Update the stat distribution UI
    private void UpdateStatDistributionUI()
    {
        if (availablePointsText != null)
        {
            availablePointsText.text = "Points: " + playerStats.availablePoints;
        }
        
        // Update the stats display
        if (playerStats != null)
        {
            playerStats.UpdateStatsUI();
        }
        
        // Enable/disable plus buttons based on available points
        bool hasPoints = playerStats.availablePoints > 0;
        if (healthPlusButton != null) healthPlusButton.interactable = hasPoints;
        if (attackPlusButton != null) attackPlusButton.interactable = hasPoints;
        if (defensePlusButton != null) defensePlusButton.interactable = hasPoints;
        if (speedPlusButton != null) speedPlusButton.interactable = hasPoints;
        
        // Enable confirm button only when all points are used or player has made at least one choice
        if (confirmButton != null)
        {
            confirmButton.interactable = (playerStats.availablePoints < playerStats.pointsPerLevel);
        }
    }
    
    // Set up the stat distribution button listeners
    private void SetupStatDistributionButtons()
    {
        // Remove existing listeners to prevent duplicates
        if (healthPlusButton != null)
        {
            healthPlusButton.onClick.RemoveAllListeners();
            healthPlusButton.onClick.AddListener(() => {
                if (playerStats.availablePoints > 0)
                {
                    playerStats.health += 10; // Health increases by 10 per point
                    playerStats.availablePoints--;
                    UpdateStatDistributionUI();
                }
            });
        }
        
        if (attackPlusButton != null)
        {
            attackPlusButton.onClick.RemoveAllListeners();
            attackPlusButton.onClick.AddListener(() => {
                if (playerStats.availablePoints > 0)
                {
                    playerStats.attack += 2; // Attack increases by 2 per point
                    playerStats.availablePoints--;
                    UpdateStatDistributionUI();
                }
            });
        }
        
        if (defensePlusButton != null)
        {
            defensePlusButton.onClick.RemoveAllListeners();
            defensePlusButton.onClick.AddListener(() => {
                if (playerStats.availablePoints > 0)
                {
                    playerStats.defense += 2; // Defense increases by 2 per point
                    playerStats.availablePoints--;
                    UpdateStatDistributionUI();
                }
            });
        }
        
        if (speedPlusButton != null)
        {
            speedPlusButton.onClick.RemoveAllListeners();
            speedPlusButton.onClick.AddListener(() => {
                if (playerStats.availablePoints > 0)
                {
                    playerStats.speed += 1; // Speed increases by 1 per point
                    playerStats.availablePoints--;
                    UpdateStatDistributionUI();
                    
                    // Update character movement speed
                    UpdateCharacterMovementSpeed();
                }
            });
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => {
                Debug.Log("Confirm button clicked - closing panel and resuming game");
                
                // Close the panel and resume the game
                statDistributionPanel.SetActive(false);
                Time.timeScale = 1f;
                
                // Update character movement speed
                UpdateCharacterMovementSpeed();
                
                // Update health bar with new max health
                if (playerHealth != null)
                {
                    playerHealth.UpdateMaxHealth(playerStats.health);
                }
                
                Debug.Log("Stat distribution confirmed - game resumed");
            });
        }
    }
    
    // Update the character's movement speed based on the speed stat
    private void UpdateCharacterMovementSpeed()
    {
        // Find the ThirdPersonController
        ThirdPersonController controller = FindObjectOfType<ThirdPersonController>();
        if (controller != null && playerStats != null)
        {
            // Base speed is 3, max speed is 8 at speed stat 50
            float baseSpeed = 3f;
            float maxSpeedBonus = 5f;
            float speedFactor = playerStats.speed / 50f; // 50 is the max speed stat we expect
            
            // Calculate new move speed
            float newMoveSpeed = baseSpeed + (maxSpeedBonus * Mathf.Clamp01(speedFactor));
            
            // Set the move speed
            controller.moveSpeed = newMoveSpeed;
            
            // Sprint speed is 1.5x move speed
            controller.sprintSpeed = newMoveSpeed * 1.5f;
            
            Debug.Log($"Updated character move speed to {newMoveSpeed} based on speed stat {playerStats.speed}");
        }
    }
}
