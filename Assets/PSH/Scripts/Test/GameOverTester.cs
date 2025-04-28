using UnityEngine;

public class GameOverTester : MonoBehaviour
{
    public GameObject victoryUI; // Victory UI object
    public GameObject defeatUI;  // Defeat UI object
    public GameObject background; // Background object

    void Update()
    {
        // if (Keyboard.current.nKey.wasPressedThisFrame) // Press N for Victory
        // {
        //     ShowVictoryUI();
        // }
        // if (Keyboard.current.mKey.wasPressedThisFrame) // Press M for Defeat
        // {
        //     ShowDefeatUI();
        // }
    }

    void ShowVictoryUI()
    {
        // Activate the background
        background.SetActive(true);

        // Activate the victory UI
        victoryUI.SetActive(true);

        // Deactivate the defeat UI
        defeatUI.SetActive(false);
    }

    void ShowDefeatUI()
    {
        // Activate the background
        background.SetActive(true);

        // Activate the defeat UI
        defeatUI.SetActive(true);

        // Deactivate the victory UI
        victoryUI.SetActive(false);
    }
}
