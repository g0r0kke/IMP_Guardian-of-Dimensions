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
            Debug.Log("��ü���ӽð��� �ʰ��Ͽ� �����Ǿ����ϴ�.");
            return;
        }

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > maxDistance)
            {
                Destroy(gameObject);
                Debug.Log("��ü�ִ�Ÿ����� �ʰ��Ͽ� �����Ǿ����ϴ�.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
  
        /*
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            Debug.Log("���� �浹�Ͽ� �����Ǿ����ϴ�.");
        }
        */

        if (IsInTargetLayer(collision.gameObject))
        {
            Destroy(gameObject);
            playerGUI.IncreaseGauge(basicAttackController.GaugeIncreaseAmount);
            Debug.Log("���� ���̾� ��ü�� �浹�Ͽ� �����Ǿ����ϴ�.");
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}