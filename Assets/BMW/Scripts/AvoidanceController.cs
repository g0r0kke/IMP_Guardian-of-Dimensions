using UnityEngine;
using System.Collections;

public class AvoidanceController : MonoBehaviour
{

    [Header("ȸ�� �ʱ� ��� ����")]
    [SerializeField] private LayerMask originLayer;
    [SerializeField] private LayerMask avoidanceLayer;
    [SerializeField] private AudioSource AvoidSound;
    

    [Header("ȸ�� �ʱ� ����")]
    [SerializeField] private float avoidanceDuration = 3.0f;
    [SerializeField] private float rotationThreshold = 5000.0f;
                     public float delayTime = 0f;
                     private bool isAvoiding = false;
                     private float previousRotationY;
    
    // �ܺ� ��ũ��Ʈ ���� ����
    private PlayerDataManager playerDataManager;
    private PlayerGUI playerGUI;
    private DamageController damageController;
    private CharacterController characterController;

    private Collider[] playerColliders;
    private MeshRenderer[] playerMeshRenderers;


    void Start()
    {
        playerDataManager = PlayerDataManager.Instance;
        playerGUI = GetComponent<PlayerGUI>();
        damageController = GetComponent<DamageController>();
        characterController = GetComponent<CharacterController>();

        playerColliders = GetComponentsInChildren<Collider>();
        playerMeshRenderers = GetComponentsInChildren<MeshRenderer>();

        previousRotationY = Camera.main.transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDataManager.isControlPlayer) return;

        float currentRotationY = Camera.main.transform.eulerAngles.y;
        float deltaRotation = Mathf.DeltaAngle(previousRotationY, currentRotationY);
        float rotationSpeed = Mathf.Abs(deltaRotation) / Time.deltaTime;


        if (rotationSpeed > rotationThreshold && !isAvoiding && delayTime <= 0)
        {
            StartCoroutine(Avoidance());
            delayTime = playerGUI.avoidanceSkillDelay;
        }

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }

        previousRotationY = currentRotationY;
    }

    IEnumerator Avoidance()
    {
        AvoidSound.Play();

        isAvoiding = true;

        // ��� 1 : ������ ��Ʈ�ѷ� �Ͻ��� ��Ȱ��ȭ
        //damageController.enabled = false;

        // ��� 2 :
        damageController.isAvoid =true;

        // ���� ���
        // ToggleComponents(false);
        // characterController.enabled = true;
        // gameObject.layer = avoidanceLayer;

        SetTransparency(0.1f);

        yield return new WaitForSeconds(avoidanceDuration);

        SetTransparency(1f);

        // ���ܹ��
        // gameObject.layer = originLayer;

        // ��� 2
        damageController.isAvoid = false;

        // ��� 1
        //damageController.enabled = true;

        ToggleComponents(true);

        isAvoiding = false;

    }

    void ToggleComponents(bool state)
    {
        foreach (Collider col in playerColliders)
        {
            col.enabled = state;
        }

        
        foreach (MeshRenderer mr in playerMeshRenderers)
        {
            mr.enabled = state;
        }
        
    }
    void SetTransparency(float alpha)
    {
        foreach (MeshRenderer mr in playerMeshRenderers)
        {
            Material[] materials = mr.materials;

            foreach (Material mat in materials)
            {
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;
            }
        }
    }
}
