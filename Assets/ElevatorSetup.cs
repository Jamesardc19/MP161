using UnityEngine;
using System.Collections.Generic;

public class ElevatorSetup : MonoBehaviour
{
    [Tooltip("The main elevator controller GameObject")]
    public GameObject elevatorController;
    
    [Tooltip("Set to true to automatically find and set up elevator buttons")]
    public bool autoSetupButtons = true;
    
    [Tooltip("Set to true to use the fixed elevator controller script")]
    public bool useFixedController = true;
    
    [Tooltip("Increase this if the player has trouble interacting with buttons")]
    public float buttonColliderRadius = 0.3f;
    
    private void Start()
    {
        if (autoSetupButtons)
        {
            SetupElevatorButtons();
        }
        
        if (useFixedController && elevatorController != null)
        {
            // Check if the controller already has the fixed script
            if (elevatorController.GetComponent<evelator_controll_fixed>() == null)
            {
                // Copy values from original controller to fixed controller
                evelator_controll originalController = elevatorController.GetComponent<evelator_controll>();
                if (originalController != null)
                {
                    evelator_controll_fixed fixedController = elevatorController.AddComponent<evelator_controll_fixed>();
                    
                    // Copy values
                    fixedController.FloorHighs = originalController.FloorHighs;
                    fixedController.ElevatorCabin = originalController.ElevatorCabin;
                    fixedController.FloorNumbers = originalController.FloorNumbers;
                    fixedController.DoorOpenTime = originalController.DoorOpenTime;
                    fixedController.Door_outside_left = originalController.Door_outside_left;
                    fixedController.Door_outside_left_close_value = originalController.Door_outside_left_close_value;
                    fixedController.Door_outside_left_open_value = originalController.Door_outside_left_open_value;
                    fixedController.Door_outside_right = originalController.Door_outside_right;
                    fixedController.Door_outside_right_close_value = originalController.Door_outside_right_close_value;
                    fixedController.Door_outside_right_open_value = originalController.Door_outside_right_open_value;
                    fixedController.Door_inside_right = originalController.Door_inside_right;
                    fixedController.Door_inside_right_close_value = originalController.Door_inside_right_close_value;
                    fixedController.Door_inside_right_open_value = originalController.Door_inside_right_open_value;
                    fixedController.Door_inside_left = originalController.Door_inside_left;
                    fixedController.Door_inside_left_close_value = originalController.Door_inside_left_close_value;
                    fixedController.Door_inside_left_open_value = originalController.Door_inside_left_open_value;
                    
                    // Disable original controller
                    originalController.enabled = false;
                    
                    Debug.Log("Replaced original elevator controller with fixed version");
                }
                else
                {
                    Debug.LogError("No original elevator controller found on " + elevatorController.name);
                }
            }
        }
    }
    
    private void SetupElevatorButtons()
    {
        if (elevatorController == null)
        {
            Debug.LogError("Elevator controller not assigned!");
            return;
        }
        
        // Find all potential elevator buttons in the scene
        List<Transform> potentialButtons = new List<Transform>();
        
        // Look for objects with names containing "floor" and "button"
        foreach (Transform child in FindObjectsOfType<Transform>())
        {
            string name = child.name.ToLower();
            if ((name.Contains("floor") && name.Contains("button")) || 
                name.StartsWith("button floor") || 
                name.Contains("elevator") && name.Contains("button"))
            {
                potentialButtons.Add(child);
            }
        }
        
        Debug.Log("Found " + potentialButtons.Count + " potential elevator buttons");
        
        // Set up each button
        foreach (Transform button in potentialButtons)
        {
            // Determine floor number from name
            int floorNumber = ExtractFloorNumber(button.name);
            
            if (floorNumber >= 1 && floorNumber <= 6)
            {
                // Rename button to match expected format
                string correctName = "Button floor " + floorNumber;
                if (button.name != correctName)
                {
                    Debug.Log("Renaming button from '" + button.name + "' to '" + correctName + "'");
                    button.name = correctName;
                }
                
                // Add pass_on_parent component if missing
                pass_on_parent parentScript = button.GetComponent<pass_on_parent>();
                if (parentScript == null)
                {
                    parentScript = button.gameObject.AddComponent<pass_on_parent>();
                    Debug.Log("Added pass_on_parent to " + button.name);
                }
                
                // Set parent reference
                parentScript.MyParent = elevatorController;
                
                // Add collider if missing
                Collider buttonCollider = button.GetComponent<Collider>();
                if (buttonCollider == null)
                {
                    SphereCollider sphereCollider = button.gameObject.AddComponent<SphereCollider>();
                    sphereCollider.radius = buttonColliderRadius;
                    Debug.Log("Added SphereCollider to " + button.name);
                }
                else if (buttonCollider is SphereCollider)
                {
                    // Adjust existing sphere collider
                    SphereCollider sphereCollider = (SphereCollider)buttonCollider;
                    sphereCollider.radius = buttonColliderRadius;
                }
                
                Debug.Log("Successfully set up button for floor " + floorNumber);
            }
            else
            {
                Debug.LogWarning("Could not determine floor number for button: " + button.name);
            }
        }
    }
    
    private int ExtractFloorNumber(string buttonName)
    {
        // Try to extract a number from the button name
        string name = buttonName.ToLower();
        
        // Check for "floor X" pattern
        for (int i = 1; i <= 6; i++)
        {
            if (name.Contains("floor " + i) || name.Contains("floor" + i))
            {
                return i;
            }
        }
        
        // Check for "X floor" pattern
        for (int i = 1; i <= 6; i++)
        {
            if (name.Contains(i + " floor") || name.Contains(i + "floor"))
            {
                return i;
            }
        }
        
        // Check for any number in the name
        for (int i = 1; i <= 6; i++)
        {
            if (name.Contains(i.ToString()))
            {
                return i;
            }
        }
        
        return -1;
    }
    
    // Debug method to be called from Inspector
    public void DebugElevatorSetup()
    {
        if (elevatorController == null)
        {
            Debug.LogError("Elevator controller not assigned!");
            return;
        }
        
        // Check which controller is active
        evelator_controll originalController = elevatorController.GetComponent<evelator_controll>();
        evelator_controll_fixed fixedController = elevatorController.GetComponent<evelator_controll_fixed>();
        
        Debug.Log("Original controller active: " + (originalController != null && originalController.enabled));
        Debug.Log("Fixed controller active: " + (fixedController != null && fixedController.enabled));
        
        // Check floor heights
        if (originalController != null && originalController.enabled)
        {
            Debug.Log("Floor heights: " + string.Join(", ", originalController.FloorHighs));
        }
        else if (fixedController != null && fixedController.enabled)
        {
            Debug.Log("Floor heights: " + string.Join(", ", fixedController.FloorHighs));
        }
        
        // Check elevator cabin
        GameObject cabin = null;
        if (originalController != null && originalController.enabled)
        {
            cabin = originalController.ElevatorCabin;
        }
        else if (fixedController != null && fixedController.enabled)
        {
            cabin = fixedController.ElevatorCabin;
        }
        
        if (cabin != null)
        {
            Debug.Log("Elevator cabin: " + cabin.name + ", Position: " + cabin.transform.localPosition);
        }
        else
        {
            Debug.LogError("Elevator cabin not assigned!");
        }
    }
}
