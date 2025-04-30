using UnityEngine;

// State interface defining required methods for state pattern implementation
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}
