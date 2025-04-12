using UnityEngine;


public class MissileController : MonoBehaviour
{
    private Transform target;
    public float moveSpeed = 5f; 
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject impactEffectPrefab;

    //플레이어 타겟 지정 메서드
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    void Start()
    {
        // 일정 시간 후 미사일 자동 제거
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target != null)
        {
            // 플레이어 방향으로 이동
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 대상이 플레이어인지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어에게 데미지 적용
            SamplePlayer player = collision.gameObject.GetComponent<SamplePlayer>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
                Debug.Log($"어둠유도탄이 플레이어에게 {damage} 데미지를 입혔습니다!");
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
