using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UltimateAttackController : MonoBehaviour
{
    public GameObject ultimateAttackPrefab;
    public GameObject ultimateAttackBeforeEffectPrefab;
    public Collider planeCollider;
    public LayerMask targetLayer;
    public AudioSource ultimateAttackStartSound;
    public AudioSource ultimateAttackEndSound;

    public float ultimateAttackStartScale = 0.03f;
    public float ultimateAttackRange = 10.0f;
    public float ultimateAttackStartHeight = 10.0f;
    public float ultimateAttackStartRange = 10.0f;
    public float ultimateAttackScaleRate = 0.5f;
    public float ultimateAttackSpeed = 8.0f;
    public float ultimateAttackDelTime = 10.0f;
    public int attackDamage = 30;

    public float delayTime = 0f;

    private PlayerGUI playerGUI;
    private HandGestureController handGestureController;
    private AnimationController animationController;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
        handGestureController = GetComponent<HandGestureController>();
        animationController = GetComponent<AnimationController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isPressedUltimateAttack = Input.GetKeyDown(KeyCode.X);

        if ((isPressedUltimateAttack || handGestureController.isUltimateAttackGesture) && delayTime <= 0 && playerGUI.ultimateAttackGauge == playerGUI.ultimateAttackGaugeLimit)
        {
            animationController.UltimateAttackAnimation();
            Invoke("UltimateAttack", 0.7f);
        }
        
        handGestureController.isUltimateAttackGesture = false;

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

        List<Vector3> enemyPositions = new List<Vector3>();
        List<Vector3> spawnPositions = new List<Vector3>();
        List<Vector3> enemyGroundPositions = new List<Vector3>();

        foreach (Collider col in hits)
        {
            enemyPositions.Add(col.transform.position);
            spawnPositions.Add(col.transform.position + new Vector3(Random.Range(-ultimateAttackStartRange, ultimateAttackStartRange),ultimateAttackStartHeight,Random.Range(-ultimateAttackStartRange, ultimateAttackStartRange)));

            Ray ray = new Ray(col.transform.position + Vector3.up * 0.1f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000) && hit.collider == planeCollider) enemyGroundPositions.Add(new Vector3(col.transform.position.x, hit.point.y, col.transform.position.z));
        

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
            attackController.Initialize(enemyPositions[enemyPositions.Count - 1], ultimateAttackDelTime, ultimateAttackSpeed, ultimateAttackStartScale, ultimateAttackScaleRate, attackDamage, targetLayer, planeCollider, ultimateAttackEndSound);

        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ultimateAttackRange);
    }
}
