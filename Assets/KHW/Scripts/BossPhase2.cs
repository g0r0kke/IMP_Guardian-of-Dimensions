using UnityEngine;
using UnityEngine.UI;
using System.Collections; // IEnumerator용


namespace Azmodan.Phase2
{
    public class BossPhase2 : Boss
    {

        [Header("Phase 2 Settings")]
        [SerializeField] private float attackDelay = 2f;        // 공격 후 대기 시간(후딜레이)
        [SerializeField] private float preAttackDelay = 0.5f;   // 공격 전 대기 시간(선딜레이)
        [SerializeField] private float phase2DamageMultiplier = 0.8f; // 2페이즈 데미지 감소 (더 강해짐)
        [SerializeField] private bool playSpawnEffect = true; // 스폰 시 이펙트 재생 여부
        [SerializeField] private GameObject spawnEffectPrefab;

        // 공격 거리(근접/원거리)
        [Header("Phase2 Attack Distances")]
        // BossPhase2.cs 상단 (변수 선언 쪽)
[SerializeField] private float attackDistance = 100f; // 💥 추가 또는 초기값 수정

        [SerializeField] private float attack1Distance = 13f;  // 근접 공격 사거리
        [SerializeField] private float attack2Distance = 20f;    // 원거리 공격 사거리

        [Header("Prefabs")]
        [SerializeField] private GameObject missilePrefab;
        [SerializeField] private GameObject minionPrefab;

        // UI (체력바)
        [SerializeField] private Slider healthBarUI;

        // 내부 처리용
        private BossStateType selectedAttackType;

        // 공격 로직 디버깅을 위한 변수 추가
        private bool attackSelected = false;
        private bool attackInitiated = false;

        protected override void Start()
        {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(0.1f); // 딜레이 조금 늘림

            // Y 위치 강제 고정
            Vector3 fixedPos = transform.position;
            fixedPos.y = 1.5f;
            transform.position = fixedPos;

            base.Start();

            // Phase2 초기 스폰 이펙트
            if (playSpawnEffect)
            {
                PlaySpawnEffect();
            }

            // 초기 체력 로그
            Debug.Log($"보스 2페이즈 초기 체력: {health}");

            // UI 설정 (있다면)
            if (healthBarUI != null)
            {
                healthBarUI.maxValue = health;
                healthBarUI.value = health;
            }
        }

        private void PlaySpawnEffect()
        {
            // 분노 애니메이션
            animator.SetTrigger("Enraged");
            Debug.Log("보스 2페이즈: 스폰 이펙트 재생 중...");

            // 파티클 프리팹 Instantiate
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
            }
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

