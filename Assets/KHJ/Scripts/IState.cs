using UnityEngine;

/// <summary>
/// State interface defining required methods for state pattern implementation
/// </summary>
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}
