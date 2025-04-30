using UnityEngine;

/// <summary>
/// Manages the boss's melee weapon collision detection based on animation frames and applies damage to the player.
/// </summary>
public class Boss1Weapon : MonoBehaviour
{
    private Azmodan.Phase1.BossPhase1 bossPhase1;
    private BoxCollider weaponCollider;
    private bool isAttacking = false;
    private DamageController playerDamageController;
    [SerializeField] private int attack1Damage = 20;

    [Header("Animation Frame Settings")]
    [SerializeField] private int attackStartFrame = 17; // Frame to activate collider
    [SerializeField] private int attackEndFrame = 30;   // Frame to deactivate collider
    [SerializeField] private int totalAnimationFrames = 60; // Total animation frame count

    void Start()
    {
        // Find BossPhase1 component in parent object
        bossPhase1 = GetComponentInParent<Azmodan.Phase1.BossPhase1>();
        
        // Get or add weapon collider
        weaponCollider = GetComponent<BoxCollider>();
        if (!weaponCollider)
        {
            Debug.LogWarning("No collider found!");
            return;
        }
        
        // Set as trigger (detect collisions without physical interaction)
        weaponCollider.isTrigger = true;
        
        // Initially disable collider
        weaponCollider.enabled = false;
        
        playerDamageController = FindFirstObjectByType<DamageController>();
        if (!playerDamageController)
        {
            Debug.LogError("Cannot load data because DamageController does not exist.");
        }
    }

    void Update()
    {
        // Draw debug line when collider is enabled
        if (weaponCollider.enabled)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.up * 2, Color.red);
        }
        
        // Check if boss is in Attack1 state
        if (bossPhase1 && bossPhase1.GetSelectedAttackType() == BossStateType.Attack1)
        {
            Animator animator = bossPhase1.GetComponent<Animator>();
            if (animator)
            {
                // Get current animation state info
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                
                // Check if in attack animation
                if (stateInfo.IsName("Attack"))
                {
                    // Current animation progress (0.0 - 1.0)
                    float normalizedTime = stateInfo.normalizedTime % 1.0f; // Account for looping animations
                    
                    // Convert progress to frame number
                    int currentFrame = Mathf.FloorToInt(normalizedTime * totalAnimationFrames);
                    
                    // Check if within specified frame range (17-30)
                    if (currentFrame >= attackStartFrame && currentFrame <= attackEndFrame)
                    {
                        // Debug log
                        // if (!weaponCollider.enabled)
                        // {
                        //     Debug.Log($"Weapon collider activated (frame: {currentFrame})");
                        // }
                        
                        // Enable collider
                        weaponCollider.enabled = true;
                        isAttacking = true;
                    }
                    else
                    {
                        // Debug log
                        // if (weaponCollider.enabled)
                        // {
                        //     Debug.Log($"Weapon collider deactivated (frame: {currentFrame})");
                        // }
                        
                        // Disable collider
                        weaponCollider.enabled = false;
                        isAttacking = false;
                    }
                }
                else
                {
                    // Not in attack animation, disable collider
                    weaponCollider.enabled = false;
                    isAttacking = false;
                }
            }
        }
        else
        {
            // Not in attack state, disable collider
            weaponCollider.enabled = false;
            isAttacking = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Log all collisions
        // Debug.Log($"Weapon collided with {other.gameObject.name} tag:{other.tag}");
        
        if (isAttacking && other.CompareTag("Player"))
        {
            // Debug.Log("Melee attack!!");
            
            // Apply damage logic
            playerDamageController.PlayerTakeDamage(attack1Damage);
            Debug.Log($"Boss1 melee attack: Dealt {attack1Damage} damage to player!");
            
            // SamplePlayer player = other.GetComponent<SamplePlayer>();
            // if (player)
            // {
            //     // Use same damage value as defined in BossPhase1.cs Attack1State
            //     player.TakeDamage(20);
            //     Debug.Log($"Boss dealt 20 damage to player with melee attack!");
            // }
        }
    }
}