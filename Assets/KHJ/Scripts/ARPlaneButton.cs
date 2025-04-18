using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ARPlaneButton : MonoBehaviour
{
    [SerializeField] private ARPlacement arPlacement;
    [SerializeField] private string targetSceneName = "Boss1Scene"; // 전환할 씬 이름
    
    private Button button;
    private GameObject fadeObject;

    private void Start()
    {
        button = GetComponent<Button>();
        if (!button) return;

        if (!arPlacement)
        {
            arPlacement = FindObjectOfType<ARPlacement>();
        }

        // 버튼 클릭 이벤트에 메서드 연결
        button.onClick.AddListener(SaveBossPosition);
        
        fadeObject = GameObject.FindWithTag("UI_Black");
        if (!fadeObject) return;
    }

    public void SaveBossPosition()
    {
        // 버튼 사운드 재생
        AudioManager.Instance.PlayButtonSFX();

        // GameManager 상태 설정
        if (GameManager.Instance)
        {
            GameManager.Instance.SetState(GameState.BossPhase1);
        }
        
        // AR 배치 오브젝트 위치 저장
        if (arPlacement && GameManager.Instance)
        {
            Vector3 cubePosition = arPlacement.GetSpawnedObjectPosition();
            
            // 유효한 위치가 있으면 저장
            if (cubePosition != Vector3.zero)
            {
                GameManager.Instance.SetBossPosition(cubePosition);
            }
        }
        
        if (fadeObject)
        {
            FadeAnimationController fadeController = fadeObject.GetComponent<FadeAnimationController>();

            if (fadeController)
            {
                // 페이드인 애니메이션 실행 (화면이 검게)
                fadeController.PlayFadeAnimation(true, () =>
                {
                    // 씬 전환
                    SceneManager.LoadScene(targetSceneName);
                    
                    // GameManager 상태 설정
                    if (GameManager.Instance)
                    {
                        GameManager.Instance.SetState(GameState.BossPhase1);
                    }

                    // 씬 로드 후 페이드아웃 실행 (화면이 다시 밝게)
                    fadeController.PlayFadeAnimation(false);
                });
            }
            else
            {
                Debug.LogWarning("UI_Black 오브젝트에 FadeAnimationController 컴포넌트가 없습니다.");
                // 씬 전환
                SceneManager.LoadScene(targetSceneName);
                
                // GameManager 상태 설정
                if (GameManager.Instance)
                {
                    GameManager.Instance.SetState(GameState.BossPhase1);
                }
            }
        }
        else
        {
            Debug.LogWarning("UI_Black 태그를 가진 오브젝트를 찾을 수 없습니다.");
            // 씬 전환
            SceneManager.LoadScene(targetSceneName);
            
            // GameManager 상태 설정
            if (GameManager.Instance)
            {
                GameManager.Instance.SetState(GameState.BossPhase1);
            }
        }
    }
}