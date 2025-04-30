using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Game states representing different phases of gameplay
/// </summary>
public enum GameState
{
    Intro,
    BossPhase1,
    BossPhase2,
    Victory,
    Defeat
}

/// <summary>
/// Manages overall game state, scene transitions, and core game functionality
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameState state = GameState.Intro;
    public GameState State 
    { 
        get { return state; }
        private set { state = value; }
    }
    
    // Boss-related variables
    [Header("Boss References")]
    // [SerializeField] private string phase1SceneName = "BossPhase1Scene";
    [SerializeField] private string phase2SceneName = "BossPhase2Scene";
    [SerializeField] private Vector3 bossPosition = new Vector3(-1.8f, -1f, -11.4f);
    
    [Header("UI References")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject defeatUI;
    [SerializeField] private GameObject uiBackground;
    [SerializeField] private GameObject bossIndicatorUI;

    [Header("References")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject virtualJoystick;
    [SerializeField] private List<GameObject> hobgoblins = new List<GameObject>();
    public GameObject fadeObject;
    public FadeAnimationController fadeController;
    
    private void Awake()
    {
        // Singleton implementation
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Find necessary references at game start
        FindReferences();
    
        // Register for scene transition events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindReferences();
    }

    private void Update()
    {
        // Debug keys for testing game states
        // if (Keyboard.current.nKey.wasPressedThisFrame) // Press N key for victory
        // {
        //     SetState(GameState.Victory);
        // }
        // if (Keyboard.current.mKey.wasPressedThisFrame) // Press M key for defeat
        // {
        //     SetState(GameState.Defeat);
        // }
    }

    /// <summary>
    /// Changes the game state and handles associated transitions
    /// </summary>
    /// <param name="newState">The new state to transition to</param>
    public void SetState(GameState newState)
    {
        // Ignore if same state
        if (state == newState)
            return;
        
        // Change state
        state = newState;
        Debug.Log($"Game state changed: {typeof(GameState)} -> {newState}");
        
        FindReferences();
        
        // Handle new state initialization
        switch (newState)
        {
            case GameState.Intro:
                // Handle intro state initialization
                HideAllUI();
                break;
            case GameState.BossPhase1:
                // Handle phase 1 initialization
                if (PlayerDataManager.Instance)
                {
                    PlayerDataManager.Instance.PlayerOriginSetting();
                    Debug.Log("Player status reset completed");
                }
                else
                {
                    Debug.LogError("PlayerDataManager.Instance is NULL!");
                }
                
                if (bossPrefab) bossPrefab.SetActive(true);
                if (virtualJoystick) virtualJoystick.SetActive(true);
                if (bossIndicatorUI) bossIndicatorUI.SetActive(true);
                HideAllUI();
                break;
            case GameState.BossPhase2:
                // Handle phase 2 initialization
                if (bossPrefab) bossPrefab.SetActive(true);
                if (virtualJoystick) virtualJoystick.SetActive(true);
                foreach (GameObject hobgoblin in hobgoblins)
                {
                    if (hobgoblin) hobgoblin.SetActive(true);
                }
                if (bossIndicatorUI) bossIndicatorUI.SetActive(true);
                HideAllUI();
                break;
            case GameState.Victory:
                // Handle victory state initialization
                if (bossPrefab) bossPrefab.SetActive(false);
                if (virtualJoystick) virtualJoystick.SetActive(false);
                foreach (GameObject hobgoblin in hobgoblins)
                {
                    if (hobgoblin) hobgoblin.SetActive(false);
                }
                if (bossIndicatorUI) bossIndicatorUI.SetActive(false);
                ShowVictoryUI();
                break;
            case GameState.Defeat:
                // Handle defeat state initialization
                if (bossPrefab) bossPrefab.SetActive(false);
                if (virtualJoystick) virtualJoystick.SetActive(false);
                foreach (GameObject hobgoblin in hobgoblins)
                {
                    if (hobgoblin) hobgoblin.SetActive(false);
                }
                if (bossIndicatorUI) bossIndicatorUI.SetActive(false);
                ShowDefeatUI();
                break;
        }
    }
    
    /// <summary>
    /// Finds and caches references to game objects and components
    /// </summary>
    private void FindReferences()
    {
        // Only find references that are null (reuse existing ones)
        bossPrefab = GameObject.Find("BossPhase1");
        if (!bossPrefab) bossPrefab = GameObject.Find("BossPhase2");
        if (!virtualJoystick) virtualJoystick = GameObject.Find("UI_JoyStick");
        if (!bossIndicatorUI) bossIndicatorUI = GameObject.Find("BossIndicatorUI");
        
        hobgoblins.Clear(); // Clear existing list
    
        // Find all objects with Enemy tag
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
    
        // Filter only enemies with Hobgoblin component
        foreach (GameObject enemy in allEnemies)
        {
            Hobgoblin hobgoblinComponent = enemy.GetComponent<Hobgoblin>();
            if (hobgoblinComponent)
            {
                hobgoblins.Add(enemy);
            }
        }
        
        // Find parent UI container first
        GameObject victoryDefeatContainer = GameObject.Find("VictoryDefeat UI");

        if (victoryDefeatContainer)
        {
            // Find child UI elements directly by index
            if (victoryDefeatContainer.transform.childCount >= 3)
            {
                uiBackground = victoryDefeatContainer.transform.GetChild(0).gameObject; // background Blur
                defeatUI = victoryDefeatContainer.transform.GetChild(1).gameObject;     // Defeat_UI
                victoryUI = victoryDefeatContainer.transform.GetChild(2).gameObject;    // Victory_UI
            }
            else
            {
                Debug.LogError("VictoryDefeat_UI object is missing required child objects.");
            }
        }
    }
    
    /// <summary>
    /// Transitions to the second phase of the boss battle
    /// </summary>
    public void TransitionToPhase2()
    {
        // Replace current scene with Phase2 scene
        Debug.Log("Manager: Transitioning to Phase 2 scene");
        SceneManager.LoadScene(phase2SceneName);
            
        SetState(GameState.BossPhase2);
    }
    
    /// <summary>
    /// Gets the current boss position
    /// </summary>
    public Vector3 GetBossPosition()
    {
        return bossPosition;
    }

    /// <summary>
    /// Sets the boss position
    /// </summary>
    /// <param name="position">New position for the boss</param>
    public void SetBossPosition(Vector3 position)
    {
        bossPosition = new Vector3(position.x, -1f, position.z);
        Debug.Log($"Boss position has been set: {bossPosition}");
    }
    
#region UI display methods
    /// <summary>
    /// Shows the victory UI
    /// </summary>
    private void ShowVictoryUI()
    {
        if (uiBackground) uiBackground.SetActive(true);
        if (victoryUI) victoryUI.SetActive(true);
        if (defeatUI) defeatUI.SetActive(false);
    }
    
    /// <summary>
    /// Shows the defeat UI
    /// </summary>
    private void ShowDefeatUI()
    {
        if (uiBackground) uiBackground.SetActive(true);
        if (defeatUI) defeatUI.SetActive(true);
        if (victoryUI) victoryUI.SetActive(false);
    }
    
    /// <summary>
    /// Hides all UI elements
    /// </summary>
    private void HideAllUI()
    {
        if (uiBackground) uiBackground.SetActive(false);
        if (victoryUI) victoryUI.SetActive(false);
        if (defeatUI) defeatUI.SetActive(false);
    }
#endregion
}