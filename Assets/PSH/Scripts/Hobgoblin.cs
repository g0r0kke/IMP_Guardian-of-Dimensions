using UnityEngine;


public class Hobgoblin : MonoBehaviour
{
    public Animator animator;
    public Transform player;

    private IState currentState;
    public float rotationSpeed = 5f;

    public float walkSpeed = 2f;
   // public float walkRange = 3f;  
    public float detectionRange = 10f;
    public float attackRange = 1.5f; // 공격 거리
    //public float runSpeed = 3.5f;
    public int hp = 1;



    void Start()
    {
        animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        ChangeState(new HobIdleState(this)); // 초기 상태
    }

    void Update()
    {
        currentState?.Update();
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
            ChangeState(new DeadState(this));
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

    }