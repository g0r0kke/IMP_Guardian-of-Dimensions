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
        
        #region Attack
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
        
        // Projectile firing method
        public void FireProjectile()
        {
            if (projectilePrefab && targetPlayer)
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
        #endregion

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
            if (GameManager.Instance)
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
    
    // Phase 1 specific Idle state
    public class Phase1IdleState : IdleState
    {
        private BossPhase1 phase1Boss;
        private float lastDistanceCheck;
        private float distanceCheckInterval = 0.1f; // Check distance every 0.1 seconds

        public Phase1IdleState(BossPhase1 boss) : base(boss)
        {
            this.phase1Boss = boss;
        }

        public override void Enter()
        {
            // Initialize timer
            idleTimer = 0f;
            boss.animator.SetTrigger("Idle");

            // Additional logging - Show current situation when entering Idle state
            // string currentStatus = "Normal";
            // if (phase1Boss.IsWaitingForAttack()) currentStatus = "Pre-attack delay";
            // if (phase1Boss.IsInPostAttackDelay()) currentStatus = "Post-attack delay";
            // Debug.Log($"Boss: Idle state started (Status: {currentStatus})");
        }

        protected override void HandleIdle()
        {
            // Increment timer
            idleTimer += Time.deltaTime;

            // If in pre-attack waiting state
            if (phase1Boss.IsWaitingForAttack())
            {
                // Periodically update player position
                lastDistanceCheck += Time.deltaTime;
                if (lastDistanceCheck >= distanceCheckInterval)
                {
                    lastDistanceCheck = 0f;

                    // Check attack range and angle
                    if (boss.targetPlayer)
                    {
                        Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
                        direction.y = 0;
                        float distanceToPlayer = direction.magnitude;

                        // If player is significantly outside attack range
                        if ((phase1Boss.GetSelectedAttackType() == BossStateType.Attack1 &&
                             distanceToPlayer > phase1Boss.GetAttack1Distance() * 1.5f) ||
                            (phase1Boss.GetSelectedAttackType() == BossStateType.Attack2 &&
                             distanceToPlayer > phase1Boss.GetAttack2Distance() * 1.5f))
                        {
                            // Cancel attack waiting and resume tracking
                            phase1Boss.SetWaitingForAttack(false);
                            // Debug.Log($"Boss: Player moved out of attack range (Distance: {distanceToPlayer}), resuming tracking");
                            boss.TransitionToWalk();
                            return;
                        }

                        // If player is outside angle range
                        if (!phase1Boss.IsPlayerInAttackAngle())
                        {
                            // If angle is significantly off, cancel attack waiting and resume tracking
                            Vector3 bossForward = boss.transform.forward;
                            bossForward.y = 0;
                            direction.Normalize();
                            float dotProduct = Vector3.Dot(bossForward, direction);
                            float angleToPlayer = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f)) * Mathf.Rad2Deg;

                            // Only transition to Walk if angle is more than 60 degrees (allow some buffer)
                            if (angleToPlayer > 60f)
                            {
                                phase1Boss.SetWaitingForAttack(false);
                                // Debug.Log($"Boss: Player moved significantly outside attack angle (Angle: {angleToPlayer}°), resuming tracking");
                                boss.TransitionToWalk();
                                return;
                            }
                        }

                        // Smoothly rotate toward player (always perform)
                        direction.Normalize();
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        boss.transform.rotation = Quaternion.Slerp(
                            boss.transform.rotation,
                            targetRotation,
                            boss.rotateSpeed * Time.deltaTime * 0.5f
                        );
                    }
                }

                // Check if pre-delay time is complete
                if (idleTimer >= phase1Boss.GetPreAttackDelay())
                {
                    // Add debug log
                    // Debug.Log($"Boss: Pre-attack delay complete ({idleTimer}s elapsed), transitioning to attack state");
                    
                    // Set attack flag - make state transition clear
                    phase1Boss.SetWaitingForAttack(false);

                    // Directly transition to attack state
                    boss.TransitionToAttack();
                    return;
                }

                return;
            }

            // If in post-attack delay
            if (phase1Boss.IsInPostAttackDelay())
            {
                if (idleTimer >= phase1Boss.GetAttackDelay())
                {
                    // After post-delay, transition to Walk state
                    // Debug.Log($"Boss: Post-attack delay complete ({idleTimer}s elapsed), transitioning to Walk state");
                    phase1Boss.SetPostAttackDelay(false);
                    boss.TransitionToWalk();
                }

                return;
            }

            // If in normal Idle state (transition to Walk after wait time)
            if (idleTimer >= boss.idleDuration)
            {
                // Find player (GameObject with Player tag)
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                if (player)
                {
                    // Set found player as target
                    boss.targetPlayer = player;
                    // Debug.Log("Boss: Wait time complete, found player and starting pursuit");

                    // Transition to Walk state
                    boss.TransitionToWalk();
                }
                else
                {
                    Debug.LogWarning("Boss: Cannot find player.");
                }
            }
        }
    }

    // Phase 1 specific Walk state
    public class Phase1WalkState : WalkState
    {
        private BossPhase1 phase1Boss;

        public Phase1WalkState(BossPhase1 boss) : base(boss)
        {
            this.phase1Boss = boss;
        }

        public override void Enter()
        {
            // Play Walk animation
            boss.animator.SetTrigger("Walk");

            // Play Phase1 specific walking sound
            phase1Boss.PlayWalkSound();
        }

        public override void Exit()
        {
            phase1Boss.StopWalkSound();
        }

        protected override void HandleWalk()
        {
            // Calculate movement vector toward player
            Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
            direction.y = 0; // Prevent Y-axis movement

            // Calculate distance to player
            float distanceToPlayer = direction.magnitude;

            // Normalize direction vector (used for rotation and movement)
            direction.Normalize();

            // Continue moving if distance to player is greater than attack distance
            if (distanceToPlayer > boss.attackDistance)
            {
                // Handle movement
                boss.transform.position += direction * boss.moveSpeed * Time.deltaTime;

                // Smoothly rotate toward player
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                boss.transform.rotation = Quaternion.Slerp(
                    boss.transform.rotation,
                    targetRotation,
                    boss.rotateSpeed * Time.deltaTime
                );
            }
            else
            {
                // Debug.Log($"Boss: Reached attack distance (Current: {distanceToPlayer}m, Attack range: {boss.attackDistance}m)");

                // Check if player is within boss's front 90-degree range
                if (!phase1Boss.IsPlayerInAttackAngle())
                {
                    // If angle is not aligned, continue rotating only
                    // Debug.Log("Boss: Player is outside attack angle range, continuing rotation");

                    // Smoothly rotate toward player (no movement)
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    boss.transform.rotation = Quaternion.Slerp(
                        boss.transform.rotation,
                        targetRotation,
                        boss.rotateSpeed * Time.deltaTime * 1.5f // Slightly faster rotation
                    );

                    return; // Don't execute other attack logic
                }

                if (!phase1Boss.IsAttackSelected())
                {
                    // If attack not selected yet, select one
                    phase1Boss.SelectAttackType();
                }
                else if (!phase1Boss.IsWaitingForAttack() && !phase1Boss.IsAttackInitiated())
                {
                    // If attack is selected but not in pre-delay and attack hasn't started yet
                    // Directly transition to attack state
                    // Debug.Log("Boss: Starting selected attack.");
                    boss.TransitionToAttack();
                }
            }
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

            // Apply damage at midpoint of attack (could be replaced with animation event)
            if (attackTimer >= 1.0f && !hasDealtDamage)
            {
                // Check player in attack range and apply damage
                // ApplyDamageToPlayer();
                hasDealtDamage = true;
            }

            // When attack animation ends, transition state
            if (attackTimer >= attackDuration)
            {
                // Transition to Idle state for post-attack delay
                phase1Boss.SetPostAttackDelay(true); // Set post-attack delay state
                boss.TransitionToIdle();
            }
        }
        
        private void SpawnAttack1Effect()
        {
            if (phase1Boss.GetAttack1EffectPrefab())
            {
                // Create effect at world position based on boss position
                // Apply offset directly to world coordinates
                Vector3 effectWorldPosition = phase1Boss.transform.TransformPoint(phase1Boss.GetAttack1EffectOffset());
        
                // Create effect at specified position (considering boss rotation)
                GameObject effect = GameObject.Instantiate(
                    phase1Boss.GetAttack1EffectPrefab(), 
                    effectWorldPosition,
                    phase1Boss.transform.rotation  // Apply boss rotation
                );
        
                // Remove effect after certain time (3 seconds, adjust as needed)
                GameObject.Destroy(effect, 3f);
        
                // Debug.Log("Boss: Attack1 effect created - Position: " + effectWorldPosition);
            }
        }

        public override void Exit()
        {
            // Debug.Log("보스: 공격1 상태 종료");
        }
    }

    // Phase 1 specific ranged attack state
    public class Attack2State : BossState
    {
        private float attackTimer = 0f;
        private float attackDuration = 3f;
        private bool hasProjectileFired = false;
        private bool hasSoundPlayed = false; // Variable to track if sound has played
        private BossPhase1 phase1Boss;

        public Attack2State(Boss boss) : base(boss)
        {
            phase1Boss = boss as BossPhase1;
        }

        public override void Enter()
        {
            // Initialize timers
            attackTimer = 0f;
            hasProjectileFired = false;
            hasSoundPlayed = false;

            // Play attack2 animation
            boss.animator.SetTrigger("Attack");
            // Debug.Log("Boss: Attack2 state started");
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // Play attack sound at 1 second mark
            if (attackTimer >= 1.0f && !hasSoundPlayed)
            {
                // Play attack sound
                phase1Boss.PlayAttackSound(2);
                hasSoundPlayed = true;
                // Debug.Log("Boss: Playing Attack2 sound");
            }

            // Fire projectile at midpoint of attack (could be replaced with animation event)
            if (attackTimer >= 1.5f && !hasProjectileFired)
            {
                // Fire projectile
                phase1Boss.FireProjectile();
                hasProjectileFired = true;
            }

            // When attack animation ends, transition state
            if (attackTimer >= attackDuration)
            {
                // Transition to Idle state for post-attack delay
                phase1Boss.SetPostAttackDelay(true); // Set post-attack delay state
                boss.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            // Debug.Log("Boss: Attack2 state ended");
        }
    }
}