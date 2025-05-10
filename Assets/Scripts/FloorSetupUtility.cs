using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
public class FloorSetupUtility : MonoBehaviour
{
    [Header("Floor Setup")]
    public List<Transform> floorParents = new List<Transform>();
    
    [ContextMenu("Setup Floor Items")]
    public void SetupFloorItems()
    {
        // Create a FloorLevelManager if one doesn't exist
        FloorLevelManager floorManager = FindObjectOfType<FloorLevelManager>();
        if (floorManager == null)
        {
            GameObject managerObj = new GameObject("FloorLevelManager");
            floorManager = managerObj.AddComponent<FloorLevelManager>();
            Debug.Log("Created new FloorLevelManager");
        }
        
        // Clear existing floor data
        floorManager.floors.Clear();
        
        // Setup each floor
        foreach (Transform floorParent in floorParents)
        {
            if (floorParent == null)
                continue;
                
            string floorName = floorParent.name;
            
            // Create new floor data
            FloorLevelManager.FloorData floorData = new FloorLevelManager.FloorData();
            floorData.floorName = floorName;
            floorData.floorTransform = floorParent;
            
            // Add to floor manager
            floorManager.floors.Add(floorData);
            
            // Setup all item pickups on this floor
            SetupItemsOnFloor(floorParent, floorName);
            
            Debug.Log($"Setup floor: {floorName}");
        }
        
        // Mark the scene as dirty so Unity knows it needs to be saved
        EditorUtility.SetDirty(floorManager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
    
    private void SetupItemsOnFloor(Transform floorParent, string floorName)
    {
        // Get all ItemPickup components under this floor
        ItemPickup[] items = floorParent.GetComponentsInChildren<ItemPickup>(true);
        
        foreach (ItemPickup item in items)
        {
            // Set the floor name
            item.floorName = floorName;
            
            // Mark the object as dirty
            EditorUtility.SetDirty(item);
            
            Debug.Log($"Assigned item {item.name} to floor {floorName}");
        }
    }
}

// Custom editor for the FloorSetupUtility
[CustomEditor(typeof(FloorSetupUtility))]
public class FloorSetupUtilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        FloorSetupUtility utility = (FloorSetupUtility)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Floor Items", GUILayout.Height(30)))
        {
            utility.SetupFloorItems();
        }
        
        EditorGUILayout.HelpBox("This utility will:\n1. Create a FloorLevelManager if needed\n2. Setup floor data for each floor parent\n3. Assign floor names to all items", MessageType.Info);
    }
}
#endif
