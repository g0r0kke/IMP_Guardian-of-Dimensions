using UnityEngine;
using UnityEngine.InputSystem;

public class Boss2Test : MonoBehaviour
{
    public GameObject hobgoblinPrefab; // Hobgoblin prefab to be summoned
    public Transform player; // Player's Transform
    public LayerMask minionLayerMask; // Minion layer mask

    public int minHobgoblins = 1; // Minimum number of hobgoblins to summon
    public int maxHobgoblins = 3; // Maximum number of hobgoblins to summon
    public float summonRadius = 3f; // Summon radius

    void Start()
    {
        if (hobgoblinPrefab == null)
        {
            Debug.LogError("Hobgoblin prefab is not assigned.");
        }
    }

    void Update()
    {
        // Test: Press the S key to summon hobgoblins
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            SummonHobgoblins();
        }
    }

    void SummonHobgoblins()
    {
        // Randomly determine the number of hobgoblins to summon
        int count = Random.Range(minHobgoblins, maxHobgoblins + 1);

        // Calculate the angle for each hobgoblin's position
        float angleStep = 180f / (count + 1); // Example: If 3 hobgoblins, positions at 45, 90, and 135 degrees
        Vector3 forward = transform.forward; // The forward direction of the current object

        for (int i = 0; i < count; i++)
        {
            // Calculate the summon angle for each hobgoblin
            float angle = -90f + angleStep * (i + 1); // Distribute angles between -90 and +90 degrees
            Quaternion rotation = Quaternion.Euler(0, angle, 0); // Calculate rotation
            Vector3 dir = rotation * forward; // Apply the rotation to the forward direction
            Vector3 spawnPos = transform.position + dir.normalized * summonRadius; // Calculate spawn position

            // Get the minion layer value
            int minionLayer = GetLayerFromMask(minionLayerMask);
            if (minionLayer == -1)
            {
                Debug.LogError("No valid layer found in enemyLayerMask!");
                return;
            }

            // Summon the hobgoblin
            Hobgoblin.Spawner(hobgoblinPrefab, spawnPos, player, minionLayerMask);
        }
    }

    // Function to get the layer value from LayerMask
    int GetLayerFromMask(LayerMask mask)
    {
        int maskValue = mask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((maskValue & (1 << i)) != 0)
                return i; // Return the index of the layer
        }
        return -1; // Return -1 if no valid layer is found
    }
}
