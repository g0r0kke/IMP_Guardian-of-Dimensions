using UnityEngine;
using UnityEngine.InputSystem;

public class SamplePlayer : MonoBehaviour
{
    public float distancePerFrame = 2.0f;
    private Vector2 movement;

    [Header("플레이어 상태")] public int maxHealth = 100;
    public int currentHealth;

    [Header("공격 설정")] public int attackDamage = 10;

    [Header("보스 타겟")] private Boss bossTarget;

    [Header("소환물 타겟")] private MinionController minionTarget;

    void Start()
    {
        // 초기 체력 설정
        currentHealth = maxHealth;

        // Enemy 태그를 가진 게임 오브젝트 찾기
        GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObject != null)
        {
            bossTarget = enemyObject.GetComponent<Boss>();
            if (bossTarget == null)
            {
                Debug.LogWarning("Enemy 태그를 가진 오브젝트에서 Boss 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("Enemy 태그를 가진 게임 오브젝트를 찾을 수 없습니다.");
        }

        GameObject summonObject = GameObject.FindGameObjectWithTag("Summon");
        if (summonObject != null)
        {
            minionTarget = summonObject.GetComponent<MinionController>();
            if (minionTarget == null)
            {
                Debug.LogWarning("Summon 태그를 가진 오브젝트에서 Minion 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("Summon 태그를 가진 게임 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
    }

    void HandleMovement()
    {
        if (Keyboard.current != null)
        {
            movement = new Vector2(
                Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0
            );

            transform.Translate(new Vector3(movement.x, 0, movement.y) * distancePerFrame * Time.deltaTime);
        }
    }

    void HandleAttack()
    {
        // Q키를 눌러 공격
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (bossTarget != null)
            {
                // 보스에게 데미지 주기
                bossTarget.TakeDamage(attackDamage);

                Debug.Log($"플레이어가 보스에게 {attackDamage} 데미지를 입혔습니다!");
            }
            else
            {
                Debug.LogWarning("보스 타겟이 설정되지 않았습니다!");
                // Enemy 태그로 다시 찾아보기
                GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
                if (enemyObject != null)
                {
                    bossTarget = enemyObject.GetComponent<Boss>();
                }
            }

            if (minionTarget != null)
            {
            // 소환물에게 데미지 주기
                minionTarget.TakeDamage(attackDamage);
                Debug.Log($"플레이어가 소환물에게 {attackDamage} 데미지를 입혔습니다!");
            }
            else
            {
                Debug.LogWarning("소환물 타겟이 설정되지 않았습니다!");
    
                // Summon 태그로 다시 찾아보기
                GameObject summonObject = GameObject.FindGameObjectWithTag("Summon");
                if (summonObject != null)
                {
                    minionTarget = summonObject.GetComponent<MinionController>();
                }
            }
        }
    }

    // 플레이어가 데미지를 받는 메서드
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 현재 체력 로그 출력
        Debug.Log($"플레이어 체력: {currentHealth}/{maxHealth}");

        // 체력이 0 이하면 사망 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 사망 처리 메서드
    private void Die()
    {
        Debug.Log("플레이어 사망!");
        // 사망 처리 로직 (선택 사항)
        // gameObject.SetActive(false);
    }
}