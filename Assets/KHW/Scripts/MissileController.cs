using UnityEngine;

// Controls the behavior of a homing missile targeting the player
public class MissileController : MonoBehaviour
{
    private Transform target;   // Target to follow
    public float moveSpeed = 5f;    // Movement speed
    public float rotationSpeed = 5f;   // Rotation smoothing speed
    [SerializeField] private float damage = 15f;  // Damage dealt to player
    [SerializeField] private float lifetime = 5f;  // Missile lifetime before self-destruct
    [SerializeField] private GameObject impactEffectPrefab;  // Visual effect on impact
    [SerializeField] private AudioClip explosionSound;  // Sound to play on explosion
    private AudioSource audioSource;

    // Assign the target player for the missile to follow
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();

        // Automatically destroy missile after a set lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target != null)
        {
            // Calculate the direction to the target
            Vector3 direction = (target.position - transform.position).normalized;

            // Smoothly rotate the missile toward the target
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // Move forward
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (audioSource != null)
        audioSource.Stop(); // Stop missile sound

        if (explosionSound != null)
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);  // Play explosion sound

        // If the missile hits the player, apply damage
        if (collision.gameObject.CompareTag("Player"))
        {
            SamplePlayer player = collision.gameObject.GetComponent<SamplePlayer>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
            }
        }
        
        // Spawn impact effect if assigned
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Destroy the missile on impact
        Destroy(gameObject);
    }
}
