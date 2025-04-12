using UnityEngine;
using UnityEngine.UI;
using System.Collections; // IEnumerator용

namespace Azmodan.Phase2
{
    public class BossPhase2 : Boss
    {

        [Header("Phase 2 Settings")]
        [SerializeField] private float attackDelay = 2f;             // 공격 후 대기 시간
        [SerializeField] private float preAttackDelay = 0.5f;        // 공격 전 대기 시간
        [SerializeField] private float phase2DamageMultiplier = 0.8f;// 2페이즈 데미지 배율
        [SerializeField] private bool playSpawnEffect = true;        // 스폰 이펙트 재생 여부
        [SerializeField] private GameObject spawnEffectPrefab;       // 스폰 이펙트 프리팹

        [Header("Phase 2 Attack Distances")]
        [SerializeField] private float attack1Distance = 13f;  // 근접 공격 거리
        [SerializeField] private float attack2Distance = 16f;  // 원거리 공격 거리

        [Header("Prefabs")]
        [SerializeField] private GameObject missilePrefab;
        [SerializeField] private GameObject minionPrefab;

        [Header("UI")]
        [SerializeField] private Slider healthBarUI;

        [Header("Audio")]
        [SerializeField] public AudioClip attack1Sound;
        [SerializeField] public AudioClip attack2Sound;
        [SerializeField] public AudioClip hitSound;
        [SerializeField] public AudioClip deathSound;
        public AudioSource audioSource;



        // 내부 처리용
        private BossStateType selectedAttackType;
        private int randomAttackNum;

        // 공격 로직 디버깅을 위한 변수
        private bool attackSelected = false;
        private bool attackInitiated = false;
        protected override void Start()
        {
            // 살짝 딜레이 후 Start
            StartCoroutine(DelayedStart());

        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(0.1f);
            audioSource = GetComponent<AudioSource>();

            // Y 위치 고정
            Vector3 fixedPos = transform.position;
            fixedPos.y = 1.5f;
            transform.position = fixedPos;

            // Base Start
            base.Start();

            // targetPlayer 자동 할당
             if (targetPlayer == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    targetPlayer = player;
                    Debug.Log("보스 2페이즈: targetPlayer 자동 설정 완료");
                }
                else
                {
                    Debug.LogWarning("보스 2페이즈: Player 태그 오브젝트를 찾지 못함!");
                }
            }

            // 스폰 이펙트
            if (playSpawnEffect)
            {
                PlaySpawnEffect();
            }

            // 체력바 세팅
            if (healthBarUI != null)
            {
                healthBarUI.maxValue = health;
                healthBarUI.value = health;
            }

            Debug.Log($"보스 2페이즈 초기 체력: {health}");
        }

