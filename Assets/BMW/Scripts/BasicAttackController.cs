using ManoMotion;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class BasicAttackController: MonoBehaviour
{
    public Transform basicAttackPos;
    public GameObject basicAttackPrefab;
    public Collider planeCollider;
    public LayerMask targetLayer;
    public AudioSource basicAttackStartSound;
    public AudioSource basicAttackEndSound;
    public float basicAttackStartScale = 1.0f;
    public float basicAttackForce = 1000.0f;
    public float basicAttackDelTime = 2.0f;
    public float basicAttackMaxDist = 30.0f;
    public int GaugeIncreaseAmount = 1;
    public int attackDamage = 10;

    public float delayTime = 0f;
    private PlayerGUI playerGUI;
    private HandGestureController handGestureController;
    private BasicAttackController basicAttackController;
    private AnimationController animationController;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
        handGestureController = GetComponent<HandGestureController>();
        basicAttackController = GetComponent<BasicAttackController>();
        animationController = GetComponent<AnimationController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isPressedBasicAttack = Input.GetKeyDown(KeyCode.Z);

        if ((isPressedBasicAttack || handGestureController.isBasicAttackGesture ) && delayTime <= 0)
        {
            animationController.BasicAttackAnimation();
            Invoke("BasicAttack", 0.5f);
            delayTime = playerGUI.basicAttackDelay;
        }
        handGestureController.isBasicAttackGesture = false;

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }
    }

    void BasicAttack()
    {
        basicAttackStartSound.Play();

        Vector3 position = (basicAttackPos != null ? basicAttackPos.position : transform.position) + transform.forward * 0.5f;

        GameObject basicAttackSphere = Instantiate(basicAttackPrefab, position, Quaternion.identity);
        
        basicAttackSphere.transform.localScale = Vector3.one * basicAttackStartScale;

        Rigidbody rb = basicAttackSphere.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = basicAttackSphere.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;

        Transform cameraView = transform.parent;
        Vector3 directionToCameraCenter = (cameraView.position + cameraView.forward * basicAttackMaxDist) - basicAttackPos.position;
        directionToCameraCenter.Normalize();

        rb.AddForce(directionToCameraCenter * basicAttackForce);

        BasicAttackSphereController attackController = basicAttackSphere.GetComponent<BasicAttackSphereController>();
        if (attackController == null)
        {
            attackController = basicAttackSphere.AddComponent<BasicAttackSphereController>();
        }
        attackController.Initialize(transform, basicAttackMaxDist, basicAttackDelTime, attackDamage, targetLayer, GetComponent<PlayerGUI>(), GetComponent<BasicAttackController>(), planeCollider, basicAttackEndSound);

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, basicAttackMaxDist);
    }
}