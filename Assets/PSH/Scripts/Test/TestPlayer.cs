using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    public float distancePerFrame = 2.0f; // Distance to move per frame
    private Vector2 movement; // Player movement vector

    [Header("Player Status")] public int maxHealth = 100; // Max health
    public int currentHealth; // Current health

    [Header("Attack Settings")] public int attackDamage = 10; // Attack damage

    [Header("Boss Target")] private Boss bossTarget; // Boss target

    void Start()
    {
        currentHealth = maxHealth; // Initialize health at the start

        // Find the game object with "Enemy" tag
        GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObject != null)
        {
            // Find the Boss component
            bossTarget = enemyObject.GetComponent<Boss>();
            if (bossTarget == null)
            {
                Debug.LogWarning("Cannot find Boss component on the object with 'Enemy' tag.");
            }
        }
        else
        {
            Debug.LogWarning("Cannot find a game object with 'Enemy' tag.");
        }
    }

    void Update()
    {
        HandleMovement(); // Handle movement
        HandleAttack(); // Handle attack
    }

    void HandleMovement()
    {
        if (Keyboard.current != null)
        {
            // Calculate movement vector based on key input
            movement = new Vector2(
                Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0
            );

            // Apply movement (move the object using transform)
            transform.Translate(new Vector3(movement.x, 0, movement.y) * distancePerFrame * Time.deltaTime);
        }
    }

    void HandleAttack()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("Q key pressed!");

            // Attack the boss
            if (bossTarget != null)
            {
                bossTarget.TakeDamage(attackDamage);
                Debug.Log($"Player dealt {attackDamage} damage to the boss!");
            }

            // Attack Hobgoblins (within range)
            GameObject[] hobgoblins = GameObject.FindGameObjectsWithTag("Enemy");
            Debug.Log($"Detected number of Hobgoblins: {hobgoblins.Length}");

            // Attack each Hobgoblin
            foreach (GameObject hob in hobgoblins)
            {
                float distance = Vector3.Distance(transform.position, hob.transform.position);
                Debug.Log($"Distance to Hobgoblin: {distance}");

                if (distance < 4f) // If within attack range
                {
                    Hobgoblin hobScript = hob.GetComponent<Hobgoblin>();
                    if (hobScript != null)
                    {
                        hobScript.TakeDamage(attackDamage);
                        Debug.Log($"Player attacked the Hobgoblin!");
                    }
                    else
                    {
                        Debug.LogWarning("Cannot find Hobgoblin script.");
                    }
                }
            }
        }
    }

    public void TakeTakeDamage(int damage)
    {
        currentHealth -= damage; // Reduce health by the damage value
        Debug.Log($"Player health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die(); // If health is 0 or below, call Die
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // Optionally disable the player object
        // gameObject.SetActive(false);
    }
}
