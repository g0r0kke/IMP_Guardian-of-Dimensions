using UnityEngine;

public class BasicAttackSphereController : MonoBehaviour
{

    private Transform player;
    [SerializeField]
    private GameObject collisionEffectPrefab;
    private float maxDistance = 10.0f;
    private float lifeTime = 10.0f;
    private float timer = 0f;
    private LayerMask targetLayer;
    private bool isInitialized = false;

    private PlayerGUI playerGUI;
    private BasicAttackController basicAttackController;

    public void Initialize(Transform playerTransform, float maxDist, float lifetime, LayerMask tarLayer, PlayerGUI GUI, BasicAttackController controller, Collider planeCollider)
    {
        player = playerTransform;
        maxDistance = maxDist;
        lifeTime = lifetime;
        targetLayer = tarLayer;
        isInitialized = true;
        playerGUI = GUI;
        basicAttackController = controller;

        Collider AttackCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(AttackCollider, planeCollider);
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

            GameObject collisionEffect = Instantiate(collisionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            Vector3 effectCircleScale = collisionEffect.transform.Find("Circle").localScale;
            effectCircleScale = new Vector3(effectCircleScale.x * 10, effectCircleScale.y * 10, effectCircleScale.z * 10);
            Vector3 effectFireScale = collisionEffect.transform.Find("Fire").localScale;
            effectFireScale = new Vector3(effectFireScale.x * 10, effectFireScale.y * 10, effectFireScale.z * 10);
            Destroy(collisionEffect, 1f);

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