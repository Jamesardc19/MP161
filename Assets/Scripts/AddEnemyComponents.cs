using UnityEngine;

public class AddEnemyComponents : MonoBehaviour
{
    public void AddComponents()
    {
        // Add EnemyStats if it doesn't exist
        if (GetComponent<EnemyStats>() == null)
        {
            gameObject.AddComponent<EnemyStats>();
            Debug.Log("Added EnemyStats component");
        }
        
        // Add HealthManager if it doesn't exist
        if (GetComponent<HealthManager>() == null)
        {
            gameObject.AddComponent<HealthManager>();
            Debug.Log("Added HealthManager component");
        }
        
        // Add NavMeshAgent if it doesn't exist
        if (GetComponent<UnityEngine.AI.NavMeshAgent>() == null)
        {
            gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
            Debug.Log("Added NavMeshAgent component");
        }
        
        // Add EnemyController if it doesn't exist
        if (GetComponent<EnemyController>() == null)
        {
            gameObject.AddComponent<EnemyController>();
            Debug.Log("Added EnemyController component");
        }
    }
}
