using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Sample player implementation for testing purposes only.
/// This class provides basic movement, combat, and health management functionality
/// for development and debugging. Do not use in production build.
/// </summary>
public class SamplePlayer : MonoBehaviour
{
    public float distancePerFrame = 2.0f;
    private Vector2 movement;

    [Header("Player Status")] public int maxHealth = 100;
    public int currentHealth;

    [Header("Attack Settings")] public int attackDamage = 10;

    [Header("Boss Target")] private Boss bossTarget;

    [Header("Minion Targets")]
    private List<MinionController> minionTargets = new List<MinionController>();


    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Find game object with Enemy tag
        GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObject)
        {
            bossTarget = enemyObject.GetComponent<Boss>();
            if (!bossTarget)
            {
                Debug.LogWarning("Could not find Boss component on object with Enemy tag.");
            }
        }
        else
        {
            Debug.LogWarning("Could not find any game object with Enemy tag.");
        }

        FindAllMinions(); // Find all minion targets
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
    }

    /// <summary>
    /// Handles basic WASD movement input for testing
    /// </summary>
    void HandleMovement()
    {
        if (Keyboard.current != null)
        {
            movement = new Vector2(
                Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0
            );

            transform.Translate(new Vector3(movement.x, 0, movement.y) * distancePerFrame * Time.deltaTime);
        }
    }

    /// <summary>
    /// Handles attack input (press Q to attack)
    /// </summary>
    void HandleAttack()
    {
        // Press Q to attack
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (bossTarget)
            {
                // Deal damage to boss
                bossTarget.TakeDamage(attackDamage);

                Debug.Log($"Player dealt {attackDamage} damage to boss!");
            }
            else
            {
                Debug.LogWarning("Boss target not set!");
                // Try to find boss again by Enemy tag
                GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
                if (enemyObject)
                {
                    bossTarget = enemyObject.GetComponent<Boss>();
                }
            }

            // Attack minions
            if (minionTargets.Count > 0)
            {
                foreach (MinionController minion in minionTargets)
                {
                    if (minion)
                    {
                        minion.TakeDamage(attackDamage);
                        Debug.Log($"Player dealt {{attackDamage}} damage to minion!");
                    }
                }
            }
            else
            {
                // Debug.LogWarning("No minion targets. Searching again.");
                FindAllMinions();
            }
        }
    }

    /// <summary>
    /// Finds all minions in the scene and adds them to the target list
    /// </summary>
    void FindAllMinions()
    {
        minionTargets.Clear();
        GameObject[] minions = GameObject.FindGameObjectsWithTag("Summon");
        foreach (GameObject minionObj in minions)
        {
            MinionController minion = minionObj.GetComponent<MinionController>();
            if (minion)
            {
                minionTargets.Add(minion);
            }
        }

        // Debug.Log($"Found {minionTargets.Count} minions");
    }

    /// <summary>
    /// Applies damage to the player
    /// </summary>
    /// <param name="damage">Amount of damage to apply</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Log current health
        Debug.Log($"Player health: {currentHealth}/{maxHealth}");

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles player death
    /// </summary>
    private void Die()
    {
        Debug.Log("Player died!");
        // Death handling logic (optional)
        // gameObject.SetActive(false);
    }
}