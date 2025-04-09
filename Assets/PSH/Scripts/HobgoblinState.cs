    // hobgoblinState.cs
    using UnityEngine;


    public class HobIdleState : IState
    {
        private Hobgoblin hobgoblin;


        // 가만히 있기 상태
        public HobIdleState(Hobgoblin hobgoblin)
        {
            this.hobgoblin = hobgoblin;
        }

        public void Enter()
        {
            hobgoblin.animator.Play("idle01");
            //hobgoblin.audioSource.PlayOneShot(hobgoblin.goblinLaugh);
            hobgoblin.audioSource.clip = hobgoblin.goblinLaugh;
            hobgoblin.audioSource.Play();
        }

        public void Update()
        {
            float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);

            // 공격 범위보다 멀면 걷기
            if (distance > hobgoblin.attackRange && distance < hobgoblin.detectionRange)
            {
                hobgoblin.ChangeState(new HobWalkState(hobgoblin));
            }
            // 공격 범위 안에 들어오면 바로 공격
            else if (distance <= hobgoblin.attackRange)
            {
                hobgoblin.ChangeState(new HobAttackState(hobgoblin));
            }
        }

        public void Exit() { }
    }

    // 걷기 상태
    public class HobWalkState : IState
    {
        private Hobgoblin hobgoblin;

        public HobWalkState(Hobgoblin hobgoblin)
        {
            this.hobgoblin = hobgoblin;
        }

        public void Enter()
        {
            // 걷기 애니메이션 재생
            hobgoblin.animator.Play("walk");
            //hobgoblin.audioSource.PlayOneShot(hobgoblin.goblinCackle);
            hobgoblin.audioSource.clip = hobgoblin.goblinCackle;
            hobgoblin.audioSource.Play();
    }

    public void Update()
    {
        float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);


        if (distance > hobgoblin.attackRange)
        {
            hobgoblin.transform.position = Vector3.MoveTowards(
                hobgoblin.transform.position,
                hobgoblin.player.position,
                hobgoblin.walkSpeed * Time.deltaTime
            );
        }

        if (distance <= hobgoblin.attackRange)
        {
            hobgoblin.ChangeState(new HobAttackState(hobgoblin));
        }
        else if (distance > hobgoblin.detectionRange)
        {
            hobgoblin.ChangeState(new HobIdleState(hobgoblin));
        }
    }

    public void Exit() { }
    }




    // 공격 상태
    public class HobAttackState : IState
    {
        private Hobgoblin hobgoblin;

        public HobAttackState(Hobgoblin hobgoblin)
        {
            this.hobgoblin = hobgoblin;
        }

        public void Enter()
        {
            hobgoblin.animator.Play("attack02");
            hobgoblin.audioSource.PlayOneShot(hobgoblin.goblinPunch);

    }

        public void Update()
        {
            // 공격 애니메이션이 끝났는지 확인
            AnimatorStateInfo stateInfo = hobgoblin.animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("attack02") && stateInfo.normalizedTime < 1f)
            {
                return; // 애니메이션이 아직 안 끝났으면 아무 것도 하지 않음
            }

            float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);

            if (distance <= hobgoblin.attackRange)
            {
                // 반복 공격 가능하게 다시 Attack 상태로 전환
                hobgoblin.ChangeState(new HobAttackState(hobgoblin));
            }
            else
            {
                hobgoblin.ChangeState(new HobIdleState(hobgoblin));
            }
        }



    public void Exit() { }
    }

    // 피격 상태
    public class HobDamageState : IState
    {
        private Hobgoblin hobgoblin;

        public HobDamageState(Hobgoblin hobgoblin)
        {
            this.hobgoblin = hobgoblin;
        }

        public void Enter()
        {
            hobgoblin.animator.Play("damage");
            hobgoblin.audioSource.Stop(); // 이전 웃음소리 강제 멈춤
            hobgoblin.audioSource.PlayOneShot(hobgoblin.goblinDeath);
    }

        public void Update()
        {
            // 애니메이션 한 번 재생하고 다시 Idle로 돌아가기 (또는 Run)
            if (hobgoblin.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                // 여기서 상황에 맞게 Idle이나 Run으로 복귀
                float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);
                if (distance < hobgoblin.attackRange)
                    hobgoblin.ChangeState(new HobAttackState(hobgoblin));
                else
                    hobgoblin.ChangeState(new HobWalkState(hobgoblin));
            }
        }

        public void Exit() { }
    }


    // 죽음 상태
    public class HobDeadState : IState
    {
        private Hobgoblin hobgoblin;

        public HobDeadState(Hobgoblin hobgoblin)
        {
            this.hobgoblin = hobgoblin;
        }

        public void Enter() 
        {
            hobgoblin.animator.Play("dead");
            
            GameObject.Destroy(hobgoblin.gameObject, 2f);
        }

        public void Update() { }

        public void Exit() { }
    }