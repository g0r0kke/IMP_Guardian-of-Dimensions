// hobgoblinState.cs
using UnityEngine;

// Idle state class
public class HobIdleState : IState
{
    private Hobgoblin hobgoblin;

    // Constructor - receives Hobgoblin instance
    public HobIdleState(Hobgoblin hobgoblin)
    {
        this.hobgoblin = hobgoblin;
    }

    public void Enter()
    {
        hobgoblin.animator.Play("idle01"); // Play idle animation

        hobgoblin.audioSource.clip = hobgoblin.goblinLaugh; // Set laugh sound
        hobgoblin.audioSource.Play(); // Play laugh sound
    }

    public void Update()
    {
        float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);

        // If the player is outside attack range but within detection range, switch to Walk state
        if (distance > hobgoblin.attackRange && distance < hobgoblin.detectionRange)
        {
            hobgoblin.ChangeState(new HobWalkState(hobgoblin));
        }
        // If the player is within attack range, switch to Attack state
        else if (distance <= hobgoblin.attackRange)
        {
            hobgoblin.ChangeState(new HobAttackState(hobgoblin));
        }
    }

    public void Exit() { }
}

// Walk state class
public class HobWalkState : IState
{
    private Hobgoblin hobgoblin;

    public HobWalkState(Hobgoblin hobgoblin)
    {
        this.hobgoblin = hobgoblin;
    }

    public void Enter()
    {
        hobgoblin.animator.Play("walk"); // Play walk animation

        // If the current sound is different or not playing, play the new sound
        if (hobgoblin.audioSource.clip != hobgoblin.goblinCackle || !hobgoblin.audioSource.isPlaying)
        {
            hobgoblin.audioSource.clip = hobgoblin.goblinCackle;
            hobgoblin.audioSource.Play();
        }
    }

    public void Update()
    {
        float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);

        // If the player is outside attack range, move toward the player
        if (distance > hobgoblin.attackRange)
        {
            hobgoblin.transform.position = Vector3.MoveTowards(
                hobgoblin.transform.position,
                hobgoblin.player.position,
                hobgoblin.walkSpeed * Time.deltaTime
            );
        }

        // If the player is within attack range, switch to Attack state
        if (distance <= hobgoblin.attackRange)
        {
            hobgoblin.ChangeState(new HobAttackState(hobgoblin));
        }
        // If the player is outside detection range, switch to Idle state
        else if (distance > hobgoblin.detectionRange)
        {
            hobgoblin.ChangeState(new HobIdleState(hobgoblin));
        }
    }

    public void Exit() { }
}

// Attack state class
public class HobAttackState : IState
{
    private Hobgoblin hobgoblin;
    private bool soundPlayed = false;  // Whether attack sound has been played
    private bool animationCompleted = false; // Whether animation has completed
    private float attackTimer = 0f; // Attack timer
    private float attackDuration = 1.5f; // Duration of attack animation
    private float attackCooldown = 0.7f; // Cooldown between attacks

    private bool damageDeal = false; // Whether damage has been dealt

    public HobAttackState(Hobgoblin hobgoblin)
    {
        this.hobgoblin = hobgoblin;
    }

    public void Enter()
    {
        Debug.Log("Entered Attack State");
        hobgoblin.animator.Play("attack02", 0, 0f); // Play attack animation from start
        attackTimer = 0f;
        soundPlayed = false;
        damageDeal = false; // Reset damage application
        animationCompleted = false;
    }

    public void Update()
    {
        attackTimer += Time.deltaTime;
        AnimatorStateInfo stateInfo = hobgoblin.animator.GetCurrentAnimatorStateInfo(0);

        // Play attack sound after a delay
        if (!soundPlayed && attackTimer >= 0.2f)
        {
            // hobgoblin.PlayPunchSound();
            soundPlayed = true;
            Debug.Log("Attack sound played");
        }

        // Apply damage after a short delay
        if (!damageDeal && attackTimer >= 0.4f)
        {
            hobgoblin.DealDamageToPlayer();
            damageDeal = true;
        }

        // Check if animation has completed
        if (!animationCompleted &&
            (attackTimer >= attackDuration || stateInfo.normalizedTime >= 0.95f))
        {
            animationCompleted = true;
            Debug.Log("Attack animation completed");
        }

        // Transition to the next state after animation and cooldown
        if (animationCompleted && attackTimer >= attackDuration + attackCooldown)
        {
            float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);
            Debug.Log($"Considering state transition. Distance: {distance}, Attack Range: {hobgoblin.attackRange}");

            if (distance <= hobgoblin.attackRange)
            {
                // Attack again
                hobgoblin.ChangeState(new HobAttackState(hobgoblin));
            }
            else if (distance <= hobgoblin.detectionRange)
            {
                // Player is within detection range
                hobgoblin.ChangeState(new HobWalkState(hobgoblin));
            }
            else
            {
                // Player is outside detection range
                hobgoblin.ChangeState(new HobIdleState(hobgoblin));
            }
        }
    }

    public void Exit()
    {
        Debug.Log("Exited Attack State");
    }
}

// Damage state class
public class HobDamageState : IState
{
    private Hobgoblin hobgoblin;

    public HobDamageState(Hobgoblin hobgoblin)
    {
        this.hobgoblin = hobgoblin;
    }

    public void Enter()
    {
        hobgoblin.animator.Play("damage"); // Play damage animation
        hobgoblin.audioSource.Stop(); // Stop current audio
        hobgoblin.audioSource.PlayOneShot(hobgoblin.goblinDeath); // Play damage sound
    }

    public void Update()
    {
        // After damage animation finishes, transition to next state
        if (hobgoblin.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);
            if (distance < hobgoblin.attackRange)
                hobgoblin.ChangeState(new HobAttackState(hobgoblin));
            else
                hobgoblin.ChangeState(new HobWalkState(hobgoblin));
        }
    }

    public void Exit() { }
}

// Dead state class
public class HobDeadState : IState
{
    private Hobgoblin hobgoblin;

    public HobDeadState(Hobgoblin hobgoblin)
    {
        this.hobgoblin = hobgoblin;
    }

    public void Enter()
    {
        hobgoblin.animator.Play("dead"); // Play death animation

        GameObject.Destroy(hobgoblin.gameObject, 2f);
    }

    public void Update() { }

    public void Exit() { }
}
