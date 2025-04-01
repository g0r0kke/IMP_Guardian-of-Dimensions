using UnityEngine;

public abstract class HobgoblinState : IState
{
    protected Hobgoblin hobgoblin;

    public HobgoblinState(Hobgoblin hobgoblin)
    {
        this.hobgoblin = hobgoblin;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}




