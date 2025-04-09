using UnityEngine;
using UnityEngine.UI;

namespace Azmodan.Phase1
{
    public class BossPhase1 : Boss
    {
        [Header("Phase 1 Settings")] [SerializeField]
        private float attackDelay = 2f; // 공격 후 대기 시간

        [SerializeField] private float attack1Distance = 2f; // 근접 공격 거리
        [SerializeField] private float attack2Distance = 5f; // 원거리 공격 거리
        [SerializeField] private float preAttackDelay = 0.5f; // 공격 전 딜레이 시간

        [Header("Projectile Settings")] [SerializeField]
        private GameObject projectilePrefab; // 투사체 프리팹

        [SerializeField] private float projectileSpeed = 10f; // 투사체 속도
        [SerializeField] private float projectileOffset = 1.5f; // 보스 앞에서 얼마나 떨어진 위치에서 발사할지
        [SerializeField] private float projectileHeight = 7.75f; // 투사체 발사 높이
        public static GameManager Instance { get; private set; }

        private IState previousState; // 스턴 전 상태 저장용
        private int randomAttackNum; // 랜덤 공격 번호 (1-100)
        private BossStateType selectedAttackType;

        // 공격 로직 디버깅을 위한 변수 추가
        private bool attackSelected = false;
        private bool attackInitiated = false;
        
        [Header("Attack Effects")]
        [SerializeField]
        private GameObject attack1EffectPrefab; // 공격1 이펙트 프리팹
        [SerializeField] private Vector3 attack1EffectOffset = new Vector3(-1.56f, 3.6f, -8.4f); // 이펙트 위치 오프셋

        public GameObject GetAttack1EffectPrefab()
        {
            return attack1EffectPrefab;
        }

        public Vector3 GetAttack1EffectOffset()
        {
            return attack1EffectOffset;
        }
        
        public bool IsAttackSelected()
        {
            return attackSelected;
        }

        public bool IsAttackInitiated()
        {
            return attackInitiated;
        }

        // UI 연결
        [SerializeField] private Slider healthBarUI;

        // Phase1 전용 오디오 설정 추가
        [Header("Phase 1 Audio")] [SerializeField]
        private AudioSource walkAudioSource; // 걷기 전용 오디오 소스

        [SerializeField] private AudioSource attackAudioSource; // 공격 전용 오디오 소스
        [SerializeField] private AudioClip walkSound;
        [SerializeField] private AudioClip attack1Sound;
        [SerializeField] private AudioClip attack2Sound;
        [SerializeField] private AudioClip takeDamageSound;
        [SerializeField] private AudioClip deathSound;

        protected override void Start()
        {
            base.Start();

            // 초기 체력 로그 출력
            Debug.Log($"보스 초기 체력: {health}");

            // 초기 체력바 설정
            if (healthBarUI != null)
            {
                healthBarUI.maxValue = health;
                healthBarUI.value = health;
            }

            // 걷기용 AudioSource 초기화 (없는 경우)
            if (walkAudioSource == null)
            {
                walkAudioSource = gameObject.AddComponent<AudioSource>();
                walkAudioSource.loop = true;
                walkAudioSource.playOnAwake = false;
            }

            // 공격용 AudioSource 초기화 (없는 경우)
            if (attackAudioSource == null)
            {
                attackAudioSource = gameObject.AddComponent<AudioSource>();
                attackAudioSource.loop = false; // 공격 소리는 루프하지 않음
                attackAudioSource.playOnAwake = false;
            }
        }

        public void PlayWalkSound()
        {
            if (walkAudioSource != null && walkSound != null)
            {
                walkAudioSource.clip = walkSound;
                if (!walkAudioSource.isPlaying)
                {
                    walkAudioSource.Play();
                    // Debug.Log("보스 Phase1: 걷기 소리 재생 시작");
                }
            }
        }

        public void StopWalkSound()
        {
            if (walkAudioSource != null && walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
                // Debug.Log("보스 Phase1: 걷기 소리 정지");
            }
        }