        private void PlaySpawnEffect()
        {
            // 분노 애니메이션
            animator.SetTrigger("Enraged");
            Debug.Log("보스 2페이즈: 스폰 이펙트 재생 중...");

            // 파티클 Instantiate
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        protected override void Update()
        {
            base.Update(); // 현재 상태 업데이트 등
            // 계속해서 Y 고정
            Vector3 pos = transform.position;
            pos.y = 1.5f;
            transform.position = pos;
        }
        protected override void InitializeStates()
        {
            // Phase2 전용 상태들 등록
            states[typeof(IdleState)]    = new Phase2IdleState(this);
            states[typeof(WalkState)]    = new Phase2WalkState(this);
            states[typeof(Attack1State)] = new Attack1State(this);
            states[typeof(Attack2State)] = new Attack2State(this);
            states[typeof(StunState)]    = new StunState(this);
            states[typeof(DeathState)]   = new DeathState(this);
            states[typeof(TeleportState)] = new TeleportState(this);
        }
        public void SelectAttackType()
        {
            // 이미 공격 선택됨 → 중복 방지
            if (attackSelected)
            {
                Debug.Log("보스: 이미 공격이 선택되어 있음(중복 선택 방지).");
                return;
            }

            // 1~100 범위에서 무작위
            randomAttackNum = Random.Range(1, 101);

            if (randomAttackNum >= 1 && randomAttackNum <= 30)
            {
                attackDistance = attack1Distance;
                selectedAttackType = BossStateType.Attack1;
                Debug.Log($"보스: 근접(소환수) 공격 선택 (거리: {attackDistance})");
            }
            else if (randomAttackNum >= 71 && randomAttackNum <= 100)
            {
                attackDistance = attack2Distance;
                selectedAttackType = BossStateType.Attack2;
                Debug.Log($"보스: 원거리 공격 선택 (거리: {attackDistance})");
            }
            else
            {
                // 중간 영역 → 랜덤
                if (Random.value < 0.5f)
                {
                    attackDistance = attack1Distance;
                    selectedAttackType = BossStateType.Attack1;
                    Debug.Log($"보스: 기본 근접(소환수) 공격 선택");
                }
                else
                {
                    attackDistance = attack2Distance;
                    selectedAttackType = BossStateType.Attack2;
                    Debug.Log($"보스: 기본 원거리 공격 선택");
                }
            }

            // 공격 선택됨
            attackSelected = true;

            // 선딜레이(SubState) 설정
            SetWaitingForAttack(true);

            // Idle로 전환 (IdleState에서 선딜 카운트)
            TransitionToIdle();
            Debug.Log("보스: SelectAttackType → 선딜 ON 설정, Idle로 전환");

        }
        public void SetWaitingForAttack(bool isWaiting)
        {
            if (isWaiting)
            {
                // 선딜레이
                SetSubState(BossSubState.PreAttackDelay, preAttackDelay);
                Debug.Log("보스: 공격 대기(선딜레이) ON");
            }
            else
            {
                // 선딜 상태 해제
                if (IsInSubState(BossSubState.PreAttackDelay))
                {
                    Debug.Log("보스: 공격 대기(선딜레이) OFF");
                }
            }
        }

        public bool IsWaitingForAttack()
        {
            return IsInSubState(BossSubState.PreAttackDelay);
        }

        public void SetPostAttackDelay(bool isDelaying)
        {
            if (isDelaying)
            {
                // 후딜레이
                SetSubState(BossSubState.PostAttackDelay, attackDelay);
                Debug.Log("보스: 공격 후 딜레이 상태 ON");

                // 공격 플래그 리셋
                attackSelected = false;
                attackInitiated = false;
            }
            else
            {
                // 후딜 상태 해제
                if (IsInSubState(BossSubState.PostAttackDelay))
                {
                    Debug.Log("보스: 공격 후 딜레이 상태 OFF");
                }
            }
        }

        public bool IsInPostAttackDelay()
        {
            return IsInSubState(BossSubState.PostAttackDelay);
        }

        public float GetPreAttackDelay() => preAttackDelay;
        public float GetAttackDelay()   => attackDelay;

        public bool IsAttackSelected()    => attackSelected;
        public bool IsAttackInitiated()   => attackInitiated;

        public void ResetAttackFlags()
        {
            attackInitiated = false;
            attackSelected = false;
        }


        public override void TransitionToAttack()
        {
            if (attackInitiated)
            {
                Debug.Log("보스: 이미 공격 중입니다. (중복 호출 무시)");
                return;
            }
            attackInitiated = true;

            Debug.Log($"보스: (Phase2) TransitionToAttack - {selectedAttackType}");

            // 플레이어와 거리 체크
            float distanceToPlayer = 0;
            if (targetPlayer != null)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                dir.y = 0;
                distanceToPlayer = dir.magnitude;
            }

            // 너무 멀면 순간이동
            float teleportThreshold = attackDistance * 1.0f;
            if (distanceToPlayer > teleportThreshold)
            {
                Debug.Log("보스: (Phase2) 순간이동 시도");
                ChangeState<TeleportState>();
                return;
            }

            // 공격 범위 밖
            if (distanceToPlayer > attackDistance * 1.5f)
            {
                Debug.Log("보스: (Phase2) 공격 범위 초과 → Walk 복귀");
                attackInitiated = false;
                TransitionToWalk();
                return;
            }

            // 공격 각도도 벗어남
            if (!IsPlayerInAttackAngle())
            {
                Debug.Log("보스: (Phase2) 공격 각도 벗어남 → Walk 복귀");
                attackInitiated = false;
                TransitionToWalk();
                return;
            }

            // 최종 공격 실행
            if (selectedAttackType == BossStateType.Attack1)
            {
                ChangeState<Attack1State>();
            }
            else
            {
                ChangeState<Attack2State>();
            }
        }

