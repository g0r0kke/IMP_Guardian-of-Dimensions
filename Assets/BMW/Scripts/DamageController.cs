using UnityEngine;

public class DamageController : MonoBehaviour
{

    [Header("플레이어 데미지 초기 요소 연결")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private AudioSource PlayerTakeDamageSound;

    [Header("플레이어 데미지 초기 세팅")]
    [SerializeField] private int collisionDamage = 2;
    [SerializeField] private int playerDamage = 0;
    [SerializeField] private float damageTime = 0.0f;

                     public bool isDefense = false;
                     public bool isAvoid = false;
    [SerializeField] private bool isConstantContact = false;

    // 외부 스크립트 연결 세팅
    private PlayerGUI playerGUI;
    private GameObject hitUI;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
        hitUI = GameObject.FindWithTag("UI_Red");
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDamage > 0)
        {
            playerGUI.decreaseHealth(playerDamage);
            playerDamage = 0;
        }
    }

    public void PlayerTakeDamage(int damageIntensity)
    {
        if (!isAvoid && !isDefense)
        {
            playerDamage += damageIntensity;
            PlayerTakeDamageSound.Play();

        }

        if (hitUI)
        {
            HitAnimationController hitAnimationController = hitUI.GetComponent<HitAnimationController>();

            if (hitAnimationController)
            {
                hitAnimationController.PlayHitAnimation();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && !isConstantContact)
        {
            if (!isAvoid && !isDefense)
            {
                playerDamage += collisionDamage;
                PlayerTakeDamageSound.Play();
            }

            isConstantContact = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && isConstantContact)
        {
            if (!isAvoid && !isDefense) damageTime += Time.deltaTime;

            if (damageTime >= 1.0f)
            {
                playerDamage += (collisionDamage - 1);
                Debug.Log("������ �����Ͽ� �������� ���̹����� �ް��ֽ��ϴ�.");
                damageTime = 0.0f;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && isConstantContact) isConstantContact = false;
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}
