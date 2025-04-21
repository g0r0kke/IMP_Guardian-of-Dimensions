using UnityEngine;
using UnityEngine.InputSystem;

public class Boss2Test : MonoBehaviour
{
    public GameObject hobgoblinPrefab;
    public Transform player;
    public LayerMask minionLayerMask;

    public int minHobgoblins = 1;
    public int maxHobgoblins = 3;
    public float summonRadius = 3f;

    void Start()
    {
        if (hobgoblinPrefab == null)
        {
            Debug.LogError("Hobgoblin 프리팹이 할당되지 않았습니다.");
        }
    }

    void Update()
    {
        // 테스트용: S 키 누르면 소환
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            SummonHobgoblins();
        }
    }

    void SummonHobgoblins()
    {
        int count = Random.Range(minHobgoblins, maxHobgoblins + 1);

        float angleStep = 180f / (count + 1); // 예: 3마리면 45도, 90도, 135도에 위치
        Vector3 forward = transform.forward;

        for (int i = 0; i < count; i++)
        {
            float angle = -90f + angleStep * (i + 1); // -90 ~ +90 사이 각도로 분산
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 dir = rotation * forward;
            Vector3 spawnPos = transform.position + dir.normalized * summonRadius;

            int minionLayer = GetLayerFromMask(minionLayerMask);
            if (minionLayer == -1)
            {
                Debug.LogError("enemyLayerMask에 유효한 레이어가 없습니다!");
                return;
            }

            Hobgoblin.Spawner(hobgoblinPrefab, spawnPos, player,minionLayerMask);
        }
    }

    int GetLayerFromMask(LayerMask mask)
    {
        int maskValue = mask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((maskValue & (1 << i)) != 0)
                return i;
        }
        return -1;
    }
}