        // 보스가 공격 타입을 랜덤 선택 
        public void SelectAttackType()
        {
            if (attackSelected)
            {
                Debug.Log("보스: 이미 공격이 선택되어 있음(중복 선택 방지).");
                return;
            }

            // 랜덤 1~100
            int randomAttackNum = Random.Range(1, 101);
            if (randomAttackNum <= 50)
            {
                // 소환물물 공격
                attackDistance = attack1Distance;
                selectedAttackType = BossStateType.Attack1;
                Debug.Log($"보스: (Phase2) 소환물물 공격 선택됨, 거리: {attackDistance}");
            }
            else
            {
                // 원거리 공격
                attackDistance = attack2Distance;
                selectedAttackType = BossStateType.Attack2;
                Debug.Log($"보스: (Phase2) 원거리 공격 선택됨, 거리: {attackDistance}");
            }

            // 공격 선택 플래그
            attackSelected = true;

            // 선딜레이(SubState) 설정
            SetWaitingForAttack(true);

            // Idle로 전환 (Idle에서 선딜레이를 카운트)
            TransitionToIdle();
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

        public float GetAttackDelay()
        {
            return attackDelay;
        }


        public bool IsInPostAttackDelay()
        {
            return IsInSubState(BossSubState.PostAttackDelay);
        }

        public bool IsAttackSelected()
        {
            return attackSelected;
        }

        public bool IsAttackInitiated()
        {
            return attackInitiated;
        }

        // 공격 전 딜레이 시간 반환
        public float GetPreAttackDelay()
        {
            return preAttackDelay;
        }

        protected override void Update()
{
    base.Update(); // 상태 업데이트 유지!

    // Y 고정
    Vector3 pos = transform.position;
    pos.y = 1.5f;
    transform.position = pos;
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

            Debug.Log($"보스: (Phase2) TransitionToAttack 호출 - 선택된 공격: {selectedAttackType}");

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
            float teleportThreshold = attackDistance * 2.0f;
            if (distanceToPlayer > teleportThreshold)
            {
                Debug.Log("보스: 공격 거리 2배 이상. 순간이동으로 접근 시도");
                // 공격 다시 시도하기 위해 Walk 등으로 안 빠지고 직접 TeleportState로 전환
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

            // 최종 공격 실행
            Debug.Log($"보스: (Phase2) 공격 시작 => {selectedAttackType}");

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

            if (healthBarUI != null)
            {
                healthBarUI.value = health;
            }
            
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
            if (missilePrefab == null)
            {
                Debug.LogWarning("보스: 미사일 프리팹이 할당되지 않음");
                return;
            }

            if (targetPlayer == null)
            {
                Debug.LogWarning("보스: 플레이어가 없음(FireMissile 실패)");
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
            missile.GetComponent<MissileController>().SetTarget(player.transform);
        }

        public void SpawnMinions()
        {
            if (minionPrefab == null)
            {
                Debug.LogWarning("보스: 미니언 프리팹이 할당되지 않음");
                return;
            }

            if (targetPlayer == null)
            {
                Debug.LogWarning("보스: 플레이어가 없음(SpawnMinions 실패)");
                return;
            }

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

            // 보스 전체 머티리얼의 알파값 낮추기
            Renderer[] renderers = phase2Boss.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    Color color = mat.color;
                    color.a = 0.3f; // 투명도 설정 (0=완전 투명, 1=불투명)
                    mat.color = color;
                }
            }

            // 보스 순간이동
            Vector3 forward = player.transform.forward;
            Vector3 targetPosition = player.transform.position + forward * teleportDistance;
            phase2Boss.transform.position = targetPosition;

            Debug.Log("보스 순간이동!");

            // 0.5초 후 다시 불투명하게 되돌리기
            phase2Boss.StartCoroutine(ResetOpacity());

            boss.TransitionToIdle(); // 필요에 따라 Walk 등
        }

        // Opacity 복원용 Coroutine
        private IEnumerator ResetOpacity()
        {
            yield return new WaitForSeconds(0.5f);

            Renderer[] renderers = phase2Boss.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    Color color = mat.color;
                    color.a = 1f; // 완전 불투명
                    mat.color = color;
                }
            }
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
        private float lastCheckTime = 0f;
        private float checkInterval = 0.1f;

        public Phase2IdleState(BossPhase2 boss) : base(boss)
        {
            phase2Boss = boss;
        }

