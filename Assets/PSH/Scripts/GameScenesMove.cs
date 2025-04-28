using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameScenesMove : MonoBehaviour
{
    private GameObject fadeObject;
    private FadeAnimationController fadeController;

    private void Start()
    {
        // Fetch fade object from GameManager
        fadeObject = GameManager.Instance.fadeObject;
        // Fetch fade controller from GameManager
        fadeController = GameManager.Instance.fadeController;
    }

    // Function to control scene transitions
    public void GameSceneCtrl()
    {
        if (fadeObject && fadeController)
        {
            // Play fade-in animation
            fadeController.PlayFadeAnimation(true, () =>
            {
                // Transition to the new scene after fade-in is complete
                SceneManager.LoadScene("ARPlaneScene");

                // Play fade-out after the scene is loaded (making the screen brighter)
                fadeController.PlayFadeAnimation(false);
            });
        }
        else
        {
            Debug.Log("Fade animation controller is not found on the fade object");
        }
    }


    void Update()
    {
    }
}