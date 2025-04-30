using UnityEngine;
using UnityEngine.UI;

namespace Azmodan.Phase1
{
    public class BossPhase1 : Boss
    {
        [Header("Phase 1 Settings")] [SerializeField]
        private float attackDelay = 2f; // Delay time after attack

        [SerializeField] private float attack1Distance = 2f; // Melee attack distance
        [SerializeField] private float attack2Distance = 5f; // Ranged attack distance
        [SerializeField] private float preAttackDelay = 0.5f; // Delay time before attack

        [Header("Projectile Settings")] [SerializeField]
        private GameObject projectilePrefab; // Projectile prefab

        [SerializeField] private float projectileSpeed = 10f; // Projectile speed
        [SerializeField] private float projectileOffset = 1.5f; // How far from boss position to spawn projectile
        [SerializeField] private float projectileHeight = 7.75f; // Projectile spawn height

        private IState previousState; // Store state before stun
        private int randomAttackNum; // Random attack number (1-100)
        private BossStateType selectedAttackType;

        // Variables for attack logic debugging
        private bool attackSelected = false;
        private bool attackInitiated = false;
        
        [Header("Attack Effects")]
        [SerializeField]
        private GameObject attack1EffectPrefab; // Attack1 effect prefab
        [SerializeField] private Vector3 attack1EffectOffset = new Vector3(-1.56f, 3.6f, -8.4f); // Effect position offset

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

        // UI connection
        [SerializeField] private Slider healthBarUI;

        // Phase1 specific audio settings
        [Header("Phase 1 Audio")] [SerializeField]
        private AudioSource walkAudioSource; // Audio source for walking

        [SerializeField] private AudioSource attackAudioSource; // Audio source for attacks
        [SerializeField] private AudioClip walkSound;
        [SerializeField] private AudioClip attack1Sound;
        [SerializeField] private AudioClip attack2Sound;
        [SerializeField] private AudioClip takeDamageSound;
        [SerializeField] private AudioClip deathSound;

        protected override void Start()
        {
            base.Start();

            // Log initial health
            Debug.Log($"Boss initial health: {health}");
            
            // Set initial health bar
            if (healthBarUI)
            {
                healthBarUI.maxValue = health;
                healthBarUI.value = health;
            }

            // Initialize walk AudioSource (if missing)
            if (!walkAudioSource)
            {
                walkAudioSource = gameObject.AddComponent<AudioSource>();
                walkAudioSource.loop = true;
                walkAudioSource.playOnAwake = false;
            }

            // Initialize attack AudioSource (if missing)
            if (!attackAudioSource)
            {
                attackAudioSource = gameObject.AddComponent<AudioSource>();
                attackAudioSource.loop = false; // Attack sound should not loop
                attackAudioSource.playOnAwake = false;
            }
        }

        public void PlayWalkSound()
        {
            if (walkAudioSource && walkSound)
            {
                walkAudioSource.clip = walkSound;
                if (!walkAudioSource.isPlaying)
                {
                    walkAudioSource.Play();
                    // Debug.Log("Boss Phase1: Started playing walk sound");
                }
            }
        }

        public void StopWalkSound()
        {
            if (walkAudioSource && walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
                // Debug.Log("Boss Phase1: Stopped walk sound");
            }
        }

