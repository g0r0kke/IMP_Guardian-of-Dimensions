using UnityEngine;
using UnityEngine.UI;

namespace Azmodan.Phase2
{
    public class BossPhase2 : Boss
    {

        [Header("Phase 2 Settings")] [SerializeField]
        private float attackDelay = 2f; // 공격 후 대기 시간
        [SerializeField] private float phase2DamageMultiplier = 0.8f; // 2페이즈 데미지 감소 (더 강해짐)
        [SerializeField] private bool playSpawnEffect = true; // 스폰 시 이펙트 재생 여부
        [SerializeField] private float preAttackDelay = 0.5f; // 공격 전 딜레이 시간
        [SerializeField] private GameObject spawnEffectPrefab;

        private BossStateType selectedAttackType;

        [SerializeField] private GameObject missilePrefab;
        [SerializeField] private GameObject minionPrefab;

        // 공격 로직 디버깅을 위한 변수 추가
        private bool attackSelected = false;
        private bool attackInitiated = false;

        [SerializeField] private Slider healthBarUI;


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
            Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);

            Debug.Log($"보스 초기 체력: {health}");

        }

        protected override void InitializeStates()
        {
            // 기존 상태들 유지
            states[typeof(IdleState)] = new Phase2IdleState(this);
            states[typeof(WalkState)] = new Phase2WalkState(this);
            states[typeof(Attack1State)] = new Attack1State(this);
            states[typeof(Attack2State)] = new Attack2State(this);
            states[typeof(StunState)] = new StunState(this);
            states[typeof(DeathState)] = new DeathState(this);
            states[typeof(TeleportState)] = new TeleportState(this);

        }

        // 공격 대기 상태 설정/확인 메서드
        public void SetWaitingForAttack(bool isWaiting)
        {
            if (isWaiting)
            {
                SetSubState(BossSubState.PreAttackDelay, preAttackDelay);
                Debug.Log("보스: 공격 대기(선딜레이) 상태 설정됨");
            }
            else
            {
                // 디버깅 메시지 추가
                if (IsInSubState(BossSubState.PreAttackDelay))
                {
                    Debug.Log("보스: 공격 대기(선딜레이) 상태 해제됨");
                }
            }
        }

        public bool IsWaitingForAttack()
        {
            return IsInSubState(BossSubState.PreAttackDelay);
        }

        // 공격 후 딜레이 상태 설정/확인 메서드
        public void SetPostAttackDelay(bool isDelaying)
        {
            if (isDelaying)
            {
                SetSubState(BossSubState.PostAttackDelay, attackDelay);
                Debug.Log("보스: 공격 후 딜레이 상태 설정됨");

                // 공격 플래그 초기화
                attackSelected = false;
                attackInitiated = false;
            }
            else
            {
                // 디버깅 메시지 추가
                if (IsInSubState(BossSubState.PostAttackDelay))
                {
                    Debug.Log("보스: 공격 후 딜레이 상태 해제됨");
                }
            }
        }

        public bool IsInPostAttackDelay()
        {
            return IsInSubState(BossSubState.PostAttackDelay);
        }

        // 공격 전 딜레이 시간 반환
        public float GetPreAttackDelay()
        {
            return preAttackDelay;
        }

        // 2페이즈 전용 공격 상태로 전환
        public override void TransitionToAttack()
        {
            if (attackInitiated)
            {
                Debug.Log("보스: 이미 공격 중입니다. 중복 호출 방지.");
                return;
            }

            attackInitiated = true;

            // 플레이어 거리 계산
            float distanceToPlayer = 0;
            if (targetPlayer != null)
            {
                Vector3 direction = targetPlayer.transform.position - transform.position;
                direction.y = 0;
                distanceToPlayer = direction.magnitude;
                Debug.Log($"보스: 플레이어와 거리 = {distanceToPlayer}, 공격 거리 = {attackDistance}");
            }

            // 너무 멀면 순간이동
            if (distanceToPlayer > attackDistance * 2f)
            {
                Debug.Log("보스: 순간이동으로 접근");
                ChangeState<TeleportState>();
                return;
            }

            // 범위 바깥이면 Walk으로 되돌림
            if (distanceToPlayer > attackDistance * 1.5f)
            {
                Debug.Log("보스: 공격 거리 초과, Walk 상태로 복귀");
                attackInitiated = false;
                TransitionToWalk();
                return;
            }

            // 공격 각도 확인 (전방 90도)
            if (!IsPlayerInAttackAngle())
            {
                Debug.Log("보스: 공격 각도 벗어남, Walk 상태로 복귀");
                attackInitiated = false;
                TransitionToWalk();
                return;
            }

            // 공격 타입 랜덤 선택
            selectedAttackType = Random.value < 0.4f ? BossStateType.Attack2 : BossStateType.Attack1;

            Debug.Log($"보스: 공격 타입 선택됨 → {selectedAttackType}");

            // ▶ 공격 상태로 실제 전환
            if (selectedAttackType == BossStateType.Attack1)
            {
                ChangeState<Attack1State>();
            }
            else
            {
                ChangeState<Attack2State>();
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

        public bool IsPlayerInAttackAngle()
        {
            if (targetPlayer == null) return false;

            // 보스로부터 플레이어까지의 방향 벡터
            Vector3 directionToPlayer = targetPlayer.transform.position - transform.position;
            directionToPlayer.y = 0; // Y축은 무시 (수평면에서만 체크)
            directionToPlayer.Normalize();

            // 보스의 forward 벡터 (정면 방향)
            Vector3 bossForward = transform.forward;
            bossForward.y = 0; // Y축은 무시
            bossForward.Normalize();

            // 두 벡터 사이의 각도 계산 (내적 사용)
            float dotProduct = Vector3.Dot(bossForward, directionToPlayer);

            // 내적 값을 각도로 변환 (라디안에서 도로 변환)
            float angleToPlayer = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

            // 45도 이내인지 확인 (90도 범위 = 양쪽으로 45도씩)
            bool isInAngle = angleToPlayer <= 45f;

            // 디버그 로그
            if (!isInAngle)
            {
                Debug.Log($"보스: 플레이어가 공격 각도 밖에 있습니다. (각도: {angleToPlayer}°)");
            }

            return isInAngle;
        }

        public void FireMissile()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
            missile.GetComponent<MissileController>().SetTarget(player.transform);
        }

        public void SpawnMinions()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            Vector3 bossPos = transform.position;
            Vector3 rightDir = transform.right;
            float offset = 2f;

            for (int i = 0; i < 2; i++)
            {
                Vector3 spawnPos = bossPos + rightDir * (offset + i);
                spawnPos.y = bossPos.y;
                GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
                minion.GetComponent<MinionController>().SetTarget(player.transform);
            }

            for (int i = 0; i < 2; i++)
            {
                Vector3 spawnPos = bossPos - rightDir * (offset + i);
                spawnPos.y = bossPos.y;
                GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
                minion.GetComponent<MinionController>().SetTarget(player.transform);
            }
        }

    }

    // 2페이즈 전용 공격 상태 1
    public class Attack1State : BossState
{
    private float attackTimer = 0f;
    private float attackDuration = 2.5f;
    private bool hasSpawnedMinions = false;
    private BossPhase2 phase2Boss;

