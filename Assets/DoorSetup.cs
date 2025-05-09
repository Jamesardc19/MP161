using UnityEngine;

public class DoorSetup : MonoBehaviour
{
    [Tooltip("Set to true to automatically tag this object as a door")]
    public bool isDoor = false;
    
    [Tooltip("Set to true to add a collider if missing")]
    public bool addColliderIfMissing = true;
    
    [Tooltip("Size of the collider to add if missing")]
    public Vector3 colliderSize = new Vector3(1f, 2f, 0.2f);
    
    private void Start()
    {
        if (isDoor)
        {
            // Set the tag to "door"
            gameObject.tag = "door";
            
            // Make sure it has a Door component
            if (GetComponent<Door>() == null)
            {
                gameObject.AddComponent<Door>();
                Debug.Log("Added Door component to " + gameObject.name);
            }
            
            // Make sure it has a collider
            if (addColliderIfMissing && GetComponent<Collider>() == null)
            {
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = colliderSize;
                boxCollider.isTrigger = false;
                Debug.Log("Added BoxCollider to " + gameObject.name);
            }
        }
    }
}
