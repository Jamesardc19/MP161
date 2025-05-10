using UnityEngine;

public class StatsToggle : MonoBehaviour
{
    public GameObject statsPanel;
    private bool isVisible = false;
    
    public void ToggleStatsPanel()
    {
        isVisible = !isVisible;
        statsPanel.SetActive(isVisible);
    }
    
    // Optional: Toggle with a key press
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleStatsPanel();
        }
    }
}