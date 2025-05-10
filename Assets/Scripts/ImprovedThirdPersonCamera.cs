using UnityEngine;

public class ImprovedThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0); // Offset to look at player's head

    [Header("Position Settings")]
    public float distance = 5.0f;
    public float height = 2.0f;
    public float damping = 5.0f;
    public float minDistance = 1.0f; // Minimum distance when colliding with objects

    [Header("Rotation Settings")]
    public float mouseSensitivity = 2.0f;
    public float minVerticalAngle = -30.0f;
    public float maxVerticalAngle = 60.0f;
    public bool invertY = false;

    [Header("Collision Settings")]
    public LayerMask collisionLayers = -1; // All layers by default
    public float collisionRadius = 0.2f; // Radius of the camera collision detection
    public float collisionDamping = 10.0f; // How quickly camera moves when avoiding obstacles

    // Private variables
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private float currentDistance;
    private Vector3 smoothPosition = Vector3.zero;
    private Vector3 desiredPosition;

    private void Start()
    {
        Debug.Log("ImprovedThirdPersonCamera started on " + gameObject.name);
        
        // Find target if not assigned
        if (target == null)
        {
            Debug.LogWarning("No target assigned to ImprovedThirdPersonCamera. Attempting to find a player character.");
            
            // Try to find a character with ThirdPersonController
            ThirdPersonController[] controllers = FindObjectsOfType<ThirdPersonController>();
            if (controllers.Length > 0)
            {
                target = controllers[0].transform;
                Debug.Log("Automatically assigned " + target.name + " as the camera target.");
            }
        }
        
        // Initialize rotation to current camera rotation
        rotationY = transform.eulerAngles.y;
        rotationX = transform.eulerAngles.x;
        
        // Initialize distance
        currentDistance = distance;
        
        // Lock and hide cursor for better camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (target == null) return;
        
        // Handle mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1 : -1);
        
        rotationY += mouseX;
        rotationX += mouseY;
        
        // Clamp vertical rotation
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        
        // Allow cursor to be shown when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate target position with offset
        Vector3 targetPosition = target.position + targetOffset;
        
        // Create the rotation quaternion based on mouse input
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        
        // Calculate the desired camera position without collision detection
        Vector3 direction = rotation * -Vector3.forward;
        desiredPosition = targetPosition + direction * distance;
        
        // Apply height offset
        desiredPosition.y = target.position.y + height;
        
        // Check for collisions
        currentDistance = CheckCameraCollision(targetPosition, desiredPosition);
        
        // Recalculate position with collision-adjusted distance
        desiredPosition = targetPosition + direction * currentDistance;
        desiredPosition.y = target.position.y + height;
        
        // Smoothly move the camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, damping * Time.deltaTime);
        
        // Look at the target
        transform.LookAt(targetPosition);
    }
    
    private float CheckCameraCollision(Vector3 targetPos, Vector3 desiredPos)
    {
        // Direction from target to desired camera position
        Vector3 direction = desiredPos - targetPos;
        float targetDistance = direction.magnitude;
        direction.Normalize();
        
        // Cast a ray from target to desired camera position
        RaycastHit hit;
        if (Physics.SphereCast(targetPos, collisionRadius, direction, out hit, distance, collisionLayers))
        {
            // If we hit something, adjust the distance
            float adjustedDistance = hit.distance - collisionRadius;
            return Mathf.Clamp(adjustedDistance, minDistance, distance);
        }
        
        // If no collision, return the original distance
        return distance;
    }
    
    // Draw debug visuals in the editor
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position + targetOffset, 0.1f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(target.position + targetOffset, desiredPosition);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(desiredPosition, collisionRadius);
    }
}
