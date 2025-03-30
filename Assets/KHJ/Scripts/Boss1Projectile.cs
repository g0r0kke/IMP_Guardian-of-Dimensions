using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject impactEffectPrefab;
    
    private float timer = 0f;
    private bool hasHit = false;

    private void Start()
    {
        // Rigidbody가 있는지 확인
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        // Collider가 있는지 확인
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // 구체 모양의 Collider 추가
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.5f;
            sphereCol.isTrigger = false;
        }
    }

    private void Update()
    {
        // 일정 시간 후 투사체 제거
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
        
        // 충돌한 대상이 플레이어인지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어에게 데미지 적용
            SamplePlayer player = collision.gameObject.GetComponent<SamplePlayer>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
                Debug.Log($"보스 투사체가 플레이어에게 {damage} 데미지를 입혔습니다!");
            }
        }
        
        // 충돌 이펙트 생성
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // 투사체 제거
        Destroy(gameObject);
    }
}