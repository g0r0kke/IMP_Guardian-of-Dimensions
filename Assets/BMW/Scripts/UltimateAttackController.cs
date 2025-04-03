using System.Collections.Generic;
using UnityEngine;

public class UltimateAttackController : MonoBehaviour
{
    public GameObject ultimateAttackPrefab;
    public LayerMask targetLayer;
    public float ultimateAttackStartScale = 0.05f;
    public float ultimateAttackRange = 10.0f;
    public float ultimateAttackStartHeight = 10.0f;
    public float ultimateAttackStartRange = 10.0f;
    public float ultimateAttackScaleRate = 0.5f;
    public float ultimateAttackSpeed = 8.0f;
    public float ultimateAttackDelTime = 10.0f;

    public float delayTime = 0f;
    private PlayerGUI playerGUI;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        bool ultimateAttackPressed = Input.GetKeyDown(KeyCode.X);

        if (ultimateAttackPressed && delayTime <= 0 && playerGUI.currentGauge == playerGUI.ultimateAttackGauge)
        {
            UltimateAttack();
            playerGUI.currentGauge = 0;
            delayTime = playerGUI.ultimateAttackDelay;
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

        List<Vector3> enemyPositions = new List<Vector3>();
        List<Vector3> spawnPositions = new List<Vector3>();

        foreach (Collider col in hits)
        {
            enemyPositions.Add(col.transform.position);
            spawnPositions.Add(col.transform.position + new Vector3(Random.Range(-ultimateAttackStartRange, ultimateAttackStartRange),ultimateAttackStartHeight,Random.Range(-ultimateAttackStartRange, ultimateAttackStartRange)));
            
            GameObject ultimateAttackSphere = Instantiate(ultimateAttackPrefab, spawnPositions[spawnPositions.Count - 1], Quaternion.identity);
            Rigidbody rb = ultimateAttackSphere.GetComponent<Rigidbody>();

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
            attackController.Initialize(enemyPositions[enemyPositions.Count - 1], ultimateAttackDelTime, ultimateAttackSpeed, ultimateAttackStartScale, ultimateAttackScaleRate, targetLayer);

        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ultimateAttackRange);
    }
}
