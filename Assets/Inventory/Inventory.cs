using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Singleton pattern for easy access
    public static Inventory Instance { get; private set; }
    
    // List of items in the inventory
    public List<Item> items = new List<Item>();
    
    // Event that will be triggered when the inventory changes
    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChanged;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Add the InventoryManager tag to this GameObject
            gameObject.tag = "InventoryManager";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add an item to the inventory
    public void AddItem(Item newItem)
    {
        // Check if we already have this type of item
        Item existingItem = items.Find(item => item.itemType == newItem.itemType);
        
        if (existingItem != null)
        {
            // We already have this type of item
            Debug.Log("Item already collected: " + newItem.itemName);
            return;
        }

        // Add the new item
        items.Add(newItem);
        Debug.Log(newItem.itemName + " added to inventory. Total items: " + items.Count);
        
        // Trigger the inventory changed event
        if (onInventoryChanged != null)
        {
            Debug.Log("Triggering inventory changed event");
            onInventoryChanged.Invoke();
        }
        else
        {
            Debug.LogWarning("onInventoryChanged event is null!");
        }
    }

    // Remove an item from the inventory
    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log(item.itemName + " removed from inventory.");
            
            // Trigger the inventory changed event
            if (onInventoryChanged != null)
                onInventoryChanged.Invoke();
        }
    }

    // Check if the inventory contains a specific item
    public bool HasItem(Item item)
    {
        return items.Contains(item);
    }

    // Get all items of a specific type
    public List<Item> GetItemsOfType(Item.ItemType type)
    {
        List<Item> result = new List<Item>();
        foreach (Item item in items)
        {
            if (item.itemType == type)
                result.Add(item);
        }
        return result;
    }

    // Save inventory data to PlayerPrefs (integrating with existing save system)
    public void SaveInventory()
    {
        // Save the number of items
        PlayerPrefs.SetInt("InventoryCount", items.Count);
        
        // Save each item by name (assuming items are ScriptableObjects that can be found by name)
        for (int i = 0; i < items.Count; i++)
        {
            PlayerPrefs.SetString("InventoryItem_" + i, items[i].name);
        }
        
        PlayerPrefs.Save();
    }

    // Load inventory data from PlayerPrefs
    public void LoadInventory()
    {
        // Clear current inventory
        items.Clear();
        
        // Get the number of saved items
        int count = PlayerPrefs.GetInt("InventoryCount", 0);
        
        // Load each item
        for (int i = 0; i < count; i++)
        {
            string itemName = PlayerPrefs.GetString("InventoryItem_" + i, "");
            if (!string.IsNullOrEmpty(itemName))
            {
                // Find the item ScriptableObject by name
                Item item = Resources.Load<Item>("Items/" + itemName);
                if (item != null)
                {
                    items.Add(item);
                }
            }
        }
        
        // Trigger the inventory changed event
        if (onInventoryChanged != null)
            onInventoryChanged.Invoke();
    }
}
