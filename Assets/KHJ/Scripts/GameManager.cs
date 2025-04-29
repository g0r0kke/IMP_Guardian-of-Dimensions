using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
    [SerializeField] private GameState state = GameState.Intro;
    public GameState State 
    { 
        get { return state; }
        private set { state = value; }
    }
    
    // 보스 관련 변수
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        if (Keyboard.current.nKey.wasPressedThisFrame) // N키 누르면 승리
        {
            SetState(GameState.Victory);
        }
        if (Keyboard.current.mKey.wasPressedThisFrame) // M키 누르면 패배
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
                // 승리 상태 시작 시 처리
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
                // 사망 상태 시작 시 처리
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
    
    private void FindReferences()
    {
        // null인 레퍼런스만 찾기 (이미 있다면 재사용)
        bossPrefab = GameObject.Find("BossPhase1");
        if (!bossPrefab) bossPrefab = GameObject.Find("BossPhase2");
        if (!virtualJoystick) virtualJoystick = GameObject.Find("UI_JoyStick");
        if (!bossIndicatorUI) bossIndicatorUI = GameObject.Find("BossIndicatorUI");
        
        hobgoblins.Clear(); // 기존 목록 초기화
    
        // 모든 Enemy 태그 오브젝트 찾기
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
    
        // Enemy 중에서 Hobgoblin 컴포넌트를 가진 것만 필터링
        foreach (GameObject enemy in allEnemies)
        {
            Hobgoblin hobgoblinComponent = enemy.GetComponent<Hobgoblin>();
            if (hobgoblinComponent)
            {
                hobgoblins.Add(enemy);
            }
        }
        
        // 먼저 부모 UI 컨테이너 찾기
        GameObject victoryDefeatContainer = GameObject.Find("VictoryDefeat UI");

        if (victoryDefeatContainer != null)
        {
            // 인덱스로 직접 자식 UI 요소들 찾기
            if (victoryDefeatContainer.transform.childCount >= 3)
            {
                uiBackground = victoryDefeatContainer.transform.GetChild(0).gameObject; // background Blur
                defeatUI = victoryDefeatContainer.transform.GetChild(1).gameObject;     // Defeat_UI
                victoryUI = victoryDefeatContainer.transform.GetChild(2).gameObject;    // Victory_UI
            }
            else
            {
                Debug.LogError("VictoryDefeat_UI 오브젝트에 필요한 자식 오브젝트가 없습니다.");
            }
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
        bossPosition = new Vector3(position.x, -1f, position.z);
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