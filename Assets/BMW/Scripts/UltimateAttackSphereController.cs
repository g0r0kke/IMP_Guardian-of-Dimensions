using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UltimateAttackSphereController : MonoBehaviour
{
                     private Transform targetTransform;
                     private LayerMask targetLayer;
    [SerializeField] private GameObject collisionEffectPrefab;

                     private float speed = 15.0f;
                     public float startScale = 0.5f;
                     public float scaleRate = 0.5f;
                     public int attackDamage;
                     private float lifeTime = 10.0f;
                     private float timer = 0f;
                     private bool isInitialized = false;
                     private AudioSource ultimateAttackEndSound;


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

        // ���� ��� �� �̵�
        transform.LookAt(targetTransform);
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, step);


        if (transform.position.y <= targetTransform.position.y)
        {
            
            /*
            GameObject collisionEffect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
            collisionEffect.transform.localScale = new Vector3(transform.localScale.x * 3, transform.localScale.y * 3, transform.localScale.z * 3);
            Destroy(collisionEffect, 1f);
            Destroy(gameObject);

            ultimateAttackEndSound.Play();

            Boss hitBoss = gameObject.GetComponent<Boss>();
            if (hitBoss)
            {
                hitBoss.TakeDamage(attackDamage);
                Debug.Log($"�÷��̾ ������������ �������� {attackDamage} �������� �������ϴ�!");
            }
            else
            {
                Hobgoblin hitHobgoblin = gameObject.GetComponent<Hobgoblin>();
                if (hitHobgoblin)
                {
                    hitHobgoblin.TakeDamage(attackDamage);
                    Debug.Log($"�÷��̾ ������������ ��ȯ������ {attackDamage} �������� �������ϴ�!");
                }
            }
            */
            Destroy(gameObject);
            Debug.Log("��ü�� ���� ��ġ���� �����Ͽ����� ���� Ÿ������ ���ϰ� �����Ǿ����ϴ�.");
            return;
            
        }
        

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            Debug.Log("��ü���ӽð��� �ʰ��Ͽ� �����Ǿ����ϴ�.");
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
                Debug.Log($"�÷��̾ �ñر��� ���� �������� �������� {attackDamage} �������� �������ϴ�!");
            }
            else
            {
                Hobgoblin hitHobgoblin = collision.gameObject.GetComponent<Hobgoblin>();
                if (hitHobgoblin)
                {
                    hitHobgoblin.TakeDamage(attackDamage);
                    Debug.Log($"�÷��̾ �ñر��� ���� �������� ��ȯ������ {attackDamage} �������� �������ϴ�!");
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
