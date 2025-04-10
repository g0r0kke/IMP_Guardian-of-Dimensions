using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public int playerHealthLimit = 50;
    public int originPlayerHealth = 50;
    public int playerGaugeLimit = 10;
    public int originPlayerGauge = 0;

    public int playerLinkHealth;
    public int playerLinkGauge;

    private PlayerGUI playerGUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerLinkHealth = originPlayerHealth;
            playerLinkGauge = originPlayerGauge;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        playerGUI = FindObjectOfType<PlayerGUI>();
        if (playerGUI == null)
        {
            Debug.LogError("PlayerGUI�� ã�� �� �����ϴ�!");
            return;
        }

        LoadPlayerData();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedLoadAfterSceneLoad());
    }

    private IEnumerator DelayedLoadAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();

        playerGUI = FindObjectOfType<PlayerGUI>();
        if (playerGUI == null)
        {
            Debug.LogError("�� ������ PlayerGUI�� ã�� �� �����ϴ�!");
            yield break;
        }

        LoadPlayerData();
    }

    public void LoadPlayerData()
    {
        if (playerGUI == null)
        {
            Debug.LogWarning("PlayerGUI�� �������� �ʾ� �����͸� �ε��� �� �����ϴ�.");
            return;
        }

        playerGUI.playerHealthLimit = playerHealthLimit;
        playerGUI.playerHealth = playerLinkHealth;
        UpdateHealthUI();

        playerGUI.ultimateAttackGaugeLimit = playerGaugeLimit;
        playerGUI.ultimateAttackGauge = playerLinkGauge;
        UpdateGaugeUI();
    }

    private void UpdateHealthUI()
    {
        playerGUI.PlayerHpBar.value = (float)playerLinkHealth / playerHealthLimit;
        playerGUI.PlayerHpText.text = $"{playerLinkHealth}/{playerHealthLimit}";
    }

    private void UpdateGaugeUI()
    {
        playerGUI.PlayerGaugeBar.value = (float)playerLinkGauge / playerGaugeLimit;
        playerGUI.PlayerGaugeText.text = $"{playerLinkGauge}/{playerGaugeLimit}";
    }
}
