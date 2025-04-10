using UnityEngine;

public class DamageController : MonoBehaviour
{

    public LayerMask targetLayer;
    public int collisionDamage = 2;
    public int playerDamage = 0;

    private float damageTime = 0.0f;
    private bool isConstantContact = false;
    public bool isDefense = false;
    public bool isAvoid = false;

    private PlayerGUI playerGUI;

    void Start()
    {
        playerGUI = GetComponent<PlayerGUI>();
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

    void PlayerTakeDamage(int damageIntensity)
    {
        if (!isAvoid && !isDefense) playerDamage += damageIntensity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInTargetLayer(collision.gameObject) && !isConstantContact)
        {
           if(!isAvoid && !isDefense) playerDamage += collisionDamage;

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
                Debug.Log("보스와 접촉하여 지속적인 데이미지를 받고있습니다.");
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
