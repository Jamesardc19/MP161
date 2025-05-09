using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public Text nameText;
    
    [Header("Tooltip")]
    public GameObject tooltip;
    public Text tooltipText;
    
    private Item item;
    
    public void SetItem(Item newItem)
    {
        item = newItem;
        
        if (icon != null && item.icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
        }
        
        if (nameText != null)
        {
            nameText.text = item.itemName;
        }
        
        if (tooltipText != null)
        {
            tooltipText.text = item.description;
        }
    }
    
    // Show tooltip on mouse over
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null && item != null)
        {
            tooltip.SetActive(true);
        }
    }
    
    // Hide tooltip when mouse leaves
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false);
        }
    }
    
    // Method to use the item (can be called from a button)
    public void UseItem()
    {
        if (item != null)
        {
            Debug.Log("Using item: " + item.itemName);
            // Add specific item usage logic here
        }
    }
}
