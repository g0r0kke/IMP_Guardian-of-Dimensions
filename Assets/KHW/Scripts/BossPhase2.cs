using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// BossPhase2.cs

namespace Azmodan.Phase2
{
    public class BossPhase2 : Boss
    {
        [Header("Phase 2 Settings")]
        [SerializeField] private float phase2DamageMultiplier = 0.8f; // Incoming damage is scaled by0 %
        [SerializeField] private bool playSpawnEffect = true;     // Play a VFX when the boss appears
        [SerializeField] private GameObject spawnEffectPrefab;     // Prefab for the spawn VFX
        [SerializeField] private LayerMask minionLayerMask;     // Layer assigned to spawned minions

        [Header("Attack Common Settings")]
        [SerializeField] private float preAttackDelay = 1.0f;  // Delay BEFORE the attack actually fires
        [SerializeField] private float postAttackDelay = 2.0f;  // Delay AFTER the attack (cool‑down animation)

        [Header("Attack Cooldowns")]
        [SerializeField] private float minionSummonCooldown = 15f;  // Time between minion waves
        [SerializeField] private float missileCooldown = 5f;      // Time between missile shots
        [SerializeField] private float closeCombatCooldown = 5f;  // Time between melee swings

        // Stores the last time each attack type was executed
        private float lastMinionSummonTime = -9999f;
        private float lastMissileTime = -9999f;
        private float lastCloseCombatTime = -9999f;

        [Header("Attack Distances")]
        [SerializeField] private float minionAttackDistance = 15f;  // Max distance for summoning
        [SerializeField] private float missileAttackDistance = 18f;   // Max distance for missile
        [SerializeField] private float closeCombatDistance = 5f;    // Max distance for melee hit

        //  Prefab references
        [Header("Prefabs")]
        [SerializeField] private GameObject missilePrefab;
        [SerializeField] private GameObject minionPrefab;

        //  UI references
        [Header("UI")]
        [SerializeField] private Slider healthBarUI;
        [SerializeField] private int maxHealth = 100; 

        //  Audio references
        [Header("Audio")]
        [SerializeField] public AudioClip minionSummonSound; // Attack1
        [SerializeField] public AudioClip missileSound;      // Attack2
        [SerializeField] public AudioClip closeCombatSound;  // Attack3
        [SerializeField] public AudioClip hitSound;
        [SerializeField] public AudioClip deathSound;
        public AudioSource audioSource;
        [SerializeField] public AudioClip teleportInSound;
        [SerializeField] public AudioClip teleportOutSound;

        //  Internal state control flags
        private BossStateType selectedAttackType;
        private bool attackSelected = false;     // Has an attack been chosen?
        private bool attackInitiated = false;     // Has the chosen attack begun?

        public bool isAttacking = false;
        public bool isTakingDamage = false;
         private float damageCooldown = 0.4f; // Continuous Hit Prevention Time
        private float lastDamageTime = -999f;

        private float teleportInterval = 15f;    // How often the boss CONSIDERS teleporting
        private float lastTeleportCheckTime = -999f;

        protected override void Start()
        {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(0.1f);

            audioSource = GetComponent<AudioSource>();

             // Lock the Y position so the boss appears to hover at eye level.
            Vector3 fixedPos = transform.position;
            fixedPos.y = 1.5f;
            transform.position = fixedPos;

            // Maximum Physical Fitness / Current Physical Fitness Settings
            health = maxHealth;

            base.Start(); // Boss.cs Start() call

            // targetPlayer Auto‑assign
            if (targetPlayer == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    targetPlayer = player;
                }
                else
                {
                    Debug.LogWarning("Player tag object not found");
                }
            }

            // Spawn VFX
            if (playSpawnEffect) PlaySpawnEffect();

            // HP bar UI initialise
            if (healthBarUI != null)
            {
                healthBarUI.maxValue = maxHealth;
                healthBarUI.value = health;
            }

