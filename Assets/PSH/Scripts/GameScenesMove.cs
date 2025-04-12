using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameScenesMove : MonoBehaviour
{
    
    public void GameSceneCtrl()
    {
        SceneManager.LoadScene("Boss1Scene"); // 어떤씬으로 이동할지
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
