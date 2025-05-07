using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damageAmount = 10;
    
    // This method can be called when a weapon hits an enemy
    public void DealDamage(GameObject target)
    {
        HealthManager healthManager = target.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            healthManager.TakeDamage(damageAmount);
        }
    }
    
    // This can be used for collisions with enemies or hazards
    private void OnTriggerEnter(Collider other)
    {
        // Check if this is a player or enemy getting hit
        HealthManager healthManager = other.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            healthManager.TakeDamage(damageAmount);
        }
    }
}
