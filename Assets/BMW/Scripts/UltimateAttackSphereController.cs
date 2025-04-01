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
            Debug.Log("적을 만나지 못하였지만 적의 위치까지와 삭제 되었습니다.");
        }

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            Debug.Log("구체지속시간이 초과하여 삭제되었습니다.");
            return;
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
            Debug.Log("설정 레이어 객체와 충돌하여 삭제되었습니다.");
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