        public void PlayAttackSound(int attackNum)
        {
            if (attackAudioSource != null)
            {
                switch (attackNum)
                {
                    case 1:
                        if (attack1Sound != null)
                        {
                            attackAudioSource.clip = attack1Sound;
                            attackAudioSource.Play();
                        }

                        break;
                    case 2:
                        if (attack2Sound != null)
                        {
                            attackAudioSource.clip = attack2Sound;
                            attackAudioSource.Play();
                        }

                        break;
                    case 3:
                        if (takeDamageSound != null)
                        {
                            attackAudioSource.clip = takeDamageSound;
                            attackAudioSource.Play();
                        }

                        break;
                    case 4:
                        if (deathSound != null)
                        {
                            attackAudioSource.clip = deathSound;
                            attackAudioSource.Play();
                        }

                        break;
                }
            }
        }

        public BossStateType GetSelectedAttackType()
        {
            return selectedAttackType;
        }

        public float GetAttack1Distance()
        {
            return attack1Distance;
        }

        public float GetAttack2Distance()
        {
            return attack2Distance;
        }

        public IState GetPreviousState()
        {
            return previousState;
        }

        protected override void InitializeStates()
        {
            states[typeof(IdleState)] = new Phase1IdleState(this);
            states[typeof(WalkState)] = new Phase1WalkState(this);
            states[typeof(Attack1State)] = new Attack1State(this);
            states[typeof(Attack2State)] = new Attack2State(this);
            states[typeof(StunState)] = new StunState(this);
            states[typeof(DeathState)] = new DeathState(this);
        }

