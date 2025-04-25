using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ARPlaneButton : MonoBehaviour
{
    [SerializeField] private ARPlacement arPlacement;
    [SerializeField] private string targetSceneName = "Boss1Scene"; // 전환할 씬 이름

    private Button button;
    private GameObject fadeObject;
    private FadeAnimationController fadeController;

    private void Start()
    {
        button = GetComponent<Button>();
        if (!button) return;

        if (!arPlacement)
        {
            arPlacement = FindFirstObjectByType<ARPlacement>();
        }

        // 버튼 클릭 이벤트에 메서드 연결
        button.onClick.AddListener(SaveBossPosition);

        if (GameManager.Instance)
        {
            fadeObject = GameManager.Instance.fadeObject;
            fadeController = GameManager.Instance.fadeController;
        }
    }

    public void SaveBossPosition()
    {
        // 버튼 사운드 재생
        AudioManager.Instance.PlayButtonSFX();

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

        if (fadeObject && fadeController)
        {
            // 페이드인 애니메이션 실행 (화면이 검게)
            fadeController.PlayFadeAnimation(true, () =>
            {
                // GameManager 상태 설정
                if (GameManager.Instance)
                {
                    GameManager.Instance.SetState(GameState.BossPhase1);
                }
                
                // 씬 전환
                SceneManager.LoadScene(targetSceneName);

                // 씬 로드 후 페이드아웃 실행 (화면이 다시 밝게)
                fadeController.PlayFadeAnimation(false);
            });
        }
        else
        {
            Debug.Log("FadeAnimationController component not found on fadeObject");
        }
    }
}