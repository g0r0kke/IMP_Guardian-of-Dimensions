using UnityEngine;

public class MinionController : MonoBehaviour
{
    private Transform target;
    public float moveSpeed = 3f;
    [SerializeField] private float damage = 15f;

    //플레이어 타겟 지정 메서드
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
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
                Debug.Log($"어둠의 소환물이 플레이어에게 {damage} 데미지를 입혔습니다!");
            }
        }
    }
}
