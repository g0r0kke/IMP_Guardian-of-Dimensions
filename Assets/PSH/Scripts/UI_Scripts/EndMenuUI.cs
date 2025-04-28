using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndMenuUI : MonoBehaviour
{
    public RectTransform arrowHead; // Arrow image position
    public Button[] Buttons; // 0 is MainMenu, 1 is Replay
    public Button okayButton;

    public GameObject victoryUI; // Victory UI object
    public GameObject defeatUI;  // Defeat UI object
    public GameObject background; // Background object

    private int currentSelection = 0; // Current selected button index

    // Variable to store the previous scene
    private string previousScene;
    private GameObject fadeObject;
    private FadeAnimationController fadeController;

    void Start()
    {
        // Initialization: Set the arrow to the default selection (Main Menu)
        ArrowMoveButton(currentSelection);
        okayButton.onClick.AddListener(ExecuteCurrentSelection); // Add listener to the OK button click event

        // Add click events for buttons
        Buttons[0].onClick.AddListener(() => SelectMainMenu()); // Main Menu button click
        Buttons[1].onClick.AddListener(() => SelectReplay()); // Replay button click

        fadeObject = GameManager.Instance.fadeObject; // Fade animation object
        fadeController = GameManager.Instance.fadeController; // Fade animation controller
    }

    // Show UI based on whether the game was won or lost
    public void ShowEndMenu(bool isVictory)
    {
        // Always enable background
        background.SetActive(true);

        // Activate victory UI or defeat UI based on the result
        if (isVictory)
        {
            victoryUI.SetActive(true); // Activate victory UI
            defeatUI.SetActive(false); // Deactivate defeat UI
        }
        else
        {
            victoryUI.SetActive(false); // Deactivate victory UI
            defeatUI.SetActive(true); // Activate defeat UI
        }
    }

    // Select Main Menu option
    public void SelectMainMenu()
    {
        // Play sound on button click
        AudioManager.Instance.PlayButtonSFX();
        currentSelection = 0; // Set current selection to Main Menu
        ArrowMoveButton(currentSelection); // Move the arrow to the selected button
    }

    // Select Replay option
    public void SelectReplay()
    {
        // Play sound on button click
        AudioManager.Instance.PlayButtonSFX();
        currentSelection = 1; // Set current selection to Replay
        ArrowMoveButton(currentSelection); // Move the arrow to the selected button
    }

    // Move the arrow to the currently selected button
    void ArrowMoveButton(int index)
    {
        if (arrowHead != null && Buttons != null && index < Buttons.Length && Buttons[index] != null)
        {
            // Move the arrow to the position of the selected button
            arrowHead.position = Buttons[index].transform.position + new Vector3(-200, -5, 0); // Adjust arrow position
        }
        else
        {
            Debug.LogWarning("ArrowMoveButton missing"); // Warning if the arrow position change fails
        }
    }

    // Execute the action for the currently selected option when OK button is clicked
    public void ExecuteCurrentSelection()
    {
        // Play sound on OK button click
        AudioManager.Instance.PlayButtonSFX();

        if (fadeObject && fadeController)
        {
            // Play fade-in animation (screen goes black)
            fadeController.PlayFadeAnimation(true, () =>
            {
                // After fade-in, transition to the selected scene
                switch (currentSelection)
                {
                    case 0:
                        // If Main Menu is selected, load the MainMenu scene
                        Debug.Log("Main Menu selected");
                        if (GameManager.Instance)
                        {
                            GameManager.Instance.SetState(GameState.Intro); // Set game state to Intro
                        }

                        SceneManager.LoadScene("MainMenuScene"); // Load MainMenu scene

                        break;
                    case 1:
                        if (GameManager.Instance)
                        {
                            GameManager.Instance.SetState(GameState.Intro); // Set game state to Intro
                        }

                        // If Replay is selected, reload the current scene
                        Debug.Log("Replay selected");
                        SceneManager.LoadScene("ARPlaneScene"); // Load ARPlane scene
                        break;
                }

                // After scene load, play fade-out animation (screen goes bright again)
                fadeController.PlayFadeAnimation(false);
            });
        }
        else
        {
            Debug.Log("FadeAnimationController component not found on fadeObject"); // Log if fadeObject doesn't have FadeAnimationController
        }
    }
}
