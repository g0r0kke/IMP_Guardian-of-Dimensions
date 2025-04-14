using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ARPlaneButton : MonoBehaviour
{
    [SerializeField] private ARPlacement arPlacement;
    private Button button;
    [SerializeField] private string targetSceneName = "Boss1Scene"; // 전환할 씬 이름
    [SerializeField] private bool transitionImmediately = true; // 저장 후 바로 씬 전환 여부

    private void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Button 컴포넌트가 없습니다!");
            return;
        }

        if (arPlacement == null)
        {
            arPlacement = FindObjectOfType<ARPlacement>();
            if (arPlacement == null)
            {
                Debug.LogError("ARPlacement를 찾을 수 없습니다!");
                return;
            }
        }

        // 버튼 클릭 이벤트에 메서드 연결
        button.onClick.AddListener(SaveBossPosition);
    }

    public void SaveBossPosition()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.BossPhase1);
        }
        else
        {
            Debug.LogWarning("GameManager를 찾을 수 없습니다.");
        }
        
        if (arPlacement != null)
        {
            Vector3 cubePosition = arPlacement.GetSpawnedObjectPosition();
            if (cubePosition != Vector3.zero)
            {
                // GameManager에 위치 저장
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetBossPosition(cubePosition);
                    Debug.Log($"보스 위치 저장 완료: {cubePosition}");
                    
                    // 즉시 씬 전환 옵션이 켜져 있으면 씬 전환
                    if (transitionImmediately && !string.IsNullOrEmpty(targetSceneName))
                    {
                        Debug.Log($"씬 전환 시작: {targetSceneName}");
                        TransitionToScene();
                    }
                }
                else
                {
                    Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다!");
                }
            }
            else
            {
                // Debug.LogWarning("큐브가 생성되지 않았습니다! AR 플레인을 터치하여 큐브를 먼저 배치해주세요.");
                Debug.LogWarning("큐브 생성 없이 씬 전환");
                // 즉시 씬 전환 옵션이 켜져 있으면 씬 전환
                if (transitionImmediately && !string.IsNullOrEmpty(targetSceneName))
                {
                    Debug.Log($"씬 전환 시작: {targetSceneName}");
                    TransitionToScene();
                }
            }
        }
    }
    
    public void TransitionToScene()
    {
        if (SceneManager.GetSceneByName(targetSceneName).IsValid() || 
            System.Array.IndexOf(GetAllScenesInBuild(), targetSceneName) != -1)
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"씬 '{targetSceneName}'을(를) 찾을 수 없습니다. 빌드 설정에 해당 씬이 포함되어 있는지 확인하세요.");
        }
    }
    
    private string[] GetAllScenesInBuild()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];
        
        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(path);
        }
        
        return scenes;
    }
}
