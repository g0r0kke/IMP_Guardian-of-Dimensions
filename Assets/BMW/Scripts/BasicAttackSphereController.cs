using System.Collections.Generic;
using UnityEngine;

public class BasicAttackSphereController : MonoBehaviour
{

                     private Transform player;
    [SerializeField] private GameObject collisionEffectPrefab;
                     private float maxDistance = 10.0f;
                     private float lifeTime = 10.0f;
                     private float timer = 0f;

                     private AudioSource basicAttackEndSound;
                     public int attackDamage;
                     private LayerMask targetLayer;
                     private bool isInitialized = false;

                        private PlayerGUI playerGUI;
                     private BasicAttackController basicAttackController;

    public void Initialize(Transform playerTransform, float maxDist, float lifetime, int attDamage, LayerMask tarLayer, PlayerGUI GUI, BasicAttackController controller, Collider planeCollider, AudioSource audioSource)
    {
        player = playerTransform;
        maxDistance = maxDist;
        lifeTime = lifetime;
        attackDamage = attDamage;
        targetLayer = tarLayer;
        isInitialized = true;
        playerGUI = GUI;
        basicAttackController = controller;
        basicAttackEndSound = audioSource;

        Collider AttackCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(AttackCollider, planeCollider);
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
            Debug.Log("구체지속시간이 초과하여 삭제되었습니다.");
            return;
        }

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > maxDistance)
            {
                Destroy(gameObject);
                Debug.Log("구체최대거리보다 초과하여 삭제되었습니다.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {       

        if (IsInTargetLayer(collision.gameObject) || collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Hobgoblin"))
        {

            GameObject collisionEffect = Instantiate(collisionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            // 적의 크기에 따라 조정
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
                Debug.Log($"플레이어가 보스에게 {attackDamage} 데미지를 입혔습니다!");
            }
            else
            {
                Hobgoblin hitHobgoblin = collision.gameObject.GetComponent<Hobgoblin>();
                if (hitHobgoblin)
                {
                    hitHobgoblin.TakeDamage(attackDamage);
                    Debug.Log($"플레이어가 소환수에게 {attackDamage} 데미지를 입혔습니다!");
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