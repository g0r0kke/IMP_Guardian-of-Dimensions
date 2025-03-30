using UnityEngine;

public class EnemyLockCamera : MonoBehaviour
{
    [Header("대상 설정")] [Tooltip("적 태그 이름")] public string enemyTag = "Enemy";

    [Header("카메라 설정")] [Tooltip("추적 속도 (값이 클수록 빠르게 회전)")]
    public float rotationSpeed = 5f;

    [Tooltip("타겟 고정 최대 거리")] public float maxLockDistance = 20f;

    [Tooltip("타겟을 찾지 못했을 때 Player의 forward 방향을 바라볼지")]
    public bool lookForwardWhenNoTarget = true;

    private Transform targetEnemy;
    private Transform playerTransform;

    void Start()
    {
        // 플레이어 트랜스폼 가져오기 (카메라의 부모)
        playerTransform = transform.parent;

        // 시작 시 가장 가까운 적 찾기
        FindClosestEnemy();
    }

    void Update()
    {
        // 매 프레임마다 가장 가까운 적 찾기
        FindClosestEnemy();

        // 타겟이 있으면 그 방향으로 회전
        if (targetEnemy != null)
        {
            // 적 방향 계산
            Vector3 direction = targetEnemy.position - transform.position;

            // 회전해야 할 각도 계산
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // 부드러운 회전 적용
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        // 타겟이 없고 lookForwardWhenNoTarget이 활성화되어 있으면 플레이어의 전방을 바라봄
        else if (lookForwardWhenNoTarget && playerTransform != null)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(playerTransform.forward),
                Time.deltaTime * rotationSpeed);
        }
    }

    void FindClosestEnemy()
    {
        // Enemy 태그를 가진 모든 오브젝트 찾기
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        float closestDistance = maxLockDistance;
        GameObject closestEnemy = null;

        // 가장 가까운 적 찾기
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        // 가장 가까운 적이 있으면 타겟으로 설정
        if (closestEnemy != null)
        {
            targetEnemy = closestEnemy.transform;
        }
        else
        {
            targetEnemy = null;
        }
    }

    // 디버그용 기즈모 그리기 (Scene 뷰에서만 보임)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxLockDistance);

        if (targetEnemy != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetEnemy.position);
        }
    }
}