            Debug.Log($"Boss 2 Phase Initial Physical Fitness: {health}");
        }

        private void PlaySpawnEffect()
        {
            if (animator != null) animator.SetTrigger("Enraged");

            // Instantiates the spawn VFX prefab
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        protected override void Update()
        {
            base.Update(); // State Update

            // Continue to secure Y (to avoid falling off)
            Vector3 pos = transform.position;
            pos.y = 0.5f;
            transform.position = pos;

            // Check if a teleport attempt should be made
            if (Time.time - lastTeleportCheckTime >= teleportInterval && TeleportState.CanTeleport())
            {
                lastTeleportCheckTime = Time.time;
                ChangeState<TeleportState>();
            }
        }

        //  Finite‑State‑Machine (FSM) state registration
        protected override void InitializeStates()
        {
            states[typeof(IdleState)]      = new Phase2IdleState(this);
            states[typeof(WalkState)]      = new Phase2WalkState(this);
            states[typeof(Attack1State)]   = new Attack1State(this); // Summon minions
            states[typeof(Attack2State)]   = new Attack2State(this); // Launch missile
            states[typeof(Attack3State)]   = new Attack3State(this); // Melee swipe
            states[typeof(StunState)]      = new StunState(this);
            states[typeof(DeathState)]     = new DeathState(this);
            states[typeof(TeleportState)]  = new TeleportState(this);
        }

        #region Attack Selection
        public void SelectAttackType()
        {
            // The boss is already attacking
            if (attackSelected)
            {
                return;
            }

            // Compute player distanc
            float distanceToPlayer = 9999f;
            if (targetPlayer != null)
            {
                distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);
            }

            // each attack off cooldown
            bool canSummonMinion = (Time.time - lastMinionSummonTime >= minionSummonCooldown);
            bool canMissile      = (Time.time - lastMissileTime      >= missileCooldown);
            bool canCloseCombat  = (Time.time - lastCloseCombatTime  >= closeCombatCooldown);

            // Build list of valid attack
            var possibleAttacks = new System.Collections.Generic.List<BossStateType>();
            if (canSummonMinion && distanceToPlayer <= minionAttackDistance)
                possibleAttacks.Add(BossStateType.Attack1);
            if (canMissile && distanceToPlayer <= missileAttackDistance)
                possibleAttacks.Add(BossStateType.Attack2);
            if (canCloseCombat && distanceToPlayer <= closeCombatDistance)
                possibleAttacks.Add(BossStateType.Attack3);

            // If none are possible we return to Walk
            if (possibleAttacks.Count == 0)
            {
                Debug.Log("Walk back");
                TransitionToWalk();
                return;
            }

            // Randomly choose one of the valid attacks
            selectedAttackType = possibleAttacks[Random.Range(0, possibleAttacks.Count)];
            Debug.Log($"{selectedAttackType} Attack Selection");

            attackSelected = true;
            SetWaitingForAttack(true);  // Begin pre‑attack wind‑up 
            TransitionToIdle();   // IdleState counts down the wind‑up
        }
        #endregion

        //  Wind‑up (pre‑attack) & cool‑down (post‑attack) helpers
        #region Attack Delay
        public void SetWaitingForAttack(bool isWaiting)
        {
            if (isWaiting)
            {
                SetSubState(BossSubState.PreAttackDelay, preAttackDelay);  // PreAttackDelay substate
            }
            else
            {
                if (IsInSubState(BossSubState.PreAttackDelay)) {}  // PreAttackDelay free
            }
        }
        public bool IsWaitingForAttack() => IsInSubState(BossSubState.PreAttackDelay);
        public void SetPostAttackDelay(bool isDelaying)
        {
            if (isDelaying)
            {
                // PostAttackDelay
                SetSubState(BossSubState.PostAttackDelay, postAttackDelay);
                // attack flags reset
                attackSelected = false;
                attackInitiated = false;
            }
            else
            {
                if (IsInSubState(BossSubState.PostAttackDelay)) {}   // PostAttackDelay free
            }
        }
        public bool IsInPostAttackDelay() => IsInSubState(BossSubState.PostAttackDelay);
        public float GetPreAttackDelay() => preAttackDelay;
        public float GetPostAttackDelay() => postAttackDelay;
        #endregion

