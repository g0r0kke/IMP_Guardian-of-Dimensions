using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    private Azmodan.Phase1.BossPhase1 bossPhase1;
    private BoxCollider weaponCollider;
    private bool isAttacking = false;

    [Header("애니메이션 프레임 설정")]
    [SerializeField] private int attackStartFrame = 17; // 콜라이더 활성화 시작 프레임
    [SerializeField] private int attackEndFrame = 30;   // 콜라이더 비활성화 프레임
    [SerializeField] private int totalAnimationFrames = 60; // 전체 애니메이션 프레임 수

    void Start()
    {
        // 부모 오브젝트에서 BossPhase1 컴포넌트 찾기
        bossPhase1 = GetComponentInParent<Azmodan.Phase1.BossPhase1>();
        
        // 무기에 콜라이더 추가 또는 가져오기
        weaponCollider = GetComponent<BoxCollider>();
        if (weaponCollider == null)
        {
            Debug.LogWarning("콜라이더가 없습니다!");
            return;
        }
        
        // 트리거로 설정 (물리적 충돌 없이 충돌 감지만)
        weaponCollider.isTrigger = true;
        
        // 초기에는 콜라이더 비활성화
        weaponCollider.enabled = false;
    }

    void Update()
    {
        // 보스가 Attack1 상태인지 확인 (네임스페이스 없이 BossStateType 사용)
        if (bossPhase1 != null && bossPhase1.GetSelectedAttackType() == BossStateType.Attack1)
        {
            Animator animator = bossPhase1.GetComponent<Animator>();
            if (animator != null)
            {
                // 공격 애니메이션의 현재 상태 정보 가져오기
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                
                // 공격 애니메이션인지 확인
                if (stateInfo.IsName("Attack"))
                {
                    // 현재 애니메이션 진행률 (0.0 ~ 1.0)
                    float normalizedTime = stateInfo.normalizedTime % 1.0f; // 반복 애니메이션 고려
                    
                    // 진행률을 프레임으로 변환
                    int currentFrame = Mathf.FloorToInt(normalizedTime * totalAnimationFrames);
                    
                    // 지정된 프레임 범위(17~30) 내에 있는지 확인
                    if (currentFrame >= attackStartFrame && currentFrame <= attackEndFrame)
                    {
                        // 디버그 로그
                        if (!weaponCollider.enabled)
                        {
                            Debug.Log($"무기 콜라이더 활성화 (프레임: {currentFrame})");
                        }
                        
                        // 콜라이더 활성화
                        weaponCollider.enabled = true;
                        isAttacking = true;
                    }
                    else
                    {
                        // 디버그 로그
                        if (weaponCollider.enabled)
                        {
                            Debug.Log($"무기 콜라이더 비활성화 (프레임: {currentFrame})");
                        }
                        
                        // 콜라이더 비활성화
                        weaponCollider.enabled = false;
                        isAttacking = false;
                    }
                }
                else
                {
                    // 공격 애니메이션이 아니면 비활성화
                    weaponCollider.enabled = false;
                    isAttacking = false;
                }
            }
        }
        else
        {
            // 공격 상태가 아니면 콜라이더 비활성화
            weaponCollider.enabled = false;
            isAttacking = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isAttacking && other.CompareTag("Player"))
        {
            Debug.Log("근접 공격!!");
            
            // 데미지를 주는 로직 추가
            SamplePlayer player = other.GetComponent<SamplePlayer>();
            if (player != null)
            {
                // BossPhase1.cs의 Attack1State에서 지정한 동일한 데미지값 사용
                player.TakeDamage(20);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (weaponCollider == null) return;
        
        // 공격 중이거나 에디터에서 확인하기 위해 항상 그리기 (isAttacking 조건 제거)
        if (weaponCollider.enabled || !Application.isPlaying)
        {
            // 콜라이더가 활성화된 경우 시각적으로 표시
            Gizmos.color = isAttacking ? Color.red : Color.yellow;
            
            // 방법 1: 콜라이더의 월드 위치 직접 계산
            Vector3 worldCenter = transform.TransformPoint(weaponCollider.center);
            
            // 스케일을 고려한 사이즈 계산
            Vector3 worldSize = Vector3.Scale(weaponCollider.size, transform.lossyScale);
            
            // 월드 공간에서 와이어 큐브 그리기
            Gizmos.DrawWireCube(worldCenter, worldSize);
            
            // 콜라이더의 경계선을 더 명확하게 표시하기 위한 추가 기즈모
            if (isAttacking)
            {
                // 콜라이더 모서리 표시
                Gizmos.color = Color.white;
                float halfWidth = worldSize.x * 0.5f;
                float halfHeight = worldSize.y * 0.5f;
                float halfDepth = worldSize.z * 0.5f;
                
                // 모서리 점들 (8개)의 위치
                Vector3[] corners = new Vector3[8];
                corners[0] = worldCenter + new Vector3(-halfWidth, -halfHeight, -halfDepth);
                corners[1] = worldCenter + new Vector3(halfWidth, -halfHeight, -halfDepth);
                corners[2] = worldCenter + new Vector3(-halfWidth, halfHeight, -halfDepth);
                corners[3] = worldCenter + new Vector3(halfWidth, halfHeight, -halfDepth);
                corners[4] = worldCenter + new Vector3(-halfWidth, -halfHeight, halfDepth);
                corners[5] = worldCenter + new Vector3(halfWidth, -halfHeight, halfDepth);
                corners[6] = worldCenter + new Vector3(-halfWidth, halfHeight, halfDepth);
                corners[7] = worldCenter + new Vector3(halfWidth, halfHeight, halfDepth);
                
                // 각 모서리에 구체 그리기
                foreach (Vector3 corner in corners)
                {
                    Gizmos.DrawSphere(corner, 0.05f);
                }
            }
        }
    }
}