using UnityEngine;

namespace Azmodan.Phase2
{
    public class BossPhase2 : Boss
    {
        protected override void InitializeStates()
        {
            states[typeof(IdleState)] = new IdleState(this);
            states[typeof(WalkState)] = new WalkState(this);
            states[typeof(Attack1State)] = new Attack1State(this);
            states[typeof(Attack2State)] = new Attack2State(this);
            states[typeof(StunState)] = new StunState(this);
            states[typeof(DeathState)] = new DeathState(this);
        }

        public override void TakeDamage(int damage)
        {
            health -= damage;
            // 2페이즈 전용 데미지 처리 로직 (ex: 분노 게이지 증가)
        }
    }

    // 2페이즈 전용 상태들
    public class Attack1State : BossState
    {
        public Attack1State(Boss boss) : base(boss) { }

        public override void Enter()
        {
            // 2페이즈 공격1 애니메이션
        }

        public override void Update()
        {
            // 2페이즈 전용 공격1 로직
        }
    }

    public class Attack2State : BossState
    {
        public Attack2State(Boss boss) : base(boss) { }

        public override void Enter()
        {
            // 2페이즈 공격2 애니메이션
        }

        public override void Update()
        {
            // 2페이즈 전용 공격2 로직
        }
    }

    public class DeathState : BossState
    {
        public DeathState(Boss boss) : base(boss) { }

        public override void Enter()
        {
            // 2페이즈 죽음 애니메이션
        }

        public override void Update()
        {
            // 2페이즈 죽음 로직
        }
    }
}