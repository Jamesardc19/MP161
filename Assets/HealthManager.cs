using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Slider healthBar;
    public int maxHealth = 100;
    private int currentHealth;
    
    // Reference to the character stats system
    private CharacterStats characterStats;
    // Reference to the ThirdPersonController
    private ThirdPersonController playerController;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Get reference to CharacterStats if available
        characterStats = GetComponent<CharacterStats>();
        
        // Get reference to ThirdPersonController
        playerController = GetComponent<ThirdPersonController>();
        
        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        
        // If we have character stats, sync our health with it
        if (characterStats != null)
        {
            maxHealth = characterStats.health;
            currentHealth = maxHealth;
            UpdateHealthBar();
        }
    }
    
    // Call this method to decrease health
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        
        UpdateHealthBar();
    }
    
    // Call this method to heal or increase health
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthBar();
    }
    
    // Updates the health bar UI
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }
    
    // Called when health reaches zero
    private void Die()
    {
        if (playerController != null)
        {
            playerController.SetDead(true);
        }
    }
    
    // Update max health (called when stats change)
    public void UpdateMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
        }
        
        // Optionally heal to new max health when leveling up
        currentHealth = maxHealth;
        UpdateHealthBar();
    }
    
    // Get current health percentage (0-1)
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    // Get current health value
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
