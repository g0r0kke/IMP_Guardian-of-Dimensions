using UnityEngine;
using System.Collections;

public class DefenseController : MonoBehaviour
{

    [Header("방어스킬 초기 요소 연결")]
    [SerializeField] private GameObject ShieldPrefab;
    [SerializeField] private AudioSource ShieldSound;
                     private GameObject shieldSphere;

    [Header("방어스킬 초기 세팅")]
    [SerializeField] private Vector3 shieldScale = new Vector3(1.0f, 1.0f, 1.0f);
    [SerializeField] private float shieldDuration = 4.0f;
    [SerializeField] private float shieldDistance = 1.0f;
                     public float delayTime = 0f;

    // 외부 스크립트 연결 세팅
    private PlayerDataManager playerDataManager;
    private PlayerGUI playerGUI;
    private HandGestureController handGestureController;
    private DamageController damageController;
    private AnimationController animationController;

    void Start()
    {
        playerDataManager = PlayerDataManager.Instance;
        playerGUI = GetComponent<PlayerGUI>();
        handGestureController = GetComponent<HandGestureController>();
        damageController = GetComponent<DamageController>();
        animationController = GetComponent<AnimationController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDataManager.isControlPlayer) return;

        bool isPressedDefense = Input.GetKeyDown(KeyCode.C);

        if ((isPressedDefense || handGestureController.isDefenseGesture) && delayTime <= 0)
        {
            animationController.DefenseAnimation();
            StartCoroutine(Defense());
            delayTime = playerGUI.defenseSkillDelay;
        }
        handGestureController.isDefenseGesture = false;

        if (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            if (delayTime <= 0) delayTime = 0f;
        }
    }

    IEnumerator Defense()
    {
        yield return new WaitForSeconds(1.1f);

        if (shieldSphere != null)
        {
            Destroy(shieldSphere);
        }

        ShieldSound.Play();

        damageController.isDefense = true;

        //Vector3 shieldPosition = transform.position + transform.forward * shieldDistance;

        shieldSphere = Instantiate(ShieldPrefab, transform.position, transform.rotation);

        shieldSphere.transform.localScale = shieldScale;

        shieldSphere.transform.parent = transform;

        yield return new WaitForSeconds(shieldDuration);

        Destroy(shieldSphere);

        damageController.isDefense = false;
    }
}
