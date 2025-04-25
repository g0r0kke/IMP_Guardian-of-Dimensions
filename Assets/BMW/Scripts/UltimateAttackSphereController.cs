using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UltimateAttackSphereController : MonoBehaviour
{

    [Header("Ultimate Attack sphere Initial Element Connection Settings")]
    [SerializeField] private GameObject collisionEffectPrefab;
                     private Transform targetTransform;
                     private LayerMask targetLayer;
                     private AudioSource ultimateAttackEndSound;

    [Header("Ultimate Attack sphere Initial Settings")]
                     public int attackDamage;
                     private float speed = 15.0f;
                     private float startScale = 0.5f;
                     private float scaleRate = 0.5f;
                     private float lifeTime = 10.0f;
                     private float timer = 0f;
                     private bool isInitialized = false;

    // Setting up an external script connection
    private PlayerGUI playerGUI;

    public void Initialize(Transform target, float lifetime, float ultimateAttackSpeed, float StarScale, float scaleRat, int attDamage, LayerMask tarLayer, Collider planeCollider, AudioSource audioSource)
    {
        targetTransform = target;
        speed = ultimateAttackSpeed;
        lifeTime = lifetime;
        targetLayer = tarLayer;
        startScale = StarScale;
        scaleRate = scaleRat;
        attackDamage = attDamage;
        ultimateAttackEndSound = audioSource;
        isInitialized = true;

        transform.localScale = Vector3.one * startScale;
        StartCoroutine(ContinuousScaling());

        Collider AttackCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(AttackCollider, planeCollider);

        playerGUI = FindAnyObjectByType<PlayerGUI>();
    }

    private void Start()
    {

    }

    void Update()
    {
        if (!isInitialized) return;

        if (targetTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        // 방향 계산 및 이동
        transform.LookAt(targetTransform);
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, step);


        if (transform.position.y <= targetTransform.position.y)
        {
            
            Destroy(gameObject);
            if (playerGUI.isDebug) Debug.Log("The sphere reached the enemy position but was deleted without colliding with the enemy.");
            return;
            
        }
        

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            if (playerGUI.isDebug) Debug.Log("Deleted sphere because the Maintenance duration exceeded.");
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (IsInTargetLayer(collision.gameObject) || collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Hobgoblin"))
        {
            GameObject collisionEffect = Instantiate(collisionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            collisionEffect.transform.localScale = new Vector3(transform.localScale.x * 3, transform.localScale.y * 3, transform.localScale.z * 3);
            Destroy(collisionEffect, 1f);
            Destroy(gameObject);

            ultimateAttackEndSound.Play();

            Boss hitBoss = collision.gameObject.GetComponent<Boss>();
            if (hitBoss)
            {
                hitBoss.TakeDamage(attackDamage);
                if (playerGUI.isDebug) Debug.Log($"Player has done {attackDamage} damage to boss with Ultimate Attack!");
            }
            else
            {
                Hobgoblin hitHobgoblin = collision.gameObject.GetComponent<Hobgoblin>();
                if (hitHobgoblin)
                {
                    hitHobgoblin.TakeDamage(attackDamage);
                    if (playerGUI.isDebug) Debug.Log($"Player has done {attackDamage} damage to minion with Ultimate Attack!");
                }
            }
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }

    private System.Collections.IEnumerator ContinuousScaling()
    {
        while (true)
        {
            
            float scaleMultiplier = 1 + (scaleRate * Time.deltaTime);
            transform.localScale *= scaleMultiplier;
            yield return null;
        }
    }

}
