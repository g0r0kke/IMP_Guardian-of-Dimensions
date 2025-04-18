using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private int attack2Damage = 10;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject impactEffectPrefab;
    private DamageController playerDamageController;
    
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
        
        playerDamageController = FindFirstObjectByType<DamageController>();
        if (playerDamageController == null)
        {

            Debug.LogError("DamageController가 존재하지 않아 데이터를 로드할 수 없습니다.");
            return;

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
            playerDamageController.PlayerTakeDamage(attack2Damage);
            Debug.Log($"보스1 원거리 공격: 플레이어에게 {attack2Damage} 데미지를 입혔습니다!");
            
            // 충돌 이펙트 생성
            if (impactEffectPrefab)
            {
                Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            }
        }
        
        // 투사체 제거
        Destroy(gameObject);
    }
}