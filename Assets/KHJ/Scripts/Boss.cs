using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;

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

public enum BossSubState
{
    None,
    PreAttackDelay,   // 공격 전 대기
    PostAttackDelay,  // 공격 후 대기
    InvinciblePeriod  // 무적 시간
}

// 추상 보스 클래스 (공통 기능 포함)
public abstract class Boss : MonoBehaviour
{
    protected IState currentState;
    protected Dictionary<System.Type, IState> states = new Dictionary<System.Type, IState>();
    
    // 현재 상태 유형
    protected BossStateType currentStateType;

    // 보스 공통 속성
    protected int health = 100;
    protected Transform target;

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
    
    // 디버그 표시 설정
    [Header("Debug Settings")]
    [SerializeField] protected bool enableDebug = true;
    [SerializeField] protected Color stateDebugColor = Color.yellow;
    [SerializeField] protected Color subStateDebugColor = Color.green;
    [SerializeField] protected Color healthDebugColor = Color.red;
    [SerializeField] protected Vector3 debugOffset = new Vector3(0, 2.0f, 0);

    protected virtual void Awake()
    {
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
    
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    
        // 시작할 때 죽음 관련 트리거와 플래그 초기화
        isDead = false;
        isDeathAnimTriggered = false;
        animator.ResetTrigger("Death");
    
        // 상태 초기화
        InitializeStates();
        // 초기 상태 설정
        TransitionToIdle();
        InitializeSubStates();
    }
    
    // 모든 애니메이터 파라미터 초기화 메서드
    protected void ResetAllAnimatorParameters()
    {
        // Boolean 파라미터 초기화
        animator.SetBool("Damage", false);
    
        // Trigger 파라미터는 ResetTrigger 메서드 사용
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Attack");
    
        // Death 트리거도 명시적으로 초기화
        animator.ResetTrigger("Death");
    }
    
    protected virtual void InitializeSubStates()
    {
        // 열거형의 모든 값에 대해 타이머와 지속시간 초기화
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
    
    // 하위 상태 설정 메서드
    public void SetSubState(BossSubState state, float duration)
    {
        subStateTimers[state] = 0f;
        subStateDurations[state] = duration;
    }

    // 자식 클래스가 구현할 상태 초기화 메소드
    protected abstract void InitializeStates();

    protected virtual void Update()
    {
        // 삭제 예정
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
           TakeDamage(50);
        }
        
        // 사망 상태면 현재 상태만 업데이트 (DeathState)
        if (isDead)
        {
            if (currentState != null && currentState is DeathState)
            {
                // Death 상태일 때도 애니메이션 트리거는 다시 설정하지 않음
                currentState.Update();
            }
            return;
        }

        // 현재 상태 업데이트
        if (currentState != null)
        {
            currentState.Update();
        }
    
        // 하위 상태 타이머 업데이트
        UpdateSubStates();
    }
    
    // 하위 상태 타이머 업데이트 메서드
    protected virtual void UpdateSubStates()
    {
        // subStateTimers는 Dictionary
        // 키 = BossSubState 열거형, 값 = 각 상태의 현재 타이머 시간
        // .Keys = Dictionary의 모든 키를 가져오기
        // .ToList() = 키 컬렉션의 복사본 만들기. 반복 중에 컬렉션을 수정하려고 할 때 발생할 수 있는 오류 방지
        foreach (var state in subStateTimers.Keys.ToList())
        {
            // 현재 상태의 타이머가 지정된 지속 시간보다 작은지 확인
            if (subStateTimers[state] < subStateDurations[state])
            {
                subStateTimers[state] += Time.deltaTime; // 타이머 증가
            }
        }
    }

    // 상태 변경 메소드 (제네릭 사용)
    // <T> = 타입 매개변수. 메서드 호출 시 특정 타입 지정
    // T는 반드시 IState 인터페이스를 구현한 타입이어야 한다.
    protected void ChangeState<T>() where T : IState
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        // DeathState가 아니면
        if (typeof(T) != typeof(DeathState))
        {
            // 모든 애니메이터 파라미터 초기화
            ResetAllAnimatorParameters();
        }

