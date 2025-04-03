using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class BasicAttackController: MonoBehaviour
{
    public Transform basicAttackPos;
    public GameObject basicAttackPrefab;
    public Collider planeCollider;
    public LayerMask targetLayer;
    public float basicAttackStartScale = 0.5f;
    public float basicAttackForce = 1000.0f;
    public float basicAttackDelTime = 2.0f;
    public float basicAttackMaxDist = 30.0f;
    public int GaugeIncreaseAmount = 1;

    public float delayTime = 0f;
    private PlayerGUI playerGUI;
    private BasicAttackController basicAttackController;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
        basicAttackController = GetComponent<BasicAttackController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool basicAttackPressed = Input.GetKeyDown(KeyCode.Z);

        if (basicAttackPressed && delayTime <= 0)
        {
            BasicAttack();
            delayTime = playerGUI.basicAttackDelay;
        }

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }
    }

    void BasicAttack()
    {
        Vector3 position = (basicAttackPos != null ? basicAttackPos.position : transform.position) + transform.forward * 0.5f;

        GameObject basicAttackSphere = Instantiate(basicAttackPrefab, position, Quaternion.identity);
        
        basicAttackSphere.transform.localScale = Vector3.one * basicAttackStartScale;

        Rigidbody rb = basicAttackSphere.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = basicAttackSphere.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;

        Vector3 directionToCameraCenter = (transform.GetChild(0).position + transform.GetChild(0).forward * basicAttackMaxDist) - basicAttackPos.position;
        directionToCameraCenter.Normalize();

        rb.AddForce(directionToCameraCenter * basicAttackForce);

        BasicAttackSphereController attackController = basicAttackSphere.GetComponent<BasicAttackSphereController>();
        if (attackController == null)
        {
            attackController = basicAttackSphere.AddComponent<BasicAttackSphereController>();
        }
        attackController.Initialize(transform, basicAttackMaxDist, basicAttackDelTime, targetLayer, GetComponent<PlayerGUI>(), GetComponent<BasicAttackController>(), planeCollider);

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, basicAttackMaxDist);
    }
}