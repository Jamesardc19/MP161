using UnityEngine;

public class SceneLightingManager : MonoBehaviour
{
    [Header("Scene Lighting")]
    public Light[] sceneLights; // Lights to adjust in this scene
    public float defaultBrightness = 1.0f; // Default brightness for this scene
    
    private void Start()
    {
        // Apply the scene's default lighting settings
        ApplyDefaultLighting();
    }
    
    // Apply the default lighting for this scene
    private void ApplyDefaultLighting()
    {
        // If no scene-specific lights are assigned, use default lighting
        if (sceneLights == null || sceneLights.Length == 0)
        {
            return;
        }
        
        // Apply default brightness to all scene lights
        foreach (Light light in sceneLights)
        {
            if (light != null)
            {
                light.intensity = light.intensity * defaultBrightness;
            }
        }
    }
}
