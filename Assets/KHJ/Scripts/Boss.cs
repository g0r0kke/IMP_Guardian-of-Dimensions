using System.Collections.Generic;
using UnityEngine;

// 추상 보스 클래스 (공통 기능 포함)
public abstract class Boss : MonoBehaviour
{
    protected IState currentState;
    protected Dictionary<System.Type, IState> states = new Dictionary<System.Type, IState>();

    // 보스 공통 속성
    protected int health = 100;
    protected Transform target;

    [SerializeField] public float idleDuration = 5f;
    [SerializeField] public float attackDistance = 2f;
    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] public float rotateSpeed = 5f;

    public Animator animator;
    public GameObject targetPlayer;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();

        // 상태 초기화
        InitializeStates();
        // 초기 상태 설정
        TransitionToIdle();
    }

    // 자식 클래스가 구현할 상태 초기화 메소드
    protected abstract void InitializeStates();

    protected virtual void Update()
    {
        // 현재 상태 업데이트
        if (currentState != null)
        {
            currentState.Update();
        }
    }

    // 상태 변경 메소드 (제네릭 사용)
    public void ChangeState<T>() where T : IState
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        System.Type type = typeof(T);
        if (states.TryGetValue(type, out IState newState))
        {
            currentState = newState;
            currentState.Enter();
        }
    }

    // 상태 전환 메서드: 자식 클래스에서 구현/재정의 가능
    public virtual void TransitionToIdle()
    {
        ChangeState<IdleState>();
    }

    public virtual void TransitionToWalk()
    {
        ChangeState<WalkState>();
    }

    // 공격 상태 전환: 자식 클래스에서 반드시 구현해야 함
    public abstract void TransitionToAttack();

    // 스턴 상태로 전환
    public virtual void TransitionToStun()
    {
        ChangeState<StunState>();
    }

    // 사망 상태로 전환
    public virtual void TransitionToDeath()
    {
        ChangeState<DeathState>();
    }

    // 공통 메소드들
    public virtual void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        TransitionToDeath();
    }
}
