using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int attack = 8;
    public int defense = 3;
    public int speed = 4;
    public int health = 80;
    public int level = 1;
    
    [Header("UI References")]
    public GameObject statsCanvas;
    public Image healthBarImage;
    public Text enemyNameText;
    public Text levelText;
    
    // Reference to the health manager
    private HealthManager healthManager;
    // Reference to the animator setup
    private SetupAnimatorParameters animatorSetup;
    
    void Start()
    {
        // Get reference to HealthManager
        healthManager = GetComponent<HealthManager>();
        if (healthManager == null)
        {
            healthManager = gameObject.AddComponent<HealthManager>();
            healthManager.maxHealth = health;
            healthManager.currentHealth = health;
            
            // Set health bar reference
            if (healthBarImage != null)
            {
                healthManager.healthBarImage = healthBarImage;
            }
        }
        
        // Get reference to SetupAnimatorParameters
        animatorSetup = GetComponent<SetupAnimatorParameters>();
        if (animatorSetup == null)
        {
            animatorSetup = gameObject.AddComponent<SetupAnimatorParameters>();
        }
        
        // Update UI
        UpdateStatsDisplay();
    }
    
    // Update the stats displayed in the UI
    private void UpdateStatsDisplay()
    {
        if (enemyNameText != null)
        {
            enemyNameText.text = gameObject.name;
        }
        
        if (levelText != null)
        {
            levelText.text = "Level: " + level.ToString();
        }
        
        // Update health bar
        if (healthManager != null && healthBarImage != null)
        {
            healthManager.RefreshHealthBar();
        }
    }
    
    // Get damage reduction from defense (can be used in combat calculations)
    public float GetDamageReduction()
    {
        // Simple formula: each point of defense reduces damage by 2%
        // Cap at 70% damage reduction to prevent invincibility
        float reduction = defense * 0.02f;
        return Mathf.Min(reduction, 0.7f);
    }
    
    // Calculate damage dealt based on attack stat
    public int CalculateDamage()
    {
        // Simple formula: base damage + random variation
        float baseDamage = attack;
        float variation = Random.Range(-attack * 0.2f, attack * 0.2f);
        return Mathf.RoundToInt(baseDamage + variation);
    }
    
    // Take damage with defense calculation
    public void TakeDamage(int incomingDamage)
    {
        if (healthManager == null) return;
        
        // Apply defense reduction
        float damageReduction = GetDamageReduction();
        int actualDamage = Mathf.RoundToInt(incomingDamage * (1f - damageReduction));
        
        // Ensure minimum damage of 1
        actualDamage = Mathf.Max(1, actualDamage);
        
        // Apply damage to health manager
        healthManager.TakeDamage(actualDamage);
        
        // Play hit animation if available
        if (animatorSetup != null)
        {
            animatorSetup.TriggerDefendHit();
        }
    }
    
    // Level up the enemy (can be called when player progresses)
    public void LevelUp()
    {
        level++;
        
        // Increase stats
        attack += 2;
        defense += 1;
        speed += 1;
        health += 15;
        
        // Update health manager with new max health
        if (healthManager != null)
        {
            healthManager.UpdateMaxHealth(health);
        }
        
        // Play level up animation if available
        if (animatorSetup != null)
        {
            // Check if the method exists using reflection
            var method = animatorSetup.GetType().GetMethod("TriggerLevelUp");
            if (method != null)
            {
                method.Invoke(animatorSetup, null);
            }
            else
            {
                // Fallback to victory animation if level up doesn't exist
                animatorSetup.TriggerVictory();
            }
        }
        
        UpdateStatsDisplay();
    }
    
    // Toggle the stats UI visibility
    public void ToggleStatsUI(bool visible)
    {
        if (statsCanvas != null)
        {
            statsCanvas.SetActive(visible);
        }
    }
}
