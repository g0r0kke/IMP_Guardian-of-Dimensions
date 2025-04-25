using System.Collections.Generic;
using UnityEngine;

public class BasicAttackSphereController : MonoBehaviour
{

    [Header("Basic Attack sphere Initial Element Connection Settings")]
    [SerializeField] private GameObject collisionEffectPrefab;
                     private Transform player;
                     private LayerMask targetLayer;
                     private AudioSource basicAttackEndSound;

    [Header("Basic Attack sphere Initial Settings")]
                     public int attackDamage;
                     private float maxDistance = 10.0f;
                     private float lifeTime = 10.0f;
                     private float timer = 0f;
                     private bool isInitialized = false;

    // Setting up an external script connection
    private PlayerGUI playerGUI;
    private BasicAttackController basicAttackController;

    public void Initialize(Transform playerTransform, float maxDist, float lifetime, int attDamage, LayerMask tarLayer, Collider planeCollider, AudioSource audioSource)
    {
        player = playerTransform;
        maxDistance = maxDist;
        lifeTime = lifetime;
        attackDamage = attDamage;
        targetLayer = tarLayer;
        isInitialized = true;
        basicAttackEndSound = audioSource;

        Collider AttackCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(AttackCollider, planeCollider);

        playerGUI = FindAnyObjectByType<PlayerGUI>();
        basicAttackController = FindAnyObjectByType<BasicAttackController>();
    }
    private void Start()
    {

    }

    void Update()
    {
        if (!isInitialized) return;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            if (playerGUI.isDebug) Debug.Log("Deleted sphere because the Maintenance duration exceeded.");
            return;
        }

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > maxDistance)
            {
                Destroy(gameObject);
                if (playerGUI.isDebug) Debug.Log("Deleted sphere above the maximum distance.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {       

        if (IsInTargetLayer(collision.gameObject) || collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Hobgoblin"))
        {

            GameObject collisionEffect = Instantiate(collisionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            // Adjusting to the size of the enemy
            Vector3 effectCircleScale = collisionEffect.transform.Find("Circle").localScale;
            effectCircleScale = new Vector3(effectCircleScale.x * 10, effectCircleScale.y * 10, effectCircleScale.z * 10);
            Vector3 effectFireScale = collisionEffect.transform.Find("Fire").localScale;
            effectFireScale = new Vector3(effectFireScale.x * 10, effectFireScale.y * 10, effectFireScale.z * 10);

            Destroy(collisionEffect, 1f);

            Destroy(gameObject);
            playerGUI.IncreaseGauge(basicAttackController.GaugeIncreaseAmount);

            Boss hitBoss = collision.gameObject.GetComponent<Boss>();
            if (hitBoss)
            {
                hitBoss.TakeDamage(attackDamage);
                if (playerGUI.isDebug) Debug.Log($"Player has done {attackDamage} damage to boss with Basic Attack!");
            }
            else
            {
                Hobgoblin hitHobgoblin = collision.gameObject.GetComponent<Hobgoblin>();
                if (hitHobgoblin)
                {
                    hitHobgoblin.TakeDamage(attackDamage);
                    if (playerGUI.isDebug) Debug.Log($"Player has done {attackDamage} damage to minion with Basic Attack!");
                }
            }

            basicAttackEndSound.Play();
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}