        // typeof(T) = T의 실제 타입 정보를 가져옴
        System.Type type = typeof(T);
        // Dictionary에서 키(type)에 해당하는 값 찾음
        // states Dictionary는 키: 상태 타입, 값: 상태 객체
        // 찾은 상태 객체를 newState 변수에 저장
        if (states.TryGetValue(type, out IState newState))
        {
            // 찾은 상태 객체를 현재 상태로 설정
            currentState = newState;
            // 해당 상태의 Enter() 메서드 호출
            currentState.Enter();
        }
    }
    
    // 상태 전환 메서드
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
    
    // 공격 상태 전환: 자식 클래스에서 반드시 구현해야 함
    public abstract void TransitionToAttack();
    
    // 스턴 상태로 전환
    public virtual void TransitionToStun()
    {
        currentStateType = BossStateType.Stun;
        ChangeState<StunState>();
    }
    
    // 사망 상태로 전환
    public virtual void TransitionToDeath()
    {
        // 이미 Death 애니메이션이 트리거 되었으면 중복 호출 방지
        if (isDeathAnimTriggered)
        {
            // Debug.Log("보스: 이미 사망 처리 중입니다. 중복 호출 무시.");
            return;
        }

        // Debug.Log("보스: Death 상태로 전환 시작");

        // Death 애니메이션 트리거 플래그 설정
        isDeathAnimTriggered = true;
        isDead = true;

        // 현재 상태 타입 설정
        currentStateType = BossStateType.Death;

        // 모든 진행 중인 코루틴 중지
        StopAllCoroutines();

        // 여기가 중요: SetTrigger 다시 사용 (트리거가 확실히 발동하도록)
        // 명시적으로 모든 트리거 리셋 후 Death 트리거 설정
        // 이렇게 해야 애니메이션이 확실하게 재생됨
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Death");  // Death 트리거 초기화 먼저 하고
        animator.SetTrigger("Death");    // 다시 설정

        // 상태 변경 실행
        ChangeState<DeathState>();
    }

    // 공통 메소드들 
    public virtual void TakeDamage(int damage)
    {
        // 이미 사망했다면 데미지 처리하지 않음
        if (isDead) 
        {
            return;
        }

        // 데미지 적용
        health -= damage;

        // 사망 처리
        if (health <= 0 && !isDeathAnimTriggered)
        {
            // 사망 플래그 설정
            isDead = true;
        
            // 사망 처리는 한 번만
            Die();
            return;
        }
    }
    
    protected virtual void ResetAllStates()
    {
        // 모든 하위 상태 타이머 초기화
        foreach (BossSubState state in System.Enum.GetValues(typeof(BossSubState)))
        {
            subStateTimers[state] = 0f;
            subStateDurations[state] = 0f;
        }
    
        // Debug.Log("보스: 모든 상태 초기화됨 (사망 처리)");
    }

    protected virtual void Die()
    {
        if (!isDead) // 이미 죽은 상태가 아닐 때만 처리
        {
            isDead = true; // 사망 플래그 설정
            TransitionToDeath();
        }
    }
    
    // 현재 상태 이름 가져오기
    public string GetCurrentStateName()
    {
        if (currentState == null) return "None";
        return currentState.GetType().Name;
    }
    
    // 활성화된 하위 상태 가져오기
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
    
    // 화면에 디버그 텍스트 표시
    protected virtual void OnGUI()
    {
        if (!enableDebug) return;
        
        // 보스 위치를 스크린 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + debugOffset);
        
        // 화면 밖이면 표시하지 않음
        if (screenPos.z < 0) return;
        
        // 상태 텍스트 위치
        Rect stateRect = new Rect(screenPos.x - 100, Screen.height - screenPos.y, 200, 20);
        Rect subStateRect = new Rect(screenPos.x - 100, Screen.height - screenPos.y + 20, 200, 20);
        Rect healthRect = new Rect(screenPos.x - 100, Screen.height - screenPos.y + 40, 200, 20);
        
        // 스타일 설정
        GUIStyle stateStyle = new GUIStyle();
        stateStyle.normal.textColor = stateDebugColor;
        stateStyle.fontSize = 30;
        stateStyle.fontStyle = FontStyle.Bold;
        stateStyle.alignment = TextAnchor.UpperCenter;
        
        GUIStyle subStateStyle = new GUIStyle(stateStyle);
        subStateStyle.normal.textColor = subStateDebugColor;
        subStateStyle.fontSize = 30;
        
        GUIStyle healthStyle = new GUIStyle(stateStyle);
        healthStyle.normal.textColor = healthDebugColor;
        
        // 텍스트 그리기 (배경 효과를 위해 약간 오프셋된 검은색 텍스트 먼저 그림)
        // 상태 표시
        GUI.Label(new Rect(stateRect.x + 1, stateRect.y + 1, stateRect.width, stateRect.height), 
            $"State: {GetCurrentStateName()}", new GUIStyle(stateStyle) { normal = { textColor = Color.black } });
        GUI.Label(stateRect, $"State: {GetCurrentStateName()}", stateStyle);
        
        // 하위 상태 표시
        List<BossSubState> activeSubStates = GetActiveSubStates();
        if (activeSubStates.Count > 0)
        {
            StringBuilder sb = new StringBuilder("SubState: ");
            for (int i = 0; i < activeSubStates.Count; i++)
            {
                sb.Append(activeSubStates[i].ToString());
                if (i < activeSubStates.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            
            GUI.Label(new Rect(subStateRect.x + 1, subStateRect.y + 1, subStateRect.width, subStateRect.height), 
                sb.ToString(), new GUIStyle(subStateStyle) { normal = { textColor = Color.black } });
            GUI.Label(subStateRect, sb.ToString(), subStateStyle);
        }
        
        // 체력 표시
        GUI.Label(new Rect(healthRect.x + 1, healthRect.y + 1, healthRect.width, healthRect.height), 
            $"Health: {health}", new GUIStyle(healthStyle) { normal = { textColor = Color.black } });
        GUI.Label(healthRect, $"Health: {health}", healthStyle);
    }
}