        // 공격 대기 상태 설정/확인 메서드
        public void SetWaitingForAttack(bool isWaiting)
        {
            if (isWaiting)
            {
                SetSubState(BossSubState.PreAttackDelay, preAttackDelay);
                // Debug.Log("보스: 공격 대기(선딜레이) 상태 설정됨");
            }
            else
            {
                // 디버깅 메시지 추가
                if (IsInSubState(BossSubState.PreAttackDelay))
                {
                    // Debug.Log("보스: 공격 대기(선딜레이) 상태 해제됨");
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
                // Debug.Log("보스: 공격 후 딜레이 상태 설정됨");

                // 공격 플래그 초기화
                attackSelected = false;
                attackInitiated = false;
            }
            else
            {
                // 디버깅 메시지 추가
                if (IsInSubState(BossSubState.PostAttackDelay))
                {
                    // Debug.Log("보스: 공격 후 딜레이 상태 해제됨");
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

        public void SelectAttackType()
        {
            // 이미 공격이 선택된 상태라면 중복 실행 방지
            if (attackSelected)
            {
                // Debug.Log("보스: 이미 공격이 선택되어 있습니다.");
                return;
            }

            randomAttackNum = Random.Range(1, 101);

            if (randomAttackNum >= 1 && randomAttackNum <= 30)
            {
                // 근접 공격 선택 및 저장
                attackDistance = attack1Distance;
                selectedAttackType = BossStateType.Attack1;
                // Debug.Log("보스: 근접 공격 선택됨, 거리: " + attackDistance);
            }
            else if (randomAttackNum >= 71 && randomAttackNum <= 100)
            {
                // 원거리 공격 선택 및 저장
                attackDistance = attack2Distance;
                selectedAttackType = BossStateType.Attack2;
                // Debug.Log("보스: 원거리 공격 선택됨, 거리: " + attackDistance);
            }
            else
            {
                // 그 외의 경우 랜덤하게 선택 (공격 실패 방지)
                if (Random.value < 0.5f)
                {
                    attackDistance = attack1Distance;
                    selectedAttackType = BossStateType.Attack1;
                    // Debug.Log("보스: 기본 근접 공격 선택됨");
                }
                else
                {
                    attackDistance = attack2Distance;
                    selectedAttackType = BossStateType.Attack2;
                    // Debug.Log("보스: 기본 원거리 공격 선택됨");
                }
            }

            // 공격 선택 플래그 설정
            attackSelected = true;

            // 선딜레이를 위한 Idle 상태로 전환
            SetWaitingForAttack(true);
            TransitionToIdle();
        }

        // 1페이즈 전용 공격 상태로 전환
        public override void TransitionToAttack()
        {
            // 공격 시작 플래그 설정 - 디버깅 및 중복 호출 방지용
            attackInitiated = true;

            // 모든 애니메이터 파라미터 초기화 (확실히 초기화)
            ResetAllAnimatorParameters();
            
            // Debug.Log($"보스: TransitionToAttack 호출됨 - 선택된 공격 타입: {selectedAttackType}");

            // 플레이어와의 거리 확인
            float distanceToPlayer = 0;
            if (targetPlayer != null)
            {
                Vector3 direction = targetPlayer.transform.position - transform.position;
                direction.y = 0; // Y축 무시
                distanceToPlayer = direction.magnitude;
                // Debug.Log($"보스: 플레이어와의 거리: {distanceToPlayer}, 공격 거리: {attackDistance}");
            }

            // 공격 거리 확인 - 거리를 크게 벗어났을 때만 Walk 상태로 전환
            // 약간의 여유를 두어 작은 움직임에는 반응하지 않도록 수정
            if (selectedAttackType == BossStateType.Attack1 && distanceToPlayer > attack1Distance * 1.5f ||
                selectedAttackType == BossStateType.Attack2 && distanceToPlayer > attack2Distance * 1.5f)
            {
                // Debug.Log("보스: 공격 거리에서 크게 벗어남, 다시 추적");
                attackInitiated = false; // 공격 시도 실패 플래그 초기화
                TransitionToWalk();
                return;
            }

            // 플레이어가 보스의 앞쪽 90도 범위 안에 있는지 확인
            if (!IsPlayerInAttackAngle())
            {
                // Debug.Log("보스: 플레이어가 공격 각도 범위(90도) 밖에 있음, 다시 추적");
                attackInitiated = false; // 공격 시도 실패 플래그 초기화
                TransitionToWalk();
                return;
            }

            // 공격 거리 내에 있으면 선택된 공격 실행
            currentStateType = selectedAttackType;

            // 현재 공격 중인지 명확히 기록
            // Debug.Log($"보스: {selectedAttackType} 상태로 명시적 전환 시작");

            // 공격 상태로 전환 완료 후
            if (selectedAttackType == BossStateType.Attack1)
            {
                ChangeState<Attack1State>();
            }
            else // Attack2
            {
                ChangeState<Attack2State>();
            }
        }

        public override void TakeDamage(int damage)
        {
            // 이미 사망했다면 데미지 처리하지 않음
            if (isDead)
            {
                // Debug.Log("보스: 이미 사망 상태입니다. 데미지 무시.");
                return;
            }

            // 데미지 적용
            health -= damage;
            PlayAttackSound(3);
            Debug.Log($"보스 1페이즈: {damage} 데미지 받음 (현재 체력: {health})");

            // UI 업데이트
            if (healthBarUI != null)
            {
                healthBarUI.value = health;
            }

            // 사망 처리
            if (health <= 0)
            {
                // 사망 플래그 설정 (중복 호출 방지)
                isDead = true;
                Debug.Log("보스 1페이즈: 사망 처리 시작");

                // 사망 상태로 전환
                Die();
                return; // 이후 코드 실행 방지
            }

            // 여기서부터는 health > 0인 경우에만 실행됨
            // Idle 또는 Walk 상태일 때만 스턴 상태로 전환
            if (currentState is IdleState || currentState is WalkState)
            {
                previousState = currentState; // 현재 상태 저장
                TransitionToStun();
            }
        }

        // 사망 처리 재정의
        protected override void Die()
        {
            // 이미 죽음 처리 중이거나 사망 상태면 중복 실행 방지
            if (isDead && deathAnimationTriggered)
            {
                // Debug.Log("보스: 이미 사망 상태입니다. 중복 사망 처리 무시.");
                return;
            }

            PlayAttackSound(4);

            // isDead 플래그만 설정하고 (deathAnimationTriggered는 TransitionToDeath에서 설정)
            isDead = true;
            

            // 모든 진행 중인 SubState 초기화
            foreach (BossSubState state in System.Enum.GetValues(typeof(BossSubState)))
            {
                subStateTimers[state] = 0f;
                subStateDurations[state] = 0f;
            }

            // 죽음 애니메이션 재생 후 Phase2로 전환을 위해 딜레이 설정
            Invoke("NotifyBossManager", 2.0f);

            // 사망 상태로 전환 (여기서 deathAnimationTriggered 설정됨)
            TransitionToDeath();
        }

        private void NotifyBossManager()
        {
            if (GameManager.Instance != null)
            {
                Debug.Log("보스 1페이즈: BossManager에 Phase2 전환 신호 보냄");
                GameManager.Instance.TransitionToPhase2();
            }
            else
            {
                Debug.LogError("보스 1페이즈: BossManager.Instance를 찾을 수 없음!");
            }
        }

        // StunState를 Override하여 이전 상태로 돌아가는 기능 구현
        public override void TransitionToStun()
        {
            // 현재 상태 저장 (Idle 또는 Walk 상태일 때만)
            if (currentState is IdleState || currentState is WalkState)
            {
                previousState = currentState;
            }

            // 스턴 상태로 전환
            base.TransitionToStun();
        }

        // 투사체 발사 메서드
        public void FireProjectile()
        {
            if (projectilePrefab != null && targetPlayer != null)
            {
                // 보스의 앞쪽 위치 계산 (보스의 현재 XZ 위치에 특정 높이 적용)
                Vector3 spawnPosition = new Vector3(
                    transform.position.x + transform.forward.x * projectileOffset,
                    projectileHeight, // 지정된 높이 사용
                    transform.position.z + transform.forward.z * projectileOffset
                );

                // 플레이어 방향 계산
                Vector3 directionToPlayer = targetPlayer.transform.position - spawnPosition;
                directionToPlayer.Normalize();

                // 투사체 생성 (spawnPosition 위치에 플레이어 방향을 향하는 회전값으로)
                Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer);
                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, rotationToPlayer);

                // Rigidbody 설정
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 플레이어 방향으로 투사체 발사
                    rb.linearVelocity = directionToPlayer * projectileSpeed;
                }
                else
                {
                    Debug.LogWarning("투사체에 Rigidbody 컴포넌트가 없습니다!");
                }
            }
            else
            {
                Debug.LogWarning("투사체 프리팹이 설정되지 않았거나 타겟 플레이어가 없습니다!");
            }
        }

        // 공격 후 대기 시간 Getter
        public float GetAttackDelay()
        {
            return attackDelay;
        }

        // 플레이어가 보스의 앞쪽 90도 범위 안에 있는지 확인하는 메서드
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
    }

