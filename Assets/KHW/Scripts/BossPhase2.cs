using UnityEngine;

namespace Azmodan.Phase2
{
    public class BossPhase2 : Boss
    {
        [SerializeField] private float phase2DamageMultiplier = 0.8f; // 2페이즈 데미지 감소 (더 강해짐)
        [SerializeField] private bool playSpawnEffect = true; // 스폰 시 이펙트 재생 여부

        protected override void Start()
        {
            base.Start();
            
            // 스폰 시 이펙트 재생 (격노 애니메이션 사용)
            if (playSpawnEffect)
            {
                PlaySpawnEffect();
            }
        }

        // 스폰 이펙트 재생 메서드
        private void PlaySpawnEffect()
        {
            // 스폰 이펙트 애니메이션 재생
            animator.SetTrigger("Enraged");
            Debug.Log("보스 2페이즈: 스폰 이펙트 재생");
            
            // 필요한 경우 파티클 효과 등 추가
            // Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
        }

        protected override void InitializeStates()
        {
            // 기존 상태들 유지
            states[typeof(IdleState)] = new IdleState(this);
            states[typeof(WalkState)] = new WalkState(this);
            states[typeof(Attack1State)] = new Attack1State(this);
            states[typeof(Attack2State)] = new Attack2State(this);
            states[typeof(StunState)] = new StunState(this);
            states[typeof(DeathState)] = new DeathState(this);
        }

        // 2페이즈 전용 공격 상태로 전환
        public override void TransitionToAttack()
        {
            // 랜덤하게 공격 패턴 선택
            float randomValue = Random.value;
            if (randomValue < 0.4f) // 40% 확률로 Attack2 사용
            {
                ChangeState<Attack2State>();
            }
            else
            {
                ChangeState<Attack1State>();
            }
        }

        // 2페이즈 전용 데미지 처리
        public override void TakeDamage(int damage)
        {
            // 2페이즈 데미지 배율 적용
            int actualDamage = Mathf.RoundToInt(damage * phase2DamageMultiplier);
            health -= actualDamage;
            
            Debug.Log($"보스 2페이즈: {actualDamage} 데미지 받음 (현재 체력: {health})");
            
            // 사망 처리
            if (health <= 0)
            {
                Die();
            }
        }

        // 2페이즈 전용 사망 처리
        protected override void Die()
        {
            Debug.Log("보스 2페이즈 사망 - 게임 종료");
            TransitionToDeath();
        }
    }

    // 2페이즈 전용 공격 상태 1
    public class Attack1State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 2.5f;
        
        public Attack1State(Boss boss) : base(boss) { }

        public override void Enter()
        {
            attackTimer = 0f;
            boss.animator.SetTrigger("Attack");
            Debug.Log("보스 2페이즈: 공격1 시작");
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= attackDuration)
            {
                boss.TransitionToWalk();
            }
        }
        
        public override void Exit()
        {
            Debug.Log("보스 2페이즈: 공격1 종료");
        }
    }

    // 2페이즈 전용 공격 상태 2
    public class Attack2State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 3.2f;
        
        public Attack2State(Boss boss) : base(boss) { }

        public override void Enter()
        {
            attackTimer = 0f;
            boss.animator.SetTrigger("Attack");
            Debug.Log("보스 2페이즈: 공격2 시작");
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= attackDuration)
            {
                boss.TransitionToWalk();
            }
        }
        
        public override void Exit()
        {
            Debug.Log("보스 2페이즈: 공격2 종료");
        }
    }

    // 2페이즈 전용 사망 상태
    public class DeathState : BossState
    {
        private float deathTimer = 0f;
        private float deathDuration = 5f;
        
        public DeathState(Boss boss) : base(boss) { }

        public override void Enter()
        {
            boss.animator.SetTrigger("Death");
            Debug.Log("보스 2페이즈: 최종 사망");
        }

        public override void Update()
        {
            deathTimer += Time.deltaTime;
            
            if (deathTimer >= deathDuration)
            {
                // 게임 승리 처리
                Debug.Log("보스 2페이즈 사망: 게임 승리!");
                
                // 보스 제거
                Object.Destroy(boss.gameObject);
            }
        }
    }
}