# Inventory System for MP161

This inventory system allows players to collect and manage items like bolts, nuts, and transistors in your game.

## Setup Instructions

### 1. Create the Tag

First, create the "InventoryManager" tag in Unity:
1. Go to Edit > Project Settings > Tags and Layers
2. Add a new tag called "InventoryManager"

### 2. Create Item ScriptableObjects

1. In the Project window, right-click and select Create > Inventory > Item
2. Create items for each collectible type (Bolts, Nuts, Transistors)
3. For each item:
   - Set a name (e.g., "Bolt", "Nut", "Transistor")
   - Assign an icon sprite
   - Add a description
   - Select the appropriate ItemType

### 3. Create a Resources Folder for Items

1. Create a "Resources" folder in your Assets directory if it doesn't exist
2. Inside Resources, create an "Items" folder
3. Move or copy your Item ScriptableObjects into this folder

### 4. Set Up the Inventory Manager

1. Create an empty GameObject in your scene
2. Name it "InventoryManager"
3. Add the Inventory.cs script to it
4. The tag "InventoryManager" will be automatically assigned in the script

### 5. Set Up the UI

1. Create a Canvas for your inventory UI
2. Create a Panel for the inventory (set it inactive by default)
3. Add a Grid Layout Group component to organize inventory slots
4. Create a slot prefab with:
   - An Image component for the item icon
   - A Text component for the item name
   - Add the InventorySlot.cs script to it
5. Create a notification panel for item pickups with a Text component

### 6. Configure UIManager References

1. Find your UIManager GameObject
2. Assign the inventory panel to the "inventoryPanel" field
3. Assign the grid layout group transform to the "inventorySlotContainer" field
4. Assign the slot prefab to the "inventorySlotPrefab" field
5. Assign the notification panel to the "pickupNotification" field
6. Assign the notification text to the "pickupNotificationText" field

### 7. Set Up Fast Item Switching (Optional)

1. Create UI elements for quick slots (usually 4)
2. Create an empty GameObject and add the FastItemSwitcher.cs script
3. Assign the quick slot UI elements to the "quickSlotObjects" array
4. Create a selection indicator (e.g., a highlighted border) and assign it

### 8. Create Collectable Items in the Scene

1. Create 3D models or sprites for your collectible items
2. Add a Collider component (set "Is Trigger" to true)
3. Add the ItemPickup.cs script
4. Assign the corresponding Item ScriptableObject to the "item" field
5. Place these objects throughout your game world

## Using the Inventory System

- Press 'I' to toggle the inventory panel (configurable in UIManager)
- Walk over items to collect them
- Use number keys 1-4 or mouse scroll wheel to switch between quick slot items
- Right-click to use the selected item
- Items are automatically saved and loaded with the game

## Integration with Existing Systems

The inventory system is integrated with:
- GameManager for saving/loading inventory data
- UIManager for displaying inventory UI and pickup notifications
- Character stats for potential item effects

## Extending the System

You can extend this system by:
1. Adding more item types to the Item.ItemType enum
2. Implementing specific item usage effects in FastItemSwitcher.UseSelectedItem()
3. Creating equipment slots for wearable items
4. Adding item stacking for consumables
5. Implementing crafting by combining items
