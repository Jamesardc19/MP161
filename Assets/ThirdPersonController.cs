using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float rotationSpeed = 700f;
    public Camera playerCamera;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float gravity = 20f; // Gravity force
    public float groundedOffset = 0.1f; // Small offset to check if grounded
    public float groundedRadius = 0.5f; // Radius to check if grounded
    public LayerMask groundLayers = -1; // All layers by default
    
    // Sword references
    public Transform swordTransform; // Assign the sword GameObject in the inspector
    public Collider swordCollider; // Assign the sword's collider in the inspector
    
    // Minimum value for MoveSpeed to avoid BlendTree warning
    private const float MIN_MOVE_SPEED = 0.01f;

    private CharacterController characterController;
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isDizzy = false;
    private float attackCooldown = 0f;
    private Transform cameraTransform;
    private Vector3 verticalVelocity = Vector3.zero; // For gravity
    private bool isGrounded = false;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        // Set up camera reference
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
            // Camera positioning is now handled by ThirdPersonCameraController
        }
        else
        {
            Debug.LogError("No camera assigned to ThirdPersonController and no Main Camera found!");
        }
        
        // Set up sword collider if assigned
        SetupSwordCollider();
    }
    
    private void SetupSwordCollider()
    {
        if (swordCollider != null)
        {
            // Make sure the sword collider is not a trigger
            swordCollider.isTrigger = false;
            
            // Get or add Rigidbody to the sword
            Rigidbody swordRb = swordCollider.GetComponent<Rigidbody>();
            if (swordRb == null && swordTransform != null)
            {
                swordRb = swordTransform.gameObject.AddComponent<Rigidbody>();
            }
            
            if (swordRb != null)
            {
                // Configure Rigidbody for proper collision
                swordRb.isKinematic = true; // Make it kinematic so it follows animations
                swordRb.useGravity = false; // No gravity needed
                swordRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Better collision detection
                Debug.Log("Sword Rigidbody configured successfully");
            }
            else
            {
                Debug.LogWarning("Could not find or add Rigidbody to sword");
            }
        }
        else
        {
            Debug.LogWarning("No sword collider assigned. Assign it in the Inspector.");
        }
    }

    private void LateUpdate()
    {
        // This method is intentionally left empty
        // Camera positioning is now handled by ThirdPersonCameraController
    }

    private void Update()
    {
        if (isDead)
        {
            // Don't process input if character is dead
            return;
        }

        // Check if grounded
        CheckGroundStatus();

        // Handle attack input
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isDizzy)
        {
            StartAttack();
        }

        // Handle attack cooldown
        if (isAttacking)
        {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0)
            {
                EndAttack();
            }
        }

        // Get movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Check for sprint input (Left Shift)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Fix for single key movement - ensure direction is normalized only if both keys are pressed
        Vector3 direction = new Vector3(horizontal, 0, vertical);
        
        // Only normalize if magnitude > 1 to preserve single-key full movement
        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }
        
        Vector3 moveDirection = Vector3.zero;

        // Apply gravity
        if (isGrounded)
        {
            // Reset vertical velocity when grounded
            verticalVelocity.y = -2f; // Small downward force to keep character grounded
        }
        else
        {
            // Apply gravity when in air
            verticalVelocity.y -= gravity * Time.deltaTime;
        }

        // Only allow movement if not attacking and not dizzy
        if (direction.magnitude >= 0.1f && !isAttacking && !isDizzy)
        {
            // Calculate movement direction relative to camera
            Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            moveDirection = (vertical * cameraForward + horizontal * cameraTransform.right).normalized;
            
            // Rotate the character to face movement direction
            if (moveDirection != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }

            // Calculate animation speed value
            // For single key presses, ensure we get full value (1.0)
            float directionMagnitude = Mathf.Max(Mathf.Abs(horizontal), Mathf.Abs(vertical));
            
            // Scale by sprint factor if sprinting
            float speedValue = isSprinting ? directionMagnitude * 2f : directionMagnitude;
            
            // Ensure minimum value to avoid BlendTree warning
            speedValue = Mathf.Max(speedValue, MIN_MOVE_SPEED);
            animator.SetFloat("MoveSpeed", speedValue);
        }
        else if (!isAttacking)
        {
            // Set to minimum value instead of zero to avoid BlendTree warning
            animator.SetFloat("MoveSpeed", MIN_MOVE_SPEED);
        }

        // Apply movement and gravity
        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
        movement += verticalVelocity * Time.deltaTime;
        characterController.Move(movement);

        // Debug keys for testing animations
        if (Input.GetKeyDown(KeyCode.K))
        {
            SetDead(!isDead);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SetDizzy(!isDizzy);
        }
    }

    private void CheckGroundStatus()
    {
        // Check if the character is grounded using a spherecast
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    // Camera positioning is now handled by ThirdPersonCameraController

    private void StartAttack()
    {
        isAttacking = true;
        attackCooldown = 1.0f; // Adjust based on your attack animation length
        animator.SetBool("IsAttacking", true);
        
        // Enable sword collider during attack
        if (swordCollider != null)
        {
            // Enable the collider for collision detection during attack
            swordCollider.enabled = true;
            Debug.Log("Sword collider enabled for attack");
        }
    }
    
    // Called when attack animation ends or is interrupted
    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("IsAttacking", false);
        
        // Disable sword collider after attack
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
            Debug.Log("Sword collider disabled after attack");
        }
    }

    public void SetDead(bool dead)
    {
        isDead = dead;
        animator.SetBool("IsDead", isDead);
        
        // Disable character controller if dead
        if (isDead)
        {
            characterController.enabled = false;
        }
        else
        {
            characterController.enabled = true;
        }
    }

    public void SetDizzy(bool dizzy)
    {
        isDizzy = dizzy;
        animator.SetBool("IsDizzy", isDizzy);
    }
}
