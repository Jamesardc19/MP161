using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;  // Add icons for the items
    [TextArea]
    public string description;
    public ItemType itemType;

    // Enum to categorize different types of items
    public enum ItemType
    {
        Bolt,
        Nut,
        Transistor,
        Other
    }
}
