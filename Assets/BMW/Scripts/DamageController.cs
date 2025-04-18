using UnityEngine;

public class DamageController : MonoBehaviour
{

    [Header("������ �ʱ� ��� ����")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private AudioSource PlayerTakeDamageSound;

    [Header("������ �ʱ� ����")]
    [SerializeField] private int collisionDamage = 2;
    [SerializeField] private int playerDamage = 0;
    [SerializeField] private float damageTime = 0.0f;

                     public bool isDefense = false;
                     public bool isAvoid = false;
    [SerializeField] private bool isConstantContact = false;

    // �ܺ� ��ũ��Ʈ ���� ����
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
