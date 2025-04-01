using UnityEngine;

public class BasicAttackSphereController : MonoBehaviour
{

    private Transform player;
    private float maxDistance = 10.0f;
    private float lifeTime = 10.0f;
    private float timer = 0f;
    private LayerMask targetLayer;
    private bool isInitialized = false;

    private PlayerGUI playerGUI;
    private BasicAttackController basicAttackController;

    public void Initialize(Transform playerTransform, float maxDist, float lifetime, LayerMask tarLayer, PlayerGUI GUI, BasicAttackController controller)
    {
        player = playerTransform;
        maxDistance = maxDist;
        lifeTime = lifetime;
        targetLayer = tarLayer;
        isInitialized = true;
        playerGUI = GUI;
        basicAttackController = controller;
    }
 
        void Update()
    {
        if (!isInitialized) return;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            Debug.Log("구체지속시간이 초과하여 삭제되었습니다.");
            return;
        }

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > maxDistance)
            {
                Destroy(gameObject);
                Debug.Log("구체최대거리보다 초과하여 삭제되었습니다.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
  
        /*
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            Debug.Log("적과 충돌하여 삭제되었습니다.");
        }
        */

        if (IsInTargetLayer(collision.gameObject))
        {
            Destroy(gameObject);
            playerGUI.IncreaseGauge(basicAttackController.GaugeIncreaseAmount);
            Debug.Log("설정 레이어 객체와 충돌하여 삭제되었습니다.");
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}