    public Attack1State(Boss boss) : base(boss)
    {
        phase2Boss = boss as BossPhase2;
    }

    public override void Enter()
    {
        attackTimer = 0f;
        hasSpawnedMinions = false;

        boss.animator.SetTrigger("Attack");
        Debug.Log("보스 2페이즈: 공격1 시작");
    }

    public override void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= 0.5f && !hasSpawnedMinions)
        {
            phase2Boss.SpawnMinions();
            hasSpawnedMinions = true;
        }

        if (attackTimer >= attackDuration)
        {
            phase2Boss.SetPostAttackDelay(true);
            boss.TransitionToIdle();
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
    private float attackDuration = 3f;
    private bool hasFiredMissile = false;
    private BossPhase2 phase2Boss;

    public Attack2State(Boss boss) : base(boss)
    {
        phase2Boss = boss as BossPhase2;
    }

    public override void Enter()
    {
        attackTimer = 0f;
        hasFiredMissile = false;

        boss.animator.SetTrigger("Attack");
        Debug.Log("보스 2페이즈: 공격2 상태 시작");
    }

    public override void Update()
    {
        attackTimer += Time.deltaTime;

        // 중간 타이밍에 미사일 발사
        if (attackTimer >= 1.5f && !hasFiredMissile)
        {
            phase2Boss.FireMissile();
            hasFiredMissile = true;
        }

        if (attackTimer >= attackDuration)
        {
            phase2Boss.SetPostAttackDelay(true);
            boss.TransitionToIdle();
        }
    }

    public override void Exit()
    {
        Debug.Log("보스 2페이즈: 공격2 상태 종료");
    }
}


    // 2페이즈 순간이동
    public class TeleportState : BossState
    {
        private float teleportDistance = 2f;
        private BossPhase2 phase2Boss;

        public TeleportState(Boss boss) : base(boss)
        {
            phase2Boss = boss as BossPhase2;
        }

        public override void Enter()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            Vector3 forward = player.transform.forward;
            Vector3 targetPosition = player.transform.position + forward * teleportDistance;

            phase2Boss.transform.position = targetPosition;

            Debug.Log("보스 순간이동!");
            boss.TransitionToIdle(); // 또는 Walk 등
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

    public class Phase2IdleState : IdleState
    {
        private BossPhase2 phase2Boss;
        
        public Phase2IdleState(BossPhase2 boss) : base(boss)
        {
            this.phase2Boss = boss;
        }
        
        protected override void HandleStateLogic()
        {
            // Phase2 전용 Idle 로직 구현
            // 기본적으로 플레이어 탐색 후 Walk 상태로 전환
            if (idleTimer >= boss.idleDuration)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                
                if (player != null)
                {
                    boss.targetPlayer = player;
                    boss.TransitionToWalk();
                }
            }
        }
    }
    
    public class Phase2WalkState : WalkState
    {
        private BossPhase2 phase2Boss;
        
        public Phase2WalkState(BossPhase2 boss) : base(boss)
        {
            this.phase2Boss = boss;
        }
        
        protected override void HandleMovement()
        {
            // Phase2 전용 이동 로직 구현
            Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
            direction.y = 0;
            
            float distanceToPlayer = direction.magnitude;
            
            if (distanceToPlayer > boss.attackDistance)
            {
                direction.Normalize();
                boss.transform.position += direction * boss.moveSpeed * Time.deltaTime;
                
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                boss.transform.rotation = Quaternion.Slerp(
                    boss.transform.rotation,
                    targetRotation,
                    boss.rotateSpeed * Time.deltaTime
                );
            }
            else
            {
                // 공격 거리에 도달하면 공격 상태로 전환
                boss.TransitionToAttack();
            }
        }
    }
}