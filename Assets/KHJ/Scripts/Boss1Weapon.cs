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
        
        if (weaponCollider.enabled)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.up * 2, Color.red);
        }
        
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
                        // if (!weaponCollider.enabled)
                        // {
                        //     Debug.Log($"무기 콜라이더 활성화 (프레임: {currentFrame})");
                        // }
                        
                        // 콜라이더 활성화
                        weaponCollider.enabled = true;
                        isAttacking = true;
                    }
                    else
                    {
                        // 디버그 로그
                        // if (weaponCollider.enabled)
                        // {
                        //     Debug.Log($"무기 콜라이더 비활성화 (프레임: {currentFrame})");
                        // }
                        
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
        // 모든 충돌 로깅
        // Debug.Log($"무기가 {other.gameObject.name} 태그:{other.tag}와 충돌");
        //
        if (isAttacking && other.CompareTag("Player"))
        {
            // Debug.Log("근접 공격!!");
            
            // 데미지를 주는 로직 추가
            SamplePlayer player = other.GetComponent<SamplePlayer>();
            if (player != null)
            {
                // BossPhase1.cs의 Attack1State에서 지정한 동일한 데미지값 사용
                player.TakeDamage(20);
                Debug.Log($"보스가 플레이어에게 근접 공격으로 20 데미지를 입혔습니다!");
            }
        }
    }
}