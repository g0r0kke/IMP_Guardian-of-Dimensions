using ManoMotion;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasicAttackController: MonoBehaviour
{

    [Header("Basic Attack Initial Element Connection Settings")]
    [SerializeField] private Transform basicAttackPos;
    [SerializeField] private GameObject basicAttackPrefab;
    [SerializeField] private Collider planeCollider;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private AudioSource basicAttackStartSound;
    [SerializeField] private AudioSource basicAttackEndSound;

    [Header("Basic Attack Initial Settings")]
    [SerializeField] private float basicAttackStartScale = 1.0f;
    [SerializeField] private float basicAttackForce = 1000.0f;
    [SerializeField] private float basicAttackDelTime = 2.0f;
    [SerializeField] private float basicAttackMaxDist = 25.0f;
                     public int GaugeIncreaseAmount = 1;
    [SerializeField] private int attackDamage = 10;
                     public float delayTime = 0f;

    // Setting up an external script connection
    private PlayerDataManager playerDataManager;
    private PlayerGUI playerGUI;
    private HandGestureController handGestureController;
    private AnimationController animationController;
    

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
        handGestureController = GetComponent<HandGestureController>();
        animationController = GetComponent<AnimationController>();

        playerDataManager = PlayerDataManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDataManager.isControlPlayer) return;
        
        bool isPressedBasicAttack = Keyboard.current.zKey.wasPressedThisFrame;

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
        attackController.Initialize(transform, basicAttackMaxDist, basicAttackDelTime, attackDamage, targetLayer, planeCollider, basicAttackEndSound);

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, basicAttackMaxDist);
    }
}