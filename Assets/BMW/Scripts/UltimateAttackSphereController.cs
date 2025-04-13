using UnityEngine;

public class UltimateAttackSphereController : MonoBehaviour
{
    private Vector3 targetPosition;
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

    [Header("���� Ÿ��")]
    private Boss bossTarget;

    public void Initialize(Vector3 enemyPositions, float lifetime, float ultimateAttackSpeed, float StarScale, float scaleRat, int attDamage, LayerMask tarLayer, Collider planeCollider, AudioSource audioSource)
    {
        targetPosition = enemyPositions;
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
        GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObject != null)
        {
            bossTarget = enemyObject.GetComponent<Boss>();
            if (bossTarget == null)
            {
                Debug.LogWarning("Enemy �±׸� ���� ������Ʈ���� Boss ������Ʈ�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogWarning("Enemy �±׸� ���� ���� ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        
        if (transform.position.y <= targetPosition.y)
        {
            GameObject collisionEffect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
            collisionEffect.transform.localScale = new Vector3(transform.localScale.x * 3, transform.localScale.y * 3, transform.localScale.z * 3);
            Destroy(collisionEffect, 1f);
            Destroy(gameObject);

            ultimateAttackEndSound.Play();

            bossTarget.TakeDamage(attackDamage);
            Debug.Log($"�÷��̾ ���� Ÿ������ �������� {attackDamage} �������� �������ϴ�!");
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

        if (IsInTargetLayer(collision.gameObject) || collision.gameObject.CompareTag("Enemy"))
        {
            GameObject collisionEffect = Instantiate(collisionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            collisionEffect.transform.localScale = new Vector3(transform.localScale.x * 3, transform.localScale.y * 3, transform.localScale.z * 3);
            Destroy(collisionEffect, 1f);
            Destroy(gameObject);

            ultimateAttackEndSound.Play();

            bossTarget.TakeDamage(attackDamage);
            Debug.Log($"�÷��̾ ���� Ÿ������ �������� {attackDamage} �������� �������ϴ�!");
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
