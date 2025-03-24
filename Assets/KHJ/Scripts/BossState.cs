using UnityEngine;

// 상태 기본 클래스
public abstract class BossState : IState
{
    protected Boss boss;

    public BossState(Boss boss)
    {
        this.boss = boss;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

public class IdleState : BossState
{
    public IdleState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        // Idle 애니메이션 재생
    }

    public override void Update()
    {
        // 플레이어 감지 로직
    }
}

public class WalkState : BossState
{
    public WalkState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        // Walk 애니메이션 재생
    }

    public override void Update()
    {
        // 플레이어 감지 로직
    }
}

public class StunState : BossState
{
    public StunState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        
    }

    public override void Update()
    {
        
    }
}