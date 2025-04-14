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

    [Header("보스 타겟")]
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
                Debug.LogWarning("Enemy 태그를 가진 오브젝트에서 Boss 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("Enemy 태그를 가진 게임 오브젝트를 찾을 수 없습니다.");
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
            Debug.Log($"플레이어가 간접 타격으로 보스에게 {attackDamage} 데미지를 입혔습니다!");
        }
        

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            Debug.Log("구체지속시간이 초과하여 삭제되었습니다.");
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
            Debug.Log($"플레이어가 직접 타격으로 보스에게 {attackDamage} 데미지를 입혔습니다!");
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
