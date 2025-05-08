using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Health Bar")]
    public Image healthBarImage;
    public Gradient healthBarGradient;
    
    [Header("Stats Panel")]
    public GameObject statsPanel;
    public Text attackText;
    public Text defenseText;
    public Text speedText;
    public Text healthText;
    public Text levelText;
    
    [Header("References")]
    public HealthManager playerHealth;
    public CharacterStats playerStats;
    
    // Toggle for stats panel visibility
    private bool statsVisible = false;
    
    void Start()
    {
        // Initialize stats panel state
        if (statsPanel != null)
        {
            statsPanel.SetActive(statsVisible);
        }
        
        // Connect health bar to player health manager
        if (playerHealth != null && healthBarImage != null)
        {
            playerHealth.healthBarImage = healthBarImage;
        }
        
        // Connect stat texts to player stats
        if (playerStats != null)
        {
            playerStats.attackText = attackText;
            playerStats.defenseText = defenseText;
            playerStats.speedText = speedText;
            playerStats.healthText = healthText;
            playerStats.levelText = levelText;
        }
    }
    
    void Update()
    {
        // Update health bar color based on current health percentage
        if (healthBarImage != null && playerHealth != null && healthBarGradient != null)
        {
            float healthPercentage = playerHealth.GetHealthPercentage();
            healthBarImage.color = healthBarGradient.Evaluate(healthPercentage);
        }
        
        // Toggle stats panel with Tab key (can be changed to any key)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleStatsPanel();
        }
    }
    
    // Toggle stats panel visibility
    public void ToggleStatsPanel()
    {
        if (statsPanel != null)
        {
            statsVisible = !statsVisible;
            statsPanel.SetActive(statsVisible);
        }
    }
    
    // Method to trigger a level up (can be called from a button or other event)
    public void TriggerLevelUp()
    {
        if (playerStats != null)
        {
            playerStats.LevelUp();
        }
    }
}
