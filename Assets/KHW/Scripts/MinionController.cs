using UnityEngine;
/// Scripts created to implement Hobgoblin recall logic in proto type
public class MinionController : MonoBehaviour
{
    private Transform target;
    public float moveSpeed = 3f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float maxHP = 30f;
    private float currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SamplePlayer player = collision.gameObject.GetComponent<SamplePlayer>();
            if (player != null)
            {
                player.TakeDamage((int)damage);
            }
        }
    }

    public Transform GetTarget() { return target; }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        Debug.Log($"소환물 체력 : {currentHP}");

        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
