using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles AR object placement confirmation and scene transition
/// </summary>
public class ARPlaneButton : MonoBehaviour
{
    [SerializeField] private ARPlacement arPlacement;
    [SerializeField] private string targetSceneName = "Boss1Scene"; // Scene to transition to

    private Button button;
    private GameObject fadeObject;
    private FadeAnimationController fadeController;

    private void Start()
    {
        // Get the button component
        button = GetComponent<Button>();
        if (!button) return;

        // Find ARPlacement if not assigned
        if (!arPlacement)
        {
            arPlacement = FindFirstObjectByType<ARPlacement>();
        }

        // Connect button click event to method
        button.onClick.AddListener(SaveBossPosition);

        // Get fade animation references from GameManager
        if (GameManager.Instance)
        {
            fadeObject = GameManager.Instance.fadeObject;
            fadeController = GameManager.Instance.fadeController;
        }
    }

    /// <summary>
    /// Saves the position of the AR placed object and transitions to the boss scene
    /// </summary>
    public void SaveBossPosition()
    {
        // Play button sound effect
        AudioManager.Instance.PlayButtonSFX();

        // Save AR placement object position
        if (arPlacement && GameManager.Instance)
        {
            Vector3 cubePosition = arPlacement.GetSpawnedObjectPosition();

            // Save position if valid
            if (cubePosition != Vector3.zero)
            {
                GameManager.Instance.SetBossPosition(cubePosition);
            }
        }

        if (fadeObject && fadeController)
        {
            // Execute fade-in animation (screen turns black)
            fadeController.PlayFadeAnimation(true, () =>
            {
                // Set GameManager state
                if (GameManager.Instance)
                {
                    GameManager.Instance.SetState(GameState.BossPhase1);
                }
                
                // Transition to target scene
                SceneManager.LoadScene(targetSceneName);

                // Execute fade-out animation after scene load (screen brightens)
                fadeController.PlayFadeAnimation(false);
            });
        }
        else
        {
            Debug.Log("FadeAnimationController component not found on fadeObject");
        }
    }
}