        //  FSM transition helpers
        #region Attack Transition
        public bool IsAttackSelected() => attackSelected;
        public bool IsAttackInitiated() => attackInitiated;
        public void ResetAttackFlags()
        {
            attackInitiated = false;
            attackSelected = false;
        }

        public override void TransitionToAttack()
        {
            // The boss is already attacking
            if (attackInitiated)
            {
                return;
            }
            attackInitiated = true;

            Debug.Log($"TransitionToAttack - {selectedAttackType}");

            // distance check
            float distanceToPlayer = 0f;
            if (targetPlayer != null)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                dir.y = 0;
                distanceToPlayer = dir.magnitude;
            }

            // Attempt to Teleport if it exceeds the selected attack range
            float selectedAttackDistance = 0f;
            switch (selectedAttackType)
            {
                case BossStateType.Attack1: selectedAttackDistance = minionAttackDistance; break;
                case BossStateType.Attack2: selectedAttackDistance = missileAttackDistance; break;
                case BossStateType.Attack3: selectedAttackDistance = closeCombatDistance; break;
            }

            // Teleport when you are away from the player
            if (distanceToPlayer > selectedAttackDistance * 1.3f)
            {
                Debug.Log("Attempt to teleport");
                ChangeState<TeleportState>();
                return;
            }

            // If out of angle -> Walk
            if (!IsPlayerInAttackAngle())
            {
                Debug.Log("back Walk");
                attackInitiated = false;
                TransitionToWalk();
                return;
            }

            // Final Attack Run
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

