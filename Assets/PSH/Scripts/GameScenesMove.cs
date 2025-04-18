using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameScenesMove : MonoBehaviour
{
    private GameObject fadeObject;

    void Start()
    {
        fadeObject = GameObject.FindWithTag("UI_Black");
    }

    public void GameSceneCtrl()
    {
        if (fadeObject)
        {
            FadeAnimationController fadeController = fadeObject.GetComponent<FadeAnimationController>();

            if (fadeController)
            {
                // 페이드인 애니메이션 실행 (화면이 검게)
                fadeController.PlayFadeAnimation(true, () =>
                {
                    // 페이드인 완료 후 씬 전환
                    SceneManager.LoadScene("ARPlaneScene");

                    // 씬 로드 후 페이드아웃 실행 (화면이 다시 밝게)
                    fadeController.PlayFadeAnimation(false);
                });
            }
            else
            {
                Debug.LogWarning("UI_Black 오브젝트에 FadeAnimationController 컴포넌트가 없습니다.");
                SceneManager.LoadScene("ARPlaneScene");
            }
        }
        else
        {
            Debug.LogWarning("UI_Black 태그를 가진 오브젝트를 찾을 수 없습니다.");
            SceneManager.LoadScene("ARPlaneScene");
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}