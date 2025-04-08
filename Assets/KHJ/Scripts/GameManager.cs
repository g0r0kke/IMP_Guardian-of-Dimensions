using Azmodan.Phase1;
using Azmodan.Phase2;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    // 보스 관련 변수
    [SerializeField] private BossPhase1 phase1Prefab;
    [SerializeField] private BossPhase2 phase2Prefab;
    
    [SerializeField] private string phase1SceneName = "BossPhase1Scene";
    [SerializeField] private string phase2SceneName = "BossPhase2Scene";
    
    [SerializeField] private bool useSceneTransition = true;
    
    private Boss currentBoss;
    
    // 플레이어 관련 변수
    public float playerHealth = 100f;
    
    // 싱글톤 패턴
    public static GameManager Instance { get; private set; }
    
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
    
    public void StartPhase1()
    {
        if (useSceneTransition)
        {
            // Load Phase 1 scene
            SceneManager.LoadScene(phase1SceneName);
            
            // Set up callback for when scene is loaded
            SceneManager.sceneLoaded += OnPhase1SceneLoaded;
        }
        else
        {
            // Original implementation - just replace the boss
            if (currentBoss != null) Destroy(currentBoss.gameObject);
            currentBoss = Instantiate(phase1Prefab, transform.position, Quaternion.identity);
        }
    }
    
    private void OnPhase1SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Remove the callback
        SceneManager.sceneLoaded -= OnPhase1SceneLoaded;
        
        // Instantiate Phase 1 boss in the new scene
        currentBoss = Instantiate(phase1Prefab, transform.position, Quaternion.identity);
    }
    
    public void TransitionToPhase2()
    {
        // Phase1 보스가 죽었을 때 호출됨
        Debug.Log("매니저: Phase 2로 전환 시작");
        
        if (useSceneTransition)
        {
            // 씬 전환 사용 시 - 현재 씬을 Phase2 씬으로 교체
            Debug.Log("매니저: Phase 2 씬으로 전환");
            SceneManager.LoadScene(phase2SceneName);
            
            // Phase2 씬이 로드된 후 콜백 설정
            SceneManager.sceneLoaded += OnPhase2SceneLoaded;
        }
        else
        {
            // 씬 전환 없이 보스만 교체
            Vector3 position = currentBoss.transform.position;
            Debug.Log("매니저: Phase 1 보스 제거 없이 Phase 2 보스 생성");
            currentBoss = Instantiate(phase2Prefab, position, Quaternion.identity);
        }
    }
    
    private void OnPhase2SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Remove the callback
        SceneManager.sceneLoaded -= OnPhase2SceneLoaded;
        
        // Instantiate Phase 2 boss in the new scene
        Debug.Log("매니저: Phase 2 씬 로드 완료, 보스 생성");
        currentBoss = Instantiate(phase2Prefab, transform.position, Quaternion.identity);
    }
}