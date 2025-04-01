using UnityEngine;

public class UltimateAttackSphereController : MonoBehaviour
{
    private Vector3 targetPosition;
    private LayerMask targetLayer;
    private float speed = 15.0f;
    public float startScale = 0.5f;
    public float scaleRate = 0.5f;
    private float lifeTime = 10.0f;
    private float timer = 0f;
    private bool isInitialized = false;

    public void Initialize(Vector3 enemyPositions, float lifetime, float ultimateAttackSpeed, float StarScale, float scaleRat, LayerMask tarLayer)
    {
        targetPosition = enemyPositions;
        speed = ultimateAttackSpeed;
        lifeTime = lifetime;
        targetLayer = tarLayer;
        startScale = StarScale;
        scaleRate = scaleRat;
        isInitialized = true;

        transform.localScale = Vector3.one * startScale;
        StartCoroutine(ContinuousScaling());
    }

    void Update()
    {
        if (!isInitialized) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (transform.position.y <= targetPosition.y)
        {
            Destroy(gameObject);
            Debug.Log("���� ������ ���Ͽ����� ���� ��ġ������ ���� �Ǿ����ϴ�.");
        }

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            Debug.Log("��ü���ӽð��� �ʰ��Ͽ� �����Ǿ����ϴ�.");
            return;
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
            Debug.Log("���� ���̾� ��ü�� �浹�Ͽ� �����Ǿ����ϴ�.");
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }

    private System.Collections.IEnumerator ContinuousScaling()
    {
        while (true)
        {
            
            float scaleMultiplier = 1 + (scaleRate * Time.deltaTime);
            transform.localScale *= scaleMultiplier;
            yield return null;
        }
    }
}
