using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameScenesMove : MonoBehaviour
{
    
    public void GameSceneCtrl()
    {
        SceneManager.LoadScene("ARPlaneScene"); // 어떤씬으로 이동할지

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameState.BossPhase1);
        }
        else
        {
            Debug.LogWarning("GameManager를 찾을 수 없습니다.");
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
