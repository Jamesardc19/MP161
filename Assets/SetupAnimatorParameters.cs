using UnityEngine;

/// <summary>
/// This script is responsible for setting up all animator parameters and transitions for the character animations.
/// </summary>
[RequireComponent(typeof(Animator))]
public class SetupAnimatorParameters : MonoBehaviour
{
    private Animator animator;
    
    // Animation clip names
    private const string IDLE_BATTLE = "Idle_Battle_SwordAndShield";
    private const string IDLE_NORMAL = "Idle_Normal_SwordAndShield";
    private const string MOVE_FWD_BATTLE = "MoveFWD_Battle_RM_SwordAndShield";
    private const string MOVE_FWD_INPLACE = "MoveFWD_Battle_InPlace_SwordAndShield";
    private const string MOVE_BWD_INPLACE = "MoveBWD_Battle_InPlace_SwordAndShield";
    private const string SPRINT_FWD_INPLACE = "SprintFWD_Battle_InPlace_SwordAndShield";
    private const string ATTACK_01 = "Attack01_SwordAndShield";
    private const string ATTACK_02 = "Attack02_SwordAndShield";
    private const string ATTACK_03 = "Attack03_SwordAndShield";
    private const string ATTACK_04 = "Attack04_SwordAndShield";
    private const string DEFEND = "Defend_SwordAndShield";
    private const string DEFEND_HIT = "DefendHit_SwordAndShield";
    private const string DIE_01 = "Die01_SwordAndShield";
    private const string DIE_01_STAY = "Die01_Stay_SwordAndShield";
    private const string GET_UP = "GetUp_SwordAndShield";
    private const string VICTORY = "Victory_Battle_SwordAndShield";
    private const string LEVEL_UP = "LevelUp_Battle_SwordAndShield";
    private const string DIZZY = "Dizzy_SwordAndShield";
    private const string JUMP_SPIN = "JumpFullSpin_RM_SwordAndShield";
    private const string JUMP_NORMAL = "JumpFullNormal_RM_SwordAndShield";
    
    // Animation parameters
    private int moveSpeedHash;
    private int isAttackingHash;
    private int attackIndexHash;
    private int isDeadHash;
    private int isDizzyHash;
    private int isDefendingHash;
    private int isHitHash;
    private int isJumpingHash;
    private int isSpinJumpHash;
    private int isVictoryHash;
    private int isLevelUpHash;
    
    // Attack state tracking
    private int currentAttackIndex = 1;
    private float lastAttackTime = 0f;
    private float attackComboWindow = 1.5f; // Time window to chain attacks
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        
        // Cache parameter hashes for better performance
        moveSpeedHash = Animator.StringToHash("MoveSpeed");
        isAttackingHash = Animator.StringToHash("isAttacking");
        attackIndexHash = Animator.StringToHash("AttackIndex");
        isDeadHash = Animator.StringToHash("isDead");
        isDizzyHash = Animator.StringToHash("isDizzy");
        isDefendingHash = Animator.StringToHash("isDefending");
        isHitHash = Animator.StringToHash("isHit");
        isJumpingHash = Animator.StringToHash("isJumping");
        isSpinJumpHash = Animator.StringToHash("isSpinJump");
        isVictoryHash = Animator.StringToHash("isVictory");
        isLevelUpHash = Animator.StringToHash("isLevelUp");
        
        // Initialize parameters
        ResetAllParameters();
    }
    
    void ResetAllParameters()
    {
        animator.SetFloat(moveSpeedHash, 0.01f); // Minimum value to avoid BlendTree warning
        animator.SetBool(isAttackingHash, false);
        animator.SetInteger(attackIndexHash, 0);
        animator.SetBool(isDeadHash, false);
        animator.SetBool(isDizzyHash, false);
        animator.SetBool(isDefendingHash, false);
        animator.SetBool(isHitHash, false);
        animator.SetBool(isJumpingHash, false);
        animator.SetBool(isSpinJumpHash, false);
        animator.SetBool(isVictoryHash, false);
        animator.SetBool(isLevelUpHash, false);
    }
    
    /// <summary>
