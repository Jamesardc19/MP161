using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FastItemSwitcher : MonoBehaviour
{
    [Header("Quick Slots")]
    public int maxQuickSlots = 4;
    public GameObject[] quickSlotObjects;
    public KeyCode[] quickSlotKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    
    [Header("Selection Indicator")]
    public GameObject selectionIndicator;
    
    // Currently selected slot index
    private int currentSlotIndex = 0;
    // References to the items in quick slots
    private List<object> quickSlotItems = new List<object>();
    
    private void Start()
    {
        // Initialize quick slots
        InitializeQuickSlots();
        
        // Update the selection indicator
        UpdateSelectionIndicator();
    }
    
    private void Update()
    {
        // Check for number key presses to select slots
        for (int i = 0; i < quickSlotKeys.Length && i < maxQuickSlots; i++)
        {
            if (Input.GetKeyDown(quickSlotKeys[i]))
            {
                SelectQuickSlot(i);
            }
        }
        
        // Check for mouse scroll wheel to cycle through slots
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel > 0f)
        {
            // Scroll up - go to previous slot
            int newIndex = (currentSlotIndex - 1 + maxQuickSlots) % maxQuickSlots;
            SelectQuickSlot(newIndex);
        }
        else if (scrollWheel < 0f)
        {
            // Scroll down - go to next slot
            int newIndex = (currentSlotIndex + 1) % maxQuickSlots;
            SelectQuickSlot(newIndex);
        }
        
        // Check for use item button (e.g., right mouse button)
        if (Input.GetMouseButtonDown(1))
        {
            UseSelectedItem();
        }
    }
    
    // Initialize quick slots with items from inventory
    private void InitializeQuickSlots()
    {
        // Clear current items
        quickSlotItems.Clear();
        
        // Find the inventory
        GameObject inventoryObject = GameObject.FindWithTag("InventoryManager");
        if (inventoryObject == null)
            return;
            
        // Get the inventory component
        MonoBehaviour inventoryComponent = inventoryObject.GetComponent<MonoBehaviour>();
        if (inventoryComponent == null)
            return;
            
        // Get the items list through reflection
        var itemsField = inventoryComponent.GetType().GetField("items");
        if (itemsField == null)
            return;
            
        var items = itemsField.GetValue(inventoryComponent) as IEnumerable<object>;
        if (items == null)
            return;
            
        // Add items to quick slots
        foreach (var item in items)
        {
            if (quickSlotItems.Count < maxQuickSlots)
            {
                quickSlotItems.Add(item);
            }
            else
            {
                break;
            }
        }
        
        // Update the UI for each quick slot
        for (int i = 0; i < maxQuickSlots; i++)
        {
            if (i < quickSlotObjects.Length)
            {
                GameObject slotObject = quickSlotObjects[i];
                if (slotObject != null)
                {
                    // Get the image component for the icon
                    Image iconImage = slotObject.GetComponentInChildren<Image>();
                    if (iconImage != null && i < quickSlotItems.Count)
                    {
                        // Get the icon from the item
                        var item = quickSlotItems[i];
                        var iconProperty = item.GetType().GetProperty("icon");
                        if (iconProperty != null)
                        {
                            var icon = iconProperty.GetValue(item) as Sprite;
                            if (icon != null)
                            {
                                iconImage.sprite = icon;
                                iconImage.enabled = true;
                            }
                        }
                    }
                    else if (iconImage != null)
                    {
                        // No item in this slot
                        iconImage.enabled = false;
                    }
                }
            }
        }
    }
    
    // Select a quick slot
    private void SelectQuickSlot(int index)
    {
        if (index >= 0 && index < maxQuickSlots)
        {
            currentSlotIndex = index;
            UpdateSelectionIndicator();
            
            // Show the selected item name if available
            if (index < quickSlotItems.Count)
            {
                var item = quickSlotItems[index];
                var nameProperty = item.GetType().GetProperty("itemName");
                if (nameProperty != null)
                {
                    var name = nameProperty.GetValue(item) as string;
                    if (name != null)
                    {
                        Debug.Log("Selected item: " + name);
                    }
                }
            }
        }
    }
    
    // Update the selection indicator position
    private void UpdateSelectionIndicator()
    {
        if (selectionIndicator != null && currentSlotIndex < quickSlotObjects.Length)
        {
            GameObject selectedSlot = quickSlotObjects[currentSlotIndex];
            if (selectedSlot != null)
            {
                selectionIndicator.transform.position = selectedSlot.transform.position;
                selectionIndicator.SetActive(true);
            }
            else
            {
                selectionIndicator.SetActive(false);
            }
        }
    }
    
    // Use the currently selected item
    private void UseSelectedItem()
    {
        if (currentSlotIndex < quickSlotItems.Count)
        {
            var item = quickSlotItems[currentSlotIndex];
            var nameProperty = item.GetType().GetProperty("itemName");
            if (nameProperty != null)
            {
                var name = nameProperty.GetValue(item) as string;
                if (name != null)
                {
                    Debug.Log("Using item: " + name);
                    // Add specific item usage logic here
                    
                    // Example: Different effects based on item type
                    var typeProperty = item.GetType().GetProperty("itemType");
                    if (typeProperty != null)
                    {
                        var type = typeProperty.GetValue(item);
                        if (type != null)
                        {
                            // Convert enum value to string
                            string typeStr = type.ToString();
                            
                            switch (typeStr)
                            {
                                case "Bolt":
                                    Debug.Log("Using Bolt: Increases player speed temporarily");
                                    // Add speed boost effect
                                    break;
                                case "Nut":
                                    Debug.Log("Using Nut: Increases player defense temporarily");
                                    // Add defense boost effect
                                    break;
                                case "Transistor":
                                    Debug.Log("Using Transistor: Increases player attack temporarily");
                                    // Add attack boost effect
                                    break;
                                default:
                                    Debug.Log("Using item with unknown effect");
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
    
    // Refresh the quick slots when the inventory changes
    public void RefreshQuickSlots()
    {
        InitializeQuickSlots();
    }
}
