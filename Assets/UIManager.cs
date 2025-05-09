using UnityEngine;
using UnityEngine.UI; // Includes GridLayoutGroup
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
    
    [Header("Pickup Notification")]
    public GameObject pickupNotification;
    public Text pickupNotificationText;
    public float notificationDuration = 2f;
    
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
        
        // Initialize pickup notification
        if (pickupNotification != null)
        {
            pickupNotification.SetActive(false);
        }
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
        
        // Ensure Grid Layout Group is properly configured
        GridLayoutGroup gridLayout = inventorySlotContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            // Configure for 3 items in a row
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            
            // Set appropriate cell size and spacing
            gridLayout.cellSize = new Vector2(100, 100); // Increased size for better visibility
            gridLayout.spacing = new Vector2(60, 40);   // X spacing of 60 as requested
            
            // Set padding to ensure items don't touch the edges
            gridLayout.padding = new RectOffset(36, 20, 20, 20); // Left padding of 36 as requested
            
            Debug.Log("Configured Grid Layout Group: " + gridLayout.cellSize + ", " + gridLayout.spacing);
        }
        else
        {
            Debug.LogWarning("No Grid Layout Group found on inventory slot container!");
            // Try to add a Grid Layout Group if it doesn't exist
            gridLayout = inventorySlotContainer.gameObject.AddComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 3;
                gridLayout.cellSize = new Vector2(100, 100);
                gridLayout.spacing = new Vector2(60, 40);
                gridLayout.padding = new RectOffset(36, 20, 20, 20); // Left padding of 36 as requested
                Debug.Log("Added and configured Grid Layout Group");
            }
        }
        
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
                slotRect.sizeDelta = new Vector2(100, 100); // Match the updated grid cell size
                
                // Let the Grid Layout Group handle positioning
                // We're not setting explicit positions to ensure consistent layout
                // The Grid Layout Group will use the spacing (60, 40) and padding (36, 20, 20, 20) we configured
                Debug.Log("Using Grid Layout Group for positioning with item #" + itemCount);
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
}
