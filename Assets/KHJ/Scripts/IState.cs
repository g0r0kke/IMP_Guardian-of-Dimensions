using UnityEngine;

// 상태 인터페이스
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}
