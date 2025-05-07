using UnityEngine;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int attack = 10;
    public int defense = 5;
    public int speed = 5;
    public int health = 100;
    
    [Header("UI References")]
    public Text attackText;
    public Text defenseText;
    public Text speedText;
    public Text healthText;
    public Text levelText;
    
    [Header("Level System")]
    public int level = 1;
    public int pointsPerLevel = 5;
    
    // Reference to the health manager
    private HealthManager healthManager;
    // Reference to the animator setup
    private SetupAnimatorParameters animatorSetup;
    
    void Start()
    {
        // Get reference to HealthManager
        healthManager = GetComponent<HealthManager>();
        
        // Get reference to SetupAnimatorParameters
        animatorSetup = GetComponent<SetupAnimatorParameters>();
        
        UpdateStatsDisplay();
    }
    
    // Increase stats when leveling up
    public void LevelUp()
    {
        attack += pointsPerLevel;
        defense += pointsPerLevel / 2;
        speed += pointsPerLevel / 2;
        health += pointsPerLevel * 10; // Increase health by a bigger amount
        level++;
        
        // Update health manager with new max health
        if (healthManager != null)
        {
            healthManager.UpdateMaxHealth(health);
        }
        
        // Play level up animation if available
        if (animatorSetup != null)
        {
            animatorSetup.TriggerLevelUp();
        }
        
        UpdateStatsDisplay();
    }
    
    // Update the stats displayed in the UI
    private void UpdateStatsDisplay()
    {
        if (attackText != null)
            attackText.text = "Attack: " + attack.ToString();
        
        if (defenseText != null)
            defenseText.text = "Defense: " + defense.ToString();
        
        if (speedText != null)
            speedText.text = "Speed: " + speed.ToString();
        
        if (healthText != null)
            healthText.text = "Health: " + health.ToString();
        
        if (levelText != null)
            levelText.text = "Level: " + level.ToString();
    }
    
    // Get damage reduction from defense (can be used in combat calculations)
    public float GetDamageReduction()
    {
        // Simple formula: each point of defense reduces damage by 2%
        // Cap at 80% damage reduction to prevent invincibility
        float reduction = defense * 0.02f;
        return Mathf.Min(reduction, 0.8f);
    }
    
    // Calculate damage dealt based on attack stat
    public int CalculateDamage()
    {
        // Simple formula: base damage + random variation
        float baseDamage = attack;
        float variation = Random.Range(-attack * 0.2f, attack * 0.2f);
        return Mathf.RoundToInt(baseDamage + variation);
    }
    
    // Apply stat bonuses from equipment or buffs (can be expanded later)
    public void ApplyStatBonus(int attackBonus, int defenseBonus, int speedBonus, int healthBonus)
    {
        attack += attackBonus;
        defense += defenseBonus;
        speed += speedBonus;
        health += healthBonus;
        
        // Update health manager with new max health
        if (healthManager != null && healthBonus != 0)
        {
            healthManager.UpdateMaxHealth(health);
        }
        
        UpdateStatsDisplay();
    }
}
