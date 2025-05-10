using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class heathBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    public Slider healthSlider;          // Main health slider
    public Slider easeHealthSlider;      // Delayed health slider for damage visualization
    public Image fillImage;              // Optional: Reference to the fill image to change color
    public Gradient healthGradient;      // Optional: Color gradient based on health percentage
    
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float health;
    public float lerpSpeed = 0.05f;      // Speed of the health bar animation
    
    [Header("References")]
    [SerializeField] private CharacterStats characterStats; // Reference to the character stats (optional)
    void Start()
    {
        // Initialize health
        health = maxHealth;
        
        // Set initial slider values
        if (healthSlider != null)
            healthSlider.value = health / maxHealth;
            
        if (easeHealthSlider != null)
            easeHealthSlider.value = health / maxHealth;
            
        // Get reference to character stats if not assigned and if the script exists
        if (characterStats == null)
        {
            characterStats = GetComponentInParent<CharacterStats>();
        }
            
        // Sync with character stats if available
        if (characterStats != null)
        {
            // Try to get the health value from CharacterStats
            try
            {
                maxHealth = characterStats.health;
                health = maxHealth;
                UpdateHealthBar();
            }
            catch (System.Exception)
            {
                // If there's an error accessing the health property, just use default values
                Debug.LogWarning("Could not access health from CharacterStats. Using default values.");
            }
        }
    }

    void Update()
    {
        // Update the main health slider
        if (healthSlider != null && healthSlider.value != health / maxHealth)
        {
            healthSlider.value = health / maxHealth;
            
            // Update color based on health percentage if gradient is assigned
            if (fillImage != null && healthGradient != null)
            {
                fillImage.color = healthGradient.Evaluate(healthSlider.value);
            }
        }
        
        // Test damage with space key (for testing only)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }
        
        // Smoothly animate the ease health slider
        if (easeHealthSlider != null && healthSlider != null && easeHealthSlider.value != healthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, healthSlider.value, lerpSpeed * Time.deltaTime * 10);
        }
    }
    // Public method to take damage
    public void TakeDamage(float damage)
    {
        health -= damage;
        
        // Clamp health to prevent negative values
        health = Mathf.Max(0, health);
        
        // Update the health bar
        UpdateHealthBar();
        
        // Check if character died
        if (health <= 0)
        {
            OnDeath();
        }
    }
    
    // Public method to heal
    public void Heal(float amount)
    {
        health += amount;
        
        // Clamp health to prevent exceeding max health
        health = Mathf.Min(maxHealth, health);
        
        // Update the health bar
        UpdateHealthBar();
    }
    
    // Update health bar visuals
    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = health / maxHealth;
        }
    }
    
    // Called when health reaches zero
    private void OnDeath()
    {
        // Get the ThirdPersonController component and call SetDead
        ThirdPersonController controller = GetComponentInParent<ThirdPersonController>();
        if (controller != null)
        {
            controller.SetDead(true);
        }
    }
    
    // Update max health (called when stats change)
    public void UpdateMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        
        // Optionally scale current health proportionally
        float healthPercent = health / maxHealth;
        health = healthPercent * newMaxHealth;
        
        UpdateHealthBar();
    }
}
