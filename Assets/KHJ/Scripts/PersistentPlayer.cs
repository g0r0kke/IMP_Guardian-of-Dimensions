using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    // 싱글톤 패턴 적용
    private static PersistentPlayer instance;
    private Transform mainCamera;
    
    void Awake()
    {
        // 현재 씬 이름 확인
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 배틀1씬 또는 배틀2씬에서 싱글톤 적용
        if (currentSceneName == "Battle1Scene" || currentSceneName == "Battle2Scene")
        {
            // 이미 인스턴스가 있는지 확인
            if (instance == null)
            {
                instance = this;
                
                // 플레이어 객체 보존
                DontDestroyOnLoad(gameObject);
                
                // 씬 로드 이벤트에 리스너 추가 (한 번만)
                SceneManager.sceneLoaded += OnSceneLoaded;
                
                Debug.Log("플레이어 인스턴스 생성: " + gameObject.name);
            }
            else
            {
                // 이미 인스턴스가 있으면 이 오브젝트 파괴
                Debug.Log("중복 플레이어 파괴: " + gameObject.name);
                Destroy(gameObject);
            }
        }
    }
    
    void Start()
    {
        // 메인 카메라 참조 설정 (Awake보다 조금 늦게 실행될 수 있음)
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
            Debug.Log("메인 카메라 참조 설정: " + mainCamera.name);
        }
        else
        {
            Debug.LogWarning("메인 카메라를 찾을 수 없습니다!");
        }
    }
    
    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 리스너 제거
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
            Debug.Log("플레이어 인스턴스 제거");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로 로드된 씬 이름 확인
        string newSceneName = scene.name;
        
        Debug.Log("씬 로드됨: " + newSceneName);
        
        if (newSceneName != "Battle1Scene" && newSceneName != "Battle2Scene")
        {
            // 배틀1, 배틀2가 아닌 다른 씬으로 이동하면 이 플레이어 파괴
            Debug.Log("플레이어 객체 파괴 (다른 씬으로 이동)");
            Destroy(gameObject);
        }
        else
        {
            // Battle1Scene 또는 Battle2Scene일 경우
            
            // 씬이 완전히 로드될 때까지 약간의 지연 후 처리
            Invoke("SetupPlayerInScene", 0.1f);
        }
    }
    
    private void SetupPlayerInScene()
    {
        // 새 씬의 메인 카메라 업데이트
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
            Debug.Log("새 씬의 메인 카메라 참조 업데이트: " + mainCamera.name);
            
            // 플레이어 위치를 카메라 기준으로 조정 (필요시)
            // 카메라의 자식으로 설정하는 대신 상대적 위치만 설정
            transform.position = mainCamera.position + new Vector3(0, 0, 0); // 필요에 따라 오프셋 조정
            transform.rotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("새 씬에서 메인 카메라를 찾을 수 없습니다!");
        }
        
        // 중복 플레이어 확인 및 처리
        PersistentPlayer[] players = FindObjectsOfType<PersistentPlayer>();
        Debug.Log("현재 씬의 플레이어 수: " + players.Length);
        
        foreach (PersistentPlayer player in players)
        {
            if (player != this && player != instance)
            {
                Debug.Log("중복 플레이어 비활성화: " + player.name);
                player.gameObject.SetActive(false);
            }
        }
    }
}