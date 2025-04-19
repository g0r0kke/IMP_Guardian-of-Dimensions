using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UltimateAttackController : MonoBehaviour
{
    
    [Header("궁극기 초기 요소 연결")]
    [SerializeField] private GameObject ultimateAttackPrefab;
    [SerializeField] private GameObject ultimateAttackBeforeEffectPrefab;
    [SerializeField] private Collider planeCollider;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private AudioSource ultimateAttackStartSound;
    [SerializeField] private AudioSource ultimateAttackEndSound;

    [Header("궁극기 초기 세팅")]
    [SerializeField] private float ultimateAttackStartScale = 0.03f;
    [SerializeField] private float ultimateAttackRange = 10.0f;
    [SerializeField] private float ultimateAttackStartHeight = 10.0f;
    [SerializeField] private float ultimateAttackStartRange = 50.0f;
    [SerializeField] private float ultimateAttackScaleRate = 0.5f;
    [SerializeField] private float ultimateAttackSpeed = 8.0f;
    [SerializeField] private float ultimateAttackDelTime = 10.0f;
    [SerializeField] private int attackDamage = 30;
                     public float delayTime = 0f;
                     private bool isEnemyCheck = false;

    // 외부 스크립트 연결 세팅
    private PlayerDataManager playerDataManager;
    private PlayerGUI playerGUI;
    private HandGestureController handGestureController;
    private AnimationController animationController;

    void Start()
    {
        playerDataManager = PlayerDataManager.Instance;
        playerGUI = GetComponent<PlayerGUI>();
        handGestureController = GetComponent<HandGestureController>();
        animationController = GetComponent<AnimationController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDataManager.isControlPlayer) return;

        bool isPressedUltimateAttack = Input.GetKeyDown(KeyCode.X);

        if ((isPressedUltimateAttack || handGestureController.isUltimateAttackGesture) && delayTime <= 0 && playerGUI.ultimateAttackGauge == playerGUI.ultimateAttackGaugeLimit)
        {
            handGestureController.isUltimateAttackGesture = false;
            isPressedUltimateAttack = false;

            if (!isEnemyCheck)
            {
                isEnemyCheck = true;
                animationController.UltimateAttackAnimation();
                Invoke("UltimateAttack", 0.7f);
            }
        }
        else {
            handGestureController.isUltimateAttackGesture = false;
        }

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }

    }

    void UltimateAttack()
    {

        Vector3 center = transform.position;
        Collider[] hits = Physics.OverlapSphere(center, ultimateAttackRange, targetLayer);

        if (hits == null || hits.Length == 0)
        {
            Debug.Log("공격 범위에 적이 존재하지 않습니다");
            return;

        }

        ultimateAttackStartSound.Play();

        playerGUI.ultimateAttackGauge = 0;
        delayTime = playerGUI.ultimateAttackDelay;
        isEnemyCheck = false;

        List<Transform> enemyTransforms = new List<Transform>();
        List<Vector3> spawnPositions = new List<Vector3>();
        List<Vector3> enemyGroundPositions = new List<Vector3>();

        foreach (Collider col in hits)
        {
            enemyTransforms.Add(col.transform);
            spawnPositions.Add(col.transform.position + new Vector3(Random.Range(-ultimateAttackStartRange, ultimateAttackStartRange),ultimateAttackStartHeight,Random.Range(-ultimateAttackStartRange, ultimateAttackStartRange)));

            Ray ray = new Ray(col.transform.position + Vector3.up * 0.1f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000) && hit.collider == planeCollider) enemyGroundPositions.Add(new Vector3(col.transform.position.x, hit.point.y, col.transform.position.z));
            else enemyGroundPositions.Add(col.transform.position);

            GameObject ultimateAttackSphere = Instantiate(ultimateAttackPrefab, spawnPositions[spawnPositions.Count - 1], Quaternion.identity);
            Rigidbody rb = ultimateAttackSphere.GetComponent<Rigidbody>();

            GameObject ultimateAttackBeforeEffect = Instantiate(ultimateAttackBeforeEffectPrefab, enemyGroundPositions[enemyGroundPositions.Count - 1], Quaternion.identity);
            ultimateAttackBeforeEffect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Destroy(ultimateAttackBeforeEffect, 1.5f);

            if (rb == null)
            {
                rb = ultimateAttackSphere.AddComponent<Rigidbody>();
            }

            rb.useGravity = false;

            UltimateAttackSphereController attackController = ultimateAttackSphere.GetComponent<UltimateAttackSphereController>();
            if (attackController == null)
            {
                attackController = ultimateAttackSphere.AddComponent<UltimateAttackSphereController>();
            }
            attackController.Initialize(enemyTransforms[enemyTransforms.Count - 1], ultimateAttackDelTime, ultimateAttackSpeed, ultimateAttackStartScale, ultimateAttackScaleRate, attackDamage, targetLayer, planeCollider, ultimateAttackEndSound);

        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ultimateAttackRange);
    }
}
