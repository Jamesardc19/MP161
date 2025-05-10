using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float height = 2.0f;
    public float damping = 5.0f;
    public float mouseSensitivity = 2.0f;
    public float minVerticalAngle = -30.0f;
    public float maxVerticalAngle = 60.0f;
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0); // Offset to look at player's head
    public bool invertY = false;
    
    [Header("Zoom Settings")]
    public float zoomSpeed = 5.0f;       // Increased zoom speed for better responsiveness
    public float minZoomDistance = 2.0f;  // Minimum zoom distance (closest to character)
    public float maxZoomDistance = 8.0f;  // Maximum zoom distance (furthest from character)
    public float defaultDistance = 5.0f;  // Default camera distance
    public KeyCode zoomInKey = KeyCode.Q;   // Alternative key for zooming in
    public KeyCode zoomOutKey = KeyCode.E;  // Alternative key for zooming out
    
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private float currentZoomDistance;
    private Vector3 smoothVelocity = Vector3.zero;

    private void Start()
    {
        Debug.Log("ThirdPersonCameraController started on " + gameObject.name);
        
        if (target == null)
        {
            Debug.LogWarning("No target assigned to ThirdPersonCameraController on " + gameObject.name + ". Please assign a target in the Inspector.");
            
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
        
        // Initialize zoom distance
        currentZoomDistance = distance;
        
        // Lock and hide cursor for better camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Handle mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1 : -1);
        
        rotationY += mouseX;
        rotationX += mouseY;
        
        // Clamp vertical rotation
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        
        // Handle zoom with multiple input methods
        float zoomAmount = 0f;
        
        // Method 1: Mouse scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            zoomAmount = scrollInput * zoomSpeed;
        }
        
        // Method 2: Keyboard keys (as backup)
        if (Input.GetKey(zoomInKey))
        {
            zoomAmount = Time.deltaTime * zoomSpeed * 0.5f; // Zoom in
        }
        else if (Input.GetKey(zoomOutKey))
        {
            zoomAmount = -Time.deltaTime * zoomSpeed * 0.5f; // Zoom out
        }
        
        // Apply zoom if there's any input
        if (zoomAmount != 0)
        {
            // Negative means zoom in (get closer), positive means zoom out (get further)
            currentZoomDistance -= zoomAmount;
            
            // Clamp the zoom distance between min and max values
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);
            
            Debug.Log("Zoom adjusted to: " + currentZoomDistance + " (input: " + zoomAmount + ")");
        }
        
        // Allow cursor to be shown when pressing Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Create the rotation quaternion based on mouse input
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        
        // Calculate camera position based on target and rotation
        Vector3 targetPosition = target.position + targetOffset;
        Vector3 direction = rotation * -Vector3.forward;
        
        // Use the current zoom distance instead of the fixed distance
        Vector3 desiredPosition = targetPosition + direction * currentZoomDistance;
        
        // Apply height offset
        desiredPosition.y = target.position.y + height;
        
        // Smoothly move the camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, damping * Time.deltaTime);
        
        // Look at the target
        transform.LookAt(targetPosition);
    }
}
