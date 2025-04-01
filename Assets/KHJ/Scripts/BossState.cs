using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

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

public abstract class IdleState : BossState
{
    protected float idleTimer = 0f;
    
    public IdleState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        idleTimer = 0f;
        boss.animator.SetTrigger("Idle");
        Debug.Log("보스: Idle 상태 시작");
    }

    public override void Update()
    {
        // 타이머 증가
        idleTimer += Time.deltaTime;
        
        // 자식 클래스에서 구현할 상태 로직 처리
        HandleStateLogic();
    }
    
    // 자식 클래스에서 구현할 추상 메서드
    protected abstract void HandleStateLogic();
    
    public override void Exit()
    {
        Debug.Log("보스: Idle 상태 종료");
    }
}

public abstract class WalkState : BossState
{
    public WalkState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        // Walk 애니메이션 재생
        boss.animator.SetTrigger("Walk");
        Debug.Log("보스: Walk 상태 시작");
    }

    public override void Update()
    {
        // 타겟 플레이어가 없거나 파괴된 경우 다시 Idle 상태로
        if (boss.targetPlayer == null)
        {
            boss.TransitionToIdle();
            return;
        }

        // 자식 클래스에서 구현할 이동 로직 처리
        HandleMovement();
    }
    
    // 자식 클래스에서 구현할 추상 메서드
    protected abstract void HandleMovement();
    
    public override void Exit()
    {
        Debug.Log("보스: Walk 상태 종료");
    }
}

public class StunState : BossState
{
    protected float stunTimer = 0f;
    protected float stunDuration = 0.6f;
    
    public StunState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        stunTimer = 0f;
        boss.animator.SetBool("Damage", true);
        Debug.Log("보스: 스턴 상태 시작");
    }

    public override void Update()
    {
        stunTimer += Time.deltaTime;
        if (stunTimer >= stunDuration)
        {
            // BossPhase1의 경우 이전 상태로 복귀
            if (boss is Azmodan.Phase1.BossPhase1 phase1Boss)
            {
                boss.animator.SetBool("Damage", false);
                
                // BossPhase1의 previousState 값에 따라 다른 상태로 전환
                IState prevState = phase1Boss.GetPreviousState();
                if (prevState is IdleState)
                {
                    boss.TransitionToIdle();
                }
                else if (prevState is WalkState)
                {
                    boss.TransitionToWalk();
                }
                else
                {
                    // 기본값으로 Idle
                    boss.TransitionToIdle();
                }
            }
            else
            {
                // 기본 동작: Idle 상태로 전환
                boss.TransitionToIdle();
            }
        }
    }

    public override void Exit()
    {
        boss.animator.SetBool("Damage", false);
        Debug.Log("보스: 스턴 상태 종료");
    }
}

public class DeathState : BossState
{
    private float deathTimer = 0f;
    private float deathDuration = 3f;
    private bool animationComplete = false;
    
    public DeathState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        deathTimer = 0f;
        animationComplete = false;
        
        // 명시적인 애니메이션 트리거 설정은 하지 않음
        // 이미 TransitionToDeath에서 설정했기 때문
        
        Debug.Log("보스: 사망 상태 시작");
    }

    public override void Update()
    {
        if (animationComplete) return;
        
        deathTimer += Time.deltaTime;
        
        if (deathTimer >= deathDuration)
        {
            animationComplete = true;
            Debug.Log("보스: 사망 애니메이션 완료");
            
            boss.gameObject.SetActive(false);
        }
    }

    public override void Exit()
    {
        Debug.Log("보스: 사망 상태 종료 - 이 메시지가 출력되면 상태 전환에 문제가 있음");
    }
}