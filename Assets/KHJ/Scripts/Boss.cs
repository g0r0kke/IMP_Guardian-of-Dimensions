using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Enum for main boss states
public enum BossStateType
{
    Idle,
    Walk,
    Attack1,
    Attack2,
    Attack3,
    Stun,
    Death
}

// Enum for sub-states that can be active alongside main states
public enum BossSubState
{
    PreAttackDelay,   // Delay before attack
    PostAttackDelay  // Delay after attack
}

// Abstract boss class (includes common functionality)
public abstract class Boss : MonoBehaviour
{
    protected IState currentState;
    protected Dictionary<System.Type, IState> states = new Dictionary<System.Type, IState>();
    
    // Current state type
    protected BossStateType currentStateType;

    // Boss common properties
    protected int health = 100;

    [SerializeField] public float idleDuration = 2f;
    [SerializeField] public float attackDistance = 2f;
    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] public float rotateSpeed = 5f;

    public Animator animator;
    public GameObject targetPlayer;

    protected Dictionary<BossSubState, float> subStateTimers = new Dictionary<BossSubState, float>();
    protected Dictionary<BossSubState, float> subStateDurations = new Dictionary<BossSubState, float>();
    protected bool isDead = false;
    protected bool isDeathAnimTriggered = false;
    
    // Debug display settings
    [Header("Debug Settings")]
    [SerializeField] protected bool enableDebug = true;
    [SerializeField] protected Color stateDebugColor = Color.yellow;
    [SerializeField] protected Color subStateDebugColor = Color.green;
    [SerializeField] protected Color healthDebugColor = Color.red;
    [SerializeField] protected Vector3 debugOffset = new Vector3(0, 2.0f, 0);

    protected virtual void Awake()
    {
        if (GameManager.Instance)
        {
            Vector3 bossPosition = GameManager.Instance.GetBossPosition();
            transform.position = bossPosition;
            Debug.Log($"Boss position set from GameManager: {bossPosition}");
        }
        else
        {
            Debug.LogWarning("GameManager not found. Using default position.");
        }
    }
    
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    
        // Initialize death-related triggers and flags at start
        isDead = false;
        isDeathAnimTriggered = false;
        animator.ResetTrigger("Death");
    
        // Initialize states
        InitializeStates();
        // Set initial state
        TransitionToIdle();
        InitializeSubStates();
    }
    
    // Method to reset all animator parameters
    protected void ResetAllAnimatorParameters()
    {
        // Reset Boolean parameters
        animator.SetBool("Damage", false);
    
        // Use ResetTrigger method for Trigger parameters
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Attack");
    
        // Explicitly reset Death trigger
        animator.ResetTrigger("Death");
    }
    
    protected virtual void InitializeSubStates()
    {
        // Initialize timer and duration for all substate values
        foreach (BossSubState state in System.Enum.GetValues(typeof(BossSubState)))
        {
            subStateTimers[state] = 0f;
            subStateDurations[state] = 0f;
        }
    }
    
    public bool IsInSubState(BossSubState state)
    {
        return subStateTimers.ContainsKey(state) && 
               subStateTimers[state] > 0f && 
               subStateTimers[state] < subStateDurations[state];
    }
    
    // Method to set a substate
    public void SetSubState(BossSubState state, float duration)
    {
        subStateTimers[state] = 0f;
        subStateDurations[state] = duration;
    }

    // State initialization method to be implemented by child classes
    protected abstract void InitializeStates();

    protected virtual void Update()
    {
        // To be removed later
        // if (Keyboard.current.qKey.wasPressedThisFrame)
        // {
        //    TakeDamage(50);
        // }
        
        // If in death state, only update the current state (DeathState)
        if (isDead)
        {
            if (currentState != null && currentState is DeathState)
            {
                // Don't set animation trigger again when in Death state
                currentState.Update();
            }
            return;
        }

        // Update current state
        if (currentState != null)
        {
            currentState.Update();
        }
    
        // Update substate timers
        UpdateSubStates();
    }
    
    // Method to update substate timers
    protected virtual void UpdateSubStates()
    {
        // subStateTimers is a Dictionary
        // Keys = BossSubState enum, Values = current timer time for each state
        // .Keys = Get all keys from the Dictionary
        // .ToList() = Create a copy of the key collection to prevent errors when trying to modify the collection during iteration
        foreach (var state in subStateTimers.Keys.ToList())
        {
            // Check if current state's timer is less than the specified duration
            if (subStateTimers[state] < subStateDurations[state])
            {
                subStateTimers[state] += Time.deltaTime; // Increment timer
            }
        }
    }

    // State change method (using generics)
    // <T> = Type parameter. Specify a specific type when calling the method
    // T must be a type that implements IState interface
    protected void ChangeState<T>() where T : IState
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        // If not DeathState
        if (typeof(T) != typeof(DeathState))
        {
            // Reset all animator parameters
            ResetAllAnimatorParameters();
        }

        // typeof(T) = Get actual type information for T
        System.Type type = typeof(T);
        // Find value corresponding to key(type) in Dictionary
        // states Dictionary has keys: state type, values: state object
        // Store found state object in newState variable
        if (states.TryGetValue(type, out IState newState))
        {
            // Set found state object as current state
            currentState = newState;
            // Call Enter() method on that state
            currentState.Enter();
        }
    }
    
    // State transition methods
    public virtual void TransitionToIdle()
    {
        currentStateType = BossStateType.Idle;
        ChangeState<IdleState>();
    }
    
    public virtual void TransitionToWalk()
    {
        currentStateType = BossStateType.Walk;
        ChangeState<WalkState>();
    }
    
    // Attack state transition: must be implemented by child classes
    public abstract void TransitionToAttack();
    
    // Transition to stun state
    public virtual void TransitionToStun()
    {
        currentStateType = BossStateType.Stun;
        ChangeState<StunState>();
    }
    
    // Transition to death state
    public virtual void TransitionToDeath()
    {
        // Prevent duplicate calls if Death animation is already triggered
        if (isDeathAnimTriggered)
        {
            // Debug.Log("Boss: Already processing death. Ignoring duplicate call.");
            return;
        }

        // Debug.Log("Boss: Starting transition to Death state");

        // Set Death animation trigger flag
        isDeathAnimTriggered = true;
        isDead = true;

        // Set current state type
        currentStateType = BossStateType.Death;

        // Stop all running coroutines
        StopAllCoroutines();

        // Use SetTrigger again (to ensure trigger activates)
        // Explicitly reset all triggers then set Death trigger
        // This ensures the animation plays reliably
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Death");  // Reset Death trigger first
        animator.SetTrigger("Death");    // Then set it again

        // Execute state change
        ChangeState<DeathState>();
    }

    // Common methods 
    public virtual void TakeDamage(int damage)
    {
        // Don't process damage if already dead
        if (isDead) 
        {
            return;
        }

        // Apply damage
        health -= damage;

        // Handle death
        if (health <= 0 && !isDeathAnimTriggered)
        {
            // Set death flag
            isDead = true;
        
            // Process death only once
            Die();
        }
    }

    protected virtual void Die()
    {
        if (!isDead) // Only process if not already dead
        {
            isDead = true; // Set death flag
            TransitionToDeath();
        }
    }
    
    // Get current state name
    public string GetCurrentStateName()
    {
        if (currentState == null) return "None";
        return currentState.GetType().Name;
    }
    
    // Get active substates
    public List<BossSubState> GetActiveSubStates()
    {
        List<BossSubState> activeStates = new List<BossSubState>();
        foreach (BossSubState state in System.Enum.GetValues(typeof(BossSubState)))
        {
            if (IsInSubState(state))
            {
                activeStates.Add(state);
            }
        }
        return activeStates;
    }
    
    // Display debug text on screen
    // protected virtual void OnGUI()
    // {
    //     if (!enableDebug) return;
    //     
    //     // Convert boss position to screen coordinates
    //     Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + debugOffset);
    //     
    //     // Don't display if off-screen
    //     if (screenPos.z < 0) return;
    //     
    //     // State text position
    //     Rect stateRect = new Rect(screenPos.x - 100, Screen.height - screenPos.y, 200, 20);
    //     Rect subStateRect = new Rect(screenPos.x - 100, Screen.height - screenPos.y + 20, 200, 20);
    //     Rect healthRect = new Rect(screenPos.x - 100, Screen.height - screenPos.y + 40, 200, 20);
    //     
    //     // Set style
    //     GUIStyle stateStyle = new GUIStyle();
    //     stateStyle.normal.textColor = stateDebugColor;
    //     stateStyle.fontSize = 30;
    //     stateStyle.fontStyle = FontStyle.Bold;
    //     stateStyle.alignment = TextAnchor.UpperCenter;
    //     
    //     GUIStyle subStateStyle = new GUIStyle(stateStyle);
    //     subStateStyle.normal.textColor = subStateDebugColor;
    //     subStateStyle.fontSize = 30;
    //     
    //     GUIStyle healthStyle = new GUIStyle(stateStyle);
    //     healthStyle.normal.textColor = healthDebugColor;
    //     
    //     // Draw text (draw black offset text first for background effect)
    //     // Display state
    //     GUI.Label(new Rect(stateRect.x + 1, stateRect.y + 1, stateRect.width, stateRect.height), 
    //         $"State: {GetCurrentStateName()}", new GUIStyle(stateStyle) { normal = { textColor = Color.black } });
    //     GUI.Label(stateRect, $"State: {GetCurrentStateName()}", stateStyle);
    //     
    //     // Display substates
    //     List<BossSubState> activeSubStates = GetActiveSubStates();
    //     if (activeSubStates.Count > 0)
    //     {
    //         StringBuilder sb = new StringBuilder("SubState: ");
    //         for (int i = 0; i < activeSubStates.Count; i++)
    //         {
    //             sb.Append(activeSubStates[i].ToString());
    //             if (i < activeSubStates.Count - 1)
    //             {
    //                 sb.Append(", ");
    //             }
    //         }
    //         
    //         GUI.Label(new Rect(subStateRect.x + 1, subStateRect.y + 1, subStateRect.width, subStateRect.height), 
    //             sb.ToString(), new GUIStyle(subStateStyle) { normal = { textColor = Color.black } });
    //         GUI.Label(subStateRect, sb.ToString(), subStateStyle);
    //     }
    //     
    //     // Display health
    //     GUI.Label(new Rect(healthRect.x + 1, healthRect.y + 1, healthRect.width, healthRect.height), 
    //         $"Health: {health}", new GUIStyle(healthStyle) { normal = { textColor = Color.black } });
    //     GUI.Label(healthRect, $"Health: {health}", healthStyle);
    // }
}