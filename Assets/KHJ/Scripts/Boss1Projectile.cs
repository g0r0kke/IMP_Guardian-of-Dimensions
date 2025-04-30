using UnityEngine;

public class Boss1Projectile : MonoBehaviour
{
    [SerializeField] private int attack2Damage = 10;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject impactEffectPrefab;
    private DamageController playerDamageController;
    
    private float timer = 0f;
    private bool hasHit = false;

    private void Start()
    {
        // Check if Rigidbody exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (!rb)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        // Check if Collider exists
        Collider col = GetComponent<Collider>();
        if (!col)
        {
            // Add sphere collider
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.5f;
            sphereCol.isTrigger = false;
        }
        
        playerDamageController = FindFirstObjectByType<DamageController>();
        if (!playerDamageController)
        {
            Debug.LogError("Cannot load data because DamageController does not exist.");
        }
    }

    private void Update()
    {
        // Destroy projectile after lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        hasHit = true;
        
        // Check if collision is with player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Apply damage to player
            playerDamageController.PlayerTakeDamage(attack2Damage);
            Debug.Log($"Boss1 ranged attack: Dealt {attack2Damage} damage to player!");
            
            // Create impact effect
            if (impactEffectPrefab)
            {
                Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            }
        }
        
        // Destroy projectile
        Destroy(gameObject);
    }
}