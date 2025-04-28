using UnityEngine;


public class Hobgoblin : MonoBehaviour
{
    public Animator animator;
    public Transform player;

    public IState currentState;
    public float rotationSpeed = 5f;// Rotation speed

    public float walkSpeed = 2f; // Walking speed
    public float detectionRange = 10f; // Player detection range
    public float attackRange = 2.7f; // Attack range
    public int hp = 1; // Hobgoblin health

    public AudioSource audioSource;

    public AudioClip goblinLaugh;   // idle sound
    public AudioClip goblinCackle;  // walk sound
    public AudioClip goblinPunch;   // attack sound
    public AudioClip goblinDeath;   // damage/death sound

    public int attackDamage = 5; // Damage dealt to the player


    // Player-related variables
    public DamageController playerDamageController;
    private PlayerGUI playerGUI;


 

    void Start()
    {
        animator = GetComponent<Animator>();

        // Find player
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Find DamageController (Minwoo)
        playerDamageController = FindFirstObjectByType<DamageController>();
        if (playerDamageController == null)
        {
            Debug.LogError("DamageController not found. Cannot load player data.");
        }

        // Add this Hobgoblin to the player's target list
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject)
        {
            playerGUI = FindFirstObjectByType<PlayerGUI>(); 
            if (playerGUI != null)
            {
                playerGUI.AddHobgoblinTarget(this);
            }
        }


        // Set initial state to Idle
        ChangeState(new HobIdleState(this)); 
    }


    // Function to deal damage to the player (Minwoo)
    public void DealDamageToPlayer()
    {
        if (playerDamageController != null)
        {
            playerDamageController.PlayerTakeDamage(attackDamage);
            Debug.Log($"Hobgoblin dealt {attackDamage} damage to the player!");
        }
    }


    void Update()
    {
        if (currentState != null)
        {
            // Rotate towards the player
            if (player != null && !(currentState is HobDeadState))
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0; // Only rotate on the Y-axis

                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                }
            }

            // Call Update of the current state
            currentState.Update();
        }
    }


    public void ChangeState(IState newState)
    {
        Debug.Log($"State changed: {currentState} → {newState}");
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // Function for taking damage
    public void TakeDamage(int damage)
    {
       
        if (hp <= 0) return; // Ignore if already dead


        hp -= damage;
        ChangeState(new HobDamageState(this)); // Switch to damage state


        if (hp <= 0)
        {
            // Remove Hobgoblin from the player's target list
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject)
            {
               // playerGUI.GetComponent<PlayerGUI>(); 
                if (playerGUI != null)
                {
                    playerGUI.RemoveHobgoblinTarget(this);
                }
            }


            ChangeState(new HobDeadState(this));
        }
    }

     // Function to allow external spawning of Hobgoblins
    public static Hobgoblin Spawner(GameObject prefab, Vector3 spawnPosition, Transform targetPlayer, int minionLayer)
        {
        
            if (prefab == null)
            {
                Debug.LogError("Hobgoblin prefab is not assigned.");
                return null;
            }

            GameObject hobgoblinObj = GameObject.Instantiate(prefab, spawnPosition, Quaternion.identity);
            hobgoblinObj.layer = minionLayer;
            Hobgoblin hob = hobgoblinObj.GetComponent<Hobgoblin>();
            hob.player = targetPlayer;  // Set the target player

        return hob;
        }

    // Play Hobgoblin punch (attack) sound
    public void PlayPunchSound()
        {
            if (audioSource != null && goblinPunch != null)
            {
                audioSource.PlayOneShot(goblinPunch);
            }
        }

}