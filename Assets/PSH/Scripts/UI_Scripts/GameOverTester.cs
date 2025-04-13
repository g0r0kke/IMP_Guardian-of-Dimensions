using UnityEngine;

public class GameOverTester : MonoBehaviour
{
    public GameObject victoryUI;
    public GameObject defeatUI;
    public GameObject background;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) // N키 누르면 승리
        {
            ShowVictoryUI();
        }
        if (Input.GetKeyDown(KeyCode.M)) // M키 누르면 패배
        {
            ShowDefeatUI();
        }
    }

    void ShowVictoryUI()
    {
        // 배경 활성화
        background.SetActive(true);

        // 승리 UI 활성화
        victoryUI.SetActive(true);

        // 패배 UI 비활성화
        defeatUI.SetActive(false);
    }

    void ShowDefeatUI()
    {
        // 배경 활성화
        background.SetActive(true);

        // 패배 UI 활성화
        defeatUI.SetActive(true);

        // 승리 UI 비활성화
        victoryUI.SetActive(false);
    }
}