            // Attack only when it is within 45 degrees from the front
            bool isInAngle = angleToPlayer <= 45f;
            if (!isInAngle)
            {
                Debug.Log($"Player out of attack angle ({angleToPlayer:F1}°)");
            }
            return isInAngle;
        }
        #endregion

        #region Attack Behaviors

        // Summoning Guided Missiles
        public void FireMissile()
        {
            if (missilePrefab == null || targetPlayer == null)
            {
                Debug.LogWarning("Prefab or Player Not Found");
                return;
            }

            // Record the time of the last missile strike
            lastMissileTime = Time.time;

            // missile production
            GameObject missile = Instantiate(missilePrefab, transform.position + Vector3.up, Quaternion.identity);
            MissileController missileCtrl = missile.GetComponent<MissileController>();
            if (missileCtrl != null)
            {
                missileCtrl.SetTarget(targetPlayer.transform);
            }
        }

        // Summoning the summoner
        public void SpawnMinions()
        {
            if (minionPrefab == null || targetPlayer == null)
            {
                Debug.LogWarning("Prefab or Player Not Found");
                return;
            }

            // Record the time of the last summon time
            lastMinionSummonTime = Time.time;

            // Calculate boss reference position and orientation
            Vector3 bossPos = transform.position;
            Vector3 forwardDir = transform.forward;
            Vector3 rightDir = transform.right;

            float forwardOffset = 4f;
            float sideOffset = 1.5f;

            // No more summon if the number of enemy already exists is 4 or more
            GameObject[] existing = GameObject.FindGameObjectsWithTag("Enemy");
            if (existing.Length >= 4) return;

            // Two summon position offsets (left and right)
            Vector3[] offsets = new Vector3[]
            {
                forwardDir * forwardOffset + rightDir * sideOffset,
                forwardDir * forwardOffset - rightDir * sideOffset
            };

            // Gets the actual layer number from the layer mask
            int minionLayer = GetLayerFromMask(minionLayerMask);
            if (minionLayer == -1)
            {
                Debug.LogError("enemyLayerMask does not have a valid layer");
                return;
            }

            // Summon minions to each location
            foreach (Vector3 offset in offsets)
            {
                Vector3 spawnPos = bossPos + offset;
                spawnPos.y = GetGroundY(spawnPos); // Fixing the floor
                Hobgoblin.Spawner(minionPrefab, spawnPos, targetPlayer.transform, minionLayer);
            }
        }

        // Extracted the actual layer number from LayerMask
        int GetLayerFromMask(LayerMask mask)
        {
            int maskValue = mask.value;
            for (int i = 0; i < 32; i++)
            {
                if ((maskValue & (1 << i)) != 0)
                    return i;
            }
            return -1;
        }

        // Calculate Y coordinates with Raycast to accurately position the ground
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
            // Record the point of the last close attack
            lastCloseCombatTime = Time.time;

            // Damage to players within a range around the boss
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
                    }
                }
            }
        }
        #endregion

        #region Damage & Death
        // Applies incoming damage to the boss, plays effects, and handles death if HP reaches 0.
         public override void TakeDamage(int damage)
        {
            // Ignore damage if within damage cooldown
            if (Time.time - lastDamageTime < damageCooldown)
                return;

            lastDamageTime = Time.time;

            // Apply damage scaling for Phase 2
            int actualDamage = Mathf.RoundToInt(damage * phase2DamageMultiplier);
            health -= actualDamage;

            // Update HP bar
            if (healthBarUI != null)
                healthBarUI.value = health;

            // Play damage animation (if not attacking)
            if (animator != null && !isAttacking)
            {
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("Idle");
                animator.SetTrigger("Damage");
            }

            if (audioSource != null && hitSound != null)
                audioSource.PlayOneShot(hitSound);

            Debug.Log($"HP Remaining: {health}");

            // Check for death
            if (health <= 0)
            {
                Die();
            }
        }

        protected override void Die()
        {
            Debug.Log("Boss 2 Phase dead");

            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            ChangeState<DeathState>();  // Transition to DeathState
        }
        #endregion
    }

    #region State Classes
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

            // If in the PreAttackDelay
            if (phase2Boss.IsWaitingForAttack())
            {
                lastCheckTime += Time.deltaTime;
                if (lastCheckTime >= checkInterval)
                {
                    lastCheckTime = 0f;
                    // Check the distance/angle
                    if (boss.targetPlayer != null)
                    {
                        Vector3 dir = boss.targetPlayer.transform.position - boss.transform.position;
                        float dist = dir.magnitude;

                        if (dist > boss.attackDistance * 1.5f)
                        {
                            phase2Boss.SetWaitingForAttack(false);
                            boss.TransitionToWalk();
                            return;
                        }
                        if (!phase2Boss.IsPlayerInAttackAngle())
                        {
                            phase2Boss.SetWaitingForAttack(false);
                            boss.TransitionToWalk();
                            return;
                        }

                        // Gradually rotate in the player direction from the Idle 
                        dir.y = 0;
                        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                        boss.transform.rotation = Quaternion.Slerp(
                            boss.transform.rotation,
                            targetRot,
                            boss.rotateSpeed * Time.deltaTime
                        );
                    }
                }

                // PreAttackDelay Time's up → Attack entry
                if (idleTimer >= phase2Boss.GetPreAttackDelay())
                {
                    phase2Boss.SetWaitingForAttack(false);
                    boss.TransitionToAttack();
                }
                return;
            }

            // If in the PostAttackDelay
            if (phase2Boss.IsInPostAttackDelay())
            {
                if (idleTimer >= phase2Boss.GetPostAttackDelay())
                {
                    phase2Boss.SetPostAttackDelay(false);
                    boss.TransitionToWalk();
                }
                return;
            }

            // Idle
            if (idleTimer >= boss.idleDuration)
            {
                // Walk
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
                boss.TransitionToIdle();
                return;
            }

            Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
            direction.y = 0;
            float distanceToPlayer = direction.magnitude;

            if (distanceToPlayer > boss.attackDistance)
            {
                // keep move
                direction.Normalize();
                boss.transform.position += direction * boss.moveSpeed * Time.deltaTime;

                // rotate angle
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                boss.transform.rotation = Quaternion.Slerp(
                    boss.transform.rotation,
                    targetRotation,
                    boss.rotateSpeed * Time.deltaTime
                );
            }
            else
            {
                if (!phase2Boss.IsPlayerInAttackAngle())
                {
                    direction.Normalize();
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    boss.transform.rotation = Quaternion.Slerp(
                        boss.transform.rotation,
                        targetRot,
                        boss.rotateSpeed * Time.deltaTime * 1.5f
                    );
                    return;
                }

                // Select an Attack if the attack is not already selected
                if (!phase2Boss.IsAttackSelected())
                {
                    phase2Boss.SelectAttackType();
                }
                else if (!phase2Boss.IsWaitingForAttack() && !phase2Boss.IsAttackInitiated())
                {
                    // If there's no PreAttackDelay and the Attack hasn't started yet
                    boss.TransitionToAttack();
                }
            }
        }
    }

    // Attack1State : Minions
    public class Attack1State : BossState
    {
        private float attackTimer = 0f;  // Time since the attack started
        private float attackDuration = 2.0f;  // Total duration of the attack
        private bool hasSpawnedMinions = false;  // Whether minions have been spawne
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

            Debug.Log("start attack1");
            if (phase2Boss != null && phase2Boss.audioSource != null && phase2Boss.minionSummonSound != null)
            {
                phase2Boss.audioSource.PlayOneShot(phase2Boss.minionSummonSound);
            }
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // Spawn minions after 0.5 seconds
            if (!hasSpawnedMinions && attackTimer >= 0.5f)
            {
                phase2Boss.SpawnMinions();
                hasSpawnedMinions = true;
            }

            // After full attack duration, go back to idle with post-attack delay
            if (attackTimer >= attackDuration)
            {
                // PostAttackDelay
                phase2Boss.SetPostAttackDelay(true);
                boss.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            phase2Boss.isAttacking = false;
            phase2Boss.ResetAttackFlags();   // Reset internal flags to allow next attack
        }
    }

    // Attack2State : Missile
    public class Attack2State : BossState
    {
        private float attackTimer = 0f;    // Time since attack started
        private float attackDuration = 3f;  // Total attack duration
        private bool hasFiredMissile = false;  // Whether missile has been fired
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


            Debug.Log("start attack2");
            if (phase2Boss != null && phase2Boss.audioSource != null && phase2Boss.missileSound != null)
            {
                phase2Boss.audioSource.PlayOneShot(phase2Boss.missileSound);
            }
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // Missile launch in the middle of the line
            if (!hasFiredMissile && attackTimer >= 1.5f)
            {
                phase2Boss.FireMissile();
                hasFiredMissile = true;
            }

            // Return to idle after the attack duration ends
            if (attackTimer >= attackDuration)
            {
                phase2Boss.SetPostAttackDelay(true);
                boss.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            phase2Boss.isAttacking = false;
            phase2Boss.ResetAttackFlags();   // Reset internal flags to allow next attack
        }
    }

    // Attack3State: Combat
    public class Attack3State : BossState
    {
        private float attackTimer = 0f;  // Time since attack started
        private float attackDuration = 1.5f;   // Duration of entire attack animation
        private bool hasAttacked = false;  // Whether attack damage has already been applied
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
            Debug.Log("start attack3");

            if (phase2Boss != null && phase2Boss.audioSource != null && phase2Boss.closeCombatSound != null)
            {
                phase2Boss.audioSource.PlayOneShot(phase2Boss.closeCombatSound);
            }
        }

        public override void Update()
        {
            attackTimer += Time.deltaTime;

            // Determination of actual strike in the middle of Attack motion
            if (!hasAttacked && attackTimer >= 0.7f)
            {
                phase2Boss.CloseCombatAttack();
                hasAttacked = true;
            }

            // Idle after the end of the Attack
            if (attackTimer >= attackDuration)
            {
                phase2Boss.SetPostAttackDelay(true);
                boss.TransitionToIdle();
            }
        }

        public override void Exit()
        {
            phase2Boss.isAttacking = false;
            phase2Boss.ResetAttackFlags();  // Reset internal flags to allow next attack
        }
    }

    // TeleportState
    public class TeleportState : BossState
    {
        private BossPhase2 phase2Boss;
        private Renderer[] renderers;  // Materials for opacity control

        private float fadeOutDuration = 0.5f;
        private float fadeInDuration = 0.5f;

        private static float teleportCooldown = 10f;
        private static float lastTeleportTime = -9999f;  // Last teleport timestamp

        public TeleportState(Boss boss) : base(boss)
        {
            phase2Boss = boss as BossPhase2;
        }

        // Called when teleport state is entered. Begins the teleport coroutine
        public override void Enter()
        {
            renderers = phase2Boss.GetComponentsInChildren<Renderer>();
            phase2Boss.StartCoroutine(DramaticTeleportRoutine());
        }

        // Static method to check if teleport is off cooldown
        public static bool CanTeleport()
        {
            return Time.time - lastTeleportTime >= teleportCooldown;
        }

        private IEnumerator DramaticTeleportRoutine()
        {
            lastTeleportTime = Time.time;

            float t = 0f;
            if (phase2Boss.audioSource != null && phase2Boss.teleportOutSound != null)
                phase2Boss.audioSource.PlayOneShot(phase2Boss.teleportOutSound);

            // First fade out and move backward
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                float alpha = 1f - (t / fadeOutDuration);
                ApplyOpacity(alpha);
                yield return null;
            }

            // Move behind current position
            Vector3 backPos = phase2Boss.transform.position - phase2Boss.transform.forward * 5f;
            backPos.y = 1.5f;
            phase2Boss.transform.position = backPos;

            // Fade back in
            t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                float alpha = (t / fadeInDuration);
                ApplyOpacity(alpha);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);  // Pause briefly before next phase

            t = 0f;
            if (phase2Boss.audioSource != null && phase2Boss.teleportOutSound != null)
                phase2Boss.audioSource.PlayOneShot(phase2Boss.teleportOutSound);

            // Second fade out
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                float alpha = 1f - (t / fadeOutDuration);
                ApplyOpacity(alpha);
                yield return null;
            }

            // Move near player and face them
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 direction = (player.transform.position - phase2Boss.transform.position).normalized;
                direction.y = 0;
                Vector3 frontPos = player.transform.position - direction * 3.5f;
                frontPos.y = 1.5f;
                phase2Boss.transform.position = frontPos;

                Quaternion targetRot = Quaternion.LookRotation(direction);
                phase2Boss.transform.rotation = targetRot;
            }

            // Final fade in
            t = 0f;
            if (phase2Boss.audioSource != null && phase2Boss.teleportInSound != null)
                phase2Boss.audioSource.PlayOneShot(phase2Boss.teleportInSound);

            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                float alpha = (t / fadeInDuration);
                ApplyOpacity(alpha);
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);  // Short delay before returning to Idle
            boss.TransitionToIdle();
        }

        // Applies transparency to all materials on the boss
        private void ApplyOpacity(float alpha)
        {
            if (renderers == null) return;
            foreach (Renderer r in renderers)
            {
                foreach (Material mat in r.materials)
                {
                    mat.SetFloat("_Mode", 3);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;

                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
    }

    // DeathState
    public class DeathState : BossState
    {
        public DeathState(Boss boss) : base(boss) { }

        public override void Enter()
        {
            if (boss.animator != null) boss.animator.SetTrigger("Death");

            // Notify the GameManager that the player has won
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Victory);
            }
            else
            {
                Debug.LogWarning("GameManager not found");
            }
        }

    }
    #endregion
}