using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Intro,
    BossPhase1,
    BossPhase2,
    Victory,
    Defeat
}
public class GameManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static GameManager Instance { get; private set; }
    private GameState state = GameState.Intro;
    public GameState State 
    { 
        get { return state; }
        private set { state = value; }
    }
    
    // 보스 관련 변수
    [Header("Boss References")]
    [SerializeField] private string phase1SceneName = "BossPhase1Scene";
    [SerializeField] private string phase2SceneName = "BossPhase2Scene";
    [SerializeField] private Vector3 bossPosition = new Vector3(-1.8f, 0f, -11.4f);
    private Boss currentBoss;
    
    [Header("UI References")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject defeatUI;
    [SerializeField] private GameObject uiBackground;

    // 플레이어 관련 변수
    private PlayerDataManager playerDataManager;

    [Header("References")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject virtualJoystick;
    [SerializeField] private GameObject hobgoblin;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerDataManager = PlayerDataManager.Instance;

    }
    
    private void Start()
    {
        // 게임 시작 시 필요한 레퍼런스 찾기
        FindReferences();
    
        // 씬 전환 이벤트에도 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindReferences();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) // N키 누르면 승리
        {
            SetState(GameState.Victory);
        }
        if (Input.GetKeyDown(KeyCode.M)) // M키 누르면 패배
        {
            SetState(GameState.Defeat);
        }
    }

    public void SetState(GameState newState)
    {
        // 같은 상태면 무시
        if (state == newState)
            return;
        
        // 상태 변경
        state = newState;
        Debug.Log($"게임 상태 변경: {typeof(GameState)} -> {newState}");
        
        FindReferences();
        
        // 새 상태 시작 처리
        switch (newState)
        {
            case GameState.Intro:
                // 인트로 시작 시 처리
                HideAllUI();
                break;
            case GameState.BossPhase1:
                // 플레이 상태 시작 시 처리
                playerDataManager.PlayerOriginSetting();
                if (bossPrefab) bossPrefab.SetActive(true);
                if (virtualJoystick) virtualJoystick.SetActive(true);
                HideAllUI();
                break;
            case GameState.BossPhase2:
                if (bossPrefab) bossPrefab.SetActive(true);
                if (virtualJoystick) virtualJoystick.SetActive(true);
                if(hobgoblin) hobgoblin.SetActive(true);
                HideAllUI();
                break;
            case GameState.Victory:
                // 승리 상태 시작 시 처리
                if (bossPrefab) bossPrefab.SetActive(false);
                if (virtualJoystick) virtualJoystick.SetActive(false);
                if (hobgoblin) hobgoblin.SetActive(false);
                ShowVictoryUI();
                break;
            case GameState.Defeat:
                // 사망 상태 시작 시 처리
                if (bossPrefab) bossPrefab.SetActive(false);
                if (virtualJoystick) virtualJoystick.SetActive(false);
                if (hobgoblin) hobgoblin.SetActive(false);
                ShowDefeatUI();
                break;
        }
    }
    
    private void FindReferences()
    {
        // null인 레퍼런스만 찾기 (이미 있다면 재사용)
        bossPrefab = GameObject.Find("BossPhase1");
        if (bossPrefab == null) bossPrefab = GameObject.Find("BossPhase2");
        if (virtualJoystick == null) virtualJoystick = GameObject.Find("UI_JoyStick");
        if (hobgoblin == null) hobgoblin = GameObject.Find("Hobgoblin 1");
        
        
        // 먼저 부모 UI 컨테이너 찾기
        GameObject victoryDefeatContainer = GameObject.Find("VictoryDefeat_UI");
    
        if (victoryDefeatContainer != null)
        {
            // 자식 UI 요소들 찾기
            Transform bgTransform = victoryDefeatContainer.transform.Find("Background_UI");
            if (bgTransform != null) uiBackground = bgTransform.gameObject;
        
            Transform victoryTransform = victoryDefeatContainer.transform.Find("Victory_UI");
            if (victoryTransform != null) victoryUI = victoryTransform.gameObject;
        
            Transform defeatTransform = victoryDefeatContainer.transform.Find("Defeat_UI");
            if (defeatTransform != null) defeatUI = defeatTransform.gameObject;
        }
    }

    public void TransitionToPhase2()
    {
        // 현재 씬을 Phase2 씬으로 교체
        Debug.Log("매니저: Phase 2 씬으로 전환");
        SceneManager.LoadScene(phase2SceneName);
            
        SetState(GameState.BossPhase2);
    }
    
    // 보스 위치 Getter
    public Vector3 GetBossPosition()
    {
        return bossPosition;
    }

    // 보스 위치 Setter
    public void SetBossPosition(Vector3 position)
    {
        bossPosition = new Vector3(position.x, 0f, position.z);
        Debug.Log($"보스 위치가 설정되었습니다: {bossPosition}");
    }
    
    // UI 표시 메서드
    private void ShowVictoryUI()
    {
        if (uiBackground) uiBackground.SetActive(true);
        if (victoryUI) victoryUI.SetActive(true);
        if (defeatUI) defeatUI.SetActive(false);
    }
    
    private void ShowDefeatUI()
    {
        if (uiBackground) uiBackground.SetActive(true);
        if (defeatUI) defeatUI.SetActive(true);
        if (victoryUI) victoryUI.SetActive(false);
    }
    
    private void HideAllUI()
    {
        if (uiBackground) uiBackground.SetActive(false);
        if (victoryUI) victoryUI.SetActive(false);
        if (defeatUI) defeatUI.SetActive(false);
    }
}