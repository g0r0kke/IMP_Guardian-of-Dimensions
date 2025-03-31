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

    // 플레이어 타겟 지정 메서드
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }
    void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // 플레이어 충돌 시 데미지 주기
    private void OnCollisionEnter(Collision collision)
    {
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

}
