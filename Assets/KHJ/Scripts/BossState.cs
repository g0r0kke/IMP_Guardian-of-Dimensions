using UnityEngine;

/// <summary>
/// Defines the base state classes for the boss state machine, including idle, walk, stun, and death behaviors.
/// </summary>
public abstract class BossState : IState
{
    protected Boss boss;

    public BossState(Boss boss)
    {
        this.boss = boss;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

// Base class for idle states
public abstract class IdleState : BossState
{
    protected float idleTimer = 0f;
    
    public IdleState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        idleTimer = 0f;
        boss.animator.SetTrigger("Idle");
        // Debug.Log("Boss: Idle state started");
    }

    public override void Update()
    {
        // Increment timer
        idleTimer += Time.deltaTime;
        
        // Handle state logic in derived classes
        HandleIdle();
    }
    
    // Abstract method to be implemented by derived classes
    protected abstract void HandleIdle();
    
    public override void Exit()
    {
        // Debug.Log("Boss: Idle state ended");
    }
}

// Base class for walk states
public abstract class WalkState : BossState
{
    public WalkState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        // Play walk animation
        boss.animator.SetTrigger("Walk");
        // Debug.Log("Boss: Walk state started");
    }

    public override void Update()
    {
        // If target player is null or destroyed, transition back to Idle
        if (!boss.targetPlayer)
        {
            boss.TransitionToIdle();
            return;
        }

        // Handle movement logic in derived classes
        HandleWalk();
    }
    
    // Abstract method to be implemented by derived classes
    protected abstract void HandleWalk();
    
    public override void Exit()
    {
        // Debug.Log("Boss: Walk state ended");
    }
}

// Stun state for when boss takes damage
public class StunState : BossState
{
    protected float stunTimer = 0f;
    protected float stunDuration = 0.6f;
    
    public StunState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        stunTimer = 0f;
        boss.animator.SetBool("Damage", true);
        // Debug.Log("Boss: Stun state started");
    }

    public override void Update()
    {
        stunTimer += Time.deltaTime;
        if (stunTimer >= stunDuration)
        {
            // For BossPhase1, return to previous state
            if (boss is Azmodan.Phase1.BossPhase1 phase1Boss)
            {
                boss.animator.SetBool("Damage", false);
                
                // Transition to different state based on BossPhase1's previousState
                IState prevState = phase1Boss.GetPreviousState();
                if (prevState is IdleState)
                {
                    boss.TransitionToIdle();
                }
                else if (prevState is WalkState)
                {
                    boss.TransitionToWalk();
                }
                else
                {
                    // Default to Idle
                    boss.TransitionToIdle();
                }
            }
            else
            {
                // Default behavior: Transition to Idle state
                boss.TransitionToIdle();
            }
        }
    }

    public override void Exit()
    {
        boss.animator.SetBool("Damage", false);
        // Debug.Log("Boss: Stun state ended");
    }
}

// Death state for boss death behavior
public class DeathState : BossState
{
    private float deathTimer = 0f;
    private float deathDuration = 3f;
    private bool animationComplete = false;
    
    public DeathState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        deathTimer = 0f;
        animationComplete = false;
        
        // No explicit animation trigger setting needed
        // Already set in TransitionToDeath
        
        // Debug.Log("Boss: Death state started");
    }

    public override void Update()
    {
        if (animationComplete) return;
        
        deathTimer += Time.deltaTime;
        
        if (deathTimer >= deathDuration)
        {
            animationComplete = true;
            // Debug.Log("Boss: Death animation completed");
            
            boss.gameObject.SetActive(false);
        }
    }

    public override void Exit()
    {
        // Debug.Log("Boss: Death state ended - If this message appears, there's an issue with state transition");
    }
}