/// Sets the movement speed parameter for the animator.
/// </summary>
    /// <param name="speed">Movement speed value (0-2, where 1=walk, 2=run)</param>
    public void SetMoveSpeed(float speed)
    {
        animator.SetFloat(moveSpeedHash, Mathf.Max(speed, 0.01f)); // Ensure minimum value
    }
    
    /// <summary>
/// Triggers the next attack in the combo sequence.
/// </summary>
    public void TriggerAttack()
    {
        // Check if we're within the combo window
        if (Time.time - lastAttackTime > attackComboWindow)
        {
            currentAttackIndex = 1; // Reset to first attack if combo window expired
        }
        
        // Set attack parameters
        animator.SetBool(isAttackingHash, true);
        animator.SetInteger(attackIndexHash, currentAttackIndex);
        
        // Update attack tracking
        lastAttackTime = Time.time;
        
        // Increment attack index for next attack (cycle through 1-4)
        currentAttackIndex = currentAttackIndex % 4 + 1;
    }
    
    /// <summary>
/// Ends the current attack animation.
/// </summary>
    public void EndAttack()
    {
        animator.SetBool(isAttackingHash, false);
    }
    
    /// <summary>
/// Sets the defending state.
/// </summary>
    /// <param name="isDefending">Whether the character is defending</param>
    public void SetDefending(bool isDefending)
    {
        animator.SetBool(isDefendingHash, isDefending);
    }
    
    /// <summary>
/// Triggers the defend hit reaction when hit while defending.
/// </summary>
    public void TriggerDefendHit()
    {
        animator.SetBool(isHitHash, true);
        
        // Reset after a short delay (animation event would be better)
        Invoke(nameof(ResetHitState), 0.5f);
    }
    
    private void ResetHitState()
    {
        animator.SetBool(isHitHash, false);
    }
    
    /// <summary>
/// Sets the jumping state.
/// </summary>
    /// <param name="isJumping">Whether the character is jumping</param>
    /// <param name="doSpin">Whether to do a spin jump or normal jump</param>
    public void SetJumping(bool isJumping, bool doSpin = false)
    {
        animator.SetBool(isJumpingHash, isJumping);
        
        // Set the spin jump parameter based on the doSpin flag
        animator.SetBool(isSpinJumpHash, isJumping && doSpin);
        
        Debug.Log($"Jump triggered - Regular Jump: {isJumping}, Spin Jump: {isJumping && doSpin}");
    }
    
    /// <summary>
/// Sets the dead state.
/// </summary>
    /// <param name="isDead">Whether the character is dead</param>
    public void SetDead(bool isDead)
    {
        animator.SetBool(isDeadHash, isDead);
    }
    
    /// <summary>
/// Sets the dizzy state.
/// </summary>
    /// <param name="isDizzy">Whether the character is dizzy</param>
    public void SetDizzy(bool isDizzy)
    {
        animator.SetBool(isDizzyHash, isDizzy);
    }
    
    /// <summary>
/// Triggers the victory animation.
/// </summary>
    public void TriggerVictory()
    {
        animator.SetBool(isVictoryHash, true);
        
        // Reset after animation duration (animation event would be better)
        Invoke(nameof(ResetVictoryState), 3.0f);
    }
    
    private void ResetVictoryState()
    {
        animator.SetBool(isVictoryHash, false);
    }
    
    /// <summary>
/// Triggers the level up animation.
/// </summary>
    public void TriggerLevelUp()
    {
        animator.SetBool(isLevelUpHash, true);
        
        // Reset after animation duration (animation event would be better)
        Invoke(nameof(ResetLevelUpState), 3.0f);
    }
    
    private void ResetLevelUpState()
    {
        animator.SetBool(isLevelUpHash, false);
    }
    
    /// <summary>
/// Triggers the get up animation after death.
/// </summary>
    public void TriggerGetUp()
    {
        // First ensure we're in dead state
        if (animator.GetBool(isDeadHash))
        {
            // Then trigger get up (could use a specific parameter or trigger)
            animator.SetBool(isDeadHash, false);
            
            // You might need a specific trigger for GetUp if the transition
            // from Die01_Stay_SwordAndShield to GetUp_SwordAndShield needs it
        }
    }
}