    // 1페이즈 전용 근접 공격 상태
    public class Attack1State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 2f;
        private bool hasDealtDamage = false;
        private bool hasSoundPlayed = false; // 소리 재생 여부 추적을 위한 변수
        private bool hasSpawnedEffect = false;
        private BossPhase1 phase1Boss;

        public Attack1State(Boss boss) : base(boss)
        {
            phase1Boss = boss as BossPhase1;
        }

        public override void Enter()
        {
            // 타이머 초기화
            attackTimer = 0f;
            hasDealtDamage = false;
            hasSoundPlayed = false;
            hasSpawnedEffect = false;

            // 공격1 애니메이션 재생
            boss.animator.SetTrigger("Attack");
            // Debug.Log("보스: 공격1 상태 시작");
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // 공격 1초 시점에서 공격 소리 재생
            if (attackTimer >= 1.0f && !hasSoundPlayed)
            {
                // 공격 소리 재생
                phase1Boss.PlayAttackSound(1);
                hasSoundPlayed = true;
                // Debug.Log("보스: 공격1 소리 재생");
            }
            
            if (attackTimer >= 1.0f && !hasSpawnedEffect)
            {
                SpawnAttack1Effect();
                hasSpawnedEffect = true;
            }

            // 공격 중간 지점에서 데미지 적용 (애니메이션 이벤트로 대체 가능)
            if (attackTimer >= 1.0f && !hasDealtDamage)
            {
                // 공격 범위 내 플레이어 확인 및 데미지 적용
                // ApplyDamageToPlayer();
                hasDealtDamage = true;
            }

            // 공격 애니메이션이 끝나면 상태 전환
            if (attackTimer >= attackDuration)
            {
                // 공격 후 딜레이를 위한 Idle 상태로 전환
                phase1Boss.SetPostAttackDelay(true); // 공격 후 딜레이 상태 설정
                boss.TransitionToIdle();
            }
        }
        