        public void PlayAttackSound(int attackNum)
        {
            if (attackAudioSource)
            {
                switch (attackNum)
                {
                    case 1:
                        if (attack1Sound)
                        {
                            attackAudioSource.clip = attack1Sound;
                            attackAudioSource.Play();
                        }

                        break;
                    case 2:
                        if (attack2Sound)
                        {
                            attackAudioSource.clip = attack2Sound;
                            attackAudioSource.Play();
                        }

                        break;
                    case 3:
                        if (takeDamageSound)
                        {
                            attackAudioSource.clip = takeDamageSound;
                            attackAudioSource.Play();
                        }

                        break;
                    case 4:
                        if (deathSound)
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

        // Methods to set/check pre-attack waiting state
        public void SetWaitingForAttack(bool isWaiting)
        {
            if (isWaiting)
            {
                SetSubState(BossSubState.PreAttackDelay, preAttackDelay);
                // Debug.Log("Boss: Pre-attack delay state set");
            }
            else
            {
                if (IsInSubState(BossSubState.PreAttackDelay))
                {
                    // Debug.Log("Boss: Pre-attack delay state cleared");
                }
            }
        }

        public bool IsWaitingForAttack()
        {
            return IsInSubState(BossSubState.PreAttackDelay);
        }

        // Methods to set/check post-attack delay state
        public void SetPostAttackDelay(bool isDelaying)
        {
            if (isDelaying)
            {
                SetSubState(BossSubState.PostAttackDelay, attackDelay);
                // Debug.Log("Boss: Post-attack delay state set");

                // Reset attack flags
                attackSelected = false;
                attackInitiated = false;
            }
            else
            {
                if (IsInSubState(BossSubState.PostAttackDelay))
                {
                    // Debug.Log("Boss: Post-attack delay state cleared");
                }
            }
        }

        public bool IsInPostAttackDelay()
        {
            return IsInSubState(BossSubState.PostAttackDelay);
        }

        // Get pre-attack delay time
        public float GetPreAttackDelay()
        {
            return preAttackDelay;
        }

        public void SelectAttackType()
        {
            // Prevent duplicate execution if attack is already selected
            if (attackSelected)
            {
                // Debug.Log("Boss: Attack already selected.");
                return;
            }

            randomAttackNum = Random.Range(1, 101);

            if (randomAttackNum >= 1 && randomAttackNum <= 30)
            {
                // Select and store melee attack
                attackDistance = attack1Distance;
                selectedAttackType = BossStateType.Attack1;
                // Debug.Log("Boss: Melee attack selected, distance: " + attackDistance);
            }
            else if (randomAttackNum >= 71 && randomAttackNum <= 100)
            {
                // Select and store ranged attack
                attackDistance = attack2Distance;
                selectedAttackType = BossStateType.Attack2;
                // Debug.Log("Boss: Ranged attack selected, distance: " + attackDistance);
            }
            else
            {
                // For other cases, select randomly (prevent attack failure)
                if (Random.value < 0.5f)
                {
                    attackDistance = attack1Distance;
                    selectedAttackType = BossStateType.Attack1;
                    // Debug.Log("Boss: Default melee attack selected");
                }
                else
                {
                    attackDistance = attack2Distance;
                    selectedAttackType = BossStateType.Attack2;
                    // Debug.Log("Boss: Default ranged attack selected");
                }
            }

            // Set attack selection flag
            attackSelected = true;

            // Transition to Idle state for pre-delay
            SetWaitingForAttack(true);
            TransitionToIdle();
        }

        // Phase 1 specific attack state transition
        public override void TransitionToAttack()
        {
            // Set attack start flag - for debugging and prevent duplicate calls
            attackInitiated = true;

            // Reset all animator parameters (ensure clean initialization)
            ResetAllAnimatorParameters();
            
            // Debug.Log($"Boss: TransitionToAttack called - Selected attack type: {selectedAttackType}");

            // Check distance to player
            float distanceToPlayer = 0;
            if (targetPlayer)
            {
                Vector3 direction = targetPlayer.transform.position - transform.position;
                direction.y = 0; // Ignore Y-axis
                distanceToPlayer = direction.magnitude;
                // Debug.Log($"Boss: Distance to player: {distanceToPlayer}, Attack distance: {attackDistance}");
            }

            // Check attack distance - only transition to Walk if significantly out of range
            // Modified to not react to small movements
            if (selectedAttackType == BossStateType.Attack1 && distanceToPlayer > attack1Distance * 1.5f ||
                selectedAttackType == BossStateType.Attack2 && distanceToPlayer > attack2Distance * 1.5f)
            {
                // Debug.Log("Boss: Player is significantly out of attack range, resuming tracking");
                attackInitiated = false; // Reset attack attempt flag
                TransitionToWalk();
                return;
            }

            // Check if player is within the boss's front 90-degree angle
            if (!IsPlayerInAttackAngle())
            {
                // Debug.Log("Boss: Player is outside of attack angle range (90 degrees), resuming tracking");
                attackInitiated = false; // Reset attack attempt flag
                TransitionToWalk();
                return;
            }

            // If within attack range, execute the selected attack
            currentStateType = selectedAttackType;

            // Clearly record the current attack
            // Debug.Log($"Boss: Explicitly transitioning to {selectedAttackType} state");

            // After completing attack state transition
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
            // If already dead, don't process damage
            if (isDead)
            {
                // Debug.Log("Boss: Already in death state. Ignoring damage.");
                return;
            }

            // Apply damage
            health -= damage;
            PlayAttackSound(3);
            Debug.Log($"Boss Phase 1: Took {damage} damage (Current health: {health})");

            // Update UI
            if (healthBarUI)
            {
                healthBarUI.value = health;
            }

            // Handle death
            if (health <= 0)
            {
                // Set death flag (prevent duplicate calls)
                isDead = true;
                Debug.Log("Boss Phase 1: Starting death process");

                // Transition to death state
                Die();
                return; // Prevent further code execution
            }

            // Only execute if health > 0
            // Only transition to stun state if currently in Idle or Walk state
            if (currentState is IdleState || currentState is WalkState)
            {
                previousState = currentState; // Save current state
                TransitionToStun();
            }
        }

        // Override death handler
        protected override void Die()
        {
            // Prevent duplicate execution if already dead or in death state
            if (isDead && isDeathAnimTriggered)
            {
                // Debug.Log("Boss: Already in death state. Ignoring duplicate death processing.");
                return;
            }

            PlayAttackSound(4);

            // Only set isDead flag (isDeathAnimTriggered will be set in TransitionToDeath)
            isDead = true;
            

            // Reset all active SubStates
            foreach (BossSubState state in System.Enum.GetValues(typeof(BossSubState)))
            {
                subStateTimers[state] = 0f;
                subStateDurations[state] = 0f;
            }

            // Set delay for Phase2 transition after death animation
            Invoke("NotifyBossManager", 2.0f);

            // Transition to death state (isDeathAnimTriggered is set here)
            TransitionToDeath();
        }

        private void NotifyBossManager()
        {
            if (GameManager.Instance != null)
            {
                Debug.Log("Boss Phase 1: Sending Phase2 transition signal to BossManager");
                GameManager.Instance.TransitionToPhase2();
            }
            else
            {
                Debug.LogError("Boss Phase 1: Cannot find BossManager.Instance!");
            }
        }

        // Override StunState to implement returning to previous state
        public override void TransitionToStun()
        {
            // Save current state (only if in Idle or Walk state)
            if (currentState is IdleState || currentState is WalkState)
            {
                previousState = currentState;
            }

            // Transition to stun state
            base.TransitionToStun();
        }

        // Projectile firing method
        public void FireProjectile()
        {
            if (projectilePrefab != null && targetPlayer != null)
            {
                // Calculate boss's forward position (using current XZ position with specific height)
                Vector3 spawnPosition = new Vector3(
                    transform.position.x + transform.forward.x * projectileOffset,
                    projectileHeight, // Use specified height
                    transform.position.z + transform.forward.z * projectileOffset
                );

                // Calculate direction to player
                Vector3 directionToPlayer = targetPlayer.transform.position - spawnPosition;
                directionToPlayer.Normalize();

                // Create projectile (at spawnPosition with rotation toward player)
                Quaternion rotationToPlayer = Quaternion.LookRotation(directionToPlayer);
                GameObject projectile = Instantiate(projectilePrefab, spawnPosition, rotationToPlayer);

                // Configure Rigidbody
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb)
                {
                    // Remove gravity influence
                    rb.useGravity = false;
            
                    rb.linearVelocity = directionToPlayer * projectileSpeed;
                }
                else
                {
                    Debug.LogWarning("Projectile does not have a Rigidbody component!");
                }
            }
            else
            {
                Debug.LogWarning("Projectile prefab is not set or target player is missing!");
            }
        }

        // Getter for attack delay time
        public float GetAttackDelay()
        {
            return attackDelay;
        }

        // Method to check if player is within the boss's front 90-degree angle
        public bool IsPlayerInAttackAngle()
        {
            if (!targetPlayer) return false;

            // Direction vector from boss to player
            Vector3 directionToPlayer = targetPlayer.transform.position - transform.position;
            directionToPlayer.y = 0; // Ignore Y-axis (check only on horizontal plane)
            directionToPlayer.Normalize();

            // Boss's forward vector (front direction)
            Vector3 bossForward = transform.forward;
            bossForward.y = 0; // Ignore Y-axis
            bossForward.Normalize();

            // Calculate angle between the two vectors (using dot product)
            float dotProduct = Vector3.Dot(bossForward, directionToPlayer);

            // Convert dot product to angle (convert from radians to degrees)
            float angleToPlayer = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

            // Check if within 45 degrees (90-degree range = 45 degrees on each side)
            bool isInAngle = angleToPlayer <= 45f;

            if (!isInAngle)
            {
                // Debug.Log($"Boss: Player is outside the attack angle. (Angle: {angleToPlayer}°)");
            }

            return isInAngle;
        }
    }

    // Phase 1 specific melee attack state
    public class Attack1State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 2f;
        private bool hasDealtDamage = false;
        private bool hasSoundPlayed = false; // Variable to track if sound has played
        private bool hasSpawnedEffect = false;
        private BossPhase1 phase1Boss;

        public Attack1State(Boss boss) : base(boss)
        {
            phase1Boss = boss as BossPhase1;
        }

        public override void Enter()
        {
            // Initialize timers
            attackTimer = 0f;
            hasDealtDamage = false;
            hasSoundPlayed = false;
            hasSpawnedEffect = false;

            // Play attack1 animation
            boss.animator.SetTrigger("Attack");
            // Debug.Log("Boss: Attack1 state started");
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // Play attack sound at 1 second mark
            if (attackTimer >= 1.0f && !hasSoundPlayed)
            {
                // Play attack sound
                phase1Boss.PlayAttackSound(1);
                hasSoundPlayed = true;
                // Debug.Log("Boss: Playing Attack1 sound");
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