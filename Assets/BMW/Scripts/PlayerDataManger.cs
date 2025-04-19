using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataManager : MonoBehaviour
{
    [Header("플레이어 체력 세팅")]
    public int playerHealthLimit = 100;
    private int originPlayerHealth = 100;
    public int playerLinkHealth;

    [Header("플레이어 궁극기게이지 세팅")]
    public int playerGaugeLimit = 3;
    private int originPlayerGauge = 0;
    public int playerLinkGauge;

    [Header("플레이어 조작 상태 세팅")]
    public bool isControlPlayer;

    // 외부 스크립트 연결 세팅
    public static PlayerDataManager Instance;
    private PlayerGUI playerGUI;
    private GameManager gameManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            PlayerOriginSetting();
        }

        // Find the GameManager reference
        gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager를 찾을 수 없습니다.");
        }

        // Only try to find PlayerGUI if we're in a valid game state
        if (ShouldLoadPlayerGUI())
        {
            FindAndLoadPlayerGUI();
        }
    }

    public void PlayerOriginSetting()
    {
        playerLinkHealth = originPlayerHealth;
        playerLinkGauge = originPlayerGauge;
        isControlPlayer = true;
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

        // Make sure we have a reference to GameManager
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
            
        }

        // Only try to find PlayerGUI if we're in a valid game state
        if (ShouldLoadPlayerGUI())
        {
            FindAndLoadPlayerGUI();

        }
    }

    // Check if the current game state requires PlayerGUI
    private bool ShouldLoadPlayerGUI()
    {
        if (gameManager == null) return false;

        Debug.Log(gameManager.State);

        // Only load PlayerGUI during active boss phases
        return gameManager.State == GameState.BossPhase1 || gameManager.State == GameState.BossPhase2;
    }

    // Find PlayerGUI and load player data if it exists
    private void FindAndLoadPlayerGUI()
    {
        playerGUI = FindAnyObjectByType<PlayerGUI>();
        if (playerGUI != null)
        {
            LoadPlayerData();
        }
    }

    public void LoadPlayerData()
    {
        // Double-check that we have a valid PlayerGUI
        if (playerGUI == null)
        {
            // Only log a warning if we're in a state where PlayerGUI should exist
            if (ShouldLoadPlayerGUI())
            {
                Debug.LogWarning("PlayerGUI를 찾을 수 없습니다.");
            }
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
        // Make sure we have a valid PlayerGUI
        if (playerGUI == null) return;

        playerGUI.PlayerHpBar.value = (float)playerLinkHealth / (float)playerHealthLimit;
        playerGUI.PlayerHpText.text = $"{playerLinkHealth}/{playerHealthLimit}";
    }

    private void UpdateGaugeUI()
    {
        // Make sure we have a valid PlayerGUI
        if (playerGUI == null) return;

        playerGUI.PlayerGaugeBar.value = (float)playerLinkGauge / (float)playerGaugeLimit;
        playerGUI.PlayerGaugeText.text = $"{playerLinkGauge}/{playerGaugeLimit}";
    }
}