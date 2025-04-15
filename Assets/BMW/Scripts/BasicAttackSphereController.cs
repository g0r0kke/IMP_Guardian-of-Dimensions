using UnityEngine;

public class BasicAttackSphereController : MonoBehaviour
{

    private Transform player;
    [SerializeField]
    private GameObject collisionEffectPrefab;
    private float maxDistance = 10.0f;
    private float lifeTime = 10.0f;
    private float timer = 0f;
    private AudioSource basicAttackEndSound;
    public int attackDamage;
    private LayerMask targetLayer;
    private bool isInitialized = false;

    private PlayerGUI playerGUI;
    private BasicAttackController basicAttackController;

    [Header("���� Ÿ��")]
    private Boss bossTarget;
    private Hobgoblin summonTarget;

    public void Initialize(Transform playerTransform, float maxDist, float lifetime, int attDamage, LayerMask tarLayer, PlayerGUI GUI, BasicAttackController controller, Collider planeCollider, AudioSource audioSource)
    {
        player = playerTransform;
        maxDistance = maxDist;
        lifeTime = lifetime;
        attackDamage = attDamage;
        targetLayer = tarLayer;
        isInitialized = true;
        playerGUI = GUI;
        basicAttackController = controller;
        basicAttackEndSound = audioSource;

        Collider AttackCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(AttackCollider, planeCollider);
    }
    private void Start()
    {
        GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
        if (enemyObject)
        {
            // ���� Boss ������Ʈ ã��
            bossTarget = enemyObject.GetComponent<Boss>();

            // Boss ������Ʈ�� ������ Hobgoblin ������Ʈ ã��
            if (!bossTarget)
            {
                summonTarget = enemyObject.GetComponent<Hobgoblin>();
                if (!summonTarget)
                {
                    Debug.LogWarning("Enemy �±׸� ���� ������Ʈ���� Boss�� Hobgoblin ������Ʈ�� ã�� �� �����ϴ�.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Enemy �±׸� ���� ���� ������Ʈ�� ã�� �� �����ϴ�.");
        }
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

        if (IsInTargetLayer(collision.gameObject) || collision.gameObject.CompareTag("Enemy"))
        {

            GameObject collisionEffect = Instantiate(collisionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            // ���� �ʿ�
            Vector3 effectCircleScale = collisionEffect.transform.Find("Circle").localScale;
            effectCircleScale = new Vector3(effectCircleScale.x * 10, effectCircleScale.y * 10, effectCircleScale.z * 10);
            Vector3 effectFireScale = collisionEffect.transform.Find("Fire").localScale;
            effectFireScale = new Vector3(effectFireScale.x * 10, effectFireScale.y * 10, effectFireScale.z * 10);

            Destroy(collisionEffect, 1f);

            Destroy(gameObject);
            playerGUI.IncreaseGauge(basicAttackController.GaugeIncreaseAmount);

            if (bossTarget)
            {
                bossTarget.TakeDamage(attackDamage);
                Debug.Log($"�÷��̾ �������� {attackDamage} �������� �������ϴ�!");
            }
            else if (summonTarget)
            {
                summonTarget.TakeDamage(attackDamage);
                Debug.Log($"�÷��̾ ��ȯ������ {attackDamage} �������� �������ϴ�!");
            }

            basicAttackEndSound.Play();
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}