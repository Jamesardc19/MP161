using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;  // Reference to the item this object represents
    
    [Header("Visual Effects")]
    public float rotationSpeed = 50f;  // Speed at which the item rotates
    public float bobSpeed = 1f;        // Speed of the bobbing motion
    public float bobHeight = 0.2f;     // Height of the bobbing motion
    
    private Vector3 startPosition;
    
    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void Update()
    {
        // Rotate the item for visual effect
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob the item up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision detected with: " + other.name + ", tag: " + other.tag);
        
        if (other.CompareTag("Player"))  // Check if the player collides with the item
        {
            Debug.Log("Player collision detected with item: " + (item != null ? item.itemName : "null"));
            
            // Check if item is valid
            if (item == null)
            {
                Debug.LogError("Item reference is null on ItemPickup!");
                return;
            }
            
            // Try to get the Inventory component
            Inventory inventory = Inventory.Instance;
            
            if (inventory == null)
            {
                // Try to find inventory by tag
                GameObject inventoryObj = GameObject.FindWithTag("InventoryManager");
                if (inventoryObj != null)
                {
                    inventory = inventoryObj.GetComponent<Inventory>();
                }
                
                if (inventory == null)
                {
                    Debug.LogError("Inventory instance not found!");
                    return;
                }
            }
            
            Debug.Log("Found inventory, adding item: " + item.itemName);
            
            // Play pickup sound if available
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
            
            // Add the item to the inventory
            inventory.AddItem(item);
            
            // Show a pickup notification
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                Debug.Log("Found UIManager, showing notification");
                uiManager.ShowPickupNotification(item.itemName);
            }
            else
            {
                Debug.LogError("UIManager not found!");
            }
            
            // Destroy the item from the scene
            Debug.Log("Destroying item: " + gameObject.name);
            Destroy(gameObject);
        }
    }
}
