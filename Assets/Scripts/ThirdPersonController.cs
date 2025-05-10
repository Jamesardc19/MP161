using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    
    // Interaction settings
    public float elevatorInteractionDistance = 2.5f;
    public LayerMask interactionLayers = -1; // All layers by default
    public Transform interactionOrigin; // Usually the camera or a point in front of the character
    public KeyCode elevatorInteractKey = KeyCode.F; // Changed from E to F for elevator interaction
    
    // Sword references
    public Transform swordTransform; // Assign the sword GameObject in the inspector
    public Collider swordCollider; // Assign the sword's collider in the inspector
    
    // Jump settings
    public float jumpHeight = 1.2f;
    public float jumpCooldown = 0.5f;
    
    // Defense settings
    public KeyCode defendKey = KeyCode.E;
    
    // Minimum value for MoveSpeed to avoid BlendTree warning
    private const float MIN_MOVE_SPEED = 0.01f;

    private CharacterController characterController;
    private Animator animator;
    private SetupAnimatorParameters animatorSetup;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isDizzy = false;
    private bool isDefending = false;
    private bool isJumping = false;
    private float attackCooldown = 0f;
    private float jumpCooldownTimer = 0f;
    private Transform cameraTransform;
    private Vector3 verticalVelocity = Vector3.zero; // For gravity
    private bool isGrounded = false;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animatorSetup = GetComponent<SetupAnimatorParameters>();
        
        // Set interaction origin to camera if not assigned
        if (interactionOrigin == null && playerCamera != null)
        {
            interactionOrigin = playerCamera.transform;
        }
        
        // Add SetupAnimatorParameters component if it doesn't exist
        if (animatorSetup == null)
        {
            animatorSetup = gameObject.AddComponent<SetupAnimatorParameters>();
            Debug.Log("Added SetupAnimatorParameters component automatically");
        }
        
        // Verify components are properly assigned
        if (animator == null)
        {
            Debug.LogError("Animator component is missing!");
        }
        else
        {
            Debug.Log("Animator found: " + animator.runtimeAnimatorController.name);
        }
        
        // Log initial state
        InvokeRepeating("LogAnimatorState", 1.0f, 1.0f);
        
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

        // Update jump cooldown
        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

        // Handle attack input
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isDizzy && !isDefending)
        {
            Debug.Log("Attack button pressed");
            StartAttack();
        }

        // Handle defend input (now using a different key than interact)
        if (Input.GetKeyDown(defendKey) && !isAttacking && !isDizzy)
        {
            Debug.Log("Defend key pressed (" + defendKey + ")");
            isDefending = true;
            animatorSetup.SetDefending(true);
        }
        else if (Input.GetKeyUp(defendKey) && isDefending)
        {
            Debug.Log("Defend key released");
            isDefending = false;
            animatorSetup.SetDefending(false);
        }
        
        // Handle interaction with elevators only (doors are automatic)
        if (Input.GetKeyDown(elevatorInteractKey))
        {
            Debug.Log("F key pressed for elevator interaction");
            if (!isAttacking && !isDizzy)
            {
                TryElevatorInteract();
            }
            else
            {
                Debug.Log("Cannot interact while attacking or dizzy");
            }
        }

        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed, isGrounded: " + isGrounded + ", jumpCooldown: " + jumpCooldownTimer + ", canJump: " + (!isAttacking && !isDizzy && !isDefending));
            if (isGrounded && jumpCooldownTimer <= 0 && !isAttacking && !isDizzy && !isDefending)
            {
                Jump(Input.GetKey(KeyCode.LeftAlt)); // Alt+Space for spin jump
            }
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
            
            // Reset jumping state when grounded, but only after a delay
            // to ensure the animation plays fully
            if (isJumping && jumpCooldownTimer <= 0)
            {
                isJumping = false;
                animatorSetup.SetJumping(false);
                Debug.Log("Jump state reset");
            }
        }
        else
        {
            // Apply gravity when in air
            verticalVelocity.y -= gravity * Time.deltaTime;
        }

        // Only allow movement if not attacking, not dizzy, and not defending
        if (direction.magnitude >= 0.1f && !isAttacking && !isDizzy && !isDefending)
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
            animatorSetup.SetMoveSpeed(speedValue);
        }
        else if (!isAttacking)
        {
            // Set to minimum value instead of zero to avoid BlendTree warning
            animatorSetup.SetMoveSpeed(MIN_MOVE_SPEED);
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
        
        // Victory animation test
        if (Input.GetKeyDown(KeyCode.V))
        {
            animatorSetup.TriggerVictory();
        }
        
        // Level up animation test (animation only, no stat distribution panel)
        if (Input.GetKeyDown(KeyCode.U))
        {
            animatorSetup.TriggerLevelUp();
            Debug.Log("Level up animation triggered (U key)");
        }
        
        // Get up animation test
        if (Input.GetKeyDown(KeyCode.G) && isDead)
        {
            animatorSetup.TriggerGetUp();
            SetDead(false);
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
        animatorSetup.TriggerAttack();
        
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
        animatorSetup.EndAttack();
        
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
        animatorSetup.SetDead(isDead);
        
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
        animatorSetup.SetDizzy(isDizzy);
    }
    
    private void Jump(bool doSpin = false)
    {
        // Apply vertical force for jump
        verticalVelocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
        
        // Set jumping state
        isJumping = true;
        jumpCooldownTimer = jumpCooldown;
        
        // Trigger jump animation
        if (animatorSetup != null)
        {
            Debug.Log("Triggering jump animation, doSpin: " + doSpin);
            animatorSetup.SetJumping(true, doSpin);
        }
        else
        {
            Debug.LogError("animatorSetup is null when trying to jump!");
        }
    }
    
    // Called when the character is hit while defending
    public void OnDefendHit()
    {
        if (isDefending)
        {
            animatorSetup.TriggerDefendHit();
        }
    }
    
    // Debug method to log animator state
    private void LogAnimatorState()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            string stateName = "Unknown";
            
            // Try to identify the current animation state name
            if (stateInfo.IsName("Idle_Battle_SwordAndShield")) stateName = "Idle_Battle";
            else if (stateInfo.IsName("JumpFullNormal_RM_SwordAndShield")) stateName = "JumpNormal";
            else if (stateInfo.IsName("JumpFullSpin_RM_SwordAndShield")) stateName = "JumpSpin";
            
            Debug.Log("Current Animation State: " + stateName + ", Hash: " + stateInfo.shortNameHash + ", Time: " + stateInfo.normalizedTime);
            Debug.Log("Jump Parameters - isJumping: " + animator.GetBool("isJumping") + 
                      ", isDizzy: " + animator.GetBool("isDizzy"));
        }
    }
    
    // Interaction system for elevators only
    private void TryElevatorInteract()
    {
        if (interactionOrigin == null)
        {
            Debug.LogWarning("Interaction origin not set");
            return;
        }
        
        Debug.Log("Trying elevator interaction from: " + interactionOrigin.position + ", direction: " + interactionOrigin.forward + ", distance: " + elevatorInteractionDistance);
        
        // Draw a debug ray in the scene view to visualize the interaction ray
        Debug.DrawRay(interactionOrigin.position, interactionOrigin.forward * elevatorInteractionDistance, Color.red, 2.0f);
        
        RaycastHit hit;
        if (Physics.Raycast(interactionOrigin.position, interactionOrigin.forward, out hit, elevatorInteractionDistance, interactionLayers))
        {
            Debug.Log("Interaction raycast hit: " + hit.transform.name + " at distance " + hit.distance);
            Debug.Log("Hit object layer: " + LayerMask.LayerToName(hit.transform.gameObject.layer));
            
            // Check for elevator buttons
            if (hit.collider.gameObject.name.StartsWith("Button floor") || hit.collider.gameObject.name.StartsWith("button floor"))
            {
                Debug.Log("Found elevator button: " + hit.collider.gameObject.name);
                
                var parentScript = hit.transform.GetComponent<pass_on_parent>();
                if (parentScript != null)
                {
                    Debug.Log("Found pass_on_parent component");
                    
                    if (parentScript.MyParent != null)
                    {
                        Debug.Log("MyParent reference is set to: " + parentScript.MyParent.name);
                        
                        // Try both original and fixed controller scripts
                        var originalController = parentScript.MyParent.GetComponent<evelator_controll>();
                        var fixedController = parentScript.MyParent.GetComponent<evelator_controll_fixed>();
                        
                        if (originalController != null && originalController.enabled)
                        {
                            Debug.Log("Using original elevator controller");
                            originalController.AddTaskEve(hit.collider.gameObject.name);
                            Debug.Log("Pressed elevator button: " + hit.collider.gameObject.name);
                        }
                        else if (fixedController != null && fixedController.enabled)
                        {
                            Debug.Log("Using fixed elevator controller");
                            fixedController.AddTaskEve(hit.collider.gameObject.name);
                            Debug.Log("Pressed elevator button: " + hit.collider.gameObject.name);
                        }
                        else
                        {
                            Debug.LogError("No active elevator controller found on parent: " + parentScript.MyParent.name);
                        }
                    }
                    else
                    {
                        Debug.LogError("pass_on_parent.MyParent is null on " + hit.collider.gameObject.name);
                    }
                }
                else
                {
                    Debug.LogError("No pass_on_parent component found on " + hit.collider.gameObject.name);
                }
            }
            else
            {
                Debug.Log("Hit object is not an elevator button: " + hit.collider.gameObject.name);
            }
        }
        else
        {
            Debug.Log("No object hit by interaction ray");
            
            // Let's try a more generous raycast to see what's around
            RaycastHit[] hits = Physics.SphereCastAll(interactionOrigin.position, 1.0f, interactionOrigin.forward, elevatorInteractionDistance);
            if (hits.Length > 0)
            {
                Debug.Log("Nearby objects detected with SphereCast:");
                foreach (RaycastHit nearbyHit in hits)
                {
                    Debug.Log("- " + nearbyHit.collider.gameObject.name + " at distance " + nearbyHit.distance);
                }
            }
        }
    }
    
    // Handle automatic door opening on collision
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if we hit a door
        if (hit.gameObject.CompareTag("door"))
        {
            Door doorScript = hit.gameObject.GetComponent<Door>();
            if (doorScript != null)
            {
                doorScript.ActionDoor();
                Debug.Log("Automatically interacted with door: " + hit.gameObject.name);
            }
        }
    }
    
    // Draw interaction ray in Scene view for debugging
    private void OnDrawGizmos()
    {
        if (interactionOrigin != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(interactionOrigin.position, interactionOrigin.forward * elevatorInteractionDistance);
        }
    }
}