        public override void TakeDamage(int damage)
        {
            // 2페이즈 데미지 배율 적용
            int actualDamage = Mathf.RoundToInt(damage * phase2DamageMultiplier);
            health -= actualDamage;

            Debug.Log($"보스 2페이즈: {actualDamage} 데미지! (HP: {health})");
            if (healthBarUI != null)
            {
                healthBarUI.value = health;
            }

            if (animator != null)
            {
                animator.SetTrigger("Damage"); 
            }

            // 피격 사운드 재생
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            if (health <= 0)
            {
                Die();
            }
        }

        protected override void Die()
        {
            Debug.Log("보스 2페이즈 사망 - 게임 종료");
            
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            TransitionToDeath();
        }
        public bool IsPlayerInAttackAngle()
        {
            if (targetPlayer == null) return false;

            Vector3 dir = targetPlayer.transform.position - transform.position;
            dir.y = 0;
            dir.Normalize();

            Vector3 bossFwd = transform.forward;
            bossFwd.y = 0;
            bossFwd.Normalize();

            float dot = Vector3.Dot(bossFwd, dir);
            float angleToPlayer = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;

            bool isInAngle = angleToPlayer <= 45f;
            if (!isInAngle)
            {
                Debug.Log($"보스: 플레이어가 공격 각도 밖 (각도: {angleToPlayer:F1}°)");
            }

            return isInAngle;
        }

        public void FireMissile()
        {
            if (missilePrefab == null)
            {
                Debug.LogWarning("보스: 미사일 프리팹이 할당되지 않았음");
                return;
            }
            if (targetPlayer == null)
            {
                Debug.LogWarning("보스: 플레이어가 없음(FireMissile 실패)");
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);

            // 미사일에게 타겟 설정
            missile.GetComponent<MissileController>().SetTarget(player.transform);
        }

        public void SpawnMinions()
        {
            if (minionPrefab == null)
            {
                Debug.LogWarning("보스: 미니언 프리팹이 없음");
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

            // 양옆으로 2마리씩 소환
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

            if (phase2Boss != null && phase2Boss.audioSource != null && phase2Boss.attack1Sound != null)
            {
                phase2Boss.audioSource.PlayOneShot(phase2Boss.attack1Sound);
            }
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
            phase2Boss.ResetAttackFlags();

        }
    }

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
            Debug.Log("보스 2페이즈: 공격2 시작");

