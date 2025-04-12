using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGUI : MonoBehaviour
{
    public int playerHealthLimit;
    public int playerHealth;
    private int previousHealth;
    public Slider PlayerHpBar;
    public TextMeshProUGUI PlayerHpText;

    public int ultimateAttackGaugeLimit;
    public int ultimateAttackGauge;
    private int previousGauge;
    public Slider PlayerGaugeBar;
    public TextMeshProUGUI PlayerGaugeText;

    public float basicAttackDelay = 0.6f;
    public Slider PlayerBasicAttackGlobe;
    public TextMeshProUGUI PlayerBasicAttackText;

    public float ultimateAttackDelay = 6.0f;
    public Slider PlayerUltimateAttackGlobe;
    public TextMeshProUGUI PlayerUltimateAttackText;

    public float defenseSkillDelay = 6.0f;
    public Slider PlayerShildGlobe;
    public TextMeshProUGUI PlayerShildText;

    public float healingSkillDelay = 10.0f;
    public Slider PlayerHealingGlobe;
    public TextMeshProUGUI PlayerHealingText;

    public float avoidanceSkillDelay = 10.0f;
    public bool debug = true;

    private BasicAttackController basicAttackController;
    private UltimateAttackController ultimateAttackController;
    private DefenseController defenseController;
    private AvoidanceController avoidanceController;
    private HealingController healingController;
    private DamageController damageController;
    private AnimationController animationController;
    private PlayerDataManager playerDataManager;

    private float basicAttackDelayTime;
    private float ultimateAttackDelayTime;
    private float avoidanceSkillDelayTime;
    private float defenseSkillDelayTime;
    private float healingSkillDelayTime;

    void Start()
    {
        basicAttackController = GetComponent<BasicAttackController>();
        ultimateAttackController = GetComponent<UltimateAttackController>();
        defenseController = GetComponent<DefenseController>();
        healingController = GetComponent<HealingController>();
        damageController = GetComponent<DamageController>();
        avoidanceController = GetComponent<AvoidanceController>();
        animationController = GetComponent<AnimationController>();

        playerDataManager = PlayerDataManager.Instance;

        if (playerDataManager == null)
        {
            Debug.LogError("PlayerDataManager 인스턴스를 찾을 수 없습니다!");
            return;
        }
        playerHealth = playerDataManager.playerLinkHealth;
        ultimateAttackGauge = playerDataManager.playerLinkGauge;
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

        PlayerHpBar.value = (float)playerHealth / (float)playerHealthLimit;
        PlayerHpText.text = playerHealth + "/" + playerHealthLimit;

        PlayerGaugeBar.value = (float)ultimateAttackGauge / (float)ultimateAttackGaugeLimit;
        PlayerGaugeText.text = ultimateAttackGauge + "/" + ultimateAttackGaugeLimit;

        basicAttackDelayTime = basicAttackController.delayTime;
        PlayerBasicAttackGlobe.value = (float)basicAttackDelayTime / (float)basicAttackDelay;
        PlayerBasicAttackText.text = $"{Mathf.Ceil(basicAttackDelayTime)}";

        ultimateAttackDelayTime = ultimateAttackController.delayTime;
        PlayerUltimateAttackGlobe.value = (float)ultimateAttackDelayTime / (float)ultimateAttackDelay;
        PlayerUltimateAttackText.text = $"{Mathf.Ceil(ultimateAttackDelayTime)}";

        defenseSkillDelayTime = defenseController.delayTime;
        PlayerShildGlobe.value = (float)defenseSkillDelayTime / (float)defenseSkillDelay;
        PlayerShildText.text = $"{Mathf.Ceil(defenseSkillDelayTime)}";

        healingSkillDelayTime = healingController.delayTime;
        PlayerHealingGlobe.value = (float)healingSkillDelayTime / (float)healingSkillDelay;
        PlayerHealingText.text = $"{Mathf.Ceil(healingSkillDelayTime)}";

        avoidanceSkillDelayTime = avoidanceController.delayTime;

        playerDataManager.playerLinkHealth = playerHealth;
        playerDataManager.playerLinkGauge = ultimateAttackGauge;
    }

    public void IncreaseGauge(int GaugeIncreaseAmount)
    {
        if (ultimateAttackGauge < ultimateAttackGaugeLimit) ultimateAttackGauge += GaugeIncreaseAmount;
         playerDataManager.playerLinkGauge = ultimateAttackGauge;
    }

    public void IncreaseHealth(int HealingAmount)
    {
        
        if (playerHealthLimit < playerHealth + HealingAmount) playerHealth = playerHealthLimit - playerHealth;
        else playerHealth += HealingAmount;
        playerDataManager.playerLinkHealth = playerHealth;
    }

    public void decreaseHealth(int damageAmount)
    {
        if (playerHealth > 0) playerHealth -= damageAmount;
        playerDataManager.playerLinkHealth = playerHealth;

    }

    void GameOver()
    {
        Debug.Log("Game Over");
        /*
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
        */
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
        if (previousGauge != ultimateAttackGauge)
        {
            Debug.Log("currentGauge: " + ultimateAttackGauge);
            if (ultimateAttackGauge >= ultimateAttackGaugeLimit) Debug.Log("Ultimate skill available");
            previousGauge = ultimateAttackGauge;
        }
        Debug.Log("basicAttackDelay:" + basicAttackDelayTime + "\n" +
                  "ultimateAttackDelay:" + ultimateAttackDelayTime + "\n" +
                  "avoidanceSkillDelay:" + avoidanceSkillDelayTime + "\n" +
                  "defenseSkillDelay:" + defenseSkillDelayTime + "\n" +
                  "healingSkillDelay:" + healingSkillDelayTime);
    }
}
