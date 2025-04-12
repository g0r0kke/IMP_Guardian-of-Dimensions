using UnityEngine;

public class MinionController : MonoBehaviour
{
    private Transform target;
    public float moveSpeed = 3f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float maxHP = 30f;
    private float currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    // 플레이어 타겟 지정
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    void Update()
    {
        // 간단하게 목표 위치로 이동 (NavMeshAgent가 아니라 직접 이동)
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 플레이어에 부딪히면 데미지
        if (collision.gameObject.CompareTag("Player"))
        {
            SamplePlayer player = collision.gameObject.GetComponent<SamplePlayer>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
                Debug.Log($"어둠의 소환물이 플레이어에게 {damage} 데미지를 입혔습니다!");
            }
        }
    }

    public Transform GetTarget()
    {
        return target;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        Debug.Log($"소환물 체력 : {currentHP}");

        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