        private void SpawnAttack1Effect()
        {
            if (phase1Boss.GetAttack1EffectPrefab() != null)
            {
                // 보스의 world position을 기준으로 정확한 월드 좌표 (-1.56, 3.6, -8.4)에 이펙트 생성
                // 오프셋을 직접 적용하지 않고 월드 좌표로 변환하여 적용
                Vector3 effectWorldPosition = phase1Boss.transform.TransformPoint(phase1Boss.GetAttack1EffectOffset());
        
                // 지정된 위치에 이펙트 생성 (보스의 회전을 고려)
                GameObject effect = GameObject.Instantiate(
                    phase1Boss.GetAttack1EffectPrefab(), 
                    effectWorldPosition,
                    phase1Boss.transform.rotation  // 보스의 회전값을 적용
                );
        
                // 일정 시간 후 이펙트 제거 (3초 후 제거, 필요에 따라 조정)
                GameObject.Destroy(effect, 3f);
        
                // Debug.Log("보스: 공격1 이펙트 생성됨 - 위치: " + effectWorldPosition);
            }
        }

        private void ApplyDamageToPlayer()
        {
            if (boss.targetPlayer == null) return;

            // 플레이어와의 거리 확인
            float distance = Vector3.Distance(boss.transform.position, boss.targetPlayer.transform.position);

            // 공격 범위 내에 있으면 데미지 적용
            if (distance <= 2.5f) // 약간의 여유를 두고
            {
                // 플레이어에게 데미지 적용
                SamplePlayer player = boss.targetPlayer.GetComponent<SamplePlayer>();
                if (player != null)
                {
                    player.TakeDamage(20); // 데미지 양은 조절 가능
                    Debug.Log($"보스가 플레이어에게 근접 공격으로 20 데미지를 입혔습니다!");
                }
            }
        }

        public override void Exit()
        {
            // Debug.Log("보스: 공격1 상태 종료");
        }
    }

    // 1페이즈 전용 원거리 공격 상태
    public class Attack2State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 3f;
        private bool hasProjectileFired = false;
        private bool hasSoundPlayed = false; // 소리 재생 여부 추적을 위한 변수
        private BossPhase1 phase1Boss;

        public Attack2State(Boss boss) : base(boss)
        {
            phase1Boss = boss as BossPhase1;
        }

        public override void Enter()
        {
            // 타이머 초기화
            attackTimer = 0f;
            hasProjectileFired = false;
            hasSoundPlayed = false;

            // 공격2 애니메이션 재생
            boss.animator.SetTrigger("Attack");
            // Debug.Log("보스: 공격2 상태 시작");
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // 공격 1초 시점에서 공격 소리 재생
            if (attackTimer >= 1.0f && !hasSoundPlayed)
            {
                // 공격 소리 재생
                phase1Boss.PlayAttackSound(2);
                hasSoundPlayed = true;
                // Debug.Log("보스: 공격2 소리 재생");
            }

            // 공격 중간 지점에서 투사체 발사 (애니메이션 이벤트로 대체 가능)
            if (attackTimer >= 1.5f && !hasProjectileFired)
            {
                // 투사체 발사
                phase1Boss.FireProjectile();
                hasProjectileFired = true;
            }

            // 공격 애니메이션이 끝나면 상태 전환
            if (attackTimer >= attackDuration)
            {
                // 공격 후 딜레이를 위한 Idle 상태로 전환
                phase1Boss.SetPostAttackDelay(true); // 공격 후 딜레이 상태 설정
                boss.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            // Debug.Log("보스: 공격2 상태 종료");
        }
    }

