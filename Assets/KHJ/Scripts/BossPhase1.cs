using UnityEngine;

namespace Azmodan.Phase1
{
   public class BossPhase1 : Boss
    {
        [SerializeField] private int stunThreshold = 30; // 스턴 발동 체력 임계값
        [SerializeField] private float phase1DamageMultiplier = 1.0f; // 1페이즈 데미지 배율

        protected override void InitializeStates()
        {
            states[typeof(IdleState)] = new IdleState(this);
            states[typeof(WalkState)] = new WalkState(this);
            states[typeof(Attack1State)] = new Attack1State(this);
            states[typeof(Attack2State)] = new Attack2State(this);
            states[typeof(StunState)] = new StunState(this);
            states[typeof(DeathState)] = new DeathState(this);
        }

        // 1페이즈 전용 공격 상태로 전환
        public override void TransitionToAttack()
        {
            // 페이즈 1의 Attack1State로 전환
            ChangeState<Attack1State>();
        }

        // 1페이즈 전용 사망 상태로 전환 메서드 재정의
        public override void TransitionToDeath()
        {
            // DeathState 대신 Phase1DeathState 사용
            ChangeState<DeathState>();
        }

        // 1페이즈 전용 데미지 처리
        public override void TakeDamage(int damage)
        {
            // 1페이즈 데미지 배율 적용
            int actualDamage = Mathf.RoundToInt(damage * phase1DamageMultiplier);
            health -= actualDamage;
            
            Debug.Log($"보스 1페이즈: {actualDamage} 데미지 받음 (현재 체력: {health})");
            
            // 특정 체력 이하로 떨어지면 스턴 상태로 전환
            if (health < stunThreshold && currentState.GetType() != typeof(StunState))
            {
                TransitionToStun();
            }
            
            // 사망 처리
            if (health <= 0)
            {
                Die();
            }
        }

        // 1페이즈 전용 사망 처리
        protected override void Die()
        {
            Debug.Log("보스 1페이즈 사망");
            // 여기서 1페이즈 사망 시 특별한 처리를 할 수 있음
            // 예: 2페이즈로 변경, 특수 이펙트 재생 등
            
            TransitionToDeath();
        }
    }

    // 1페이즈 전용 공격 상태 1
    public class Attack1State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 2f;
        
        public Attack1State(Boss boss) : base(boss) { }

        public override void Enter()
        {
            // 타이머 초기화
            attackTimer = 0f;
            
            // 공격 애니메이션 재생
            boss.animator.SetTrigger("Attack");
            Debug.Log("보스 1페이즈: 공격1 상태 시작");
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;
            
            // 공격 애니메이션이 끝나면 상태 전환
            if (attackTimer >= attackDuration)
            {
                // 확률에 따라 다른 공격 또는 Idle 상태로 전환
                float randomValue = Random.value;
                if (randomValue < 0.3f)
                {
                    boss.ChangeState<Attack2State>();
                }
                else
                {
                    // Idle 상태로 전환
                    boss.TransitionToIdle();
                }
            }
        }

        public override void Exit()
        {
            Debug.Log("보스 1페이즈: 공격1 상태 종료");
        }
    }

    // 1페이즈 전용 공격 상태 2
    public class Attack2State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 3f;
        
        public Attack2State(Boss boss) : base(boss) { }

        public override void Enter()
        {
            // 타이머 초기화
            attackTimer = 0f;
            
            // 공격 애니메이션 재생
            boss.animator.SetTrigger("Attack");
            Debug.Log("보스 1페이즈: 공격2 상태 시작");
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;
            
            // 공격 애니메이션이 끝나면 상태 전환
            if (attackTimer >= attackDuration)
            {
                // Idle 상태로 전환
                boss.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            Debug.Log("보스 1페이즈: 공격2 상태 종료");
        }
    }

    // 1페이즈 전용 사망 상태
    public class DeathState : BossState
    {
        private float deathTimer = 0f;
        private float deathDuration = 5f;
        private bool hasTriggeredNextPhase = false;
        
        public DeathState(Boss boss) : base(boss) { }

        public override void Enter()
        {
            // 1페이즈 사망 애니메이션 재생
            boss.animator.SetTrigger("Death");
            Debug.Log("보스 1페이즈: 사망 상태 시작");
            hasTriggeredNextPhase = false;
        }

        public override void Update()
        {
            deathTimer += Time.deltaTime;
            
            // 사망 애니메이션이 끝나고 다음 페이즈로 전환 또는 게임 종료
            if (deathTimer >= deathDuration && !hasTriggeredNextPhase)
            {
                hasTriggeredNextPhase = true;
                
                // 여기서 2페이즈로 전환하는 코드를 추가할 수 있음
                // 예: GameManager.Instance.ActivateBossPhase2();
                
                Debug.Log("보스 1페이즈 종료: 다음 페이즈 또는 게임 종료 이벤트 발생");
                
                // 1페이즈 보스 제거 - 필요에 따라 조정
                Object.Destroy(boss.gameObject, 1f);
            }
        }

        public override void Exit()
        {
            Debug.Log("보스 1페이즈: 사망 상태 종료");
        }
    }
}