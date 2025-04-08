using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    public float distancePerFrame = 2.0f;
    private Vector2 movement;

    [Header("플레이어 상태")] public int maxHealth = 100;
    public int currentHealth;

    [Header("공격 설정")] public int attackDamage = 10;

    [Header("보스 타겟")] private Boss bossTarget;

    void Start()
    {
        currentHealth = maxHealth;

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
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {

            Debug.Log("Q 키 눌림!");

            // 보스 공격
            if (bossTarget != null)
            {
                bossTarget.TakeDamage(attackDamage);
                Debug.Log($"플레이어가 보스에게 {attackDamage} 데미지를 입혔습니다!");
            }

            // Hobgoblin 공격 (범위 내)
            GameObject[] hobgoblins = GameObject.FindGameObjectsWithTag("Hobgoblin");
            Debug.Log($"Hobgoblin 감지 수: {hobgoblins.Length}");


            foreach (GameObject hob in hobgoblins)
            {
                float distance = Vector3.Distance(transform.position, hob.transform.position);
                Debug.Log($"Hobgoblin 거리: {distance}");

                if (distance < 4f)
                {
                    Hobgoblin hobScript = hob.GetComponent<Hobgoblin>();
                    if (hobScript != null)
                    {
                        hobScript.TakeHit();
                        Debug.Log($"플레이어가 Hobgoblin을 공격했습니다!");
                    }
                    else
                    {
                        Debug.LogWarning("Hobgoblin 스크립트를 찾을 수 없음");
                    }
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"플레이어 체력: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
        // 필요시 비활성화
        // gameObject.SetActive(false);
    }
}