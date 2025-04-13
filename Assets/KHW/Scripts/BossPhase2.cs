using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Azmodan.Phase2
{
    public class BossPhase2 : Boss
    {
        [Header("Phase 2 Settings")]
        [SerializeField] private float phase2DamageMultiplier = 0.8f; // 2페이즈 데미지 배율
        [SerializeField] private bool playSpawnEffect = true;        // 스폰 이펙트 재생 여부
        [SerializeField] private GameObject spawnEffectPrefab;       // 스폰 이펙트 프리팹

        [Header("Attack Common Settings")]
        [SerializeField] private float preAttackDelay = 1.0f;   // 공격 전 대기 (선딜)
        [SerializeField] private float postAttackDelay = 2.0f;  // 공격 후 대기 (후딜)

        [Header("Attack Cooldowns")]
        [SerializeField] private float minionSummonCooldown = 15f; // 미니언 소환 쿨타임
        [SerializeField] private float missileCooldown = 5f;       // 미사일 공격 쿨타임
        [SerializeField] private float closeCombatCooldown = 5f;   // 근접 공격 쿨타임

        // 각각 언제 마지막으로 공격했는지 기록
        private float lastMinionSummonTime = -9999f;
        private float lastMissileTime = -9999f;
        private float lastCloseCombatTime = -9999f;

        [Header("Attack Distances")]
        [SerializeField] private float minionAttackDistance = 15f;  
        [SerializeField] private float missileAttackDistance = 18f; 
        [SerializeField] private float closeCombatDistance = 5f;    

        [Header("Prefabs")]
        [SerializeField] private GameObject missilePrefab;
        [SerializeField] private GameObject minionPrefab;

        [Header("UI")]
        [SerializeField] private Slider healthBarUI;
        [SerializeField] private int maxHealth = 100; 

        [Header("Audio")]
        [SerializeField] public AudioClip minionSummonSound; // Attack1
        [SerializeField] public AudioClip missileSound;      // Attack2
        [SerializeField] public AudioClip closeCombatSound;  // Attack3
        [SerializeField] public AudioClip hitSound;
        [SerializeField] public AudioClip deathSound;
        public AudioSource audioSource;

        // 내부 상태 관리
        private BossStateType selectedAttackType;
        private bool attackSelected = false;
        private bool attackInitiated = false;

        public bool isAttacking = false;
        public bool isTakingDamage = false;
         private float damageCooldown = 0.4f; // 연속 피격 방지 시간
        private float lastDamageTime = -999f;

        protected override void Start()
        {
            StartCoroutine(DelayedStart());

            if (GameManager.Instance != null)
            {
                Vector3 bossPosition = GameManager.Instance.GetBossPosition();
                transform.position = bossPosition;
                Debug.Log($"GameManager에서 가져온 보스 위치로 설정됨: {bossPosition}");
            }
            else
            {
                Debug.LogWarning("GameManager를 찾을 수 없습니다. 기본 위치를 사용합니다.");
            }
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(0.1f);

            audioSource = GetComponent<AudioSource>();

            // Y 위치 고정
            Vector3 fixedPos = transform.position;
            fixedPos.y = 1.5f;
            transform.position = fixedPos;

            // 최대체력 / 현재체력 설정
            health = maxHealth;

            base.Start(); // Boss.cs Start() 호출

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
            if (playSpawnEffect) PlaySpawnEffect();

            // 체력바 세팅
            if (healthBarUI != null)
            {
                healthBarUI.maxValue = maxHealth;
                healthBarUI.value = health;
            }

            Debug.Log($"보스 2페이즈 초기 체력: {health}");
        }

        private void PlaySpawnEffect()
        {
            // 분노(Enraged) 애니메이션
            if (animator != null) animator.SetTrigger("Enraged");
            Debug.Log("보스 2페이즈: 스폰 이펙트 재생 중...");

            // 파티클 Instantiate
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        protected override void Update()
        {
            base.Update(); // 상태머신 Update

            // 계속해서 Y 고정 (떨어지지 않도록)
            Vector3 pos = transform.position;
            pos.y = 1.5f;
            transform.position = pos;
        }

        protected override void InitializeStates()
        {
            // 2페이즈 전용 상태들 등록
            states[typeof(IdleState)]      = new Phase2IdleState(this);
            states[typeof(WalkState)]      = new Phase2WalkState(this);
            states[typeof(Attack1State)]   = new Attack1State(this); // 미니언 소환
            states[typeof(Attack2State)]   = new Attack2State(this); // 미사일
            states[typeof(Attack3State)]   = new Attack3State(this); // 근접 공격
            states[typeof(StunState)]      = new StunState(this);
            states[typeof(DeathState)]     = new DeathState(this);
            states[typeof(TeleportState)]  = new TeleportState(this);
        }

        #region Attack Selection Logic
        public void SelectAttackType()
        {
            if (attackSelected)
            {
                Debug.Log("보스: 이미 공격이 선택되어 있음(중복 선택 방지).");
                return;
            }

            // 플레이어 거리 계산
            float distanceToPlayer = 9999f;
            if (targetPlayer != null)
            {
                distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);
            }

            // 쿨타임 체크 (사용 가능한 공격만 후보에 넣기)
            bool canSummonMinion = (Time.time - lastMinionSummonTime >= minionSummonCooldown);
            bool canMissile      = (Time.time - lastMissileTime      >= missileCooldown);
            bool canCloseCombat  = (Time.time - lastCloseCombatTime  >= closeCombatCooldown);

            // 공격 후보 리스트
            var possibleAttacks = new System.Collections.Generic.List<BossStateType>();

            // 거리 & 쿨타임을 만족하면 후보 등록
            if (canSummonMinion && distanceToPlayer <= minionAttackDistance)
                possibleAttacks.Add(BossStateType.Attack1);

            if (canMissile && distanceToPlayer <= missileAttackDistance)
                possibleAttacks.Add(BossStateType.Attack2);

            // 근접은 더 짧은 거리 필요
            if (canCloseCombat && distanceToPlayer <= closeCombatDistance)
                possibleAttacks.Add(BossStateType.Attack3);

            // 아무것도 후보가 없으면 -> Walk
            if (possibleAttacks.Count == 0)
            {
                Debug.Log("보스: 사용 가능한 공격 없음(쿨타임 or 거리 문제) → Walk 복귀");
                TransitionToWalk();
                return;
            }

            // 후보 중 랜덤 선택
            selectedAttackType = possibleAttacks[Random.Range(0, possibleAttacks.Count)];
            Debug.Log($"보스: {selectedAttackType} 공격 선택!");

            attackSelected = true;
            SetWaitingForAttack(true);

            // Idle로 전환 (IdleState에서 선딜 카운트)
            TransitionToIdle();
        }
        #endregion

        #region Attack Delay Handling
        public void SetWaitingForAttack(bool isWaiting)
        {
            if (isWaiting)
            {
                // 선딜(PreAttackDelay) 서브 스테이트
                SetSubState(BossSubState.PreAttackDelay, preAttackDelay);
            }
            else
            {
                if (IsInSubState(BossSubState.PreAttackDelay))
                {
                    // 선딜 해제
                }
            }
        }
        public bool IsWaitingForAttack() => IsInSubState(BossSubState.PreAttackDelay);

        public void SetPostAttackDelay(bool isDelaying)
        {
            if (isDelaying)
            {
                // 후딜(PostAttackDelay)
                SetSubState(BossSubState.PostAttackDelay, postAttackDelay);
                // 공격 플래그들 리셋
                attackSelected = false;
                attackInitiated = false;
            }
            else
            {
                if (IsInSubState(BossSubState.PostAttackDelay))
                {
                    // 후딜 해제
                }
            }
        }
        public bool IsInPostAttackDelay() => IsInSubState(BossSubState.PostAttackDelay);

        public float GetPreAttackDelay() => preAttackDelay;
        public float GetPostAttackDelay() => postAttackDelay;
        #endregion

        #region Attack State Transition
        public bool IsAttackSelected() => attackSelected;
        public bool IsAttackInitiated() => attackInitiated;

        public void ResetAttackFlags()
        {
            attackInitiated = false;
            attackSelected = false;
        }

        public override void TransitionToAttack()
        {
            if (attackInitiated)
            {
                Debug.Log("보스: 이미 공격 중 (중복 호출 무시)");
                return;
            }
            attackInitiated = true;

            Debug.Log($"보스: (Phase2) TransitionToAttack - {selectedAttackType}");

            // 거리 체크
            float distanceToPlayer = 0f;
            if (targetPlayer != null)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                dir.y = 0;
                distanceToPlayer = dir.magnitude;
            }

            // 선택된 공격 범위를 넘어서면 → Teleport 시도
            float selectedAttackDistance = 0f;
            switch (selectedAttackType)
            {
                case BossStateType.Attack1: selectedAttackDistance = minionAttackDistance; break;
                case BossStateType.Attack2: selectedAttackDistance = missileAttackDistance; break;
                case BossStateType.Attack3: selectedAttackDistance = closeCombatDistance; break;
            }

            if (distanceToPlayer > selectedAttackDistance * 1.3f)
            {
                Debug.Log("보스: (Phase2) 공격 거리 넘김 → 순간이동 시도");
                ChangeState<TeleportState>();
                return;
            }

            // 각도 벗어나면 Walk
            if (!IsPlayerInAttackAngle())
            {
                Debug.Log("보스: (Phase2) 공격 각도 벗어남 → Walk 복귀");
                attackInitiated = false;
                TransitionToWalk();
                return;
            }

            // 최종 공격 실행
            switch (selectedAttackType)
            {
                case BossStateType.Attack1: ChangeState<Attack1State>(); break; // 미니언
                case BossStateType.Attack2: ChangeState<Attack2State>(); break; // 미사일
                case BossStateType.Attack3: ChangeState<Attack3State>(); break; // 근접 공격
            }
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

            // 정면 기준 45도 이내일 때만 공격 가능
            bool isInAngle = angleToPlayer <= 45f;
            if (!isInAngle)
            {
                Debug.Log($"보스: 플레이어가 공격 각도 밖 (각도: {angleToPlayer:F1}°)");
            }
            return isInAngle;
        }
        #endregion

        #region Attack Behaviors (Minion, Missile, CloseCombat)
        public void FireMissile()
        {
            if (missilePrefab == null)
            {
                Debug.LogWarning("보스: 미사일 프리팹이 없음");
                return;
            }
            if (targetPlayer == null)
            {
                Debug.LogWarning("보스: 플레이어 없음 → 미사일 발사 불가");
                return;
            }

            // 마지막 미사일 공격 시점 기록
            lastMissileTime = Time.time;

            // 미사일 생성
            GameObject missile = Instantiate(missilePrefab, transform.position + Vector3.up, Quaternion.identity);
            MissileController missileCtrl = missile.GetComponent<MissileController>();
            if (missileCtrl != null)
            {
                missileCtrl.SetTarget(targetPlayer.transform);
            }
        }

        public void SpawnMinions()
        {
            if (minionPrefab == null || targetPlayer == null)
            {
                Debug.LogWarning("보스: 프리팹 또는 플레이어 없음");
                return;
            }

            lastMinionSummonTime = Time.time;

            Vector3 bossPos = transform.position;
            Vector3 forwardDir = transform.forward;
            Vector3 rightDir = transform.right;

            float forwardOffset = 4f;
            float sideOffset = 1.5f;

            // [앞 + 오른쪽] / [앞 + 왼쪽]
            Vector3[] offsets = new Vector3[]
            {
                forwardDir * forwardOffset + rightDir * sideOffset,
                forwardDir * forwardOffset - rightDir * sideOffset,
                forwardDir * (forwardOffset + 1.5f), // 약간 더 전방
                forwardDir * (forwardOffset - 1.5f)  // 약간 더 가까이
            };

            foreach (Vector3 offset in offsets)
            {
                Vector3 spawnPos = bossPos + offset;
                spawnPos.y = GetGroundY(spawnPos); // 바닥 고정
                Hobgoblin.Spawner(minionPrefab, spawnPos, targetPlayer.transform);
            }

        }

        private float GetGroundY(Vector3 position)
        {
            Ray ray = new Ray(position + Vector3.up * 5f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, LayerMask.GetMask("Default", "Terrain")))
            {
                return hit.point.y;
            }
            return position.y;
        }


            


        public void CloseCombatAttack()
        {
            // 마지막 근접 공격 시점 기록
            lastCloseCombatTime = Time.time;

            // 보스 주변 일정 범위 내 플레이어에게 데미지
            float attackRadius = 2.5f;
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    SamplePlayer player = hit.GetComponent<SamplePlayer>();
                    if (player != null)
                    {
                        player.TakeDamage(10); 
                        Debug.Log("보스 근접공격 → 플레이어 데미지");
                    }
                }
            }
        }
        #endregion

        #region Damage & Death
         public override void TakeDamage(int damage)
        {
            if (Time.time - lastDamageTime < damageCooldown)
                return;

            lastDamageTime = Time.time;

            int actualDamage = Mathf.RoundToInt(damage * phase2DamageMultiplier);
            health -= actualDamage;

            if (healthBarUI != null)
                healthBarUI.value = health;

            if (animator != null && !isAttacking)
            {
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("Idle");
                animator.SetTrigger("Damage");
            }

            if (audioSource != null && hitSound != null)
                audioSource.PlayOneShot(hitSound);

            Debug.Log($"보스 2페이즈: {actualDamage} 데미지! 남은 HP: {health}");

            if (health <= 0)
            {
                Die();
            }
        }



        protected override void Die()
        {
            Debug.Log("보스 2페이즈 사망 처리");

            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            ChangeState<DeathState>();
        }
        #endregion
    }

    #region State Classes
    // --------------------------------------------------
    public class Phase2IdleState : IdleState
    {
        private BossPhase2 phase2Boss;
        private float lastCheckTime = 0f;
        private float checkInterval = 0.2f;

        public Phase2IdleState(BossPhase2 boss) : base(boss)
        {
            phase2Boss = boss;
        }

        protected override void HandleIdle()
        {
            idleTimer += Time.deltaTime;

            // 공격 선딜(PreAttackDelay) 중이면
            if (phase2Boss.IsWaitingForAttack())
            {
                lastCheckTime += Time.deltaTime;
                if (lastCheckTime >= checkInterval)
                {
                    lastCheckTime = 0f;
                    // 거리/각도 다시 확인
                    if (boss.targetPlayer != null)
                    {
                        Vector3 dir = boss.targetPlayer.transform.position - boss.transform.position;
                        float dist = dir.magnitude;

                        if (dist > boss.attackDistance * 1.5f)
                        {
                            phase2Boss.SetWaitingForAttack(false);
                            Debug.Log("보스: 선딜 중 거리 벗어남 → Walk");
                            boss.TransitionToWalk();
                            return;
                        }
                        if (!phase2Boss.IsPlayerInAttackAngle())
                        {
                            phase2Boss.SetWaitingForAttack(false);
                            Debug.Log("보스: 선딜 중 각도 벗어남 → Walk");
                            boss.TransitionToWalk();
                            return;
                        }

                        // Idle 상태에서 플레이어 방향으로 서서히 회전
                        dir.y = 0;
                        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                        boss.transform.rotation = Quaternion.Slerp(
                            boss.transform.rotation,
                            targetRot,
                            boss.rotateSpeed * Time.deltaTime
                        );
                    }
                }

                // 선딜 시간 완료 → Attack 진입
                if (idleTimer >= phase2Boss.GetPreAttackDelay())
                {
                    phase2Boss.SetWaitingForAttack(false);
                    boss.TransitionToAttack();
                }
                return;
            }

            // 공격 후딜(PostAttackDelay) 중이면
            if (phase2Boss.IsInPostAttackDelay())
            {
                if (idleTimer >= phase2Boss.GetPostAttackDelay())
                {
                    phase2Boss.SetPostAttackDelay(false);
                    Debug.Log("보스: 후딜 종료 → Walk");
                    boss.TransitionToWalk();
                }
                return;
            }

            // 일반 Idle
            if (idleTimer >= boss.idleDuration)
            {
                // Walk로 전환
                boss.TransitionToWalk();
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
                Debug.LogWarning("보스: Walk 중 targetPlayer 없음 → Idle 복귀");
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
                Debug.Log("보스: 공격 범위 도달!");

                if (!phase2Boss.IsPlayerInAttackAngle())
                {
                    Debug.Log("보스: 각도 벗어남 → 회전만 진행");
                    direction.Normalize();
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    boss.transform.rotation = Quaternion.Slerp(
                        boss.transform.rotation,
                        targetRot,
                        boss.rotateSpeed * Time.deltaTime * 1.5f
                    );
                    return;
                }

                // 공격이 이미 선택된 상태가 아니면 공격 선택
                if (!phase2Boss.IsAttackSelected())
                {
                    phase2Boss.SelectAttackType();
                }
                else if (!phase2Boss.IsWaitingForAttack() && !phase2Boss.IsAttackInitiated())
                {
                    // 선딜도 없고 아직 공격 개시 전이라면
                    boss.TransitionToAttack();
                }
            }
        }
    }

    // Attack1State: 미니언 소환
    public class Attack1State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 2.0f;
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
            phase2Boss.isAttacking = true;
            boss.animator.ResetTrigger("Damage");
            boss.animator.SetTrigger("Attack");

            Debug.Log("보스 2페이즈: 소환 공격 시작");
            if (phase2Boss != null && phase2Boss.audioSource != null && phase2Boss.minionSummonSound != null)
            {
                phase2Boss.audioSource.PlayOneShot(phase2Boss.minionSummonSound);
            }
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            if (!hasSpawnedMinions && attackTimer >= 0.5f)
            {
                phase2Boss.SpawnMinions();
                hasSpawnedMinions = true;
            }

            if (attackTimer >= attackDuration)
            {
                // 후딜로 전환
                phase2Boss.SetPostAttackDelay(true);
                boss.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            phase2Boss.isAttacking = false;
            phase2Boss.ResetAttackFlags();
        }
    }

    // Attack2State: 미사일 발사
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
            boss.animator.ResetTrigger("Damage");
            boss.animator.SetTrigger("Attack");


            Debug.Log("보스 2페이즈: 미사일 공격 시작");
            if (phase2Boss != null && phase2Boss.audioSource != null && phase2Boss.missileSound != null)
            {
                phase2Boss.audioSource.PlayOneShot(phase2Boss.missileSound);
            }
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // 중간 타이밍에 미사일 발사
            if (!hasFiredMissile && attackTimer >= 1.5f)
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
            Debug.Log("보스 2페이즈: 미사일 공격 종료");
            phase2Boss.isAttacking = false;
            phase2Boss.ResetAttackFlags();
        }
    }

    // Attack3State: 근접 공격
    public class Attack3State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 1.5f;
        private bool hasAttacked = false;
        private BossPhase2 phase2Boss;

        public Attack3State(Boss boss) : base(boss)
        {
            phase2Boss = boss as BossPhase2;
        }

        public override void Enter()
        {
            attackTimer = 0f;
            hasAttacked = false;
            boss.animator.ResetTrigger("Damage");
            boss.animator.SetTrigger("Attack"); 
            Debug.Log("보스 2페이즈: 근접 공격 시작");

            if (phase2Boss != null && phase2Boss.audioSource != null && phase2Boss.closeCombatSound != null)
            {
                phase2Boss.audioSource.PlayOneShot(phase2Boss.closeCombatSound);
            }
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // 공격 모션 중간 타이밍에 실제 타격 판정
            if (!hasAttacked && attackTimer >= 0.7f)
            {
                phase2Boss.CloseCombatAttack();
                hasAttacked = true;
            }

            // 공격 종료 후 Idle
            if (attackTimer >= attackDuration)
            {
                phase2Boss.SetPostAttackDelay(true);
                boss.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            Debug.Log("보스 2페이즈: 근접 공격 종료");
            phase2Boss.isAttacking = false;
            phase2Boss.ResetAttackFlags();
        }
    }

    // TeleportState: 순간이동 (투명도 점진적 변화 예시)
    public class TeleportState : BossState
    {
        private BossPhase2 phase2Boss;
        private Renderer[] renderers;
        private float fadeOutDuration = 0.5f;
        private float fadeInDuration = 0.5f;
        private float fadeTimer = 0f;
        private bool hasTeleported = false;
        private float teleportDistance = 3f;

        public TeleportState(Boss boss) : base(boss)
        {
            phase2Boss = boss as BossPhase2;
        }

        public override void Enter()
        {
            renderers = phase2Boss.GetComponentsInChildren<Renderer>();
            // 순간이동 초기화
            fadeTimer = 0f;
            hasTeleported = false;
            Debug.Log("보스: 순간이동 시작 (FadeOut)");
        }

        public override void Update()
        {
            fadeTimer += Time.deltaTime;

            // 1) FadeOut 구간
            if (!hasTeleported && fadeTimer <= fadeOutDuration)
            {
                float t = fadeTimer / fadeOutDuration;
                SetOpacity(1f - t);
                return;
            }

            // 2) 아직 순간이동 안 했다면: 위치 이동 후 fadeTimer 리셋
            if (!hasTeleported)
            {
                DoTeleport();
                hasTeleported = true;
                fadeTimer = 0f;
                Debug.Log("보스: 순간이동 완료 → FadeIn 시작");
                return;
            }

            // 3) FadeIn 구간
            if (hasTeleported && fadeTimer <= fadeInDuration)
            {
                float t = fadeTimer / fadeInDuration;
                SetOpacity(t);
            }
            else
            {
                // 페이드인 끝나면 Idle로
                boss.TransitionToIdle();
            }
        }

        private void DoTeleport()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            Vector3 direction = (player.transform.position - phase2Boss.transform.position).normalized;
            direction.y = 0;

            Vector3 teleportPosition = player.transform.position - direction * teleportDistance;
            teleportPosition.y = 1.5f;
            phase2Boss.transform.position = teleportPosition;
            Debug.Log("보스 순간이동 완료");
        }

        private void SetOpacity(float alpha)
        {
            if (renderers == null) return;
            foreach (Renderer r in renderers)
            {
                foreach (Material mat in r.materials)
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
    }

    // DeathState: 사망 처리
    public class DeathState : BossState
    {
        private float deathTimer = 0f;
        // 필요하다면 애니메이션 시간 정도
        private float deathDuration = 2f;

        public DeathState(Boss boss) : base(boss) { }

        public override void Enter()
        {
            if (boss.animator != null) boss.animator.SetTrigger("Death");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Victory);
            }
            else
            {
                Debug.LogWarning("GameManager를 찾을 수 없습니다.");
            }
        }

    }
    #endregion
}
