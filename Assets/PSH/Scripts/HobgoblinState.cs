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
        if (hobgoblin.audioSource.clip != hobgoblin.goblinCackle || !hobgoblin.audioSource.isPlaying)
        {
            hobgoblin.audioSource.clip = hobgoblin.goblinCackle;
            hobgoblin.audioSource.Play();
        }
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





public class HobAttackState : IState
{
    private Hobgoblin hobgoblin;
    private bool soundPlayed = false;
    private bool animationCompleted = false;
    private float attackTimer = 0f;
    private float attackDuration = 1.5f; // 공격 애니메이션  지속 시간
    private float attackCooldown = 0.7f; // 공격 사이의 간격

    private bool damageDeal = false; // 데미지 적용 여부 체크 (민우)


    public HobAttackState(Hobgoblin hobgoblin)
    {
        this.hobgoblin = hobgoblin;
    }

    public void Enter()
    {
        Debug.Log("공격 상태 진입");
        hobgoblin.animator.Play("attack02", 0, 0f); // 애니메이션을 처음부터 시작하도록 강제
        attackTimer = 0f;
        soundPlayed = false;
        damageDeal = false; // 데미지 초기화 (민우)
        animationCompleted = false;
    }

    public void Update()
    {
        attackTimer += Time.deltaTime;
        AnimatorStateInfo stateInfo = hobgoblin.animator.GetCurrentAnimatorStateInfo(0);

        // 공격 사운드 재생 (망치 휘두르는 시점)
        if (!soundPlayed && attackTimer >= 0.2f) // 시간 기준으로 사운드 재생 (더 안정적)
        {
            //hobgoblin.PlayPunchSound();
            soundPlayed = true;
            Debug.Log("공격 사운드 재생");
        }

        // 데미지 적용 시점 (공격 타이밍에 맞게) (민우)
        if (!damageDeal && attackTimer >= 0.4f) // 공격 사운드 직후 데미지 적용
        {
            hobgoblin.DealDamageToPlayer();
            damageDeal = true;
        }


        // 애니메이션 완료 여부 확인 (시간 또는 normalizedTime 기준)
        if (!animationCompleted &&
            (attackTimer >= attackDuration || stateInfo.normalizedTime >= 0.95f))
        {
            animationCompleted = true;
            Debug.Log("공격 애니메이션 완료");
        }

        // 애니메이션이 완료되고 쿨다운이 지났는지 확인
        if (animationCompleted && attackTimer >= attackDuration + attackCooldown)
        {
            float distance = Vector3.Distance(hobgoblin.transform.position, hobgoblin.player.position);
            Debug.Log($"상태 전환 고려 중. 거리: {distance}, 공격 범위: {hobgoblin.attackRange}");

            // 플레이어와의 거리에 따른 상태 전환
            if (distance <= hobgoblin.attackRange)
            {
                // 다시 공격
                hobgoblin.ChangeState(new HobAttackState(hobgoblin));
            }
            else if (distance <= hobgoblin.detectionRange)
            {
                // 플레이어가 감지 범위 내에 있음
                hobgoblin.ChangeState(new HobWalkState(hobgoblin));
            }
            else
            {
                // 플레이어가 감지 범위 밖에 있음 
                hobgoblin.ChangeState(new HobIdleState(hobgoblin));
            }
        }
    }

    public void Exit()
    {
        Debug.Log("공격 상태 종료");
    }
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