using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ARPlaneButton : MonoBehaviour
{
    [SerializeField] private ARPlacement arPlacement;
    [SerializeField] private string targetSceneName = "Boss1Scene"; // 전환할 씬 이름
    
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        if (arPlacement == null)
        {
            arPlacement = FindObjectOfType<ARPlacement>();
        }

        // 버튼 클릭 이벤트에 메서드 연결
        button.onClick.AddListener(SaveBossPosition);
    }

    public void SaveBossPosition()
    {
        // GameManager 상태 설정
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.BossPhase1);
        }
        
        // AR 배치 오브젝트 위치 저장
        if (arPlacement != null && GameManager.Instance != null)
        {
            Vector3 cubePosition = arPlacement.GetSpawnedObjectPosition();
            
            // 유효한 위치가 있으면 저장
            if (cubePosition != Vector3.zero)
            {
                GameManager.Instance.SetBossPosition(cubePosition);
            }
        }
        
        // 씬 전환
        SceneManager.LoadScene(targetSceneName);
    }
}