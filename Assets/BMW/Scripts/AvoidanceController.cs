using UnityEngine;
using System.Collections;

public class AvoidanceController : MonoBehaviour
{

    [Header("회피 초기 요소 연결")]
    [SerializeField] private LayerMask originLayer;
    [SerializeField] private LayerMask avoidanceLayer;
    [SerializeField] private AudioSource AvoidSound;
    

    [Header("회피 초기 세팅")]
    [SerializeField] private float avoidanceDuration = 3.0f;
    [SerializeField] private float rotationThreshold = 5000.0f;
                     public float delayTime = 0f;
                     private bool isAvoiding = false;
                     private float previousRotationY;
    
    // 외부 스크립트 연결 세팅
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

        // 방안 1 : 데미지 컨트롤러 일시적 비활성화
        //damageController.enabled = false;

        // 방안 2 :
        damageController.isAvoid =true;

        // 예외 방법
        // ToggleComponents(false);
        // characterController.enabled = true;
        // gameObject.layer = avoidanceLayer;

        SetTransparency(0.1f);

        yield return new WaitForSeconds(avoidanceDuration);

        SetTransparency(1f);

        // 예외방법
        // gameObject.layer = originLayer;

        // 방안 2
        damageController.isAvoid = false;

        // 방안 1
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
