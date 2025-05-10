using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float detectionRadius = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 5f;
    public float idleTime = 2f;
    public float wanderRadius = 5f;
    
    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    public int attackDamage = 10;
    public float attackAnimationDuration = 1.2f;
    
    [Header("References")]
    public Transform playerDetectionOrigin;
    public LayerMask playerLayer;
    
    // State machine
    public enum EnemyState { Idle, Wander, Chase, Attack, Hit, Dead }
    [HideInInspector]
    public EnemyState currentState = EnemyState.Idle;
    
    // Components
    private NavMeshAgent navAgent;
    private Animator animator;
    private SetupAnimatorParameters animatorSetup;
    private EnemyStats enemyStats;
    private HealthManager healthManager;
    private Transform playerTransform;
    private CharacterStats playerStats;
    
    // Internal variables
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isActive = false;
    private Vector3 wanderTarget;
    private float stateTimer;
    
    private void Start()
    {
        // Get components
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animatorSetup = GetComponent<SetupAnimatorParameters>();
        enemyStats = GetComponent<EnemyStats>();
        healthManager = GetComponent<HealthManager>();
        
        // Add components if missing
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = rotationSpeed * 100;
            navAgent.acceleration = 8f;
            navAgent.stoppingDistance = attackRange * 0.8f;
        }
        
        if (animatorSetup == null)
        {
            animatorSetup = gameObject.AddComponent<SetupAnimatorParameters>();
        }
        
        // Set up detection origin if not assigned
        if (playerDetectionOrigin == null)
        {
            playerDetectionOrigin = transform;
        }
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerStats = player.GetComponent<CharacterStats>();
        }
        
        // Subscribe to health manager death event
        if (healthManager != null)
        {
            // Use reflection to add a method to the Die method
            var dieMethod = healthManager.GetType().GetMethod("Die", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dieMethod != null)
            {
                // We can't directly modify the method, so we'll check health in Update
                Debug.Log("Will monitor health for death state");
            }
        }
        
        // Start in inactive state (will be activated by trigger)
        SetActive(false);
        
        // Start state machine
        ChangeState(EnemyState.Idle);
    }
    
    private void Update()
    {
        if (!isActive || isDead) return;
        
        // Check for death
        if (healthManager != null && healthManager.GetCurrentHealth() <= 0 && !isDead)
        {
            Die();
            return;
        }
        
        // Update state machine
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdleState();
                break;
            case EnemyState.Wander:
                UpdateWanderState();
                break;
            case EnemyState.Chase:
                UpdateChaseState();
                break;
            case EnemyState.Attack:
                UpdateAttackState();
                break;
            case EnemyState.Hit:
                UpdateHitState();
                break;
        }
        
        // Update animator parameters
        if (navAgent != null && animatorSetup != null)
        {
            float speed = navAgent.velocity.magnitude;
            animatorSetup.SetMoveSpeed(speed);
        }
    }
    
    // State machine methods
    private void ChangeState(EnemyState newState)
    {
        // Exit current state
        switch (currentState)
        {
            case EnemyState.Idle:
                ExitIdleState();
                break;
            case EnemyState.Wander:
                ExitWanderState();
                break;
            case EnemyState.Chase:
                ExitChaseState();
                break;
            case EnemyState.Attack:
                ExitAttackState();
                break;
            case EnemyState.Hit:
                ExitHitState();
                break;
        }
        
        // Set new state
        currentState = newState;
        stateTimer = 0f;
        
        // Enter new state
        switch (newState)
        {
            case EnemyState.Idle:
                EnterIdleState();
                break;
            case EnemyState.Wander:
                EnterWanderState();
                break;
            case EnemyState.Chase:
                EnterChaseState();
                break;
            case EnemyState.Attack:
                EnterAttackState();
                break;
            case EnemyState.Hit:
                EnterHitState();
                break;
            case EnemyState.Dead:
                EnterDeadState();
                break;
        }
    }
    
    // Idle state methods
    private void EnterIdleState()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
        
        stateTimer = Random.Range(idleTime * 0.5f, idleTime * 1.5f);
    }
    
    private void UpdateIdleState()
    {
        // Check for player in detection radius
        if (DetectPlayer())
        {
            ChangeState(EnemyState.Chase);
            return;
        }
        
        // After idle time, start wandering
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            ChangeState(EnemyState.Wander);
        }
    }
    
    private void ExitIdleState()
    {
        // Nothing specific to do
    }
    
    // Wander state methods
    private void EnterWanderState()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = false;
            
            // Find a random point to wander to
            wanderTarget = GetRandomWanderPoint();
            navAgent.SetDestination(wanderTarget);
        }
        
        stateTimer = Random.Range(3f, 6f);
    }
    
    private void UpdateWanderState()
    {
        // Check for player in detection radius
        if (DetectPlayer())
        {
            ChangeState(EnemyState.Chase);
            return;
        }
        
        // Check if we've reached the wander target or if the timer is up
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0 || (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.5f))
        {
            ChangeState(EnemyState.Idle);
        }
    }
    
    private void ExitWanderState()
    {
        // Nothing specific to do
    }
    
    // Chase state methods
    private void EnterChaseState()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = false;
        }
    }
    
    private void UpdateChaseState()
    {
        // If player is null or too far, go back to idle
        if (playerTransform == null || Vector3.Distance(transform.position, playerTransform.position) > detectionRadius * 1.5f)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // Update destination to follow player
        if (navAgent != null)
        {
            navAgent.SetDestination(playerTransform.position);
        }
        
        // If within attack range, attack
        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange && 
            Time.time - lastAttackTime >= attackCooldown)
        {
            ChangeState(EnemyState.Attack);
        }
    }
    
    private void ExitChaseState()
    {
        // Nothing specific to do
    }
    
    // Attack state methods
    private void EnterAttackState()
    {
        isAttacking = true;
        
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
        
        // Face the player
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Trigger attack animation
        if (animatorSetup != null)
        {
            animatorSetup.TriggerAttack();
        }
        
        lastAttackTime = Time.time;
        stateTimer = attackAnimationDuration;
        
        // Deal damage to player
        DealDamageToPlayer();
    }
    
    private void UpdateAttackState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            isAttacking = false;
            
            // Return to chase state
            ChangeState(EnemyState.Chase);
        }
    }
    
    private void ExitAttackState()
    {
        isAttacking = false;
    }
    
    // Hit state methods
    private void EnterHitState()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
        
        stateTimer = 0.5f; // Hit reaction time
    }
    
    private void UpdateHitState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            // Return to chase if player is detected, otherwise idle
            if (DetectPlayer())
            {
                ChangeState(EnemyState.Chase);
            }
            else
            {
                ChangeState(EnemyState.Idle);
            }
        }
    }
    
    private void ExitHitState()
    {
        // Nothing specific to do
    }
    
    // Dead state methods
    private void EnterDeadState()
    {
        isDead = true;
        
        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }
        
        // Trigger death animation
        if (animatorSetup != null)
        {
            animatorSetup.SetDead(true);
        }
        
        // Disable colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        
        // Hide stats UI
        if (enemyStats != null)
        {
            enemyStats.ToggleStatsUI(false);
        }
        
        // Destroy after delay
        Destroy(gameObject, 5f);
    }
    
    // Helper methods
    private bool DetectPlayer()
    {
        if (playerTransform == null) return false;
        
        float distanceToPlayer = Vector3.Distance(playerDetectionOrigin.position, playerTransform.position);
        
        // Simple distance check
        if (distanceToPlayer <= detectionRadius)
        {
            // Line of sight check
            Vector3 directionToPlayer = (playerTransform.position - playerDetectionOrigin.position).normalized;
            Ray ray = new Ray(playerDetectionOrigin.position, directionToPlayer);
            
            if (Physics.Raycast(ray, out RaycastHit hit, detectionRadius, playerLayer))
            {
                if (hit.transform == playerTransform)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private Vector3 GetRandomWanderPoint()
    {
        // Get a random point within wanderRadius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        
        // Find a valid point on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
        {
            return hit.position;
        }
        
        // If no valid point found, return current position
        return transform.position;
    }
    
    private void DealDamageToPlayer()
    {
        if (playerTransform == null) return;
        
        // Check if player is within attack range
        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange * 1.2f)
        {
            // Get player's health manager
            HealthManager playerHealthManager = playerTransform.GetComponent<HealthManager>();
            if (playerHealthManager != null)
            {
                // Calculate damage based on enemy stats
                int damage = attackDamage;
                if (enemyStats != null)
                {
                    damage = enemyStats.CalculateDamage();
                }
                
                // Apply damage
                playerHealthManager.TakeDamage(damage);
                
                Debug.Log($"Enemy dealt {damage} damage to player");
            }
        }
    }
    
    // Public methods
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        // Apply damage through EnemyStats
        if (enemyStats != null)
        {
            enemyStats.TakeDamage(damage);
        }
        else if (healthManager != null)
        {
            healthManager.TakeDamage(damage);
        }
        
        // Change to hit state if not already in it
        if (currentState != EnemyState.Hit && currentState != EnemyState.Dead)
        {
            ChangeState(EnemyState.Hit);
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        ChangeState(EnemyState.Dead);
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (navAgent != null)
        {
            navAgent.enabled = active;
        }
        
        // If becoming active, start in idle state
        if (active && currentState != EnemyState.Dead)
        {
            ChangeState(EnemyState.Idle);
        }
        
        // Toggle stats UI
        if (enemyStats != null)
        {
            enemyStats.ToggleStatsUI(active);
        }
    }
    
    // Draw gizmos for visualization in editor
    private void OnDrawGizmosSelected()
    {
        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Wander radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
