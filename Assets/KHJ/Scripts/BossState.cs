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
    private float idleTimer = 0f;
    
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
        
        // 지정된 시간(idleDuration)이 지나면 플레이어 찾고 Walk 상태로 전환
        if (idleTimer >= boss.idleDuration)
        {
            // 플레이어 찾기 (Player 태그를 가진 게임오브젝트)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                // 찾은 플레이어를 타겟으로 설정
                boss.targetPlayer = player;
                
                // Walk 상태로 전환
                boss.TransitionToWalk();
            }
        }
    }
    
    public override void Exit()
    {
        Debug.Log("보스: Idle 상태 종료");
    }
}

public class WalkState : BossState
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

        // 플레이어 방향으로 이동 벡터 계산
        Vector3 direction = boss.targetPlayer.transform.position - boss.transform.position;
        direction.y = 0; // Y축 이동 방지
        
        // 플레이어와의 거리 계산
        float distanceToPlayer = direction.magnitude;

        // 플레이어와의 거리가 공격 거리보다 크면 계속 이동
        if (distanceToPlayer > boss.attackDistance)
        {
            // 방향 벡터 정규화
            direction.Normalize();
            
            // 이동 처리
            boss.transform.position += direction * boss.moveSpeed * Time.deltaTime;
            
            // 플레이어 방향으로 부드럽게 회전
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            boss.transform.rotation = Quaternion.Slerp(
                boss.transform.rotation, 
                targetRotation, 
                boss.rotateSpeed * Time.deltaTime
            );
        }
        else
        {
            // 공격 거리에 도달했으면 공격 상태로 전환
            // 직접 Attack1State를 참조하지 않고 전환 메서드 사용
            boss.TransitionToAttack();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("보스: Walk 상태 종료");
    }
}

public class StunState : BossState
{
    private float stunTimer = 0f;
    private float stunDuration = 3f;
    
    public StunState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        stunTimer = 0f;
        boss.animator.SetTrigger("Stun");
        Debug.Log("보스: 스턴 상태 시작");
    }

    public override void Update()
    {
        stunTimer += Time.deltaTime;
        if (stunTimer >= stunDuration)
        {
            boss.ChangeState<IdleState>();
        }
    }

    public override void Exit()
    {
        Debug.Log("보스: 스턴 상태 종료");
    }
}

public class DeathState : BossState
{
    private float deathTimer = 0f;
    private float deathDuration = 3f;
    
    public DeathState(Boss boss) : base(boss) { }

    public override void Enter()
    {
        boss.animator.SetTrigger("Death");
        Debug.Log("보스: 사망 상태 시작");
    }

    public override void Update()
    {
        deathTimer += Time.deltaTime;
        
        if (deathTimer >= deathDuration)
        {
            // 보스 게임 오브젝트 비활성화 또는 파괴
            Object.Destroy(boss.gameObject);
        }
    }
}