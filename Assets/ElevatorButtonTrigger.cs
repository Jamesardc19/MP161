using UnityEngine;
using System.Collections;

public class ElevatorButtonTrigger : MonoBehaviour
{
    [Tooltip("The floor number this button calls (1-6)")]
    public int floorNumber = 1;
    
    [Tooltip("The elevator controller object")]
    public GameObject elevatorController;
    
    [Tooltip("Size of the trigger collider")]
    public Vector3 triggerSize = new Vector3(1f, 1f, 1f);
    
    [Tooltip("Visual feedback when button is pressed")]
    public GameObject buttonVisualFeedback;
    
    private bool canInteract = false;
    private bool isProcessing = false;
    
    void Start()
    {
        // Make sure we have a box collider
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        // Set it as a trigger
        boxCollider.isTrigger = true;
        boxCollider.size = triggerSize;
        
        // Find elevator controller if not assigned
        if (elevatorController == null)
        {
            // Try to find by tag
            GameObject taggedController = GameObject.FindGameObjectWithTag("ElevatorController");
            if (taggedController != null)
            {
                elevatorController = taggedController;
                Debug.Log("Found elevator controller by tag: " + elevatorController.name);
            }
            else
            {
                // Try to find by component
                evelator_controll[] controllers = FindObjectsOfType<evelator_controll>();
                if (controllers.Length > 0)
                {
                    elevatorController = controllers[0].gameObject;
                    Debug.Log("Found elevator controller by component: " + elevatorController.name);
                }
                else
                {
                    evelator_controll_fixed[] fixedControllers = FindObjectsOfType<evelator_controll_fixed>();
                    if (fixedControllers.Length > 0)
                    {
                        elevatorController = fixedControllers[0].gameObject;
                        Debug.Log("Found fixed elevator controller: " + elevatorController.name);
                    }
                    else
                    {
                        Debug.LogError("No elevator controller found in scene!");
                    }
                }
            }
        }
        
        // Set the name to match expected format
        gameObject.name = "Button floor " + floorNumber;
        
        // Add pass_on_parent if needed
        pass_on_parent parentScript = GetComponent<pass_on_parent>();
        if (parentScript == null)
        {
            parentScript = gameObject.AddComponent<pass_on_parent>();
        }
        
        if (elevatorController != null)
        {
            parentScript.MyParent = elevatorController;
        }
        
        // Set up visual feedback if available
        if (buttonVisualFeedback != null)
        {
            buttonVisualFeedback.SetActive(false);
        }
    }
    
    void Update()
    {
        // Check for key press when player is in range
        if (canInteract && Input.GetKeyDown(KeyCode.F) && !isProcessing)
        {
            PressButton();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player") || other.GetComponent<ThirdPersonController>() != null)
        {
            canInteract = true;
            Debug.Log("Player can now interact with elevator button " + floorNumber);
            
            // Show UI prompt or highlight
            if (buttonVisualFeedback != null)
            {
                buttonVisualFeedback.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player") || other.GetComponent<ThirdPersonController>() != null)
        {
            canInteract = false;
            Debug.Log("Player left elevator button " + floorNumber + " area");
            
            // Hide UI prompt or highlight
            if (buttonVisualFeedback != null)
            {
                buttonVisualFeedback.SetActive(false);
            }
        }
    }
    
    public void PressButton()
    {
        if (isProcessing || elevatorController == null)
            return;
            
        Debug.Log("Elevator button " + floorNumber + " pressed!");
        StartCoroutine(ProcessButtonPress());
    }
    
    private IEnumerator ProcessButtonPress()
    {
        isProcessing = true;
        
        // Visual feedback
        if (buttonVisualFeedback != null)
        {
            buttonVisualFeedback.SetActive(true);
        }
        
        // Try to call elevator using original controller
        evelator_controll originalController = elevatorController.GetComponent<evelator_controll>();
        if (originalController != null && originalController.enabled)
        {
            Debug.Log("Calling elevator using original controller");
            originalController.AddTaskEve("Button floor " + floorNumber);
        }
        else
        {
            // Try fixed controller
            evelator_controll_fixed fixedController = elevatorController.GetComponent<evelator_controll_fixed>();
            if (fixedController != null && fixedController.enabled)
            {
                Debug.Log("Calling elevator using fixed controller");
                fixedController.AddTaskEve("Button floor " + floorNumber);
            }
            else
            {
                Debug.LogError("No active elevator controller found!");
            }
        }
        
        // Wait a moment before allowing another press
        yield return new WaitForSeconds(1.0f);
        
        isProcessing = false;
        
        // Turn off visual feedback after a delay
        if (buttonVisualFeedback != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (!canInteract)
            {
                buttonVisualFeedback.SetActive(false);
            }
        }
    }
    
    // Draw gizmos to visualize the interaction area
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, triggerSize);
    }
}
