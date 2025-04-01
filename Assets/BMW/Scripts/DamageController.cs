using UnityEngine;

public class DamageController : MonoBehaviour
{

    public LayerMask targetLayer;
    public int collisionDamage = 2;
    public int playerDamage = 0;

    private float damageTime = 0.0f;
    private bool constantContact = false;
    private bool isAttacked = false;

    PlayerGUI playerGUI;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        // ���� ��ȣ�ۿ� �ʿ�, ���ݱ⿡ ���� ���ط� ���� �ʿ�
        if (isAttacked)
        {
            Damage(2);
        }
        
        if (playerDamage > 0)
        {
            playerGUI.decreaseHealth(playerDamage);
            playerDamage = 0;
        }


    }

    void Damage(int damageIntensity)
    {
        // ���� �ʿ�
        playerDamage += damageIntensity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && !constantContact)
        {
            playerDamage += collisionDamage;

            constantContact = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && constantContact)
        {
            damageTime += Time.deltaTime;

            if (damageTime >= 1.0f)
            {
                playerDamage += (collisionDamage - 1);
                Debug.Log("I am constantly being damaged by the " + collision.gameObject.name + "\n" + " playerDamage:" + playerDamage);
                damageTime = 0.0f;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && constantContact) constantContact = false;
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}