            if (phase2Boss != null && phase2Boss.audioSource != null && phase2Boss.attack2Sound != null)
            {
                phase2Boss.audioSource.PlayOneShot(phase2Boss.attack2Sound);
            }

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
            Debug.Log("보스 2페이즈: 공격2 종료");
            phase2Boss.ResetAttackFlags();

        }
    }

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

            // 플레이어 방향 계산 → 보스 기준 방향으로 수정
            Vector3 direction = (player.transform.position - phase2Boss.transform.position).normalized;
            direction.y = 0;

            // 새로운 순간이동 위치 계산 (플레이어 쪽으로 일정 거리)
            float teleportDistance = 3f; // 필요 시 조절
            Vector3 teleportPosition = player.transform.position - direction * teleportDistance;

            // Y 위치 고정
            teleportPosition.y = 1.5f;

            // 보스 이동
            phase2Boss.transform.position = teleportPosition;

            Debug.Log("보스 순간이동!");

            phase2Boss.ResetAttackFlags();

            // 투명도 조절 + 복원
            phase2Boss.StartCoroutine(ResetOpacity());

            boss.TransitionToIdle(); // 다음 행동으로
}


        private System.Collections.IEnumerator ResetOpacity()
        {
            yield return new WaitForSeconds(0.5f);

            Renderer[] renderers = phase2Boss.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                foreach (Material mat in r.materials)
                {
                    Color c = mat.color;
                    c.a = 1f;
                    mat.color = c;
                }
            }
        }
    }

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
                Debug.Log("보스 2페이즈 사망: 게임 승리!");
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
            idleTimer += Time.deltaTime;

            // 공격 선딜레이 중
            if (phase2Boss.IsWaitingForAttack())
            {
                lastCheckTime += Time.deltaTime;
                if (lastCheckTime >= checkInterval)
                {
                    lastCheckTime = 0f;

                    if (boss.targetPlayer != null)
                    {
                        Vector3 dir = boss.targetPlayer.transform.position - boss.transform.position;
                        float dist = dir.magnitude;

                        // 너무 멀면 취소 -> Walk
                        if (dist > boss.attackDistance * 1.5f)
                        {
                            phase2Boss.SetWaitingForAttack(false);
                            Debug.Log($"보스: (Phase2) 공격 범위 밖 → Walk");
                            boss.TransitionToWalk();
                            return;
                        }

                        // 각도도 벗어나면 Walk
                        if (!phase2Boss.IsPlayerInAttackAngle())
                        {
                            phase2Boss.SetWaitingForAttack(false);
                            Debug.Log("보스: (Phase2) 플레이어 각도 벗어남 → Walk");
                            boss.TransitionToWalk();
                            return;
                        }

                        // 방향 회전
                        dir.y = 0;
                        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                        boss.transform.rotation = Quaternion.Slerp(
                            boss.transform.rotation,
                            targetRot,
                            boss.rotateSpeed * Time.deltaTime * 0.5f
                        );
                    }
                }

                // 선딜 시간 지남 -> Attack
                if (idleTimer >= phase2Boss.GetPreAttackDelay())
                {
                    phase2Boss.SetWaitingForAttack(false);
                    Debug.Log($"보스: (Phase2) 선딜레이 완료 → Attack");
                    boss.TransitionToAttack();
                }
                return;
            }

            // 공격 후 딜레이 중
            if (phase2Boss.IsInPostAttackDelay())
            {
                if (idleTimer >= phase2Boss.GetAttackDelay())
                {
                    phase2Boss.SetPostAttackDelay(false);
                    Debug.Log($"보스: (Phase2) 후딜 완료 → Walk");
                    boss.TransitionToWalk();
                }
                return;
            }

            // 일반 Idle 대기 끝 -> Walk
            if (idleTimer >= boss.idleDuration)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    boss.targetPlayer = player;
                    Debug.Log("보스: (Phase2) Idle 끝, Walk 시작");
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
            phase2Boss = boss;
        }

        protected override void HandleWalk()
{
    if (boss.targetPlayer == null)
    {
        Debug.LogWarning("[WalkState] targetPlayer 없음 → Idle 복귀");
        boss.TransitionToIdle();
        return;
    }

    Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
    direction.y = 0;
    float distanceToPlayer = direction.magnitude;

    if (distanceToPlayer > boss.attackDistance)
    {
        // 계속 이동
        direction.Normalize();
        boss.transform.position += direction * boss.moveSpeed * Time.deltaTime;

        // 방향 회전
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        boss.transform.rotation = Quaternion.Slerp(
            boss.transform.rotation,
            targetRotation,
            boss.rotateSpeed * Time.deltaTime
        );
    }
    else
    {
        Debug.Log("[WalkState] 공격 거리 도달!");

        if (!phase2Boss.IsPlayerInAttackAngle())
        {
            Debug.Log("[WalkState] 각도 벗어남 → 회전만");
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            boss.transform.rotation = Quaternion.Slerp(
                boss.transform.rotation,
                targetRotation,
                boss.rotateSpeed * Time.deltaTime * 1.5f
            );
            return;
        }

        Debug.Log($"[WalkState] 각도 OK / 공격선택? {phase2Boss.IsAttackSelected()} / 대기중? {phase2Boss.IsWaitingForAttack()} / 공격중? {phase2Boss.IsAttackInitiated()}");

        if (!phase2Boss.IsAttackSelected())
        {
            Debug.Log("[WalkState] 공격 선택 시도!");
            phase2Boss.SelectAttackType();
        }
        else if (!phase2Boss.IsWaitingForAttack() && !phase2Boss.IsAttackInitiated())
        {
            Debug.Log("[WalkState] 공격 진입 시도!");
            boss.TransitionToAttack();
        }
    }
}

    }
}
