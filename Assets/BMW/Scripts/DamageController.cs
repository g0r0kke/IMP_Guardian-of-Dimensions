using UnityEngine;

public class DamageController : MonoBehaviour
{

    public LayerMask targetLayer;
    public int collisionDamage = 2;
    public int playerDamage = 0;

    private float damageTime = 0.0f;
    private bool isConstantContact = false;
    private bool isAttacked = false;
    public bool isAvoid = false;

    private PlayerGUI playerGUI;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        // 적과 상호작용 필요, 공격기에 따라 피해량 수정 필요
        if (isAttacked && !isAvoid)
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
        // 수정 필요
        playerDamage += damageIntensity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && !isConstantContact)
        {
           if(!isAvoid) playerDamage += collisionDamage;

            isConstantContact = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && isConstantContact && !isAvoid)
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
        if (IsInTargetLayer(collision.gameObject) && isConstantContact) isConstantContact = false;
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}
