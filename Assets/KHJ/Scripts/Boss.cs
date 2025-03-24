using System.Collections.Generic;
using UnityEngine;

// 추상 보스 클래스 (공통 기능 포함)
public abstract class Boss : MonoBehaviour
{
    protected IState currentState;
    protected Dictionary<System.Type, IState> states = new Dictionary<System.Type, IState>();

    // 보스 공통 속성
    protected int health;
    protected Transform target;

    protected virtual void Start()
    {
        // 상태 초기화
        InitializeStates();
        // 초기 상태 설정
        ChangeState<IdleState>();
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

    // 공통 메소드들
    public abstract void TakeDamage(int damage);
}