    // 페이즈 1 전용 Idle 상태
    public class Phase1IdleState : IdleState
    {
        private BossPhase1 phase1Boss;
        private float lastDistanceCheck = 0f;
        private float distanceCheckInterval = 0.1f; // 0.1초마다 거리 체크

        public Phase1IdleState(BossPhase1 boss) : base(boss)
        {
            this.phase1Boss = boss;
        }

        public override void Enter()
        {
            // 타이머 초기화
            idleTimer = 0f;
            boss.animator.SetTrigger("Idle");

            // 추가 로깅 - Idle 상태 진입 시 현재 상황 표시
            // string currentStatus = "일반";
            // if (phase1Boss.IsWaitingForAttack()) currentStatus = "공격 대기";
            // if (phase1Boss.IsInPostAttackDelay()) currentStatus = "공격 후 딜레이";
            // Debug.Log($"보스: Idle 상태 시작 (상태: {currentStatus})");
        }

        protected override void HandleIdle()
        {
            // 타이머 증가
            idleTimer += Time.deltaTime;

            // 공격 대기 중인 경우 (선딜레이)
            if (phase1Boss.IsWaitingForAttack())
            {
                // 주기적으로 플레이어 위치 업데이트
                lastDistanceCheck += Time.deltaTime;
                if (lastDistanceCheck >= distanceCheckInterval)
                {
                    lastDistanceCheck = 0f;

                    // 공격 범위와 각도를 체크
                    if (boss.targetPlayer != null)
                    {
                        Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
                        direction.y = 0;
                        float distanceToPlayer = direction.magnitude;

                        // 플레이어가 공격 거리를 크게 벗어난 경우
                        if ((phase1Boss.GetSelectedAttackType() == BossStateType.Attack1 &&
                             distanceToPlayer > phase1Boss.GetAttack1Distance() * 1.5f) ||
                            (phase1Boss.GetSelectedAttackType() == BossStateType.Attack2 &&
                             distanceToPlayer > phase1Boss.GetAttack2Distance() * 1.5f))
                        {
                            // 공격 대기 취소하고 다시 추적
                            phase1Boss.SetWaitingForAttack(false);
                            // Debug.Log($"보스: 플레이어가 공격 범위 벗어남 (거리: {distanceToPlayer}), 추적 재개");
                            boss.TransitionToWalk();
                            return;
                        }

                        // 플레이어가 각도 범위를 벗어난 경우
                        if (!phase1Boss.IsPlayerInAttackAngle())
                        {
                            // 각도가 많이 벗어났으면 공격 대기 취소하고 다시 추적
                            Vector3 bossForward = boss.transform.forward;
                            bossForward.y = 0;
                            direction.Normalize();
                            float dotProduct = Vector3.Dot(bossForward, direction);
                            float angleToPlayer = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

                            // 60도 이상 벗어난 경우에만 Walk로 전환 (약간의 여유를 둠)
                            if (angleToPlayer > 60f)
                            {
                                phase1Boss.SetWaitingForAttack(false);
                                // Debug.Log($"보스: 플레이어가 공격 각도 크게 벗어남 (각도: {angleToPlayer}°), 추적 재개");
                                boss.TransitionToWalk();
                                return;
                            }
                        }

                        // 플레이어 방향으로 부드럽게 회전 (항상 수행)
                        direction.Normalize();
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        boss.transform.rotation = Quaternion.Slerp(
                            boss.transform.rotation,
                            targetRotation,
                            boss.rotateSpeed * Time.deltaTime * 0.5f
                        );
                    }
                }

                // 선딜레이 시간 완료 확인
                if (idleTimer >= phase1Boss.GetPreAttackDelay())
                {
                    // 디버깅용 로그 추가
                    // Debug.Log($"보스: 선딜레이 완료 ({idleTimer}초 경과), 공격 상태로 전환");
                    
                    // 공격 플래그 설정 - 상태 전환 명확히
                    phase1Boss.SetWaitingForAttack(false);

                    // 직접 공격 상태로 전환
                    boss.TransitionToAttack();
                    return;
                }

                return;
            }

            // 공격 후 딜레이 중인 경우
            if (phase1Boss.IsInPostAttackDelay())
            {
                if (idleTimer >= phase1Boss.GetAttackDelay())
                {
                    // 후딜레이 완료 후 Walk 상태로 전환
                    // Debug.Log($"보스: 공격 후 딜레이 완료 ({idleTimer}초 경과), Walk 상태로 전환");
                    phase1Boss.SetPostAttackDelay(false);
                    boss.TransitionToWalk();
                }

                return;
            }

            // 일반 Idle 상태인 경우 (대기 시간 후 Walk 상태로 전환)
            if (idleTimer >= boss.idleDuration)
            {
                // 플레이어 찾기 (Player 태그를 가진 게임오브젝트)
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    // 찾은 플레이어를 타겟으로 설정
                    boss.targetPlayer = player;
                    // Debug.Log("보스: 대기 시간 완료, 플레이어 발견하여 추적 시작");

                    // Walk 상태로 전환
                    boss.TransitionToWalk();
                }
                else
                {
                    Debug.LogWarning("보스: 플레이어를 찾을 수 없습니다.");
                }
            }
        }
    }


    // 페이즈 1 전용 Walk 상태
    public class Phase1WalkState : WalkState
    {
        private BossPhase1 phase1Boss;

        public Phase1WalkState(BossPhase1 boss) : base(boss)
        {
            this.phase1Boss = boss;
        }

        public override void Enter()
        {
            // Walk 애니메이션 재생
            boss.animator.SetTrigger("Walk");

            // Phase1 전용 걷기 소리 재생
            phase1Boss.PlayWalkSound();
        }

        public override void Exit()
        {
            phase1Boss.StopWalkSound();
        }

        protected override void HandleWalk()
        {
            // 플레이어 방향으로 이동 벡터 계산
            Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
            direction.y = 0; // Y축 이동 방지

            // 플레이어와의 거리 계산
            float distanceToPlayer = direction.magnitude;

            // 방향 벡터 정규화 (회전 및 이동에 사용)
            direction.Normalize();

            // 플레이어와의 거리가 공격 거리보다 크면 계속 이동
            if (distanceToPlayer > boss.attackDistance)
            {
                // 이동 처리
                boss.transform.position += direction * boss.moveSpeed * Time.deltaTime;

                // 플레이어 방향으로 부드럽게 회전
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                boss.transform.rotation = Quaternion.Slerp(
                    boss.transform.rotation,
                    targetRotation,
                    boss.rotateSpeed * Time.deltaTime
                );
            }
            else
            {
                // Debug.Log($"보스: 공격 거리에 도달함 (현재: {distanceToPlayer}m, 공격 거리: {boss.attackDistance}m)");

                // 플레이어가 보스의 앞쪽 90도 범위 안에 있는지 확인
                if (!phase1Boss.IsPlayerInAttackAngle())
                {
                    // 각도가 맞지 않으면 플레이어 방향으로 회전만 계속함
                    // Debug.Log("보스: 플레이어가 공격 각도 범위 밖에 있음, 회전 중");

                    // 플레이어 방향으로 부드럽게 회전 (이동은 하지 않음)
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    boss.transform.rotation = Quaternion.Slerp(
                        boss.transform.rotation,
                        targetRotation,
                        boss.rotateSpeed * Time.deltaTime * 1.5f // 약간 빠르게 회전
                    );

                    return; // 다른 공격 로직은 수행하지 않음
                }

                if (!phase1Boss.IsAttackSelected())
                {
                    // 공격이 아직 선택되지 않았으면 선택
                    phase1Boss.SelectAttackType();
                }
                else if (!phase1Boss.IsWaitingForAttack() && !phase1Boss.IsAttackInitiated())
                {
                    // 공격이 선택됐지만 선딜레이 중이 아니고 아직 공격이 시작되지 않았다면
                    // 직접 공격 상태로 전환
                    // Debug.Log("보스: 선택된 공격을 시작합니다.");
                    boss.TransitionToAttack();
                }
            }
        }
    }
}