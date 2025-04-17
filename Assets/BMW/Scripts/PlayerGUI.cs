using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGUI : MonoBehaviour
{
    public int playerHealthLimit = 1;
    public int playerHealth = 1;
    private int previousHealth;
    public Slider PlayerHpBar;
    public TextMeshProUGUI PlayerHpText;

    public int ultimateAttackGaugeLimit = 1;
    public int ultimateAttackGauge = 0;
    private int previousGauge;
    public Slider PlayerGaugeBar;
    public TextMeshProUGUI PlayerGaugeText;

    public float basicAttackDelay = 0.6f;
    public Slider PlayerBasicAttackGlobe;
    public TextMeshProUGUI PlayerBasicAttackText;

    public float ultimateAttackDelay = 6.0f;
    public Slider PlayerUltimateAttackGlobe;
    public TextMeshProUGUI PlayerUltimateAttackText;

    public float defenseSkillDelay = 4.0f;
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

    [Header("���� Ÿ��")]
    private Boss bossTarget;
    private List<Hobgoblin> summonTargets = new List<Hobgoblin>();

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
            Debug.LogError("PlayerDataManager �ν��Ͻ��� ã�� �� �����ϴ�!");
            return;
        }
        playerHealth = playerDataManager.playerLinkHealth;
        ultimateAttackGauge = playerDataManager.playerLinkGauge;

        // ��� Enemy �±� ������Ʈ ã��
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemyObject in allEnemies)
        {
            // Boss ������Ʈ ���� �� ã��
            Boss boss = enemyObject.GetComponent<Boss>();
            if (boss)
            {
                bossTarget = boss;
                break; // ���� ã���� �ߴ�
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (playerHealth <= 0 && playerDataManager.isControlPlayer)
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

        if (playerHealth > 0)
        {
            PlayerHpBar.value = (float)playerHealth / (float)playerHealthLimit;
            PlayerHpText.text = playerHealth + "/" + playerHealthLimit;
        }
        else
        {
            PlayerHpBar.value = 0;
            PlayerHpText.text = "0/" + playerHealthLimit;
        }

            PlayerGaugeBar.value = (float)ultimateAttackGauge / (float)ultimateAttackGaugeLimit;
        PlayerGaugeText.text = ultimateAttackGauge + "/" + ultimateAttackGaugeLimit;

        basicAttackDelayTime = basicAttackController.delayTime;
        PlayerBasicAttackGlobe.value = (float)basicAttackDelayTime / (float)basicAttackDelay;
        if (Mathf.Ceil(basicAttackDelayTime) != 0) PlayerBasicAttackText.text = $"{Mathf.Ceil(basicAttackDelayTime)}";
        else PlayerBasicAttackText.text = $" ";

        ultimateAttackDelayTime = ultimateAttackController.delayTime;
        PlayerUltimateAttackGlobe.value = (float)ultimateAttackDelayTime / (float)ultimateAttackDelay;
        if (Mathf.Ceil(ultimateAttackDelayTime) != 0) PlayerUltimateAttackText.text = $"{Mathf.Ceil(ultimateAttackDelayTime)}";
        else PlayerUltimateAttackText.text = " ";

        defenseSkillDelayTime = defenseController.delayTime;
        PlayerShildGlobe.value = (float)defenseSkillDelayTime / (float)defenseSkillDelay;
        if (Mathf.Ceil(defenseSkillDelayTime) != 0) PlayerShildText.text = $"{Mathf.Ceil(defenseSkillDelayTime)}";
        else PlayerShildText.text = " ";

        healingSkillDelayTime = healingController.delayTime;
        PlayerHealingGlobe.value = (float)healingSkillDelayTime / (float)healingSkillDelay;
        if (Mathf.Ceil(healingSkillDelayTime) != 0) PlayerHealingText.text = $"{Mathf.Ceil(healingSkillDelayTime)}";
        else PlayerHealingText.text = " ";

        avoidanceSkillDelayTime = avoidanceController.delayTime;

        if (playerDataManager != null)
        {
            playerDataManager.playerLinkHealth = playerHealth;
            playerDataManager.playerLinkGauge = ultimateAttackGauge;
        }
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
        
      playerDataManager.isControlPlayer = false;
        Debug.Log("Game Over");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.Defeat);
        }
        else
        {
            Debug.LogWarning("GameManager�� ã�� �� �����ϴ�.");
        }

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

    // ȩ����� ����Ʈ���� �����ϴ� �޼���
    public void RemoveHobgoblinTarget(Hobgoblin hobgoblin)
    {
        if (hobgoblin && summonTargets.Contains(hobgoblin))
        {
            summonTargets.Remove(hobgoblin);
            Debug.Log($"ȩ��� '{hobgoblin.gameObject.name}'��(��) Ÿ�� ����Ʈ���� ���ŵǾ����ϴ�. ���� ��: {summonTargets.Count}��");
        }
    }

    // ȩ����� �÷��̾��� Ÿ�� ����Ʈ�� �߰��ϴ� public �޼���
    public void AddHobgoblinTarget(Hobgoblin hobgoblin)
    {
        if (hobgoblin && !summonTargets.Contains(hobgoblin))
        {
            summonTargets.Add(hobgoblin);
            Debug.Log($"ȩ��� '{hobgoblin.gameObject.name}'��(��) Ÿ�� ����Ʈ�� �߰��Ǿ����ϴ�. ���� �� {summonTargets.Count}��");
        }
    }
}
