using UnityEngine;


public class Hobgoblin : MonoBehaviour
{
    public Animator animator;
    public Transform player;

    public IState currentState;
    public float rotationSpeed = 5f;

    public float walkSpeed = 2f;
   // public float walkRange = 3f;  
    public float detectionRange = 10f;
    public float attackRange = 1.5f; // 공격 거리
    //public float runSpeed = 3.5f;
    public int hp = 1;

    public AudioSource audioSource;

    public AudioClip goblinLaugh;   // idle
    public AudioClip goblinCackle;  // walk
    public AudioClip goblinPunch;   // attack
    public AudioClip goblinDeath;   // damage/death

    public int attackDamage = 5; // 플레이어 데미지 입히기

    void Start()
    {
        animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        ChangeState(new HobIdleState(this)); // 초기 상태
    }

    void Update()
    {
        if (currentState != null)
        {
            // 플레이어를 향해 회전
            if (player != null && !(currentState is HobDeadState))
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0; // Y축 회전만 적용

                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                }
            }

            currentState.Update();
        }
    }

    public void ChangeState(IState newState)
    {
        Debug.Log($"State changed: {currentState} → {newState}");
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void TakeHit()
    {
        if (hp <= 0) return;

        hp--;
        ChangeState(new HobDamageState(this));


        if (hp <= 0)
        {
            ChangeState(new HobDeadState(this));
        }
    }

    // 외부에서 Hobgoblin 소환활 수 있도록 Spawn 하는 함수
        public static Hobgoblin Spawner(GameObject prefab, Vector3 spawnPosition, Transform targetPlayer)
        {
        
            if (prefab == null)
            {
                Debug.LogError("Hobgoblin 프리팹 안 넣음");
                return null;
            }

            GameObject hobgoblinObj = GameObject.Instantiate(prefab, spawnPosition, Quaternion.identity);
            Hobgoblin hob = hobgoblinObj.GetComponent<Hobgoblin>();
            hob.player = targetPlayer;
            return hob;
        }

    
        public void PlayPunchSound()
        {
            if (audioSource != null && goblinPunch != null)
            {
                audioSource.PlayOneShot(goblinPunch);
            }
        }
    
}