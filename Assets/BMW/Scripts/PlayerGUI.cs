using UnityEngine;

public class PlayerGUI : MonoBehaviour
{
    public int playerHealth = 50;
    public int playerHealthLimit = 50;
    public int ultimateAttackGauge = 10;

    public float basicAttackDelay = 0.6f;
    public float ultimateAttackDelay = 1.0f;
    public float avoidanceSkillDelay = 10.0f;
    public float defenseSkillDelay = 6.0f;
    public float healingSkillDelay = 10.0f;
    public bool debug = true;

    private BasicAttackController basicAttackController;
    private UltimateAttackController ultimateAttackController;
    private DefenseController defenseController;
    private AvoidanceController avoidanceController;
    private HealingController healingController;
    private DamageController damageController;
    private AnimationController animationController;

    private float basicAttackDelayTime;
    private float ultimateAttackDelayTime;
    private float avoidanceSkillDelayTime;
    private float defenseSkillDelayTime;
    private float healingSkillDelayTime;

    private int previousHealth = 0;
    private int previousGauge = 0;
    public int currentGauge = 0;


    void Start()
    {
        basicAttackController = GetComponent<BasicAttackController>();
        ultimateAttackController = GetComponent<UltimateAttackController>();
        defenseController = GetComponent<DefenseController>();
        avoidanceController = GetComponent<AvoidanceController>();
        healingController = GetComponent<HealingController>();
        damageController = GetComponent<DamageController>();
        animationController = GetComponent<AnimationController>();
    }

    // Update is called once per frame
    void Update()
    {

        if (playerHealth <= 0)
        {
            animationController.DieAnimation();
            GameOver();
        }
        /*if ()
        {
            animationController.VictoryAnimation();
            Victory();
        }*/

        if (debug)
        {
            DEBUG();
        }

        basicAttackDelayTime = basicAttackController.delayTime;
        ultimateAttackDelayTime = ultimateAttackController.delayTime;
        defenseSkillDelayTime = defenseController.delayTime;
        avoidanceSkillDelayTime = avoidanceController.delayTime;
        healingSkillDelayTime = healingController.delayTime;
    }

    public void IncreaseGauge(int GaugeIncreaseAmount)
    {
        if(currentGauge < ultimateAttackGauge) currentGauge += GaugeIncreaseAmount;
    }

    public void IncreaseHealth(int HealingAmount)
    {
        
        if (playerHealthLimit < playerHealth + HealingAmount) playerHealth = playerHealthLimit;
        else playerHealth += HealingAmount;
    }

    public void decreaseHealth(int damageAmount)
    {
        playerHealth -= damageAmount;
    }

    void GameOver()
    {
        Debug.Log("Game Over");
    }

    void Victory()
    {
        Debug.Log("Victory");
    }

    public void DEBUG()
    {
        if (previousHealth != playerHealth)
        {
            Debug.Log("Player Health:" + playerHealth);
            previousHealth = playerHealth;
        }
        if (previousGauge != currentGauge)
        {
            Debug.Log("currentGauge: " + currentGauge);
            if (currentGauge >= ultimateAttackGauge) Debug.Log("Ultimate skill available");
            previousGauge = currentGauge;
        }
        Debug.Log("basicAttackDelay:" + basicAttackDelayTime + "\n" +
                  "ultimateAttackDelay:" + ultimateAttackDelayTime + "\n" +
                  "avoidanceSkillDelay:" + avoidanceSkillDelayTime + "\n" +
                  "defenseSkillDelay:" + defenseSkillDelayTime + "\n" +
                  "healingSkillDelay:" + healingSkillDelayTime);
    }
}