        protected override void HandleIdle()
        {
        Debug.Log("[디버깅] Phase2IdleState.HandleIdle() 호출 중");

            idleTimer += Time.deltaTime;

            // 선딜레이 중(공격 대기)
            if (phase2Boss.IsWaitingForAttack())
            {
                lastCheckTime += Time.deltaTime;
                if (lastCheckTime >= checkInterval)
                {
                    lastCheckTime = 0f;

                    // 플레이어 거리/각도 체크
                    if (boss.targetPlayer != null)
                    {
                        Vector3 dir = boss.targetPlayer.transform.position - boss.transform.position;
                        float dist = dir.magnitude;

                        // 너무 멀리 벗어나면 공격 취소 후 다시 추적
                        if (dist > boss.attackDistance * 1.5f)
                        {
                            Debug.Log($"보스: (Phase2) 플레이어가 공격 범위 밖 (거리: {dist:F2}), 추적 재개");
                            phase2Boss.SetWaitingForAttack(false);
                            boss.TransitionToWalk();
                            return;
                        }

                        // 각도도 확인
                        if (!phase2Boss.IsPlayerInAttackAngle())
                        {
                            Debug.Log("보스: (Phase2) 플레이어 각도 벗어남, 추적으로 전환");
                            phase2Boss.SetWaitingForAttack(false);
                            boss.TransitionToWalk();
                            return;
                        }

                        // 방향으로 회전
                        dir.y = 0;
                        dir.Normalize();
                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        boss.transform.rotation = Quaternion.Slerp(
                            boss.transform.rotation,
                            targetRot,
                            boss.rotateSpeed * Time.deltaTime * 0.5f
                        );
                    }
                }

                // 선딜레이 시간 지나면 실제 공격으로 전환
                if (idleTimer >= phase2Boss.GetPreAttackDelay())
                {
                    Debug.Log($"보스: (Phase2) 선딜레이 완료({idleTimer:F2}초). 공격 진입!");
                    phase2Boss.SetWaitingForAttack(false);

                    // 공격 수행
                    boss.TransitionToAttack();
                }
                return;
            }

            // 공격 후 딜레이 중
            if (phase2Boss.IsInPostAttackDelay())
            {
                if (idleTimer >= phase2Boss.GetAttackDelay())
                {
                    Debug.Log($"보스: (Phase2) 공격 후 딜레이 종료({idleTimer:F2}초). Walk 전환");
                    phase2Boss.SetPostAttackDelay(false);
                    boss.TransitionToWalk();
                }
                return;
            }

            if (idleTimer >= boss.idleDuration)
{
    Debug.Log($"[디버깅] Idle 타이머 도달: {idleTimer:F2}s / 기준: {boss.idleDuration}s");

    GameObject player = GameObject.FindGameObjectWithTag("Player");

    if (player != null)
    {
        Debug.Log("[디버깅] Player 태그 오브젝트 찾음!");
        boss.targetPlayer = player;

        Debug.Log("보스: (Phase2) Idle 종료, 플레이어 추적 시작");
        boss.TransitionToWalk();
    }
    else
    {
        Debug.LogWarning("[디버깅] Player 태그 오브젝트를 찾을 수 없음!!");
    }
}



        }
    }

    
    public class Phase2WalkState : WalkState
    {
        private BossPhase2 phase2Boss;

        public Phase2WalkState(BossPhase2 boss) : base(boss)
        {
            phase2Boss = boss;
        }

        protected override void HandleWalk()
{
    if (boss.targetPlayer == null)
    {
        Debug.LogWarning("보스: (Phase2) 플레이어가 없음, Idle로 전환");
        boss.TransitionToIdle();
        return;
    }

    // 이동 방향 계산
    Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
    direction.y = 0;
    float distanceToPlayer = direction.magnitude;

    Debug.Log($"[디버깅] 현재 거리: {distanceToPlayer:F2}, 목표 공격 거리: {boss.attackDistance}");
    Debug.Log($"[디버깅] 공격 선택됨? {phase2Boss.IsAttackSelected()} / 선딜레이 중? {phase2Boss.IsWaitingForAttack()} / 공격 중? {phase2Boss.IsAttackInitiated()}");

    if (distanceToPlayer > boss.attackDistance)
    {
        // 계속 이동
        direction.Normalize();
        boss.transform.position += direction * boss.moveSpeed * Time.deltaTime;

        // 회전
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        boss.transform.rotation = Quaternion.Slerp(
            boss.transform.rotation,
            targetRotation,
            boss.rotateSpeed * Time.deltaTime
        );
    }
    else
    {
        Debug.Log("보스: (Phase2) 공격 거리 도달");

        // 각도 체크
        if (!phase2Boss.IsPlayerInAttackAngle())
        {
            Debug.Log("보스: (Phase2) 공격 각도 밖 → 회전 중...");
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            boss.transform.rotation = Quaternion.Slerp(
                boss.transform.rotation,
                targetRotation,
                boss.rotateSpeed * Time.deltaTime * 1.5f
            );
            return;
        }

        // 디버깅 로그 추가
        Debug.Log("[디버깅] 공격 각도 OK!");

        // 공격 선택 안 됐으면 선택 시도
        if (!phase2Boss.IsAttackSelected())
        {
            Debug.Log("[디버깅] 공격 선택 안됨 → SelectAttackType() 호출 예정");
            phase2Boss.SelectAttackType();  // 여기가 안 불리는지 확인
        }
        else
        {
            Debug.Log("[디버깅] 공격 선택 완료됨");

            // 공격 시작 조건
            if (!phase2Boss.IsWaitingForAttack() && !phase2Boss.IsAttackInitiated())
            {
                Debug.Log("[디버깅] 공격 상태 진입 시도!");
                boss.TransitionToAttack();
            }
        }
    }
}

    }
}